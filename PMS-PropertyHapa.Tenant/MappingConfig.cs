using AutoMapper;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.Tenant
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            //CreateMap<TO,FROM>().ReverseMap();

            CreateMap<PMS_PropertyHapa.Models.Entities.Tenant, TenantModelDto>().ReverseMap();
        }
    }
}
