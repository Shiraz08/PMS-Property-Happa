using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Services.IServices;

namespace PMS_PropertyHapa.Controllers
{
    public class AssestsController : Controller
    {

        private readonly IAuthService _authService;

        public AssestsController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        public async Task<IActionResult> AddAsset([FromBody] AssetDTO assetDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.CreateAssetAsync(assetDTO);
                return Ok(new { success = true, message = "Asset added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while adding the asset." });
            }
        }



        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddAssest()
        {
            return View();
        }
    }
}
