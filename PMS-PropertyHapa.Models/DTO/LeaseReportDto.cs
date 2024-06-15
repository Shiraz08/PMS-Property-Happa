using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class LeaseReportDto
    {
        public  string Lease {  get; set; }
        public  DateTime? StartDate { get; set; }
        public  DateTime? EndDate { get; set; }
        public  bool Status { get; set; }
        public  int? UnitId { get; set; }
        public  int? PropertyId { get; set; }
        public  string Property { get; set; }
        public  string Unit { get; set; }
        public  decimal RentCharges { get; set; }
        public  decimal SecurityDeposit { get; set; }
    }           
}
