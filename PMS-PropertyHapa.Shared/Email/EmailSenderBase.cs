using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace PMS_PropertyHapa.Shared.EmailSenderFile
{
    public class EmailSenderBase
    {
        public async Task SendEmailWithFile(Stream fileStream, IEnumerable<string> emailAddresses, string subject, string message, string fileName)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("info@propertyhapa.com", "hcwzkjtnqnlytfoj") 
                };

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress("info@propertyhapa.com");
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
    }
}