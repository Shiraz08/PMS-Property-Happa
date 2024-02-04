using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Admin.Data;

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
        public AdminController(UserManager<ApplicationUser> usrMgr, IPasswordHasher<ApplicationUser> passwordHash, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signinMgr, IWebHostEnvironment webHostEnvironment, PropertyHapaAdminContext context, IEmailSender emailSender)
        {
            userManager = usrMgr;
            _passwordHasher = passwordHash;
            _UserManager = userMgr;
            _SignInManager = signinMgr;
            _context = context;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create() => View();
        [HttpPost]
        public async Task<JsonResult> Create(ApplicationUser user)
        {
            var selectdate = Request.Form.Where(x => x.Key == "BirthDate").FirstOrDefault().Value.ToString();
            if (selectdate != "")
            {
                DateTime dob = DateTime.Parse(selectdate);
                user.BirthDate = dob;
            }
            else
            {
                user.BirthDate = null;
            }
            ApplicationUser appUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                AddedBy = User.Identity?.Name,
                AddedDate = DateTime.Now,
                Status = true,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Country = user.Country,
                Group = user.Group
            };
            IdentityResult result = null;

            try
            {
                result = await userManager.CreateAsync(appUser, user.Password);
            }
            catch (Exception e)
            {

                throw;
            }
            if (result.Succeeded)
            {
                //Add Role
                await _UserManager.AddToRoleAsync(appUser, user.Group);
                return Json(true);
            }
            else
            {
                return Json(result.Errors.Select(x => x.Description).FirstOrDefault());
            }
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
            return Json(false);
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
            var list = _context.Users.Where(x => x.Status == true).ToList();
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
