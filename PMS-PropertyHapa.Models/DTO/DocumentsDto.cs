using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class DocumentsDto
    {
        public int DocumentsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile Document { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUrl { get; set; }
        public string AddedBy { get; set; }
    }
}
