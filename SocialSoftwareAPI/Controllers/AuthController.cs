using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialSoftwareAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace SocialSoftwareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Username = request.Username;
            user.UserAccount = request.UserAccount;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;


            string query = @"select * from dbo.Users 
            where (Users.UserName = '" + user.Username + @"' or Users.UserAccount = '" + user.UserAccount + @"')";
            if (GetDataFromDB(query).Rows.Count > 0)
            {
                return BadRequest("User name or account is already existing");
            }

            AddUserInfo(user.Username, user.UserAccount, request.Password, user.PasswordHash, user.PasswordSalt);

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            string query = @"select * from dbo.Users 
            where (Users.UserName = '" + request.Username + @"' collate Chinese_PRC_CS_AI ) ";

            DataTable dt = GetDataFromDB(query);

            if (dt.Rows.Count == 0)
            {
                return BadRequest("User not found.");
            }

            byte[] passwordHash = (byte[]) dt.Rows[0]["PasswordHash"];
            byte[] passwordSalt = (byte[]) dt.Rows[0]["PasswordSalt"];

            if (!VerifyPasswordHash(request.Password, passwordHash, passwordSalt))
            {
                return BadRequest("Wrong password");
            }

            string token = CreateToken(request);
            return Ok(token);
        }

        private DataTable GetDataFromDB(String query)
        {
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
            return table;
        }

        private void AddUserInfo(string userName, string userAccount, string userPassword, byte[] passwordHash, byte[] passwordSalt)
        {

            //System.Diagnostics.Debug.WriteLine(passwordHash);

            string query = @"insert into dbo.Users values(
                '" + userName + @"'
                ,'" + userAccount + @"'
                ,'" + userPassword + @"'
                , @PasswordHash, @PasswordSalt
                )";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("SocialSoftwareAppCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {

                    myCommand.Parameters.Add("@PasswordHash", SqlDbType.VarBinary);
                    myCommand.Parameters["@PasswordHash"].Value = passwordHash;
                    myCommand.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary);
                    myCommand.Parameters["@PasswordSalt"].Value = passwordSalt;

                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();
                }
            }
        }

        private string CreateToken(UserDto user)
        {
            //List<Claim> claims = new List<Claim>()
            //{
            //    new Claim(ClaimTypes.Name, user.Username)
            //};

            var claims = new Claim[]
            {
                new Claim("name", user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
