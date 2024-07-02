using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Entities
{
    public class VideoTutorial : BaseEntities
    {
        [Key]
        public int TutorialId { get; set; }
        public string Title { get; set; }
        public string VideoLink { get; set; }
    }
}
