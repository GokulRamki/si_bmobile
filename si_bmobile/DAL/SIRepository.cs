using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using bemobile.Utils;
using Newtonsoft.Json;
using bemobile.Models;
using System.Configuration;

namespace bemobile.DAL
{
    public class SIRepository : ISIRepository, IDisposable
    {
        private UnitOfWork UOW;
        
        private IUtilityRepository _util_repo;
        private string si_uname;
        private string si_pwd;
        private string si_keycode;
        private string si_merch_id;
        public SIRepository()
        {
            
            this._util_repo = new UtilityRepository();
            this.si_uname = ConfigurationManager.AppSettings["si_uname"].ToString();
            this.si_pwd = ConfigurationManager.AppSettings["si_pwd"].ToString();
            this.si_keycode = ConfigurationManager.AppSettings["si_keycode"].ToString();
            this.si_merch_id = ConfigurationManager.AppSettings["si_merch_id"].ToString();
        }

        #region For Recharge APIs for bmobile SI

        /// <summary>
        /// Validate Merchant using the config details
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <returns>json format string</returns>
        public string bmsi_validate_merchant(string msisdn, string amount)
        {
            string sRes = string.Empty;
            RechargeModel obj_regcharge = new RechargeModel();
            obj_regcharge.keycode = si_keycode;
            obj_regcharge.username = si_uname;
            obj_regcharge.password = si_pwd;
            obj_regcharge.msisdn = msisdn;
            obj_regcharge.amount = amount;

            var data = JsonConvert.SerializeObject(obj_regcharge, Formatting.Indented);

            var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.ValidateMerchant(si_merch_id, _util_repo.AES_JENC(data)));

            if (!string.IsNullOrEmpty(resultmerchant))
                sRes = resultmerchant;

            return sRes;
        }

        /// <summary>
        /// Recharge MSISDN w.r.t amount
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <param name="data"></param>
        /// <returns>json format string</returns>
        public string bmsi_recharge_msisdn(string msisdn, string amount, string data)
        {
            string sRes = string.Empty;

            if (data != null)
            {
                var opRes = JsonConvert.DeserializeObject<ValidateOutputModel>(data);

                if (opRes.resultcode == 0)
                {
                    RechargeMsisdnModel obj_recharge = new RechargeMsisdnModel();
                    obj_recharge.amount = amount;
                    obj_recharge.msisdn = msisdn;
                    obj_recharge.reference = opRes.reference;

                    string sRec_res = JsonConvert.SerializeObject(obj_recharge, Formatting.Indented);
                    string encry_data = _util_repo.AES_JENC(sRec_res);

                    var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.RechargeMsisdn(si_merch_id, encry_data));

                    if (!string.IsNullOrEmpty(resultmerchant))
                        sRes = resultmerchant;
                }
                else
                {
                    sRes = data;
                }
            }

            return sRes;
        }
        #endregion



        #region Dispose Objects

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    UOW.Dispose();
                    _easyRecharge.Dispose();
                    _util_repo.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}