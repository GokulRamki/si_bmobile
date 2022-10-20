using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace si_bmobile.Models
{
    public class SimRegModel
    {
    }

    public class web_tbl_sim_details
    {
        [Key()]
        public long id { get; set; }
        public string sim_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string driving_lic { get; set; }
        public string status { get; set; }
        public Nullable<DateTime> created_on { get; set; }
    }

    public class tbl_customer
    {
        [Key()]
        public long cust_id { get; set; }
        public string name { get; set; }
        public string mobile_no { get; set; }
        public string sex { get; set; }
        public bool is_employed { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public int reg_status_id { get; set; }
        public Nullable<DateTime> created_on { get; set; }
        public Nullable<DateTime> modified_on { get; set; }
        public Nullable<DateTime> deleted_on { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
    }

    public class tbl_customer_acknowlegement
    {
        [Key()]
        public long ack_id { get; set; }

        public long cust_id { get; set; }

        public bool is_original_sign { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public Nullable<DateTime> cust_ack_date { get; set; }
    }

    public class tbl_customer_identification
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
    }

    public class tbl_customer_mobile_no
    {
        [Key()]
        public long cust_mob_id { get; set; }

        public long cust_id { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public string cust_mobile_no1 { get; set; }

        public string cust_mobile_no2 { get; set; }

        public string cust_mobile_no3 { get; set; }

        public string cust_mobile_no4 { get; set; }
    }

    public class tbl_customer_purchase
    {
        [Key()]
        public long cust_pur_id { get; set; }

        public long cust_id { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public string cust_status { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public string cust_handset_model1 { get; set; }

        public string cust_handset_model2 { get; set; }

        public string cust_handset_model3 { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public string cust_mobile_no1 { get; set; }

        public string cust_mobile_no2 { get; set; }

        public string cust_mobile_no3 { get; set; }

        [Required(ErrorMessage = "This field is required field")]
        public string cust_imei_no1 { get; set; }

        public string cust_imei_no2 { get; set; }

        public string cust_imei_no3 { get; set; }
    }

    public class tbl_customer_reg_status
    {
        [Key()]
        public long id { get; set; }
        public string status { get; set; }
    }

    public class tbl_customer_retailer_use
    {
        [Key()]
        public long cust_ret_id { get; set; }
        public long ret_id { get; set; }
        public long cust_id { get; set; }
        public bool is_match_id { get; set; }
        public bool is_doc_expired { get; set; }
        public string payment_receipt { get; set; }
        public Nullable<DateTime> modified_on { get; set; }
        public string sale_person_name { get; set; }
        public bool is_retailer_sign { get; set; }
    }

    public class tbl_retailers
    {
        [Key()]
        public long id { get; set; }
        public string name { get; set; }
    }

    //customize models

    public class new_sim_customer_model
    {
        public long cust_id { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public string name { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public string sex { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public bool is_employed { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public string address { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        public int reg_status_id { get; set; }
        public Nullable<DateTime> created_on { get; set; }
        public Nullable<DateTime> modified_on { get; set; }
        public Nullable<DateTime> deleted_on { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }

    }

    public class sim_reg_model
    {
        public new_sim_customer_model cust { get; set; }
        public tbl_customer_identification cust_iden { get; set; }
        public tbl_customer_purchase cust_purchase { get; set; }
        public tbl_customer_acknowlegement cust_ack { get; set; }

    }

    public class replace_sim_reg_model
    {
        public replace_sim_customer_model cust { get; set; }
        public tbl_customer_identification cust_iden { get; set; }
        public tbl_customer_acknowlegement cust_ack { get; set; }
        public tbl_customer_mobile_no cust_mob { get; set; }

    }

    public class update_sim_reg_model
    {
        public new_sim_customer_model cust { get; set; }
        public tbl_customer_identification cust_iden { get; set; }
        public tbl_customer_purchase cust_purchase { get; set; }
        public tbl_customer_acknowlegement cust_ack { get; set; }
        public tbl_customer_retailer_use retailerUser { get; set; }
    }

    public class update_replace_sim_reg_model
    {
        public replace_sim_customer_model cust { get; set; }
        public tbl_customer_identification cust_iden { get; set; }
        public tbl_customer_acknowlegement cust_ack { get; set; }
        public tbl_customer_mobile_no cust_mob { get; set; }
        public tbl_customer_retailer_use retailerUser { get; set; }
    }

    public class replace_sim_customer_model
    {
        public long cust_id { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public string name { get; set; }
        [Required(ErrorMessage = "This field is required field")]
        public string mobile_no { get; set; }
        public int reg_status_id { get; set; }
        public Nullable<DateTime> created_on { get; set; }
        public Nullable<DateTime> modified_on { get; set; }
        public Nullable<DateTime> deleted_on { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }

    }



    //public class Cust_SimRegModel
    //{
    //    public Cust_RegModel customer { get; set; }
    //    public Cust_IdentityModel cust_iden { get; set; }
    //    public Cust_MobileModel cust_mob { get; set; }
    //    public Cust_PurchaseModel cust_purchase { get; set; }
    //    public Cust_AckModel cust_ack { get; set; }
    //    public Cust_RetailerUseModel retailer_user { get; set; }
    //}

    //public class NewSimRegModel
    //{
    //    public long cust_id { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string name { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string sex { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public bool is_employed { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string address { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email format")]
    //    public string email { get; set; }
    //    public int reg_status_id { get; set; }
    //    public Nullable<DateTime> created_on { get; set; }
    //    public Nullable<DateTime> modified_on { get; set; }
    //    public Nullable<DateTime> deleted_on { get; set; }
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }

    //    public Cust_IdentityModel cust_iden { get; set; }
    //    public Cust_PurchaseModel cust_purchase { get; set; }
    //    public Cust_AckModel cust_ack { get; set; }
    //    //public Cust_MobileModel cust_mob { get; set; }
    //    //public Cust_RetailerUseModel retailer_user { get; set; }
    //}

    //public class ReplaceSimRegModel
    //{
    //    public long cust_id { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string name { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string mobile_no { get; set; }
    //    public int reg_status_id { get; set; }
    //    public Nullable<DateTime> created_on { get; set; }
    //    public Nullable<DateTime> modified_on { get; set; }
    //    public Nullable<DateTime> deleted_on { get; set; }
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }

    //    public Cust_IdentityModel cust_iden { get; set; }
    //    public Cust_AckModel cust_ack { get; set; }
    //    public Cust_MobileModel cust_mob { get; set; }
    //    //public Cust_PurchaseModel cust_purchase { get; set; }
    //    //public Cust_RetailerUseModel retailer_user { get; set; }
    //}

    //public class UpdateNewSimRegModel
    //{
    //    public long cust_id { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string name { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string sex { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public bool is_employed { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string address { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email format")]
    //    public string email { get; set; }
    //    public int reg_status_id { get; set; }
    //    public Nullable<DateTime> created_on { get; set; }
    //    public Nullable<DateTime> modified_on { get; set; }
    //    public Nullable<DateTime> deleted_on { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }

    //    public Cust_IdentityModel cust_iden { get; set; }
    //    public Cust_PurchaseModel cust_purchase { get; set; }
    //    public Cust_AckModel cust_ack { get; set; }
    //    public Cust_RetailerUseModel retailerUser { get; set; }

    //}

    //public class UpdateReplaceSimRegModel
    //{
    //    public long cust_id { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string name { get; set; }
    //    [Required(ErrorMessage = "This field is required field")]
    //    public string mobile_no { get; set; }
    //    public int reg_status_id { get; set; }
    //    public Nullable<DateTime> created_on { get; set; }
    //    public Nullable<DateTime> modified_on { get; set; }
    //    public Nullable<DateTime> deleted_on { get; set; }
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }

    //    public Cust_IdentityModel cust_iden { get; set; }
    //    public Cust_AckModel cust_ack { get; set; }
    //    public Cust_MobileModel cust_mob { get; set; }
    //    public Cust_RetailerUseModel retailerUser { get; set; }

    //}



    //#region Sim Reg

    //public class Cust_RegModel
    //{
    //    public long cust_id { get; set; }

    //    public string name { get; set; }

    //    public string mobile_no { get; set; }

    //    public string sex { get; set; }

    //    public bool is_employed { get; set; }

    //    public string address { get; set; }

    //    public string email { get; set; }

    //    public int reg_status_id { get; set; }

    //    public Nullable<DateTime> created_on { get; set; }

    //    public Nullable<DateTime> modified_on { get; set; }

    //    public Nullable<DateTime> deleted_on { get; set; }

    //    public bool is_active { get; set; }

    //    public bool is_deleted { get; set; }
    //}

    //public class Cust_IdentityModel
    //{
    //    public long cust_identity_id { get; set; }
    //    public long cust_id { get; set; }
    //    public bool is_drv_lic { get; set; }
    //    public string drv_lic { get; set; }
    //    public bool is_student_id { get; set; }
    //    public string student_id { get; set; }
    //    public bool is_emp_id { get; set; }
    //    public string emp_id { get; set; }
    //    public bool is_nasfund_id { get; set; }
    //    public string nasfund_id { get; set; }
    //    public bool is_nbw_super_id { get; set; }
    //    public string nbw_super_id { get; set; }
    //    public bool is_other_id { get; set; }
    //    public string other_id { get; set; }
    //}

    //public class Cust_MobileModel
    //{
    //    public long cust_mob_id { get; set; }

    //    public long cust_id { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public string cust_mobile_no1 { get; set; }

    //    public string cust_mobile_no2 { get; set; }

    //    public string cust_mobile_no3 { get; set; }

    //    public string cust_mobile_no4 { get; set; }

    //}

    //public class Cust_PurchaseModel
    //{
    //    public long cust_purchase_id { get; set; }

    //    public long cust_id { get; set; }

    //    //public bool is_new_cust { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public string cust_status { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public string cust_mobile_no1 { get; set; }

    //    public string cust_mobile_no2 { get; set; }

    //    public string cust_mobile_no3 { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public string cust_handset_model1 { get; set; }

    //    public string cust_handset_model2 { get; set; }

    //    public string cust_handset_model3 { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public string cust_imei_no1 { get; set; }

    //    public string cust_imei_no2 { get; set; }

    //    public string cust_imei_no3 { get; set; }
    //}

    //public class Cust_AckModel
    //{

    //    public long ack_id { get; set; }

    //    public long cust_id { get; set; }

    //    public bool is_original_sign { get; set; }

    //    [Required(ErrorMessage = "This field is required field")]
    //    public Nullable<DateTime> cust_ack_date { get; set; }
    //}

    //public class Cust_RetailerUseModel
    //{
    //    public long custwithret_id { get; set; }

    //    public long cust_id { get; set; }

    //    public bool is_doc_expired { get; set; }

    //    public bool is_match_id { get; set; }

    //    public bool is_retailer_sign { get; set; }

    //    public string payment_receipt { get; set; }

    //    public long ret_id { get; set; }

    //    public string cust_retailer_date { get; set; }

    //    public string sale_person_name { get; set; }
    //}

    //#endregion


}