using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    

    public class ProductModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Currency { get; set; }
        public long Price { get; set; }
        //public bool IsYearly { get; set; }
        //public bool IsTrial { get; set; }
    }
}
