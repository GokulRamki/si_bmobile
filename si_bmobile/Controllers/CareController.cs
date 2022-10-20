using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using si_bmobile.Models;
using si_bmobile.DAL;
using si_bmobile.Utils;
using System.Web.Routing;
using si_bmobile.Filters;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Configuration;
using bmdoku;
using MvcPaging;
using si_bmobile.GetPowerRef;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using si_bmobile.dokuRef;
using si_bmobile.esipayRef;
using System.Web.Security;

namespace si_bmobile.Controllers
{
    public class CareController : Controller
    {
        //
        // GET: /Care/
        private IcareRepository _care_repo;
        private IUtilityRepository _util_repo;
        private IShopRepository _shop_repo;
        private IPlanRepository _plan_repo;
        private ISimRepository _sim_repo;
        private string _Hide;
        private string _Show;
        private UnitOfWork UOW;
        bmtestshopDBEntities _dbctxt = new bmtestshopDBEntities();
        // private IdokuRepository _doku_repo;
        private int defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pgSize"]);
        private IdokuServiceClient _doku_client;

        private int cookies_expiry_date = Convert.ToInt32(ConfigurationManager.AppSettings["Days_remember_me"]);

        #region Params for GetPower
        private string gp_svc_uid = ConfigurationManager.AppSettings["gp_svc_uid"];
        private string gp_svc_pwd = ConfigurationManager.AppSettings["gp_svc_pwd"];
        private string gp_java_uid = ConfigurationManager.AppSettings["gp_java_uid"];
        private string gp_java_pwd = ConfigurationManager.AppSettings["gp_java_pwd"];
        private string gp_merc_id = ConfigurationManager.AppSettings["gp_merc_id"];
        private string gp_merc_uname = ConfigurationManager.AppSettings["gp_merc_uname"];
        private string gp_merc_pwd = ConfigurationManager.AppSettings["gp_merc_pwd"];
        private string gp_merc_keycode = ConfigurationManager.AppSettings["gp_merc_keycode"];
        private string device_setting_url = ConfigurationManager.AppSettings["device_setting_url"];
        private GetPowerClient _getpower;
        #endregion

        private bmdoku.bmkuRef.EasyRecharge proxy = new bmdoku.bmkuRef.EasyRecharge();
        private IesipayServiceClient _esipay;
        public CareController()
        {
            this._sim_repo = new SimRepository();
            this._care_repo = new careRepository();
            this._util_repo = new UtilityRepository();
            this._shop_repo = new ShopRepository();
            this._plan_repo = new PlanRepository();
            this._Hide = "display:none;";
            this._Show = "display:block;";
            this.UOW = new UnitOfWork();
           // this._doku_repo = new dokuRepository();
            this._getpower = new GetPowerClient();
            this._doku_client = new IdokuServiceClient();
            this._esipay = new IesipayServiceClient();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login", "Care");
        }

        public ActionResult Test()
        {

            return View();
        }

        public ActionResult TermsConditions()
        {
            return View();
        }



