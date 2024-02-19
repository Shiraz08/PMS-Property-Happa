using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.DTO
{
    public class PropertySubTypeDto
    {
        public int PropertySubTypeId { get; set; }
        public int PropertyTypeId { get; set; }
        public string PropertySubTypeName { get; set; }
        public string Icon_String { get; set; }
        public string Icon_SVG { get; set; }
        public IFormFile Icon_SVG2 { get; set; }

        public Guid AppTenantId { get; set; }
        public string TenantId { get; set; }
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
