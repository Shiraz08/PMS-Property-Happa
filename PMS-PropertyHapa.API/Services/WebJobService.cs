using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;

namespace PMS_PropertyHapa.API.Services
{
    public class WebJobService
    {

        private readonly ApiDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public WebJobService(ApiDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task SendInvoices()
        {
            var today = DateTime.Now.Date;
            var invoices = await _db.Invoices.Where(x => x.InvoiceDate.Value.Date == today && x.IsDeleted != true).ToListAsync();

            foreach (var invoice in invoices)
            {

                var tenant = await _db.Tenant.FirstOrDefaultAsync(x => x.TenantId == invoice.TenantId && x.IsDeleted != true);
                var tenantEmail = tenant.EmailAddress;
                var tenantName = tenant.FirstName + " " + tenant.LastName;

                var propertyManager = await _userManager.FindByIdAsync(invoice.AddedBy);
                var propertyManagerEmail = propertyManager.Email;
                var propertyManagerName = propertyManager.Name;

                var emailContent = $@"
                    <p>Dear {tenantName},</p>
                    <p>This is to inform you that the invoice dated {invoice.InvoiceDate.Value.ToString("yyyy-MM-dd")} has been generated.</p>
                    <p>Thank you,</p>
                    <p>{propertyManagerName}</p>
                    ";

                var emailSubject = "Invoice Generated";

                // Sending email to both tenant and property manager
                var recipients = $"{tenantEmail},{propertyManagerEmail}";
                await _emailSender.SendEmailAsync(recipients, emailSubject, emailContent);

                //await _emailSender.SendEmailAsync(user.Email, Subject, emailContent);
            }
        }

        public async Task ReminderInvoices()
        {
           var reminderDate = DateTime.Now.Date.AddDays(-1);

            var invoices = await _db.Invoices
                                    .Where(x => x.InvoiceDate <= reminderDate && x.InvoicePaid != true && x.IsDeleted != true)
                                    .ToListAsync();

            foreach (var invoice in invoices)
            {
                var tenant = await _db.Tenant.FirstOrDefaultAsync(x => x.TenantId == invoice.TenantId && x.IsDeleted != true);
                var tenantEmail = tenant.EmailAddress;
                var tenantName = tenant.FirstName + " " + tenant.LastName;

                var propertyManager = await _userManager.FindByIdAsync(invoice.AddedBy);
                var propertyManagerEmail = propertyManager.Email;
                var propertyManagerName = propertyManager.Name;

                var emailContent = $@"
            <p>Dear {tenantName},</p>
            <p>This is a reminder that the invoice dated {invoice.InvoiceDate.Value.ToString("yyyy-MM-dd")} is due.</p>
            <p>Please make the payment at your earliest convenience.</p>
            <p>Thank you,</p>
            <p>{propertyManagerName}</p>
            ";

                var emailSubject = "Invoice Payment Reminder";

                var recipients = $"{tenantEmail},{propertyManagerEmail}";
                await _emailSender.SendEmailAsync(recipients, emailSubject, emailContent);

            }
        }

    }
}
