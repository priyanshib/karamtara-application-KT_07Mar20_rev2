using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Karamtara_Application.Models;

namespace Karamtara_Application.DAL
{
    public class EditUserDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public int UpdateDetails(EditUserDetails user)
        {
            string sUpdate = "Update users set Firstname='" + user.FirstName + "', LastName='" + user.LastName + "', ";
            sUpdate += " salutation='" + user.Salutation + "', PasswordChangedDate=GetDate() ";

            if (!string.IsNullOrEmpty(user.Password))
            {
                SignupDAL PassHash = new SignupDAL();
                var HashedPassword = PassHash.PasswordHasher(user.Password);

                sUpdate += ", PasswordHash='" + HashedPassword+ "' ";
            }
            else if (user.DOB != null)
                sUpdate += ", Dob='" + user.DOB +"' ";
            
            sUpdate += " where userid=" + user.UserId + " ";

            connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(sUpdate, connection);
            int Result = cmd.ExecuteNonQuery();
            connection.Close();

            return Result;

        }

        public EditUserDetails GetUser(int Id)
        {
            EditUserDetails userDetail = new EditUserDetails();
            connection = new SqlConnection(connectionString);
            connection.Open();
            SqlDataAdapter adp = new SqlDataAdapter("Select UserId,FirstName,LastName,Dob,Salutation from Users where UserId = " + Id + " ", connection);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            connection.Close();

            if (dt.Rows.Count > 0)
            {
                userDetail.UserId = Convert.ToInt32(dt.Rows[0]["UserId"]);
                userDetail.FirstName = dt.Rows[0]["FirstName"].ToString();
                userDetail.LastName = dt.Rows[0]["LastName"].ToString();
                if (dt.Rows[0]["DOB"] == DBNull.Value)
                    userDetail.DOB = null;
                else
                    userDetail.DOB = Convert.ToDateTime(dt.Rows[0]["DOB"]);
                userDetail.Salutation = dt.Rows[0]["Salutation"].ToString();
                return userDetail;
            }
            else
            {
                return userDetail;
            }

        }

    }
}