using Microsoft.AspNetCore.Http;
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
        public string AccountName { get; set; }
        public string AccountHolder { get; set; }
        public string AccountIBAN { get; set; }
        public string AccountSwift { get; set; }
        public string AccountBank { get; set; }
        public string AccountCurrency { get; set; }
        public IFormFile DocumentFile { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentFileUrl { get; set; }
        public int? Expense { get; set; }
        public string AddedBy { get; set; }
    }
}
