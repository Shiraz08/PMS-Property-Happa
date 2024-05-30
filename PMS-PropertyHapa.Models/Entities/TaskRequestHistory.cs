using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class TaskRequestHistory : BaseEntities
    {
        public int TaskRequestHistoryId { get; set; }
        public int TaskRequestId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
}
