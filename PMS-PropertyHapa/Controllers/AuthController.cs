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
using Stripe;

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
                    CountryCode = model.CountryCode,
                    Country = model.Country,
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
                   // await _emailSender.SendEmailAsync(user.Email, "Confirm your email.", htmlContent);
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
            var stripeSettings = _configuration.GetSection("StripeSettings");
            var currentUser = await _authService.GetProfileAsync(productModel.UserId);
            Guid newGuid = Guid.NewGuid();

            try
            {
                var product = new ProductModel
                {
                    Id = productModel.SubscriptionId ?? 0,
                    Title = productModel.SubscriptionName,
                    Description = (productModel.Units ?? 0).ToString() + " Units",
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRG_TUukNvS-0E486weXLkJDpTubsAcdHdmKw&usqp=CAU",
                    Price = productModel.Price * 100,  //cents to dollar
                    Currency = "USD"
                };

                var priceOptions = new PriceCreateOptions
                {
                    UnitAmount = product.Price,
                    Currency = product.Currency,
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = productModel.IsYearly ? "year" : "month"
                    },
                    ProductData = new PriceProductDataOptions
                    {
                        Name = product.Title
                    }
                };
                var priceService = new PriceService();
                var price = await priceService.CreateAsync(priceOptions);

                var customerOptions = new CustomerCreateOptions
                {
                    Email = currentUser.Email,
                    Metadata = new Dictionary<string, string>
                            {
                                { "UserId", currentUser.UserId.ToString() },
                                { "ProductId", product.Id.ToString() },
                                { "ProductTitle", product.Title },
                                { "PaymentGuid", newGuid.ToString() },
                                { "IsTrial", productModel.IsTrial ? "true" : "false" }
                            }
                };
                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);

                var setupIntentOptions = new SetupIntentCreateOptions
                {
                    Customer = customer.Id,
                    PaymentMethodTypes = new List<string> { "card" },
                };
                var setupIntentService = new SetupIntentService();
                var setupIntent = await setupIntentService.CreateAsync(setupIntentOptions);

                PaymentGuidDto paymentGuid = new PaymentGuidDto
                {
                    Guid = newGuid.ToString(),
                    Description = "CheckOut to Dashboard",
                    DateTime = DateTime.Now,
                    SessionId = setupIntent.ClientSecret,
                    UserId = currentUser.UserId,
                };

                await _authService.SavePaymentGuid(paymentGuid);
                var pubKey = stripeSettings["PublicKey"];
                var checkoutOrderResponse = new CheckoutOrderDto
                {
                    ClientSecret = setupIntent.ClientSecret,
                    PubKey = pubKey,
                    CustomerId = customer.Id,
                    PriceId = price.Id,
                };

                return Ok(checkoutOrderResponse);
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine($"Error processing payment: {exp.Message}");
                return StatusCode(500, new { error = "An error occurred while processing your payment." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest request)
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentMethod = await paymentMethodService.AttachAsync(
                    request.PaymentMethodId,
                    new PaymentMethodAttachOptions
                    {
                        Customer = request.CustomerId,
                    }
                );

                var customerService = new CustomerService();
                await customerService.UpdateAsync(
                    request.CustomerId,
                    new CustomerUpdateOptions
                    {
                        InvoiceSettings = new CustomerInvoiceSettingsOptions
                        {
                            DefaultPaymentMethod = paymentMethod.Id,
                        },
                    }
                );

                var customer = await customerService.GetAsync(request.CustomerId);

                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = request.CustomerId,
                    Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = request.PriceId,
                },
            },
                    Metadata = customer.Metadata,
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CreateAsync(subscriptionOptions);

                var paymentIntent = subscription.LatestInvoice.PaymentIntent;

                var paymentInformationDto = new PaymentInformationDto
                {
                    TransactionId = paymentIntent.Id,
                    ProductPrice = paymentIntent.Amount / 100,
                    AmountCharged = paymentIntent.AmountReceived / 100,
                    ChargeDate = paymentIntent.Created,
                    PaymentStatus = paymentIntent.Status,
                    Currency = paymentIntent.Currency,
                    CustomerId = paymentIntent.CustomerId,
                };

                var result = await _authService.SavePaymentInformation(paymentInformationDto);

                var subscriptionDto = new Models.Stripe.StripeSubscriptionDto
                {
                    IsCanceled = false,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.CurrentPeriodEnd,
                    EmailAddress = customer.Email,
                    SubscriptionId = subscription.Id,
                    UserId = customer.Metadata.ContainsKey("UserId") ? customer.Metadata["UserId"] : null,
                    BillingInterval = subscription.Items.Data[0].Plan.Interval,
                    SubscriptionType = subscription.Items.Data[0].Price.Type,
                    IsTrial = customer.Metadata.ContainsKey("IsTrial") && bool.Parse(customer.Metadata["IsTrial"]),
                    GUID = customer.Metadata.ContainsKey("PaymentGuid") ? customer.Metadata["PaymentGuid"] : null,
                    Currency = subscription.Currency,
                    Status = subscription.Status,
                    CustomerId = subscription.CustomerId,
                };

                var paymentMethodInformationDto = new PaymentMethodInformationDto
                {
                    Country = paymentMethod.BillingDetails.Address.Country,
                    CardType = paymentMethod.Card.Brand,
                    CardHolderName = paymentMethod.BillingDetails.Name,
                    CardLast4Digit = paymentMethod.Card.Last4,
                    ExpiryMonth = paymentMethod.Card.ExpMonth.ToString(),
                    ExpiryYear = paymentMethod.Card.ExpYear.ToString(),
                    Email = paymentMethod.BillingDetails.Email,
                    GUID = customer.Metadata.ContainsKey("PaymentGuid") ? customer.Metadata["PaymentGuid"] : null,
                    PaymentMethodId = paymentMethod.Id,
                    CustomerId = paymentMethod.CustomerId,
                };

                var paymentMethodResult = await _authService.SavePaymentMethodInformation(paymentMethodInformationDto);
                var subscriptionResult = await _authService.SaveStripeSubscription(subscriptionDto);

                var invoiceData = await _authService.GetSubscriptionInvoice(subscription.Id);
                // Send Email
                string body = string.Empty;
                _environment.WebRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string contentRootPath = _environment.WebRootPath + "/htmltopdf.html";
                //Generate PDF
                using (StreamReader reader = new StreamReader(contentRootPath))
                {
                    body = reader.ReadToEnd();
                }
                //Fill EMail By Parameter
                body = body.Replace("{title}", "PropertyHappa Payment Invoice");
                body = body.Replace("{currentdate}", DateTime.UtcNow.ToString("MM/dd/yyyy"));

                body = body.Replace("{InvocieStatus}", "Payment Captured");
                body = body.Replace("{InvoiceID}", subscription.Id);

                body = body.Replace("{User_Name}", invoiceData.FullName);
                body = body.Replace("{User_Email}", invoiceData.Email);
                body = body.Replace("{User_Company}", invoiceData.CompanyName);
                body = body.Replace("{User_Phone}", invoiceData.PhoneNumber);


                body = body.Replace("{SubscriptionName}", invoiceData.SubscriptionName);
                string startDate = invoiceData.StartDate.ToString("MMMM dd, yyyy");
                string endDate = invoiceData.EndDate.ToString("MMMM dd, yyyy");

                body = body.Replace("{SubscriptionPeriod}", $"{startDate} - {endDate}");
                body = body.Replace("{SetupFee}", invoiceData.Price.ToString());
                body = body.Replace("{Total}", invoiceData.Price.ToString());

                var bytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(body);

                using (var stream = new MemoryStream(bytes))
                {
                    var bodyemail = "<htmlxmlns=\"http://www.w3.org/1999/xhtml\"xmlns:v=\"urn:schemas-microsoft-com:vml\"xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head><metahttp-equiv=\"Content-Type\"content=\"text/html;charset=UTF-8\"><metaname=\"viewport\"content=\"width=device-width,initial-scale=1.0\"><metaname=\"x-apple-disable-message-reformatting\"><metahttp-equiv=\"X-UA-Compatible\"content=\"IE=edge\"><title></title><styletype=\"text/css\">@mediaonlyscreenand(min-width:620px){.u-row{width:600px!important;}.u-row.u-col{vertical-align:top;}.u-row.u-col-100{width:600px!important;}}@media(max-width:620px){.u-row-container{max-width:100%!important;padding-left:0px!important;padding-right:0px!important;}.u-row.u-col{min-width:320px!important;max-width:100%!important;display:block!important;}.u-row{width:100%!important;}.u-col{width:100%!important;}.u-col>div{margin:0auto;}}body{margin:0;padding:0;}table,tr,td{vertical-align:top;border-collapse:collapse;}p{margin:0;}.ie-containertable,.mso-containertable{table-layout:fixed;}*{line-height:inherit;}a[x-apple-data-detectors='true']{color:inherit!important;text-decoration:none!important;}table,td{color:#000000;}#u_bodya{color:#0000ee;text-decoration:underline;}@media(max-width:480px){#u_content_image_4.v-container-padding-padding{padding:20px10px10px!important;}#u_content_image_4.v-text-align{text-align:center!important;}#u_content_heading_1.v-container-padding-padding{padding:40px10px5px!important;}#u_content_heading_1.v-font-size{font-size:26px!important;}#u_content_text_1.v-container-padding-padding{padding:0px10px10px!important;}#u_content_button_1.v-container-padding-padding{padding:10px10px40px!important;}#u_content_button_1.v-size-width{width:65%!important;}#u_content_social_3.v-container-padding-padding{padding:30px10px10px!important;}#u_content_text_4.v-container-padding-padding{padding:30px10px!important;}}</style></head><bodyclass=\"clean-bodyu_body\"style=\"margin:0;padding:0;-webkit-text-size-adjust:100%;background-color:#e7e7e7;color:#000000;font-family:Silka\"><tableid=\"u_body\"style=\"border-collapse:collapse;table-layout:fixed;border-spacing:0;mso-table-lspace:0pt;mso-table-rspace:0pt;vertical-align:top;min-width:320px;Margin:0auto;background-color:#e7e7e7;width:100%\"cellpadding=\"0\"cellspacing=\"0\"><tbody><trstyle=\"vertical-align:top\"><tdstyle=\"word-break:break-word;border-collapse:collapse!important;vertical-align:top\"><divclass=\"u-row-container\"style=\"padding:0px;background-color:transparent\"><divclass=\"u-row\"style=\"margin:0auto;min-width:320px;max-width:600px;overflow-wrap:break-word;word-wrap:break-word;word-break:break-word;background-color:transparent;\"><divstyle=\"border-collapse:collapse;display:table;width:100%;height:100%;background-color:transparent;\"><divclass=\"u-colu-col-100\"style=\"max-width:320px;min-width:600px;display:table-cell;vertical-align:top;\"><divstyle=\"background-color:#ffffff;height:100%;width:100%!important;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"><divstyle=\"box-sizing:border-box;height:100%;padding:0px;border-top:0pxsolidtransparent;border-left:0pxsolidtransparent;border-right:0pxsolidtransparent;border-bottom:0pxsolidtransparent;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"><tablestyle=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:Silka;\"><tableheight=\"0px\"align=\"center\"border=\"0\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"style=\"border-collapse:collapse;table-layout:fixed;border-spacing:0;mso-table-lspace:0pt;mso-table-rspace:0pt;vertical-align:top;border-top:4pxsolid#6362e7;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%\"><tbody><trstyle=\"vertical-align:top\"><tdstyle=\"word-break:break-word;border-collapse:collapse!important;vertical-align:top;font-size:0px;line-height:0px;mso-line-height-rule:exactly;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%\"><span>&nbsp;</span></td></tr></tbody></table></td></tr></tbody></table><tableid=\"u_content_image_4\"style=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;font-family:Silka;\"><tablewidth=\"100%\"cellpadding=\"0\"cellspacing=\"0\"border=\"0\"><tbody><tr><tdclass=\"v-text-align\"style=\"padding-right:0px;padding-left:0px;\"align=\"center\"><ahref=\"https://PropertyHapa.com/\"target=\"_blank\"><imgalign=\"center\"border=\"0\"src=\"https://propertyhapa.com/Dashboard/assets/images/Logo-01.jpg\"alt=\"image\"title=\"image\"style=\"outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;clear:both;display:inline-block!important;border:none;height:auto;float:left;width:29%;max-width:168.2px;padding:0px23px10px;\"width=\"168.2\"></a></td></tr></tbody></table></td></tr></tbody></table><tablestyle=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:10px;font-family:Silka;\"><tableheight=\"0px\"align=\"center\"border=\"0\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"style=\"border-collapse:collapse;table-layout:fixed;border-spacing:0;mso-table-lspace:0pt;mso-table-rspace:0pt;vertical-align:top;border-top:4pxsolid#6362e7;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%\"><tbody><trstyle=\"vertical-align:top\"><tdstyle=\"word-break:break-word;border-collapse:collapse!important;vertical-align:top;font-size:0px;line-height:0px;mso-line-height-rule:exactly;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%\"><span>&nbsp;</span></td></tr></tbody></table></td></tr></tbody></table><tableid=\"u_content_heading_1\"style=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:10px38px26px28px;font-family:Silka;\"><h1class=\"v-text-alignv-font-size\"style=\"margin:0px;text-align:left;word-wrap:break-word;font-size:23px;font-weight:400;\"><strong>InvoiceVoid</strong></h1></td></tr></tbody></table><tableid=\"u_content_text_1\"style=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:0px30px10px;font-family:Silka;\"><divclass=\"v-text-alignv-font-size\"style=\"font-size:14px;line-height:140%;text-align:left;word-wrap:break-word;\"><pstyle=\"line-height:140%;\"><spandata-metadata=\"<!--(figmeta)eyJmaWxlS2V5IjoiYlVBeVgza1J3QnVxOTVrdG5aZXFtdSIsInBhc3RlSUQiOjE2ODkwMjMzNjUsImRhdGFUeXBlIjoic2NlbmUifQo=(/figmeta)-->\"style=\"line-height:19.6px;\"></span><spandata-buffer=\"<!--(figma)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Jn2AJMv//Jvycdg2uWpjicxttsrBM7FITPiwftQ6vvNaN59475ehjvXnoiui+QTeDV7GKLIySSJPlprqFv2y2jp7woeDlNj6WjP34Yb1Uu2Cw8u51s0FnYvtjjg3dhrv2LWWzXR2Xbvf6fFSb+fZ3Cqujf84KLNdjgunZwcPZmyY4/KNavtX88Fs560zu9wmXY45Dx455to3+1e6l/dccE+/freLbiBqr9z8jFvunYJR8sHR7rluhRLbt20L3sBcnbJy83lX16j0SFKUi7ZuUDiquxR4qESU23JUPkiQsc+fKxwFT669cHpSlKxvavWbq2fhQjzc4Tbbg9MOja7U45MPjviiWvug83J5rEZi17WPur1P/OHgXdc+3KY493IHn7zeXXLPN459x6jG7qyfX3cqPdIVq/qOzXnbt7V2uBpH+9ZCi5WY3zcneOOmTp+63/2xyCm8+IBLCdvf/XESrJV0Ly06485tdrtrX9Hxolux9mMux9WVY8GDz66drm+Mm/KOq1MeG/+hyzXSj+10ddFncw+53GXWzydc3fareadd8sFRGSPRpEtG8MZj45volFk/3xCswcO1LPahnvZfZbELj//B7FfBSyAPeSWC+8nYW+6thU659g11R7rFrR/c8zU6uYMmfeXaXOjr6hvdqZml3fZHFrgWCRibWHAwtTslmnQpELUoxHwiO28LGa7pfvHcSTet/uNu829+D9b/unpytM7B+TraH/36hhLkoP0RzCVBVuQszBlLq/rsy3ueU6LcO/cECr5/7FWQfEHnxV4nAnOmbfJMNNYZG1JorFVgrG0hjirO19XjwYPd985Vc27+zRo+mO3s/G03eD7mdHvqeGDOVAt3UWPltrTekVdO09GGsylR4KEdwRvPnzyuU049cMHVNeocTIrmDNztdl5TOKq7jJtSIsptOZ5+PR48eGz8CH0w8cchwYOe6wbSCKOXFvVzLVohCZuno2+9XD7JXb+lqqteTFV+nFIHTrfRsbPzWroDur/kNB3TC+fIcOxy7/iQX02HpsURGnxdCXrinSua07zX66trW2xzX7lrjGO7Rb6EcOchChwKFn23/3duldgPTocSP+KNoq619bUC0Qkf/urAvqzo7Y+vcarvvwiHHeWA95+oEJ177tKeVJtWlzOwTOqBi7lqJ3aHEm9j9WrrErqUCA6AVya59u2P/+7SAOgCWNW1aStk63yNA+7IK/cGLkDnfu+XHJfc6GoPNzzncjWONlVL4ovnegRvUF6cgjMHa9BiuSj0FexCfb/b/w03o9Ehh3xYCYbkXZs293AQWGvUbf43ce7Sy65NYsTkj9wZG9Kir07D0ld0bBhdPvNnGPtNOtp9SldT4nyNa6L0ZyosWumRo+7ymSWjb1+zj15eOJp66FOXIqUm7HfuiUTvXrUcR7zkHt8517VIYFuxEOL+JpQBEgunb0Ds2ebqap88k4ulfwZHc3SEgkYoUSHpIfeD2XiDUW3rax+rA19oN+8/OvDaFsVgGSFBE4e4b1Gp7rn3CWXQfmPjGHUAhiKO9tld25VYv+Ub13v6T9ee1+w8lHFS/V2DwMcppaJ8I/1YHR3t8zVSlSj65V3R4ZPtqL1oY6vogYt/ufc9cb9Kzx4WfRAp4LC7qVWH6Ct3bXVtKoW+x/F8jRkueA60hMas9v+YOYEEEEaR1xFYGHBpjPb2bS8qAYcA++tS7ZK33Ar2ezgUE+OJyuvtax7JxZbTnHxCxUTfyCfoRRrwPk457Y5u3MX9achl1+7njFbx0Mw5IndtUOLogv1w6UjURjes0YN/1Dl4lFOKIN195l61t0T08p73XfvuVWWQ92a7HDV1kGBCRIIBUMW1i1tFNSEe32lFNRrCWTU8wsThHfVS7dYNPtFzJg4uEYZg5jB44RolFm3cCm9HPKi17CAOsdShATMMIKufd799vqy7oO8leNCdrg1Xii65J50uFXBEglmFFqBppue6cy7NGxEMakUiormSm3LvbEJ2Qqqa8OFKzV00XY42PZAEcz5zkM2YRk9mFGdSBKd3ugzYTEpcRwN27xNr1bh5MlUWnNK1275fGXOucb+/vq5GT8i9KVXkMLOVvGWZAw4HQoDjHaYkVEuOXSHpFZVDQjAyHLclqLVUXvZPQ6JKcDGMqTajLsSc2uOlH1MpNouLwPAdG3EQ8XC2M2hS+WBl5hNYhsNozxHczVLiz0XvBmF0deWPXfiywxFadfVB+yPXuRHJowJdm7lkxOS7IKGlsIz76NgLaZfu/WOnqHvad64YCXX1pPW6PJz9aNdGMH0PTlMBIQBagm85YJVVBux7oossOsWBNbvXJQ93kAFce+SVDzsUJEfwEzyYmrlLDYMuDgtf7Hx//RmtlOjiNmMF0wJT0urKBaI4fntopyDLVB1hTzOVWLl5lb5pM/492vUPtQdqGgXDDheX2i6k/IGqHLUU8jSiOcPesOiv0PK/dLTpWSSQyqDHs65NDeNgLjUO8QclD2owt09pG0ff69pILw5tkCPDtz6gZyEYYI1lrg1XgejegiyvQhZfGJQ967e85pbJSnPJPsz3EUj7VQTBoYitr7j2oElT3dRD46HGxcj1owLWh0/O0lTDMhgxay8Wewqx4HCgMdYKm1o1cDkiLwQPqAKGSXDLbBp4J3fhKOtxw373qgH6h017JoHDObjlc2yaL0pnLYBo1xYJDUiUQT7xbv/ugVRo1fQujvanv01SgjlHrZTGyAWYaxksUVts0RD47fN7YKWFXbvd+1+TAZdhio2FRUK3KfqlHc0nHm64wrFJcFnmO44WCQhcYLjJUZx2LSsljkiathK777V0tBkO+5TOWsNogLmpqBK/g+QvpXJk+WaRoBbtB8Z+Ca33cK7ouBrBB/H76ILp5HFdQmryGFP6v2c6xol8AkgAYURCIbXknqpRfdT8mzuDl/KJfIiEvIfqmb6uMJWaTyh7JChLujLim4MC8nZsnIG6s52OdkajzkqMm5KJKf0cm7rHvw49lrHBYspF9EpFkB1BsTTCTSjHNcybQDaspVFxVDsmwUAHf07VEhc2rOGIXm+RIItyDowHr1XL1TqzzsHquYhnOlIlDD+5yEU6qu2RYDmB8jpX8+TRBRVzYYgu3srVugtnzmG0RvbIUU9Fx5Ozb8ezgYipShU6MkSghgIPjdPYA+M4CSngCcsiJfAYBczVuUg+zyA/3pFLAz+7695clDhNXLRmuZQtR5vZlwSSEBz/yVz1bxI8H9JUrkWCB5UfkPtp+Phjnf3VvL5KsN9TAVO9iL6Y8w0LQ2bCmUyhfzcVeN3hATnSk/QBMxjfqMbuFfaHubtdpmNtCFDVIyVd1MQJDqMQdjcNhzw8RxRIS5Xo52zRFIuW+Cjiyim33m2/IPntci0S1JrFRJ9PqJ+RgGARk6/Q0SIBaQs9Hv1z07V8g6O2JiRGNxaOqTb7ZxTDqRwZCiwSV+3d4YDHE/qEwUwN6fvr3wVRbV1CeBLBQfUVdAtvKMHwTg5slpsMJMwbCK+08UMuVcC8T8vCamdoPHpAGq8eEFbh2kPK/oRYvxb6PAqZIMWziGXOZ52JYORq+0LB1rttOWI2ygTma0YKt8gUHfHGcCVogDyIPb+vgxY0iF04nqsWw8yKwh+d02LXpsbZ5HFUvZJg1wpBKoeItSVRyJRCZrqRMnO0oEDwRyfRXf0UDhBXOSQEIx8heDOboFpPtVl7Q2GpFWvPZDJlBztFrY5JBAs20t4Y0mnEKgHjWnRDPfDGhrWMFRz1HCQ++rUGGRObRRkeNELeZXhJtSEQxO9LqWh8HPIApjbiVmQsusiNCAR5jkViztU/uxa5yifU5QjlE1Mzb40izNZAbRUSCsV/eOlvQqcTyicgbprkzSoZsks7QE94vwZp/KujzTxJ4qq941Cm+g7S66swiOno4GdCS/McKyE3mY44BbkFjFXfP9hlLx1Gh1ddi9WnPkHRFTDGplKJb59/GcLJdvUJpCX6DkbRWamH2gTrUM26sj7hXngliFYY2X09GqzDJ7oy3yEhl8EcuAwmkFBBQJuBZEnwPYXyiY9+LRRF37HNuWNU1ShN6qZOtaNa20Jg6AmH68j6UonRjefgnqhR1J5z9RLcnN2hl2TnLtWLWnzwbv8aUVQrr8Ombo5Setu31Yzab56ZiP6tVpQmxREe2FkJZm0E+CjKuJoowSqij6up92oWCWURGeJvQo9BKJ/ASYOjYhTh/4J/oZ2tAQe5/0gAk2Dhhuh4BBXfh+rAMElW4Ru0WKN6OaoXkGDxqtUbK1dOo9RZQcGbG0NCnyI81NKOAhG2Ilqh5TDM/9jRapgklE+wytS81XTMKfePYU8jzV5yVc6MvohUOmqrRIIM82xafXzyzF+IOL+oHGwGaVZ11yVHoiyS4Nwsdlbq69qekgAj/1cCokwKDv8c5eU+F63CJpXH78Pe1NHmPQ4JVgHMknonxehIs9UpCCIw3U+RdOv8rwSkSmb1nk+ogNjyI4xAGz2CbITAovLY1Oo9HZFYgpafPZn278xV7N9Zl7P7xbXEXwylLu822FaqgFAHgZ1/F9AvaLKoAvjKWoSvnkrA7plKUm26B46DuHCQjYqDs00H48c0IzOcQ4LVHHYcHOml+oA+A/61KkQdPg05excsan9wa82MzCto9keakfGqHui+JyboaFddGtgCeVcjYaHLexB2wlzLIqFyY3bNJzRsk0BFl8NSBpE0xy6TJUj2JXJpO4iNuSgrDmL5e3KZKHFRk2sz9bGCoag5ovRuqQQ1h64v12bIwVVGLgoqB6k0x4LVoM6YlIOq0HJxHZfDyhL5INemZUOJuhhHFNwLlTj1wPtoaqrm2p3XfIK+46pctlyYnguZ0eH251DvEFeOTVNH25nDkYfSBI3Uybr+CG6W73foJWAqVXsWaGRtQokSoclq8qERk6AaEGAdXKBtdsEWqt/dLrhxUCMeZheIJPqdy+IVNdlp7eS4H/Oubkzx2GwHUeFD0UcQLTazJdsHJ9qjlQElikjxb5VBky4LtBDIrwzKZHlKQIrahWnU5VyIJbB+9jm8uty34zeHHSEi5yG99+fI+3J9wPmwC4cyV3NidcD8x9ZMHRINLA7VHYdCduMdJeWQEIzMgtGHZeQ6dTgS9EDIuhF67r24WRiUirDuQE0O6tl+vA/Wpg8yd1jn48EZhyPcQxtY9OsuBNNcO0HcH8RwSTsYDWyewythm6UOe34W8DwPmvA2iCGz8Iano7baJMC65labcqDoGH3pGhYJVnt6LUvfeLTrWg2EqBdfV++nhDhCSp2VgBHB9tDIU7D0Itw2OYx2FmsmBmJEj5rwhoLwlbiOiHDvK8HXqH7E7LNg5EsUk2dhYgtci4QKggRE5TD6cLRIIKqIXkNCEWshJh21qCbBqAVx84vY5zSoVNoQJWvRKpEFghtyvIJAs4CrpsJhRvHBuoTOJIL2AjVOTQeJ/EUlGGjZtsGk1uCC6nV8c9hIKwzaaBoLkiYKxcKB9OCpKtdayyoFcmXeQ++Xg6QyHFHiLfAKY+MtA5M41KkjLDhNCQYGmARvfTYjurWF5jejEUkJHtxQtxD9GQdAoYrs4YApfQjndhAQK/BYaBnqqitjSnOXZ0gcileq2DFIFSS4Ju+l/yaABBDG4EqVN5yUDTQdCItOgjKQ3XsNOGp7XD0V1Rii16WYFaQhjAGRn6fzCUZa7ZjZFdCbmFeQfsaqeTDwcFQBk+CNMW9Rta+BA2u2YbhgvY/gvpNuqmlMb1Db4hKOowqaBGQB/koAqBzc09MR7xgViWLHwNToZmxIYNWpNq+SeM7EwSXCRpD+jZ51lRIQF1Q+ytGLe0Qx7YBQ5ji44f4VkbAMDO9PdIcNgxtUNiwclSMSEIs78cc/4UFvBxfrcGfOV9PhpzyXVzK8IUfh7dqsDBneEh2RujQJRCHY1A8uLpv76/0aqys6Ag5xF/jYrfek9s2bgotTujXlD46D4oE60MtWdOQw7hvIZRA84Txqfuo3vLBlwU/+aWBaOlAOCcHQsChMTZgkEOKjFoIeY31A5EMkpKVEuChMRCyk/qCaRiGhKEMI3cVhoR1MZDmiBAMYJomVmC4X8W0Hl+Xwyzg//f39LZCnRUqeqfdcDOCaGXi7gLSKD2J3o8FAkbAP1xi0MuqEo174keBNPlzCxRu/QVM/MojrJx6LIYcXuYzocL298OkNUM5RprmlqhhGYo7aEZGgCzIZqqMztOLSxeXXAL1nfbn8BjLlcDrVj0Q0E8nsYHAoEoy7DPWQJz2lOPRRUjXGg+nVCcI7pJdBm2CqCm4DGWc4asokwctAlEUOSrlcHChL8z004NiMktQAj42g7NhYmd6mtx3MhTh2Er5o/cI4GtGvhbzPg64czeHwHYdWC8thfRd8LeQu0NHfXwvZEHK0mcRIMEZhKvvmq8D7/+bLrxxFm0aFMD5bJLCT0HqC4ENCIdRiwTsk9HIbF06IzOfURcm8TXXwoBQt1YOS9Qd8Fdqn5UyFpC+CzEuFMaVwtEhAHFTHWCVgBQjIWB1Wg6ZcojwSL2dwL1IXgiqMz8NN3IdKFI9C2O0QoktFyVDJW0pHLXbM+YTyyvOQkJ0Iwmd3jcWxZ7ta85FAgoOWprg2WeEcXuIfuDgsSKoMOLyIHxqt79q0cgRKtXZcSTn6gJImr1QBrFZLpxyUQIzsOSh9djDH5TCm4s1c1HNzXHz2yKXYOcKeMpRgFwfjyNX4guuKXPROcNljOXpDgbCWU3D4ZN5U5CCjtkaVUy8HlsvKfh26pXkOtktVA8ecdVAstNE7B4d6AEaxIoelA0ekNn4WXpEDI4fDoZCF+bkIEXhjI8+QYzOn4P4Fn6P3Mkal2swacDaYzmEGPEcf0MmoVS2uGB/ZK7A3446opdbCSbtTBrAO5G22NKyjWCdwhE88qQQuLiGgnkGR6BbpwDiNS4LavHVprQQvRxzKHxeV+BDQ0tVqj4qhIUGx2qE6oWJ5keuyXXAQcKlcxNoNrLYOOUjziA9JgXWw6sknEKyjFjlR4uvqyS4vGXDOFPSN4zXCsQ2Hd/bmHiiox+qovyjijxP0gwcJHpo/bFDGabHwCfVuyDq4NKTSaJGotFJxj/8+hb6u8OIZOtrQhBL0Tn0Df+gUWoOugUAAR/4Rde+UQDwUC7flqHyQ2LfjPbKsfYROoWVyDf28xEXpPdzlzTOlo7rtX3vKo2GdoaMyRoKc6htknVN4Fl2Dh+OiPK3uwuNzW47kQyKwEXRv4IgE4yW/5LLDQUV7ShMzfxTBH0ioBbEy5S9x+GEEjv4uLKiQcsQRfb+vBNOVvkFT4BTeV+garNq4KF1Ud2GY57YclQ8SNEe+gfYymMKMzTUQeFiKtqR7oO7upb8a0W3h0Q754KiMkSCn+gZZ5xSeRdfg4bgoT6u78PjcliP5YNJm/Gh/ZEGOBXeDiTZtpE6cT6AucvQjST6hELOIzkpMl7cRuXn5pTl9bYvWAUGVIXALmuCkgKAiWRbjpn9KEN3JEcbgHk9nYUFEDxDZeaNhdOMdiz+sQuJ3MX083A0Q048SXJB9pMVj6hOUz/jWDeL4zm+DWdl5RfRru5XgUJ7lZ/Wjwb78JQZvtbSAYtelBJCg2sUYEPnVbj6xfGYf14JLB/uyiFUCL0M12Fe3wj/6+Q2jqJiQk9FV4FrwyVJfBQQvIFhwWj8NqRLVJ1/NqxMQzCnsm627V90WpWCghEG4wqrPa9PsgOh9olGwDVqCgOAPtHixhC4rSNd6R4iRmmiaqrMQl9C8gmCNoivjoxJCCvZKcCjyfwA=(/figma)-->\"style=\"line-height:19.6px;\"></span>Hello\"+invoiceData.CompanyName+\",<br><br>We'reheretoinformyouthatyourInvoiceisvoidandnomorevalid!<br><br><b>SubscriptionDetails:</b>\"+invoiceData.SubscriptionName+\"<br><br>Weappreciateyourbusinessandthetrustyou'veplacedinus.Ifyouhaveanyquestionsaboutyoursubscriptionorneedassistancewithanything,feelfreetoreachout.We'reheretosupportyouinachievingyourgoals.</p></div></td></tr></tbody></table><tableid=\"u_content_text_4\"style=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:0px27px10px;font-family:Silka;\"><divclass=\"v-text-alignv-font-size\"style=\"font-size:14px;color:#000000;line-height:140%;text-align:left;word-wrap:break-word;\"><pstyle=\"font-size:14px;line-height:140%;\">ThankyouforyourcontinuedtrustinPropertyHapa!</p></div></td></tr></tbody></table></div></div></div></div></div></div><divclass=\"u-row-container\"style=\"padding:0px;background-color:transparent\"><divclass=\"u-row\"style=\"margin:0auto;min-width:320px;max-width:600px;overflow-wrap:break-word;word-wrap:break-word;word-break:break-word;background-color:transparent;\"><divstyle=\"border-collapse:collapse;display:table;width:100%;height:100%;background-color:#6362e7;\"><divclass=\"u-colu-col-100\"style=\"max-width:320px;min-width:600px;display:table-cell;vertical-align:top;\"><divstyle=\"height:100%;width:100%!important;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"><divstyle=\"box-sizing:border-box;height:100%;padding:0px;border-top:0pxsolidtransparent;border-left:0pxsolidtransparent;border-right:0pxsolidtransparent;border-bottom:0pxsolidtransparent;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"><tablestyle=\"font-family:Silka;\"role=\"presentation\"cellpadding=\"0\"cellspacing=\"0\"width=\"100%\"border=\"0\"><tbody><tr><tdclass=\"v-container-padding-padding\"style=\"overflow-wrap:break-word;word-break:break-word;padding:10px9px10px30px;font-family:Silka;\"><divclass=\"v-text-alignv-font-size\"style=\"font-size:14px;color:#ffffff;line-height:140%;text-align:left;word-wrap:break-word;\"><pstyle=\"font-size:14px;line-height:140%;\">Foranyinquiries,calluson+1234567890or+9123456780.</p><br><pstyle=\"font-size:14px;line-height:140%;\">Warmregards,<br>PropertyHapaTeam💚🏋️‍♀️</p><br></div></td></tr></tbody></table></div></div></div></div></div></div><divclass=\"u-row-container\"style=\"padding:0px;background-color:transparent\"><divclass=\"u-row\"style=\"margin:0auto;min-width:320px;max-width:600px;overflow-wrap:break-word;word-wrap:break-word;word-break:break-word;background-color:transparent;\"><divstyle=\"border-collapse:collapse;display:table;width:100%;height:100%;background-color:transparent;\"><divclass=\"u-colu-col-100\"style=\"max-width:320px;min-width:600px;display:table-cell;vertical-align:top;\"><divstyle=\"background-color:#ffffff;height:100%;width:100%!important;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"><divstyle=\"box-sizing:border-box;height:100%;padding:0px;border-top:0pxsolidtransparent;border-left:0pxsolidtransparent;border-right:0pxsolidtransparent;border-bottom:0pxsolidtransparent;border-radius:0px;-webkit-border-radius:0px;-moz-border-radius:0px;\"></div></div></div></div></div></div></td></tr></tbody></table></body></html>";
                    var emailSubject = "PropertyHapa – Payment Receipt - " + subscription.Id;
                    var recipients = new List<string> { invoiceData.Email };

                    _emailSender.SendEmailWithFIle(bytes, invoiceData.Email, emailSubject, bodyemail, "invoice");
                }

                return Ok(subscription);
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine($"Error creating subscription: {exp.Message}");
                return StatusCode(500, new { error = "An error occurred while creating the subscription." });
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
