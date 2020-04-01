using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class UserDAL
    {
        private Random random = new Random();
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<UserModel> GetAllUsers()
        {
            var userModel = new UserModel();
            userModel.UserList = new List<UserModel>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_GetAllUsers", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        UserModel model = new UserModel();
                        model.UserName = Convert.ToString(ds.Tables[0].Rows[i]["UserName"]);
                        model.FirstName = Convert.ToString(ds.Tables[0].Rows[i]["FirstName"]);
                        model.LastName = Convert.ToString(ds.Tables[0].Rows[i]["LastName"]);
                        model.Email = Convert.ToString(ds.Tables[0].Rows[i]["EmailId"]);
                        model.DOB = Convert.ToDateTime(ds.Tables[0].Rows[i]["DOB"]);
                        model.CreatedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreatedDate"]);
                        model.IsActive = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsActive"]);
                        model.UserTypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        model.UserType = Convert.ToString(ds.Tables[0].Rows[i]["UserType"]);
                        userModel.UserList.Add(model);
                    }
                }
                return userModel.UserList;
            }
            catch (Exception ex)
            {
                return new List<UserModel>();
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<UserType> GetUserTypes()
        {
            List<UserType> types = new List<UserType>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_GetUserTypes", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        UserType type = new UserType();
                        type.TypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        type.TypeName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        type.IsActive = true;
                        types.Add(type);
                    }
                }
                return types;
            }
            catch (Exception ex)
            {
                return new List<UserType>();
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int CreateUser(FormCollection form, out List<string> failed)
        {
            List<string> emailList = new List<string>();
            List<string> codes = new List<string>();
            int failedCount = 0;
            int status = 0;
            connection = new SqlConnection(connectionString);
            failed = new List<string>();
            var emailIds = form["emailId"];
            var userTypes = form["UserType"];
            string pattern = ",";

            var emails = emailIds.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
            emails = emails.Select(x => x = x.Replace(",", "")).ToList();
            var types = userTypes.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
            types = types.Select(x => x = x.Replace(",", "")).ToList();
            
            SqlCommand cmd = new SqlCommand();
            for (int i = 0; i < emails.Count; i++)
            {
                if (!string.IsNullOrEmpty(emails[i]))
                {
                    cmd = new SqlCommand("sp_CreateUser", connection);
                    cmd.Parameters.Add(new SqlParameter("@emailId", emails[i]));
                    var randomCode = RandomString();
                    cmd.Parameters.Add(new SqlParameter("@signupCode", randomCode));
                    cmd.Parameters.Add(new SqlParameter("@userType", Convert.ToInt32(types[i])));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();

                    if (status <= 0)
                    {
                        failed.Add(emails[i]);
                    }
                    else
                    {
                        emailList.Add(emails[i]);
                        codes.Add(randomCode);
                    }
                }
            }
            failedCount = failed.Count();
            SendEmail(emailList, codes);

            return status;
        }

        public string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void SendEmail(List<string> recievers, List<string> signupCodes)
        {
            List<string> bodies = new List<string>();
            for (int i = 0; i < recievers.Count; i++)
            {
                string host = string.Empty;
                string port = string.Empty;
                if (HttpContext.Current != null)
                {
                    host = HttpContext.Current.Request.Url.Host;
                    port = HttpContext.Current.Request.Url.Port.ToString();
                }
                StringBuilder emailBody = new StringBuilder();
                emailBody.Append("<p>Hello,</p>");
                emailBody.Append("<br/>");
                emailBody.Append("<p>Your account for Karamtara Web Application has been created.</p>");
                emailBody.Append("<br/>");
                emailBody.Append("<p>You can use your Email and Code mentioned below to Signup on the website<p>");
                emailBody.Append("<br/><br/>");
                emailBody.Append("<p>Email - " + recievers[i] + "<p>");
                emailBody.Append("<br/><br/>");
                emailBody.Append("<p>Signup Code - " + signupCodes[i] + "<p>");
                emailBody.Append("<br/><br/>");
                emailBody.Append("<br/><br/>");
                emailBody.Append("<p>Regards,<p>");
                emailBody.Append("<br/>");
                emailBody.Append("Karamtara");

                bodies.Add(Convert.ToString(emailBody));
            }
            string subject = "Karamtara Account Creation";
            EmailService emailService = new EmailService();
            emailService.SendDifferentEmailsAsync(recievers, subject, bodies);
        }
    }
}