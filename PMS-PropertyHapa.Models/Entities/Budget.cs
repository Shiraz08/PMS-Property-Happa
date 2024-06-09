using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PMS_PropertyHapa.Shared.Enum.SD;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Budget : BaseEntities
    {
        [Key]
        public int BudgetId { get; set; }
        public string BudgetName { get; set; }
        public BudgetType BudgetType { get; set; }
        public BudgetBy BudgetBy { get; set; }
        public int PropertyId { get; set; }
        public string StartingMonth { get; set; }
        public string FiscalYear { get; set; }
        public BudgetPeriod Period { get; set; }
        public List<BudgetItem>? Items { get; set; }
        public string ReferenceData { get; set; }
        public AccountingMethod AccountingMethod { get; set; } = AccountingMethod.Cash;
        public bool ShowReferenceData { get; set; } = false;
        public int? BudgetDuplicateId { get; set; }
        public string? DuplicatedBudgetName { get; set; }
        [Column(TypeName = "date")]
        public DateTime? DuplicationDate { get; set; }
        public bool IsDuplicated { get; set; } = false;


    }
}
