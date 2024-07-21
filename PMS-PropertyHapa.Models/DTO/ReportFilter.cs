using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ReportFilter
    {
        public string AddedBy { get; set; }
        public List<int?> TenantsIds { get; set; }
        public List<int?> LandlordIds { get; set; }
        public List<int?> AssetsIds { get; set; }
        public List<int?> PropertiesIds { get; set; }
        public List<int?> UnitsIds { get; set; }
        public DateTime? LeaseStartDateFilter { get; set; }
        public DateTime? LeaseEndDateFilter { get; set; }
        public DateTime? InvoiceStartDateFilter { get; set; }
        public DateTime? InvoiceEndDateFilter { get; set; }
        public DateTime? TaskStartDateFilter { get; set; }
        public DateTime? TaskEndDateFilter { get; set; }
        public DateTime? TaskDueStartDateFilter { get; set; }
        public DateTime? TaskDueEndDateFilter { get; set; }
        public DateTime? TenantAddedStartDateFilter { get; set; }
        public DateTime? TenantAddedEndDateFilter { get; set; }
        public List<int?> TenantTypes { get; set; }
        public List<int?> InvoicePaid { get; set; }
        public decimal? LeaseMinRentFilter { get; set; }
        public decimal? LeaseMaxRentFilter { get; set; }

    }
}
