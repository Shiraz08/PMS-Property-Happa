using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Tenant.Services.IServices;
using PMS_PropertyHapa.Tenant.Models;
using System.Diagnostics;
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.Tenant.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger,IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

      
        public async Task<IEnumerable<CommunicationDto>> ViewCommunicationsByTenantId(string tenantId)
        {
            var communications = await _authService.GetAllCommunicationAsync();
            return communications.Where(c => c.TenantIds != null && c.TenantIds.Split(',').Contains(tenantId)).ToList();
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}