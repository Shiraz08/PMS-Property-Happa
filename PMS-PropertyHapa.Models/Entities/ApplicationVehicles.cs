using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class ApplicationVehicles : BaseEntities
    {
        [Key]
        public int VehicleId { get; set; }
        public int ApplicationId { get; set; }
        public string Manufacturer { get; set; }
        public string ModelName { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public string Year { get; set; }
    }
}
