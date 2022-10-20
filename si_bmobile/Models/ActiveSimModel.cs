using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class ActiveSimModel
    {
        public string sim_number { get; set; }

        public string msidn_number { get; set; }

        public string puk_code { get; set; }

        public string ref_number { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string email { get; set; }

        public string address1 { get; set; }

        public string address2 { get; set; }

        public string driving_lic { get; set; }

        public string status { get; set; }
    }   
}