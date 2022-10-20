using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace si_bmobile.Service1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBundlePlan" in both code and config file together.
    [ServiceContract]
    public interface IBundlePlan : IDisposable
    {
        //[OperationContract]
        //string GetBunldePlans(string sUname, string sPwd);

        [OperationContract]
        string CreateBunldePlan(string sUname, string sPwd, string sData);

        [OperationContract]
        string DeleteBunldePlan(string sUname, string sPwd, string sData);

        [OperationContract]
        string getbundleplan_details(string sUname, string sPwd, string sData);

        [OperationContract]
        string EditBunldePlan(string sUname, string sPwd, string sData);
    }
}
