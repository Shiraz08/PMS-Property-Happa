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
    public class CommunicationsViewModel
    {
        public IEnumerable<CommunicationDto> Communications { get; set; }
        public int TotalPropertiesCount { get; set; }
        public int TotalTenantsCount { get; set; }
    }

}