        #region Register
        public ActionResult Register(string id)
        {
            try
            {
                if (Session["userReg"] != null)
                {
                    RegistrationModel oRM = new RegistrationModel();
                    oRM = (RegistrationModel)Session["userReg"];

                    return View(oRM);
                }

                if (id != null)
                    Session["sess_yp"] = id;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpPost]
        [ValidateInput(true)]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegistrationModel oRM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Session["userReg"] = oRM;

                    return RedirectToAction("Confirm", "Care");
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult Confirm()
        {
            RegistrationModel oRM = new RegistrationModel();
            try
            {
                if (Session["userReg"] == null)
                    return RedirectToAction("Register");
                else
                {
                    oRM = (RegistrationModel)Session["userReg"];
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oRM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(RegistrationModel oRM)
        {
            string sMsg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    int iRet = -1;
                    bool bflag = false;
                    _care_repo.Register_User(oRM, out iRet, out bflag);
                    if (iRet == 0)
                    {
                        //bool IsShopEmail = _user_repo.IsEmailAvailable(oRM.Email);
                        //if (IsShopEmail == false)
                        //{
                        //    _user_repo.RegisterShopping(oRM);
                        //}
                        return RedirectToAction("Thankyou", "Care", new { id = "Register" });
                    }
                    else if (iRet == 106)
                        sMsg = "Already Registered  Account, Please click on ‘Forgot Password’ if you have forgotten your details.";
                    else if (iRet == 107)
                        sMsg = "Already Registered  Account, Please click on ‘Forgot Password’ if you have forgotten your details.";
                    else if (iRet == 104)
                        sMsg = "Registration Failed!";
                    else if (iRet == 111 || iRet == 112 || iRet == 113)
                        sMsg = "Invalid Mobile Number!";
                    else if (iRet == 113)
                        sMsg = "Not Active";
                    else if (iRet == 114)
                        sMsg = "SMS Error";
                    else if (iRet == 116)
                        sMsg = "This service is not available for Postpaid Customers";
                    else
                        sMsg = "Expired";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oRM);
        }

        [SessionExpireFilter]
        public ActionResult EditPro()
        {
            RegistrationModel oRM = new RegistrationModel();
            try
            {
                if (Session["subscriber"] == null && Session["postpdsubscriber"] == null)
                    return RedirectToAction("Login");
                else
                {


                    AccountModel oAM = new AccountModel();
                    string sEmailID = Session["subscriber"].ToString();
                    oAM = _care_repo.GetMultipleSubscribers(sEmailID);

                    if (oAM != null)
                    {
                        oRM = oAM.Reg;
                        if (oAM.Reg.paidType == "502")
                        {
                            //ViewBag.posType = "502";
                            //oRM._rd_action = "Postpaid";
                        }
                        else if (oAM.Reg.paidType == "501")
                        {
                            ViewBag.preType = "501";
                            oRM._rd_action = "Prepaid";
                        }

                        if (oAM.Subr.Count > 1)
                        {
                            ViewBag.preType = "501";
                            oRM._rd_action = "Prepaid";
                        }
                        oRM.paidType = oAM.Reg.paidType;

                    }


                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oRM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult EditPro(RegistrationModel oRM)
        {
            string sMsg = "";
            try
            {
                if (Session["subscriber"] == null && Session["postpdsubscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    if (ModelState.IsValid)
                    {

                        if (oRM.paidType == "502")
                        {
                            //ViewBag.posType = "502";
                            //oRM._rd_action = "Postpaid";
                        }
                        else if (oRM.paidType == "501")
                        {
                            ViewBag.preType = "501";
                            oRM._rd_action = "Prepaid";
                        }

                        int iRet = 0;
                        bool bflag = false;
                        _care_repo.Update_User(oRM, out iRet, out bflag);
                        if (iRet == 0)
                        {
                            //Session["subscriber"] = oRM.Email;
                            sMsg = "Profile Updated Successfully";
                        }
                        else if (iRet == 108)
                            sMsg = "Email Already Exists!";
                        else
                            sMsg = "Failed to update!";
                    }
                }
                ViewBag.Message = sMsg;
                ViewBag.rdAction = oRM._rd_action;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }
        #endregion

        #region Thankyou
        public ActionResult Thankyou(string id)
        {
            if (id != "")
            {
                if (Session["sess_yp"] != null)
                    id = Session["sess_yp"].ToString();
                ViewBag.Message = id;
                //Session.Clear();
            }
            return View();
        }
        #endregion



        #region Vodafone Login
        public ActionResult VodafoneLogin(string id)
        {
            LoginModel oLM = new LoginModel();
            if (id != null)
            {
                Session["sess_yp"] = id;
            }
            return View(oLM);
        }

        [HttpPost]
        //[ValidateInput(true)]
        [ValidateAntiForgeryToken]
        public ActionResult VodafoneLogin(LoginModel oLM)
        {
            string sMsg = "";
            // PaidTypeModel oPM = new PaidTypeModel();
            try
            {
                if (ModelState.IsValid)
                {
                    int iRet = 0;
                    bool bflag = false;
                    _care_repo.Authenticate_User(oLM, out iRet, out bflag);


                    if (iRet == 102)
                        sMsg = "Invalid Request!";
                    else if (iRet == 101)
                    {

                        Session["subscriber"] = "677" + oLM.MSISDN;
                        return RedirectToAction("ResetPassword");
                    }
                    else if (iRet == 503)
                    {

                        return RedirectToAction("ResetPassword");
                    }
                    else if (iRet == 105)
                        sMsg = "Login Failed!";
                    else if (iRet == 501)
                    {

                        Session["subscriber"] = "677" + oLM.MSISDN.Trim();
                        return RedirectToAction("PlanPricing");
                    }
                    else if (iRet == 502)
                    {

                        Session["subscriber"] = "677" + oLM.MSISDN;
                        if (Session["sess_yp"] != null)
                            return RedirectToAction("PlanPricing");
                        //else
                        //    return RedirectToAction("Postpaid");

                    }
                    else if (iRet == 505)
                    {
                        return RedirectToAction("LockAccount");
                    }
                    else
                        sMsg = "Invalid Request!";

                }

                ViewBag.Message = sMsg;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }
        #endregion



        #region Login
        public ActionResult Login(string id)
        {
            //if (Session["ShoppingUserID"] != null)
            //{
            //    return RedirectToAction("MSISDN","Shop");
            //}           
            LoginModel oLM = new LoginModel();
            if (id != null)
            {
                Session["sess_yp"] = id;
            }

            HttpCookie cookie = Request.Cookies[FormsAuthentication.FormsCookieName];


            if (cookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                oLM.MSISDN = ticket.Name.Split('|')[0];
                oLM.RememberMe = ticket.IsPersistent;
                oLM.Pwd = ticket.Name.Split('|')[1];
            }
            if(TempData["Res"]!=null)
            {
                oLM.MSISDN = string.Empty;
                oLM.RememberMe = false;
                oLM.Pwd = string.Empty;

                TempData["ResLOG"] = "Success";
            }

            return View(oLM);
        }

        [HttpPost]
        //[ValidateInput(true)]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel oLM)
        {
            string sMsg = "";
            // PaidTypeModel oPM = new PaidTypeModel();
            try
            {
                if (ModelState.IsValid)
                {
                    int iRet = 0;
                    bool bflag = false;
                    _care_repo.Authenticate_User(oLM, out iRet, out bflag);


                    if (oLM.RememberMe == true)
                    {


                        bool rememberMe = oLM.RememberMe;

                        int timeout = ((cookies_expiry_date) * (1440)); // Timeout in minutes
                                                                        // int timeout = cookies_expiry_date;
                        var authTicket = new FormsAuthenticationTicket(oLM.MSISDN + "|" + oLM.Pwd, rememberMe, timeout);

                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                    }
                    else
                    {
                        bool rememberMe = false;
                        int timeout = rememberMe ? 1 : 0; // Timeout in minutes, 525600 = 365 days.
                        var authTicket = new FormsAuthenticationTicket(oLM.MSISDN + "|" + oLM.Pwd, rememberMe, timeout);
                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookie);

                    }

                    if (iRet == 102)
                        sMsg = "Invalid Request!";
                    else if (iRet == 101)
                    {
                        Session["subscriber_Reset"] = "677" + oLM.MSISDN;
                        return RedirectToAction("ResetPassword");
                    }
                    else if (iRet == 503)
                    {

                        return RedirectToAction("ResetPassword");
                    }
                    else if (iRet == 105)
                        sMsg = "Login Failed!";
                    else if (iRet == 501)
                    {

                        Session["subscriber"] = "677" + oLM.MSISDN.Trim();

                        if (Session["sess_yp"] != null)//if (Session["YourPlanPrice"] != null)
                        {
                            if (Convert.ToString(Session["sess_yp"]).ToLower() == "devicesettings")
                                return RedirectToAction("devicesettings", new { id = oLM.MSISDN.Trim() });

                            return RedirectToAction("PlanPricing/" + Session["sess_yp"]);
                        }
                        else
                            return RedirectToAction("Prepaid/"+"promo");

                    }
                    //else if (iRet == 502)
                    //{

                    //    Session["subscriber"] = "677" + oLM.MSISDN;
                    //    if (Session["sess_yp"] != null)
                    //    {
                    //        if (Convert.ToString(Session["sess_yp"]).ToLower() == "devicesettings")
                    //            return RedirectToAction("devicesettings", new { id = oLM.MSISDN.Trim() });

                    //        return RedirectToAction("PlanPricing");
                    //    }
                    //    //else
                    //    //    return RedirectToAction("Postpaid");

                    //}
                    else if (iRet == 505)
                    {
                        return RedirectToAction("LockAccount");
                    }
                    else
                        sMsg = "Invalid Request!";

                }

                ViewBag.Message = sMsg;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }
        #endregion

        #region Reset Password
        [SessionExpireFilter]
        public ActionResult ResetPassword()
        {
            ResetModel oRSM = new ResetModel();
            try
            {
                if (Session["subscriber"] == null && Session["subscriber_Reset"] == null)
                    return RedirectToAction("Login");
                else
                {
                    string _sMSISDN = "";
                    AccountModel oAM = new AccountModel();
                    if (Session["subscriber"] != null)
                    {
                        _sMSISDN = Session["subscriber"].ToString();

                        oAM = _care_repo.GetMultipleSubscribers(_sMSISDN);
                        if (oAM != null)
                        {
                            if (oAM.Reg.paidType == "502")
                                ViewBag.posType = "502";
                            else if (oAM.Reg.paidType == "501")
                                ViewBag.preType = "501";
                            if (oAM.Subr.Count > 1)
                                ViewBag.preType = "501";
                            oRSM.UserId = oAM.Reg.UserId.ToString();
                        }
                    }
                    else if (Session["subscriber_Reset"] != null)
                    {
                        _sMSISDN = Session["subscriber_Reset"].ToString();
                        oAM = _care_repo.GetMultipleSubscribers(_sMSISDN);
                        if (oAM != null)
                        {
                            oRSM.UserId = oAM.Reg.UserId.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oRSM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetModel oRSM)
        {
            string sMsg = "";
            try
            {
                if (Session["subscriber"] == null && Session["subscriber_Reset"] == null)
                    return RedirectToAction("Login");
                else
                {
                    if (ModelState.IsValid)
                    {
                        int iRet = 0;
                        bool bFlag = false;
                        _care_repo.ResetPwd(oRSM, out iRet, out bFlag);

                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookie);

                        TempData["Res"] = "success";

                        if (iRet == 0)
                        {
                            return RedirectToAction("Thankyou", "Care", new { id = "Reset" });
                        }
                        else
                            sMsg = "Please enter valid current password!";
                    }
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oRSM);
        }
        #endregion

        #region Forgot Password
        public ActionResult ForgotPassword()
        {

            FPModel oFP = new FPModel();
            try
            {
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oFP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(FPModel oFP)
        {
            string sMsg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    Dictionary<string, string> dicDetails = new Dictionary<string, string>();
                    dicDetails = _care_repo.TempPassEmail(oFP);
                    if (dicDetails.Count > 0)
                    {
                        if (dicDetails["ResultCode"] == "0")
                        {
                            string resetlink = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "") + "/Care/TempReset/" + dicDetails["RegId"];
                            dicDetails.Add("ResetLink", resetlink);
                            dicDetails.Add("ToEmail", oFP.Email);
                            bool bEmail = false;
                            bEmail = _care_repo.EmailTemppwd(dicDetails);
                            if (bEmail == false)
                                sMsg = "Failed to send Email!";
                            else
                                sMsg = "Reset link has been sent to your registered email.";
                        }
                        else
                            sMsg = "Failed to process";
                    }
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult TempReset(string id)
        {
            TPModel oTP = new TPModel();
            try
            {
                oTP.Id = id;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oTP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TempReset(TPModel oTP)
        {
            string sMsg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    bool bVerify = false;
                    _care_repo.VerifyUser(oTP, out bVerify);
                    if (bVerify == true)
                    {
                        int iRet = -1;
                        _care_repo.UpdatePassword(oTP, out iRet);
                        if (iRet == 0)
                        {
                            return RedirectToAction("Thankyou", new { id = "Temppwd" });
                        }
                        else if (iRet == 201)
                        { sMsg = "Your password got expired!"; }
                        else
                        { sMsg = "Failed to Proceed further!"; }
                    }
                    else
                    {
                        sMsg = "Failed to proceed further!";
                    }
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }
        #endregion

        #region Prepaid
        //[SessionExpireFilter]
        //public ActionResult Prepaid()
        //{
        //    string sLogMSISDN="Unknown";
        //    string sLogEmail = "Unknown";
        //    AccountModel oAM = new AccountModel();
        //    try
        //    {
        //        if (Session["subscriber"] == null)
        //            return RedirectToAction("Login");
        //        else
        //        {
        //            string sMSISDN = Session["subscriber"].ToString();
        //            oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
        //            if (oAM != null)
        //            {
        //                sLogMSISDN=oAM.Reg.BroadBandNo;
        //                sLogEmail=oAM.Reg.Email;
        //                ViewBag.preType = "501";
        //                if (oAM.Reg.paidType == "501")
        //                {
        //                    oAM.Reg._rd_action = "Prepaid";
        //                }
        //                else
        //                {
        //                    return RedirectToAction("Login");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _util_repo.ErrorLog_Txt(ex.Message+"MSISDN :"+sLogMSISDN+"Email : "+sLogEmail, ex.StackTrace);

        //    }
        //    return View(oAM);

        //}


        [SessionExpireFilter]
        public ActionResult Prepaid(string id)
        {
            AccountModel oAM = new AccountModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    if (id=="promo")
                        ViewBag.promoIB = "netbank";

                    string sMSISDN = Session["subscriber"].ToString();
                    string sInvoiceNumber = "";
                    oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                    if (oAM != null)
                    {
                       
                       
                        double ppbal = 0;

                        foreach (var sb in oAM.Subr)
                        {
                            //if (sb.paidtype == "502")
                            //{
                            //    if (!string.IsNullOrEmpty(sb.totalAmount))
                            //    {
                            //        ppbal = Convert.ToDouble(sb.stotalAmount) / 100;
                            //        sInvoiceNumber = sb.transactionNo;
                            //    }
                            //}
                        }
                        //ViewBag.postpaidBal = ppbal;
                        //ViewBag.pptransno = sInvoiceNumber;


                        if (oAM.Subr.Count > 1)
                        {
                            ViewBag.preType = "501";
                            ViewBag.subCnt = true;
                        }
                        //else
                        //    ViewBag.posType = "502";

                        if (oAM.Reg.paidType == "501")
                        {
                            ViewBag.preType = "501";
                            oAM.Reg._rd_action = "Prepaid";
                        }
                        //else
                        //{

                        //    //oAM.Reg._rd_action = "Prepaid";
                        //    ViewBag.Disp_postpaid = "display:none;";
                        //    ViewBag.Disp_prepaid = "display:block;";
                        //}

                    }
                }

                oAM.Subr.OrderByDescending(m => m.accountNo);

                ViewBag.MSISDN_Details = JsonConvert.SerializeObject(oAM);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
            return View(oAM);

        }

        #endregion

        #region Postpaid_bak
        [SessionExpireFilter]
        public ActionResult Postpaid_bak()
        {
            string sLogMSISDN = "Unknown";
            string sLogEmail = "Unknown";
            AccountModel oAM = new AccountModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    string sMSISDN = Session["subscriber"].ToString();
                    string sInvoiceNumber = "";
                    oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                    if (oAM != null)
                    {
                        sLogMSISDN = oAM.Reg.BroadBandNo;
                        sLogEmail = oAM.Reg.Email;
                        double ppbal = 0;
                        
                        foreach (var sb in oAM.Subr)
                        {
                            if (sb.isPrimary == true)
                            {
                                if (!string.IsNullOrEmpty(sb.totalAmount))
                                {
                                    ppbal = Convert.ToDouble(sb.stotalAmount) / 100;
                                    sInvoiceNumber = sb.transactionNo;
                                }
                            }
                        }
                        ViewBag.postpaidBal = ppbal;
                        ViewBag.pptransno = sInvoiceNumber;


                        if (oAM.Subr.Count > 1)
                        {
                            ViewBag.preType = "501";
                            ViewBag.subCnt = true;
                        }
                        else
                            ViewBag.posType = "502";

                        if (oAM.Reg.paidType == "502")
                        {
                            ViewBag.posType = "502";
                            oAM.Reg._rd_action = "Postpaid";
                        }
                        else
                        {

                            //oAM.Reg._rd_action = "Prepaid";
                            ViewBag.Disp_postpaid = "display:none;";
                            ViewBag.Disp_prepaid = "display:block;";
                        }

                    }
                }

                ViewBag.MSISDN_Details = JsonConvert.SerializeObject(oAM);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message + "MSISDN :" + sLogMSISDN + "Email : " + sLogEmail, ex.StackTrace);
            }
            return View(oAM);
        }



        public JsonResult PostpaidBillpay(string sAmount, string sPaymentType,string invoiceno, string paid_msisdn,string ac_no)
        {
            string sRes = "";
            string sResMsg = string.Empty;
            string sRetURL = string.Empty;
            if (Session["subscriber"] != null)
            {
                if (!string.IsNullOrEmpty(sAmount) && !string.IsNullOrWhiteSpace(sAmount))
                {
                    if (!string.IsNullOrWhiteSpace(sPaymentType) && !string.IsNullOrEmpty(sPaymentType))
                    {
                        double dRes = 0;
                        if (double.TryParse(sAmount, out dRes))
                        {
                            string sMSISDN = Session["subscriber"].ToString();
                            AccountModel oAM = new AccountModel();
                            oAM.Reg = _care_repo.GetRegDetails(sMSISDN);
                            if (oAM.Reg != null)
                            {
                                PaymentEntModel pay = new PaymentEntModel();
                                pay.email = oAM.Reg.Email;
                                pay.fname = oAM.Reg.FirstName;
                                pay.lname = oAM.Reg.LastName;
                                pay.paid_amount = sAmount;
                                pay.paid_for_msisdn = paid_msisdn;
                                pay.paid_userid = oAM.Reg.UserId;
                                pay.sess_id = Session.SessionID;
                                pay.primary_msisdn = oAM.Reg.BroadBandNo;
                                pay.payment_mode = 2;
                                pay.payment_gateway = "DOKU";
                                pay.payment_type = "CREDITCARD";
                                pay.paid_amount = sAmount;
                                pay.payment_receipt_no = invoiceno;
                                //pay.product_id = -2;
                                //pay.product_name = oAM.Reg.account_number;
                                //pay.purchse_desc = "Postpaid_Pdf_bill";
                                

                                List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();

                                tbl_doku_order_item o_item = new tbl_doku_order_item();

                                o_item.product_id = -2;
                                o_item.product_name = ac_no;
                                o_item.purchase_desc = "Postpaid_Pdf_bill";
                                o_item.product_qty = 1;
                                o_item.product_price = Convert.ToDecimal(sAmount);
                               
                                order_items.Add(o_item);

                                if (order_items.Count > 0)
                                    pay.order_items = order_items.ToArray();

                                long retOrderId = _doku_client.save_selfcare_order(pay); 

                                if (retOrderId > 0)
                                {
                                    pay.sess_id = Session.SessionID;
                                    long dokuId = _doku_client.save_doku_order_payment(pay, retOrderId, 1);
                                    if (dokuId > 0)
                                    {
                                        temp_orders_model oT = new temp_orders_model();
                                        oT.Id = 0;
                                        oT.Order_Id = retOrderId;
                                        oT.Session_Id = Session.SessionID;
                                        oT.SiteId = 1;
                                        string sJStemp = JsonConvert.SerializeObject(oT);
                                        long TempOrderId = _doku_client.save_temp_order(sJStemp);
                                        if (TempOrderId > 0)
                                        {
                                            sResMsg = "success";

                                            string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                            sRetURL = dk_process_url + "/" + oT.Session_Id;
                                            //return Redirect(dk_process_url + "/" + oT.Session_Id);
                                        }
                                    }
                                    else
                                    {
                                       sRes = "Failed to Proceed..";
                                        //return View(oTP);
                                    }

                                }


                            }
                        }
                        else
                        {
                            sRes = "Only numbers allowed!";
                        }
                    }
                    else
                        sRes = "Payment type Required!";
                }
                else
                    sRes = "Amount Required!";
            }
            else
            {
                sRes = "Invalid Request!";
            }
            //return Json(sRes);

            return Json(new { res = sRes, rmsg = sResMsg, rurl = sRetURL }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilter]
        public ActionResult Dwnld_bill(string id)
        {
            byte[] result = null;
            try
            {
                string sMsg = "";
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {

                    if (id != "")
                    {
                        string acpath = "";
                        string pattern = id;
                        string fpath = System.Configuration.ConfigurationManager.AppSettings["fpathpdf"].ToString();
                        var bills = Directory.GetFiles(fpath, pattern, SearchOption.AllDirectories);
                        if (bills.Count() != 0)
                        {
                            acpath = bills.FirstOrDefault().ToString();
                            result = _care_repo.DwldPostpaidbill(acpath, id);
                        }
                    }
                    else
                        id = "No Records found!";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            if (result != null)
                return File(result, "application/pdf", id);
            else
                return RedirectToAction("Prepaid");  // RedirectToAction("Postpaid"); 
        }


        private string sMonthname(string sFilename)
        {
            string sRes = "";
            if (sFilename != "")
            {
                string[] sfn = sFilename.Split('_');
                if (sfn.Length > 0 && sfn[1] != null)
                {
                    string[] sDate = sfn[1].Split('.');
                    if (sDate.Length > 0 && sDate[0] != null)
                    {
                        DateTime dt = DateTime.ParseExact(sDate[0], "yyyyMMdd", null);
                        sRes = dt.ToString("MMMM-yyyy");
                    }
                }
            }
            return sRes;
        }

        private DateTime dFileDate(string sFilename)
        {
            DateTime dRes = new DateTime();
            if (sFilename != "")
            {
                string[] sfn = sFilename.Split('_');
                if (sfn.Length > 0 && sfn[1] != null)
                {
                    string[] sDate = sfn[1].Split('.');
                    if (sDate.Length > 0 && sDate[0] != null)
                    {
                        DateTime dt = DateTime.ParseExact(sDate[0], "yyyyMMdd", null);
                        dRes = dt;
                    }
                }
            }
            return dRes;
        }
        //for app
        public ActionResult AppDwnldbill(string id)
        {
            byte[] result = null;
            try
            {
                if (id != "")
                {
                    string acpath = "";
                    string pattern = id;
                    string fpath = System.Configuration.ConfigurationManager.AppSettings["fpathpdf"].ToString();
                    var bills = Directory.GetFiles(fpath, pattern, SearchOption.AllDirectories);
                    if (bills.Count() != 0)
                    {
                        acpath = bills.FirstOrDefault().ToString();
                        result = _care_repo.DwldPostpaidbill(acpath, id);
                    }
                }
                else
                    id = "No Records found!";
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            if (result != null)
                return File(result, "application/pdf", id);
            else
                return Json("InvalidRequest", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Validate Broadband Number
        public JsonResult checkbroadbandno(RegistrationModel Reg)
        {

            Utils.General oGen = new Utils.General();
            bool bRes = oGen.CheckContactNo(Reg.BroadBandNo);
            return Json(bRes, JsonRequestBehavior.AllowGet);

        }

        public JsonResult checkMSISDN(LoginModel Logi)
        {
            Utils.General oGen = new Utils.General();
            bool bRes = oGen.CheckContactNo(Logi.MSISDN);
            return Json(bRes, JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region POST Method to Load Plans
        public ActionResult MsisDNBundles(string sMsisdn)
        {

            BundlesModel oBM = new BundlesModel();
            if (Session["subscriber"] != null)
            {
                string sMSIDN = Session["subscriber"].ToString();
                AccountModel oAM = new AccountModel();
                oAM = _care_repo.GetMultipleSubscribers(sMSIDN);
                if (oAM != null)
                {
                    if (oAM.Subr != null && oAM.Subr.Count > 0)
                        ViewBag.AccountBalance = oAM.Subr.Where(x => x._MSISDNNumber == sMsisdn).Select(m => m._sbalance).FirstOrDefault();

                    if (sMsisdn == "") 
                    {
                        var prepaidno = oAM.Subr.Where(p => p.paidtype != "502").FirstOrDefault();
                        if (prepaidno != null)
                        {
                            sMsisdn = prepaidno._MSISDNNumber;
                            oBM = BindUserBundles(oAM, sMsisdn);
                        }
                        else
                        {
                            oBM.err_Msg = "Not applicable for a  postpaid user!";
                            oBM.btnMode = "disabled=disabled";
                            oBM.dtopuptext = _Hide;
                        }
                    }
                    else
                    {
                        var chkPostpaid = oAM.Subr.Where(p => p._MSISDNNumber == sMsisdn).FirstOrDefault();
                        if (chkPostpaid != null)
                        {
                            if (chkPostpaid.paidtype == "501")
                                oBM = BindUserBundles(oAM, sMsisdn);
                            else
                            {
                                oBM.err_Msg = "Not applicable for a  postpaid user!";
                                oBM.btnMode = "disabled=disabled";
                                oBM.dtopuptext = _Hide;
                            }
                        }
                        else
                        {
                            oBM.err_Msg = "Forbidden access!";
                            oBM.btnMode = "disabled=disabled";
                            oBM.dtopuptext = _Hide;
                        }
                    }

                }

            }
            ;
            return PartialView("_bm_PlanDetails", oBM);
        }

        #region Bind User Bundles
        private BundlesModel BindUserBundles(AccountModel oAM, string sMsisdn)
        {
            BundlesModel oBM = new BundlesModel();
            if (oAM.Reg.paidType == "502")
                ViewBag.posType = "502";
            else if (oAM.Reg.paidType == "501")
                ViewBag.preType = "501";
            if (oAM.Subr.Count > 1)
                ViewBag.preType = "501";

            var user = oAM.Subr.Where(u => u._MSISDNNumber == sMsisdn).FirstOrDefault();
            if (user != null)
            {
                double dBalance = user.balance;
                long regId = oAM.Reg.UserId;
                var bundles = _care_repo.GetBundles(regId.ToString(), dBalance).Where(b => b.isActive == true && b.isDeleted == false).ToList();
                if (bundles.Count > 0)
                {
                    //oBM.DataPlan = bundles.Where(d => d.isVoice == false).ToList();
                    oBM.DataPlan = bundles.Where(d => d.AccountType != "0" && d.VoiceAccountType == "0" && d.SmsAccountType == "0" && d.IddAccountType == "0" && d.romingDataAccountType=="0" && d.roamingVoiceAccountType == "0" && d.roamingSmsAccountType == "0").ToList();
                    oBM.Voice = _care_repo.GetVoicePlans(bundles, user.planDetails, dBalance).ToList();

                    oBM.VoiceSMSDataPlan = bundles.Where(d => d.AccountType != "0" && d.VoiceAccountType != "0" && d.SmsAccountType != "0" && d.IddAccountType == "0" && d.romingDataAccountType =="0" && d.roamingVoiceAccountType =="0" && d.roamingSmsAccountType == "0").ToList();

                    oBM.RoamingPlan = bundles.Where(d => d.romingDataAccountType != "0" || d.roamingSmsAccountType !="0" || d.roamingVoiceAccountType !="0").ToList();

                    oBM.MsisdnLst = FillMSISDN(oAM.Subr, sMsisdn);
                    if (dBalance == 0)
                    {
                        oBM.dtopuptext = _Show;
                        oBM.btnMode = "disabled=disabled";

                    }
                    else
                    {
                        oBM.dtopuptext = _Hide;
                        oBM.btnMode = "enabled=enabled";
                    }
                }
            }
            return oBM;
        }
        #endregion


        #endregion

        #region Plans

        private List<SelectListItem> FillMSISDN(List<SubscriberModel> oSM, string qm)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var res = (from b in oSM select new { _msis = b._MSISDNNumber, MSISDN = b._MSISDNNumber, ptype = b.paidtype }).ToList();
            for (int i = 0; i < res.Count; i++)
            {

                if (res[i].ptype != "502")
                {
                    bool bselected = false;
                    if (qm == res[i].MSISDN)
                        bselected = true;
                    var item = new SelectListItem { Text = res[i].MSISDN, Value = res[i].MSISDN.ToString(), Selected = bselected };
                    result.Add(item);
                }
            }
            return result;
        }


        [SessionExpireFilter]
        public ActionResult Plans(string id)
        {
            BundlesModel oBM = new BundlesModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");

                oBM.q_msisdn = id;

                if (Session["subscriber"] != null)
                {
                    string sMSIDN = Session["subscriber"].ToString();
                    AccountModel oAM = new AccountModel();
                    oAM = _care_repo.GetMultipleSubscribers(sMSIDN);
                    if (oAM != null)
                    {

                        if (oAM.Reg.paidType == "501")
                            ViewBag.preType = "501";

                        if (oAM.Subr != null && oAM.Subr.Count > 0)
                            ViewBag.AccountBalance = oAM.Subr.Where(x => x._MSISDNNumber == sMSIDN).Select(m => m._sbalance).FirstOrDefault();

                        if (id == null)
                        {
                            var prepaidno = oAM.Subr.Where(p => p.paidtype != "502").FirstOrDefault();
                            if (prepaidno != null)
                            {
                                id = prepaidno._MSISDNNumber;
                                oBM = BindUserBundles(oAM, id);
                            }
                            else
                            {
                                oBM.err_Msg = "Not applicable for a  postpaid user!";
                                oBM.btnMode = "disabled=disabled";
                                oBM.dtopuptext = _Hide;
                            }
                        }
                        else
                        {
                            var chkPostpaid = oAM.Subr.Where(p => p._MSISDNNumber == id).FirstOrDefault();
                            if (chkPostpaid != null)
                            {
                                if (chkPostpaid.paidtype == "501")
                                    oBM = BindUserBundles(oAM, id);
                                else
                                {
                                    oBM.err_Msg = "Not applicable for a  postpaid user!";
                                    oBM.btnMode = "disabled=disabled";
                                    oBM.dtopuptext = _Hide;
                                }
                            }
                            else
                            {
                                oBM.err_Msg = "Forbidden access!";
                                oBM.btnMode = "disabled=disabled";
                                oBM.dtopuptext = _Hide;
                            }
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            // return PartialView("_bm_PlanDetails", oBM);
            return View(oBM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Plans(BundlesModel oBM, string Command)
        {
            string sPlanId = "";
            string sMsg = "";
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    if (ModelState.IsValid)
                    {
                        if (Command == "Next")
                        {
                            if (oBM != null)
                            {
                                if (oBM.DataPlan.Count > 0 || oBM.Voice.Count > 0)
                                {
                                    sPlanId = oBM._planId;
                                    Session.Remove("msval");
                                    Session.Add("msval", oBM._sel_msisdn);
                                }
                                return RedirectToAction("ChangeType", "Care", new { id = sPlanId });
                            }
                            else
                            {
                                sMsg = "Insufficient balance!";
                            }
                            ViewBag.Message = sMsg;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oBM);
        }
        #endregion

        #region Refresh
        [SessionExpireFilter]
        public ActionResult RefreshACC()
        {

            if (Session["subscriber"] == null && Session["subscriber_Reset"] == null)
                return RedirectToAction("Login");
            else
            {
                RegistrationModel oRM = new RegistrationModel();
                if (Session["subscriber"] != null)
                {
                    string sMSISDN = Session["subscriber"].ToString();
                    oRM = _care_repo.GetRegDetails(sMSISDN);
                    if (oRM != null)
                    {
                        if (oRM.paidType == "501")
                            return RedirectToAction("Prepaid");
                        //else if (oRM.paidType == "502")
                        //    return RedirectToAction("Postpaid");
                    }
                }
                else if (Session["subscriber_Reset"] != null)
                {
                    return RedirectToAction("ResetPassword");
                }
            }
            return View();
        }
        #endregion


        #region POPUP Add/Verify MSISDN
       
        public ActionResult AddMsisdn()
        {
            try
            {

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("AddMsisdn");

        }
        public ActionResult VerifyMSISDN()
        {
            try
            {

                return PartialView("VerifyMSISDN");
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("VerifyMSISDN");

        }


        public string SendSMSVerficationCode(string sMsisdn)
        {
            int iRet = -1;
            string sMSISDN = "";
            if (Session["subscriber"] != null)
            {
                if (sMsisdn != "")
                {
                    long lRes = 0;
                    if (long.TryParse(sMsisdn, out lRes))
                    {
                        if (Session["subscriber"] != null)
                            sMSISDN = Session["subscriber"].ToString();
                        RegistrationModel oRM = new RegistrationModel();
                        oRM = _care_repo.GetRegDetails(sMSISDN);
                        if (oRM != null)
                        {
                            long regId = oRM.UserId;
                            bool bflag = true;
                            iRet = _care_repo.SendSimVerification_Res(regId, "677" + sMsisdn, out iRet, out bflag);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Only numbers allowed!";
                    }
                }
                else
                    ViewBag.Message = "Mobile Number Required!";
            }
            return iRet.ToString();
        }

        public string UpdateMSISDN_Description(long sRegId, string sMsisdn, string sDesc)
        {
            string Res = "1";
            try
            {
                if (Session["subscriber"] != null)
                {
                    bool bflag = _care_repo.Update_MSISDN_Description(sRegId, sMsisdn, sDesc);
                    if (bflag)
                        Res = "0";
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return Res;
        }

        public string AddMultiSIM(string sMsisdn, string sVCode, string sDesc)
        {
            string res = "";
            int iRet = -1;
            string sMSISDN = "";
            if (Session["subscriber"] != null)
            {
                sMSISDN = Session["subscriber"].ToString();

                RegistrationModel oRM = new RegistrationModel();
                oRM = _care_repo.GetRegDetails(sMSISDN);
                if (oRM != null)
                {

                    string sURL = "";

                    if (oRM.paidType == "501")
                        sURL = "Prepaid";
                    //else if (oRM.paidType == "502")
                    //    sURL = "Postpaid";

                    long regId = oRM.UserId;
                    bool bflag = true;
                    iRet = _care_repo.AddMulitSIM_Res(regId, sMsisdn, sVCode, sDesc, out iRet, out bflag);
                    ResultModel r = new ResultModel();
                    r.rcode = iRet.ToString();
                    r.rURL = sURL;
                    res = JsonConvert.SerializeObject(r, Formatting.Indented);
                }
            }
            return res;
        }
        public string RemoveSIM(string sMsisdn)
        {
            string res = "";
            int iRet = -1;
            string sMSISDN = "";
            if (Session["subscriber"] != null)
            {
                sMSISDN = Session["subscriber"].ToString();

                RegistrationModel oRM = new RegistrationModel();
                oRM = _care_repo.GetRegDetails(sMSISDN);
                if (oRM != null)
                {
                    string sURL = "";
                    if (oRM.paidType == "501")
                        sURL = "Prepaid";
                    //else if (oRM.paidType == "502")
                    //    sURL = "Postpaid";
                    ViewBag.reURL = sURL;
                    long regId = oRM.UserId;
                    iRet = _care_repo.RemoveSIM_Res(regId, sMsisdn);
                    ResultModel r = new ResultModel();
                    r.rcode = iRet.ToString();
                    r.rURL = sURL;
                    res = JsonConvert.SerializeObject(r, Formatting.Indented);
                }
            }
            return res;
        }

        #endregion


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


        #region Purchase Bundle

        [SessionExpireFilter]
        public ActionResult ChangeType(string id)
        {

            ChangeTypeModel oCM = new ChangeTypeModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {

                    string _sMSISDN = Session["subscriber"].ToString();
                    AccountModel oAM = new AccountModel();
                    oAM = _care_repo.GetMultipleSubscribers(_sMSISDN);
                    if (oAM != null)
                    {
                        if (oAM.Reg.paidType == "502")
                            ViewBag.posType = "502";
                        else if (oAM.Reg.paidType == "501")
                            ViewBag.preType = "501";
                        if (oAM.Subr.Count > 1)
                            ViewBag.preType = "501";



                        string sMSISDN = "";
                        if (Session["msval"] != null)
                            sMSISDN = Session["msval"].ToString();

                        bool bUserExists = false;
                        var reg = oAM.Subr.Where(s => s._MSISDNNumber == sMSISDN).FirstOrDefault();
                        if (reg != null)
                        {
                            bUserExists = _care_repo.VerifyMSISN(oAM.Reg.UserId, sMSISDN);
                            if (bUserExists == true)
                            {
                                oCM.MobileNo = reg._MSISDNNumber;
                                oCM.FormattedMobileNo = oCM.MobileNo.Substring(0, 3) + " " + oCM.MobileNo.Substring(3); ;
                                oCM.Email = oAM.Reg.Username;
                                oCM.balance = reg.balance;
                                oCM.UserId = oAM.Reg.UserId;
                                long _planID = long.Parse(id);
                                oCM.PlanId = _planID;
                                var bundle = _care_repo.GetBundles(oAM.Reg.UserId.ToString(), oCM.balance).Where(b => b.Id == _planID).FirstOrDefault();
                                if (bundle != null)
                                {
                                    oCM._planPrice = bundle._sPrice;
                                    oCM.dPlanPrice = bundle.Price;
                                    oCM.planName = bundle.PlanName;
                                    oCM.Description = bundle.Description;
                                    //if (bundle.isVoice == true)
                                    //    oCM.BundleType = "Voice";
                                    //else
                                    //    oCM.BundleType = "Data";
                                    if (bundle.AccountType != "0" && bundle.VoiceAccountType == "0" && bundle.SmsAccountType == "0" && bundle.IddAccountType == "0" && bundle.romingDataAccountType =="0" && bundle.roamingVoiceAccountType =="0" && bundle.roamingSmsAccountType=="0")
                                        oCM.BundleType = ConfigurationManager.AppSettings["changetyp1"].ToString();
                                    else if (bundle.AccountType != "0" && bundle.VoiceAccountType != "0" && bundle.SmsAccountType != "0" && bundle.IddAccountType == "0" && bundle.romingDataAccountType =="0" && bundle.roamingVoiceAccountType == "0" && bundle.roamingSmsAccountType == "0")
                                        oCM.BundleType = ConfigurationManager.AppSettings["changetyp2"].ToString();
                                    else if (bundle.AccountType == "0" && bundle.VoiceAccountType == "0" && bundle.SmsAccountType == "0" && bundle.IddAccountType != "0" && bundle.romingDataAccountType == "0" && bundle.roamingVoiceAccountType == "0" && bundle.roamingSmsAccountType == "0")
                                        oCM.BundleType = ConfigurationManager.AppSettings["changetyp3"].ToString();
                                    else if (bundle.romingDataAccountType != "0" || bundle.roamingSmsAccountType !="0" || bundle.roamingVoiceAccountType !="0")
                                        oCM.BundleType = ConfigurationManager.AppSettings["changetyp4"].ToString();
                                }
                            }

                        }
                    }

                    return View(oCM);

                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oCM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeType(ChangeTypeModel oCM, string Command)
        {
            string sMsg = "";
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {


                    if (ModelState.IsValid)
                    {
                        #region To Insert Log For pending status 

                        tbl_web_plan_purchase_trans objTrans = new tbl_web_plan_purchase_trans();
                        objTrans = _care_repo.CreatePlanPurchaseTransLog(oCM);

                        #endregion

                        if (objTrans != null)
                        {

                            bool bUserExists = false;
                            bUserExists = _care_repo.VerifyMSISN(oCM.UserId, oCM.MobileNo);
                            if (bUserExists == true)
                            {
                                if (oCM.balance >= oCM.dPlanPrice)
                                {
                                    sMsg = _care_repo.UpdateSubscription(oCM);
                                    //sMsg = "your promotion purchase is successful.";
                                }
                                else
                                    sMsg = "You have insufficient balance to buy this plan!";
                            }
                            else
                                sMsg = "Operation failed. Please try again!";


                            tbl_web_plan_purchase_trans trans_detail = new tbl_web_plan_purchase_trans();
                            trans_detail = UOW.plan_purchase_trans_Repo.Get(filter: x => x.trans_number == objTrans.trans_number && x.msisdn == objTrans.msisdn && x.plan_id == objTrans.plan_id && x.user_id == objTrans.user_id && x.trans_status == "PENDING" && x.is_processed == false).FirstOrDefault();
                            if (!string.IsNullOrEmpty(sMsg) && sMsg.Trim().ToLower() == "your promotion purchase is successful.")
                            {
                                #region To Update Log For Approved status
                                

                                if (trans_detail != null)
                                {
                                    trans_detail.trans_status = "APPROVED";
                                    trans_detail.plan_purchased_on = DateTime.Now;
                                    trans_detail.API_response = sMsg;
                                    trans_detail.is_processed = true;
                                    UOW.plan_purchase_trans_Repo.Update(trans_detail);
                                    UOW.Save();
                                }
                                return RedirectToAction("ConfirmProduct", "Care", new { id = oCM.PlanId });
                                #endregion
                            }
                            else
                            {
                                #region To Update Log For Failed Status
                                trans_detail.trans_status = "FAILED";
                                trans_detail.API_response = sMsg;
                                trans_detail.is_processed = true;
                                UOW.plan_purchase_trans_Repo.Update(trans_detail);
                                UOW.Save();
                                #endregion
                            }
                        }
                        else
                            sMsg = "Invalid transaction!";
                    }
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oCM);
        }

        [SessionExpireFilter]
        public ActionResult ConfirmProduct(long id)
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");
            else
            {
                if (id != 0)
                {
                    var bundle = _plan_repo.get_bundles_byId(id);
                    if (bundle != null)
                        ViewBag.bdata = bundle.PlanName;
                    else
                        ViewBag.bdata = "";
                }
                else
                    ViewBag.bdata = "";

                string _sMSISDN = Session["subscriber"].ToString();
                AccountModel oAM = new AccountModel();
                oAM = _care_repo.GetMultipleSubscribers(_sMSISDN);
                if (oAM != null)
                {
                    if (oAM.Reg.paidType == "502")
                        ViewBag.posType = "502";
                    else if (oAM.Reg.paidType == "501")
                        ViewBag.preType = "501";
                    if (oAM.Subr.Count > 1)
                        ViewBag.preType = "501";
                }
            }

            return View();
        }
        #endregion

        #region Top up

        public String GetIPAddress()
        {
            String ipAddr = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddr))
            {
                ipAddr = Request.ServerVariables["REMOTE_ADDR"];
            }

            return ipAddr;
        }

        [SessionExpireFilter]
        public ActionResult Topup()
        {
            TopupModel oTP = new TopupModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {

                    string sMSISDN = Session["subscriber"].ToString();
                  //  string sMSISDN = "8731281";

                    bool bAllow = false;
                    if (ConfigurationManager.AppSettings["tpMode"].ToString() == "true")
                        bAllow = true;


                    bool bOpen = false;
                    if (ConfigurationManager.AppSettings["tpOpenAll"].ToString() == "true")
                        bOpen = true;

                    oTP.isTOPUP = AllowTopUpNumber(sMSISDN.Trim(), bAllow, bOpen);

                    //string sMYAmount = ConfigurationManager.AppSettings["TPAmounts"].ToString();
                    string sOtherAmount = ConfigurationManager.AppSettings["TPOthers"].ToString();

                    oTP.lstTopupVals = FillTopupAmount();
                    oTP.lstOtherTopupVals = FillTopupAmount(sOtherAmount);
                    AccountModel oAM = new AccountModel();
                oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                if (oAM != null)
                {
                    PaymentPGModel op = new PaymentPGModel();

                    op.fname = oAM.Reg.FirstName;
                    op.lname = oAM.Reg.LastName;
                    op.email = oAM.Reg.Email;
                    op.primary_msisdn = oAM.Reg.BroadBandNo;
                    op.paid_userid = oAM.Reg.UserId;
                    oTP.tpPaidType = oAM.Reg.paidType;
                    if (oAM.Reg.paidType == "502")
                        ViewBag.posType = "502";
                    else
                        ViewBag.preType = "501";

                    if (oAM.Subr.Count > 0)
                    {
                        oTP.UserId = oAM.Reg.UserId;
                        if (oAM.Subr.Count > 1)
                        {
                            ViewBag.preType = "501";
                            ViewBag.subCnt = true;
                            oTP.MsisdnLst = FillMSISDN(oAM.Subr, "");
                            oTP.MsisdnLst_temp = FillMSISDN(oAM.Subr, "");
                            if (oTP.MsisdnLst.Count == 1)
                            {
                                oTP.MSISDN_Number = oTP.MsisdnLst[0].Text;
                                oTP.MsisdnLst = null;
                            }
                            if (oTP.MsisdnLst_temp.Count == 1)
                            {
                                oTP.MSISDN_Number_temp = oTP.MsisdnLst_temp[0].Text;
                                oTP.MsisdnLst_temp = null;
                                op.paid_for_msisdn = oTP.MSISDN_Number_temp;
                            }

                        }
                        else
                        {
                            if (oAM.Reg.paidType == "502" && oAM.Subr.Count == 1)
                            {
                                Session.Clear();
                                return RedirectToAction("Login");
                            }
                            else
                            {
                                oTP.MSISDN_Number = oAM.Subr[0]._MSISDNNumber;
                                oTP.MSISDN_Number_temp = oAM.Subr[0]._MSISDNNumber;
                                op.paid_for_msisdn = oTP.MSISDN_Number_temp;
                            }
                        }
                        op.paid_for_msisdn = oTP.MSISDN_Number_temp;
                        oTP.pay = op;
                    }

                }
            }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oTP);
        }
        private bool AllowTopUpNumber(string sMSISDN, bool bAllow, bool bOpen)
        {
            bool bRes = false;
            if (bOpen == true)
            {
                bRes = true;
            }
            else
            {
                if (bAllow == true)
                {
                    string[] sTPnumbers = ConfigurationManager.AppSettings["topupnumbers"].ToString().Split(',');
                    if (sTPnumbers.Length > 0)
                    {
                        foreach (string sno in sTPnumbers)
                        {
                            if (sno == sMSISDN)
                                bRes = true;
                        }
                    }
                }
            }
            return bRes;
        }

        private List<SelectListItem> FillTopupAmount()
        {
            //List<SelectListItem> lstTopup = new List<SelectListItem>();

            //string[] Amt = sAmount.Split(',');
            //if (Amt.Length > 0)
            //{
            //    for (int i = 0; i < Amt.Length; i++)
            //    {
            //        var item = new SelectListItem { Text = "$ " + Amt[i].ToString(), Value = Convert.ToDecimal(Amt[i]).ToString("#0.00") };
            //        lstTopup.Add(item);
            //    }
            //}
            //return lstTopup;
            List<SelectListItem> lstTopup = new List<SelectListItem>();

            //string[] Amt = sAmount.Split(',');
            //if (Amt.Length > 0)
            //{
            //    for (int i = 0; i < Amt.Length; i++)
            //    {
            //        var item = new SelectListItem { Text = "K " + Amt[i].ToString(), Value = Convert.ToDecimal(Amt[i]).ToString("#0.00") };
            //        lstTopup.Add(item);
            //    }
            //}

            List<tbl_bm_topup_value> obj = new List<tbl_bm_topup_value>();
            obj = (from t in _dbctxt.topup_value
                   where t.is_active == true && t.is_deleted == false && t.is_web == true
                   select t).ToList();
            if (obj.Count > 0)
            {

                foreach (var item in obj)
                {
                    SelectListItem lstitem = new SelectListItem();
                    lstitem = new SelectListItem { Text = item.currency + " " + item.value, Value = Convert.ToDecimal(item.value).ToString("#0.00") };
                    lstTopup.Add(lstitem);
                }

            }

            return lstTopup;
        }

        private List<SelectListItem> FillTopupAmount(string sAmount)
        {
            List<SelectListItem> lstTopup = new List<SelectListItem>();

            string[] Amt = sAmount.Split(',');
            if (Amt.Length > 0)
            {
                for (int i = 0; i < Amt.Length; i++)
                {
                    var item = new SelectListItem { Text = "SBD " + Amt[i].ToString(), Value = Convert.ToDecimal(Amt[i]).ToString("#0.00") };
                    lstTopup.Add(item);
                }
            }

            return lstTopup;
        }
        public ActionResult ajax_AddTopup(string msisdn_no, string voucher_no)
        {
            List<TopupModel> TopupList = new List<TopupModel>();
            if (Session["voucher_list"] != null)
                TopupList = (List<TopupModel>)Session["voucher_list"];

            TopupModel oTP = new TopupModel();

            int vcount = TopupList.Where(t => t.Status == false).Count();
            if (vcount > 4)
            {
                oTP.Message = "attempt_failed";
                var reg = _care_repo.GetRegDetails(msisdn_no);
                _care_repo.LockAccount(reg.UserId);
                Session.RemoveAll();
            }
            else
            {
                if (Session["subscriber"] != null)
                {
                    if (!string.IsNullOrEmpty(msisdn_no) && !string.IsNullOrEmpty(voucher_no))
                    {
                        oTP.MSISDN_Number = msisdn_no;
                        oTP.VoucherNumber = voucher_no;
                        string Data = _care_repo.Voucher_Topup(oTP);
                        if (!string.IsNullOrEmpty(Data))
                        {
                            VoucherResultModel obj_voucher = new VoucherResultModel();

                            obj_voucher = JsonConvert.DeserializeObject<VoucherResultModel>(Data);

                            if (obj_voucher != null && obj_voucher.ResultDescription.ToLower() == "operation successfully.")
                            {
                                oTP.Status = true;
                                oTP.Message = "success";

                                if (!string.IsNullOrEmpty(obj_voucher.VoucherAmount))
                                {
                                    double v_amt = Convert.ToDouble(obj_voucher.VoucherAmount);
                                    oTP.tpAmount = ToKina(v_amt);
                                }

                            }
                            else
                            {
                                oTP.Status = false;
                                oTP.Message = "fail";
                            }
                        }
                        else
                        {
                            oTP.Status = false;
                            oTP.Message = "fail";
                        }

                        TopupList.Add(oTP);
                    }
                }
                else
                    oTP.Message = "session_expired";
            }

            Session["voucher_list"] = TopupList;
            string vouchers = JsonConvert.SerializeObject(oTP);
            return Json(vouchers);
        }
        private string ToKina(double _dval)
        {
            double dRes = _dval / 100;
            string sRes = "$ " + dRes.ToString("#0.00");
            return sRes;
        }
        public JsonResult ChkPostpaid(TopupModel oTP)
        {
            int iRet = 0;
            iRet = _care_repo.CheckPaidType(oTP.MSISDN_Number_other);
            if (iRet == 501)
                return Json(true, JsonRequestBehavior.AllowGet);
            else
                return Json(false, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ChkMyNumber(string id)
        {
            bool bRet = true;
            if (id == "#s2")
                bRet = false;
            return Json(bRet, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Topup(TopupModel oTP, string Command)
        {
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    if (oTP.tpPaidType == "502")
                        ViewBag.posType = "502";
                    else
                        ViewBag.preType = "501";

                    if (Command == "Pay")
                    {

                        ModelState.Remove("VoucherNumber");
                        ModelState.Remove("MSISDN_Number");
                        ModelState.Remove("tpAmount");
                        string amount = "";
                        string sResMSISDN = "";

                        if (oTP.isMyNumber == true)
                        {
                            ModelState.Remove("MSISDN_Number_other");
                            ModelState.Remove("tpAmountOther");
                            sResMSISDN = oTP.MSISDN_Number_temp;
                            amount = oTP.tpAmountMy;
                        }
                        else if (oTP.isMyNumber == false)
                        {
                            ModelState.Remove("MSISDN_Number_temp");
                            ModelState.Remove("tpAmountMy");
                            sResMSISDN = oTP.MSISDN_Number_other;
                            amount = oTP.tpAmountOther;
                        }

                        if (oTP.pay.payment_mode == 2)
                        {

                            if (ModelState.IsValid)
                            {

                                PaymentEntModel pay = new PaymentEntModel();
                                pay.email = oTP.pay.email;
                                pay.fname = oTP.pay.fname;
                                pay.lname = oTP.pay.lname;
                                pay.paid_for_msisdn = sResMSISDN;
                                pay.paid_userid = oTP.pay.paid_userid;
                                pay.sess_id = Session.SessionID;
                                pay.primary_msisdn = oTP.pay.primary_msisdn;
                                pay.payment_mode = 2;
                                pay.payment_gateway = "WIRECARD";
                                pay.payment_type = "CREDITCARD";
                                pay.paid_amount = amount;
                                pay.ip_address = GetIPAddress();
                                //pay.product_id = -1;
                                //pay.product_name = "RECHARGE";
                                //pay.purchse_desc = "SETOPUP";
                                //pay.paid_amount = amount;

                                List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();

                                tbl_doku_order_item o_item = new tbl_doku_order_item();

                                o_item.product_id = -4;
                                o_item.product_name = "RECHARGE";
                                o_item.purchase_desc = "SITOPUP";
                                o_item.product_qty = 1;
                                o_item.product_price = Convert.ToDecimal(amount);

                                order_items.Add(o_item);

                                if (order_items.Count > 0)
                                    pay.order_items = order_items.ToArray();

                                long retOrderId = _doku_client.save_selfcare_order(pay);
                                if (retOrderId > 0)
                                {
                                    long dokuId = _doku_client.save_doku_order_payment(pay, retOrderId, 4);
                                    if (dokuId > 0)
                                    {
                                        temp_orders_model oT = new temp_orders_model();
                                        oT.Id = 0;
                                        oT.Order_Id = retOrderId;
                                        oT.Session_Id = Session.SessionID;
                                        oT.SiteId = 4;
                                        string sJStemp = JsonConvert.SerializeObject(oT);
                                        long TempOrderId = _doku_client.save_temp_order(sJStemp);
                                        if (TempOrderId > 0)
                                        {
                                            string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                            return Redirect(dk_process_url + "/" + oT.Session_Id);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Message = "Failed to Proceed..";
                                        return View(oTP);
                                    }

                                }

                            }
                            else
                            {
                                return View(oTP);
                            }
                        }
                        else if (oTP.pay.payment_mode == 3)//internet banking
                        {
                            if (ModelState.IsValid)
                            {
                                if (!string.IsNullOrEmpty(amount))
                                {
                                    List<cartpanel_model> cpm = new List<cartpanel_model>();
                                    cartpanel_model ocm = new cartpanel_model();
                                    ocm.is_topup = false;
                                    ocm.product_id = -11;
                                    ocm.product_name = sResMSISDN;
                                    ocm.product_sku = oTP.pay.email;
                                    ocm.product_price = Convert.ToDecimal(amount);
                                    ocm.product_qty = 1;
                                    cpm.Add(ocm);

                                    long ret_order_id = _shop_repo.save_shopping_order(oTP.pay.paid_userid, Convert.ToDecimal(amount), cpm, 3, "1");//1-recharge
                                    if (ret_order_id > 0)
                                    {
                                        Session["seEztOrederid"] = ret_order_id.ToString();
                                        ViewBag.reURL = "/ShopEzy/Ezy";
                                        // return RedirectToAction("ShopEzy", "Ezy");
                                    }
                                    else
                                    {
                                        ViewBag.Message = "Failed to Proceed..";
                                        return View(oTP);
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Failed to Proceed..";
                                    return View(oTP);
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Failed to Proceed..";
                                return View(oTP);
                            }
                        }

                    }
                    else
                    {
                        if (Session["voucher_list"] != null)
                        {
                            var TopupList = (List<TopupModel>)Session["voucher_list"];
                            TopupList = TopupList.Where(t => t.Status == true).ToList();
                            if (TopupList.Count > 0)
                            {
                                if (Session["purchase_plan"] != null)
                                {
                                    var oPP = (PurchasePlanModel)Session["purchase_plan"];
                                    int Res = _plan_repo.purchase_plan(oPP);
                                    Session.Remove("purchase_plan");
                                    Session.Remove("voucher_list");
                                    return RedirectToAction("YMessage", new { id = Res });
                                }
                                else
                                {
                                    Session.Remove("voucher_list");
                                    return RedirectToAction("TopupSuccess");
                                }
                            }
                        }
                        else
                            ViewBag.Message = "Please add Voucher!";
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(oTP);
        }




        [SessionExpireFilter]
        public ActionResult TopupSuccess()
        {
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");
                else
                {
                    string sMSISDN = Session["subscriber"].ToString();
                    RegistrationModel oRM = new RegistrationModel();
                    oRM = _care_repo.GetRegDetails(sMSISDN);
                    if (oRM != null)
                    {
                        if (oRM.paidType == "502")
                            ViewBag.posType = "502";
                        else
                            ViewBag.preType = "501";
                    }
                    return View();

                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult LockAccount()
        {

            return View();
        }

        //public ActionResult RemoveVoucher(TopupModel topup, int index)
        //{
        //    topup.RemoveVoucher(index);
        //    return Json(topup);
        //}




        [SessionExpireFilter]
        public ActionResult CCTopup()
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");


            return View();
        }
        #endregion

        #region Activate_Sim

        public ActionResult Activate_Sim()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Activate_Sim(ActiveSimModel active_sim)
        {

            return View();
        }
        #endregion

        #region Verify SIM

        [HttpPost]
        public ActionResult VerifySIM(string msidnno, string simno, string pukcode)
        {
            try
            {
                string updated_by = string.Empty;
                if (Session["subscriber"] != null)
                {
                    RegistrationModel oRM = new RegistrationModel();
                    string sUsername = Session["subscriber"].ToString();
                    oRM = _care_repo.GetRegDetails(sUsername);
                    updated_by = oRM.BroadBandNo;
                }
                if (simno.Length > 18)
                    simno = simno.Substring(0, 18);

                string Status = string.Empty;
                string Res = _care_repo.VerifySIMActivation("677" + msidnno, simno, pukcode, updated_by);
                var opRes = JsonConvert.DeserializeObject<OPModel>(Res);

                Status = opRes.result_code;

                return Json(new { Status = Status, RefNo = opRes.reference });
            }
            catch (Exception ex)
            {
                return Json(new { Status = "-111" });
            }
        }
        #endregion

        #region Activate SIM

        [HttpPost]
        public ActionResult ActivateSIM(ActiveSimModel active_sim)
        {
            int _iRes = 0;
            try
            {
                active_sim.status = "failed";

                if (active_sim.sim_number.Length > 18)
                    active_sim.sim_number = active_sim.sim_number.Substring(0, 18);

                _iRes = _care_repo.ActivateSIM("677" + active_sim.msidn_number, active_sim.ref_number);

                if (_iRes == 0)
                    active_sim.status = "success";

                var bRet = _sim_repo.save_sim_contact_details(active_sim);
                //_shop_repo.Save_SIMContactDetail(active_sim);
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Status = "-111" });
            }

            return Json(new { Status = _iRes });
        }
        #endregion

        #region Members Activate SIM

        [HttpPost]
        public ActionResult MembersActivateSIM(string msidnno, string refno)
        {
            int _iRes = 0;
            try
            {
                _iRes = _care_repo.ActivateSIM("677" + msidnno, refno);

            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Status = "-111" });
            }

            return Json(new { Status = _iRes });
        }
        #endregion

        #region Choose your plan pricing
       


        public ActionResult PlanPricing(string id)
        {
            NewPurchasePriceModel oNP = new NewPurchasePriceModel();
            try
            {
                AccountModel oAM = new AccountModel();
                if (Session["subscriber"] != null)
                {
                    string sUsername = Session["subscriber"].ToString();

                    oAM = _care_repo.GetMultipleSubscribers(sUsername);
                    if (oAM != null)
                    {

                        oNP.jsonSubs = JsonConvert.SerializeObject(oAM);

                        var MsisdnBalList = (from Bal in oAM.Subr select new MSISDNBalanceModel { msisdn_no = Bal._MSISDNNumber, balance = Bal._sbalance }).ToList();
                        oNP.jsonallmsisdn = JsonConvert.SerializeObject(MsisdnBalList);

                        oNP.YMSISDNlst = FillMSISDN(oAM.Subr, "");
                        if (oNP.YMSISDNlst.Count == 1)
                        {
                            oNP.purchase_msisdn = oNP.YMSISDNlst[0].Text;
                            oNP.YMSISDNlst = null;
                        }
                        string _msisdnno = sUsername;
                        var yPlanSubs = oAM.Subr.Where(s => s._MSISDNNumber == _msisdnno).FirstOrDefault();
                        if (yPlanSubs != null)
                        {
                            if (yPlanSubs.YPlans != null)
                            {
                                var ypExp = yPlanSubs.YPlans.FirstOrDefault();
                                if (ypExp != null)
                                {
                                    DateTime dtExpiry = Convert.ToDateTime(ypExp.expiry);
                                    if (DateTime.Now.Date < dtExpiry.Date)
                                    {
                                        ViewBag.fpn = true;
                                        ViewBag.ypExpiryDate = _util_repo.ToOrdinalString(dtExpiry.Day) + " " + dtExpiry.ToString("MMMM");
                                        if (id != null)
                                            return RedirectToAction("RefreshACC", "Care");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oNP);

        }


        public ActionResult ajax_ShowPlans(long typeid, string sjsonSubs)
        {
            NewPurchasePriceModel oNP = new NewPurchasePriceModel();
            oNP.bfav = false;
            try
            {
                //string sBal = string.Empty;
                var Plans = _plan_repo.get_yplanprice().Where(p => p.isActive == true).ToList();
                List<plandetails> objplandeno_ids = new List<plandetails>();
                AccountModel oAM = new AccountModel();
                oNP.jsonallplans = JsonConvert.SerializeObject(Plans);
                Plans = Plans.Where(p => p.plan_type_id == typeid).ToList();

                long _user_id = 0;
              
                if (Session["subscriber"] != null)
                {
                    string sUsername = Session["subscriber"].ToString();
                    oAM = JsonConvert.DeserializeObject<AccountModel>(sjsonSubs);

                    if (oAM != null)
                    {
                        oNP.ptype = oAM.Reg.paidType;
                        _user_id = oAM.Reg.UserId;
                        oNP.UserId = _user_id;
                    }
                }

                if (oNP.denomination_ids == null)
                {
                    oNP.denomination_ids = _plan_repo.GetPlans(Plans);
                }

                decimal totalprice = 0;
                int i = 0;
                
                foreach (var ind_plan in oNP.denomination_ids)
                {
                    var plandenominations = (from v in Plans where v.plan_id == ind_plan.plan_id select new SelectListItem { Text = v.denomination.ToString() + " " + v.denomination_name, Value = v.Denomination_id.ToString() }).ToList();
                
                    oNP.denomination_ids[i].deno_id = Convert.ToInt64(plandenominations[i].Value);
                    // plandenominations.Insert(0, (new SelectListItem { Text = "0", Value = "0" }));
                    var planselected = new SelectList(plandenominations, "Value", "Text", oNP.denomination_ids[i].deno_id);
                    ViewData["ddlPLANShow_" + i] = planselected;
                    long _den_id = oNP.denomination_ids[i].deno_id;
                    var _pprice = Plans.Where(p => p.Denomination_id == _den_id).FirstOrDefault();
                    if (_pprice != null)
                        totalprice += Convert.ToDecimal(_pprice.price);
                    i++;
                }
                oNP.tot_amt = totalprice / 100;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("bm_yplans_show", oNP);
        }


        public ActionResult ajax_GetPlans(long typeid, string sjsonSubs, string sSelMSISDN)
        {
            NewPurchasePriceModel oNP = new NewPurchasePriceModel();
            oNP.bfav = false;
            try
            {
                //string sBal = string.Empty;
                var Plans = _plan_repo.get_yplanprice().Where(p => p.isActive == true).ToList();
                List<plandetails> objplandeno_ids = new List<plandetails>();
                AccountModel oAM = new AccountModel();
                oNP.jsonallplans = JsonConvert.SerializeObject(Plans);
                Plans = Plans.Where(p => p.plan_type_id == typeid).ToList();

                long _user_id = 0;
                string[] fixedDenIDs = null;
                if (Session["subscriber"] != null)
                {
                    string sUsername = Session["subscriber"].ToString();
                    oAM = JsonConvert.DeserializeObject<AccountModel>(sjsonSubs);

                    if (oAM != null)
                    {
                        oNP.ptype = oAM.Reg.paidType;
                        oNP.isFixedbuy = false;
                        _user_id = oAM.Reg.UserId;
                        oNP.UserId = _user_id;

                        if (string.IsNullOrEmpty(sSelMSISDN))
                            sSelMSISDN = sUsername.Trim();

                        string _msisdnno = sSelMSISDN;
                        string fixedIDS = "";
                        var yPlanSubs = oAM.Subr.Where(s => s._MSISDNNumber == _msisdnno).FirstOrDefault();
                        if (yPlanSubs != null)
                        {
                            if (yPlanSubs.YPlans != null)
                            {
                                var ypExp = yPlanSubs.YPlans.FirstOrDefault();
                                if (ypExp != null)
                                {
                                    DateTime dtExpiry = Convert.ToDateTime(ypExp.expiry);
                                    if (DateTime.Now.Date < dtExpiry.Date)
                                    {
                                        fixedIDS = ConfigurationManager.AppSettings["fixedplanIds"].ToString();
                                        fixedDenIDs = fixedIDS.Split(',');
                                        oNP.isFixedbuy = true;
                                        ViewBag.ypExpiryDate = _util_repo.ToOrdinalString(dtExpiry.Day) + " " + dtExpiry.ToString("MMMM");
                                    }
                                }
                            }
                        }
                        else if (Session["YourPlanPrice"] == null)
                        {
                            var Fav_denom = _plan_repo.GetFavouritePlan(_user_id, Plans).ToList();
                            if (Fav_denom.Count > 0)
                            {
                                oNP.bfav = true;
                                oNP.denomination_ids = Fav_denom;

                            }
                        }
                        else
                        {
                            var objPurchasePlan = (NewPurchasePriceModel)Session["YourPlanPrice"];
                            oNP.denomination_ids = objPurchasePlan.denomination_ids;
                        }
                    }
                }


                if (oNP.denomination_ids == null)
                {
                    oNP.denomination_ids = _plan_repo.GetPlans(Plans);
                }

                decimal totalprice = 0;
                int i = 0;
                tempDenominationModel otD = new tempDenominationModel();

                if (Session["sess_yp"] != null)
                    otD = _plan_repo.GetTempPlans(Session["sess_yp"].ToString());
                else
                    otD = null;
                string[] tempDenIDs = null;

                if (otD != null)
                {
                    if (!string.IsNullOrEmpty(otD.denomination_Id))
                        tempDenIDs = otD.denomination_Id.Split(',');
                }

                foreach (var ind_plan in oNP.denomination_ids)
                {
                    var plandenominations = (from v in Plans where v.plan_id == ind_plan.plan_id select new SelectListItem { Text = v.denomination.ToString() + " " + v.denomination_name, Value = v.Denomination_id.ToString() }).ToList();
                    if (fixedDenIDs != null)
                    {
                        oNP.denomination_ids[i].deno_id = Convert.ToInt64(fixedDenIDs[i]);
                        oNP.bfav = false;
                    }
                    else if (tempDenIDs != null)
                    {
                        oNP.denomination_ids[i].deno_id = Convert.ToInt64(tempDenIDs[i]);
                        oNP.bfav = false;
                    }
                    else
                    {
                        oNP.denomination_ids[i].deno_id = Convert.ToInt64(plandenominations[i].Value);
                    }
                    // plandenominations.Insert(0, (new SelectListItem { Text = "0", Value = "0" }));
                    var planselected = new SelectList(plandenominations, "Value", "Text", oNP.denomination_ids[i].deno_id);
                    ViewData["ddlPLAN_" + i] = planselected;
                    long _den_id = oNP.denomination_ids[i].deno_id;
                    var _pprice = Plans.Where(p => p.Denomination_id == _den_id).FirstOrDefault();
                    if (_pprice != null)
                        totalprice += Convert.ToDecimal(_pprice.price);
                    i++;
                }

                oNP.tot_amt = totalprice / 100;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("_bm_yplans", oNP);
        }

        public ActionResult ajax_GetYPlanUsage(string msisdn)
        {
            var YPlanUsage = new List<YPlanUsageModel>();
            try
            {
                YPlanUsage = _plan_repo.get_yourplan_usage(msisdn);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("_bm_yplan_usage", YPlanUsage);
        }

        public ActionResult ajax_GetPlansbyDenominations(long[] dnoIDs, long typeid, string jsonPlans)
        {
            NewPurchasePriceModel oNP = new NewPurchasePriceModel();
            try
            {

                var Plans = JsonConvert.DeserializeObject<List<YPriceModel>>(jsonPlans);
                List<plandetails> objplandeno_ids = new List<plandetails>();
                RegistrationModel oRM = new RegistrationModel();
                Plans = Plans.Where(p => p.plan_type_id == typeid).ToList();
                oNP.jsonallplans = jsonPlans;

                if (oNP.denomination_ids == null)
                {
                    oNP.denomination_ids = _plan_repo.GetPlans(Plans);
                }

                decimal totalprice = 0;
                int i = 0;
                if (dnoIDs.Length > 0)
                {
                    foreach (var ind_plan in oNP.denomination_ids)
                    {
                        var plandenominations = (from v in Plans where v.plan_id == ind_plan.plan_id select new SelectListItem { Text = v.denomination.ToString() + " " + v.denomination_name, Value = v.Denomination_id.ToString() }).ToList();

                        oNP.denomination_ids[i].deno_id = dnoIDs[i];

                        //plandenominations.Insert(0, (new SelectListItem { Text = "0", Value = "0" }));
                        var planselected = new SelectList(plandenominations, "Value", "Text", oNP.denomination_ids[i].deno_id);
                        ViewData["ddlPLAN_" + i] = planselected;
                        long _den_id = oNP.denomination_ids[i].deno_id;
                        var _pprice = Plans.Where(p => p.Denomination_id == _den_id).FirstOrDefault();
                        if (_pprice != null)
                            totalprice += Convert.ToDecimal(_pprice.price);
                        i++;
                    }
                }

                oNP.tot_amt = totalprice / 100;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return PartialView("_bm_yplans", oNP);
        }

        public ActionResult aj_GetPlanPrice(long[] plan, long typeID, string jallPlans)
        {
            int iRet = -1;
            decimal tot = 0;
            try
            {
                if (plan.Length > 0)
                {
                    var price = JsonConvert.DeserializeObject<List<YPriceModel>>(jallPlans);
                    for (int i = 0; i < plan.Length; i++)
                    {
                        var Res = price.Where(t => t.Denomination_id == plan[i]).FirstOrDefault();
                        if (Res != null)
                        {
                            tot += Convert.ToDecimal(Res.price);
                        }
                    }
                }
                if (typeID == 1)
                {
                    if (tot == 0)
                        iRet = -115;
                    else
                        iRet = 0;
                }

                tot = tot / 100;
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            return Json(new { TPrice = tot, RetVal = iRet });
        }

        [HttpPost]
        public ActionResult PlanPricing(NewPurchasePriceModel objPurchasePlan)
        {
            ViewBag.Valid = "false";
            string[] denominationIDs = objPurchasePlan.plan_denominations.Split(',');

            List<plandetails> denominations = new List<plandetails>();
            int i = 0;
            foreach (var item in objPurchasePlan.denomination_ids)
            {
                plandetails obj = new plandetails();
                obj.deno_id = Convert.ToInt64(denominationIDs[i]);
                obj.plan_id = item.plan_id;
                obj.plan_name = item.plan_name;
                denominations.Add(obj);
                i += 1;
            }
            objPurchasePlan.denomination_ids = denominations;

            Session["YourPlanPrice"] = objPurchasePlan;
            if (Session["subscriber"] != null)
            {
                string sUsername = Session["subscriber"].ToString();
                AccountModel oAM = new AccountModel();
                //oAM = _care_repo.GetMultipleSubscribers(sUsername);
                oAM = JsonConvert.DeserializeObject<AccountModel>(objPurchasePlan.jsonSubs);
                if (oAM != null)
                {
                    string _msisdnno = objPurchasePlan.purchase_msisdn;
                    string expdate = "";
                    // string expdate = oAM.Subr.Where(S => S._MSISDNNumber == _msisdnno).FirstOrDefault().YPlans.FirstOrDefault().expiry;

                    var yPlanSubs = oAM.Subr.Where(s => s._MSISDNNumber == sUsername).FirstOrDefault();
                    if (yPlanSubs != null)
                    {
                        if (yPlanSubs.YPlans != null)
                        {
                            var ypExp = yPlanSubs.YPlans.FirstOrDefault();
                            if (ypExp != null)
                            {
                                expdate = ypExp.expiry;
                            }
                        }
                    }

                    if (objPurchasePlan.isFixedbuy == true)
                    {
                        DateTime dtExpiry = Convert.ToDateTime(expdate);
                        if (DateTime.Now.Date < dtExpiry.Date)
                        {
                            return RedirectToAction("YPlanConfirm");
                        }
                    }
                    else
                    {
                        bool IsypExpired = _plan_repo.IsypExpired(_msisdnno);//true; 
                        if (IsypExpired == false)
                        {
                            ViewBag.Valid = "true";

                            ViewBag.NotifMsg = "You are unable to purchase a new plan as you have a current 30 days plan that will expire on " + expdate + ".  You can choose from our available <b><a href='/Care/Plans'>Promotions</a></b>.";
                            //objPurchasePlan.denomination_ids = null;                        
                            return View(objPurchasePlan);
                        }
                        else
                            return RedirectToAction("YPlanConfirm");
                    }

                }

                //objPurchasePlan.denomination_ids = null; 
                return View(objPurchasePlan);
            }
            else
            {

                return RedirectToAction("Login");
            }

        }

        public ActionResult SkipAndContinue()
        {
            if (Session["subscriber"] != null)
            {
                NewPurchasePriceModel objPurchasePlan = (NewPurchasePriceModel)Session["YourPlanPrice"];
                objPurchasePlan.displayPromotion = _Hide;
                Session["YourPlanPrice"] = objPurchasePlan;

                return Json(new { Status = "true" });
            }
            else
                return Json(new { Status = "false" });
        }

        public ActionResult YPlanConfirm()
        {
            string sMsg = "";
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");
            NewPurchasePriceModel oNP = new NewPurchasePriceModel();
            try
            {
                if (Session["YourPlanPrice"] == null)
                    return RedirectToAction("PlanPricing");

                oNP = (NewPurchasePriceModel)(Session["YourPlanPrice"]);
                string sUsername = Session["subscriber"].ToString();

                AccountModel oAM = new AccountModel();
                oAM = _care_repo.GetMultipleSubscribers(sUsername);
                if (oAM != null)
                {
                    PaymentPGModel oPay = new PaymentPGModel();
                    oPay.email = oAM.Reg.Email;
                    oPay.fname = oAM.Reg.FirstName;
                    oPay.lname = oAM.Reg.LastName;
                    oPay.paid_userid = oAM.Reg.UserId;

                    if (oAM.Reg.paidType == "502")
                    {
                        ViewBag.posType = "502";
                        oNP.po_type = "502";
                    }
                    else
                    {
                        ViewBag.preType = "501";
                        oNP.pr_type = "501";
                    }

                    if (oAM.Subr.Count > 1)
                    {
                        ViewBag.preType = "501";
                        oNP.pr_type = "501";
                    }

                    if (ViewBag.posType == "502" && ViewBag.preType == null)
                        ViewBag.Message = "Not Applicable for a Postpaid user";

                    var primaryMsisdn = oAM.Subr.Where(p => p.isPrimary == true).FirstOrDefault();
                    if (primaryMsisdn != null)
                        oNP.from_msisdn = primaryMsisdn._MSISDNNumber;
                    oPay.primary_msisdn = oNP.from_msisdn;

                    oNP.pay = oPay;
                    oNP.UserId = oAM.Reg.UserId;
                    var MsisdnBalList = (from Bal in oAM.Subr
                                         select new MSISDNBalanceModel
                                         {
                                             msisdn_no = Bal._MSISDNNumber,
                                             balance = Bal._sbalance,
                                             YPlanList = Bal.YPlans
                                         }).ToList();
                    oNP.jsonallmsisdn = JsonConvert.SerializeObject(MsisdnBalList);

                    BundlesModel oBM = new BundlesModel();
                    var user = oAM.Subr.Where(u => u._MSISDNNumber == sUsername).FirstOrDefault();
                    if (user != null)
                    {
                        double dBalance = user.balance;
                        long regId = oAM.Reg.UserId;
                        var userbundles = _care_repo.GetBundles(regId.ToString(), dBalance).Where(b => b.isActive == true && b.isDeleted == false).ToList();
                        if (userbundles.Count > 0)
                        {
                            oBM.Voice = _care_repo.GetVoicePlans(userbundles, user.planDetails, dBalance).ToList();
                            oNP.Bundles = oBM;
                        }
                    }

                    oNP.YMSISDNlst = FillMSISDN(oAM.Subr, "");
                    if (oNP.YMSISDNlst.Count == 1)
                    {
                        oNP.purchase_msisdn = oNP.YMSISDNlst[0].Text;
                        oNP.YMSISDNlst = null;
                    }

                    long ldenom_id = 0;
                    decimal tot_price = 0;
                    List<plandetails> objdenom = new List<plandetails>();
                    var plans = JsonConvert.DeserializeObject<List<YPriceModel>>(oNP.jsonallplans);
                    string[] denominationIDs = oNP.plan_denominations.Split(',');
                    if (denominationIDs.Length > 0)
                    {
                        foreach (var di in denominationIDs)
                        {
                            ldenom_id = Convert.ToInt64(di);
                            plandetails obj = new plandetails();
                            var Res = plans.Where(p => p.Denomination_id == ldenom_id).FirstOrDefault();
                            if (Res != null)
                            {
                                obj.plan_id = Res.plan_id;
                                obj.deno_id = Res.Denomination_id;
                                obj.denomination = Res.denomination;
                                obj.denomination_name = Res.denomination_name;
                                obj.plan_name = Res.planName;
                                obj.denomination_name = Res.denomination_name;
                                obj.plan_type_id = Res.plan_type_id;
                                obj.price = Res.price;
                                tot_price += Convert.ToDecimal(Res.price);
                                objdenom.Add(obj);
                                oNP.type_id = obj.plan_type_id;
                            }

                        }
                        oNP.tot_amt = tot_price / 100;
                    }
                    oNP.denomination_ids = objdenom;

                }

                Session.Remove("YourPlanPrice");

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View(oNP);
        }

        [HttpPost]
        public ActionResult YPlanConfirm(NewPurchasePriceModel objPurchasePlan)
        {
            bool IsypExpired = false;
            int Res = -1;
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");
            else
            {
                try
                {
                    if (objPurchasePlan.pr_type == "501")
                        ViewBag.preType = "501";
                    if (objPurchasePlan.pr_type == "502")
                        ViewBag.posType = "502";

                    PurchasePlanModel oPP = new PurchasePlanModel();
                    if (objPurchasePlan != null)
                    {
                        IsypExpired = _plan_repo.IsypExpired(objPurchasePlan.purchase_msisdn); //true;
                        string _msisdnno = objPurchasePlan.purchase_msisdn;

                        oPP.from_msisdn = objPurchasePlan.from_msisdn;
                        oPP.purchase_msisdn = objPurchasePlan.purchase_msisdn;
                        oPP.tot_amt = objPurchasePlan.tot_amt;
                        oPP.isTopUp = objPurchasePlan.isFixedbuy;
                        List<Purchasedenomination> lstden = new List<Purchasedenomination>();
                        if (objPurchasePlan.denomination_ids.Count > 0)
                        {

                            foreach (var itm in objPurchasePlan.denomination_ids)
                            {
                                Purchasedenomination obj = new Purchasedenomination();
                                obj.denomination_id = itm.deno_id;
                                obj.plan_name = itm.plan_name + " : " + itm.denomination + " " + itm.denomination_name;
                                obj.price = Convert.ToDouble(itm.price);
                                lstden.Add(obj);
                            }
                        }

                        List<Purchasedpromotion> lstPromotions = new List<Purchasedpromotion>();
                        //if (objPurchasePlan.Bundles.Voice.Count() > 0)
                        //{
                        //    var objBundles = objPurchasePlan.Bundles.Voice.Where(b => b.isChecked == true).ToList();
                        //    foreach (var item in objBundles)
                        //    {
                        //        Purchasedpromotion objPromo = new Purchasedpromotion();
                        //        objPromo.promotion_id = item.Id;
                        //        objPromo.plan_name = item.PlanName;
                        //        objPromo.price = item.Price;
                        //        lstPromotions.Add(objPromo);
                        //    }
                        //}
                        oPP.pDenomIds = lstden;
                        oPP.PromotionIds = lstPromotions;
                        oPP.isMobile = objPurchasePlan.type_id.ToString();


                        if (objPurchasePlan.isFixedbuy == true)
                        {
                            if (objPurchasePlan.Payment_Id == 1)
                            {
                                Res =_plan_repo.purchase_plan(oPP);
                                if (Res == 0)
                                {
                                    SendOrderSummary_email(oPP);
                                    Session["purchase_plan"] = oPP;
                                }
                                return RedirectToAction("YMessage", new { id = Res });
                            }
                            else if (objPurchasePlan.Payment_Id == 2)
                            {
                                //string jsonPurchanseplan = JsonConvert.SerializeObject(oPP);
                                Session["purchase_plan"] = oPP;
                                return RedirectToAction("Topup");
                            }
                            else if (objPurchasePlan.Payment_Id == 3)
                            {
                                //Session["purchase_plan"] = oPP;
                                PaymentEntModel pay = new PaymentEntModel();
                                pay.email = objPurchasePlan.pay.email;
                                pay.fname = objPurchasePlan.pay.fname;
                                pay.lname = objPurchasePlan.pay.lname;

                                pay.paid_for_msisdn = oPP.purchase_msisdn;
                                pay.paid_userid = objPurchasePlan.UserId;
                                pay.sess_id = Session.SessionID;
                                pay.primary_msisdn = objPurchasePlan.pay.primary_msisdn;
                                pay.payment_mode = 2;
                                pay.payment_gateway = "DOKU";
                                pay.payment_type = "CREDITCARD";
                                pay.payment_receipt_no = objPurchasePlan.isFixedbuy.ToString();//this is for byp topup

                                List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();
                                decimal dtotal_amount = 0;
                                foreach (var bitem in lstden)
                                {
                                    tbl_doku_order_item obypitem = new tbl_doku_order_item();
                                    obypitem.product_id = bitem.denomination_id;
                                    obypitem.product_name = bitem.plan_name;
                                    obypitem.product_price = (Convert.ToDecimal(bitem.price) / 100);
                                    obypitem.product_qty = 1;
                                    obypitem.purchase_desc = "BYP";
                                    dtotal_amount += obypitem.product_price;
                                    order_items.Add(obypitem);
                                }
                                pay.paid_amount = dtotal_amount.ToString("#0.00");
                                if (order_items.Count > 0)
                                    pay.order_items = order_items.ToArray();

                                long retOrderId = _doku_client.save_selfcare_order(pay);
                                if (retOrderId > 0)
                                {
                                    long dokuId = _doku_client.save_doku_order_payment(pay, retOrderId, 1);
                                    if (dokuId > 0)
                                    {
                                        temp_orders_model oT = new temp_orders_model();
                                        oT.Id = 0;
                                        oT.Order_Id = retOrderId;
                                        oT.Session_Id = Session.SessionID;
                                        oT.SiteId = 1;
                                        string sJStemp = JsonConvert.SerializeObject(oT);
                                        long TempOrderId = _doku_client.save_temp_order(sJStemp);
                                        if (TempOrderId > 0)
                                        {
                                            string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                            return Redirect(dk_process_url + "/" + oT.Session_Id);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Message = "Failed to Proceed..";
                                        return View(objPurchasePlan);
                                    }

                                }








                            }
                        }
                        else if (IsypExpired == true)
                        {
                            if (objPurchasePlan.Payment_Id == 1)
                            {
                                Res = _plan_repo.purchase_plan(oPP);
                                if (Res == 0)
                                {
                                    SendOrderSummary_email(oPP);
                                    Session["purchase_plan"] = oPP;
                                }
                                return RedirectToAction("YMessage", new { id = Res });
                            }
                            else if (objPurchasePlan.Payment_Id == 2)
                            {
                                //string jsonPurchanseplan = JsonConvert.SerializeObject(oPP);
                                Session["purchase_plan"] = oPP;
                                return RedirectToAction("Topup");
                            }
                            else if (objPurchasePlan.Payment_Id == 3)
                            {


                                PaymentEntModel pay = new PaymentEntModel();
                                pay.email = objPurchasePlan.pay.email;
                                pay.fname = objPurchasePlan.pay.fname;
                                pay.lname = objPurchasePlan.pay.lname;

                                pay.paid_for_msisdn = oPP.purchase_msisdn;
                                pay.paid_userid = objPurchasePlan.UserId;
                                pay.sess_id = Session.SessionID;
                                pay.primary_msisdn = objPurchasePlan.pay.primary_msisdn;
                                pay.payment_mode = 2;
                                pay.payment_gateway = "DOKU";
                                pay.payment_type = "CREDITCARD";
                                pay.payment_receipt_no = objPurchasePlan.isFixedbuy.ToString();//this is for byp topup

                                List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();
                                decimal dtotal_amount = 0;
                                foreach (var bitem in lstden)
                                {
                                    tbl_doku_order_item obypitem = new tbl_doku_order_item();
                                    obypitem.product_id = bitem.denomination_id;
                                    obypitem.product_name = bitem.plan_name;
                                    obypitem.product_price = (Convert.ToDecimal(bitem.price) / 100);
                                    obypitem.product_qty = 1;
                                    obypitem.purchase_desc = "BYP";
                                    dtotal_amount += obypitem.product_price;
                                    order_items.Add(obypitem);
                                }
                                pay.paid_amount = dtotal_amount.ToString("#0.00");
                                if (order_items.Count > 0)
                                    pay.order_items = order_items.ToArray();

                                long retOrderId = _doku_client.save_selfcare_order(pay);
                                if (retOrderId > 0)
                                {
                                    long dokuId = _doku_client.save_doku_order_payment(pay, retOrderId, 1);
                                    if (dokuId > 0)
                                    {
                                        temp_orders_model oT = new temp_orders_model();
                                        oT.Id = 0;
                                        oT.Order_Id = retOrderId;
                                        oT.Session_Id = Session.SessionID;
                                        oT.SiteId = 1;
                                        string sJStemp = JsonConvert.SerializeObject(oT);
                                        long TempOrderId = _doku_client.save_temp_order(sJStemp);
                                        if (TempOrderId > 0)
                                        {
                                            string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                            return Redirect(dk_process_url + "/" + oT.Session_Id);
                                        }
                                    }
                                    else
                                    {
                                        ViewBag.Message = "Failed to Proceed..";
                                        return View(objPurchasePlan);
                                    }

                                }

                            }
                        }
                        else
                        {

                            var oBal = JsonConvert.DeserializeObject<List<MSISDNBalanceModel>>(objPurchasePlan.jsonallmsisdn);
                            oBal = oBal.Where(P => P.msisdn_no == _msisdnno).ToList();
                            string expdate = oBal.FirstOrDefault().YPlanList.FirstOrDefault().expiry;
                            ViewBag.Message = "Your existing 30 days plan has not yet expired! It will expire on " + expdate;
                        }
                    }
                }
                catch (Exception ex)
                {
                     _util_repo.ErrorLog_Txt(ex);
                }
                return View(objPurchasePlan);
            }

        }

        private void SendOrderSummary_email(PurchasePlanModel obj_plans)
        {
            if (obj_plans != null)
            {
                XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                XElement emailsubj = doc.Element("subj_ACC_bal_ordersummary");
                XElement emailBody = doc.Element("body_ACC_bal_ordersummary");
                XElement emailMain = doc.Element("main_ACC_bal_ordersummary");
                XElement emailMain_Total = doc.Element("main_total_ACC_bal_ordersummary");
                string sData = emailBody.Value;
                string sMain = emailMain.Value;
                string sMain_Total = emailMain_Total.Value;
                string sbmobileurl = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                sData = sData.Replace("#bmobile_url#", sbmobileurl).Replace("#payment_date#", DateTime.Now.ToLongDateString());

                if (obj_plans.pDenomIds.Count > 0)
                {
                    decimal dtotal = 0;
                    for (int i = 0; i < obj_plans.pDenomIds.Count; i++)
                    {
                        int sno = 0;
                        sno = i + 1;
                        if (obj_plans.pDenomIds[i].price > 0)
                            dtotal = Convert.ToDecimal(obj_plans.pDenomIds[i].price);

                        var item_price = obj_plans.pDenomIds[i].price / 100;

                        sMain = sMain.Replace("#sno#", sno.ToString()).Replace("#plan_name#", obj_plans.pDenomIds[i].plan_name).Replace("#price#", item_price.ToString("#0.00"));
                        sData += sMain;
                        sMain = emailMain.Value;
                    }
                    sMain_Total = sMain_Total.Replace("#Order_total#", obj_plans.tot_amt.ToString("#0.00"));
                    sData += sMain_Total;

                    string email = _care_repo.GetMultipleSubscribers(obj_plans.from_msisdn).Reg.Email;
                    //obj_plans.from_msisdn

                    if (!string.IsNullOrEmpty(email))
                        _util_repo.SendEmailMessage(email, emailsubj.Value.Trim(), sData);

                }

            }

        }


        public ActionResult testpurchase()
        {
            List<Purchasedenomination> pDenomIds = new List<Purchasedenomination>();

            Purchasedenomination o = new Purchasedenomination();
            o.denomination_id = 6;
            pDenomIds.Add(o);

            List<Purchasedpromotion> PromotionIds = new List<Purchasedpromotion>();
            Purchasedpromotion p = new Purchasedpromotion();
            p.promotion_id = 1210;
            //PromotionIds.Add(p);

            decimal tot_amt = 1; ;

            string from_msisdn = "6778720658";

            string purchase_msisdn = "6778720659";

            string isMobile = "1";

            PurchasePlanModel test = new PurchasePlanModel();
            test.from_msisdn = from_msisdn;
            test.isMobile = isMobile;
            test.pDenomIds = pDenomIds;
            test.PromotionIds = PromotionIds;
            test.purchase_msisdn = purchase_msisdn;
            test.tot_amt = tot_amt;

            // int i = _plan_repo.purchase_planDoku(test);



            return View();
        }

        public ActionResult YMessage(string id)
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");
            string sMessage = "";
            try
            {
                AccountModel oAM = new AccountModel();
                oAM = _care_repo.GetMultipleSubscribers(Session["subscriber"].ToString());
                if (oAM != null)
                {
                    if (oAM.Reg.paidType == "502")
                        ViewBag.posType = "502";
                    else
                        ViewBag.preType = "501";

                    if (oAM.Subr.Count > 1)
                        ViewBag.preType = "501";
                }
                if (id == "0")
                {
                    var oPP = (PurchasePlanModel)Session["purchase_plan"];
                    if (oPP != null)
                    {
                        Session.Remove("purchase_plan");
                        NewPurchasePriceModel oNP = new NewPurchasePriceModel();

                        decimal tot_price = 0;
                        List<plandetails> objdenom = new List<plandetails>();

                        string expdate = "";

                        var yPlanSubs = oAM.Subr.Where(s => s._MSISDNNumber == oNP.purchase_msisdn).FirstOrDefault();
                        if (yPlanSubs != null)
                        {
                            if (yPlanSubs.YPlans != null)
                            {
                                var ypExp = yPlanSubs.YPlans.FirstOrDefault();
                                if (ypExp != null)
                                {
                                    expdate = ypExp.expiry;
                                    ViewBag.ExpyDate = Convert.ToDateTime(expdate).ToLongDateString();
                                    //ViewBag.ExpyDate = DateTime.Now.ToLongDateString();
                                }
                            }
                        }

                        foreach (var item in oPP.pDenomIds)
                        {
                            plandetails obj = new plandetails();
                            if (item != null)
                            {
                                obj.deno_id = item.denomination_id;
                                obj.plan_name = item.plan_name;
                                obj.price = Convert.ToDecimal(item.price);
                                tot_price += obj.price;
                                objdenom.Add(obj);
                            }
                        }
                        oNP.tot_amt = tot_price / 100;
                        oNP.denomination_ids = objdenom;
                        return View(oNP);
                    }
                    //sMessage = "Plan Purchased Successfully";
                }
                else if (id == "960")
                    sMessage = "You have Insufficient balance to buy this plan";
                else
                    sMessage = "Failed to Process!";
                ViewBag.SMessage = sMessage;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult YPlanSlider()
        {
            string yplan_url = ConfigurationManager.AppSettings["yplan_url"].ToString();
            return Redirect(yplan_url);
        }

        #region Add Favourite


        public ActionResult AddFavouritePlanPack(string denm_ids, string jallplans)
        {
            RegistrationModel oRM = new RegistrationModel();
            int iRet = -1;
            long ldenom_id = 0; ;
            string[] denominations = denm_ids.Split(',');
            if (Session["subscriber"] != null)
            {
                string sUsername = Session["subscriber"].ToString();
                oRM = _care_repo.GetRegDetails(sUsername);
                if (oRM != null)
                {

                    List<plandetails> objdenom = new List<plandetails>();
                    var plans = JsonConvert.DeserializeObject<List<YPriceModel>>(jallplans);
                    foreach (var di in denominations)
                    {
                        ldenom_id = Convert.ToInt64(di);
                        plandetails obj = new plandetails();
                        var Res = plans.Where(p => p.Denomination_id == ldenom_id).FirstOrDefault();
                        if (Res != null)
                        {
                            obj.plan_id = Res.plan_id;
                            obj.deno_id = Res.Denomination_id;
                            obj.plan_type_id = Res.plan_type_id;
                            objdenom.Add(obj);
                        }
                    }
                    long UserId = oRM.UserId;
                    if (UserId != 0)
                        iRet = _plan_repo.AddFavouritePlan(UserId, objdenom);
                    else
                        iRet = 111;
                }
            }
            return Json(new { Status = iRet });
        }


        #endregion

        #endregion

        #region Redeem FB Voucher

        public ActionResult RedeemFBPromotions()
        {
            RedeemFBPromotionsModel RedeemFB = new RedeemFBPromotionsModel();
            try
            {
                if (Session["subscriber"] != null)
                {
                    string sMSISDN = Session["subscriber"].ToString();
                    AccountModel oAM = new AccountModel();
                    oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                    if (oAM != null)
                    {
                        if (oAM.Subr.Count > 0)
                        {
                            RedeemFB.msisdn_list = FillMSISDN(oAM.Subr, "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return PartialView("_FBPromotionsPartial", RedeemFB);
        }

        public ActionResult RedeemFBPromotions2(string fbvoucher, string msisdnno)
        {
            string _iRes = "Fail";
            decimal Amt = 0;
            try
            {
                if (Session["subscriber"] != null)
                {
                    string parent_msisdn = Session["subscriber"].ToString();
                    
                    List<web_tbl_fb_attempt_log> obj_fb_attempt_log = UOW.fb_attempt_log_repo.Get(filter: f => f.msisdn_no == parent_msisdn && f.fb_voucher == fbvoucher).ToList();
                    int attempt_count = obj_fb_attempt_log.Count;
                    if (attempt_count < 3)
                    {
                        web_tbl_fb_promotions fb_promotion = UOW.redeem_fb_promotions_repo.Get(filter: f => f.fb_vouchers == fbvoucher && f.is_active == true && f.is_deleted == false).FirstOrDefault();
                        if (fb_promotion != null)
                        {
                            RedeemFBPromotionsModel RedeemFB = new RedeemFBPromotionsModel();
                            RedeemFB.msisdn_no = msisdnno;
                            RedeemFB.serial_number = fb_promotion.serial_number;
                            RedeemFB.pin_number = fb_promotion.pin_number;
                            RedeemFB.amount = fb_promotion.amount;

                            string dRes = _care_repo.RedeemFBPromotion_GetVoucher(RedeemFB);
                            var dataop = JsonConvert.DeserializeObject<FBPromotionOutputModel>(dRes);
                            if (dataop.resultcode == 0)
                            {
                                dRes = _care_repo.RedeemFBPromotion_TopupVoucher(RedeemFB);
                                if (!string.IsNullOrEmpty(dRes) && dRes.ToLower().Contains("operation successfully"))
                                {
                                    _iRes = "Success";
                                    Amt = fb_promotion.amount;

                                    fb_promotion.is_active = false;
                                    UOW.redeem_fb_promotions_repo.Update(fb_promotion);
                                    UOW.Save();
                                }
                            }
                            else
                            {
                                _iRes = dataop.desc;
                            }
                        }
                        else
                        {
                            web_tbl_fb_attempt_log obj = new web_tbl_fb_attempt_log();
                            obj.Id = 0;
                            obj.msisdn_no = parent_msisdn;
                            obj.fb_voucher = fbvoucher;
                            obj.created_on = DateTime.Now;

                            UOW.fb_attempt_log_repo.Insert(obj);
                            UOW.Save();

                            if ((attempt_count + 1) >= 3)
                                _iRes = "AttemptFail";
                        }
                    }
                    else
                    {
                        _iRes = "AttemptFail";
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return Json(new { Status = _iRes, Amount = Amt }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Voice Call Activation Deactivation

        public ActionResult GetVoiceCallActDeact()
        {
            VoiceCallActDeactModel objVC = new VoiceCallActDeactModel();
            try
            {
                if (Session["subscriber"] != null)
                {
                    string sMSISDN = Session["subscriber"].ToString();
                    AccountModel oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                    if (oAM != null)
                    {
                        if (oAM.Subr.Count > 0)
                        {
                            objVC.msisdn_list = FillMSISDN(oAM.Subr, "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return PartialView("_VoiceCall_ActDeact", objVC);
        }

        public ActionResult VoiceCallActDeact(string msisdnno, string vc_type)
        {
            string _iRes = "";
            try
            {
                if (Session["subscriber"] != null)
                {
                    VoiceCallActDeactModel objVC = new VoiceCallActDeactModel();
                    objVC.msisdn_no = msisdnno;
                    objVC.vc_type = vc_type;

                    _iRes = _care_repo.VoiceCallActivationDeactivation(objVC);
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return Json(new { Status = _iRes }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult testmail()
        {
            try
            {
                _util_repo.SendEmailMessage(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "test from si", "test si email success");
            }
            catch (Exception ex)
            {
                return Content(ex.Message + "<br>" + ex.StackTrace);
            }
            return Content("DONE");
        }

        public ActionResult testgmail()
        {
            try
            {
                _util_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "test from gmail acc", "gmail message");


            }
            catch (Exception ex)
            {

                return Content(ex.Message + "<br>" + ex.StackTrace);
            }
            return Content("DONE");
        }


        #region BYP Report

        private List<SelectListItem> FillOrderStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = _doku_client.OrderStatus().ToList();
            if (res.Count > 0)
            {
                
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i], Value = res[i].ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        private List<SelectListItem> FillDokuStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = _doku_client.DokuOrderStatus().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i], Value = res[i].ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        //private List<SelectListItem> FillPayType()
        //{
        //    List<SelectListItem> result = new List<SelectListItem>();
        //    var res = UOW.selfcare_order_payment_repo.Get().ToList();
        //    if (res.Count > 0)
        //    {
        //        var payType = (from s in res select new { s.payment_type }).Distinct().ToList();
        //        for (int i = 0; i < payType.Count; i++)
        //        {
        //            var item = new SelectListItem { Text = payType[i].payment_type, Value = payType[i].payment_type.ToString() };
        //            result.Add(item);
        //        }
        //    }
        //    return result;
        //}

        public ActionResult BYPTransactions(string transmerchantid, string email, string ddlDokuStatus, string sFrom, string sTo, int? page)
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<DokuCareModel> oD = new List<DokuCareModel>();
            ViewData["transmerchantid"] = transmerchantid;
            ViewData["email"] = email;
            ViewData["ddlDokuStatus"] = ddlDokuStatus;
            ViewData["sFrom"] = sFrom;
            ViewData["sTo"] = sTo;
            try
            {
                ViewBag.Orderstatuslist = FillOrderStatus();
                ViewBag.Dokustatuslist = FillDokuStatus();
                //ViewBag.PayTypelist = FillPayType();

                string user_msisdn = Session["subscriber"].ToString();
                string sTransactions = _doku_client.get_doku_order_transactionsByPurchase(1, "BYP",sFrom,sTo);
                oD = JsonConvert.DeserializeObject<List<DokuCareModel>>(sTransactions).ToList();

                if (oD.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(transmerchantid))
                        oD = oD.Where(d => d.doku.transidmerchant == transmerchantid).ToList();

                    if (!string.IsNullOrWhiteSpace(email))
                        oD = oD.Where(d => d.order.cust_email == email).ToList();


                    if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
                        oD = oD.Where(d => d.doku.resultmsg == ddlDokuStatus).ToList();



                    if ((!string.IsNullOrWhiteSpace(sFrom)) && (!string.IsNullOrWhiteSpace(sTo)))
                    {
                        DateTime dtFrom = DateTime.ParseExact(sFrom, "MM/dd/yyyy", null);
                        DateTime dtTo = DateTime.ParseExact(sTo, "MM/dd/yyyy", null);
                        oD = oD.Where(t => t.doku.created_on.Date >= dtFrom && t.doku.created_on <= dtTo).ToList();
                    }

                    if (oD.Count > 0)
                        TempData["BYPTransactionList"] = oD;
                    oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oD.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "No Records found!";
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_BYPTransactions", oD);
                else
                    return View(oD);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oD);
        }



        #endregion

        #region Get Power

        public ActionResult GetPower()
        {
            return PartialView("_GetPower");
        }

        public JsonResult GetPowerCheck(string meterno, string amount)
        {
            string ResCode = "-555";
            string ResMsg = string.Empty;
            try
            {
                if (Session["subscriber"] != null)
                {

                    long lminLimit = Convert.ToInt64(ConfigurationManager.AppSettings["gp_minlimit"]);
                    long lmaxLimit = Convert.ToInt64(ConfigurationManager.AppSettings["gp_maxlimit"]);


                    long l_Amount = 0;
                    bool bdValidate = Int64.TryParse(amount, out l_Amount);
                    if (!bdValidate)
                    {
                        ResCode = "-76";
                        ResMsg = "Amount should not be in decimal";

                    }
                    else
                    {
                        if (Convert.ToInt64(amount) <= lmaxLimit && Convert.ToInt64(amount) >= lminLimit)
                        {
                            string msisdn_no = Session["subscriber"].ToString();
                            GetPowerModelIP buypower = new GetPowerModelIP();
                            buypower.sUserName = gp_java_uid;
                            buypower.sPassword = gp_java_pwd;
                            buypower.sMeterNo = meterno;
                            buypower.dAmount = Convert.ToInt64(amount);
                            buypower.sMSISDN = msisdn_no;
                            buypower.sKeyCode = gp_merc_keycode;
                            buypower.sIPAddress = "172.16.23.186";
                            buypower.iProcessMode = 0;
                            buypower.user_id = 0;

                            string ipdata = JsonConvert.SerializeObject(buypower);
                            string enc_ipdata = _util_repo.AES_ENCWR(ipdata);

                            string enc_uid = _util_repo.AES_ENCWR(gp_svc_uid);
                            string enc_pwd = _util_repo.AES_ENCWR(gp_svc_pwd);
                            ResCode = _esipay.CheckCustomer(enc_uid, enc_pwd, ipdata);

                            int rcode;
                            bool res = int.TryParse(ResCode, out rcode);
                            if (res == false)
                            {
                                string xmldata = ResCode;
                                //using (StreamReader sr = new StreamReader(Server.MapPath("~/EmailTemplate/customer.xml")))
                                //{
                                //    xmldata = sr.ReadToEnd();
                                //}
                                xmldata = ReplaceSPLStr(xmldata);
                                if (!string.IsNullOrEmpty(xmldata))
                                {

                                    if (ValidateXML(xmldata))
                                    {

                                        string name = "";
                                        bool bsuccess = false;
                                        System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();

                                        xDoc.LoadXml(xmldata);
                                        System.Xml.XmlNodeList xNodelst = xDoc.SelectNodes("//suprima/thin-client/consumer-chk/token");
                                        if (xNodelst.Count > 0)
                                        {
                                            System.Xml.XmlNode xNode = xNodelst.Item(0);
                                            for (int i = 0; i < xNode.ChildNodes.Count; i++)
                                            {
                                                if (xNode.ChildNodes[i].Name.ToLower() == "tk1")
                                                {
                                                    name += " " + xNode.ChildNodes[i].InnerText.Trim();
                                                    bsuccess = true;
                                                }
                                                else if (xNode.ChildNodes[i].Name.ToLower() == "tk2")
                                                {
                                                    name += " " + xNode.ChildNodes[i].InnerText.Trim();
                                                    bsuccess = true;
                                                }
                                                else if (xNode.ChildNodes[i].Name.ToLower() == "tk3")
                                                {
                                                    name += " " + xNode.ChildNodes[i].InnerText.Trim();
                                                    bsuccess = true;
                                                }
                                            }

                                            if (bsuccess == true)
                                            {
                                                string trans_fee = "";
                                                long amount_toea = Convert.ToInt64(Convert.ToDecimal(amount) * 100);
                                                var vTranFee = UOW.transaction_fees_repo.Get(filter: t => t.max >= amount_toea && t.min <= amount_toea).ToList();
                                                if (vTranFee.Count > 0)
                                                    trans_fee = (Convert.ToDecimal(vTranFee.FirstOrDefault().fee) / 100).ToString();

                                                ResCode = "0";
                                                ResMsg = "You are purchasing K" + Convert.ToDecimal(amount).ToString("#0.00") + " for meter no " + name + ".A fee of K" + Convert.ToDecimal(trans_fee).ToString("#0.00") + " will be charged. Do you want to continue?";
                                            }

                                        }
                                    }
                                    else
                                    {
                                        ResCode = "-1";
                                        ResMsg = xmldata;
                                    }
                                }
                            }
                            else
                                ResCode = rcode.ToString();

                        }
                        else
                        {
                            ResCode = "-77";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return Json(new { rcode = ResCode, rmsg = ResMsg }, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateXML(string text)
        {
            bool bRes = true;
            Regex tagsWithData = new Regex("<\\w+>[^<]+</\\w+>");
            if (string.IsNullOrEmpty(text) || tagsWithData.IsMatch(text) == false)
            {
                bRes = false;
            }
            return bRes;
               
        }

        private string ReplaceSPLStr(string sString)
        {
            string sRes = sString.Replace("&", "");
            sRes = sRes.Replace("'", "");
            return sRes;
        }

        #region Order Summary

        public ActionResult OrderSummary(string id)
        {

            //if (Session["subscriber"] == null)
            //    return RedirectToAction("Login");

            DokuCareModel oDCM = new DokuCareModel();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {


                    long n_out;
                    bool isNumeric = long.TryParse(id, out n_out);

                    if (isNumeric)
                    {
                        long orderId = n_out;

                        string sjRes = _doku_client.get_doku_order_byid(orderId);

                        oDCM = JsonConvert.DeserializeObject<DokuCareModel>(sjRes);

                        if (oDCM.doku != null)
                        {
                            if (!string.IsNullOrEmpty(oDCM.doku.transaction_state))
                            {
                                if (oDCM.doku.transaction_state.ToLower() == "success")
                                {
                                    ViewBag.dSuccess = "display:block;";
                                    ViewBag.dFail = "display:none;";
                                }
                                else
                                {
                                    ViewBag.dFail = "display:block;";
                                    ViewBag.dSuccess = "display:none;";
                                }
                            }
                            else
                            {
                                ViewBag.dFail = "display:block;";
                                ViewBag.dSuccess = "display:none;";
                            }
                        }
                    }
                    else
                    {
                        ViewBag.dFail = "display:block;";
                        ViewBag.dSuccess = "display:none;";
                    }

                }
                else
                {
                    ViewBag.dFail = "display:block;";
                    ViewBag.dSuccess = "display:none;";
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(oDCM);
        }

        #endregion


        public JsonResult GetPowerProcess(string meterno, string amount)
        {
            string ResCode = "-555";
            string ResMsg = string.Empty;
            string RURL = string.Empty;
            try
            {
                if (Session["subscriber"] != null)
                {

                    string sMSISDN = Session["subscriber"].ToString();

                    AccountModel oAM = new AccountModel();
                    oAM = _care_repo.GetMultipleSubscribers(sMSISDN);
                    if (oAM != null)
                    {
                        PaymentEntModel pay = new PaymentEntModel();
                        pay.email = oAM.Reg.Email;
                        pay.fname = oAM.Reg.FirstName;
                        pay.lname = oAM.Reg.LastName;
                        pay.paid_amount = amount;
                        pay.paid_for_msisdn = sMSISDN;
                        pay.paid_userid = oAM.Reg.UserId;
                        pay.sess_id = Session.SessionID;
                        pay.primary_msisdn = sMSISDN;
                        pay.payment_mode = 2;
                        pay.payment_gateway = "WIRECARD";
                        pay.payment_type = "CREDITCARD";
                        pay.ip_address = GetIPAddress();
                        long amount_toea = Convert.ToInt64(Convert.ToDecimal(amount) * 100);
                        var vTranFee = UOW.transaction_fees_repo.Get(filter: t => t.max >= amount_toea && t.min <= amount_toea).ToList();
                        if (vTranFee.Count > 0)
                            pay.order_surcharge = (Convert.ToDecimal(vTranFee.FirstOrDefault().fee) / 100).ToString();
                        else
                        {
                            ResCode = "-77";
                            ResMsg = "Invalid Amount!";
                            return Json(new { rcode = ResCode, rmsg = ResMsg, rurl = RURL }, JsonRequestBehavior.AllowGet);
                        }

                        decimal tot_trans_amount = Convert.ToDecimal(amount) + Convert.ToDecimal(pay.order_surcharge);

                        List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();

                        tbl_doku_order_item o_item = new tbl_doku_order_item();

                        o_item.product_id = -3;
                        o_item.product_name = meterno;
                        o_item.purchase_desc = "GETPOWER";
                        o_item.product_qty = 1;
                        o_item.product_price = Convert.ToDecimal(amount);

                        order_items.Add(o_item);

                        if (order_items.Count > 0)
                            pay.order_items = order_items.ToArray();

                        long retOrderId = _doku_client.save_selfcare_order(pay);
                        if (retOrderId > 0)
                        {
                            pay.paid_amount = tot_trans_amount.ToString();
                            long dokuId = _doku_client.save_doku_order_payment(pay, retOrderId, 1);
                            if (dokuId > 0)
                            {
                                temp_orders_model oT = new temp_orders_model();
                                oT.Id = 0;
                                oT.Order_Id = retOrderId;
                                oT.Session_Id = Session.SessionID;
                                oT.SiteId = 1;
                                string sJStemp = JsonConvert.SerializeObject(oT);
                                long TempOrderId = _doku_client.save_temp_order(sJStemp);
                                if (TempOrderId > 0)
                                {
                                    ResCode = "0";
                                    string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                    RURL = dk_process_url + "/" + oT.Session_Id;
                                }
                            }


                        }
                        #region fortesting
                        //string msisdn_no = Session["subscriber"].ToString();
                        //IPBuyPowerModel buypower = new IPBuyPowerModel();
                        //buypower.sUserName = gp_java_uid;
                        //buypower.sPassword = gp_java_pwd;
                        //buypower.sMeterNo = meterno;
                        //buypower.dAmount = amount;
                        //buypower.sMSISDN = msisdn_no;
                        //buypower.sKeyCode = gp_merc_keycode;
                        //buypower.sIPAddress = "172.16.23.186";
                        //buypower.iProcessMode = 0;
                        //buypower.user_id = 0;

                        //string ipdata = JsonConvert.SerializeObject(buypower);
                        //string enc_ipdata = _util_repo.AES_JENC(ipdata);

                        //string enc_uid = _util_repo.AES_ENC(gp_svc_uid);
                        //string enc_pwd = _util_repo.AES_ENC(gp_svc_pwd);
                        //ResCode = "";  //_buypower.GetToken(enc_uid, enc_pwd, msisdn_no, enc_ipdata, amount);

                        //int rcode;
                        //bool res = int.TryParse(ResCode, out rcode);
                        //if (res == false)
                        //{
                        //    string xmldata = string.Empty;
                        //    using (StreamReader sr = new StreamReader(Server.MapPath("~/EmailTemplate/gettoken.xml")))
                        //    {
                        //        xmldata = sr.ReadToEnd();
                        //    }

                        //    if (!string.IsNullOrEmpty(xmldata))
                        //    {
                        //        string token = string.Empty;
                        //        string units = string.Empty;
                        //        bool bsuccess = false;
                        //        System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                        //        xDoc.LoadXml(xmldata);
                        //        System.Xml.XmlNodeList xNodelst = xDoc.SelectNodes("//suprima/thin-client/vend/token");
                        //        if (xNodelst.Count > 0)
                        //        {
                        //            System.Xml.XmlNode xNode = xNodelst.Item(0);
                        //            for (int i = 0; i < xNode.ChildNodes.Count; i++)
                        //            {
                        //                if (xNode.ChildNodes[i].Name.ToLower() == "tk50")
                        //                {
                        //                    units = xNode.ChildNodes[i].InnerText.Trim();
                        //                    bsuccess = true;
                        //                }
                        //                else if (xNode.ChildNodes[i].Name.ToLower() == "tk60")
                        //                {
                        //                    token = xNode.ChildNodes[i].InnerText.Trim();
                        //                    bsuccess = true;
                        //                }
                        //            }

                        //            if (bsuccess == true)
                        //            {
                        //                ResCode = "0";
                        //                ResMsg = "Your K" + amount + " Easipay re-charge voucher for meter " + meterno + " is:";
                        //                ResMsg += "+" + token + " for " + units + " units.";
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //    ResCode = rcode.ToString();
                        #endregion
                    }
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return Json(new { rcode = ResCode, rmsg = ResMsg, rurl = RURL }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Credit Me

        public ActionResult CreditMe()
        {
            return PartialView("_CreditMe_Partial");
        }

        public JsonResult CreditMeCheck(string msidn_no, string amount)
        {
            string sResMsg = "";
            AccountModel oAM = new AccountModel();
            try
            {
                if (Session["subscriber"] != null)
                {
                    string sUsername = Session["subscriber"].ToString();
                    double dRes = 0;
                    if (double.TryParse(amount, out dRes))
                    {
                        oAM = _care_repo.GetMultipleSubscribers(sUsername);
                        if (oAM != null)
                        {
                            double ppbal = 0;
                            foreach (var sb in oAM.Subr)
                            {
                                if (sb.isPrimary == true)
                                    ppbal = sb.balance / 100;
                            }
                            if (ppbal > dRes)
                            {
                                sResMsg = "success";
                            }
                            else
                            {
                                sResMsg = "Your balance is below $ " + dRes.ToString();
                            }
                        }
                    }
                    else
                    {
                        sResMsg = "Only numbers allowed";
                    }
                }
                else
                {
                    sResMsg = "Invalid Request!";
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return Json(new { rmsg = sResMsg }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CreditMeProcess(string msidn_no, string amount)
        {
            string sRes = "";
            RegistrationModel oRM = new RegistrationModel();
            try
            {
                if (Session["subscriber"] != null)
                {
                    string sUsername = Session["subscriber"].ToString();

                    if (oRM != null)
                    {
                        CreditMeModel Cm = new CreditMeModel();
                        Cm.username = "";
                        Cm.password = "";
                        Cm.keycode = "1234";
                        Cm.fromMsisdn = sUsername;
                        Cm.toMsisdn = msidn_no.ToString();
                        Cm.amount = amount.ToString();

                        string merchantId = sUsername;
                        string data = JsonConvert.SerializeObject(Cm);

                        CreditMeOPModel r = new CreditMeOPModel();
                        //r.Rescode = proxy.creditMe(merchantId, data);
                        //sRes = JsonConvert.SerializeObject(r, Formatting.Indented);
                        sRes = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return Json(new { rmsg = sRes }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult getMSISDN_Details(string sdata, string msisdn_no)
        {
            AccountModel objAccModel = new AccountModel();
            try
            {
                if (!string.IsNullOrEmpty(sdata))
                {
                    objAccModel = JsonConvert.DeserializeObject<AccountModel>(sdata);

                    objAccModel.Subr = objAccModel.Subr.AsEnumerable().Where(x => x._MSISDNNumber == msisdn_no).ToList();

                    double ppbal = 0;
                    string sInvoiceNumber = string.Empty;
                    foreach (var sb in objAccModel.Subr)
                    {
                        if (sb.paidtype == "502")
                        {
                            if (!string.IsNullOrEmpty(sb.totalAmount))
                            {
                                ppbal = Convert.ToDouble(sb.stotalAmount) / 100;
                                sInvoiceNumber = sb.transactionNo;
                            }
                        }
                    }

                    ViewBag.postpaidBal = ppbal;
                    ViewBag.pptransno = sInvoiceNumber;
                }
            }
            catch (Exception ex)
            {
            }

            return PartialView("_cms_MSISDN_Details", objAccModel);
        }

        #region Gen FB Voucher No
        //public ActionResult gen_fb_voucher_no()
        //{
        //    List<web_tbl_fb_promotions> objFB = UOW.redeem_fb_promotions_repo.Get().ToList();
        //    foreach (var item in objFB)
        //    {
        //        item.fb_vouchers = (GenRefNo());

        //        UOW.redeem_fb_promotions_repo.Update(item);
        //        UOW.Save();
        //    }

        //    return Content("success");
        //}


        //private string GenRefNo()
        //{
        //    string res = string.Empty;

        //    string ref_no = _util_repo.GetRandomString(6);
        //    web_tbl_fb_promotions obj = UOW.redeem_fb_promotions_repo.Get(filter: (m => m.fb_vouchers == ref_no)).FirstOrDefault();
        //    if (obj != null)
        //    {
        //        res = GenRefNo();
        //    }
        //    else
        //    {
        //        res = ref_no;
        //    }

        //    return res;
        //}
        #endregion

        #region device settings

        [SessionExpireFilter]
        public ActionResult devicesettings(string id)
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login");
            try
            {
                if (Session["subscriber"] != null)
                    id = Convert.ToString(Session["subscriber"]).Replace("677", "").Trim();

                ViewBag.msisdn_no = device_setting_url + id;

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        #endregion

        public ActionResult wcinfo(string id)
        {
            //if (Session["subscriber"] == null)
            //    return RedirectToAction("Login");
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (id == "cancelled")
                    {
                        ViewBag.msg = id;
                    }
                    else if (id == "error")
                    {
                        ViewBag.msg = id;
                    }
                    else if (id == "failed")
                    {
                        ViewBag.msg = id;
                    }
                    else
                    {
                        ViewBag.msg = "";
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        #region Dispose All Objects
        protected override void Dispose(bool disposing)
        {
            _care_repo.Dispose();
            _util_repo.Dispose();
            _shop_repo.Dispose();
            _sim_repo.Dispose();
              UOW.Dispose();
           
            base.Dispose(disposing);
        }
        #endregion
    }
}

