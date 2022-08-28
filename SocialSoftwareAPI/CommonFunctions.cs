using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace SocialSoftwareAPI
{
    public class CommonFunctions
    {
        public static string GetUser(string token)
        {
            string[] split = token.Split('.');
            var iv = split[0];
            var encrypt = split[1];
            var signature = split[2];


            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            String username = encoding.GetString(Convert.FromBase64String(encrypt));

            username = username.Replace("\"", "");
            username = username.Replace("\\", "");

            int seperatorIndex1 = username.IndexOf(':');
            int seperatorIndex2 = username.IndexOf(',');


            return username.Substring(seperatorIndex1 + 1, seperatorIndex2 - seperatorIndex1 - 1);
        }

        public static int GetUserId(string UseraName, string sqlDataSource)
        {
            int UserId = 0;

            string query = @"select UserId from dbo.Users where UserName='" + UseraName + @"'";
            DataTable table = new DataTable();
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

                    UserId = (int)table.Rows[0]["UserId"];
                }

                return UserId;
            }
        }
    }
}
