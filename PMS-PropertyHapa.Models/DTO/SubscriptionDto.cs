﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class SubscriptionDto
    { 
        public int? Id { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionType { get; set; }
        public decimal Price { get; set; }

        public string Currency { get; set; }
        public string SmallDescription { get; set; }
        public decimal Tax { get; set; } 
        public int NoOfUnits { get; set; }

        public string AppTenantId { get; set; }
        public int TenantId { get; set; }

    }
}