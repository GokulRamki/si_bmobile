using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace si_bmobile.Models
{
    public class ShoppingModel
    {
    }

    public class tbl_order_topup
    {
        [Key]
        public long id { get; set; }

        public long order_id { get; set; }

        public long product_id { get; set; }

        public decimal topup_amount { get; set; }
    }


    public class web_tbl_product_brand_conj
    {
        [Key()]
        public long id { get; set; }
        public long product_id { get; set; }
        [Required(ErrorMessage = "This Brand name is required")]
        public long brand_id { get; set; }
    }

    public class web_tbl_product_category_conj
    {
        [Key()]
        public long id { get; set; }
        public long product_id { get; set; }
        [Required(ErrorMessage = "This Category name is required")]
        public long Category_id { get; set; }
    }

    public class web_tbl_products
    {
        [Key()]
        public long Product_ID { get; set; }
        public string Product_Code { get; set; }

        [Required(ErrorMessage = "This Product sku is required")]
        public string SKU_Number { get; set; }

        [Required(ErrorMessage = "This Product name is required")]
        [RegularExpression("([a-zA-Z0-9\\-\\)\\( .&'-]+)", ErrorMessage = "Enter only alphabets and numbers")]
        public string Product_Name { get; set; }

        [Required(ErrorMessage = "This Product price is required")]
        public decimal? Product_Price { get; set; }

        public decimal? Offer_Price { get; set; }

        public string Model_No { get; set; }

        [UIHint("tinymce_jquery_full"), AllowHtml]
        public string Product_Description { get; set; }

        public bool PromoFront { get; set; }
        public string Keywords { get; set; }
        public Nullable<DateTime> Last_Update_Date { get; set; }
        public long? Last_Updated_By { get; set; }
        public Nullable<DateTime> Creation_Date { get; set; }
        public long? Created_By { get; set; }
        public Nullable<DateTime> Deleted_Date { get; set; }
        public long? Deleted_By { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsFeatured { get; set; }
        public string MetaKeyword { get; set; }
        public string MetaDescription { get; set; }
        public string PageTitle { get; set; }
        public bool is_topup { get; set; }

    }

    public class web_tbl_products_img
    {
        [Key()]
        public long Id { get; set; }
        public long Product_ID { get; set; }
        public string img_small { get; set; }
        public string img_medium { get; set; }
        public string img_large { get; set; }
    }

    public class web_tbl_related_products
    {
        [Key()]
        public long ID { get; set; }
        public long Product_ID { get; set; }
        public long Related_Product_ID { get; set; }
    }

    public class web_tbl_category
    {
        [Key()]
        public long Category_ID { get; set; }
        public string Category_Code { get; set; }
        [Required(ErrorMessage = "This Category name is required")]
        [Remote("is_category_exist", "Admin", HttpMethod = "POST", AdditionalFields = "Category_ID", ErrorMessage = "Category name already exist !")]
        public string Category_Name { get; set; }
        public string Category_Description { get; set; }
        public string Category_Img { get; set; }
        public long? Parent_ID { get; set; }
        public Nullable<DateTime> Last_Update_Date { get; set; }
        public long? Last_Updated_By { get; set; }
        public Nullable<DateTime> Creation_Date { get; set; }
        public long? Created_By { get; set; }
        public Nullable<DateTime> Deleted_Date { get; set; }
        public long? Deleted_By { get; set; }
        public bool? IsDeleted { get; set; }
        public string MetaKeyword { get; set; }
        public string MetaDescription { get; set; }
        [Required(ErrorMessage = "This Page title is required")]
        public string PageTitle { get; set; }

    }

    public class web_tbl_price_range
    {
        [Key()]
        public int ID { get; set; }
        public string Description { get; set; }
        public decimal RangeFrom { get; set; }
        public decimal RangeTo { get; set; }
    }

    public class web_tbl_brand
    {
        [Key()]
        public long Brand_ID { get; set; }

        public string Brand_Code { get; set; }

        [Required(ErrorMessage = "This Brand name is required")]
        [Remote("is_brand_exist", "Admin", HttpMethod = "POST", AdditionalFields = "Brand_ID", ErrorMessage = "Brand name already exist!")]
        public string Brand_Name { get; set; }

        [Required(ErrorMessage = "This Brand description is required")]
        public string Brand_Description { get; set; }

        public Nullable<DateTime> Last_Update_Date { get; set; }

        public long? Last_Updated_By { get; set; }

        public Nullable<DateTime> Creation_Date { get; set; }

        public long? Created_By { get; set; }

        public Nullable<DateTime> Deleted_Date { get; set; }

        public long? Deleted_By { get; set; }

        public bool? IsDeleted { get; set; }
    }

    public class web_tbl_shopping_order
    {
        [Key()]
        public long order_id { get; set; }

        public string order_number { get; set; }

        public decimal order_product_total { get; set; }

        public decimal order_freight_total { get; set; }

        public decimal order_surcharge { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal order_total { get; set; }

        public DateTime order_datetime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string order_date { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string order_time { get; set; }

        public long order_city_id { get; set; }

        public long order_store_id { get; set; }

        public bool order_is_delivery { get; set; }

        public long user_id { get; set; }

        public int payment_mode { get; set; }

        public bool delivery_status { get; set; }
    }

    public class web_tbl_shopping_order_item
    {
        [Key()]
        public long order_item_id { get; set; }
        public long product_id { get; set; }
        public string product_name { get; set; }
        public string product_sku { get; set; }
        public long product_qty { get; set; }
        public decimal product_price { get; set; }
        public long product_shipping_matrix_id { get; set; }
        public long order_id { get; set; }
    }

    public class web_tbl_shopping_order_payment
    {
        [Key()]
        public long payment_id { get; set; }

        public string payment_type { get; set; }

        public string payment_gateway { get; set; }

        public string payment_status { get; set; }

        public decimal payment_total { get; set; }

        public DateTime payment_datetime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string payment_date { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string payment_time { get; set; }

        public string payment_transaction_number { get; set; }

        public string payment_response { get; set; }

        public long order_id { get; set; }

        public string payment_receipt_no { get; set; }
    }

    public class doku_shop
    {
        [Key]
        public long id { get; set; }
        public string transidmerchant { get; set; }
        public long order_id { get; set; }
        public long customer_id { get; set; }
        public string totalamount { get; set; }
        public string words { get; set; }
        public string statustype { get; set; }
        public string response_code { get; set; }
        public string approvalcode { get; set; }
        public string resultmsg { get; set; }
        public string payment_channel { get; set; }
        public string paymentcode { get; set; }
        public string session_id { get; set; }
        public string bank_issuer { get; set; }
        public string mcn { get; set; }
        public string payment_date_time { get; set; }
        public string verifyid { get; set; }
        public string verifyscore { get; set; }
        public string verifystatus { get; set; }
        public DateTime created_on { get; set; }

    }

    public class web_tbl_shopping_shipping_matrix
    {
        [Key()]
        public long matrix_id { get; set; }
        public string matrix_code { get; set; }
        public decimal matrix_primary_cost { get; set; }
        public decimal matrix_secondary_cost { get; set; }
    }

    public class web_tbl_shopping_surcharge
    {
        [Key()]
        public long id { get; set; }
        public string postcode { get; set; }
        public string amount { get; set; }
        public string RDnumber { get; set; }
        public string town { get; set; }
    }

    public class web_tbl_shopping_user
    {
        [Key()]
        public long user_id { get; set; }
        public bool user_allows_contact { get; set; }
        public bool is_approved { get; set; }
    }

    public class web_tbl_shopping_user_contact
    {
        [Key()]
        public long contact_id { get; set; }

        public long user_id { get; set; }

        [Required(ErrorMessage = "This first name is required")]
        public string first_name { get; set; }

        [Required(ErrorMessage = "This last name is required")]
        public string last_name { get; set; }

        [Required(ErrorMessage = "This Password is required")]
        [DataType(DataType.Password)]
        public string pwd { get; set; }

        [Required(ErrorMessage = "This Email is required")]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        [Remote("IsEmailAvailable", "Shop", ErrorMessage = "Email already exist!")]
        public string email { get; set; }

        [Required(ErrorMessage = "This Phone number is required")]
        public string phone_number { get; set; }

        [Required(ErrorMessage = "This Mobile number is required")]
        public string mobile_number { get; set; }

        [Required(ErrorMessage = "This Address1 is required")]
        public string address1 { get; set; }

        public string address2 { get; set; }

        [Required(ErrorMessage = "This City is required")]
        public string city { get; set; }

        [Required(ErrorMessage = "This Postcode is required")]
        public string postcode { get; set; }

        [Required(ErrorMessage = "This Country is required")]
        public string country { get; set; }

        public string enc_id { get; set; }

        public bool IsShopping { get; set; }

    }

    public class web_tbl_shopping_user_contact_type
    {
        [Key()]
        public long contact_type_id { get; set; }
        public string contact_type_name { get; set; }
    }

    public class web_tbl_promotions
    {
        public int id { get; set; }
        [Required(ErrorMessage = "This Title is required")]
        public string title { get; set; }
        [Required(ErrorMessage = "This description is required")]
        public string description { get; set; }
        [Required(ErrorMessage = "This imageURL is required")]
        public string image_url { get; set; }
        public bool Isdeleted { get; set; }
        public bool IsActive { get; set; }
    }

    public class web_tbl_payment_mode
    {
        [Key()]
        public int ID { get; set; }
        public string Payment_Mode { get; set; }
        public bool IsActive { get; set; }
    }


    //customize model

    public class all_products_model
    {
        public web_tbl_products product { get; set; }
        public web_tbl_products_img product_img { get; set; }
        public web_tbl_brand brand { get; set; }
        public web_tbl_category category { get; set; }
        public web_tbl_product_brand_conj brand_conj { get; set; }
        public web_tbl_product_category_conj category_conj { get; set; }
    }

    public class main_category_model
    {
        public long category_id { get; set; }
        public long? parent_cat_id { get; set; }
        public string category_name { get; set; }
        public string parent_cat_name { get; set; }

    }

    public class all_user_details
    {
        public web_tbl_shopping_user user { get; set; }
        public web_tbl_shopping_user_contact user_contact { get; set; }
    }

    public class all_orders_model
    {
        public web_tbl_shopping_order order { get; set; }
        public web_tbl_shopping_order_payment order_pay { get; set; }
        public web_tbl_shopping_user_contact ordered_user { get; set; }
        public List<web_tbl_shopping_order_item> order_items { get; set; }
       // public doku_selfcare doku_selfcare { get; set; }
        public tbl_order_topup ordered_topup { get; set; }
    }

    public class orderlist_model
    {
        public long order_id { get; set; }
        public long order_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string payment_mode { get; set; }
        public string payment_status { get; set; }
        public string deliver_status { get; set; }
        public string payment_receipt_no { get; set; }
        public decimal order_total { get; set; }
        public DateTime order_datetime { get; set; }
    }



    public class order_summary_model
    {

        public web_tbl_shopping_order order { get; set; }
        public web_tbl_shopping_order_payment order_pay { get; set; }
        public web_tbl_shopping_user_contact ordered_user { get; set; }
        public List<order_item_model> order_items { get; set; }
        public List<tbl_order_topup> order_topup { get; set; }
    }

    public class order_item_model
    {

        public long order_item_id { get; set; }
        public long product_id { get; set; }
        public string product_name { get; set; }
        public string product_model { get; set; }
        public string product_img { get; set; }
        public string product_sku { get; set; }
        public long product_qty { get; set; }
        public decimal product_price { get; set; }
        public long product_shipping_matrix_id { get; set; }
        public long order_id { get; set; }
        public tbl_order_topup order_topup { get; set; }

    }

    public class ConfirmModel
    {
        [Required]
        public int PayModeId { get; set; }

        public List<web_tbl_payment_mode> PaymentModes { get; set; }

        //[Required(ErrorMessage = "you must accept terms and conditions")]
        //[Remote("CheckTerms", "Shop", ErrorMessage = "you must accept terms and conditions")]
        [MustBeTrue(ErrorMessage = "you must accept terms and conditions")]
        public bool Terms { get; set; }

        public decimal totalprice { get; set; }

        public List<cartpanel_model> Order_Items { get; set; }

        //[Required(ErrorMessage = "Please select topup amount")]
        public string topupAmt { get; set; }

    }

    public class MustBeTrueAttribute : ValidationAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
        // Implement IClientValidatable for client side Validation
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new ModelClientValidationRule[] {
            new ModelClientValidationRule { ValidationType = "checkboxtrue", ErrorMessage = this.ErrorMessage } };
        }
    }



    public class ShopProductsModel
    {
        public IEnumerable<category_model> CategoryList { get; set; }

        public IEnumerable<BrandModel> BrandList { get; set; }

        public IEnumerable<PriceModel> PriceList { get; set; }

        public IEnumerable<web_tbl_category> CategoryTreeList { get; set; }

        public IEnumerable<all_products_model> Products { get; set; }
    }

    public class CategoryModel
    {
        public web_tbl_category category { get; set; }
        public web_tbl_category parent_category { get; set; }
        public int? parent_product_count { get; set; }
        public int? product_count { get; set; }
    }


    public class category_model
    {
        public web_tbl_category parent_category { get; set; }
        public int? parent_product_count { get; set; }
        public List<sub_category_model> sub_category { get; set; }
    }

    public class sub_category_model
    {
        public web_tbl_category category { get; set; }
        public int? sub_category_count { get; set; }

    }

    public class BrandModel
    {
        public web_tbl_brand brand { get; set; }
        public int? product_count { get; set; }
    }

    public class PriceModel
    {
        public int? ID { get; set; }

        public long? Category_id { get; set; }

        public string Description { get; set; }

        public decimal? RangeFrom { get; set; }

        public decimal? RangeTo { get; set; }

        public int? product_count { get; set; }
    }


    public class temp_yplans
    {
        [Key()]
        public long Id { get; set; }

        public string Denomination_Ids { get; set; }

        public string Session_Id { get; set; }

        public Nullable<DateTime> CreatedOn { get; set; }
    }

    public class cartpanel_model
    {
        public long product_id { get; set; }

        public string product_name { get; set; }

        public string product_sku { get; set; }

        public string product_model { get; set; }

        public string product_image { get; set; }

        public decimal product_price { get; set; }

        public long product_qty { get; set; }

        public decimal sub_product_price { get; set; }

        public decimal topup_amt { get; set; }

        public long sim_qty { get; set; }

        public decimal sub_topup_price { get; set; }

        public bool is_topup { get; set; }

        public List<SelectListItem> TopupAmtlist { get; set; }

    }

    public class web_tbl_care_user
    {
        [Key]
        public long id { get; set; }

        [Required]
        public int role_id { get; set; }

        [Required]
        [Remote("UserExists", "Admin", AdditionalFields = "user_name,role_id", ErrorMessage = "Username already exists!")]
        public string user_name { get; set; }

        [Required]
        public string user_pwd { get; set; }

        [Required]
        public string first_name { get; set; }

        [Required]
        public string last_name { get; set; }

        [Required]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        public string email_id { get; set; }

        [Required]
        public string contact_number { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }

        public Nullable<DateTime> creadted_on { get; set; }

        public Nullable<DateTime> modified_on { get; set; }

    }

    // CREATE TABLE [dbo].[web_tbl_role](
    //   [id] [int] IDENTITY(1,1) NOT NULL,
    //   [role_name] [varchar](50) NULL,
    //CONSTRAINT [PK_web_

    public class web_tbl_role
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string role_name { get; set; }
    }


    //Customized User Model

    public class Care_User_Model
    {
        public long id { get; set; }

        [Required]
        public int role_id { get; set; }

        [Required]
        [Remote("UserExists", "Admin", AdditionalFields = "user_name,role_id", ErrorMessage = "Username already exists!")]
        public string user_name { get; set; }

        [Required]
        //[StringLength(16, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string user_pwd { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("user_pwd", ErrorMessage = "The password and confirm password do not match.")]
        public string confirm_pwd { get; set; }

        [Required]
        public string first_name { get; set; }

        [Required]
        public string last_name { get; set; }

        [Required]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        public string email_id { get; set; }
             
        [Required]
        public string contact_number { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }

        public List<Menu_Model> MenuList { get; set; }

        public bool is_send_login { get; set; }

    }

    public class web_tbl_fb_attempt_log
    {
        [Key]
        public long Id { get; set; }

        public string msisdn_no { get; set; }

        public string fb_voucher { get; set; }

        public DateTime created_on { get; set; }
    }

    public class TopupProductModel
    {
        public long order_id { get; set; }

        public long product_id { get; set; }

        public decimal topup_amt { get; set; }

    }

    //public class tbl_fb_voucher_excel
    //{
    //    public string voucher_no { get; set; }
    //}

    //public class tbl_purcharse_type
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    public string purchase_type { get; set; }
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }

    //}

    public class tbl_evd_plan_transaction
    {
        public long Id { get; set; }
        public long user_id { get; set; }
        public string trans_number { get; set; }
        public string msisdn { get; set; }
        public Decimal total_price { get; set; }
        public string trans_status { get; set; }
        public DateTime created_on { get; set; }
        public Nullable<DateTime> plan_purchased_on { get; set; }
        public string API_response { get; set; }
        public bool is_processed { get; set; }
    }

    public class tbl_evd_plan_details
    {
        public long Id { get; set; }
        public long transaction_id { get; set; }
        public long plan_id { get; set; }
        public string plan_name { get; set; }
        public decimal plan_price { get; set; }
    }

    public class tbl_web_plan_purchase_trans
    {
        [Key]
        public long id { get; set; }

        public string trans_number { get; set; }

        public string msisdn { get; set; }

        public long plan_id { get; set; }

        public string plan_name { get; set; }

        public decimal plan_price { get; set; }

        public decimal curr_bal { get; set; }

        public long user_id { get; set; }

        public string trans_status { get; set; }

        public string API_response { get; set; }

        public DateTime created_on { get; set; }

        public Nullable<DateTime> plan_purchased_on { get; set; }

        public bool is_processed { get; set; }

        public int? type_id { get; set; }
    }
}