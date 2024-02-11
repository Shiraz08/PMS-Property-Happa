using AutoMapper;
using PMS_PropertyHapa.API.Areas.Identity.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();

            CreateMap<Tenant, TenantModelDto>().ReverseMap();
        }
    }
}
