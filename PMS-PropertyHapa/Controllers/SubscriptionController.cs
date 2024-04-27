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
using PMS_PropertyHapa.Services.IServices;
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
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        EmailSender _emailSender = new EmailSender();
        private IWebHostEnvironment _environment;

        public SubscriptionController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
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

        // POST: Subscriptions/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubscriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            try
            {
                var sub = await _authService.CreateSubscriptionAsync(dto);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription added successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding subscription: " + ex.Message });
            }
        }

        // PUT: Subscriptions/Edit/5
        [HttpPut]
        public async Task<IActionResult> Edit(int id, [FromBody] SubscriptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var subscription = await _authService.GetSubscriptionsByIdAsync(id);
            if (subscription == null)
            {
                return Json(new { success = false, message = "Subscription not found" });
            }

            try
            {
                var sub = await _authService.UpdateSubscriptionAsync(dto);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription updated successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Update Failed" });
                }
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
                var sub = await _authService.DeleteSubscriptionAsync(id);

                if (sub)
                {
                    return Json(new { success = true, message = "Subscription Deleted successfully" });

                }

                else
                {
                    return Json(new { success = false, message = "Subscription Delete Failed" });
                }
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
                var subscriptions = await _authService.GetAllSubscriptionsAsync();
                var sortedItems = subscriptions.OrderBy(item => item.SubscriptionType == "Yearly").ToList();

                if (subscriptions == null || !subscriptions.Any())
                {
                    return Json(new { success = false, message = "No subscriptions found." });
                }

                return Json(new { success = true, data = sortedItems });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve subscriptions: {Exception}", ex);
                return Json(new { success = false, message = ex.Message });
            }
        }



    }
}