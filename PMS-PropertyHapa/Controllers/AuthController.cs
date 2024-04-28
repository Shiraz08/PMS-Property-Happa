﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Enum;
using PMS_PropertyHapa.Shared.ImageUpload;
using PMS_PropertyHapa.Services.IServices;
using System.Text.Encodings.Web;
using PMS_PropertyHapa.Shared.Email;
using System.Net;
using System.Text;
using System.Web;

namespace PMS_PropertyHapa.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        EmailSender _emailSender = new EmailSender();
        private IWebHostEnvironment _environment;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.ErrorMessage = ViewBag.ErrorMessage ?? string.Empty;
            LoginRequestDTO obj = new();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {
            try
            {
                ApplicationUser appUser = _context.Users.FirstOrDefault(x => x.Email == login.Email);
                if (appUser == null)
                {
                    ModelState.AddModelError("", "Login Failed: Invalid Email or password.");
                    return View(login);
                }

                var result = await _authService.LoginAsync<APIResponse>(login);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(login);
                }

                if (!await _userManager.IsInRoleAsync(appUser, "PropertyManager"))
                {
                    ModelState.AddModelError("", "Login Failed: Only Tenants are allowed to log in.");
                    return View(login);
                }

                return Json(new
                {
                    success = true,
                    message = "Logged In Successfully..!",
                    result = new
                    {
                        userId = appUser.Id,
                        tenantId = appUser.TenantId,
                        organization = new
                        {
                            tenant = "",
                            icon = ""
                        }
                    }
                });

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "An unexpected error occurred during login. Please try again later.");
                return View(login);
            }
        }



        [HttpGet]
        public IActionResult Register(string subscription,int id)
        {
            var model = new RegisterationRequestDTO();
            ViewBag.SubscriptionType = subscription; 
            ViewBag.SubscriptionId = id; 
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterationRequestDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    SubscriptionName = model.SubscriptionName,
                    SubscriptionId = model.SubscriptionId
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "PropertyManager");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedCode = HttpUtility.UrlEncode(code);
                    var callbackUrl = Url.Action("ConfirmEmail", "Auth", new { name = user.UserName, code = encodedCode }, protocol: HttpContext.Request.Scheme);
                    string htmlContent = $@"<!DOCTYPE html>
                                            <html lang=""en"">
                                            <head>
                                                <meta charset=""UTF-8"">
                                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                                <title>Email Confirmation</title>
                                            </head>
                                            <body>
                                                <div style=""font-family: Arial, sans-serif;"">
                                                    <h2>Confirm Your Email Address</h2>
                                                    <p>Hello,</p>
                                                    <p>Please confirm your email address by clicking the link below:</p>
                                                    <p><a href=""{callbackUrl}"" target=""_blank"" style=""background-color: #007bff; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Confirm Email Address</a></p>
                                                    <p>If you did not create an account, you can safely ignore this email.</p>
                                                    <p>Thank you,</p>
                                                </div>
                                            </body>
                                            </html>";
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email", htmlContent);

                    return RedirectToAction("ConfirmEmailPage", "Auth");
                }

                foreach (var error in result.Errors)
                {
                    ViewBag.SubscriptionType = model.SubscriptionName;
                    ViewBag.SubscriptionId = model.SubscriptionId;
                    ModelState.AddModelError("", error.Description);
                    //return RedirectToAction("Auth", "Register");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string name, string code)
        {
            if (name == null || code == null)
            {
                return RedirectToAction("ConfirmEmailFailure");
            }
           
            return RedirectToAction("Index2", new { username = name });
        }

        [HttpGet]
        public IActionResult Index2(string username)
        {
            ViewData["Username"] = username;
            return View();
        }
        
        [HttpGet]
        public IActionResult ConfirmEmailPage()
        {
            return View();
        }
    }
}