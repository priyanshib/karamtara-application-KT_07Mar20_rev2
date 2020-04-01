using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Karamtara_Application.HelperClass
{
    public class EmailService
    {
        public async Task SendEmailAsync(List<string> recievers, string subject, string message)
        {
            try
            {
                string smtpHost = WebConfigurationManager.AppSettings["smtpHost"];
                string smtpEmail = WebConfigurationManager.AppSettings["smtpEmailId"];
                string password = WebConfigurationManager.AppSettings["smtpPassword"];

                MailMessage mail = new MailMessage();
                foreach (var reciever in recievers)
                {
                    mail.To.Add(reciever);
                }
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                MailAddress mailAddress = new MailAddress(smtpEmail);
                mail.From = mailAddress;

                var smtpCliient = new SmtpClient(smtpHost);
                smtpCliient.Port = 587;
                smtpCliient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpCliient.UseDefaultCredentials = false;
                smtpCliient.EnableSsl = true;
                smtpCliient.Credentials = new NetworkCredential(smtpEmail, password);
                Task.Run(() => smtpCliient.SendMailAsync(mail));
            }
            catch(Exception ex)
            {

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

                var smtpCliient = new SmtpClient(smtpHost);
                smtpCliient.Port = 587;
                smtpCliient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpCliient.UseDefaultCredentials = false;
                smtpCliient.EnableSsl = true;
                smtpCliient.Credentials = new NetworkCredential(smtpEmail, password);

                Task.Run(() => smtpCliient.SendMailAsync(mail));
            }
            catch(Exception ex)
            {

            }
            
        }

    }
}