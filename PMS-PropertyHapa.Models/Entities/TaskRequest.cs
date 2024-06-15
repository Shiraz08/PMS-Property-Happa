using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class TaskRequest : BaseEntities
    {
        [Key]
        public int TaskRequestId { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public bool IsOneTimeTask { get; set; }
        public bool IsRecurringTask { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public int? DueDays { get; set; }
        public bool IsTaskRepeat { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Assignees { get; set; }
        public bool IsNotifyAssignee { get; set; }
        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual Assets Asset { get; set; }
        public int? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual AssetsUnits Unit { get; set; }
        public string TaskRequestFile { get; set; }
        public int? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public virtual Owner Owner { get; set; }
        public bool IsNotifyOwner { get; set; }
        public int? TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        public bool IsNotifyTenant { get; set; }
        public bool HasPermissionToEnter { get; set; }
        public string EntryNotes { get; set; }

        //Work Order
        public int? VendorId { get; set; }
        public bool ApprovedByOwner { get; set; }
        public bool PartsAndLabor { get; set; }
        public virtual ICollection<LineItem> LineItem { get; set; }
    }
}
