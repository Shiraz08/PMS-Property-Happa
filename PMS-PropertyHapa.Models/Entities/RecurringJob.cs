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
        public int RecurringJobs_Id { get; set; }
        public string JobType { get; set; }     
        public int? LateFeeId { get; set; }
        public int? LatefeePropertyId { get; set; }
        public int? AssetId { get; set; }
        public int AgreementFormId { get; set; }
        public string RentAmount { get; set; }
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DueDays { get; set; }
        public decimal LatefeeChargeAmount { get; set; }
        public bool IsRun { get; set; } = false;
    }
}
