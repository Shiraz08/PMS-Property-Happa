﻿using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult RentReports()
        {
            return View();
        }
        public IActionResult LeaseReports()
        {
            return View();
        }
        public IActionResult InvoiceReports()
        {
            return View();
        }
    }
}