using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class CreditMeModel
    {
      public  string username { get; set; }

      public string password { get; set; }

      public string keycode { get; set; }

      public string fromMsisdn { get; set; }

      public string toMsisdn { get; set; }

      public string amount { get; set; }
    }

    public class CreditMeOPModel
    {
        public string Rescode { get; set; }
    }
}