﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Assets : BaseEntities
    {
        [Key]
        public int AssetId { get; set; }
        public int OwnerId { get; set; }
        public string SelectedPropertyType { get; set; }
        public string SelectedSubtype { get; set; }
        public string BuildingNo { get; set; }
        public string BuildingName { get; set; }
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


        public string AppTenantId { get; set; }
        public virtual ICollection<TaskRequest> TaskRequest { get; set; }
    }
}
