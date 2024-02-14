using FluentFTP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.ProjectModel;
using PMS_PropertyHapa.Admin.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using System.Net;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace PMS_PropertyHapa.Admin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private IPasswordHasher<ApplicationUser> _passwordHasher;
        private UserManager<ApplicationUser> _UserManager;
        private SignInManager<ApplicationUser> _SignInManager;
        private PropertyHapaAdminContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        public AdminController(UserManager<ApplicationUser> usrMgr, IPasswordHasher<ApplicationUser> passwordHash, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signinMgr, IWebHostEnvironment webHostEnvironment, PropertyHapaAdminContext context, IEmailSender emailSender, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            userManager = usrMgr;
            _passwordHasher = passwordHash;
            _UserManager = userMgr;
            _SignInManager = signinMgr;
            _context = context;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> Create(TenantViewModel model)
        {
            if (model == null || model.User == null || model.OrganizationInfo == null)
            {
                return Json(new { success = false, message = "Invalid model data." });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var existingUser = await userManager.FindByNameAsync(model.User.UserName) ??
                                       await userManager.FindByEmailAsync(model.User.Email);
                    if (existingUser != null)
                    {
                        return Json(new { success = false, message = "Username or email already exists." });
                    }

                    var appUser = new ApplicationUser
                    {
                        UserName = model.User.UserName,
                        Email = model.User.Email,
                        AddedBy = User.Identity?.Name,
                        AddedDate = DateTime.Now,
                        Status = true,
                        BirthDate = model.User.BirthDate,
                        PhoneNumber = model.User.PhoneNumber,
                        FirstName = model.User.FirstName,
                        LastName = model.User.LastName,
                        Country = model.User.Country,
                        Group = model.User.Group
                    };

                    var createUserResult = await userManager.CreateAsync(appUser, model.User.Password);
                    if (!createUserResult.Succeeded)
                    {
                        var errors = string.Join("; ", createUserResult.Errors.Select(e => e.Description));
                        return Json(new { success = false, message = $"User creation failed: {errors}" });
                    }

                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    var orgIconFileName = await ProcessFileUpload(model.OrganizationInfo.OrganizationIconFile, uploadsFolder);
                    var orgLogoFileName = await ProcessFileUpload(model.OrganizationInfo.OrganizationLogoFile, uploadsFolder);
                    if (orgIconFileName == null || orgLogoFileName == null)
                    {
                        return Json(new { success = false, message = "File upload failed." });
                    }

                    

                    await userManager.AddToRoleAsync(appUser, model.User.Group);

                    var tenantOrgInfo = new TenantOrganizationInfo
                    {
                        TenantUserId = Guid.Parse(appUser.Id),
                        OrganizationName = model.OrganizationInfo.OrganizationName,
                        OrganizationDescription = model.OrganizationInfo.OrganizationDescription,
                        OrganizationIcon = orgIconFileName,
                        OrganizationLogo = orgLogoFileName,
                        OrganizatioPrimaryColor = model.OrganizationInfo.OrganizatioPrimaryColor,
                        OrganizationSecondColor = model.OrganizationInfo.OrganizationSecondColor
                    };

                    _context.TenantOrganizationInfo.Add(tenantOrgInfo);
                    await _context.SaveChangesAsync();
                    System.IO.File.Delete(Path.Combine(uploadsFolder, orgLogoFileName));
                    System.IO.File.Delete(Path.Combine(uploadsFolder, orgIconFileName));
                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
                }
            }
        }


        

        private async Task<string> ProcessFileUpload(IFormFile file, string uploadsFolder)
        {
            if (file != null && file.Length > 0)
            {
                var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                // Ensure the filename is safe to use
                var safeFileName = WebUtility.HtmlEncode(originalFileName);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return uniqueFileName; 
            }

            return null; 
        }

        public async Task<IActionResult> Update(string id)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            var userlist = _context.Users.Where(x => x.Id == id).FirstOrDefault();
            if (user != null)
            {
                ViewBag.Group = userlist.Group;
                ViewBag.Country = userlist.Country;
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public JsonResult Update(ApplicationUser applicationUser)
        {
            ApplicationUser user = _context.Users.Find(applicationUser.Id);
            if (user != null)
            {
                Nullable<DateTime> bdate = null;
                var date = Request.Form.Where(x => x.Key == "BirthDate").FirstOrDefault().Value.ToString();
                if (date != "")
                {
                    bdate = Convert.ToDateTime(Request.Form.Where(x => x.Key == "BirthDate").FirstOrDefault().Value.ToString());
                }
                if (!string.IsNullOrEmpty(applicationUser.Email) && !string.IsNullOrEmpty(applicationUser.FirstName) && !string.IsNullOrEmpty(applicationUser.LastName))
                {
                    user.Email = applicationUser.Email;
                    user.ModifiedBy = User.Identity?.Name;
                    user.ModifiedDate = DateTime.Now;
                    user.BirthDate = bdate;
                    user.PhoneNumber = applicationUser.PhoneNumber;
                    user.FirstName = applicationUser.FirstName;
                    user.LastName = applicationUser.LastName;
                    var Result = _context.Update(user);
                    _UserManager.AddToRoleAsync(applicationUser, applicationUser.Group);
                    return Json(true);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return Json(new { success = false});
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            ApplicationUser role = _context.Users.Find(id);
            if (role != null)
            {
                role.Status = false;
                _context.Users.Update(role);
                _context.SaveChanges();
                return Json(true);
            }
            else
                return Json(false);
        }



        public JsonResult GetAllUser()
        {
            // Fetch all users marked as active
            var users = _context.Users
                        .Where(user => user.Status == true)
                        .ToList();

            // Fetch all TenantOrganizationInfos
            var tenantInfos = _context.TenantOrganizationInfo.ToList();
            var list = users.Select(user => {
                Guid userIdGuid = Guid.Empty;
                bool isValidGuid = Guid.TryParse(user.Id, out userIdGuid);
                var tenantInfo = isValidGuid ? tenantInfos.FirstOrDefault(info => info.TenantUserId == userIdGuid) : null;

                return new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.AddedDate,
                    OrganizationName = tenantInfo?.OrganizationName,
                    OrganizationLogo = tenantInfo?.OrganizationLogo,
                    OrganizationIcon = tenantInfo?.OrganizationIcon
                };
            }).ToList();

            return Json(list);
        }


        public JsonResult GetFilterAllUser(string Searchtext, string Shop)
        {
            var list = _context.Users.Where(x => x.Status == true).ToList();
            if (Searchtext != null)
            {
                list = list.Where(x => (x.UserName.ToLower().Contains(Searchtext.ToLower())  || x.Email.ToLower() == Searchtext.ToLower())).ToList();
            }
            return Json(list);
        }
    }
}
