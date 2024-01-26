using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ChangePasswordRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }


    }
}
