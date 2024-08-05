﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class LateFeeDto
    {
        public int LateFeeId { get; set; }
        public bool ChargeLateFeeActive { get; set; }
        public int DueDays { get; set; }
        public string Frequency { get; set; }
        public string CalculateFee { get; set; }
        public decimal Amount { get; set; }
        public int? ChartAccountId { get; set; }
        public string? Description { get; set; }
        public bool IsSendARemainder { get; set; } = false;
        public bool IsNotifyTenants { get; set; } = false;
        public bool IsEnableSms { get; set; } = false;
        public bool IsChargeLateFee { get; set; } = false;
        public bool IsMonthlyLimit { get; set; } = false;
        public bool IsDailyLimit { get; set; } = false;
        public bool IsMinimumBalance { get; set; } = false;
        public bool IsChargeLateFeeonSpecific { get; set; } = false;
        public string AddedBy { get; set; }
    }
}