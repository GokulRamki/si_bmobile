//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace si_bmobile.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class bundle
    {
        public long Id { get; set; }
        public System.DateTime Created_at { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Size { get; set; }
        public Nullable<int> Validity { get; set; }
        public Nullable<bool> isPostpaid { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> Modified_on { get; set; }
        public Nullable<int> orderby { get; set; }
        public bool isVoice { get; set; }
        public Nullable<bool> isDelete { get; set; }
        public string AccountType { get; set; }
        public string SmsAccountType { get; set; }
        public string VoiceAccountType { get; set; }
        public string IddAccountType { get; set; }
        public string UlAccountType { get; set; }
        public string voiceSize { get; set; }
        public string smsCount { get; set; }
        public Nullable<bool> isOnlyData { get; set; }
        public Nullable<bool> isSelfCare { get; set; }
        public string voice_desc { get; set; }
        public string sms_desc { get; set; }
        public string validity_txt { get; set; }
        public Nullable<bool> isCorporate { get; set; }
        public string ussddesc { get; set; }
        public Nullable<bool> isUssd { get; set; }
        public string menu { get; set; }
        public string roamingVoiceAccountType { get; set; }
        public string roamingSmsAccountType { get; set; }
        public string romingDataAccountType { get; set; }
    }
}
