﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class PaymentInformationDto
    {
        public int Id { get; set; }

        public decimal? ProductPrice { get; set; }

        public decimal? AmountCharged { get; set; }

        public DateTime? ChargeDate { get; set; }

        public string TransactionId { get; set; }

        public string PaymentStatus { get; set; }

        public string Currency { get; set; }

        public string CustomerId { get; set; }
        public string AddedBy { get; set; }
    }
}