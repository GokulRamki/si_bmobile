using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using si_bmobile.DAL;
using si_bmobile.Models;
using si_bmobile.Utils;
namespace si_bmobile.Controllers
{
    public class bmapiController : Controller
    {
         private IcareRepository _care_repo;
        private IUtilityRepository _util_repo;

        public bmapiController()
        {
            this._care_repo = new careRepository();
            this._util_repo = new UtilityRepository();
        }

        public ActionResult Index()
        {
            return Content("");
        }

        [ValidateInput(false)]
        public ActionResult validate_login(string username, string password)
        {
            Utils.General oGen = new Utils.General();
            try
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    if (oGen.CheckHTMLdata(username.Trim()) && oGen.CheckHTMLdata(password.Trim()))
                    {
                        bool bRes = oGen.CheckContactNo(username.Trim());
                        if (bRes)
                        {
                            LoginModel obj = new LoginModel();
                            obj.MSISDN = username.Trim();
                            obj.Pwd = password.Trim();
                            int iRet = -1;
                            bool bflag = true;
                            _care_repo.Authenticate_User(obj, out iRet, out bflag);
                            if (iRet == 501 || iRet == 502)
                                return Content("success");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return Content("failure");
        }


        #region Dispose All Objects
        protected override void Dispose(bool disposing)
        {
            _care_repo.Dispose();
            _util_repo.Dispose();

            base.Dispose(disposing);
        }
        #endregion

    }
}
