﻿using Microsoft.AspNetCore.Mvc;

namespace PMS_PropertyHapa.Owner.Controllers
{
    public class RequestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
