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
using PMS_PropertyHapa.MigrationsFiles.Migrations;
using PMS_PropertyHapa.Shared.Twilio;
using PMS_PropertyHapa.Models.Stripe;
using static PMS_PropertyHapa.Shared.Enum.SD;
using PMS_PropertyHapa.Services;

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
        private readonly IConfiguration _configuration;
        private readonly IStripeService _stripeService;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore, IConfiguration configuration, IStripeService stripeService)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
            _configuration = configuration;
            _stripeService = stripeService;
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
        public IActionResult Register()
        {
            //var model = new RegisterationRequestDTO();
            //ViewBag.SubscriptionType = subscription; 
            //ViewBag.SubscriptionId = id; 
            //return View(model);
            return View();
        }
        [HttpGet("Subscription/Success")]
        public IActionResult Success(string sessionId)
        {
            // Optionally, you can retrieve the session details using the sessionId if needed.
            ViewBag.SessionId = sessionId;
            return View();
        }
        [HttpGet("Subscription/Cancel")]
        public IActionResult Cancel()
        {
            return View();
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
                    SubscriptionId = model.SubscriptionId,
                    PhoneNumber = model.PhoneNumber,
                    CompanyName = model.CompanyName,
                    PropertyTypeId = model.PropertyTypeId,
                    Units = model.Units,
                    LeadGenration = model.LeadGenration,
                    CardholderName = model.CardholderName,
                    CardNumber = model.CardNumber,
                    ExpirationDate = model.ExpirationDate,
                    CVV = model.CVV,
                    StreetAdress = model.StreetAdress,
                    City = model.City,
                    State = model.State,
                    Currency = model.Currency,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "PropertyManager");
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    await _userManager.UpdateAsync(user);

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
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email.", htmlContent);
                    await _context.SaveChangesAsync();
                    model.UserId = user.Id;
                    return await SavePayment(model);
                    //return Ok(new { success = true, message = "User registered successfully." });
                }
                var errorMessage = "";
                foreach (var error in result.Errors)
                {
                    errorMessage += error.Description + " ";
                }
                return Ok(new { success = false, message = errorMessage });
            }

            return Ok(new { success = false, message = "Model is not valid." });
        }

        [HttpPost]
        public async Task<IActionResult> SavePayment([FromBody] RegisterationRequestDTO productModel)
        {
            var _stripeSettings = _configuration.GetSection("StripeSettings");

            var CurrentUser = await _authService.GetProfileAsync(productModel.UserId);
            Guid newGuid = Guid.NewGuid();
            try
            {
                PaymentGuid paymentGatewaysGuid = new PaymentGuid
                {
                    Guid = newGuid.ToString(),
                };
                ProductModel product = new ProductModel
                {
                    Id = productModel.SubscriptionId ?? 0,
                    Title = productModel.SubscriptionName,
                    Description = (productModel.Units ?? 0).ToString() + " Units",
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
                    Price = productModel.Price * 100,  //cents to dollar
                    Currency = "USD"
                };

                CheckoutData request = new CheckoutData
                {
                    CustomerEmail = CurrentUser.Email,
                    UserId = CurrentUser.UserId,
                    Quantity = 1,
                    Product = product,
                    PaymentGuid = paymentGatewaysGuid,
                    PaymentMode = PaymentMode.Subscription,
                    PaymentInterval = productModel.IsYearly ? PaymentInterval.Year : PaymentInterval.Month,
                    PaymentIntervalCount = 1,
                    SuccessCallbackUrl = _stripeSettings["SuccessCallbackUrl"],
                    CancelCallbackUrl = _stripeSettings["CancelCallbackUrl"],
                };
                var sessionId = await _stripeService.CreateSessionAsync(request, productModel.IsTrial);
                var pubKey = _stripeSettings["PublicKey"];

                var CheckouOrderResponse = new CheckoutOrderDto()
                {
                    SessionId = sessionId,
                    PubKey = pubKey,
                };
                PaymentGuidDto paymentGuid = new PaymentGuidDto
                {
                    Guid = newGuid.ToString(),
                    Description = "CheckOut to Dashboard",
                    DateTime = DateTime.Now,
                    SessionId = sessionId,
                    UserId = CurrentUser.UserId,
                };
                //SAve Payment Guid Api
                await _authService.SavePaymentGuid(paymentGuid);
                return Ok(new { success = true, message = "user created successfully.", PubKey = CheckouOrderResponse.PubKey, SessionId = CheckouOrderResponse.SessionId });
            }
            catch (Exception exp)
            {
                throw;
            }
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
        public async Task<ActionResult> GetAllPropertyTypes()
        {
            try
            {
                var propertyTypes = await _authService.GetAllPropertyTypesAsync();
                return Ok(propertyTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPropertyTypesDll()
        {
            try
            {
                var filter = new Filter();
                var propertyTypes = await _authService.GetAllPropertyTypesDllAsync(filter);
                return Ok(propertyTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching Communications: {ex.Message}");
            }

        }

        [HttpGet]
        public async Task<IActionResult> SendEmailOTP(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            var success = await _authService.IsEmailExists(email);
            if (!success)
            {
                var otp = GenerateOTP();
                var model = new OTPDto
                {
                    Code = otp,
                    Email = email
                };

                string htmlContent = $@"<!DOCTYPE html>
                                        <html lang=""en"">
                                        <head>
                                            <meta charset=""UTF-8"">
                                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                            <title>OTP Verification</title>
                                        </head>
                                        <body>
                                            <div style=""font-family: Arial, sans-serif;"">
                                                <h2>OTP Verification</h2>
                                                <p>Hello,</p>
                                                <p>Your OTP (One Time Password) for email verification is:</p>
                                                <p style=""font-size: 24px; color: #007bff; padding: 10px 20px; background-color: #f4f4f4; border-radius: 5px;"">{otp}</p>
                                                <p>Please enter this OTP on the verification page to confirm your email address.</p>
                                                <p>If you did not request this verification, you can safely ignore this email.</p>
                                                <p>Thank you,</p>
                                            </div>
                                        </body>
                                        </html>
                                        ";
                await _emailSender.SendEmailAsync(email, "Confirm your email.", htmlContent);

                await _authService.SaveEmailOTP(model);

                return Ok(new { success = true, message = "Please Check Your email for OTP." });

            }
            else
            {
                return Ok(new { success = false, message = "Email Already Exists." });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> VerifyEmailOTP(OTPDto model)
        {
            var success = await _authService.IsEmailOTPValid(model);
            if (success)
            {
                return Ok(new { success = true, message = "Email verification successfull" });
            }
            else
            {
                return Ok(new { success = false, message = "please enter correct OTP." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SendPhoneOTP(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest("Phone Number is required.");
            }

            var success = await _authService.IsPhoneNumberExists(phoneNumber);
            if (!success)
            {
                var otp = GenerateOTP();
                var model = new OTPDto
                {
                    Code = otp,
                    PhoneNumber = phoneNumber
                };

                //Twilio need to replace cresentials
                //var message = $"Your OTP (One Time Password) for verification is: {otp}. Please enter this OTP to confirm your Phone number.";
                //var smsSender = new SMSSender();
                //await smsSender.SmsSender(model.PhoneNumber, message, "accounSid", "authToken", "twilioPhoneNumber");

                await _authService.SavePhoneOTP(model);

                return Ok(new { success = true, message = "Please check your phone for OTP." });

            }
            else
            {
                return Ok(new { success = false, message = "Phone number Already Exists." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPhoneOTP(OTPDto model)
        {
            var success = await _authService.IsPhoneOTPValid(model);
            if (success)
            {
                return Ok(new { success = true, message = "Phone number verification successfull." });
            }
            else
            {
                return Ok(new { success = false, message = "Please enter correct OTP." });
            }
        }

        private string GenerateOTP()
        {
            Random rand = new Random();
            int otp = rand.Next(100000, 999999);
            return otp.ToString();
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
