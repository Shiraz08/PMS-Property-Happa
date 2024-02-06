﻿using Microsoft.AspNetCore.Authentication;
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
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }
        [HttpGet]
        public IActionResult Login()
        {
            // Check if there's an error message passed via ViewBag and pass it to the view if needed
            ViewBag.ErrorMessage = ViewBag.ErrorMessage ?? string.Empty;
            LoginRequestDTO obj = new();
            return View(obj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            var response = await _authService.LoginAsync<APIResponse>(obj);
            if (response != null && response.IsSuccess)
            {
                return Json(new { success = true, message = "Logged In Successfully..!", result = response.Result });
            }
            else
            {
                var errorMessage = response?.ErrorMessages?.FirstOrDefault() ?? "An unexpected error occurred.";
                return Json(new { success = false, message = errorMessage });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var profileModel = await _authService.GetProfileAsync(userId);
            if (profileModel == null)
            {
                return NotFound("User not found");
            }

            return Ok(profileModel);
        }


        // Action to update profile information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEditProfile(ProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _authService.UpdateProfileAsync(model);

            if (success)
            {

                return RedirectToAction("ProfileUpdated");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the profile.");
            return View(model);
        }



        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                  new SelectListItem{Text=SD.Admin,Value=SD.Admin},
                new SelectListItem{Text=SD.User,Value=SD.User},
            };
            ViewBag.RoleList = roleList;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO obj)
        {
            if (string.IsNullOrEmpty(obj.Role))
            {
                obj.Role = SD.User;
            }
            APIResponse result = await _authService.RegisterAsync<APIResponse>(obj);
            if (result != null && result.IsSuccess)
            {
                return RedirectToAction("Login");
            }
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.Admin,Value=SD.Admin},
                new SelectListItem{Text=SD.Customer,Value=SD.Customer},
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordDto
            {
                Email = email
            };
            return View(model);
        }


        [HttpGet]
        public IActionResult UpdateProfile()
        {
            var model = new ProfileModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    statusCode = 400, 
                    isSuccess = false,
                    errorMessages = new List<string> { "Validation failed. Please check the input fields." },
                    result = (string)null 
                });
            }

            var response = await _authService.ForgotPasswordAsync(model);

            if (response != null && response.IsSuccess)
            {
               
                return Json(new
                {
                    statusCode = 200, 
                    isSuccess = true,
                    errorMessages = new List<string>(), 
                    result = "Reset password email sent successfully" 
                });
            }
            else
            {
              
                string errorMessage = response?.ErrorMessages?.FirstOrDefault() ?? "An unexpected error occurred. Please try again.";
                return Json(new
                {
                    statusCode = 400,
                    isSuccess = false,
                    errorMessages = new List<string> { errorMessage },
                    result = (string)null 
                });
            }
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            var token = _tokenProvider.GetToken();
            await _authService.LogoutAsync<APIResponse>(token);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
