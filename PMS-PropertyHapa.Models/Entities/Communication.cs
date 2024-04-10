using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Communication : BaseEntities
    {
        [Key]
        public int Communication_Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string PropertyIds { get; set; }
        public string TenantIds { get; set; }
        public bool IsByEmail { get; set; }
        public bool IsByText { get; set; }
        public bool IsShowCommunicationInTenantPortal { get; set; }
        public bool IsPostOnTenantScreen { get; set; }
        public DateTime? RemoveFeedDate { get; set; }
     
        public string Communication_File { set; get; }
    }
}
