using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using si_bmobile.Models;
using si_bmobile.DAL;
using si_bmobile.Utils;
using System.Configuration;
using Newtonsoft.Json;

namespace si_bmobile.Service1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BundlePlan" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BundlePlan.svc or BundlePlan.svc.cs at the Solution Explorer and start debugging.
    public class BundlePlan : IBundlePlan, IDisposable
    {
        private string sNewEncKey;
        private IPlanRepository _plan_repo;
        private IUtilityRepository _util_repo;
        bemobileselfcareEntities _dbselfcare;
        public BundlePlan()
        {
            this.sNewEncKey = ConfigurationManager.AppSettings["NewencKey"];
            this._plan_repo = new PlanRepository();
            this._dbselfcare = new bemobileselfcareEntities();
            this._util_repo = new UtilityRepository();
        }

        //public string GetBunldePlans(string sUname, string sPwd)
        //{
        //    GetBunldePlansModel obj = new GetBunldePlansModel();
        //    obj.resultCode = "-555";

        //    try
        //    {
        //        bool Res = AuthenticateService(sUname, sPwd);
        //        if (Res)
        //        {
        //            obj.bundles = _plan_repo.GetBunldePlans();
        //            obj.resultCode = "0";
        //        }
        //        else
        //            obj.resultCode = "-222";
        //    }
        //    catch (System.Exception ex)
        //    {
        //        _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
        //    }

        //    string data = JsonConvert.SerializeObject(obj);

        //    return data;
        //}

        public string CreateBunldePlan(string sUname, string sPwd, string sData)
        {
            ReturnModel obj = new ReturnModel();
            obj.resultCode = "-555";

            try
            {
                bool Res = AuthenticateService(sUname, sPwd);
                if (Res)
                {
                    if (sData != null)
                    {
                                obj.resultCode = _plan_repo.CreateBundlePlanCCtpService(sData);
                       
                    }
                    else
                        obj.resultCode = "-222";
                }
            }
            catch (System.Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            string data = JsonConvert.SerializeObject(obj);

            return data;
        }

        public string EditBunldePlan(string sUname, string sPwd, string sData)
        {
            ReturnModel obj = new ReturnModel();
            obj.resultCode = "-555";

            try
            {
                bool Res = AuthenticateService(sUname, sPwd);
                if (Res)
                {
                    if (sData != null)
                    {
                        obj.resultCode = _plan_repo.UpdateBunldePlanCCtpService(sData);
                    }
                    else
                        obj.resultCode = "-222";
                }
            }
            catch (System.Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            string data = JsonConvert.SerializeObject(obj);

            return data;
        }

        public string DeleteBunldePlan(string sUname, string sPwd, string sData)
        {
            ReturnModel obj = new ReturnModel();
            obj.resultCode = "-555";

            try
            {
                bool Res = AuthenticateService(sUname, sPwd);
                if (Res)
                {
                    if (sData != null)
                    {
                        obj.resultCode = _plan_repo.DeleteBunldePlanCCtpService(sData);
                    }
                }
                else
                    obj.resultCode = "-222";
            }
            catch (System.Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            string data = JsonConvert.SerializeObject(obj);

            return data;
        }

        public string getbundleplan_details(string sUname, string sPwd, string sData)
        {
            GetBunldePlansModel obj = new GetBunldePlansModel();
            obj.resultCode = "-555";

            try
            {
                bool Res = AuthenticateService(sUname, sPwd);
                if (Res)
                {

                    if (sData != null)
                    {
                        long bid = JsonConvert.DeserializeObject<long>(sData);

                        if (bid > 0)
                        {
                            obj = _plan_repo.getBundlePlan(bid);
                        }
                    }
                }
                else
                    obj.resultCode = "-222";
            }
            catch (System.Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            string data = JsonConvert.SerializeObject(obj);

            return data;
        }

        #region Service Authentication

        private bool AuthenticateService(string sUname, string sPwd)
        {
            bool bResult = false;
            try
            {
                string _Uname = _util_repo.AES_JENC(ConfigurationManager.AppSettings["svc_uid"]);
                string _Pwd = _util_repo.AES_JENC(ConfigurationManager.AppSettings["svc_pwd"]);
                if (_Uname == sUname && _Pwd == sPwd)
                    bResult = true;
            }
            catch (System.Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return bResult;
        }

        #endregion

        #region Dispose Objects

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _plan_repo.Dispose();
                _util_repo.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
