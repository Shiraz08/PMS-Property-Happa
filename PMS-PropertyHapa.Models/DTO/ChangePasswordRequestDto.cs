using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ChangePasswordRequestDto
    {
        public string UserId { get; set; }
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
        public string newRepeatPassword { get; set; }

    }
}
