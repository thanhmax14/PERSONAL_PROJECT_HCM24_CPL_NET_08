using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Lab01
{
    public class SendEmailMail
    {
        private string _smtpServer = "smtp.gmail.com";
        private int _smtpPort = 587;
        private string _senderEmail = "thanhpqce171732@fpt.edu.vn";
        private string _senderPassword = "gbtl dqnu hobr wunt";
        public bool SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    client.EnableSsl = true;

                    MailMessage mailMessage = new MailMessage(_senderEmail, recipientEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    client.Send(mailMessage);
                    Console.WriteLine("Email sent successfully.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}