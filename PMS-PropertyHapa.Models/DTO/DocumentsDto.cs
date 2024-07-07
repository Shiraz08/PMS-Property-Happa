using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class DocumentsDto
    {
        public int DocumentsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public IFormFile Document { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUrl { get; set; }
        public int? AssetId { get; set; }
        public string AssetName { get; set; }
        public int? UnitId { get; set; }
        public string UnitName { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string AddedBy { get; set; }
    }
}
