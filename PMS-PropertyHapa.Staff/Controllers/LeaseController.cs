using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
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
            return View(lease);
        }

        public IActionResult AddLease()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(LeaseDto lease)
        {
            if (lease == null)
            {
                return Json(new { success = false, message = "lease data is  empty" });
            }
            else
            {
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
            return Json(new { success = true, data = leases });
        }

        [HttpPost]
        public async Task<IActionResult> Update(LeaseDto lease)
        {
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

    }
}
