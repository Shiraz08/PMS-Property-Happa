using Microsoft.AspNetCore.Authentication;
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
        public IActionResult Register()
        {
            var model = new RegisterationRequestDTO();
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
                    EmailConfirmed = false  
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "PropertyManager");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedCode = HttpUtility.UrlEncode(code);
                    var callbackUrl = Url.Action("ConfirmEmail", "Auth", new { email = user.Email, code = encodedCode }, protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                    return RedirectToAction("Index", "Home");
                }
             
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return View("ConfirmEmailFailure");
            }
          else
            {
                return View("ConfirmEmailSuccess");

            }
        }
    }
}
