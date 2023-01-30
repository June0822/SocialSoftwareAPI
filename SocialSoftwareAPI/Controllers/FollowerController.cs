using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using SocialSoftwareAPI.Models;

namespace SocialSoftwareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FollowerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get() //Models.UserDto request
        {
            string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

            string query = @"select UserId, UserName, 'N' as isFollow, ProfilePhotoSrc from dbo.Users where UserName!= '" + Username + @"'";
            DataTable AllUserTable = new DataTable();
            DataTable RelationTable = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    AllUserTable.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                query = @"select Active, Passive from dbo.UserFollowRelation
                    left join dbo.Users on UserFollowRelation.Active=Users.UserId 
                    where Users.UserName = '" + Username + @"'";

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    RelationTable.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }

                foreach(DataRow Relation in RelationTable.Rows)
                {
                    foreach(DataRow AllUser in AllUserTable.Rows)
                    {
                        if (AllUser["UserId"].Equals(Relation["Passive"]))
                        {
                            AllUser["isFollow"] = "Y";
                            break;
                        }
                    }
                }
            }

            return new JsonResult(AllUserTable);
        }

        [Route("contact")]
        [HttpGet]
        public JsonResult GetContact()
        {
            string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

            string query = @"select Passive, tempUsers.UserName, tempUsers.ProfilePhotoSrc from UserFollowRelation 
                left join Users on Active = UserId
                left join Users as tempUsers on Passive = tempUsers.UserID
                where Users.UserName = '" + Username + @"' and (select Count(*) from UserFollowRelation as temp where temp.Active = UserFollowRelation.Passive and temp.Passive = UserFollowRelation.Active) != 0 ";
            DataTable ContactTable = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    ContactTable.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(ContactTable);
        }

        [HttpPost]
        public JsonResult Post(FollowRelation FollowRelation)
        {
            string query ;

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                int UserId = CommonFunctions.GetUserId(FollowRelation.Active, sqlDataSource);

                if (FollowRelation.Action == "Follow")
                {
                    query = @"insert into dbo.UserFollowRelation values(" + UserId + @", " + FollowRelation.Passive + ")";
                }
                else
                {
                    query = @"delete from dbo.UserFollowRelation where ( Active = " + UserId + @" and Passive = " + FollowRelation.Passive + " )";
                }
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Action completed");
        }

        
    }
}
