using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Documents : BaseEntities
    {
        [Key]
        public int DocumentsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int? AssetId { get; set; }
        public int? UnitId { get; set; }
        public int? OwnerId { get; set; }
        public int? TenantId { get; set; }
        [NotMapped]
        public IFormFile Document { get; set; }
        public string DocumentUrl { get; set; }
    }
}
