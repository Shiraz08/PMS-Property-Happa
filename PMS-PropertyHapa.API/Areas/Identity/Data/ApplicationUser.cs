using Microsoft.AspNetCore.Identity;

namespace PMS_PropertyHapa.API.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
