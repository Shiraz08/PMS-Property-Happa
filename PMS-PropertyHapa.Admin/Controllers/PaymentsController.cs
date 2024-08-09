using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;

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
        public async Task<IEnumerable<PaymentInfoData>> GetPayments()
        {
            try
            {
                var users = await (from ss in _context.StripeSubscriptions
                                   from u in _context.ApplicationUsers.Where(x=> x.Id == ss.UserId).DefaultIfEmpty()
                                   from pi in _context.PaymentInformations.Where(x => x.CustomerId == ss.CustomerId).DefaultIfEmpty()
                                   from s in _context.Subscriptions.Where(x=>x.Id == pi.SelectedSubscriptionId).DefaultIfEmpty()
                                   where ss.IsTrial != true
                                   select new PaymentInfoData
                                   {
                                       Payee = u.FirstName + " " + u.LastName,
                                       Amount = pi.AmountCharged,
                                       SubscriptionType = s.SubscriptionName,
                                       EmailAddress = ss.EmailAddress,
                                       BillingInterval = ss.BillingInterval,
                                       NoOfUnits = ss.NoOfUnits,
                                       StartDate = ss.StartDate,
                                       EndDate = ss.EndDate,
                                   }).ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the users.", ex);
            }
        }
    }
}
