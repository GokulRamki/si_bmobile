using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class ReturnModel
    {
        public string resultCode { get; set; }
    }
   

    public class GetBunldePlansModel
    {
        public string resultCode { get; set; }

        public bundle bundles { get; set; }
    }

    public class BunldePlanServiceModel
    {
       
        public long bundleId { get; set; }
        public string planName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Size { get; set; }
        public Nullable<int> Validity { get; set; }
        public Nullable<bool> isPostpaid { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<int> orderby { get; set; }
        public bool isVoice { get; set; }
        public string AccountType { get; set; }
        public string SmsAccountType { get; set; }
        public string VoiceAccountType { get; set; }
        public string IddAccountType { get; set; }
        public string UlAccountType { get; set; }
        public string voiceSize { get; set; }
        public string smsCount { get; set; }
        public Nullable<bool> isOnlyData { get; set; }
        public bool iscorporate { get; set; }
        public string roamingVoiceAccountType { get; set; }
        public string roamingSmsAccountType { get; set; }
        public string romingDataAccountType { get; set; }
    }

    public class BundleplanModel
    {
        public long bundle_id { get; set; }

        public string bundle_Name { get; set; }

        public string desc { get; set; }

        public decimal Price { get; set; }

        public string Validity { get; set; }

        public bool isActive { get; set; }

        public int bundle_type_id { get; set; }


    }    

    public class getBundleTypeModel : ReturnModel
    {
        public int bdleTypeId { get; set; }
    }
}