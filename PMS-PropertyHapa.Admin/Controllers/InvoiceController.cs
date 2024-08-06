using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PMS_PropertyHapa.Admin.Models.ViewModels;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Stripe;
using System.Xml.Linq;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class InvoiceController : Controller
    {
        private ApiDbContext _context;
        private readonly GoogleCloudStorageOptions _googleCloudStorageOptions;


        public InvoiceController(ApiDbContext context, IOptions<GoogleCloudStorageOptions> googleCloudStorageOptions)
        {
            _context = context;
            _googleCloudStorageOptions = googleCloudStorageOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IEnumerable<SubscriptionInvoiceDto>> GetInvoices()
        {
            try
            {
                var invoices = await (from si in _context.SubscriptionInvoices
                                      select new SubscriptionInvoiceDto
                                      {
                                          Id = si.Id,
                                          UserId = si.UserId,
                                          ToName = si.ToName,
                                          ToEmail = si.ToEmail,
                                          File = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"SubscriptionInvoice_File_" + si.Id + ".pdf"}",
                                      }).ToListAsync();

                return invoices;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the users.", ex);
            }
        }

    }
}
