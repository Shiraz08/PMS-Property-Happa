using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PMS_PropertyHapa.Admin.Models.ViewModels;
using PMS_PropertyHapa.Admin.Services;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Models.Roles;
using PMS_PropertyHapa.Shared.Email;
using System.Diagnostics;
using System.Globalization;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApiDbContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;
        private IWebHostEnvironment _environment;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        EmailSender _emailSender = new EmailSender();

        private readonly GoogleCloudStorageService _googleCloudStorageService;
        private readonly GoogleCloudStorageOptions _googleCloudStorageOptions;

        public HomeController(IWebHostEnvironment Environment, ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager, ApiDbContext context, IUserStore<ApplicationUser> userStore,
                              GoogleCloudStorageService googleCloudStorageService, IOptions<GoogleCloudStorageOptions> googleCloudStorageOptions)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
            _environment = Environment;
            _googleCloudStorageService = googleCloudStorageService;
            _googleCloudStorageOptions = googleCloudStorageOptions.Value;
        }

        public async Task<IActionResult> Index()
        {

            var totalEarnings = await _context.PaymentInformations.SumAsync(s => s.AmountCharged);

            ViewBag.totalEarnings = totalEarnings;

            var taskRequests = await (from t in _context.TaskRequest
                                               from a in _context.Assets.Where(x => x.AssetId == t.AssetId && x.IsDeleted != true).DefaultIfEmpty()
                                               from u in _context.AssetsUnits.Where(x => x.UnitId == t.UnitId && x.IsDeleted != true).DefaultIfEmpty()
                                               from o in _context.Owner.Where(x => x.OwnerId == t.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                               from tnt in _context.Tenant.Where(x => x.TenantId == t.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                               where t.IsDeleted != true
                                               select new TaskRequestDto
                                               {
                                                   TaskRequestId = t.TaskRequestId,
                                                   Type = t.Type,
                                                   Subject = t.Subject,
                                                   Description = t.Description,
                                                   IsOneTimeTask = t.IsOneTimeTask,
                                                   IsRecurringTask = t.IsRecurringTask,
                                                   StartDate = t.StartDate,
                                                   EndDate = t.EndDate,
                                                   Frequency = t.Frequency,
                                                   DueDays = t.DueDays,
                                                   IsTaskRepeat = t.IsTaskRepeat,
                                                   DueDate = t.DueDate,
                                                   Status = t.Status,
                                                   Priority = t.Priority,
                                                   Assignees = t.Assignees,
                                                   IsNotifyAssignee = t.IsNotifyAssignee,
                                                   AssetId = t.AssetId,
                                                   Asset = a.BuildingNo + "-" + a.BuildingName,
                                                   UnitId = t.UnitId,
                                                   Unit = u.UnitName,
                                                   TaskRequestFileName = t.TaskRequestFile,
                                                   TaskRequestFile = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Task_TaskRequestFile_" + t.TaskRequestId + Path.GetExtension(t.TaskRequestFile)}",
                                                   OwnerId = t.OwnerId,
                                                   Owner = o.FirstName + " " + o.LastName,
                                                   IsNotifyOwner = t.IsNotifyOwner,
                                                   TenantId = t.TenantId,
                                                   Tenant = tnt.FirstName + " " + tnt.LastName,
                                                   IsNotifyTenant = t.IsNotifyTenant,
                                                   HasPermissionToEnter = t.HasPermissionToEnter,
                                                   EntryNotes = t.EntryNotes,
                                                   VendorId = t.VendorId,
                                                   ApprovedByOwner = t.ApprovedByOwner,
                                                   PartsAndLabor = t.PartsAndLabor,
                                                   AddedBy = t.AddedBy,
                                                   AddedDate = t.AddedDate,
                                                   ModifiedDate = t.ModifiedDate,
                                                   LineItems = (from item in _context.LineItem
                                                                from ca in _context.ChartAccount.Where(x => x.ChartAccountId == item.ChartAccountId && x.IsDeleted != true).DefaultIfEmpty()
                                                                where item.TaskRequestId == t.TaskRequestId && item.IsDeleted != true
                                                                select new LineItemDto
                                                                {
                                                                    LineItemId = item.LineItemId,
                                                                    TaskRequestId = item.TaskRequestId,
                                                                    Quantity = item.Quantity,
                                                                    Price = item.Price,
                                                                    ChartAccountId = item.ChartAccountId,
                                                                    AccountName = ca.Name,
                                                                    Memo = item.Memo
                                                                }).ToList()
                                               })
                     .AsNoTracking()
                     .ToListAsync();


            List<PieChart> dataPoints = new List<PieChart>
            {
                new PieChart(TaskStatusTypes.Completed.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.Completed.ToString())),
                new PieChart(TaskStatusTypes.InProgress.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.InProgress.ToString())),
                new PieChart(TaskStatusTypes.OnHold.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.OnHold.ToString())),
                new PieChart(TaskStatusTypes.NotStarted.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.NotStarted.ToString()))
            };

            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

           
            ViewBag.totalTaskRequests = taskRequests.Count();
            taskRequests = taskRequests.Where(s => (s.AddedDate.HasValue && s.AddedDate.Value.ToString("yyyy-MM-dd") == currentDate)
                                        || (s.ModifiedDate.HasValue && s.ModifiedDate.Value.ToString("yyyy-MM-dd") == currentDate)).ToList();


            var landlords = await (from owner in _context.Owner
                                   join organization in _context.OwnerOrganization
                                   on owner.OwnerId equals organization.OwnerId into orgGroup
                                   from org in orgGroup.DefaultIfEmpty()
                                   where owner.IsDeleted != true
                                   select new OwnerDto
                                   {
                                       OwnerId = owner.OwnerId,
                                       FirstName = owner.FirstName,
                                       MiddleName = owner.MiddleName,
                                       LastName = owner.LastName,
                                       Fax = owner.Fax,
                                       TaxId = owner.TaxId,
                                       EmailAddress = owner.EmailAddress,
                                       EmailAddress2 = owner.EmailAddress2,
                                       PictureName = owner.Picture,
                                       Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + owner.OwnerId + Path.GetExtension(owner.Picture)}",
                                       PhoneNumber = owner.PhoneNumber,
                                       PhoneNumber2 = owner.PhoneNumber2,
                                       EmergencyContactInfo = owner.EmergencyContactInfo,
                                       LeaseAgreementId = owner.LeaseAgreementId,
                                       OwnerNationality = owner.OwnerNationality,
                                       Gender = owner.Gender,
                                       DocumentName = owner.Document,
                                       Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + owner.OwnerId + Path.GetExtension(owner.Document)}",
                                       DOB = owner.DOB,
                                       VAT = owner.VAT,
                                       LegalName = owner.LegalName,
                                       Account_Name = owner.Account_Name,
                                       Account_Holder = owner.Account_Holder,
                                       Account_IBAN = owner.Account_IBAN,
                                       Account_Swift = owner.Account_Swift,
                                       Account_Bank = owner.Account_Bank,
                                       Account_Currency = owner.Account_Currency,
                                       OrganizationName = org.OrganizationName,
                                       OrganizationDescription = org.OrganizationDescription,
                                       OrganizationIcon = org.OrganizationIcon,
                                       OrganizationLogo = org.OrganizationLogo,
                                       Website = org.Website,
                                       AddedBy = owner.AddedBy,
                                       AddedDate = owner.AddedDate,
                                       ModifiedDate = owner.ModifiedDate

                                   })
                                       .AsNoTracking()
                                       .ToListAsync();

                ViewBag.totalLandlords = landlords.Count();


            var tenants = await _context.Tenant.Where(x => x.IsDeleted != true)
                                   .AsNoTracking()
                                   .ToListAsync();

            var tenantDtos = tenants.Select(tenant => new TenantModelDto
            {
                TenantId = tenant.TenantId,
                FirstName = tenant.FirstName,
                LastName = tenant.LastName,
                EmailAddress = tenant.EmailAddress,
                PhoneNumber = tenant.PhoneNumber,
                EmergencyContactInfo = tenant.EmergencyContactInfo,
                LeaseAgreementId = tenant.LeaseAgreementId,
                TenantNationality = tenant.TenantNationality,
                Gender = tenant.Gender,
                DOB = tenant.DOB,
                VAT = tenant.VAT,
                LegalName = tenant.LegalName,
                Account_Name = tenant.Account_Name,
                Account_Holder = tenant.Account_Holder,
                Account_IBAN = tenant.Account_IBAN,
                Account_Swift = tenant.Account_Swift,
                Account_Bank = tenant.Account_Bank,
                Account_Currency = tenant.Account_Currency,
                Address = tenant.Address,
                Address2 = tenant.Address2,
                Locality = tenant.Locality,
                Region = tenant.Region,
                PostalCode = tenant.PostalCode,
                Country = tenant.Country,
                CountryCode = tenant.CountryCode,
                PictureName = tenant.Picture,
                Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Picture_" + tenant.TenantId + Path.GetExtension(tenant.Picture)}",
                DocumentName = tenant.Document,
                Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Tenant_Document_" + tenant.TenantId + Path.GetExtension(tenant.Document)}",
                AddedBy = tenant.AddedBy,
                AddedDate = tenant.AddedDate,
                ModifiedDate = tenant.ModifiedDate,
                AppTid = tenant.AppTenantId.ToString()
            }).ToList();

            
                ViewBag.totalTenants = tenants.Count();
            


            var assets =await ( from asset in _context.Assets
                                from owner in _context.Owner.Where(x => x.OwnerId == asset.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                where asset.IsDeleted != true
                                select new AssetDTO
                                {
                                    AssetId = asset.AssetId,
                                    SelectedPropertyType = asset.SelectedPropertyType,
                                    SelectedBankAccountOption = asset.SelectedBankAccountOption,
                                    SelectedReserveFundsOption = asset.SelectedReserveFundsOption,
                                    SelectedSubtype = asset.SelectedSubtype,
                                    SelectedOwnershipOption = asset.SelectedOwnershipOption,
                                    BuildingNo = asset.BuildingNo,
                                    BuildingName = asset.BuildingName,
                                    Street1 = asset.Street1,
                                    Street2 = asset.Street2,
                                    City = asset.City,
                                    Country = asset.Country,
                                    Zipcode = asset.Zipcode,
                                    State = asset.State,
                                    AppTid = asset.AppTenantId,
                                    PictureFileName = asset.Image,
                                    Image = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Assets_Image_" + asset.AssetId + Path.GetExtension(asset.Image)}",
                                    AddedBy = asset.AddedBy,
                                    AddedDate = asset.AddedDate,
                                    ModifiedDate = asset.ModifiedDate,
                                    Units = asset.Units.Where(x => x.IsDeleted != true).Select(u => new UnitDTO
                                    {
                                        UnitId = u.UnitId,
                                        AssetId = u.AssetId,
                                        UnitName = u.UnitName,
                                        Bath = u.Bath,
                                        Beds = u.Beds,
                                        Rent = u.Rent,
                                        Size = u.Size,
                                    }).ToList(),
                                    OwnerData = new OwnerDto
                                    {
                                        OwnerId = owner.OwnerId,
                                        FirstName = owner.FirstName,
                                        MiddleName = owner.MiddleName,
                                        LastName = owner.LastName,
                                        Fax = owner.Fax,
                                        TaxId = owner.TaxId,
                                        EmailAddress = owner.EmailAddress,
                                        EmailAddress2 = owner.EmailAddress2,
                                        PictureName = owner.Picture,
                                        Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Picture_" + owner.OwnerId + Path.GetExtension(owner.Picture)}",
                                        PhoneNumber = owner.PhoneNumber,
                                        PhoneNumber2 = owner.PhoneNumber2,
                                        EmergencyContactInfo = owner.EmergencyContactInfo,
                                        LeaseAgreementId = owner.LeaseAgreementId,
                                        OwnerNationality = owner.OwnerNationality,
                                        Gender = owner.Gender,
                                        DocumentName = owner.Document,
                                        Document = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Owner_Document_" + owner.OwnerId + Path.GetExtension(owner.Document)}",
                                        DOB = owner.DOB,
                                        VAT = owner.VAT,
                                        LegalName = owner.LegalName,
                                        Account_Name = owner.Account_Name,
                                        Account_Holder = owner.Account_Holder,
                                        Account_IBAN = owner.Account_IBAN,
                                        Account_Swift = owner.Account_Swift,
                                        Account_Bank = owner.Account_Bank,
                                        Account_Currency = owner.Account_Currency,
                                        AddedBy = owner.AddedBy,
                                        AddedDate = owner.AddedDate,
                                        ModifiedDate = owner.ModifiedDate
                                    }
                                }).AsNoTracking()
                                        .ToListAsync();

            ViewBag.totalAssets = assets.Count();
            

            var invoiceHistory = await (from inv in _context.Invoices
                                        from t in _context.Tenant.Where(x => x.TenantId == inv.TenantId && x.IsDeleted != true).DefaultIfEmpty()
                                        from o in _context.Owner.Where(x => x.OwnerId == inv.OwnerId && x.IsDeleted != true).DefaultIfEmpty()
                                        where inv.IsDeleted != true // Remove leaseId filter
                                        select new InvoiceDto
                                        {
                                            InvoiceId = inv.InvoiceId,
                                            OwnerId = inv.OwnerId,
                                            OwnerName = o.FirstName + " " + o.LastName,
                                            TenantId = inv.TenantId,
                                            TenantName = t.FirstName + " " + t.LastName,
                                            InvoiceCreatedDate = inv.InvoiceCreatedDate,
                                            InvoicePaid = inv.InvoicePaid,
                                            RentAmount = inv.RentAmount,
                                            LeaseId = inv.LeaseId,
                                            InvoiceDate = inv.InvoiceDate,
                                            InvoicePaidToOwner = inv.InvoicePaidToOwner,
                                            AddedBy = inv.AddedBy,
                                        }).ToListAsync();

            invoiceHistory = invoiceHistory.ToList();

            ViewBag.totalInvoices = invoiceHistory.Count();

            var appplications = await (from a in _context.Applications
                                       from p in _context.Assets.Where(x => x.AssetId == a.PropertyId && x.IsDeleted != true).DefaultIfEmpty()
                                       where a.IsDeleted != true
                                       select new ApplicationsDto
                                       {
                                           ApplicationId = a.ApplicationId,
                                           FirstName = a.FirstName,
                                           MiddleName = a.MiddleName,
                                           LastName = a.LastName,
                                           SSN = a.SSN,
                                           ITIN = a.ITIN,
                                           DOB = a.DOB,
                                           Email = a.Email,
                                           PhoneNumber = a.PhoneNumber,
                                           Gender = a.Gender,
                                           MaritalStatus = a.MaritalStatus,
                                           DriverLicenseState = a.DriverLicenseState,
                                           DriverLicenseNumber = a.DriverLicenseNumber,
                                           Note = a.Note,
                                           Address = a.Address,
                                           LandlordName = a.LandlordName,
                                           ContactEmail = a.ContactEmail,
                                           ContactPhoneNumber = a.ContactPhoneNumber,
                                           MoveInDate = a.MoveInDate,
                                           MonthlyPayment = a.MonthlyPayment,
                                           JobType = a.JobType,
                                           JobTitle = a.JobTitle,
                                           AnnualIncome = a.AnnualIncome,
                                           CompanyName = a.CompanyName,
                                           WorkStatus = a.WorkStatus,
                                           StartDate = a.StartDate,
                                           EndDate = a.EndDate,
                                           SupervisorName = a.SupervisorName,
                                           SupervisorEmail = a.SupervisorEmail,
                                           SupervisorPhoneNumber = a.SupervisorPhoneNumber,
                                           EmergencyFirstName = a.EmergencyFirstName,
                                           EmergencyLastName = a.EmergencyLastName,
                                           EmergencyEmail = a.EmergencyEmail,
                                           EmergencyPhoneNumber = a.EmergencyPhoneNumber,
                                           EmergencyAddress = a.EmergencyAddress,
                                           SourceOfIncome = a.SourceOfIncome,
                                           SourceAmount = a.SourceAmount,
                                           Assets = a.Assets,
                                           AssetAmount = a.AssetAmount,
                                           PropertyId = a.PropertyId,
                                           UnitIds = a.UnitIds,
                                           IsSmoker = a.IsSmoker,
                                           IsBankruptcy = a.IsBankruptcy,
                                           IsEvicted = a.IsEvicted,
                                           HasPayRentIssue = a.HasPayRentIssue,
                                           IsCriminal = a.IsCriminal,
                                           StubPicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_StubPicture_" + a.ApplicationId + Path.GetExtension(a.StubPicture)}",
                                           StubPictureName = a.StubPicture,
                                           LicensePicture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"Applications_LicensePicture_" + a.ApplicationId + Path.GetExtension(a.LicensePicture)}",
                                           LicensePictureName = a.LicensePicture,
                                           IsAgree = a.IsAgree,
                                           Pets = _context.ApplicationPets
                                              .Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true)
                                              .Select(x => new ApplicationPetsDto
                                              {
                                                  // Map properties from ApplicationPets to ApplicationPetsDto
                                                  PetId = x.PetId,
                                                  ApplicationId = x.ApplicationId,
                                                  Type = x.Type,
                                                  Name = x.Name,
                                                  Breed = x.Breed,
                                                  Quantity = x.Quantity,
                                                  Picture = $"https://storage.googleapis.com/{_googleCloudStorageOptions.BucketName}/{"ApplicationPets_Picture_" + x.PetId + Path.GetExtension(x.Picture)}",
                                                  PictureName = x.Picture,
                                              }).ToList(),
                                           Vehicles = _context.ApplicationVehicles.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                           Dependent = _context.ApplicationDependent.Where(x => x.ApplicationId == a.ApplicationId && x.IsDeleted != true).ToList(),
                                           AddedBy = p.AddedBy,
                                           AddedDate = a.AddedDate
                                       })
                     .AsNoTracking()
                     .ToListAsync();


            appplications = appplications.ToList();
            ViewBag.totalApplications = appplications.Count();
            List<PieChart> invoiceStatus = new List<PieChart>
            {
                new PieChart("Paid",  invoiceHistory.Count(i => i.InvoicePaid == true)),
                new PieChart("UnPaid", invoiceHistory.Count(i => i.InvoicePaid != true)),
                new PieChart("Paid To Owner", invoiceHistory.Count(i => i.InvoicePaidToOwner == true)),
                new PieChart("UnPaid To Owner", invoiceHistory.Count(i => i.InvoicePaidToOwner != true))
            };


            var taskCountsByMonth = taskRequests
                    .GroupBy(t => new { t.AddedDate?.Year, t.AddedDate?.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Year)
                    .ThenBy(g => g.Month)
                    .ToList();

            List<string> monthNames = new List<string>();
            for (int month = 1; month <= 12; month++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                monthNames.Add(monthName);
            }

            List<DataPoint> dataPoints1 = new List<DataPoint>();
            int i = 1;
            foreach (var monthName in monthNames)
            {
                dataPoints1.Add(new DataPoint(monthName, taskCountsByMonth.Where(tc => tc.Month == i).Select(tc => tc.Count).FirstOrDefault()));
                i++;
            }


            ViewBag.DataPoints1 = JsonConvert.SerializeObject(dataPoints1);

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            ViewBag.invoiceStatus = JsonConvert.SerializeObject(invoiceStatus);
            ViewBag.taskRequest = taskRequests.ToList();
            ViewBag.landlords = landlords.ToList();
            ViewBag.tenants = tenants.ToList();
            ViewBag.assets = assets.ToList();
            ViewBag.InvoiceHistory = JsonConvert.SerializeObject(invoiceHistory);




            List<LineChartDataPoint> dataPoints3 = new List<LineChartDataPoint>();

            dataPoints3.Add(new LineChartDataPoint(1388514600000, 102.1));
            dataPoints3.Add(new LineChartDataPoint(1391193000000, 104.83));
            dataPoints3.Add(new LineChartDataPoint(1393612200000, 104.04));
            dataPoints3.Add(new LineChartDataPoint(1396290600000, 104.87));
            dataPoints3.Add(new LineChartDataPoint(1398882600000, 105.71));
            dataPoints3.Add(new LineChartDataPoint(1401561000000, 108.37));
            dataPoints3.Add(new LineChartDataPoint(1404153000000, 105.23));
            dataPoints3.Add(new LineChartDataPoint(1406831400000, 100.05));
            dataPoints3.Add(new LineChartDataPoint(1409509800000, 95.85));
            dataPoints3.Add(new LineChartDataPoint(1412101800000, 86.08));
            dataPoints3.Add(new LineChartDataPoint(1414780200000, 76.99));
            dataPoints3.Add(new LineChartDataPoint(1417372200000, 60.7));
            dataPoints3.Add(new LineChartDataPoint(1420050600000, 47.11));
            dataPoints3.Add(new LineChartDataPoint(1422729000000, 54.79));
            dataPoints3.Add(new LineChartDataPoint(1425148200000, 52.83));
            dataPoints3.Add(new LineChartDataPoint(1427826600000, 57.54));
            dataPoints3.Add(new LineChartDataPoint(1430418600000, 62.51));
            dataPoints3.Add(new LineChartDataPoint(1433097000000, 61.31));
            dataPoints3.Add(new LineChartDataPoint(1435689000000, 54.34));
            dataPoints3.Add(new LineChartDataPoint(1438367400000, 45.69));
            dataPoints3.Add(new LineChartDataPoint(1441045800000, 46.28));
            dataPoints3.Add(new LineChartDataPoint(1443637800000, 46.96));
            dataPoints3.Add(new LineChartDataPoint(1446316200000, 43.11));
            dataPoints3.Add(new LineChartDataPoint(1448908200000, 36.57));
            dataPoints3.Add(new LineChartDataPoint(1451586600000, 29.78));
            dataPoints3.Add(new LineChartDataPoint(1454265000000, 31.03));
            dataPoints3.Add(new LineChartDataPoint(1456770600000, 37.34));
            dataPoints3.Add(new LineChartDataPoint(1459449000000, 40.75));
            dataPoints3.Add(new LineChartDataPoint(1462041000000, 45.94));
            dataPoints3.Add(new LineChartDataPoint(1464719400000, 47.69));
            dataPoints3.Add(new LineChartDataPoint(1467311400000, 44.13));
            dataPoints3.Add(new LineChartDataPoint(1469989800000, 44.87));
            dataPoints3.Add(new LineChartDataPoint(1472668200000, 45.04));
            dataPoints3.Add(new LineChartDataPoint(1475260200000, 49.29));
            dataPoints3.Add(new LineChartDataPoint(1477938600000, 45.26));
            dataPoints3.Add(new LineChartDataPoint(1480530600000, 52.62));

            ViewBag.DataPoints3 = JsonConvert.SerializeObject(dataPoints3);

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            try
            {
                // Fetch distinct subscriptions with their user count
                var subscriptionCounts = await _context.Subscriptions
                    .Select(s => new
                    {
                        s.SubscriptionName,
                        UserCount = _context.ApplicationUsers.Count(u => u.SubscriptionId == s.Id)
                    })
                    .Distinct()
                    .ToListAsync();

                return Json(new { success = true, data = subscriptionCounts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLandlords()
        {
            try
            {
                var landlordsPerMonth = await _context.Owner
                    .GroupBy(l => new { Month = l.AddedDate.Month, Year = l.AddedDate.Year })
                    .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Count = g.Count() })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToListAsync();

                var labels = landlordsPerMonth.Select(g => $"{g.Month}/{g.Year}").ToArray();
                var series = landlordsPerMonth.Select(g => g.Count).ToArray();

                return Json(new { success = true, labels, series });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllSubscribedUsers()
        {
            try
            {
                var activeSubscriptions = await _context.StripeSubscriptions
                    .Where(x => x.EndDate >= DateTime.UtcNow && !x.IsCanceled)
                    .ToListAsync();

                var latestSubscriptions = activeSubscriptions
                    .GroupBy(x => x.UserId)
                    .Select(g => g.OrderByDescending(x => x.Id).FirstOrDefault())
                    .Where(x => x != null)
                    .ToList();

                var subscribedUsersPerMonth = latestSubscriptions
                     .GroupBy(u => new { Month = u.AddedDate.Month, Year = u.AddedDate.Year })
                     .Select(g => new { g.Key.Month, g.Key.Year, Count = g.Count() })
                     .OrderBy(g => g.Year).ThenBy(g => g.Month)
                     .ToList();

                // Prepare labels and series for the chart
                var labels = subscribedUsersPerMonth.Select(g => $"{g.Month}/{g.Year}").ToArray();
                var series = subscribedUsersPerMonth.Select(g => g.Count).ToArray();

                return Json(new { success = true, labels, series });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        public async Task<IEnumerable<CustomerData>> GetAllUsers()
        {
            try
            {
                //var users = await (from ss in _context.StripeSubscriptions
                //                   join u in _context.ApplicationUsers on ss.UserId equals u.Id into userGroup
                //                   from u in userGroup.DefaultIfEmpty()
                //                   join s in _context.Subscriptions on u.SubscriptionId equals s.Id into subGroup
                //                   from s in subGroup.DefaultIfEmpty()
                //                   join o in _context.Owner on u.Id equals o.AddedBy into ownerGroup
                //                   from o in ownerGroup.DefaultIfEmpty()
                //                   where ss.EndDate >= DateTime.UtcNow && !ss.IsCanceled
                //                   group new { ss, u, s, o } by new
                //                   {
                //                       u.Id,
                //                       ss.UserId,
                //                       UserName = u.UserName ?? string.Empty,
                //                       Email = u.Email ?? string.Empty,
                //                       PhoneNumber = u.PhoneNumber ?? string.Empty,
                //                       CompanyName = u.CompanyName ?? string.Empty,
                //                       AddedDate = u.AddedDate,
                //                       SubscriptionName = s.SubscriptionName ?? string.Empty,
                //                       SubscriptionType = s.SubscriptionType ?? string.Empty,
                //                       StartDate = ss.StartDate,
                //                       EndDate = ss.EndDate
                //                   } into g
                //                   select new CustomerData
                //                   {
                //                       UserId = g.Key.UserId ?? string.Empty,
                //                       Name = g.Key.UserName,
                //                       EmailAddress = g.Key.Email,
                //                       PhoneNumber = g.Key.PhoneNumber,
                //                       SubscriptionName = g.Key.SubscriptionName,
                //                       SubscriptionType = g.Key.SubscriptionType,
                //                       Expiring = g.Key.EndDate.HasValue ?
                //                           EF.Functions.DateDiffDay(DateTime.UtcNow, g.Key.EndDate.Value) : (int?)null,
                //                       EndDate = g.Key.EndDate,
                //                       CompanyName = g.Key.CompanyName,
                //                       OwnerCount = g.Count(x => x.o != null)
                //                   }).OrderByDescending(x => x.UserId).ToListAsync();

                //return users;

                var users = await (from ss in _context.StripeSubscriptions
                                   join u in _context.ApplicationUsers on ss.UserId equals u.Id into userGroup
                                   from u in userGroup.DefaultIfEmpty()
                                   join s in _context.Subscriptions on u.SubscriptionId equals s.Id into subGroup
                                   from s in subGroup.DefaultIfEmpty()
                                   join o in _context.Owner on u.Id equals o.AddedBy into ownerGroup
                                   from o in ownerGroup.DefaultIfEmpty()
                                   where ss.EndDate >= DateTime.UtcNow && !ss.IsCanceled
                                   select new
                                   {
                                       ss.UserId,
                                       UserName = u.UserName ?? string.Empty,
                                       Email = u.Email ?? string.Empty,
                                       PhoneNumber = u.PhoneNumber ?? string.Empty,
                                       CompanyName = u.CompanyName ?? string.Empty,
                                       SubscriptionName = s.SubscriptionName ?? string.Empty,
                                       SubscriptionType = s.SubscriptionType ?? string.Empty,
                                       Expiring = ss.EndDate.HasValue ? EF.Functions.DateDiffDay(DateTime.UtcNow, ss.EndDate.Value) : (int?)null,
                                       EndDate = ss.EndDate,
                                       OwnerCount = o != null ? 1 : 0
                                   })
                                   .Distinct()
                                   .GroupBy(x => new { x.UserId })
                                   .Select(g => new CustomerData
                                   {
                                       UserId = g.Key.UserId,
                                       Name = g.FirstOrDefault().UserName,
                                       EmailAddress = g.FirstOrDefault().Email,
                                       PhoneNumber = g.FirstOrDefault().PhoneNumber,
                                       SubscriptionName = g.FirstOrDefault().SubscriptionName,
                                       SubscriptionType = g.FirstOrDefault().SubscriptionType,
                                       Expiring = g.FirstOrDefault().Expiring,
                                       EndDate = g.FirstOrDefault().EndDate,
                                       CompanyName = g.FirstOrDefault().CompanyName,
                                       OwnerCount = g.Sum(x => x.OwnerCount)
                                   })
                                   .OrderByDescending(x => x.UserId)
                                   .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the users.", ex);
            }
        }


    }
}