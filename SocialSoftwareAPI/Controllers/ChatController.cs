using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialSoftwareAPI.Hubs;
using SocialSoftwareAPI.Models;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;

namespace SocialSoftwareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private readonly IConfiguration _configuration;

        public ChatController(IHubContext<ChatHub, IChatClient> chatHub, IConfiguration configuration)
        {
            _chatHub = chatHub;
            _configuration = configuration;
        }

        [HttpPost("AddConnectionId")]
        public async Task AddConnectionId([FromBody] UserAndConnectionId _UserAndConnectionId)
        {
            ConnectionMapping<string>.Add(_UserAndConnectionId.User, _UserAndConnectionId.ConnectionId);

        }

        [HttpPost("RemoveConnectionId")]
        public async Task RemoveConnectionId([FromBody] UserAndConnectionId _UserAndConnectionId)
        {
            ConnectionMapping<string>.Remove(_UserAndConnectionId.User);
        }

        [HttpPost("messages")]
        public async Task Post([FromBody] ChatMessage message)
        {

            int SenderId = 0, ReceiverId = 0;

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            string query = @"select (select UserA.UserId from dbo.Users as UserA where UserA.UserName = '"+message.User+"') as SenderId, " +
                                   "(select UserB.UserId from dbo.Users as UserB where UserB.UserName = '" + message.Receiver + "') as ReveiverId";

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                foreach (DataRow t in table.Rows)
                {
                    SenderId = (int)t["SenderId"];
                    ReceiverId = (int)t["ReveiverId"];
                }

                query = @"insert into dbo.Messages values( " +
                    SenderId + " , " +
                    ReceiverId + " , ' " +
                    message.Message + "' , " +
                    " GETDATE() , " +
                    " 0 " +
                    ") " ;

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            if (ConnectionMapping<string>.GetConnectionId(message.User) != null)
                await _chatHub.Clients.Client(ConnectionMapping<string>.GetConnectionId(message.User)).ReceiveMessage(message);
            if (ConnectionMapping<string>.GetConnectionId(message.Receiver) != null)
                await _chatHub.Clients.Client(ConnectionMapping<string>.GetConnectionId(message.Receiver)).ReceiveMessage(message);
        }

        [HttpPost("GetChatRecord")]
        public JsonResult GetChatRecord([FromBody] ChatMessage message)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            string query = @"select MessageId, UsersA.UserName as Sender, UsersB.UserName as Receiver, Message, CAST(Time as DATETIME) as Time, Readed, DATEDIFF(Hour, Time, GETDATE()) as DateDiff, " +
                "(" +
                    "select count(*) " +
                    "from dbo.Messages as tempMessages " +
                    "left join dbo.Users as tempUsersA on tempUsersA.UserId = tempMessages.Sender " +
                    "left join dbo.Users as tempUsersB on tempUsersB.UserId = tempMessages.Receiver " +
                    "where (tempUsersA.UserName = '" + message.Receiver + "' and tempUsersB.UserName = '" + message.User + "' and tempMessages.Readed = 0) " +
                ") as NOU " +
                "from dbo.Messages " +
                "left join dbo.Users as UsersA on UsersA.UserId = Sender " +
                "left join dbo.Users as UsersB on UsersB.UserId = Receiver " +
                "where (UsersA.UserName = '" + message.User + "' and UsersB.UserName = '" + message.Receiver + "') or " +
                "(UsersA.UserName = '" + message.Receiver + "' and UsersB.UserName = '" + message.User + "') " +
                "order by Time asc";

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPost("SetNOU")]
        public JsonResult SetNOU([FromBody] ChatMessage message)
        {
            int SenderId = 0, ReceiverId = 0;

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            string query = @"select (select UserA.UserId from dbo.Users as UserA where UserA.UserName = '" + message.User + "') as SenderId, " +
                                   "(select UserB.UserId from dbo.Users as UserB where UserB.UserName = '" + message.Receiver + "') as ReveiverId ";

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                foreach (DataRow t in table.Rows)
                {
                    SenderId = (int)t["SenderId"];
                    ReceiverId = (int)t["ReveiverId"];
                }

                query = @"update dbo.Messages set Readed = 1 " +
                    "where Sender = " + ReceiverId + " and Receiver = " + SenderId;

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                return new JsonResult(table);
            }
        }
    }

    public class UserAndConnectionId
    {
        public string User { get; set; }

        public string ConnectionId { get; set; }
    }

    public static class ConnectionMapping<T>
    {
        private static readonly ConcurrentDictionary<T, string> _connections = new ConcurrentDictionary<T, string>();

        public static int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public static void Add(T key, string connectionId)
        {
            _connections.TryAdd(key, connectionId);
        }

        public static string GetConnectionId(T key)
        {
            _connections.TryGetValue(key, out string connectionId);

            return connectionId;
        }

        public static IEnumerable<string> GetConnectionIds()
        {
            return _connections.Values;
        }

        public static void Remove(T key)
        {
            _connections.TryRemove(key, out string connectionId);
        }
    }
}
