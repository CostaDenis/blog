using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Blog.Services
{
    public class EmailService
    {
        public bool Send(
            string toName,
            string toEmail,
            string subject,
            string body,
            string fromName = "Equipe denis.io",
            string fromEmail = "deniscostaf@gmail.com"
        )
        {
            var smtp = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port);

            smtp.Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            //Criando o email
            var mail = new MailMessage();

            mail.From = new MailAddress(fromEmail, fromName);
            mail.To.Add(new MailAddress(toEmail, toName));
            //Dá para mandar para alguns endereços de uma só vez
            // mail.To.Add(new MailAddress(toEmail, toName));
            // mail.To.Add(new MailAddress(toEmail, toName));
            // mail.To.Add(new MailAddress(toEmail, toName));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            try
            {
                smtp.Send(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}