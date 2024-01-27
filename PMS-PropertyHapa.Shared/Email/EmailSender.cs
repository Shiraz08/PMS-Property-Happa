using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Shared.Email
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailWithFIle(byte[]? bytesArray, string emails, string subject, string message,String FileName)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("info@allsolace.com", "?Q3WFVWuxBce%Y$");
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("info@allsolace.com");
                foreach (var address in emails.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(new MailAddress(address));
                }
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(bytesArray), "Invoice.pdf"));
                smtpClient.Send(mailMessage);
            }
            catch (Exception e)
            {

                throw;
            }
            await Task.CompletedTask;
        }
        public async Task SendEmailAsync(string emails, string subject, string message)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("info@propertyhapa.com", "hcwzkjtnqnlytfoj");
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("info@propertyhapa.com");
                foreach (var address in emails.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(new MailAddress(address));
                }
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                smtpClient.Send(mailMessage);
            }
            catch (Exception e)
            {

                throw;
            }
            await Task.CompletedTask;
        }
    }
}
