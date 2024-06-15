using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TaskRequestReportDto
    {
        public string Task { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PropertyId { get; set; }
        public string Property { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
    }
}
