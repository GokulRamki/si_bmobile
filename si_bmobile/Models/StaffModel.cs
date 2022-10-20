using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace si_bmobile.Models
{
    public class StaffModel
    {
    }

    public class bm_staffs_trans
    {
        public long Id { get; set; }
        public string msisdn_number { get; set; }
        public decimal amount { get; set; }
        public string trans_desc { get; set; }
        public DateTime trans_date { get; set; }
        public string invoice_number { get; set; }
        public string email { get; set; }
        public string ip_address { get; set; }
        public bool is_recharged { get; set; }
    }

    public class bm_staffs_deduction_trans
    {
        public long Id { get; set; }
        public string msisdn_number { get; set; }
        public decimal amount { get; set; }
        public string trans_desc { get; set; }
        public DateTime trans_date { get; set; }
        public string invoice_number { get; set; }
        public string email { get; set; }
        public bool is_deducted { get; set; }
    }


    public class bm_staffs_topup
    {
        [Key]
        public long Id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string msisdn_number { get; set; }

        public decimal amount { get; set; }

        public string invoice { get; set; }

        public string email { get; set; }

        public string reason { get; set; }

        public DateTime created_on { get; set; }

        public Nullable<DateTime> recharged_on { get; set; }

        public bool is_recharged { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }
    }

    public class StaffsTopupModel
    {
        public string first_name { get; set; }

        public string last_name { get; set; }

        public string msisdn_number { get; set; }

        public Nullable<decimal> amount { get; set; }

        public string invoice { get; set; }

        public string email { get; set; }

        public DateTime created_on { get; set; }

        public bool is_recharged { get; set; }

        public bool is_error { get; set; }

        public string description { get; set; }
    }

    public class recharge_model
    {
        public string username { get; set; }
        public string password { get; set; }
        public string keycode { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }
    }

    public class validate_output_model
    {
        public int resultcode { get; set; }
        public string reference { get; set; }
    }

    public class recharge_msisdn_model
    {
        public string reference { get; set; }
        public string msisdn { get; set; }
        public string amount { get; set; }
        public string invoice { get; set; }
        public string masterMsisdn { get; set; }
    }

    public class recharge_OP_model
    {
        public long resultCode { get; set; }
        public string rechargeAmount { get; set; }
        public string newBalance { get; set; }
        public string reference { get; set; }
        public string msidnNumber { get; set; }
    }
    public class tbl_transaction_fees
    {
        public int Id { get; set; }
        public int platform_id { get; set; }
        public int acct_id { get; set; }
        public string fee_for { get; set; }
        public string type { get; set; }
        public long min { get; set; }
        public long max { get; set; }
        public int percentage { get; set; }
        public int fee { get; set; }
    }
}