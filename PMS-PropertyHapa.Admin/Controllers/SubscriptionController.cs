using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using System.Text.Encodings.Web;
using PMS_PropertyHapa.Shared.Email;
using System.Net;
using System.Text;
using System.Web;
using Humanizer;

namespace PMS_PropertyHapa.Controllers
{
    //[Authorize]
    public class SubscriptionController : Controller
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        EmailSender _emailSender = new EmailSender();
        private IWebHostEnvironment _environment;

        public SubscriptionController(IWebHostEnvironment Environment, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
        }

        [HttpGet]
        public IActionResult AddSubscription()
        {
            return View(new SubscriptionDto());
        }


        [HttpPost]
        public async Task<IActionResult> Create(SubscriptionDto dto)
        {
            dto.Id = null;
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            try
            {
                Subscription newSubscription = new Subscription
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description,
                    AppTenantId = dto.AppTenantId,
                    TenantId = dto.TenantId,
                    DiskSpaceGB = dto.DiskSpaceGB,
                    EmailAccounts = dto.EmailAccounts,
                    BandwidthGB = dto.BandwidthGB,
                    Subdomains = dto.Subdomains,
                    Domains = dto.Domains,
                };

                _context.Subscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Subscription added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding subscription: " + ex.Message });
            }
        }


        [HttpPut]
        public async Task<IActionResult> Edit(int id, SubscriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return Json(new { success = false, message = "Subscription not found" });
            }

            try
            {
                subscription.Name = dto.Name;
                subscription.Price = dto.Price;
                subscription.Description = dto.Description;
                subscription.AppTenantId = dto.AppTenantId;
                subscription.TenantId = dto.TenantId;
                subscription.DiskSpaceGB = dto.DiskSpaceGB;
                subscription.EmailAccounts = dto.EmailAccounts;
                subscription.BandwidthGB = dto.BandwidthGB;
                subscription.Subdomains = dto.Subdomains;
                subscription.Domains = dto.Domains;


                _context.Subscriptions.Update(subscription);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Subscription updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating subscription: " + ex.Message });
            }
        }

        // DELETE: Subscriptions/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var subscription = await _context.Subscriptions.FindAsync(id);
                if (subscription == null)
                {
                    return Json(new { success = false, message = "Subscription not found" });
                }

                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Subscription deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting subscription: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _context.Subscriptions.ToListAsync();
                return Json(new { success = true, data = subscriptions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var subscription = await _context.Subscriptions.FindAsync(id);
                if (subscription == null)
                {
                    return Json(new { success = false, message = "Subscription not found." });
                }

                // Assuming SubscriptionDto is already mapped or directly usable
                var subscriptionDto = new SubscriptionDto
                {
                    Id = subscription.Id,
                    Name = subscription.Name,
                    Price = subscription.Price,
                    Description = subscription.Description,
                    DiskSpaceGB = subscription.DiskSpaceGB,
                    EmailAccounts = subscription.EmailAccounts,
                    BandwidthGB = subscription.BandwidthGB,
                    Subdomains = subscription.Subdomains,
                    Domains = subscription.Domains
                };

                return Json(new { success = true, data = subscriptionDto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving subscription data." });
            }
        }


    }
}