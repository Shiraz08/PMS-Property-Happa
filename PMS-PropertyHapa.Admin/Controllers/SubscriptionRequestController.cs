using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Stripe;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Admin.Controllers
{


    public class SubscriptionRequestController : Controller
    {
        private ApiDbContext _context;
        private readonly IEmailSender _emailSender;


        public SubscriptionRequestController(ApiDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IEnumerable<StripeSubscriptionDto>> GetSubscriptionRequests()
        {
            try
            {
                var subscriptionRequests = await (from s in _context.StripeSubscriptions
                                                  from u in _context.ApplicationUsers.Where(x=>x.Id == s.UserId).DefaultIfEmpty()
                                                  where s.IsDeleted != true
                                                  select new StripeSubscriptionDto
                                                  {
                                                      Id = s.Id,
                                                      UserId = s.UserId,
                                                      User = u.FirstName + " " + u.LastName,
                                                      StartDate = s.StartDate,
                                                      EndDate = s.EndDate,
                                                      SubscriptionId = s.SubscriptionId,
                                                      EmailAddress = s.EmailAddress,
                                                      IsCanceled = s.IsCanceled,
                                                      BillingInterval = s.BillingInterval,
                                                      SubscriptionType = s.SubscriptionType,
                                                      IsTrial = s.IsTrial,
                                                      GUID = s.GUID,
                                                      Status = s.Status,
                                                      Currency = s.Currency,
                                                      CustomerId = s.CustomerId,
                                                      HasAdminPermission = s.HasAdminPermission,
                                                      AddedBy = s.AddedBy,
                                                  }).ToListAsync();

                return subscriptionRequests;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChangeAdminPermission(int id, bool hasPermission)
        {
            try
            {
                var userSubscription = await _context.StripeSubscriptions.FirstOrDefaultAsync(s => s.Id == id);

                if (userSubscription != null)
                {
                    var user = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userSubscription.UserId);
                    string htmlContent;

                    if (hasPermission)
                    {
                        htmlContent =
                            $@"<!DOCTYPE html>
                                <html lang=""en"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Admin Permission Granted</title>
                                </head>
                                <body>
                                    <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                        <p>Hello {user.FirstName} {user.LastName},</p>
                                        <p>We are pleased to inform you that you have been granted admin permission. You can now access and use the Property Manager Portal.</p>
                                        <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                        <p>Thank you!</p>
                                    </div>
                                </body>
                                </html>";
                    }
                    else
                    {
                        htmlContent =
                            $@"<!DOCTYPE html>
                                <html lang=""en"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Admin Permission Denied</title>
                                </head>
                                <body>
                                    <div style=""font-family: Arial, sans-serif; padding: 20px;"">
                                        <p>Hello {user.FirstName} {user.LastName},</p>
                                        <p>We regret to inform you that your request for admin permission has been denied. You will not be able to access the Property Manager Portal with admin privileges.</p>
                                        <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                                        <p>Thank you!</p>
                                    </div>
                                </body>
                                </html>";
                    }

                    string subject = hasPermission ? "Admin Permission Granted" : "Admin Permission Denied";

                    await _emailSender.SendEmailAsync(user.Email, subject, htmlContent);

                    userSubscription.HasAdminPermission = hasPermission;
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Admin permission updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "User subscription not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the admin permission." });
            }
        }




    }
}
