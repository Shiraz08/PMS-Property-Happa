using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class PaymentsController : Controller
    {
        private ApiDbContext _context;

        public PaymentsController(ApiDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IEnumerable<CustomerData>> GetPaymentsData()
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
                                   select new
                                   {
                                       ss.UserId,
                                       UserName = u.UserName ?? string.Empty,
                                       Email = u.Email ?? string.Empty,
                                       PhoneNumber = u.PhoneNumber ?? string.Empty,
                                       CompanyName = u.CompanyName ?? string.Empty,
                                       SubscriptionName = s.SubscriptionName ?? string.Empty,
                                       SubscriptionType = s.SubscriptionType ?? string.Empty,
                                       Expiring = ss.EndDate.HasValue ? EF.Functions.DateDiffDay(DateTime.UtcNow, ss.EndDate.Value) : (int?)null,
                                       EndDate = ss.EndDate,
                                       OwnerCount = o != null ? 1 : 0
                                   })
                                   .Distinct()
                                   .GroupBy(x => new { x.UserId })
                                   .Select(g => new CustomerData
                                   {
                                       UserId = g.Key.UserId,
                                       Name = g.FirstOrDefault().UserName,
                                       EmailAddress = g.FirstOrDefault().Email,
                                       PhoneNumber = g.FirstOrDefault().PhoneNumber,
                                       SubscriptionName = g.FirstOrDefault().SubscriptionName,
                                       SubscriptionType = g.FirstOrDefault().SubscriptionType,
                                       Expiring = g.FirstOrDefault().Expiring,
                                       EndDate = g.FirstOrDefault().EndDate,
                                       CompanyName = g.FirstOrDefault().CompanyName,
                                       OwnerCount = g.Sum(x => x.OwnerCount)
                                   })
                                   .OrderByDescending(x => x.UserId)
                                   .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the users.", ex);
            }
        }

    }
}
