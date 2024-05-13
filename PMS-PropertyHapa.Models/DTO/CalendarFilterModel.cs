using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class CalendarFilterModel
    {
        public string UserId { get; set; }
        public List<string> TenantFilter { get; set; }
        public DateTime? StartDateFilter { get; set; }
        public DateTime? EndDateFilter { get; set; }
    }

    public class CalendarEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string BoxColor { get; set; }
        public string TextColor { get; set; }
        public int? TenantId { get; set; }
    }

    public class OccupancyOverviewEvents
    {
        public int Id { get; set; }
        public string ResourceTitle { get; set; }
        public string ResourceId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    
    public class LeaseDataDto
    {
        public int Id { get; set; }
        public string Asset { get; set; }
        public string AssetUnit { get; set; }
        public string Tenant { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }


}
