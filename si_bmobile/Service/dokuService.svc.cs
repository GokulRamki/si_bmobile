using bemobile.DAL;
using bemobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace bemobile.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "dokuService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select dokuService.svc or dokuService.svc.cs at the Solution Explorer and start debugging.
    public class dokuService : IdokuService
    {
        IdokuRepository _doku_repo;
        UnitOfWork _uow;
        public dokuService()
        {
            this._doku_repo = new dokuRepository();
            this._uow = new UnitOfWork();
        }

        public long save_selfcare_order(Payment_EntModel obj_sc_order)
        {
            long iRet = 0;

            if (obj_sc_order != null)
                iRet = _doku_repo.SaveSelfcareOrder(obj_sc_order);

            return iRet;
        }

        public long save_doku_order_payment(Payment_EntModel obj_order_pay, long order_id, int site_id)
        {
            long iRet = 0;
            if (obj_order_pay != null)
                iRet = _doku_repo.SaveDokuSelfcareTrans(obj_order_pay, order_id, site_id);

            return iRet;
        }

        public string get_doku_order_transactions()
        {
            string sRet = "";

            var doku_trans = _doku_repo.GetDokuOrderTransaction();
            if (doku_trans != null && doku_trans.Count > 0)
                sRet = JsonConvert.SerializeObject(doku_trans);

            return sRet;
        }

        public string get_doku_order_byid(long order_id)
        {
            string sRet = "";

            var doku_trans = _doku_repo.GetDokuOrderCUG(order_id);
            if (doku_trans != null)
                sRet = JsonConvert.SerializeObject(doku_trans);

            return sRet;
        }

        public long save_temp_order(string jstemp_order)
        {
            long iRet = 0;

            if (!string.IsNullOrEmpty(jstemp_order))
            {
                var obj_order = JsonConvert.DeserializeObject<temp_orders>(jstemp_order);

                if (obj_order != null)
                    _uow.temp_orders_repo.Insert(obj_order);
                    _uow.Save();
                    if (obj_order.Id > 0)
                        iRet = obj_order.Id;
            }
            return iRet;
        }

        public string get_temp_order_byid(string session_id)
        {
            string sRet = "";

            var temp_order = _uow.temp_orders_repo.Get(filter: T => T.Session_Id == session_id).LastOrDefault();
            if (temp_order != null)
                sRet = JsonConvert.SerializeObject(temp_order);

            return sRet;
        }



    }
}
