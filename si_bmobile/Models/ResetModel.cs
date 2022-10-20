using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace si_bmobile.Models
{
    public class ResetModel
    {
        public string UserId { get; set; }

        public string Enc_Id { get; set; }

        [Required(ErrorMessage="Please enter correct old password!")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [RegularExpression(@"(\S)+", ErrorMessage = "Space not allowed between characters!")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword",ErrorMessage="Confirm Password does not match!")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePwdModel
    {
        public long UserId { get; set; }

        [Required(ErrorMessage = "Please enter correct old password!")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirm does not match!")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotModel
    {

        [Required]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{1,6}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail address")]
        public string Email { get; set; }
    
    }

}