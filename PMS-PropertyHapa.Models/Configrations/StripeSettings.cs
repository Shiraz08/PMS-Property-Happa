using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.Configrations
{
    public class StripeSettings
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string EndPointKey { get; set; }
        public string SuccessCallbackUrl { get; set; }
        public string CancelCallbackUrl { get; set; }
    }
}
