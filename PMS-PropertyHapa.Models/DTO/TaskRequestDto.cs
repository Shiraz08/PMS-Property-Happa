using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class TaskRequestDto
    {
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
        public string Asset { get; set; }
        public int? UnitId { get; set; }
        public string Unit { get; set; }
        public string TaskRequestFile { get; set; }
        public int? OwnerId { get; set; }
        public string Owner { get; set; }
        public bool IsNotifyOwner { get; set; }
        public int? TenantId { get; set; }
        public string Tenant { get; set; }
        public bool IsNotifyTenant { get; set; }
        public bool HasPermissionToEnter { get; set; }
        public string EntryNotes { get; set; }
        public int? VendorId { get; set; }
        public bool ApprovedByOwner { get; set; }
        public bool PartsAndLabor { get; set; }
        public string AddedBy { get; set; }
        public List<LineItemDto> LineItems { get; set; } = new List<LineItemDto>(); 
    }

    public class LineItemDto {
        public int LineItemId { get; set; }
        public int? TaskRequestId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int ChartAccountId { get; set; }
        public string AccountName { get; set; }
        public string Memo { get; set; }
    }
}
