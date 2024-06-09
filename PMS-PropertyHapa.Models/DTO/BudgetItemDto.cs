using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class BudgetItemDto
    {
        public string AccountName { get; set; }
        public string Period { get; set; }
        public string Total { get; set; }
        public BudgetItemMonthDto? BudgetItemMonth { get; set; }
    }
    public class BudgetItemMonthDto
    {
        public BudgetItemDto BudgetItem { get; set; }
        public string? Jan { get; set; }
        public string? Feb { get; set; }
        public string? March { get; set; }
        public string? April { get; set; }
        public string? May { get; set; }
        public string? June { get; set; }
        public string? July { get; set; }
        public string? Aug { get; set; }
        public string? Sep { get; set; }
        public string? Oct { get; set; }
        public string? Nov { get; set; }
        public string? Dec { get; set; }
        public string? YearStart { get; set; }
        public string? YearEnd { get; set; }
        public string? quat1 { get; set; }
        public string? quat2 { get; set; }
        public string? quat4 { get; set; }
        public string? quat5 { get; set; }
    }
}
