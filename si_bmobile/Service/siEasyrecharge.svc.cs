using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using bmdoku.bmkuRef;
namespace si_bmobile.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "siEasyrecharge" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select siEasyrecharge.svc or siEasyrecharge.svc.cs at the Solution Explorer and start debugging.
    public class siEasyrecharge : IsiEasyrecharge
    {
        private EasyRecharge _easyRecharge;

        public siEasyrecharge()
        {
            this._easyRecharge = new EasyRecharge();
        }

        public string ValidateMerchant(string merchant_id, string data)
        {
            string sRes = string.Empty;
            sRes = _easyRecharge.ValidateMerchant(merchant_id, data);
            return sRes;
        }

        public string RechargeMsisdn(string merchant_id, string data)
        {
            string sRes = string.Empty;
            sRes = _easyRecharge.RechargeMsisdn(merchant_id, data);
            return sRes;
        }
    }
}
