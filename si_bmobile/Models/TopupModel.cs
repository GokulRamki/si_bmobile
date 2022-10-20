using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Configuration;
namespace si_bmobile.Models
{

    public class TopupModel
    {
        public TopupModel()
        {
            isMyNumber = true;
        }

        public string VoucherNumber { get; set; }

        [Required(ErrorMessage = "The Mobile Number field is required!")]
        public string MSISDN_Number { get; set; }

        public List<SelectListItem> MsisdnLst { get; set; }

        public string Message { get; set; }
        public bool Status { get; set; }

        public List<SelectListItem> MsisdnLst_temp { get; set; }

        [Required(ErrorMessage = "The Mobile Number field is required!")]
        public string MSISDN_Number_temp { get; set; }

        [Required(ErrorMessage = "The Mobile Number field is required!")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Only Numbers allowed!")]
        [Remote("ChkPostpaid", "Care", AdditionalFields = "MSISDN_Number_other", ErrorMessage = "Only Valid Prepaid number is allowed!")]
        public string MSISDN_Number_other { get; set; }


        [Required(ErrorMessage = "The Amount field is required!")]
        public string tpAmount { get; set; }


        [Required(ErrorMessage = "The Amount field is required!")]
        //[Range(10, 200, ErrorMessage = "TopUp value must be between K10.00 and K200.00")]
        public string tpAmountMy { get; set; }

        [Required(ErrorMessage = "The Amount field is required!")]
        //[Range(10, 200, ErrorMessage = "TopUp value must be between K10.00 and K200.00")]
        public string tpAmountOther { get; set; }

        public long UserId { get; set; }


        public PaymentPGModel pay { get; set; }


        public bool isTOPUP { get; set; }
        public List<SelectListItem> lstTopupVals { get; set; }

        public List<SelectListItem> lstOtherTopupVals { get; set; }

        
        public bool isMyNumber { get; set; }


        public string tpPaidType { get; set; }
    }

    public class PaymentPGModel
    {
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public string primary_msisdn { get; set; }
        public string paid_for_msisdn { get; set; }
        public string paid_amount { get; set; }
        public long paid_userid { get; set; }
        public int payment_mode { get; set; }
        public long product_id { get; set; }
        public string product_name { get; set; }
        public string payment_type { get; set; }
        public string payment_gateway { get; set; }
        public string sess_id { get; set; }
        public string purchse_desc { get; set; }
        public List<web_tbl_selfcare_order_item> order_items { get; set; }

        public string receiptno { get; set; }

    }

    public class VoucherResultModel
    {
        public string ResultDescription { get; set; }
        public string ResultCode { get; set; }
        public string VoucherAmount { get; set; }

    }
    public class tbl_bm_topup_value
    {
        [Key()]
        public Int64 id { get; set; }
        public string currency { get; set; }
        public string value { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_on { get; set; }
        public System.Nullable<DateTime> modified_on { get; set; }
        public bool is_android { get; set; }
        public bool is_ios { get; set; }
        public bool is_web { get; set; }
    }
}