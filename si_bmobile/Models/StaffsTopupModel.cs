using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace bemobile.Models
{
 
    //public class tags_val
    //{
    //    [Key]
    //    public long Id { get; set; }

    //    public string v_key { get; set; }

    //    public string t_value { get; set; }
    //}

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
}
