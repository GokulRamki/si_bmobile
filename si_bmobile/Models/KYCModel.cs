using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace si_bmobile.Models
{
    public class KYCModel
    {


    }

    public class tbl_kyc_customer_purchase
    {

        [Key()]
        public long cust_pur_id { get; set; }

        public long cust_id { get; set; }

        [Required()]
        public string cust_status { get; set; }

        public string cust_handset_model1 { get; set; }

        public string cust_handset_model2 { get; set; }

        public string cust_handset_model3 { get; set; }

        public string cust_mobile_no1 { get; set; }

        public string cust_mobile_no2 { get; set; }

        public string cust_mobile_no3 { get; set; }

        public string cust_imei_no1 { get; set; }

        public string cust_imei_no2 { get; set; }

        public string cust_imei_no3 { get; set; }

    }

    public class tbl_kyc_customer
    {

        [Key()]
        public long cust_id { get; set; }

        [Required()]
        public string name { get; set; }

        public string surname { get; set; }

        [Required()]
        public string sex { get; set; }

        [Required()]
        public bool is_employed { get; set; }


        public string pobox { get; set; }

        [Required()]
        public int town { get; set; }

        [Required()]
        public int province { get; set; }

        [Required()]
        public string suburb { get; set; }

       
        public string email { get; set; }

        [Required()]
        public DateTime kycdate { get; set; }

        public bool signature { get; set; }


        public DateTime created_on { get; set; }

        public Nullable<DateTime> modified_on { get; set; }

        public Nullable<DateTime> deleted_on { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }

        public byte[] photo_image { get; set; }

        public string dob { get; set; }

        public string lt_ref { get; set; }
    }

    public class tbl_kyc_customer_identification
    {

        [Key()]
        public long cust_identity_id { get; set; }

        public long cust_id { get; set; }

        public bool is_drv_lic { get; set; }

        public string drv_lic { get; set; }

        public bool is_student_id { get; set; }

        public string student_id { get; set; }

        public bool is_emp_id { get; set; }

        public string emp_id { get; set; }

        public bool is_nasfund_id { get; set; }

        public string nasfund_id { get; set; }

        public bool is_nbw_super_id { get; set; }

        public string nbw_super_id { get; set; }

        public bool is_other_id { get; set; }

        public string other_id { get; set; }

        public string other_id_name { get; set; }
    }

    public class tbl_kyc_customer_acknowlegement
    {
        [Key()]
        public long ack_id { get; set; }

        public long cust_id { get; set; }

        public bool is_name_matched { get; set; }

        public bool is_doc_genuine { get; set; }

        public DateTime ack_date { get; set; }

        public bool is_officer_sign { get; set; }

    }
    public class tbl_kyc_simactivation_log
    {
        [Key()]
        public long Id { get; set; }

        public string msisdn_no { get; set; }

        public string puk_code { get; set; }

        public string sim_no { get; set; }

        public string ref_no { get; set; }

        public string activatedby { get; set; }

        public bool is_sim_active { get; set; }

        public Nullable<DateTime> activated_on { get; set; }

        public Nullable<DateTime> created_on { get; set; }

    }
    public class PROVINCIAL_CODES
    {
        [Key()]
        public int id { get; set; }

        public string PROVINCE { get; set; }

        public int CODE { get; set; }

        public string STATE_CODE { get; set; }

        public string DISTRICT_CODE { get; set; }
    }

    public class DISTRICT_CODES
    {
        [Key()]
        public int id { get; set; }

        public string DISTRICT_N { get; set; }

        public int DISTRICT_C { get; set; }

        public int PROVINCE_C { get; set; }
    }


   

    public class tbl_kyc_taccode
    {
        public long Id { get; set; }

        public string model_id { get; set; }

        public string tac_code { get; set; }
    }

    public class kyc_Custome_Detail_Model
    {

        public tbl_kyc_customer_purchase kyc_cust_purchase { get; set; }

        public tbl_kyc_customer_identification kyc_cust_identification { get; set; }

        public tbl_kyc_customer kyc_cust_Details { get; set; }

        public tbl_kyc_customer_acknowlegement kyc_cust_ack { get; set; }
    }

    public class kyc_grid_list
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Sex { get; set; }

        public string Is_employed { get; set; }

        public string pobox { get; set; }

        public string town { get; set; }

        public string province { get; set; }

        public string suburb { get; set; }

        public string email { get; set; }

        public string Customersignature { get; set; }

        public string Is_nameMatched { get; set; }

        public string Is_documentGenuine { get; set; }

        public string is_officerSignature { get; set; }

        public string drv_lic { get; set; }

        public string student_id { get; set; }

        public string emp_id { get; set; }

        public string nasfund_id { get; set; }

        public string nbw_super_id { get; set; }

        public string other_id { get; set; }

        public string mobile_no { get; set; }

        public long id { get; set; }

        public string Acknowledge_date { get; set; }

        public string Purchase_date { get; set; }

        public string cust_status { get; set; }

        public string cust_handset_model1 { get; set; }

        public string cust_handset_model2 { get; set; }

        public string cust_handset_model3 { get; set; }

        public string cust_mobile_no1 { get; set; }

        public string cust_mobile_no2 { get; set; }

        public string cust_mobile_no3 { get; set; }

        public string cust_imei_no1 { get; set; }

        public string cust_imei_no2 { get; set; }

        public string cust_imei_no3 { get; set; }

        public List<tbl_kyc_simactivation_log> kyc_sim_log_list { get; set; }

        public string dob { get; set; }

        public string lt_ref { get; set; }
    }
    public class tbl_kyc_version_update
    {
        [Key]
        public int Id { get; set; }

        [Required]
        //[Remote("check_kyc_version", "Admin", HttpMethod = "GET", ErrorMessage = "This version already exist!")]
        [Display(Name = "App Version")]
        public decimal kyc_version { get; set; }

        [Required]
        [Display(Name = "APK Url")]
        public string apk_url { get; set; }

        public string description { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }

        public DateTime created_on { get; set; }
    }
   

    #region customer detail model
    public class Custome_Detail_Model
    {

        public tbl_kyc_customer_purchase cust_purchase { get; set; }

        public tbl_kyc_customer_identification cust_identification { get; set; }

        public tbl_kyc_customer cust_Details { get; set; }

        public tbl_kyc_customer_acknowlegement cust_ack { get; set; }

        public string photo { get; set; }
    }

    public class resultModel
    {
        public string result_code { get; set; }

        public string reference { get; set; }
    }
    public class number_exist_model
    {
        public string code { get; set; }

        public string msisdn_number { get; set; }
    }
    public class Province_Model
    {
        public string Province_name { get; set; }

        public int Province_code { get; set; }
    }
    public class District_Model
    {
        public string District_name { get; set; }

        public int District_code { get; set; }
    }
    #endregion

    #region LT model
    public class LTModel
    {
        public string title { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }

        public string gender { get; set; }

        public string dob { get; set; }

        public string idType { get; set; }

        public string idNumber { get; set; }

        public string msisdn { get; set; }

        public string email { get; set; }

        public string postBoxNo { get; set; }

        public string countryCode { get; set; }

        public string stateCode { get; set; }

        public string districtCode { get; set; }

        public string address1 { get; set; }

        public string address2 { get; set; }

    }


    #endregion

    #region service4 LT model
    public class LTService4Model
    {
        public string title { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }

        public string gender { get; set; }

        public string dob { get; set; }

        public string idType { get; set; }

        public string idNumber { get; set; }

        public string msisdn { get; set; }

        public string email { get; set; }


    }
    #endregion

    #region LT return model
    public class LTReturnModel
    {
        public string resultCode { get; set; }

        public string reference { get; set; }

    }
    #endregion

    #region special promotion customer
    public class tbl_spc_promo_cust
    {
        [Key()]
        public long Id { get; set; }

        [Required]
        public string f_name { get; set; }

        public string l_name { get; set; }

        [Required]
        public DateTime dob { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }

        [Required]
        public DateTime created_on { get; set; }

        public Nullable<DateTime> modified_on { get; set; }

        public string u_identification { get; set; }
    }
    #endregion


}
