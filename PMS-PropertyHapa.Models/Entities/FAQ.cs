using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class FAQ : BaseEntities
    {
        [Key]
        public int FAQId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
