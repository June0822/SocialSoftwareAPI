using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using SocialSoftwareAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace SocialSoftwareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PostController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            int UserId = CommonFunctions.GetUserId(CommonFunctions.GetUser(Request.Headers["Authorization"]), sqlDataSource);

            string query = @"select Posts.PostId, Users.UserName, Content, CAST(CreateDate as date) as date, CAST(CreateDate as time(0)) as time, DATEDIFF(Hour, CreateDate, GETDATE()) as DateDiff, ProfilePhotoSrc 
                , iif(FavoritePost.UserId is null, 'N' , 'Y' ) as isLiked, (select count(FavoritePost.UserId) from FavoritePost where FavoritePost.PostId = Posts.PostId) as LikeCount
                from dbo.Posts
                left join dbo.Users on Users.UserId = Posts.Owner
                left join dbo.FavoritePost on FavoritePost.UserId = " + UserId + " and FavoritePost.PostId = Posts.PostId " +
                " where (Posts.Owner = " + UserId + " or Posts.Owner in (select Passive from dbo.UserFollowRelation where Active = " + UserId + ")) order by CreateDate desc";

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

        [HttpPost]
        public JsonResult Post(Post post)
        {
            string query = @"select UserId from dbo.Users where UserName='" + post.Owner + @"'";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
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

                int UserId = (int)table.Rows[0]["UserId"];

                query = @"insert into dbo.Posts values(
                    '" + post.Content + @"'
                    ,'" + UserId + @"'
                    ," + @" GETDATE()
                    )";
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Added Successfully");
        }

        public class LikeModel
        {
            public int PostId { get; set; }
            public string Action { get; set; } = "like";

        }

        [HttpPost("like")]
        public void Like([FromBody] LikeModel likemodel) //Action -> like and cancel
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            int UserId = CommonFunctions.GetUserId(CommonFunctions.GetUser(Request.Headers["Authorization"]), sqlDataSource);

            string query = "";

            if (likemodel.Action == "like") query = "insert into dbo.FavoritePost values(" + likemodel.PostId + ", " + UserId + ")";
            else query = "delete from dbo.FavoritePost where (PostId= " + likemodel.PostId + " and UserId= " + UserId + ")";


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
        }

        [HttpPut]
        public JsonResult Put(Post post)
        {
            string query ;
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                int UserId = CommonFunctions.GetUserId(post.Owner, sqlDataSource);

                query = @"update dbo.Posts set
                    Content = '" + post.Content + @"'
                    where PostId = '" + UserId + @"'";

                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult("Update Successfully");
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string query = @"delete from dbo.Posts
                where PostId = '" + id + @"'";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
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

            return new JsonResult("Delete Successfully");
        }
    }
}
