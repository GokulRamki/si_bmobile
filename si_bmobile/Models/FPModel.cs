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
    public class FPModel
    {
        [Required]
        [Display(Name = "Email")]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress!")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }
    }

    public class TPModel
    {
        public string Id { get; set; }

        public string encryptedid { get; set; }

        [RegularExpression(@"(\S)+", ErrorMessage = "Space not allowed between characters!")]
        [Required(ErrorMessage = "Temporary password required!")]
        public string TempPwd { get; set; }

        [Required(ErrorMessage = "New password required!")]
        [RegularExpression(@"(\S)+", ErrorMessage = "Space not allowed between characters!")]
        [DataType(DataType.Password)]
        public string NewPwd { get; set; }

        [Required(ErrorMessage = "Confirm password required!")]
        [DataType(DataType.Password)]
        [Compare("NewPwd", ErrorMessage = "Confirm password does not match!")]
        public string ConfirmPwd { get; set; }
    }
}