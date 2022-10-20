using System;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace si_bmobile.Models
{
    public class RegistrationModel
    {
        public long UserId { get; set; }

        public string Username { get; set; }

        [Required(ErrorMessage = "Email required!")]
        [Display(Name="Email")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }

        public string Password { get; set; }

        public string dPassword { get; set; }

        [Required(ErrorMessage = "Firstname required!")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        private string _MiddleName;
        public string MiddleName { get { return HttpUtility.HtmlEncode(_MiddleName); } set { _MiddleName = value; } }

        [Required(ErrorMessage = "Lastname required!")]
        public string LastName { get; set; }

        public string Company { get; set; }

        [Required(ErrorMessage = "Address required!")]
        [Display(Name="Address 1")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        //[Required]
        //[Remote("checkcontactno", "Care", ErrorMessage = "Invalid Broadband Number!")]
        //[Display(Name="Contact Number")]
        //public string ContactNo { get; set; }

        [Required(ErrorMessage="Mobile Number required!")]
        [Remote("checkbroadbandno", "Care", ErrorMessage = "Invalid Mobile Number!")]
        [Display(Name = "Mobile Number")]
        public string BroadBandNo { get; set; }

        public string FormattedBroadBandNo { get; set; }

        public string paidType { get; set; }

        public string account_number { get; set; }

        public string _rd_action { get; set; }

        public List<FilesModel> _ppbills { get; set; }

        [Required(ErrorMessage = "Mother’s maiden name required!")]
        public string mother_maiden_name { get; set; }

        [Required(ErrorMessage = "Favourite colour required!")]
        public string favourite_color { get; set; }

        [Required(ErrorMessage = "Pets name required!")]
        public string pets_name { get; set; }
    }

    public class ShopModel
    {
        [Required(ErrorMessage = "Mobile required!")]
        public string MSISDN { get; set; }
    }

    public class VerificationModel
    {
        [Required(ErrorMessage = "Verification Code required!")]
        public string VerificationCode { get; set; }
    }

    public class OPModel
    {
        public string result_code { get; set; }

        public string reference { get; set; }

        public string reURL { get; set; }
    }
}