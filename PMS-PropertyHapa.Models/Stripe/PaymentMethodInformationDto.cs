﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Stripe
{
    public class PaymentMethodInformationDto
    {
        public int Id { get; set; }

        public string Country { get; set; }

        public string CardType { get; set; }

        public string CardHolderName { get; set; }

        public string CardLast4Digit { get; set; }

        public string ExpiryMonth { get; set; }

        public string ExpiryYear { get; set; }

        public string Email { get; set; }

        public string GUID { get; set; }

        public string PaymentMethodId { get; set; }

        public string CustomerId { get; set; }
        public string AddedBy { get; set; }
    }
}
