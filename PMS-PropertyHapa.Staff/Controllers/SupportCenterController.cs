using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PMS_PropertyHapa.Models.Configrations;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class SupportCenterController : Controller
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly AdminInfo _adminInfo;
        private readonly IPermissionService _permissionService;


        public SupportCenterController(IAuthService authService, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IOptions<AdminInfo> adminInfo, IPermissionService permissionService)
        {
            _authService = authService;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _adminInfo = adminInfo.Value;
            _permissionService = permissionService;
        }
        public IActionResult AddTickets()
        {
            return View();
        }
        public IActionResult ViewTickets()
        {
            return View();
        }
        public async Task<IActionResult> Index()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewSupportCenter);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> FAQ()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewFAQ);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            IEnumerable<FAQ> faq = new List<FAQ>();
            faq = await _authService.GetFAQsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            return View(faq);
        }
        public async Task<IActionResult> ContactUs()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ContactUs);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> SaveContactUs(ContactUsDto contactUsDto)
        {
            string htmlContent = $@"<!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Contact Us Form Submission</title>
                        </head>
                        <body>
                            <div style=""font-family: Arial, sans-serif;"">
                                <h2>Contact Us Form Submission</h2>
                                <p>Hello Admin,</p>
                                <p>You have received a new message from the Contact Us form on your website. Here are the details:</p>
                                <p><strong>Name:</strong> {contactUsDto.Name}</p>
                                <p><strong>Email:</strong> {contactUsDto.Email}</p>
                                <p><strong>Phone Number:</strong> {contactUsDto.PhoneNumber}</p>
                                <p><strong>Message:</strong></p>
                                <p>{contactUsDto.Message}</p>
                                <p>Thank you,</p>
                                <p>Your Website Team</p>
                            </div>
                        </body>
                        </html>";

            await _emailSender.SendEmailAsync(_adminInfo.Email, "Confirm your email.", htmlContent);

            return Ok();
        }
        public async Task<IActionResult> BookDemo()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.BookAFreeDemo);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            return View();
        }
        public async Task<IActionResult> SaveBookDemo(BookDemoDto bookDemoDto)
        {
            string htmlContent = $@"<!DOCTYPE html>
                                <html lang=""en"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Book a Demo Request</title>
                                </head>
                                <body>
                                    <div style=""font-family: Arial, sans-serif;"">
                                        <h2>Book a Demo Request</h2>
                                        <p>Hello Admin,</p>
                                        <p>You have received a new demo request from your website. Here are the details:</p>
                                        <p><strong>First Name:</strong> {bookDemoDto.FirstName}</p>
                                        <p><strong>Last Name:</strong> {bookDemoDto.LastName}</p>
                                        <p><strong>Email:</strong> {bookDemoDto.Email}</p>
                                        <p><strong>Phone Number:</strong> {bookDemoDto.PhoneNumber}</p>
                                        <p><strong>Company Name:</strong> {bookDemoDto.CompanyName}</p>
                                        <p><strong>Total Units:</strong> {bookDemoDto.TotalUnits}</p>
                                        <p>Thank you,</p>
                                        <p>Your Website Team</p>
                                    </div>
                                </body>
                                </html>";
            await _emailSender.SendEmailAsync(_adminInfo.Email, "Confirm your email.", htmlContent);
            return Ok();
        }
        public async Task<IActionResult> VideoTutorial()
        {
            var userId = Request?.Cookies["userId"]?.ToString();

            bool hasAccess = await _permissionService.HasAccess(userId, (int)UserPermissions.ViewVideoTutorial);
            if (!hasAccess)
            {
                return Unauthorized();
            }
            IEnumerable<VideoTutorial> vt = new List<VideoTutorial>();
            vt = await _authService.GetVideoTutorialsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            return View(vt);
        }


    }
}
