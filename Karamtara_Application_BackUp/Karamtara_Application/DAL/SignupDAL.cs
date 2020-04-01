using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;


namespace Karamtara_Application.DAL
{
    public class SignupDAL
    {
        private Random random = new Random();
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public int Signup(UserModel user)
        {
            var hashedPassword = PasswordHasher(user.Password);
            int status = 0;

            connection = new SqlConnection(connectionString);
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("sp_Signup", connection);
                cmd.Parameters.Add(new SqlParameter("@email", user.Email));
                cmd.Parameters.Add(new SqlParameter("@passwordHash", hashedPassword));
                cmd.Parameters.Add(new SqlParameter("@uName", user.UserName ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@fName", user.FirstName ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@lName", user.LastName ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@dob", user.DOB != null ? user.DOB.ToString() : null));
                cmd.Parameters.Add(new SqlParameter("@code", user.SignUpCode));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                return status;
            }
            catch(Exception ex)
            {
                return status;
            }
        }

        public string PasswordHasher(string password)
        {
            PasswordHash hasher = new PasswordHash(password);
            var hashedArray = hasher.ToArray();
            return Convert.ToBase64String(hashedArray);
            
        }
    }
}