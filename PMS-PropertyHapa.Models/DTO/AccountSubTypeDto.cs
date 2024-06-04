using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class AccountSubTypeDto
    {
        public int AccountSubTypeId { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountType { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string AddedBy { get; set; }
    }
}
