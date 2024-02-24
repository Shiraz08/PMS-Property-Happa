using AutoMapper;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;

namespace PMS_PropertyHapa.Owner
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            //CreateMap<TO,FROM>().ReverseMap();

            CreateMap<Tenant, TenantModelDto>().ReverseMap();
        }
    }
}
