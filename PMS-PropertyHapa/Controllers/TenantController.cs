﻿using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class TenantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddTenant()
        {
            return View();
        }
    }
}
