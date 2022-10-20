using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace si_bmobile.bkService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "Idownloadbkup" in both code and config file together.
    [ServiceContract]
    public interface Idownloadbkup
    {
        [OperationContract]
        bool zipprocess(string id);
    }
}
