using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PMS_PropertyHapa.Models.DTO
{
    public class AssetAndOwnersViewModel
    {
        public AssetDTO Asset { get; set; }
        public IEnumerable<OwnerDto> Owners { get; set; }
    }

}
