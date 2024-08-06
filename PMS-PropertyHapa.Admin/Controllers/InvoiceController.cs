using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class InvoiceController : Controller
    {
        private ApiDbContext _context;

        public InvoiceController( ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
      

    }
}
