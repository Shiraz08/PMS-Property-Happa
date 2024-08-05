﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class CheckoutOrderDto
    {
        public string ClientSecret { get; set; } 
        public string SessionId { get; set; } 
        public string PubKey { get; set; } 
        public bool Success { get; set; }
        public string CustomerId { get; set; }
        public string PriceId { get; set; }
    }
}