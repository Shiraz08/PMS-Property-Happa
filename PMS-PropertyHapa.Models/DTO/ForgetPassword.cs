using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ForgetPassword
    {
        public long userId { get; set; }
        public string UserName { get; set; }
        public string email { get; set; }
        public string officialPhone { get; set; }
    }
}
