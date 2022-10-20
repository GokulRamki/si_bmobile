using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace si_bmobile.Models
{
    public class DokuCareModel
    {
        public doku_selfcare doku { get; set; }
        public web_tbl_selfcare_order order { get; set; }
        public List<web_tbl_selfcare_order_item> orderitems { get; set; }
        public web_tbl_selfcare_payment orderpayment { get; set; }

        public double amt { get; set; }
        public double bsp_amt { get; set; }
        public double dep_amt { get; set; }

    }

    public class DokuCareModel_New
    {
        public doku_selfcare doku { get; set; }
        public web_tbl_selfcare_order order { get; set; }
        public web_tbl_selfcare_order_item orderitems { get; set; }
        public web_tbl_selfcare_payment orderpayment { get; set; }

        public string perc { get; set; }

    }

    public class temp_orders_model
    {
        public long Id { get; set; }
        public long? Order_Id { get; set; }
        public int? SiteId { get; set; }
        public string Session_Id { get; set; }
        public Nullable<DateTime> CreatedOn { get; set; }
    }
    public class DokuShopModel
    {
        public doku_selfcare sdoku { get; set; }
        public web_tbl_shopping_order sorder { get; set; }
        public List<web_tbl_shopping_order_item> sorderitems { get; set; }
        public web_tbl_shopping_order_payment sorderpayment { get; set; }
        public web_tbl_shopping_user_contact suser { get; set; }
        public List<tbl_order_topup> sordertopup { get; set; }
    }

    public class DokuRDModel
    {
        public DokuCareModel dkuCare { get; set; }
        public DokuShopModel dkuShop { get; set; }
    }


    public class doku_selfcare
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
        public int site_id { get; set; }
        public DateTime created_on { get; set; }

        public string request_time_stamp { get; set; }
        public string request_signature { get; set; }
        public string transaction_type { get; set; }
        public string transaction_state { get; set; }
        public string transaction_id { get; set; }
        public string request_id { get; set; }
        public string requested_amount { get; set; }
        public string completion_time_stamp { get; set; }
        public string status_code_n { get; set; }
        public string status_description_n { get; set; }
        public string status_severity_n { get; set; }
        public string authorization_code { get; set; }
        public string masked_account_number { get; set; }
        public string token_id { get; set; }
        public string ip_address { get; set; }
    }

    public class web_tbl_selfcare_order
    {
        [Key]
        public long order_id { get; set; }
        public string order_number { get; set; }
        public decimal order_product_total { get; set; }
        public decimal order_freight_total { get; set; }
        public decimal order_surcharge { get; set; }
        public DateTime order_datetime { get; set; }
        public long user_id { get; set; }
        public string cust_fname { get; set; }
        public string cust_lname { get; set; }
        public string cust_email { get; set; }
        public string cust_mobile_number { get; set; }
        public string purchase_msisdn { get; set; }
        public int payment_mode { get; set; }
    }

    public class web_tbl_selfcare_order_item
    {
        [Key]
        public long order_item_id { get; set; }
        public long product_id { get; set; }
        public string product_name { get; set; }
        public long product_qty { get; set; }
        public decimal product_price { get; set; }
        public string purchase_desc { get; set; }
        public long product_shipping_matrix_id { get; set; }
        public long order_id { get; set; }
    }

    public class web_tbl_selfcare_payment
    {
        [Key]
        public long payment_id { get; set; }
        public string payment_type { get; set; }
        public string payment_gateway { get; set; }
        public string payment_status { get; set; }
        public decimal payment_total { get; set; }
        public DateTime payment_datetime { get; set; }
        public string payment_transaction_number { get; set; }
        public string payment_response { get; set; }
        public long order_id { get; set; }
        public string payment_receipt_no { get; set; }
    }

    public class web_tbl_fb_promotions
    {
        [Key]
        public long Id { get; set; }
        public string serial_number { get; set; }
        public string pin_number { get; set; }
        public string fb_vouchers { get; set; }
        public decimal amount { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
        public DateTime created_on { get; set; }
    }

    public class temp_orders
    {
        [Key]
        public long Id { get; set; }
        public long? Order_Id { get; set; }
        public int? SiteId { get; set; }
        public string Session_Id { get; set; }
        public Nullable<DateTime> CreatedOn { get; set; }
    }

    public class tbl_email_template
    {
        [Key]
        public int Id { get; set; }
        public string template_content { get; set; }
        public string site_desc { get; set; }
        public bool IsActive { get; set; }
    }

    public class Payment_Transaction_model
    {

        #region old doku    
        public string transidmerchant { get; set; }
        public long order_id { get; set; }
        public long customer_id { get; set; }
        public string totalamount { get; set; }
        public string resultmsg { get; set; }
        #endregion

        #region WC    
        public string request_id { get; set; }
        public string requested_amount { get; set; }
        public int site_id { get; set; }
        public string transaction_state { get; set; }
        public string authorization_code { get; set; }
        #endregion

        #region order
        public long order_number { get; set; }
        public decimal order_product_total { get; set; }
        public decimal order_freight_total { get; set; }
        public decimal order_surcharge { get; set; }
        public DateTime order_datetime { get; set; }
        public long user_id { get; set; }
        public string cust_fname { get; set; }
        public string cust_lname { get; set; }
        public string cust_email { get; set; }
        public string cust_mobile_number { get; set; }
        public string purchase_msisdn { get; set; }
        public int payment_mode { get; set; }
        #endregion

        #region order item
        public long product_id { get; set; }
        public string product_name { get; set; }
        public long product_qty { get; set; }
        public decimal product_price { get; set; }
        public string purchase_desc { get; set; }
        public long product_shipping_matrix_id { get; set; }
        #endregion

        #region order payment
        public string payment_type { get; set; }
        public string payment_gateway { get; set; }
        public string payment_status { get; set; }
        public decimal payment_total { get; set; }
        public DateTime payment_datetime { get; set; }
        public string payment_transaction_number { get; set; }
        public string payment_receipt_no { get; set; }
        #endregion


    }
}