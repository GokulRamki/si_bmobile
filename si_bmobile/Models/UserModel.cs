using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace bemobile.Models
{
    public class UserModel
    {

        public long contact_id { get; set; }

        public long user_id { get; set; }

        [Required(ErrorMessage = "This first name is required")]
        public string first_name { get; set; }

        [Required(ErrorMessage = "This Last name is required")]
        public string last_name { get; set; }

        [Required(ErrorMessage = "This Password is required")]
        [DataType(DataType.Password)]
        public string pwd { get; set; }

        [Required(ErrorMessage = "This Email is required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        [Remote("IsEmailAvailable","Shop",ErrorMessage="Email already exist!")]
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

    
}