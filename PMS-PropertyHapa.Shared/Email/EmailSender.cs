using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace PMS_PropertyHapa.Shared.Email
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailWithFIle(byte[]? bytesArray, string emails, string subject, string message, String FileName)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("pms@propertyhapa.com", "pfml cdht nrga cqzw");
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("pms@propertyhapa.com");
                foreach (var address in emails.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(new MailAddress(address));
                }
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
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

        public async Task SendEmailWithFilebyStream(Stream fileStream, IEnumerable<string> emailAddresses, string subject, string message, string fileName)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("pms@propertyhapa.com", "pfml cdht nrga cqzw")
            };

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("pms@propertyhapa.com");
                    foreach (var address in emailAddresses)
                    {
                        mailMessage.To.Add(new MailAddress(address));
                    }
                    mailMessage.Subject = subject;
                    mailMessage.Body = message;
                    fileStream.Position = 0;
                    mailMessage.Attachments.Add(new Attachment(fileStream, fileName, MediaTypeNames.Application.Octet));

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task SendEmailAsync(string emails, string subject, string message)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("pms@propertyhapa.com", "pfml cdht nrga cqzw");
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("pms@propertyhapa.com");
                foreach (var address in emails.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(new MailAddress(address));
                }
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
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
