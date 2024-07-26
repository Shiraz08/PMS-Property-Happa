using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Models.Stripe;

namespace PMS_PropertyHapa.Admin.Controllers
{
    
  
    public class SubscriptionRequestController : Controller
    {
        private ApiDbContext _context;

        public SubscriptionRequestController( ApiDbContext context)
        {
            _context = context;
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
                                                  join u in _context.ApplicationUsers on s.UserId equals u.Id
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
