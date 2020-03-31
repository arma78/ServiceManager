using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceManager.Models
{
    public class TwilioSMS
    {
        public string accountSid { get; set; }
        public string authToken { get; set; }
        public string statePerfix { get; set; }
        public string TwilioNumber { get; set; }
        public string Active { get; set; }
    }
}
