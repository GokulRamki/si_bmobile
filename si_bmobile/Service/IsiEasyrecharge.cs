using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace si_bmobile.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IsiEasyrecharge" in both code and config file together.
    [ServiceContract]
    public interface IsiEasyrecharge
    {
        [OperationContract]
        string ValidateMerchant(string merchant_id, string data);

        [OperationContract]
        string RechargeMsisdn(string merchant_id, string data);
    }
}
