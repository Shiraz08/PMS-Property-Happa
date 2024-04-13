using Microsoft.AspNetCore.Http;
using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PMS_PropertyHapa.Models.DTO.TenantModelDto;

namespace PMS_PropertyHapa.Models.DTO
{
    public class CommunicationDto
    {
        public int Communication_Id { get; set; }

        public string UserID { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string PropertyIds { get; set; }
        public string TenantIds { get; set; }
        public bool IsByEmail { get; set; }
        public bool IsByText { get; set; }
        public bool IsShowCommunicationInTenantPortal { get; set; }
        public bool IsPostOnTenantScreen { get; set; }
        public bool Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? RemoveFeedDate { get; set; }
        public DateTime AddedAt { get; set; }

        public string AddedBy { get; set; }

        public string Communication_File { set; get; }
        public IFormFile CommunicationFile { set; get; }

        public int TotalPropertiesCount { get; set; }
        public int TotalTenantsCount { get; set; }
    }

}