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
        public string Manufacturer { get; set; } //Make
        public string ModelName { get; set; } //Model
        public string ModelVariant { get; set; } //License Plate
        public string color { get; set; } 
        public string Year { get; set; } //Year
        public Tenant Tenant { get; set; }
    }
}