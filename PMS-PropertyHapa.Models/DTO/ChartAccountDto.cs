using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ChartAccountDto
    {
        public int ChartAccountId { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountType { get; set; }
        public int AccountSubTypeId { get; set; }
        public string AccountSubType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSubAccount { get; set; }
        public int? ParentAccountId { get; set; }
        public string ParentAccount { get; set; }
        public List<ChildAccountDto?>? ChildAccountsDto { get; set; }
        public string AddedBy { get; set; }
    }

    public class ChildAccountDto
    {
        public int ChartAccountId { get; set; }
        public int AccountTypeId { get; set; }
        public string Name { get; set; }
        public int? ParentAccountId { get; set; }
    }
}
