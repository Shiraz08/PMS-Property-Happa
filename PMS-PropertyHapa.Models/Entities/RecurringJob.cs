using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class RecurringJob : BaseEntities
    {
        [Key]
        public int RecurringJobsId { get; set; }
        public string JobType { get; set; }     
        public int? TenantId { get; set; }
        public int? LateFeeId { get; set; }
        public int? LateFeeAssetId { get; set; }
        public int? AssetId { get; set; }
        public int? LateFeeLeaseId { get; set; }
        public int LeaseId { get; set; }
        public string RentAmount { get; set; }
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DueDays { get; set; }
        public int? DueDaysProcess { get; set; }
        public decimal LateFeeChargeAmount { get; set; }
        public string Frequency { get; set; }
        public string CalculateFee { get; set; }
        public bool IsRun { get; set; } = false;
    }
}
