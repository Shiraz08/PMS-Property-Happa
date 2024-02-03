using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.Models.DTO;

namespace PMS_PropertyHapa.API.Controllers.V1
{
    public class teontroller : Controller
    {
        private readonly DbContext _context;


        [HttpPost]
        public async Task<IActionResult> RegisterTenant(RegisterationRequestDTO model)
        {
            return Ok();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
