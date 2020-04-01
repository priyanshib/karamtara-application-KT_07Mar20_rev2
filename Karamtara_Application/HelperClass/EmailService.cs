//using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Karamtara_Application.HelperClass
{
    public class EmailService
    {
        public async Task SendEmailAsync(List<string> recievers, string subject, string message, List<string> ccRecievers = null)
        {
            try
            {
                string smtpHost = WebConfigurationManager.AppSettings["smtpHost"];
                string smtpEmail = WebConfigurationManager.AppSettings["smtpEmailId"];
                string password = WebConfigurationManager.AppSettings["smtpPassword"];
                bool enableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["enableSsl"]);
                bool useDefaultCredentials = Convert.ToBoolean(WebConfigurationManager.AppSettings["useDefaultCredentials"]);
                int smtpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["smtpPort"]);

                MailMessage mail = new MailMessage();
                foreach (var reciever in recievers)
                {
                    mail.To.Add(reciever);
                }

                if (ccRecievers != null)
                {
                    foreach (var reciever in ccRecievers)
                    {
                        mail.CC.Add(reciever);
                    }
                }

                mail.Subject = subject;
                mail.Body = ReplaceImageSourceFromHTMLBody(message);
                mail.IsBodyHtml = true;
                MailAddress mailAddress = new MailAddress(smtpEmail);
                mail.From = mailAddress;
                var smtpCliient = new SmtpClient(smtpHost);
                smtpCliient.Port = smtpPort;
                smtpCliient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpCliient.UseDefaultCredentials = useDefaultCredentials;
                if (!smtpCliient.UseDefaultCredentials)
                {
                    smtpCliient.Credentials = new NetworkCredential(smtpEmail, password);
                }
                smtpCliient.EnableSsl = enableSsl;

                Task.Run(() => smtpCliient.SendMailAsync(mail));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void SendDifferentEmailsAsync(List<string> recievers, string subject, List<string> messages)
        {
            try
            {
                string smtpEmail = WebConfigurationManager.AppSettings["smtpEmailId"];
                List<MailMessage> mails = new List<MailMessage>();

                for (int i = 0; i < recievers.Count; i++)
                {
                    MailMessage mail = new MailMessage();
                    mail.To.Add(recievers[i]);
                    mail.Subject = subject;
                    mail.Body = messages[i];
                    mail.IsBodyHtml = true;
                    MailAddress mailAddress = new MailAddress(smtpEmail);
                    mail.From = mailAddress;
                    mails.Add(mail);
                }
                foreach (var mail in mails)
                {
                    SendMultipleDifferentEmailsAsync(mail);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void SendMultipleDifferentEmailsAsync(MailMessage mail)
        {
            try
            {
                string smtpHost = WebConfigurationManager.AppSettings["smtpHost"];
                string smtpEmail = WebConfigurationManager.AppSettings["smtpEmailId"];
                string password = WebConfigurationManager.AppSettings["smtpPassword"];
                bool enableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["enableSsl"]);
                bool useDefaultCredentials = Convert.ToBoolean(WebConfigurationManager.AppSettings["useDefaultCredentials"]);
                int smtpPort = Convert.ToInt32(WebConfigurationManager.AppSettings["smtpPort"]);
                mail.Body = ReplaceImageSourceFromHTMLBody(mail.Body);
                var smtpCliient = new SmtpClient(smtpHost);
                smtpCliient.Port = smtpPort;
                smtpCliient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpCliient.UseDefaultCredentials = useDefaultCredentials;
                smtpCliient.EnableSsl = enableSsl;
                if (!smtpCliient.UseDefaultCredentials)
                {
                    smtpCliient.Credentials = new NetworkCredential(smtpEmail, password);
                }

                Task.Run(() => smtpCliient.SendMailAsync(mail));
            }
            catch (Exception ex)
            {

            }
        }
        private string ReplaceImageSourceFromHTMLBody(string body)
        {
            body = body.Replace("{hrefTag}", GetApplicationURL());
            body = body.Replace("{<imgSource>}", "<img src = 'https://i.ibb.co/yVY8P6S/logo.png' width='120' alt='logo' border='0'>");
            return body;
        }


        private AlternateView getEmbeddedImage(string filePath, string body)
        {
            var path = HostingEnvironment.MapPath(filePath);
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            try
            {
                LinkedResource res = new LinkedResource(path);
                res.ContentId = "myImageID";
                alternateView.LinkedResources.Add(res);
                return alternateView;
            }
            catch
            {
                return alternateView;
            }
        }

        public void SendEmailWithAttachment(string message, string subject, Attachment attachment, List<string> toList, List<string> ccList)
        {
            try
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
                if (toList != null)
                {
                    foreach (var item in toList)
                    {
                        if (item != "")
                        {
                            mail.To.Add(item);
                        }
                    }
                }
                if (ccList != null)
                {
                    foreach (var item in ccList)
                    {
                        if (item != "")
                        {
                            mail.CC.Add(item);
                        }
                    }
                }
                //mail.AlternateViews.Add(getEmbeddedImage("~/EmailTemplates/logo.png", message));
                mail.Body = ReplaceImageSourceFromHTMLBody(message);
                mail.Subject = subject;
                MailAddress mailAddress = new MailAddress(smtpEmail);
                mail.From = mailAddress;
                mail.IsBodyHtml = true;
                if (attachment != null)
                    mail.Attachments.Add(attachment);

                smtp.Send(mail);
            }
            catch (Exception ex)
            {

            }
        }

        public string GetApplicationURL()
        {
            var url = string.Empty;

            //string host = string.Empty;
            //string port = string.Empty;
            //if (HttpContext.Current != null)
            //{
            //    host = HttpContext.Current.Request.Url.Host;
            //    port = HttpContext.Current.Request.Url.Port.ToString();
            //}

            url = string.Format("{0}://{1}{2}{3}",
              System.Web.HttpContext.Current.Request.Url.Scheme,
              System.Web.HttpContext.Current.Request.Url.Host,
              System.Web.HttpContext.Current.Request.Url.Port == 80 ? string.Empty : ":" + System.Web.HttpContext.Current.Request.Url.Port,
              System.Web.HttpContext.Current.Request.ApplicationPath);

            return url;
        }
    }
}