using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TaskRequestHistoryDto
    {
        public int TaskRequestHistoryId { get; set; }
        public int TaskRequestId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string DocumentFile { get; set; }
        public int? Expense { get; set; }
        public string AddedBy { get; set; }
    }
}
