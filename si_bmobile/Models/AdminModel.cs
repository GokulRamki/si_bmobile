using System.Data.Entity;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace si_bmobile.Models
{
    public class AdminModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class User_History
    {
        public Nullable<long> id { get; set; }
        public Nullable<long> planId { get; set; }
        public string userId { get; set; }
        public string createdDate { get; set; }
        public string modifyDate { get; set; }
        public bool isDeleted { get; set; }
    }

    public class UserHistoryLogs
    {
        public long? id { get; set; }
        public long? planId { get; set; }
        public string planName { get; set; }
        public long? userId { get; set; }
        public string userName { get; set; }
        public Nullable<DateTime> createdDate { get; set; }
        public Nullable<DateTime> modifyDate { get; set; }
        public bool isDeleted { get; set; }
    }

    public class Application_Transaction
    {
        public Nullable<DateTime> date { get; set; }
        public long id { get; set; }
        public Transaction_LogDetail log { get; set; }
        public string msisdn { get; set; }
        public string reference { get; set; }
        public string status { get; set; }
    }

    public class Transaction_Logs
    {
        public Nullable<DateTime> date { get; set; }
        public string details { get; set; }
        public Nullable<long> id { get; set; }
        public string msisdn { get; set; }
        public string status { get; set; }
        public string task { get; set; }

    }

    //public class Application_Transaction
    //{
    //    public Nullable<DateTime> date { get; set; }
    //    public long id { get; set; }
    //    public object log { get; set; }
    //    public string msisdn { get; set; }
    //    public string reference { get; set; }
    //    public string status { get; set; }
    //}

    public class Transaction_LogDetail
    {
        public long id { get; set; }
        public long appTxId { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string details { get; set; }
        public Nullable<DateTime> date { get; set; }
    }

    public class YourPlanTransModel
    {
        public long Id { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public string from_msisdn { get; set; }
        public string to_msisdn { get; set; }
        public Nullable<double> total_amt { get; set; }
        public Nullable<System.DateTime> expiry_date { get; set; }
        public Nullable<bool> isExpired { get; set; }
    }


    public class RedeemFBPromotionsModel
    {       
        public string serial_number { get; set; }
        public string pin_number { get; set; }
        public string msisdn_no {get; set;}
        public string fb_voucher { get; set; }
        public decimal amount { get; set; }
        public List<SelectListItem> msisdn_list { get; set; }
    }

    public class VoiceCallActDeactModel
    {       
        public string msisdn_no { get; set; }
        public List<SelectListItem> msisdn_list { get; set; }

        public string vc_type { get; set; }
    }

    public class VCActDeactInputModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string keycode { get; set; }
        public string msisdn { get; set; }        
    }

    public class VCActDeactOutputModel
    {
        public string resultcode { get; set; }
        public string desc { get; set; }
    }

    public class FBPromotionInputModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string keycode { get; set; }
        public string serialNo { get; set; }
        public string amount { get; set; }
    }

    public class FBPromotionOutputModel
    {
        public int resultcode { get; set; }
        public string reference { get; set; }
        public string serialNo { get; set; }
        public string desc { get; set; }
    }

    #region top kad
    public class tbl_top_kad_log
    {
        [Key()]
        public long id { get; set; }

        [Required()]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "The MSISDN should be numeric")]
        public string msisdn { get; set; }

        [Required()]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "The Serial Number should be numeric")]
        public string serial_number { get; set; }

        [Required()]
        //[RegularExpression(@"^[0-9]+$", ErrorMessage = "The Invoice field should be a number")]
        public string invoice { get; set; }

        public string server_response { get; set; }

        public string server_reference { get; set; }

        [Required()]
        public DateTime created_on { get; set; }

        public string sever_desc { get; set; }

        [Required()]
        public string updated_by { get; set; }

        [Required()]
        public decimal recharge_amount { get; set; }

        public bool is_recharged { get; set; }
    }

    [NotMapped]
    public class top_kad_verify_model : tbl_top_kad_log
    {
        [Required()]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Confirm MSISDN should be numeric")]
        [Compare("msisdn", ErrorMessage = "Confirm MSISDN number does not match.")]
        public string confirm_msisdn { get; set; }

    }
    public class top_kad_model
    {
        public string username { get; set; }

        public string password { get; set; }

        public string keycode { get; set; }

        public string msisdn { get; set; }

        public string serialNo { get; set; }
    }
    public class top_kad_return_model
    {
        public string resultcode { get; set; }

        public string reference { get; set; }

        public string desc { get; set; }

        public string rechargedAmount { get; set; }
    }
    #endregion

    #region Import_FB_Vouchers

    public class Import_FB_Vouchers
    {
        public web_tbl_fb_promotions fb_promotions { get; set; }

        public string msg { get; set; }

        public bool is_valid { get; set; }
    }
    #endregion

    public class CheckMsisdnInputModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string keycode { get; set; }
        public string msisdn { get; set; }
    }

    public class CheckMsisdnOutputModel
    {
        public string resultcode { get; set; }
        public string reference { get; set; }
        public int msisdn_Status { get; set; }
    }

    public class RegistrationReportModel
    {
        public registration Reg { get; set; }

        public List<multiplesim> MultipileSim { get; set; }
    }

    public class SISubscriptionModel
    {
        //public long id { get; set; }
        //public string user_name { get; set; }
        //public string trans_number { get; set; }
        //public string msisdn { get; set; }
        //public DateTime created_on { get; set; }
        //public Nullable<DateTime> plan_purchased_on { get; set; }
        //public Decimal total_price { get; set; }
        //public long transaction_id { get; set; }
        //public string plan_name { get; set; }
        //public string plan_price { get; set; }
        public tbl_evd_plan_transaction et { get; set; }
        public tbl_evd_plan_details pd { get; set; }
        public web_tbl_care_user c { get; set; }
    }


    public class TransactionReportModel
    {
        public string user_name { get; set; }
        public string trans_number { get; set; }
        public string msisdn { get; set; }
        public string plans { get; set; }
       // public string plan_price { get; set; }
        public decimal total_price { get; set; }
        public DateTime trans_initiated { get; set; }
        public DateTime? trans_purchase { get; set; }
        public long transaction_id { get; set; }
        public string trans_status { get; set; }
    }

    [Serializable]
    [XmlRoot("mobileModel"), XmlType("osType")]
    public class mobileModel
    {
        public string Type { get; set; }
        public string Value { get; set; }

    }

    public class profilemodel
    {
        public string rolename { get; set; }

        public string first_name { get; set; }

        //public string username { get; set; }

        public string last_name { get; set; }

        public string email_id { get; set; }

        public string contact_number { get; set; }

        public string status { get; set; }

        public string location { get; set; }

        public string teamname { get; set; }
    }

}