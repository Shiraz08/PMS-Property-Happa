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
    }

}

