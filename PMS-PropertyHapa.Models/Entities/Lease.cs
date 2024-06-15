using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class Lease : BaseEntities
    {
        public int LeaseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual Assets Assets { get; set; }
        public int? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual AssetsUnits AssetsUnits { get; set; }
        public string SelectedProperty { get; set; }

        public string SelectedUnit { get; set; }
        public bool IsSigned { get; set; }
        public string SignatureImagePath { get; set; }

        // Lease Type Flags
        public bool IsFixedTerm { get; set; }
        public bool IsMonthToMonth { get; set; }

        // Security Deposit Flag
        public bool HasSecurityDeposit { get; set; }



        public int TenantsTenantId { get; set; }

        // Late Fee Policy
        public string LateFeesPolicy { get; set; }

        public string AppTenantId { get; set; }

        // Collections of related entities
        public virtual Tenant Tenants { get; set; }
        public virtual ICollection<RentCharge> RentCharges { get; set; }
        public virtual ICollection<SecurityDeposit> SecurityDeposit { get; set; }

        public virtual ICollection<FeeCharge> FeeCharge { get; set; }
    }
}
