using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace si_bmobile.Models
{
    public class BundlesModel
    {
        public List<BundleModel> DataPlan { get; set; }
        public List<BundleModel> Voice { get; set; }
        public List<BundleModel> VoiceSMSDataPlan { get; set; }

        public List<BundleModel> RoamingPlan { get; set; }

        [Required(ErrorMessage = "Please select a plan to purchase!")]
        public string _planId { get; set; }

        public List<SelectListItem> MsisdnLst { get; set; }

        public string dtopuptext { get; set; }
        public string btnMode { get; set; }

        public string _sel_msisdn { get; set; }

        public string q_msisdn { get; set; }
        public string err_Msg { get; set; }
        public string pr_type { get; set; }
        public string po_type { get; set; }
    }

    public class MSISDNModel
    {
        public string sMsisdn { get; set; }
        public string sbal { get; set; }
       
    }

    public class BundleModel
    {
        [Required]
        public long Id { get; set; }

        public string UserId { get; set; }

        [Required]
        public string PlanName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        public string _sPrice { get; set; }

        [Required]
        public string Size { get; set; }

        //[Required]
        public int Validity { get; set; }

        public string AccountType { get; set; }

        public bool isPostpaid { get; set; }

        public bool isActive { get; set; }

        public Nullable<int> orderby { get; set; }

        //public Dictionary<string,object> htmlAttributes { get; set; }
        public string htmlAttributes { get; set; }

        public bool isChecked { get; set; }

        public bool isVoice { get; set; }

        public bool isDeleted { get; set; }

        public string sStatus { get; set; }

       public string sType { get; set; }

        public bool isPlanActive { get; set; }

        public string SmsAccountType { get; set; }

        public string VoiceAccountType { get; set; }

        public string IddAccountType { get; set; }

        public string voiceSize { get; set; }

        public string smsCount { get; set; }

        public string voice_desc { get; set; }

        public string sms_desc { get; set; }

        public string validity_txt { get; set; }

        public bool isSelfcare { get; set; }

        public bool isOnlyData { get; set; }

        //public bool isCCtopup { get; set; }

        public bool isCorporate { get; set; }

        [Required]
        public int bundle_type_id { get; set; }

        public string roamingVoiceAccountType { get; set; }
        public string roamingSmsAccountType { get; set; }
        public string romingDataAccountType { get; set; }
               
    }
    //test
    public class DModel {
        public string Name { get; set; }
        public object Value { get; set; }

        
    }

    
}