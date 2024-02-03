using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Please Enter Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        public string? Password { get; set; }

        public string? ReturnUrl { get; set; }

        public bool Remember { get; set; }
    }

}

