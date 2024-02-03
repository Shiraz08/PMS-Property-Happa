using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using PMS_PropertyHapa.Admin.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Shared.Email;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly UserManager<ApplicationUser> _userManager;
        private PropertyHapaAdminContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
 
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        EmailSender _emailSender = new EmailSender();
        public AccountController(IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, PropertyHapaAdminContext context, IUserStore<ApplicationUser> userStore)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            LoginRequestDTO login = new LoginRequestDTO();
            login.ReturnUrl = returnUrl;
            return View(login);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser appUser = await _userManager.FindByEmailAsync(login.Email);
                    if (appUser != null)
                    {
                        await _signInManager.SignOutAsync();
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, login.Password, login.Remember, false);

                        if (result.Succeeded)
                            return Redirect(login.ReturnUrl ?? "/");

                        if (result.IsLockedOut)
                            ModelState.AddModelError("", "Your account is locked out. Kindly wait for 10 minutes and try again");
                    }
                    ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
                }
                catch (Exception e)
                {

                    throw;
                }
            }
            return View(login);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
