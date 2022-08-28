using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using SocialSoftwareAPI.Models;

namespace SocialSoftwareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            int UserId = CommonFunctions.GetUserId(Username, sqlDataSource);
            string query = "select Nickname, Bio, (select count(Active) from dbo.UserFollowRelation where Active = " + UserId + ") as Following, " +
                "(select count(Passive) from dbo.UserFollowRelation where Passive = " + UserId + ") as Followers, " +
                "CoverPhotoSrc, ProfilePhotoSrc " +
                "from dbo.Users where UserId = " + UserId;

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
        public JsonResult Post(Profile profile)
        {
            string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;

            int UserId = CommonFunctions.GetUserId(Username, sqlDataSource);
            string query = "update dbo.Users set Bio = '" + profile.Bio + "', Nickname = '" + profile.Nickname + "' where UserId = " + UserId;

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

            return new JsonResult("Action completed");
        }

        [Route("SaveCoverFile")]
        [HttpPost]

        public JsonResult SaveCoverFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];

                string filename = DateTime.Now.Ticks.ToString()+"_"+postedFile.FileName;

                var physicalPath = _env.ContentRootPath + "/img/" + filename;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
                SqlDataReader myReader;

                int UserId = CommonFunctions.GetUserId(Username, sqlDataSource);

                string query = "update dbo.Users set CoverPhotoSrc = '" + filename + "' where UserId = " + UserId;

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

                return new JsonResult(filename);
            }
            catch (Exception)
            {
                return new JsonResult("Upload failed");
            }
        }

        [Route("SaveProfileFile")]
        [HttpPost]

        public JsonResult SaveProfileFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];

                string filename = DateTime.Now.Ticks.ToString() + "_" + postedFile.FileName;

                var physicalPath = _env.ContentRootPath + "/img/" + filename;

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                string Username = CommonFunctions.GetUser(Request.Headers["Authorization"]);

                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
                SqlDataReader myReader;

                int UserId = CommonFunctions.GetUserId(Username, sqlDataSource);

                string query = "update dbo.Users set ProfilePhotoSrc = '" + filename + "' where UserId = " + UserId;

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

                return new JsonResult(filename);
            }
            catch (Exception)
            {
                return new JsonResult("Upload failed");
            }
        }
    }
}
