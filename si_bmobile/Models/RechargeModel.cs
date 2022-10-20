using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class RechargeModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string keycode { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }

    }


    public class ValidateOutputModel
    {
        public int resultcode { get; set; }
        public string reference { get; set; }

    }

    public class RechargeMsisdnModel
    {
        public string reference { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }
    
    }

    public class RechargeOPModel {
        public long resultCode { get; set; }
        public string rechargeAmount { get; set; }
        public string newBalance { get; set; }
        public string reference { get; set; }
        public string msidnNumber { get; set; }
    }

    public class SMSModel : RechargeModel {
        public string message { get; set; }
    }
}