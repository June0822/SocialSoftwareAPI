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
    //[Authorize]
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
            string query = @"select PostId, Users.UserName, Content, CAST(CreateDate as date) as date, CAST(CreateDate as time(0)) as time, DATEDIFF(Hour, CreateDate, GETDATE()) as DateDiff from dbo.Posts
                left join dbo.Users on Users.UserId = Posts.Owner";
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

            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(Post post)
        {
            string query = @"insert into dbo.Posts values(
                '" + post.Content + @"'
                ,'" + post.Owner + @"'
                ," + @" GETDATE()
                )";
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

            return new JsonResult("Added Successfully");
        }

        [HttpPut]
        public JsonResult Put(Post post)
        {
            string query = @"update dbo.Posts set
                Content = '" + post.Content + @"'
                where PostId = '" + post.PostId + @"'";
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
