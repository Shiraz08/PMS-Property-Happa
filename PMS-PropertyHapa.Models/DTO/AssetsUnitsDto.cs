using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class AssetUnitDTO
    {
       
        public int UnitId { get; set; }
        public int AssetId { get; set; }
        public string UnitName { get; set; }
        public int Beds { get; set; }
        public int Bath { get; set; }
        public int Size { get; set; }
        public decimal Rent { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }

    }
}
