using System.Data.Entity;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace si_bmobile.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Mobile Number required!")]
        [Remote("checkMSISDN", "Care", ErrorMessage = "Invalid Mobile Number!")]
        [Display(Name = "Mobile Number")]
        public string MSISDN { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Password required!")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Pwd { get; set; }

        public bool RememberMe { get; set; }
    }
}