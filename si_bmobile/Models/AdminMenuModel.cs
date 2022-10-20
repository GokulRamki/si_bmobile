using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class AdminMenuModel
    {
        private string _bundleActive = "";
        public string bundeActive
        {
            get { return _bundleActive; }
            set { _bundleActive = value; }
        }

        private string _promoActive = "";
        public string promoActive
        {
            get { return _promoActive; }
            set { _promoActive = value; }
        }

        private string _bypActive = "";
        public string bypActive
        {
            get { return _bypActive; }
            set { _bypActive = value; }
        }

        private string _reportActive = "";
        public string reportActive
        {
            get { return _reportActive; }
            set { _reportActive = value; }
        }
    }
}