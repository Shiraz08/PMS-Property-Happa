using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        public int TenantId { get; set; }
        public string Manufacturer { get; set; }
        public string ModelName { get; set; }
        public string ModelVariant { get; set; }
        public string Engine { get; set; }
        public string Year { get; set; }
        public Tenant Tenant { get; set; }
    }
}