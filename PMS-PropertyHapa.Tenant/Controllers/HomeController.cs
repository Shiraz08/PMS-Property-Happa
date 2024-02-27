using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Tenant.Models;
using System.Diagnostics;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public HomeController(IWebHostEnvironment Environment)
        {
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

    }
}