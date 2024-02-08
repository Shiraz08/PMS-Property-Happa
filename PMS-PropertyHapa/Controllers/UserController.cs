using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PMS_PropertyHapa.API.Areas.Identity.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Services.IServices;
using PMS_PropertyHapa.Shared.Enum;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PMS_PropertyHapa.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public UserController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }


        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> DoesEmailExist(string email)
        //{
        //    var userExists = await _userManager.FindByEmailAsync(email) != null;
        //    return Json(!userExists);
        //}


        //[HttpPost]
        //public async Task<IActionResult> CreateTenant(ApplicationUser model, string password, string group)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser
        //        {
        //            UserName = model.UserName,
        //            Email = model.Email,
        //            // Set other properties as necessary
        //        };

        //        var result = await _userManager.CreateAsync(user, password);

        //        if (result.Succeeded)
        //        {
        //            // Optionally, add the user to a role
        //            await _userManager.AddToRoleAsync(user, group);
        //            return Json(new { success = true });
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Failed to create tenant" });
        //        }
        //    }
        //    return Json(new { success = false, message = "Invalid model state" });
        //}


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching users: {ex.Message}");
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> DeleteUser(string id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user != null)
        //    {
        //        var result = await _userManager.DeleteAsync(user);
        //        if (result.Succeeded)
        //        {
        //            return Json(new { success = true });
        //        }
        //    }
        //    return Json(new { success = false, message = "User not found" });
        //}



        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}
    }
}
