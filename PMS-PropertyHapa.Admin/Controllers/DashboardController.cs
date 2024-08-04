using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Email;
using System.Diagnostics;

namespace PMS_PropertyHapa.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        EmailSender _emailSender = new EmailSender();
        public DashboardController(IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                var subscriptions = await _context.Subscriptions.ToListAsync();
                var subscriptionCounts = subscriptions
                    .GroupBy(s => s.SubscriptionName)
                    .Select(g => new { SubscriptionName = g.Key, Count = g.Count() })
                    .ToList();
                var totalEarnings = subscriptions.Sum(s => s.Price);
                return Json(new { success = true, data = subscriptionCounts, totalEarnings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLandlords()
        {
            try
            {
                var landlordsPerMonth = await _context.Owner
                    .GroupBy(l => new { Month = l.AddedDate.Month, Year = l.AddedDate.Year })
                    .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Count = g.Count() })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToListAsync();

                var labels = landlordsPerMonth.Select(g => $"{g.Month}/{g.Year}").ToArray();
                var series = landlordsPerMonth.Select(g => g.Count).ToArray();

                return Json(new { success = true, labels, series });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllSubscribedUsers()
        {
            try
            {
                var activeSubscriptions = await _context.StripeSubscriptions
                    .Where(x => x.EndDate >= DateTime.UtcNow && !x.IsCanceled)
                    .ToListAsync();

                var latestSubscriptions = activeSubscriptions
                    .GroupBy(x => x.UserId)
                    .Select(g => g.OrderByDescending(x => x.Id).FirstOrDefault())
                    .Where(x => x != null)
                    .ToList();

                var subscribedUsersPerMonth = latestSubscriptions
                     .GroupBy(u => new { Month = u.AddedDate.Month, Year = u.AddedDate.Year })
                     .Select(g => new { g.Key.Month, g.Key.Year, Count = g.Count() })
                     .OrderBy(g => g.Year).ThenBy(g => g.Month)
                     .ToList();

                // Prepare labels and series for the chart
                var labels = subscribedUsersPerMonth.Select(g => $"{g.Month}/{g.Year}").ToArray();
                var series = subscribedUsersPerMonth.Select(g => g.Count).ToArray();

                return Json(new { success = true, labels, series });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        public async Task<IEnumerable<CustomerData>> GetAllUsers()
        {
            try
            {
                var users = await (from ss in _context.StripeSubscriptions
                                   join u in _context.ApplicationUsers on ss.UserId equals u.Id into userGroup
                                   from u in userGroup.DefaultIfEmpty()
                                   join s in _context.Subscriptions on u.SubscriptionId equals s.Id into subGroup
                                   from s in subGroup.DefaultIfEmpty()
                                   join o in _context.Owner on u.Id equals o.AddedBy into ownerGroup
                                   from o in ownerGroup.DefaultIfEmpty()
                                   where ss.EndDate >= DateTime.UtcNow && !ss.IsCanceled
                                   group new { ss, u, s, o } by new
                                   {
                                       u.Id,
                                       ss.UserId,
                                       UserName = u.UserName ?? string.Empty,
                                       Email = u.Email ?? string.Empty,
                                       PhoneNumber = u.PhoneNumber ?? string.Empty,
                                       CompanyName = u.CompanyName ?? string.Empty,
                                       AddedDate = u.AddedDate,
                                       SubscriptionName = s.SubscriptionName ?? string.Empty,
                                       SubscriptionType = s.SubscriptionType ?? string.Empty,
                                       StartDate = ss.StartDate,
                                       EndDate = ss.EndDate
                                   } into g
                                   select new CustomerData
                                   {
                                       UserId = g.Key.UserId ?? string.Empty, 
                                       Name = g.Key.UserName,
                                       EmailAddress = g.Key.Email,
                                       PhoneNumber = g.Key.PhoneNumber,
                                       SubscriptionName = g.Key.SubscriptionName,
                                       SubscriptionType = g.Key.SubscriptionType,
                                       Expiring = g.Key.EndDate.HasValue ?
                                           EF.Functions.DateDiffDay(DateTime.UtcNow, g.Key.EndDate.Value) : (int?)null,
                                       EndDate = g.Key.EndDate,
                                       CompanyName = g.Key.CompanyName,
                                       OwnerCount = g.Count(x => x.o != null)
                                   }).OrderByDescending(x => x.UserId).ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the users.", ex);
            }
        }


    }
}
