using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.ContentModel;
using PMS_PropertyHapa.Models;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Shared.Email;
using PMS_PropertyHapa.Staff.Services.IServices;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Staff.Auth.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;

        public HomeController(IAuthService authService/*, Logger<HomeController> logger*/)
        {
            _authService = authService;
           // _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var taskRequests = await _authService.GetAllTaskRequestsAsync();
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            
            if (currentUserId != null)
            {
                taskRequests = taskRequests.Where(s => s.AddedBy == currentUserId);
            }

            List<PieChart> dataPoints = new List<PieChart>
            {
                new PieChart(TaskStatusTypes.Completed.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.Completed.ToString())),
                new PieChart(TaskStatusTypes.InProgress.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.InProgress.ToString())),
                new PieChart(TaskStatusTypes.OnHold.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.OnHold.ToString())),
                new PieChart(TaskStatusTypes.NotStarted.ToString(), taskRequests.Count(x => x.Status == TaskStatusTypes.NotStarted.ToString()))
            };

            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            if (currentUserId != null)
            {
                ViewBag.totalTaskRequests = taskRequests.Count();
                taskRequests = taskRequests.Where(s => s.AddedBy == currentUserId
                                                      && ((s.AddedDate.HasValue && s.AddedDate.Value.ToString("yyyy-MM-dd") == currentDate)
                                                      || (s.ModifiedDate.HasValue && s.ModifiedDate.Value.ToString("yyyy-MM-dd") == currentDate)));
            }


            var userProfile = await _authService.GetProfileAsync(currentUserId);

            ViewBag.UserName = userProfile?.Name ?? "User";



            var addedDate = DateTime.Now;

            var oneMonthAgo = addedDate.AddMonths(-1);
            var threeMonthsAgo = addedDate.AddMonths(-3);
            var sixMonthsAgo = addedDate.AddMonths(-6);
            // Landlord Count
            var landlords = await _authService.GetAllLandlordAsync();

            if (currentUserId != null)
            {
                landlords = landlords.Where(s => s.AddedBy == currentUserId);

                
                ViewBag.totalLandlordsOneMonth = landlords.Count(s => s.AddedDate >= oneMonthAgo);
                ViewBag.totalLandlordsThreeMonths = landlords.Count(s => s.AddedDate >= threeMonthsAgo);
                ViewBag.totalLandlordsSixMonths = landlords.Count(s => s.AddedDate >= sixMonthsAgo);
            }

            //Tenant Count
            var tenants = await _authService.GetAllTenantsAsync();

            if (currentUserId != null)
            {
                tenants = tenants.Where(s => s.AddedBy == currentUserId);
                ViewBag.totalTenantsOneMonth  = tenants.Count(s => s.AddedDate >= oneMonthAgo);
                ViewBag.totalTenantsThreeMonths  = tenants.Count(s => s.AddedDate >= threeMonthsAgo);
                ViewBag.totalTenantsSixMonths = tenants.Count(s => s.AddedDate >= sixMonthsAgo);
            }

            //Assets Count
            var assets = await _authService.GetAllAssetsAsync();

            if (currentUserId != null)
            {
                assets = assets.Where(s => s.AddedBy == currentUserId);
                ViewBag.totalAssetsOneMonth   = assets.Count(s => s.AddedDate >= oneMonthAgo);
                ViewBag.totalAssetsThreeMonths   = assets.Count(s => s.AddedDate >= threeMonthsAgo);
                ViewBag.totalAssetsSixMonths = assets.Count(s => s.AddedDate >= sixMonthsAgo);

            }
            //Units Count
           // var filter = new Filter();
            //filter.AddedBy = currentUserId;

            //var assetUnits = await _authService.GetUnitsByUserAsync(filter);
            var assetUnits = await _authService.GetAllUnitsAsync();
            if(currentUserId != null)
            {
                assetUnits = assetUnits.Where(s => s.AddedBy == currentUserId);
                ViewBag.totalUnitsOneMonth = assetUnits.Count(s => s.AddedDate >= oneMonthAgo);
                ViewBag.totalUnitsThreeMonths = assetUnits.Count(s => s.AddedDate >= threeMonthsAgo);
                ViewBag.totalUnitsSixMonths = assetUnits.Count(s => s.AddedDate >= sixMonthsAgo);
                ViewBag.totalUnits = assetUnits.Count();
            }

            //Occupied and Vacant Units Count
            var leases = await _authService.GetAllLeasesAsync();
            if (currentUserId != null)
            {
                leases = leases.Where(s => s.AddedBy == currentUserId).ToList();
            }

            var occupiedUnitsOneMonth = leases.Where(l => l.AddedDate >= oneMonthAgo).Select(l => l.UnitId).Distinct().Count();

            var occupiedUnitsThreeMonths = leases.Where(l => l.AddedDate >= threeMonthsAgo).Select(l => l.UnitId).Distinct().Count();

            var occupiedUnitsSixMonths = leases.Where(l => l.AddedDate >= sixMonthsAgo).Select(l => l.UnitId).Distinct().Count();

            var vacancyOneMonth = ViewBag.totalUnitsOneMonth - occupiedUnitsOneMonth;
            var vacancyThreeMonths = ViewBag.totalUnitsThreeMonths - occupiedUnitsThreeMonths;
            var vacancySixMonths = ViewBag.totalUnitsSixMonths - occupiedUnitsSixMonths;

            ViewBag.totalOccupiedUnitsOneMonth = occupiedUnitsOneMonth;
            ViewBag.totalOccupiedUnitsThreeMonths = occupiedUnitsThreeMonths;
            ViewBag.totalOccupiedUnitsSixMonths = occupiedUnitsSixMonths;

            ViewBag.totalVacancyOneMonth = vacancyOneMonth;
            ViewBag.totalVacancyThreeMonths = vacancyThreeMonths;
            ViewBag.totalVacancySixMonths = vacancySixMonths;

            var totalOccupiedUnits = leases.Select(l => l.UnitId).Distinct().Count();
            ViewBag.totalVacancy = ViewBag.totalUnits - totalOccupiedUnits;

            //TaskRequest Count
            var tasks = await _authService.GetAllTaskRequestsAsync();
            if (currentUserId != null)
            {
                tasks = tasks.Where(s => s.AddedBy == currentUserId && s.Status == "NotStarted").ToList();
                ViewBag.totalTasks = tasks.Count();
            }

            var assignTasks = await _authService.GetAllTaskRequestsAsync();
            if (currentUserId != null)
            {
                assignTasks = assignTasks.Where(s => s.AddedBy == currentUserId && s.Status == "InProgress").ToList();
                ViewBag.totalAssignTasks = assignTasks.Count();
            }

            var closedTasks = await _authService.GetAllTaskRequestsAsync();
            if (currentUserId != null)
            {
                closedTasks = closedTasks.Where(s => s.AddedBy == currentUserId && s.Status == "Completed").ToList();
                ViewBag.totalClosedTasks = closedTasks.Count();
            }

            var invoiceHistory = await _authService.GetAllInvoicesAsync();

            if (currentUserId != null)
            {
                invoiceHistory = invoiceHistory.Where(s => s.AddedBy == currentUserId).ToList();

            }
            ViewBag.totalInvoices = invoiceHistory.Count();

            var appplications = await _authService.GetApplicationsAsync();

            if (currentUserId != null)
            {
                appplications = appplications.Where(s => s.AddedBy == currentUserId).ToList();
            }
            ViewBag.totalApplications = appplications.Count();
            List<PieChart> invoiceStatus = new List<PieChart>
            {
                new PieChart("Paid",  invoiceHistory.Count(i => i.InvoicePaid == true)),
                new PieChart("UnPaid", invoiceHistory.Count(i => i.InvoicePaid != true)),
                new PieChart("Paid To Owner", invoiceHistory.Count(i => i.InvoicePaidToOwner == true)),
                new PieChart("UnPaid To Owner", invoiceHistory.Count(i => i.InvoicePaidToOwner != true))
            };


            var taskCountsByMonth = taskRequests
                    .Where(t => t.AddedBy == currentUserId)
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
        
        public async Task<IActionResult> GetRevenueExpense()
        {
            var invoiceHistory = await _authService.GetAllInvoicesAsync();
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            if (currentUserId != null)
            {
                invoiceHistory = invoiceHistory.Where(s => s.AddedBy == currentUserId && s.InvoicePaid == true).ToList();
            }
            var monthlyRevenue = invoiceHistory
                .GroupBy(x => new { x.InvoiceDate?.Year, x.InvoiceDate?.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(x => x.RentAmount)
                }).ToList();

            var revenueData = monthlyRevenue
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => x.TotalRevenue)
                .ToList();

            var revenueByMonth = new Dictionary<string, decimal>
            {
                { "Jan", 0 },
                { "Feb", 0 },
                { "Mar", 0 },
                { "Apr", 0 },
                { "May", 0 },
                { "Jun", 0 },
                { "Jul", 0 },
                { "Aug", 0 },
                { "Sep", 0 },
                { "Oct", 0 },
                { "Nov", 0 },
                { "Dec", 0 }
            };

            foreach (var item in monthlyRevenue)
            {
                var monthName = new DateTime(item.Year.Value, item.Month.Value, 1).ToString("MMM");
                revenueByMonth[monthName] = item.TotalRevenue;
            }

            var lineItems = await _authService.GetAllLineItemsAsync();
            if (currentUserId != null)
            {
                lineItems = lineItems.Where(s => s.AddedBy == currentUserId).ToList();
            }
            var monthlyExpense = lineItems
                .GroupBy(x => new { x.AddedDate?.Year, x.AddedDate?.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalExpense = g.Sum(x => x.Price)
                }).ToList();

            var expenseData = monthlyExpense
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => x.TotalExpense)
                .ToList();

            var expenseByMonth = new Dictionary<string, decimal>
            {
                { "Jan", 0 },
                { "Feb", 0 },
                { "Mar", 0 },
                { "Apr", 0 },
                { "May", 0 },
                { "Jun", 0 },
                { "Jul", 0 },
                { "Aug", 0 },
                { "Sep", 0 },
                { "Oct", 0 },
                { "Nov", 0 },
                { "Dec", 0 }
            };

            foreach (var item in monthlyExpense)
            {
                var monthName = new DateTime(item.Year.Value, item.Month.Value, 1).ToString("MMM");
                expenseByMonth[monthName] = item.TotalExpense;
            }
            var data = new BarChartDataModel
            {
                Series = new List<SeriesData>
                {
                    new SeriesData { Name = "Revenue", Data = revenueByMonth.Values.ToList() },
                    new SeriesData { Name = "Expense", Data = expenseByMonth.Values.ToList() },
                },
                Categories = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
            };


            //var data = new BarChartDataModel
            //{
            //    Series = new List<SeriesData>
            //{
            //        new SeriesData { Name = "Revenue", Data = new List<int> { 44, 55, 57, 56, 61, 58, 63, 60, 66 } },
            //        new SeriesData { Name = "Expense", Data = new List<int> { 76, 85, 101, 98, 87, 105, 91, 114, 94 } }
            //    },
            //    Categories = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
            //};

            return Ok(data);
        }

        //public async Task<IActionResult> GetOccupancyRate()
        //{
        //    var filter = new Filter();
        //    var currentUserId = Request?.Cookies["userId"]?.ToString();
        //    filter.AddedBy = currentUserId;
        //    var units = await _authService.GetUnitsByUserAsync(filter);

        //    var totalUnits = units.Count();
        //    var unitIds = units.Select(x => x.UnitId).ToList();

        //    var leases = await _authService.GetAllLeasesAsync();
        //    if (currentUserId != null)
        //    {
        //        leases = leases.Where(s => s.AddedBy == currentUserId);
        //    }
        //    var occupiedUnits = leases.Select(l => l.UnitId).Distinct().Count();


        //    var occupancyRate = (occupiedUnits / (double)totalUnits) * 100;
        //    var vacancyRate = 100 - occupancyRate;

        //    var radialChart = new ChartDataModel
        //    {
        //        Series = new List<int> { (int)occupancyRate, (int)vacancyRate },
        //        Labels = new List<string> { "Occupancy Rate", "Vacancy Rate" },
        //        Total = totalUnits,
        //    };

        //    return Ok(radialChart);
        //}

        public async Task<IActionResult> GetOccupancyRate()
        {
            var filter = new Filter();
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            filter.AddedBy = currentUserId;
            var units = await _authService.GetUnitsByUserAsync(filter);

            var totalUnits = units.Count();
            var unitIds = units.Select(x => x.UnitId).ToList();

            var leases = await _authService.GetAllLeasesAsync();
            if (currentUserId != null)
            {
                leases = leases.Where(s => s.AddedBy == currentUserId);
            }
            var occupiedUnits = leases.Select(l => l.UnitId).Distinct().Count();

            var occupancyRate = (occupiedUnits / (double)totalUnits) * 100;

            var radialChart = new ChartDataModel
            {
                Series = new List<int> { occupiedUnits, (int)occupancyRate },
                Labels = new List<string> { $"Total Occupancy", $"Occupancy Rate" },
                Total = totalUnits,
            };

            return Ok(radialChart);
        }


        public async Task<IActionResult> GetVacancyRate()
        {
            var filter = new Filter();
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            filter.AddedBy = currentUserId;
            var units = await _authService.GetUnitsByUserAsync(filter);

            var totalUnits = units.Count();
            var unitIds = units.Select(x => x.UnitId).ToList();

            var leases = await _authService.GetAllLeasesAsync();
            if (currentUserId != null)
            {
                leases = leases.Where(s => s.AddedBy == currentUserId);
            }
            var occupiedUnits = leases.Select(l => l.UnitId).Distinct().Count();

            var vacancyRate = 100 - (occupiedUnits / (double)totalUnits) * 100;
            var vacantUnits = totalUnits - occupiedUnits;

            var radialChart = new ChartDataModel
            {
                Series = new List<int> { vacantUnits, (int)vacancyRate },
                Labels = new List<string> { $"Total Vacancy", $"Vacancy Rate" },
                Total = totalUnits,
            };

            return Ok(radialChart);
        }


        //public async Task<IActionResult> GetRentStatus()
        //{
        //    var currentUserId = Request?.Cookies["userId"]?.ToString();
        //    var invoiceHistory = await _authService.GetAllInvoicesAsync();

        //    if (currentUserId != null)
        //    {
        //        invoiceHistory = invoiceHistory.Where(s => s.AddedBy == currentUserId).ToList();
        //    }

        //    var paidCount = invoiceHistory.Count(i => i.InvoicePaid == true);
        //    var unpaidCount = invoiceHistory.Count(i => i.InvoicePaid != true);
        //    var paidToOwnerCount = invoiceHistory.Count(i => i.InvoicePaidToOwner == true);
        //    var unpaidToOwnerCount = invoiceHistory.Count(i => i.InvoicePaidToOwner != true);

        //    var data = new
        //    {
        //        Series = new List<object>
        //{
        //    new { name = "Paid/Unpaid", data = new List<int> { paidCount, unpaidCount } },
        //    new { name = "Paid to Owner/Unpaid to Owner", data = new List<int> { paidToOwnerCount, unpaidToOwnerCount } }
        //},
        //        Categories = new List<string> { "Paid", "Unpaid"},
        //        SubCategories = new List<string> { "Paid/Unpaid", "Paid to Owner/Unpaid to Owner" }
        //    };

        //    return Json(data);
        //}


        public async Task<IActionResult> GetRentStatus()
        {
            var currentUserId = Request?.Cookies["userId"]?.ToString();
            var invoiceHistory = await _authService.GetAllInvoicesAsync();

            if (currentUserId != null)
            {
                invoiceHistory = invoiceHistory.Where(s => s.AddedBy == currentUserId).ToList();
            }

            // Generate a list of all 12 months
            var allMonths = Enumerable.Range(1, 12).Select(m => new DateTime(DateTime.Now.Year, m, 1).ToString("MMM yyyy")).ToList();

            // Group by month and year, calculate sums
            var groupedData = invoiceHistory
                .GroupBy(i => new { Year = i.AddedDate.Year, Month = i.AddedDate.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    PaidAmount = g.Where(i => i.InvoicePaid == true).Sum(i => i.RentAmount),
                    UnpaidAmount = g.Where(i => i.InvoicePaid != true).Sum(i => i.RentAmount)
                })
                .ToList();

            // Merge with all 12 months
            var finalData = allMonths.Select(month => new
            {
                Date = month,
                PaidAmount = groupedData.FirstOrDefault(g => g.Date == month)?.PaidAmount ?? 0,
                UnpaidAmount = groupedData.FirstOrDefault(g => g.Date == month)?.UnpaidAmount ?? 0
            }).ToList();

            var data = new
            {
                Series = new List<object>
        {
            new { name = "Paid", data = finalData.Select(g => new { x = g.Date, y = g.PaidAmount }).ToList() },
            new { name = "Unpaid", data = finalData.Select(g => new { x = g.Date, y = g.UnpaidAmount }).ToList() }
        },
                Categories = allMonths
            };

            return Json(data);
        }






        public async Task<IActionResult> GetKanbanData()
        {
            var currentUserId = Request?.Cookies["userId"]?.ToString();

            var tasks = await _authService.GetAllTaskRequestsAsync();
            if (currentUserId != null)
            {
                tasks = tasks.Where(s => s.AddedBy == currentUserId).ToList();
            }

            var boards = new List<object>
                                {
                                    new
                                    {
                                        id = "_todo",
                                        title = "Not Started",
                                        classs = "bg-info", 
                                        dragTo = new string[] { "_working", "_hold" },
                                        item = tasks.Where(t => t.Status == TaskStatusTypes.NotStarted.ToString())
                                                    .Select(t => new { 
                                                        title = GenerateTaskHtml(t)
                                                    }).ToList()
                                    },
                                    new
                                    {
                                        id = "_working",
                                        title = "In-Progress",
                                        classs = "bg-warning", 
                                        dragTo = new string[] { "_done", "_hold" },
                                        item = tasks.Where(t => t.Status == TaskStatusTypes.InProgress.ToString())
                                                    .Select(t => new {
                                                        title = GenerateTaskHtml(t)
                                                    }).ToList()
                                    },
                                    //new
                                    //{
                                    //    id = "_hold",
                                    //    title = "On-Hold",
                                    //    classs = "bg-warning", 
                                    //    dragTo = new string[] { "_todo", "_working" },
                                    //    item = tasks.Where(t => t.Status == TaskStatusTypes.OnHold.ToString())
                                    //                .Select(t => new {
                                    //                    title = GenerateTaskHtml(t)
                                    //                }).ToList()
                                    //},
                                    new
                                    {
                                        id = "_done",
                                        title = "Completed",
                                        classs = "bg-success",
                                        dragTo = new string[] { "_working" },
                                        item = tasks.Where(t => t.Status == TaskStatusTypes.Completed.ToString())
                                                    .Select(t => new {
                                                        title = GenerateTaskHtml(t)
                                                    }).ToList()
                                    }
                                };

            return Ok(new { boards });
        }

        private string GenerateTaskHtml(TaskRequestDto task)
        {
            return $@"
                        <a class='kanban-box' href='#'>
                            <span class='date'>{task.DueDate?.ToString("dd/MM/yy")}</span>
                            <span class='badge badge-{GetPriorityBadgeClass(task.Priority)} f-right'>{task.Priority}</span>
                            <h6>{task.Subject}</h6>
                            <div class='media'>
                                <img class='img-20 me-1 rounded-circle' src='../assets/images/user/3.jpg' alt=''>
                                <div class='media-body'>
                                    <p>{task.Description}</p>
                                </div>
                            </div>
                            <div class='d-flex mt-3'>
                                <div class='customers'>
                                    <ul>
                                        <li class='d-inline-block'><img class='img-20 rounded-circle' src='../assets/images/user/3.jpg' alt=''></li>
                                        <li class='d-inline-block'><img class='img-20 rounded-circle' src='../assets/images/user/1.jpg' alt=''></li>
                                        <li class='d-inline-block'><img class='img-20 rounded-circle' src='../assets/images/user/5.jpg' alt=''></li>
                                    </ul>
                                </div>
                            </div>
                        </a>";
        }

        private string GetPriorityBadgeClass(string priority)
        {
            return priority.ToLower() switch
            {
                "high" => "danger",
                "medium" => "info",
                "low" => "success",
                _ => "secondary"
            };
        }


        public async Task<IActionResult> GetCashFlowData()
        {
            var invoiceHistory = await _authService.GetAllInvoicesAsync();
            var currentUserId = Request?.Cookies["userId"]?.ToString();

            // Filter invoices by current user and paid status if applicable
            if (currentUserId != null)
            {
                invoiceHistory = invoiceHistory.Where(s => s.AddedBy == currentUserId && s.InvoicePaid == true).ToList();
            }

            // Calculate monthly revenue for the current year
            var currentYear = DateTime.Now.Year;
            var monthlyRevenue = invoiceHistory
                .Where(x => x.InvoiceDate.HasValue && x.InvoiceDate.Value.Year == currentYear)
                .GroupBy(x => new { x.InvoiceDate?.Year, x.InvoiceDate?.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(x => x.RentAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            // Prepare data for chart
            var seriesData = monthlyRevenue.Select(x => (int)x.TotalRevenue).ToList();
            var monthLabels = new List<string>();

            // Format month labels as "yyyy-MM-dd"
            foreach (var item in monthlyRevenue)
            {
                var monthLabel = new DateTime(item.Year.Value, item.Month.Value, 1).ToString("yyyy-MM-dd");
                monthLabels.Add(monthLabel);
            }

            var data = new ChartDataModel
            {
                Series = seriesData,
                Labels = monthLabels
            };

            return Ok(data);
        }


        public class ChartDataModel
        {
            public List<int> Series { get; set; }
            public List<string> Labels { get; set; }
            public int Total { get; set; }
        }
        public class BarChartDataModel
        {
            public List<SeriesData> Series { get; set; }
            public List<string> Categories { get; set; }
        }

        public class SeriesData
        {
            public string Name { get; set; }
            public List<decimal> Data { get; set; }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRoles()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var roles = await _authService.GetUserRolesAsync(currenUserId);
            return Ok(roles);
        }

        [HttpGet]
        public async Task<IActionResult> IsUserTrial()
        {
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            var roles = await _authService.IsUserTrialAsync(currenUserId);
            return Ok(roles);
        }
    }
}