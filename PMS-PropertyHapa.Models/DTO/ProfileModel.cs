using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ProfileModel
    {
        public string UserId { get; set; } = "043a35d4-d995-469b-a43c-e9d173f50071";
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? ExistingPictureUrl { get; set; }
        public IFormFile NewPicture { get; set; }
        public string PhoneNumber { get; set; }

        public string Picture { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Locality { get; set; }
        public string District { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool Status { get; set; }
    }

}

