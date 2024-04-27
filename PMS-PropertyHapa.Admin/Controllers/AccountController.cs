using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Shared.Enum;
using System.Security.Cryptography;
using System.Text;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        private readonly string EncryptionKey = "bXlTZWN1cmVLZXlIZXJlMTIzNDU2Nzg5";

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        EmailSender _emailSender = new EmailSender();
        public AccountController(IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore)
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
            var user = _context.Users.Where(x => x.Email == login.Email).FirstOrDefault();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(login);
            }

            // Manually verify the password
            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid login attempt. Please try again.");
                return View(login);
            }

            // Manually sign in the user
            if (passwordVerificationResult == PasswordVerificationResult.Success)
            {
                await _signInManager.SignInAsync(user, isPersistent: login.Remember);
                _logger.LogInformation("User {Email} logged in successfully.", user.Email);
                return RedirectToAction("Index", "Home");
            }

            // Fallback for any other case (such as needing a rehash)
            ModelState.AddModelError("", "Invalid login attempt. Please try again.");
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


        public IActionResult ForgotPassword()
        {
            return View(); 
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


        [HttpPost]
        public async Task<IActionResult> ForgotPasswordRequest(ForgetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No user found with this email.");
                return BadRequest(new { isSuccess = false });
            }

            try
            {
                // Encrypt the email
                var encryptedEmail = await EncryptEmail(model.Email);

                var baseUrl = $"https://localhost:7220";
                var resetPasswordUrl = $"{baseUrl}/Account/ResetPassword?email={encryptedEmail}";

                string emailContent = $"To reset your password, follow this link: {resetPasswordUrl}";
                string subject = "Reset Password Request";
                await _emailSender.SendEmailAsync(model.Email, subject, emailContent);

                return Ok(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false });
            }
        }


        [HttpGet("Account/ResetPassword")]
        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordDto { Email = email };
            return View(model);
        }

        private async Task<string> EncryptEmail(string email)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(email);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    email = Convert.ToBase64String(ms.ToArray());
                }
            }
            return email;
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string email;
            try
            {
                email = await DecryptEmail(model.Email);
            }
            catch
            {
                return BadRequest("Invalid reset link.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("The password and confirmation password do not match.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.Password = model.Password;
            user.LockoutEnabled = true;
            user.Group = "Admin";

            _context.Users.Update(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "An error occurred while resetting the password.");
            }

            return Ok("Password has been reset successfully");
        }

        public async Task<string> DecryptEmail(string encryptedEmail)
        {
            encryptedEmail = encryptedEmail.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encryptedEmail);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    encryptedEmail = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return encryptedEmail;
        }

    }
}
