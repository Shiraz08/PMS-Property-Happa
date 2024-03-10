using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Assets
    {
        [Key]
        public int AssetId { get; set; }
        public string SelectedPropertyType { get; set; }
        public string SelectedSubtype { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zipcode { get; set; }
        public string Image { get; set; } 
       public virtual ICollection<AssetsUnits> Units { get; set; }

        public string SelectedBankAccountOption { get; set; }
        public string SelectedReserveFundsOption { get; set; }
        public string SelectedOwnershipOption { get; set; }

        public string OwnerName { get; set; }
        public string OwnerCompanyName { get; set; }
        public string OwnerAddress { get; set; }
        public string OwnerStreet { get; set; }
        public string OwnerZipcode { get; set; }
        public string OwnerCity { get; set; }
        public string OwnerCountry { get; set; }
    }
}
