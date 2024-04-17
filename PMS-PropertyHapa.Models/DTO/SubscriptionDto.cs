using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class SubscriptionDto
    { 
        public int? Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        // Additional properties for subscription details
        public int DiskSpaceGB { get; set; } // in GB
        public int EmailAccounts { get; set; }
        public int BandwidthGB { get; set; } // in GB
        public int Subdomains { get; set; }
        public int Domains { get; set; }

        public string AppTenantId { get; set; }
        public int TenantId { get; set; }

    }
}
