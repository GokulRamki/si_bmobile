using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class EzyModel
    {
        public string ezt_user_key { get; set; }
        public string ezt_keycode { get; set; }
        public string ezt_secure_secret { get; set; }
        public string ezt_amount { get; set; }
        public string ezt_refnumber { get; set; }
        public string ezt_orderinfo { get; set; }
        public string ezt_return_url { get; set; }
        public string ezt_cancel_url { get; set; }
        public string ezt_logo_url { get; set; }
        public string ezt_txn_data1 { get; set; }
        public string ezt_txn_data2 { get; set; }
        public string ezt_txn_data3 { get; set; }

        public string paymentURL { get; set; }
    }

    public class Doku_Model
    {
        public long id { get; set; }
        public long order_id { get; set; }
        public decimal totalamount { get; set; }
        public string words { get; set; }
        public string statustype { get; set; }
        public string response_code { get; set; }
        public string approvalcode { get; set; }
        public string resultmsg { get; set; }
        public int payment_channel { get; set; }
        public int paymentcode { get; set; }
        public string session_id { get; set; }
        public string bank_issuer { get; set; }
        public string creditcard { get; set; }
        public string payment_date_time { get; set; }
        public string verifyid { get; set; }
        public int verifyscore { get; set; }
        public string verifystatus { get; set; }
    
    }

    public class DokuPostModel {
        public string MALLID { get; set; }
        public string CHAINMERCHANT { get; set; }
        public string AMOUNT { get; set; }
        public string PURCHASEAMOUNT { get; set; }
        public string TRANSIDMERCHANT { get; set; }
        public string WORDS { get; set; }
        public string REQUESTDATETIME { get; set; }
        public string CURRENCY { get; set; }
        public string PURCHASECURRENCY { get; set; }
        public string SESSIONID { get; set; }
        public string NAME { get; set; }
        public string EMAIL { get; set; }
        public string ADDITIONALDATA { get; set; }
        public string BASKET { get; set; }
        public string CUSTOMERID { get; set; }
        public string SHIPPING_ADDRESS { get; set; }
        public string SHIPPING_CITY { get; set; }
        public string SHIPPING_STATE { get; set; }
        public string SHIPPING_COUNTRY { get; set; }
        public string SHIPPING_ZIPCODE { get; set; }

        public string PAYMENTCHANNEL { get; set; }
        public string ACTIONURL { get; set; }


    }

    public class EzyOrderModel
    {
        public web_tbl_shopping_order eo { get; set; }
        public List<web_tbl_shopping_order_item> eoi { get; set; }
        public web_tbl_shopping_order_payment eop { get; set; }
    }
}