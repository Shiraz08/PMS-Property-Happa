using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class AssetDTO
    {
        public int AssetId { get; set; }
        public string SelectedPropertyType { get; set; }
        public string SelectedSubtype { get; set; }

        

        public string AppTenantId { get; set; }

        public string Image { get; set; }
        public string PictureFileName { get; set; }
        public IFormFile PictureFile { get; set; }

        public string BuildingNo { get; set; }
        public string BuildingName { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }

        public string AppTid { get; set; }

        public List<UnitDTO> Units { get; set; } = new List<UnitDTO>();
        public OwnerDto OwnerData { get; set; }

        public string SelectedBankAccountOption { get; set; }
        public string SelectedReserveFundsOption { get; set; }
        public string SelectedOwnershipOption { get; set; }

        public int OwnerId { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerCompanyName { get; set; }
        public string OwnerAddress { get; set; }
        public string OwnerDistrict { get; set; }
        public string OwnerRegion { get; set; }
        public string OwnerCountryCode { get; set; }
        public string OwnerCountry { get; set; }

        public string OwnerImage { get; set; }  
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }

    public class UnitDTO
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Beds { get; set; }
        public int Bath { get; set; }
        public int Size { get; set; }
        public decimal Rent { get; set; }
    }
}
