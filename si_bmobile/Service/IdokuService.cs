using si_bmobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace bemobile.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IdokuService" in both code and config file together.
    [ServiceContract]
    public interface IdokuService
    {

        [OperationContract]
        long save_selfcare_order(Payment_EntModel obj_sc_order);

        [OperationContract]
        long save_doku_order_payment(Payment_EntModel obj_order_pay, long order_id, int site_id);

        [OperationContract]
        string get_doku_order_transactions();

        [OperationContract]
        string get_doku_order_byid(long order_id);

        [OperationContract]
        long save_temp_order(string jstemp_order);

        [OperationContract]
        string get_temp_order_byid(string session_id);

    }
}
