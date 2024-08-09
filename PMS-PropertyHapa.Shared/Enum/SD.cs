using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Shared.Enum
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public static string AccessToken = "JWTToken";
        public static string RefreshToken = "RefreshToken";
        public static string CurrentAPIVersion = "v2";
        public const string Admin = "admin";
        public const string User = "user";
        public const string Customer = "customer";
        public enum ContentType
        {
            Json,
            MultipartFormData,
        }
        public enum SubscriptionTypes
        {
            Free
        }

        public static class TaskTypes
        {
            public const string Task = "Task";
            public const string TenantRequest = "TenantRequest";
            public const string OwnerRequest = "OwnerRequest";
            public const string WorkOrderRequest = "WorkOrderRequest";
        }

        public enum PaymentMode
        {
            OneTime,
            Subscription
        }

        public enum PaymentInterval
        {
            None,
            Day,
            Week,
            Month,
            Year
        }

        public enum TenantTypes
        {
          Tenanted = 1,
          NonTenanted = 2,
        }
        
        public enum InvoiceTypes
        {
          Paid = 1,
          UnPaid = 2,
        }
        
        public enum TaskStatusTypes
        {
          NotStarted,
          InProgress,
          Completed,
          OnHold,
        }

        public enum BudgetType
        {
            ProfitAndLoss = 1,
            BalanceSheet = 2,
        }

        public enum BudgetBy
        {
            Property = 1,
            Portfolio = 2,
        }
        public enum BudgetPeriod
        {
            Monthly = 1,
            Quarterly = 2,
            Yearly = 3,
        }
        public enum AccountingMethod
        {
            Cash = 1,
            Accrual = 2,
        }

        public enum UserPermissions
        {
            ViewLandlord = 1,
            AddLandlord = 2,
            ViewAssets = 3,
            AddAssets = 4,
            ViewTenant = 5,
            AddTenant = 6,
            ViewApplication = 7,
            AddApplication = 8,
            ViewLease = 9,
            AddLease = 10,
            ViewLeaseInvoices = 11,
            ViewMaintenance = 12,
            AddMaintenance = 13,
            ViewVendor = 14,
            AddVendor = 15,
            ViewVendorCategories = 16,
            AddVendorCategories = 17,
            ViewVendorClassification = 18,
            AddVendorClassification = 19,
            ViewAccounting = 20,
            ViewRent = 21,
            ViewAssestExpense = 22,
            ViewDeposit = 23,
            ViewBilling = 24,
            ViewInvoices = 25,
            ViewAccountType = 26,
            AddAccountType = 27,
            ViewAccountSubType = 28,
            AddAccountSubType = 29,
            ViewChartofAccounts = 30,
            AddChartofAccounts = 31,
            ViewBudget = 32,
            AddBudget = 33,
            ViewTask = 34,
            AddTask = 35,
            AddOwnerRequest = 36,
            AddTenantRequest = 37,
            AddWorkOrder = 38,
            ViewCommunication = 39,
            AddCommunication = 40,
            ViewCalendar = 41,
            ViewOccupancyOverview = 42,
            ViewReports = 43,
            ViewLeaseReports = 44,
            ViewFinanceReports = 45,
            ViewTenantReports = 46,
            ViewAssetReports = 47,
            ViewLandlordReports = 48,
            ViewDocuments = 49,
            AddDocuments = 50,
            PaymentIntegration = 51,
            QuickBookIntegration = 52,
            ViewSupportCenter = 53,
            ViewFAQ = 54,
            ContactUs = 55,
            BookAFreeDemo = 56,
            ViewVideoTutorial = 57,
            ViewSettings = 58,
            AddLateFee = 59,
            AddSubscription = 60
        }

    }
}
