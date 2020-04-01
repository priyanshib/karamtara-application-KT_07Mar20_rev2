using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using static Karamtara_Application.HelperClass.Flags;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class LoginDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public UserModel CheckLogin(LoginModel model)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            UserModel user = new UserModel();
            var passwordHash = new byte[36];
            try
            {
                cmd = new SqlCommand("sp_GetPasswordHash", connection);
                cmd.Parameters.Add(new SqlParameter("@UserName", model.UserName));
                //cmd.Parameters.Add(new SqlParameter("@passwordHash", hashedPass));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    var passString = Convert.ToString(ds.Tables[0].Rows[0]["passwordhash"]);

                    if (!string.IsNullOrEmpty(passString))
                    {
                        passwordHash = Convert.FromBase64String(passString);
                        PasswordHash pass = new PasswordHash(passwordHash);
                        var result = pass.Verify(model.Password);
                        if (result)
                        {
                            status = 1;
                            user = GetUserbyCred(model.UserName, passString);
                        }
                        else
                            status = 0;
                    }
                }
                else
                    status = 0;

            }
            catch (Exception ex)
            {
                return user;
            }
            return user;
        }

        public UserModel GetUserbyCred(string userName, string passwordHash)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            UserModel user = new UserModel();
            try
            {
                cmd = new SqlCommand("sp_CheckLogin", connection);
                cmd.Parameters.Add(new SqlParameter("@UserName", userName));
                cmd.Parameters.Add(new SqlParameter("@passwordHash", passwordHash));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    user.UserId = Convert.ToInt32(ds.Tables[0].Rows[0]["UserId"]);
                    user.FirstName = Convert.ToString(ds.Tables[0].Rows[0]["FirstName"]);
                    user.LastName = Convert.ToString(ds.Tables[0].Rows[0]["LastName"]);
                    user.UserName = Convert.ToString(ds.Tables[0].Rows[0]["UserName"]);
                    user.Email = Convert.ToString(ds.Tables[0].Rows[0]["EmailId"]);
                    user.DOB = Convert.ToDateTime(ds.Tables[0].Rows[0]["DOB"]);
                    user.IsActive = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsActive"]);
                    user.UserTypeId = Convert.ToInt32(ds.Tables[0].Rows[0]["Type"]);
                }
                return user;
            }
            catch (Exception ex)
            {
                return user;
            }
        }
    }
}