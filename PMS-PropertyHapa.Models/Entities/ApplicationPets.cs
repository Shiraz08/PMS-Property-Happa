using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class ApplicationPets : BaseEntities
    {
        [Key]
        public int PetId { get; set; }
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public string Picture { get; set; }
    }
}
