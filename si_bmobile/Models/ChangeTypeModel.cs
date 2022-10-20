using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace si_bmobile.Models
{
    public class ChangeTypeModel
    {
        [Required(ErrorMessage = "Mobile Number required!")]
        public string MobileNo { get; set; }

        public string FormattedMobileNo { get; set; }
        
        public string _planPrice { get; set; }
        
        public double balance { get; set; }
        
        public string Email { get; set; }
        
        public long PlanId { get; set; }

        public double dPlanPrice { get; set; }

        public string planName { get; set; }

        public string Description { get; set; }

        public long UserId { get; set; }

        public string BundleType { get; set; }

    }
}