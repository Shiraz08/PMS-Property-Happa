using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ReportFilter
    {
        public List<int?>? PropertiesIds { get; set; }
        public DateTime? LeaseStartDateFilter { get; set; }
        public DateTime? LeaseEndDateFilter { get; set; }
        public DateTime? InvoiceStartDateFilter { get; set; }
        public DateTime? InvoiceEndDateFilter { get; set; }
        public DateTime? TaskStartDateFilter { get; set; }
        public DateTime? TaskEndDateFilter { get; set; }
        public DateTime? TaskDueStartDateFilter { get; set; }
        public DateTime? TaskDueEndDateFilter { get; set; }
    }
}
