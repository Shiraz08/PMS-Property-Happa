using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Staff.Auth.Controllers;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class LeaseController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;

        public LeaseController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
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
        public async Task<IActionResult> Index()
        {
            var lease = await _authService.GetAllLeasesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                lease = lease.Where(s => s.AddedBy == currenUserId);
            }
            return View(lease);
        }

        public async Task<IActionResult> AddLease()
        {
            var selectedAssets = await _authService.GetAllAssetsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                selectedAssets = selectedAssets.Where(s => s.AddedBy == currenUserId);
            }
            var selectedUnits = selectedAssets.SelectMany(asset => asset.Units).ToList();
            var leaseDto = new LeaseDto
            {
                Assets = selectedAssets,
                SelectedUnits = selectedUnits
            };

            return View(leaseDto); 
        }

        [HttpGet]
        public async Task<IActionResult> ByUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID is required."); 
            }

            try
            {
                var properties2 = await _authService.GetAllAssetsAsync();
                var currenUserId = Request?.Cookies["userId"]?.ToString();
                if (currenUserId != null)
                {
                    properties2 = properties2.Where(s => s.AddedBy == currenUserId);
                }

                var properties = properties2
                    .Where(s=>s.AppTid == userId)
                    .Select(a => new {
                        AssetId = a.AssetId,
                        SelectedPropertyType = a.BuildingNo + " - " + a.BuildingName
                    })
                    .ToList();

                return Json(properties);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message); 
            }
        }

        [HttpGet]
        public async Task<IActionResult> ByProperty(int propertyId)
        {
            try
            {
                var units = await _authService.GetAllUnitsAsync();

                var filteredUnits = units
                    .Where(u => u.AssetId == propertyId) 
                    .Select(u => new {
                        UnitId = u.UnitId,
                        UnitName = u.UnitName
                    })
                    .ToList();

                return Json(filteredUnits);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(LeaseDto lease)
        {
            lease.TenantId = Convert.ToInt32(lease.TenantIdValue);
            if (lease == null)
            {
                return Json(new { success = false, message = "lease data is  empty" });
            }
            else
            {
                lease.AddedBy = Request?.Cookies["userId"]?.ToString();
                await _authService.CreateLeaseAsync(lease);
                return Json(new { success = true, message = "lease added successfully" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var lease = await _authService.GetLeaseByIdAsync(id);
            if (lease == null)
            {
                return NotFound();
            }
            return Json(new { success = true, data = lease });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leases = await _authService.GetAllLeasesAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                leases = leases.Where(s => s.AddedBy == currenUserId);
            }
            return Json(new { success = true, data = leases });
        }

        [HttpPost]
        public async Task<IActionResult> Update(LeaseDto lease)
        {
            lease.TenantId = Convert.ToInt32(lease.TenantIdValue);
            if (lease == null)
            {
                return Json(new { success = false, message = "Lease data is empty." });
            }
            else
            {
                var result = await _authService.UpdateLeaseAsync(lease);
                return Json(new { success = result, message = result ? "Lease updated successfully." : "Error updating lease." });
            }
        }


        public async Task<IActionResult> EditLease(int leaseId)
        {
            LeaseDto lease;

            if (leaseId > 0)
            {
                lease = await _authService.GetLeaseByIdAsync(leaseId);

                if (lease == null)
                {
                    return NotFound();
                }
            }
            else
            {
                lease = new LeaseDto();
            }

            return View("AddLease", lease);
        }
        
        public async Task<IActionResult> InvoiceDetails(int leaseId)
        {
            List<InvoiceDto> invoices = await _authService.GetInvoicesAsync(leaseId);

            ViewBag.Paid = invoices?.Where(x => x.InvoicePaid == true).Select(x => x.RentAmount).Sum() ?? 0;
            ViewBag.UnPaid = invoices?.Where(x => x.InvoicePaid != true).Select(x => x.RentAmount).Sum() ?? 0;
            ViewBag.OwnerPaid = invoices?.Where(x => x.InvoicePaidToOwner == true).Select(x => x.RentAmount).Sum() ?? 0;
            ViewBag.OwnerUnPaid = invoices?.Where(x => x.InvoicePaidToOwner != true).Select(x => x.RentAmount).Sum() ?? 0;

            return View(invoices);
        }

        public async Task<IActionResult> AllInvoicePaid(int leaseId)
        {
            await _authService.AllInvoicePaidAsync(leaseId);
            return Json(new { success = true, message = "All invoice paid successfully" });
        }

        public async Task<IActionResult> AllInvoiceOwnerPaid(int leaseId)
        {
            await _authService.AllInvoiceOwnerPaidAsync(leaseId);
            return Json(new { success = true, message = "All invoice paid to owner successfully" });
        }

        public async Task<IActionResult> InvoicePaid(int invoiceId)
        {
            await _authService.InvoicePaidAsync(invoiceId);
            return Json(new { success = true, message = "Invoice paid successfully" });
        }

        public async Task<IActionResult> InvoiceOwnerPaid(int invoiceId)
        {
            await _authService.InvoiceOwnerPaidAsync(invoiceId);
            return Json(new { success = true, message = "Invoice paid to owner successfully" });
        }

        public async Task<IActionResult> DownloadInvoice(int id)
        {
            InvoiceDto invoice = await _authService.GetInvoiceByIdAsync(id);
            var lease = await _authService.GetLeaseByIdAsync(invoice.LeaseId.Value);

            ViewBag.invoice = invoice;
            return View(lease);
        }




    }
}
