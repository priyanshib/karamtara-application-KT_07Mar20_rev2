using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Karamtara_Application.Models;

namespace Karamtara_Application.DAL
{
    
    public class ManagePassDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        private Random random = new Random();
        public int forgotPassword(String emailId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int Status = 0;
            try
            {
                cmd = new SqlCommand("sp_CheckUsername",connection);
                cmd.Parameters.Add(new SqlParameter("@email",emailId ?? string.Empty));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                Status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                return Status;
            }
            catch (Exception ex)
            {
                return Status;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();

                }
            }
        }

        public void GetUserId(string emailId)
        {
           //var guid = Guid.NewGuid().ToString();
           //guid = guid.Substring(0, guid.Length > 6 ? 6 : guid.Length);

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetUserIdName", connection);
                cmd.Parameters.Add(new SqlParameter("@email", emailId ?? string.Empty));
                var randomCode = RandomString();
                cmd.Parameters.Add(new SqlParameter("@passwordCode", randomCode));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (Convert.ToBoolean(rdr["ReturnCode"]))
                    {
                        sendPasswordResetEmail(rdr["EmailId"].ToString(), rdr["UserId"].ToString(), rdr["UserName"].ToString(),rdr["PasswordCode"].ToString());
                    }
                    else
                    {

                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();

                }
            }
        }

        public string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void sendPasswordResetEmail(string toEmail, string UserId, string UserName,string PasswordCode)
        {
            string host = string.Empty;
            string port = string.Empty;
            if (HttpContext.Current != null)
            {
                host = HttpContext.Current.Request.Url.Host;
                port = HttpContext.Current.Request.Url.Port.ToString();
            }

            string redirectUrl = "http://" + host + ":" + port + "/ManagePassword/ResetPassword?UserId=" + PasswordCode;
            StringBuilder emailBody = new StringBuilder();
            emailBody.Append("Dear " + UserName);
            emailBody.Append("<br/><br/>");
            emailBody.Append(" Please click on following link to reset your Password");
            emailBody.Append("<br/><br/>");
            emailBody.Append("<a href='" + redirectUrl + "'>Click Here</a>");
            emailBody.Append("<br/><br/>");
            emailBody.Append("Your Password Reset Code is :"+ PasswordCode);
            emailBody.Append("<br/><br/>");

            string subject = "Karamtara Application: Reset Password link";
            ResetPasswordEmail(toEmail, subject, Convert.ToString(emailBody));

        }

        public void ResetPasswordEmail(string recievers, string subject, string message)
        {
            string smtpHost = WebConfigurationManager.AppSettings["smtpHost"];
            string smtpEmail = WebConfigurationManager.AppSettings["smtpEmailId"];
            string password = WebConfigurationManager.AppSettings["smtpPassword"];
            bool enableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["enableSsl"]);
            bool useDefaultCredentials = Convert.ToBoolean(WebConfigurationManager.AppSettings["useDefaultCredentials"]);
            int smtpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["smtpPort"]);

            SmtpClient smtp = new SmtpClient(smtpHost);
            smtp.Port = smtpPort;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = useDefaultCredentials;
            smtp.EnableSsl = enableSsl;
            if (!smtp.UseDefaultCredentials)
            {
                smtp.Credentials = new NetworkCredential(smtpEmail, password);
            }

            MailMessage mail = new MailMessage();
            if (recievers != null)
            {
                mail.To.Add(recievers);
            }
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            MailAddress mailAddress = new MailAddress(smtpEmail);
            mail.From = mailAddress;

            smtp.Send(mail);
        }

        public ResetPasswordModel GetMailId(string ResetPasswordCode)
        {
            string Reciever = null;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            ResetPasswordModel resetPassword = new ResetPasswordModel();

            try
            {
                cmd = new SqlCommand("sp_getRecieverMail", connection);
                cmd.Parameters.Add(new SqlParameter("@passwordCode", ResetPasswordCode));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                connection.Open();
                da.Fill(ds);
                connection.Close();
                if(ds.Tables[0] != null)
                {
                    for (int i=0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        resetPassword.UserId = Convert.ToInt32(ds.Tables[0].Rows[i]["UserId"]);
                        resetPassword.Receiver = Convert.ToString(ds.Tables[0].Rows[i]["EmailId"]);
                    }
                }
                return resetPassword;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return resetPassword;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();

                }
            }

        }

        public int ResetPassword(string UserId, string newPassword, string hashedPassword, string ResetPassCode)
        {
            int Status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_resetPassword", connection);
                cmd.Parameters.Add(new SqlParameter("@UserId", UserId));
                cmd.Parameters.Add(new SqlParameter("@Password", hashedPassword));
                cmd.Parameters.Add(new SqlParameter("@ResetPassCode", ResetPassCode));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                connection.Open();
                Status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (Status != 0)
                    return 1;
                else
                   return -1;

            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}