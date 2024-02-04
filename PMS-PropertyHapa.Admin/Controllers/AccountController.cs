using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Plugins;
using PMS_PropertyHapa.Admin.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Shared.Enum;

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

        [HttpGet]
        public IActionResult Registration()
        {
            var roleList = new List<SelectListItem>()
            {
                  new SelectListItem{Text=SD.Admin,Value=SD.Admin},
                new SelectListItem{Text=SD.Customer,Value=SD.Customer},
            };
            ViewBag.RoleList = roleList;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {
            try
            {
                ApplicationUser appUser = _context.Users.Where(x => x.Email == login.Email).FirstOrDefault();
                if (appUser != null)
                {
                    try
                    {


                        await _signInManager.SignOutAsync();
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, login.Password, login.Remember, false);

                        if (result.Succeeded)
                            return RedirectToAction("Index", "Home");

                        if (result.IsLockedOut)
                            ModelState.AddModelError("", "Your account is locked out. Kindly wait for 10 minutes and try again");
                    }
                    catch(Exception ex) { }
                    }
                ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
            }
            catch (Exception e)
            {

                throw;
            }
            return View(login);
        }
        public JsonResult DoesUserEmailExist(string email)
        {
            if (_context.Users.Any(o => o.Email == email))
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegisterationRequestDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name }; 
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                   
                     await _userManager.AddToRoleAsync(user, model.Role);

                 

                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var checkPassword = await _userManager.CheckPasswordAsync(user, model.currentPassword);
            if (!checkPassword)
            {
                ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                return View(model);
            }
            
            if (model.newPassword != model.newRepeatPassword)
            {
                ModelState.AddModelError(string.Empty, "New password and confirmation password do not match.");
                return View(model);
            }
            var result = await _userManager.ChangePasswordAsync(user, model.currentPassword, model.newPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordRequestDto(); 
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
    }
}
