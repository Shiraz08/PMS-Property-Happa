using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class ApplicationPetsDto
    {
        public int PetId { get; set; }
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public string Picture { get; set; }
        public string PictureName { get; set; }
    }
        
}
