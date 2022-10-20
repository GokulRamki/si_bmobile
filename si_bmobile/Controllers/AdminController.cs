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
using System.Configuration;
using MvcPaging;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using System.Xml.Linq;
using si_bmobile.dokuRef;
using System.Data;

namespace si_bmobile.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        private IcareRepository _care_repo;
        private IUtilityRepository _util_repo;
        private IShopRepository _shop_repo;
        private IPlanRepository _yplan_repo;
        //private IdokuRepository _doku_repo;
        private ISimRepository _sim_repo;
        private IUserRepository _user_repo;
        private IStaffsTopup_Repo _staffs_topup_repo;

        bemobileselfcareEntities _dbselfcare;
        UnitOfWork _UOW;
        string sAdmName;
        string sAdmPwd;
        private int defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pgSize"]);

        static long Care_User_id = 0;
        private IdokuServiceClient _doku_client;
        private IKYCRepository _kyc_repo;
        string dokudomain;
        private decimal kad_bal_limtit = Convert.ToDecimal(ConfigurationManager.AppSettings["kad_bal_limit"]);
        public AdminController()
        {
            this._care_repo = new careRepository();
            this._util_repo = new UtilityRepository();
            this._shop_repo = new ShopRepository();
            this._yplan_repo = new PlanRepository();
            //this._doku_repo = new dokuRepository();
            this._sim_repo = new SimRepository();
            this._user_repo = new UserRepository();
            this.sAdmName = ConfigurationManager.AppSettings["admuname"];
            this.sAdmPwd = ConfigurationManager.AppSettings["admpwd"];
            this._UOW = new UnitOfWork();
            this._dbselfcare = new bemobileselfcareEntities();
            this._staffs_topup_repo = new StaffsTopup_Repo();
            this._doku_client = new IdokuServiceClient();
            this._kyc_repo = new KYCRepository();
            this.dokudomain = ConfigurationManager.AppSettings["dokudomain"];
            if (Care_User_id > 0)
                ViewBag.Menus_val = _user_repo.GetUserMenu(Care_User_id);
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login", "Admin");
        }

        #region Login/ Logout

        public ActionResult Login()
        {
            AdminModel oAM = new AdminModel();

            Care_User_id = 0;

            if (Session["beadmin"] != null)
                Session.Clear();
            //string pwd = _util_repo.AES_DEC("xzkadou3kMnehO1XtahCAA ==");
            return View(oAM);
            //return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminModel oAM)
        {
            string sMsg = "";
            var be_admin = _UOW.care_user_repo.Get(f => f.user_name == oAM.Username && f.user_pwd == oAM.Password && f.is_active == true && f.is_deleted == false).FirstOrDefault();
            if (be_admin != null)
            {
                Session["beadmin"] = be_admin.id;
                Session["role_id"] = be_admin.role_id.ToString();
                if (be_admin.role_id == 1)
                {
                    return RedirectToAction("Bundles");
                    //return RedirectToAction("TopupTransactions");
                }
                else
                {
                    Care_User_id = be_admin.id;
                    var menus = _user_repo.GetUserMenu(be_admin.id);

                    if (menus.Count > 0)
                        return RedirectToAction("ProfileView");
                    else
                        return RedirectToAction("Login");
                }
            }
            else
                sMsg = "Login Failed!";

            ViewBag.Message = sMsg;

            return View();
        }

        public ActionResult Logout()
        {
            Care_User_id = 0;
            Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Bundles
        private List<SelectListItem> FillType()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var res = (from b in _dbselfcare.bundles
                       where b.isDelete == false
                       select new { b.isVoice }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = (res[i].isVoice == true ? "Voice" : "Data"), Value = res[i].isVoice.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }
        private List<SelectListItem> FillStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = (from b in _dbselfcare.bundles
                       where b.isDelete == false
                       select new { b.isActive }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = (res[i].isActive == true ? "Active" : "InActive"), Value = res[i].isActive.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        [SessionExpireFilter]
        public ActionResult Bundles(string bundle_id, string plan_name, bool? ddlStatus, bool? ddlType, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Bundles"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<BundleModel> oB = new List<BundleModel>();

            ViewData["bundle_id"] = bundle_id;
            ViewData["plan_name"] = plan_name;
            ViewData["ddlStatus"] = ddlStatus;
            ViewData["ddlType"] = ddlType;
            try
            {
                ViewBag.statuslist = FillStatus();
                ViewBag.typeslist = FillType();
                oB = _yplan_repo.get_all_bundles();
                if (oB.Count > 0)
                {
                    
                    if (!string.IsNullOrWhiteSpace(bundle_id))
                    {
                        long bId = 0;
                        if (long.TryParse(bundle_id, out bId))
                        {
                            oB = oB.Where(b => b.Id == bId).ToList();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(plan_name))
                        oB = oB.Where(b => b.PlanName.ToUpper().StartsWith(plan_name.ToUpper())).ToList();

                    if (ddlStatus != null)
                        oB = oB.Where(b => b.isActive == ddlStatus).ToList();

                    if (ddlType != null)
                        oB = oB.Where(b => b.isVoice == ddlType).ToList();

                    oB = oB.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oB.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "No bundles found!";
                ViewBag.Message = sMsg;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Bundles", oB);
                else
                    return View(oB);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oB);
        }

        [SessionExpireFilter]
        public ActionResult CreateBundle()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Bundles"))
                return RedirectToAction("Logout");

            BundleModel oB = new BundleModel();
            return View(oB);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBundle(BundleModel oB)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = "";
            try
            {
                ModelState.Remove("bundle_type_id");
                if (ModelState.IsValid)
                {
                    var bundle = _yplan_repo.get_bundles_byId(oB.Id);
                    if (bundle == null)
                    {
                        string Msg = string.Empty;
                        long lRet = _yplan_repo.create_bundle(oB, out Msg);
                        if (lRet > 0)
                            sMsg = Msg;
                        else
                            sMsg = Msg;
                    }
                    else
                        sMsg = "Bundle id already exits!";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oB);
        }

        [SessionExpireFilter]
        public ActionResult EditBundle(long Id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Bundles"))
                return RedirectToAction("Logout");

            BundleModel oB = new BundleModel();

            try
            {
                if (Id > 0)
                    oB = _yplan_repo.get_bundles_byId(Id);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oB);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult EditBundle(BundleModel oB)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = "";

            try
            {
                if (ModelState.IsValid)
                {
                    
                    if (oB != null)
                    {
                        string Msg = string.Empty;
                        long iRet = _yplan_repo.update_bundle(oB, out Msg);
                        if (iRet > 0)
                            sMsg = Msg;
                        else
                            sMsg = Msg;
                    }
                    else
                        sMsg = "Bundle doesn't Exist!";                   
                }
                ViewBag.Msg = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oB);
        }

        public ActionResult DelBundle(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            long B_id = Convert.ToInt64(Session["beadmin"]);
            int R_id = Convert.ToInt32(Session["role_id"]);
            if (R_id != 1)
            {
                if (!_user_repo.checkMenuAccess(B_id, R_id, "Bundles"))
                    return RedirectToAction("Logout");
            }
            string sMsg = "";
            try
            {
                if (id > 0)
                {                  
                    int iRet = 0;
                    string Msg = string.Empty;
                    iRet = _yplan_repo.delete_bundle(id, out Msg);
                    if (iRet == 1)
                    {                      
                        sMsg = "Bundle deleted successfully";
                        return RedirectToAction("Bundles", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
                      

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

        #region PROMOTIONS

        private List<SelectListItem> FillPromoStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var resStatus = _UOW.promotions_repo.Get(filter: p => p.Isdeleted == false).Distinct().ToList();
            if (resStatus.Count > 0)
            {
                var res = (from b in resStatus select new { sStatus = (b.IsActive == true ? "Active" : "InActive"), isactive = b.IsActive }).Distinct().ToList();
                if (res.Count > 0)
                {
                    for (int i = 0; i < res.Count; i++)
                    {
                        var item = new SelectListItem { Text = res[i].sStatus, Value = res[i].isactive.ToString() };
                        result.Add(item);
                    }
                }
            }
            return result;
        }


        [SessionExpireFilter]
        public ActionResult Promotions(string sTitle, bool? ddlStatus, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Promotions"))
                return RedirectToAction("Logout");

            string sMsg = "";

            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<web_tbl_promotions> oPM = new List<web_tbl_promotions>();
            ViewData["sTitle"] = sTitle;
            ViewData["ddlStatus"] = ddlStatus;
            try
            {
                ViewBag.prstatus = FillPromoStatus();

                oPM = _UOW.promotions_repo.Get(filter: (p => p.Isdeleted == false)).ToList();
                if (oPM.Count > 0)
                {


                    if (!string.IsNullOrEmpty(sTitle))
                        oPM = oPM.Where(b => b.title.Trim().ToLower().Contains(sTitle.Trim().ToLower())).ToList();

                    if (ddlStatus != null)
                        oPM = oPM.Where(b => b.IsActive == ddlStatus).ToList();

                    oPM = oPM.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oPM.Count == 0)
                        sMsg = "No Promotions found!";
                }
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Promotions", oPM);
                else
                    return View(oPM);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oPM);

        }

        [SessionExpireFilter]
        public ActionResult CreatePromotion()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Promotions"))
                return RedirectToAction("Logout");
            try
            {
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePromotion(web_tbl_promotions oP)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = "";

            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(oP.image_url))
                    {
                        string rURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                        oP.image_url = rURL + "/images/coming-soon.jpg";
                    }

                    //iRet = _shop_repo.SavePromotion(oP);
                    _UOW.promotions_repo.Insert(oP);
                    _UOW.Save();
                    if (oP.id > 0)
                        sMsg = "Promotion created successfully";
                    else
                        sMsg = "Failed to create!";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oP);
        }

        [SessionExpireFilter]
        public ActionResult EditPromotion(int Id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Promotions"))
                return RedirectToAction("Logout");
            try
            {
                web_tbl_promotions oP = _UOW.promotions_repo.GetByID(Id);
                if (oP != null)
                    return View(oP);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }


        [HttpPost]
        [SessionExpireFilter]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult EditPromotion(web_tbl_promotions oP)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = "";

            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(oP.image_url))
                    {
                        string rURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                        oP.image_url = rURL + "/images/coming-soon.jpg";
                    }

                    _UOW.promotions_repo.Update(oP);
                    _UOW.Save();
                    if (oP.id > 0)
                        sMsg = "Promotion detail updated successfully";
                    else
                        sMsg = "Failed to update";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oP);
        }



        public ActionResult DelPromo(int id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Promotions"))
                return RedirectToAction("Logout");

            string sMsg = "";
            try
            {
                if (id > 0)
                {
                    var promotions = _UOW.promotions_repo.GetByID(id);

                    if (promotions != null)
                    {
                        promotions.Isdeleted = true;
                        _UOW.promotions_repo.Update(promotions);
                        _UOW.Save();
                        sMsg = "Promotion deleted successfully";
                        return RedirectToAction("Promotions", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";

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

        #region Common YPLAN stuffs
        private List<SelectListItem> FillYplanType()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var res = (from b in _dbselfcare.plan_type select new { plan_type_id = b.id, sPlantype = b.name }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i].sPlantype, Value = res[i].plan_type_id.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        public string GetPlansbyTypeId(long plan_type_id)
        {
            string sRes = "";
            var plansList = (from p in _dbselfcare.plans where p.isActive == true && p.isDeleted == false && p.plan_type_id == plan_type_id select new { p.Id, p.name }).ToList();
            if (plansList.Count > 0)
            {
                sRes = JsonConvert.SerializeObject(plansList).ToString();
            }
            return sRes;
        }
        public string GetDenominationbyPlanId(long plan_id)
        {
            string sRes = "";
            var deno = (from dt in _dbselfcare.plan_denomination
                        join pd in _dbselfcare.denomination_type on dt.denomination_type_id equals pd.Id
                             where dt.isActive == true && dt.isDeleted == false && dt.plan_id == plan_id
                             select new
                             {
                                 dt.id,
                                 denomination= dt.denomination+" "+pd.type
                             }).ToList();
            if (deno.Count > 0)
            {
                sRes = JsonConvert.SerializeObject(deno).ToString();
            }
            return sRes;
        }
        

        public string GetDenominationTypebyPlanId(long plan_id)
        {
            string sRes = "";
            var deno_type = (from  dt in _dbselfcare.denomination_type 
                             where dt.isActive==true && dt.isDeleted==false && dt.plan_id==plan_id 
                             select new 
                             {
                                 dt.plan_id,
                                 dt.type
                             }).ToList();
            if (deno_type.Count > 0)
            {
                sRes = JsonConvert.SerializeObject(deno_type).ToString();
            }
            return sRes;
        }

        public JsonResult CheckPlanNameExists(plansModel obj)
        {
            var plan = (from p in _dbselfcare.plans
                        where p.isDeleted == false && p.name == obj.name && p.plan_type_id == obj.plan_type_id
                        select p).ToList();
            if (plan.Count > 0)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckDenoTypeExists(DenominationTypeModel obj)
        {
            var plan = (from p in _dbselfcare.denomination_type
                        where p.isDeleted == false && p.type == obj.type
                        select p).ToList();
            if (plan.Count > 0)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(true, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region yplan

        private List<SelectListItem> FillYplanStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            string sStatus = "";
            var res = (from b in _dbselfcare.plans where b.isDeleted == false select new { isactive = b.isActive }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    if (res[i].isactive == true)
                        sStatus = "Active";
                    else
                        sStatus = "InActive";
                    var item = new SelectListItem { Text = sStatus, Value = res[i].isactive.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }




        public ActionResult Yplans(string sPlan, string ddlStatus, string ddlType, int? page)
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplans"))
                    return RedirectToAction("Logout");

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                IList<plansModel> Pm = new List<plansModel>();
                ViewData["sPlan"] = sPlan;
                ViewData["ddlStatus"] = ddlStatus;
                ViewData["ddlType"] = ddlType;
                Pm = _yplan_repo.get_yplans();
                ViewBag.statusList = FillYplanStatus();
                ViewBag.pTypeList = FillYplanType();
                if (Pm.Count > 0)
                {

                    //Pm = Pm.Where(p => p.isDeleted == false).ToList();

                    if (!string.IsNullOrEmpty(sPlan))
                        Pm = Pm.Where(m => m.name.Trim().ToLower().Contains(sPlan.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlStatus))
                    {
                        bool bStatus;
                        if (bool.TryParse(ddlStatus, out bStatus))
                            Pm = Pm.Where(b => b.isActive == bStatus).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(ddlType))
                    {
                        long bType;
                        if (long.TryParse(ddlType, out bType))
                            Pm = Pm.Where(b => b.plan_type_id == bType).ToList();
                    }

                  

                    if (Pm.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "No plans found!";
                ViewBag.Message = sMsg;


                Pm = Pm.ToPagedList(currentPageIndex, defaultPageSize);
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Yplans", Pm);
                else
                    return View(Pm);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [SessionExpireFilter]
        public ActionResult Create_yplan()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplans"))
                return RedirectToAction("Logout");

            plansModel plan = new plansModel();
            try
            {
                plan.lstPlanType = FillYplanType();
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Create_yplan(plansModel plan)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = "";
            try
            {
                if (ModelState.IsValid)
                {

                    plan.isDeleted = false;
                    long returnvalue = _yplan_repo.create_yplan(plan);
                    if (returnvalue > 0)
                        sMsg = "Plan added successfully!";
                    else
                        sMsg = "Failed to create plan";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        public ActionResult Edit_yplan(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplans"))
                return RedirectToAction("Logout");


            plansModel plan = new plansModel();
            try
            {


                plan = _yplan_repo.get_yplan_byId(id);

                plan.lstPlanType = FillYplanType();

                if (plan != null)
                    return View(plan);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_yplan(plansModel plan)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = "";
            try
            {
                if (ModelState.IsValid)
                {
                    long returnvalue = _yplan_repo.update_yplan(plan);

                    if (returnvalue > 0)
                        sMsg = "Plan details updated successfully!";
                    else
                        sMsg = "Failed to Update!";

                    ViewBag.Msg = sMsg;
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        public ActionResult Delete_yplan(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplans"))
                return RedirectToAction("Logout");

            try
            {
                string sMsg = "";
                if (id > 0)
                {
                    bool returnvalue = _yplan_repo.delete_yplan(id);
                    if (returnvalue == true)
                    {
                        sMsg = "Plan deleted successfully";
                        return RedirectToAction("Yplans", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
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

        #region Yplan denomination

        private List<SelectListItem> FillYden_plan()
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var res = (from b in _dbselfcare.plans
                       where b.isDeleted == false
                       select new { plan_id = b.Id, plan_name = b.name }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i].plan_name, Value = res[i].plan_id.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }
        private List<SelectListItem> FillYdenStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            string sStatus = "";
            var res = (from b in _dbselfcare.plan_denomination
                       where b.isDeleted == false
                       select new { isactive = b.isActive }).Distinct().ToList();
            if (res.Count > 0)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    if (res[i].isactive == true)
                        sStatus = "Active";
                    else
                        sStatus = "InActive";
                    var item = new SelectListItem { Text = sStatus, Value = res[i].isactive.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }



        public ActionResult Yplan_denomination(long? ddlType, long? ddlPlans, string sPlanDenomination, bool? ddlStatus, int? page)
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplan_denomination"))
                    return RedirectToAction("Logout");

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                IList<plan_denominationModel> Pd = new List<plan_denominationModel>();
                ViewData["ddlType"] = ddlType;
                ViewData["ddlPlans"] = ddlPlans;
                ViewData["sPlanDenomination"] = sPlanDenomination;
                ViewData["ddlStatus"] = ddlStatus;


                ViewBag.typeList = FillYplanType();

                ViewBag.statusList = FillYdenStatus();



                Pd = _yplan_repo.get_yplandenominations();

                if (Pd.Count > 0)
                {


                    if (ddlPlans > 0)
                        Pd = Pd.Where(b => b.plan_id == ddlPlans).ToList();

                    if (!string.IsNullOrEmpty(sPlanDenomination))
                        Pd = Pd.Where(m => m.denomination.ToString().ToLower().Contains(sPlanDenomination.Trim().ToLower())).ToList();

                    if (ddlStatus != null)
                        Pd = Pd.Where(b => b.isActive == ddlStatus).ToList();

                    if (ddlType > 0)
                        Pd = Pd.Where(b => b.plan_type_id == ddlType).ToList();

                   

                    if (Pd.Count == 0)
                        sMsg = "No Details found matching your search criteria!";


                }
                else
                    sMsg = "No plan denomination found!";
                Pd = Pd.ToPagedList(currentPageIndex, defaultPageSize);
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Yplan_denomination", Pd);
                else
                    return View(Pd);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult Create_yplan_denomination()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplan_denomination"))
                return RedirectToAction("Logout");

            plan_denominationModel oDM = new plan_denominationModel();
            try
            {
                oDM.lstPlantypes = FillYplanType();

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }


        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Create_yplan_denomination(plan_denominationModel oDM)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {
                if (ModelState.IsValid)
                {
                    string sMsg = "";
                    var deno_type = _yplan_repo.get_yplandenom_typebyId(oDM.denomination_type_id);

                    if (deno_type != null)
                        oDM.unit = deno_type.unit * Convert.ToInt64(oDM.denomination);

                    long returnvalue = _yplan_repo.create_yplandenomination(oDM);

                    if (returnvalue > 0)
                    {
                        #region Insert Plan Price
                        //plan_priceModel pr = new plan_priceModel();
                        //pr.id = 0;
                        //pr.isActive = true;
                        //pr.isDeleted = false;
                        //pr.plan_id = oDM.plan_id;
                        //pr.denom_id = returnvalue;
                        //pr.price = _yplan_repo.calc_price(pr.plan_id, pr.denom_id.ToString());

                        //long returnPriceId = _yplan_repo.create_yplanprice(pr);
                        #endregion
                        //if (returnPriceId > 0)
                        //{
                            sMsg = "Plan denomination added successfully!";
                        }
                        else
                        {
                            sMsg = "Failed to add denomination!";
                        }

                        
                    //}
                    //else
                    //    sMsg = "Failed to add denomination";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        public ActionResult Edit_yplan_denomination(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplan_denomination"))
                return RedirectToAction("Logout");

            plan_denominationModel oDM = new plan_denominationModel();
            try
            {

                oDM = _yplan_repo.get_yplandenominationbyId(id);

                oDM.lstPlantypes = FillYplanType();


                if (oDM != null)
                    return View(oDM);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_yplan_denomination(plan_denominationModel oDM)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {

                if (ModelState.IsValid)
                {
                    string sMsg = "";
                    oDM.unit = oDM.unit * Convert.ToInt64(oDM.denomination);
                    long returnvalue = _yplan_repo.update_yplandenomination(oDM);
                    if (returnvalue > 0)
                    {

                        #region Update Plan Price
                        plan_priceModel pr = new plan_priceModel();
                       
                        pr.isActive = true;
                        pr.isDeleted = false;
                        pr.plan_id = oDM.plan_id;
                        pr.denom_id = oDM.denom_id;
                        pr.price = _yplan_repo.calc_price(pr.plan_id, oDM.denomination);

                        long returnPriceId = _yplan_repo.update_yplanprice(pr);
                        if (returnPriceId > 0)
                            sMsg = "Plan denomination details updated successfully!";
                        else
                            sMsg = "Failed to update!";
                        #endregion
                    }
                    else
                        sMsg = "Failed to update!";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        public ActionResult Delete_yplan_denomination(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplan_denomination"))
                return RedirectToAction("Logout");

            try
            {
                string sMsg = "";
                if (id > 0)
                {
                    bool returnvalue = _yplan_repo.delete_yplandenomination(id);
                    if (returnvalue == true)
                    {
                        sMsg = "plan denomination deleted successfully";
                        return RedirectToAction("Yplan_denomination", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
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

        #region Yplan denomination Type

        private List<SelectListItem> FillYdenTypeStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            string sStatus = "";
            var res = (from b in _dbselfcare.denomination_type
                       where
                           b.isDeleted == false
                       select new { sStatus = (b.isActive == true ? "Active" : "InActive"), denId = b.Id }).Distinct().ToList();
            for (int i = 0; i < res.Count; i++)
            {
                var item = new SelectListItem { Text = sStatus, Value = res[i].denId.ToString() };
                result.Add(item);
            }
            return result;
        }


        public ActionResult Yplan_denominationType(long? ddlPlantype, long? ddlPlans, string sPlanDenominationType, string ddlStatus, int? page)
        {
            IList<DenominationTypeModel> Pd = new List<DenominationTypeModel>();
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplan_denominationType"))
                    return RedirectToAction("Logout");

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;

                ViewData["sPlanDenominationType"] = sPlanDenominationType;
                ViewData["ddlPlantype"] = ddlPlantype;
                ViewData["ddlPlans"] = ddlPlans;
                ViewData["ddlStatus"] = ddlStatus;
                ViewBag.typeList = FillYplanType();
                // ViewBag.plansList = FillYden_plan();
                ViewBag.statusList = FillYdenTypeStatus();

                Pd = _yplan_repo.get_yplandenom_types();
                if (Pd.Count > 0)
                {

                    if (!string.IsNullOrEmpty(sPlanDenominationType))
                        Pd = Pd.Where(m => m.type.ToString().ToLower().Contains(sPlanDenominationType.Trim().ToLower())).ToList();

                    if (ddlPlans > 0)
                        Pd = Pd.Where(b => b.plan_id == ddlPlans).ToList();


                    if (ddlPlantype > 0)
                        Pd = Pd.Where(b => b.plan_type_id == ddlPlantype).ToList();


                    if (!string.IsNullOrWhiteSpace(ddlStatus))
                    {
                        bool bStatus;
                        if (bool.TryParse(ddlStatus, out bStatus))
                        {
                            Pd = Pd.Where(b => b.isActive == bStatus).ToList();
                        }
                    }

               

                    if (Pd.Count == 0)
                        sMsg = "No Details found matching your search criteria!";

                }
                else
                    sMsg = "No plan denomination found!";
                Pd = Pd.ToPagedList(currentPageIndex, defaultPageSize);
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Yplan_denominationType", Pd);
                else
                    return View(Pd);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(Pd);
        }

        public ActionResult Create_yplan_denominationType()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplan_denominationType"))
                return RedirectToAction("Logout");

            DenominationTypeModel oDM = new DenominationTypeModel();
            try
            {
                oDM.lstPlantypes = FillYplanType();

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Create_yplan_denominationType(DenominationTypeModel oDM)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {
                string sMsg = "";

                ViewBag.PlansList = _yplan_repo.getplan_andplantype();

                if (ModelState.IsValid)
                {
                    long returnvalue = _yplan_repo.create_yplandenom_type(oDM);
                    if (returnvalue > 0)
                        sMsg = "Denomination type added successfully!";
                    else
                        sMsg = "Failed to add denomination type";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        public ActionResult Edit_yplan_denominationType(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Yplan_denominationType"))
                return RedirectToAction("Logout");

            DenominationTypeModel oDM = new DenominationTypeModel();
            try
            {


                oDM = _yplan_repo.get_yplandenom_typebyId(id);

                oDM.lstPlantypes = FillYplanType();

                if (oDM != null)
                    return View(oDM);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_yplan_denominationType(DenominationTypeModel oDM)
        {

            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");


                if (ModelState.IsValid)
                {
                    string sMsg = "";

                    long returnvalue = _yplan_repo.update_yplandenom_type(oDM);

                    if (returnvalue > 0)
                        sMsg = "Plan denomination details updated successfully!";
                    else
                        sMsg = "Failed to update!";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oDM);
        }

        public ActionResult Delete_yplan_denominationType(long id)
        {
            if (!Checkuserlevel("Yplan_denominationType"))
                return RedirectToAction("Logout");

            DenominationTypeModel oDM = new DenominationTypeModel();

            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                string sMsg = "";
                if (id > 0)
                {
                    bool returnvalue = _yplan_repo.delete_yplandenom_type(id);
                    if (returnvalue == true)
                    {
                        sMsg = "Denomination Type deleted successfully";
                        return RedirectToAction("Yplan_denomination", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
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

        #region yplan price


        private List<SelectListItem> FillYplanPriceStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            string sStatus = "";
            var res = (from b in _dbselfcare.plan_price where b.isDeleted == false select new { isactive = b.isActive }).Distinct().ToList();
            for (int i = 0; i < res.Count; i++)
            {
                if (res[i].isactive == true)
                    sStatus = "Active";
                else
                    sStatus = "InActive";
                var item = new SelectListItem { Text = sStatus, Value = res[i].isactive.ToString() };
                result.Add(item);
            }
            return result;
        }

        //private List<SelectListItem> FillYplanPriceType(IList<YPriceModel> oYP)
        //{
        //    List<SelectListItem> result = new List<SelectListItem>();
        //    string sPlantype = "";
        //    var Plantype = _yplan_repo.get_yplan_types();
        //    var res = (from b in oYP select new { plan_type_id = b.plan_type_id }).Distinct().ToList();
        //    for (int i = 0; i < res.Count; i++)
        //    {
        //        var pt = Plantype.Where(t => t.id == res[i].plan_type_id).FirstOrDefault();
        //        if (pt != null)
        //            sPlantype = pt.name;
        //        var item = new SelectListItem { Text = sPlantype, Value = res[i].plan_type_id.ToString() };
        //        result.Add(item);
        //    }
        //    return result;
        //}

        //private List<SelectListItem> FillYplans(IList<YPriceModel> oYP)
        //{
        //    List<SelectListItem> result = new List<SelectListItem>();
        //    string sPlantype = "";
        //    var Plantype = _yplan_repo.get_yplan_types();
        //    var res = (from b in oYP select new { planid = b.plan_id, planname = b.planName, plantypeid = b.plan_type_id }).Distinct().ToList();
        //    for (int i = 0; i < res.Count; i++)
        //    {
        //        var pt = Plantype.Where(t => t.id == res[i].plantypeid).FirstOrDefault();
        //        if (pt != null)
        //            sPlantype = pt.name;

        //        if (Plantype != null)
        //            sPlantype = pt.name;
        //        var item = new SelectListItem { Text = res[i].planname + " " + "(" + sPlantype + ")", Value = res[i].planid.ToString() };
        //        result.Add(item);
        //    }
        //    return result;
        //}

        public ActionResult Yplan_price(string ddlPlans, string sPlanDenomination, string ddlStatus, string ddlType, int? page)
        {
            IList<YPriceModel> price = new List<YPriceModel>();
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplan_price"))
                    return RedirectToAction("Logout");

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;
             
                ViewData["ddlType"] = ddlType;
                ViewData["ddlPlans"] = ddlPlans;
                ViewData["sPlanDenomination"] = sPlanDenomination;
                ViewData["ddlStatus"] = ddlStatus;


                ViewBag.typeList = FillYplanType();
                ViewBag.plansList = FillYden_plan();
                ViewBag.statusList = FillYplanPriceStatus();


                price = _yplan_repo.get_allyplanprice();
                if (price.Count > 0)
                {



                    if (!string.IsNullOrWhiteSpace(ddlPlans))
                    {
                        long lplanID;
                        if (long.TryParse(ddlPlans, out lplanID))
                            price = price.Where(b => b.plan_id == lplanID).ToList();
                    }

                    if (!string.IsNullOrEmpty(sPlanDenomination))
                        price = price.Where(m => m.denomination.Trim().ToLower().Contains(sPlanDenomination.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlStatus))
                    {
                        bool bStatus;
                        if (bool.TryParse(ddlStatus, out bStatus))
                            price = price.Where(b => b.isActive == bStatus).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(ddlType))
                    {
                        long lType;
                        if (long.TryParse(ddlType, out lType))
                            price = price.Where(b => b.plan_type_id == lType).ToList();

                    }

                    

                    if (price.Count == 0)
                        sMsg = "No Details found matching your search criteria!";

                }
                else
                    sMsg = "No plan price details found!";
                ViewBag.Message = sMsg;
                price = price.ToPagedList(currentPageIndex, defaultPageSize);
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Yplans_price", price);
                else
                    return View(price);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(price);
        }

        public ActionResult Create_yplan_price()
        {
            plan_priceModel plan = new plan_priceModel();
            try
            {

                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplan_price"))
                    return RedirectToAction("Logout");

                plan.lstPlantypes = FillYplanType();


            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Create_yplan_price(plan_priceModel plan)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {

                if (ModelState.IsValid)
                {
                    string sMsg = "";

                    plan.price = plan.price * 100;

                    plan.isDeleted = false;

                    long returnvalue = _yplan_repo.create_yplanprice(plan);

                    if (returnvalue > 0)
                        sMsg = "Price added successfully..";
                    else
                        sMsg = "Failed to Create Price!";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(plan);
        }

        public ActionResult Edit_yplan_price(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            plan_priceModel yplan = new plan_priceModel();
            if (!Checkuserlevel("Yplan_price"))
                return RedirectToAction("Logout");
            try
            {
                yplan = _yplan_repo.get_yplanprice_byId(id);

                yplan.lstPlantypes = FillYplanType();

                if (yplan != null)
                {
                    yplan.price = yplan.price / 100;
                    return View(yplan);
                }
                else
                    return RedirectToAction("Yplan_price");

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(yplan);
        }

        [HttpPost]
        [SessionExpireFilter]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_yplan_price(plan_priceModel yplan)
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                ViewBag.PlansList = _yplan_repo.getplan_andplantype();

                ViewBag.DenominationList = (from m in _yplan_repo.getdenom_anddenomtype().Where(m => m.plan_id == yplan.plan_id).ToList()
                                            select new SelectListItem { Text = m.denomination.ToString(), Value = m.id.ToString() }).ToList();

                if (ModelState.IsValid)
                {
                    string sMsg = "";
                    yplan.price = yplan.price * 100;
                    long returnvalue = _yplan_repo.update_yplanprice(yplan);

                    if (returnvalue > 0)
                        sMsg = "Plan price details updated successfully!";

                    ViewBag.Message = sMsg;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(yplan);
        }

        public ActionResult Delete_plan_price(long id)
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("Yplan_price"))
                    return RedirectToAction("Logout");

                string sMsg = "";
                if (id > 0)
                {
                    plan_priceModel plan = new plan_priceModel();

                    bool returnvalue = _yplan_repo.delete_yplanprice(id);
                    if (returnvalue == true)
                    {
                        sMsg = "plan price deleted successfully";
                        return RedirectToAction("Yplan_price", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public JsonResult denomination_selection(long id)
        {
            var denominations = (from m in _yplan_repo.getdenom_anddenomtype().Where(m => m.plan_id == id)
                                 select new SelectListItem { Text = m.denomination.ToString(), Value = m.id.ToString() }).ToList();

            denominations.Insert(0, new SelectListItem { Text = "--- Choose denomination ---", Value = "" });
            JsonResult result = new JsonResult();
            result.Data = denominations;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;
        }

        #endregion

        #region DOKU REPORTS

         private List<SelectListItem> FillPeiezyOrderStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res =_UOW.shopping_order_payment_repo.Get().Select(p=>p.payment_status).Distinct().ToList();
            if (res.Count > 0)
            {
                
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i], Value = res[i] };
                    result.Add(item);
                }
            }
            return result;
        }


        private List<SelectListItem> FillOrderStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = _doku_client.OrderStatus().ToList();
            if (res.Count > 0)
            {
                
                for (int i = 0; i < res.Count; i++)
                {
                    var item = new SelectListItem { Text = res[i], Value = res[i] };
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
                    var item = new SelectListItem { Text = res[i], Value = res[i] };
                    result.Add(item);
                }
            }
            return result;
        }
        //private List<SelectListItem> FillPayType()
        //{
        //    List<SelectListItem> result = new List<SelectListItem>();
        //    var res = _UOW.selfcare_order_payment_repo.Get().ToList();
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
        [SessionExpireFilter]
        public ActionResult SelfcareTransactions(string transmerchantid, string email, string ddlDokuStatus, string sFrom, string sTo, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("SelfcareTransactions"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<DokuCareModel> oD = new List<DokuCareModel>();
            try
            {
                ViewBag.Orderstatuslist = FillOrderStatus();
                ViewBag.Dokustatuslist = FillDokuStatus();
               // ViewBag.PayTypelist = FillPayType();

                string sTranscations = _doku_client.get_doku_order_transactionsBySite(1);

                oD = JsonConvert.DeserializeObject<List<DokuCareModel>>(sTranscations).ToList();
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

                    oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oD.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "No Records found!";
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_SelfcareTransactions", oD);
                else
                    return View(oD);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oD);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult TopupTransactions()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("TopupTransactions"))
                return RedirectToAction("Logout");

            try
            {

                long user_id = Convert.ToInt64(Session["beadmin"]);
                int role_id = Convert.ToInt32(Session["role_id"]);
                string gui_id = Guid.NewGuid().ToString();
                string menu_id = string.Empty;

                string svc_uid = ConfigurationManager.AppSettings["svc_uid"];
                string svc_pwd = ConfigurationManager.AppSettings["svc_pwd"];

                string svcUname = _util_repo.AES_ENCWR(svc_uid);
                string svcPwd = _util_repo.AES_ENCWR(svc_pwd);


                menu_id = _user_repo.get_MenuListByUserId(user_id, role_id);


                long rtn = _doku_client.save_doku_gui_access(svcUname, svcPwd, user_id, gui_id, menu_id);
                if (rtn > 0)
                {
                    return new PermanentRedirectResult(dokudomain + gui_id + "&user_id=" + user_id + "&report=SITopupTransactions");
                }
                return RedirectToAction("Login");
            }

            //    string sMsg = "";
            //    int currentPageIndex = page.HasValue ? page.Value : 1;
            //    IList<Payment_Transaction_model> oD = new List<Payment_Transaction_model>();

            //    IList<Payment_Transaction_model> oPeiezy = new List<Payment_Transaction_model>();
            //    if (sptype == null)
            //        sptype = "WIRECARD";

            //    ViewData["transmerchantid"] = transmerchantid;
            //    ViewData["mobile_no"] = mobile_no;
            //    ViewData["ddlDokuStatus"] = ddlDokuStatus;
            //    ViewData["ddlOrderstatus"] = ddlOrderstatus;
            //    ViewData["sFrom"] = sFrom;
            //    ViewData["sTo"] = sTo;
            //    ViewData["sptype"] = sptype;

            //    ViewBag.Orderstatuslist = FillOrderStatus();
            //    ViewBag.Dokustatuslist = FillDokuStatus();
            //    //ViewBag.PayTypelist = FillPayType();
            //    try
            //    {
            //  string sTranscations = _doku_client.get_order_transactionsByGateway(1, "TOPUP", sFrom, sTo, sptype);

            //    if (!string.IsNullOrEmpty(sTranscations))
            //    {
            //        oD = JsonConvert.DeserializeObject<List<Payment_Transaction_model>>(sTranscations).ToList();




            //        if (sptype == "DOKU")
            //        {
            //            if (!string.IsNullOrWhiteSpace(transmerchantid))
            //                oD = oD.Where(d => d.transidmerchant == transmerchantid).ToList();


            //            if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
            //                oD = oD.Where(d => d.resultmsg.ToLower() == ddlDokuStatus).ToList();
            //        }
            //        else if (sptype == "WIRECARD")
            //        {
            //            if (!string.IsNullOrWhiteSpace(transmerchantid))
            //                oD = oD.Where(d => d.request_id == transmerchantid).ToList();


            //            if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
            //                oD = oD.Where(d => d.transaction_state == ddlDokuStatus).ToList();
            //        }


            //        if (!string.IsNullOrWhiteSpace(mobile_no))
            //            oD = oD.Where(d => d.purchase_msisdn == mobile_no).ToList();

            //        if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
            //            oD = oD.Where(d => d.payment_status == ddlOrderstatus).ToList();


            //        if (oD.Count > 0)
            //            TempData["TopupTransactions"] = oD;
            //    }
            //    else if (sptype == "INTERNETBANKING")
            //    {
            //        oPeiezy = _shop_repo.GetPeiezyOrders(sFrom, sTo);
            //        if (oPeiezy != null && oPeiezy.Count > 0)
            //            oD = oPeiezy;
            //    }
            //    oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

            //    if (oD.Count == 0)
            //        sMsg = "No Details found matching your search criteria!";

            //    ViewBag.Message = sMsg;
            //    if (Request.IsAjaxRequest())
            //        return PartialView("_Ajax_TopupTransactions", oD);
            //    else
            //        return View(oD);
            //}
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
            return Content("Invalid Page Request");
        }

        //public void TopupTransactionCsvList()
        //{

        //    try
        //    {

        //        if (TempData["TopupTransactions"] != null)
        //        {
        //            List<Payment_Transaction_model> TransReport = new List<Payment_Transaction_model>();

        //            TransReport = (List<Payment_Transaction_model>)TempData["TopupTransactions"];

        //            if (TransReport != null && TransReport.Count > 0)
        //            {
        //                TempData["TopupTransactions"] = TransReport;

        //                string filename = "TopupTransactions";

        //                Response.ClearContent();
        //                Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
        //                Response.ContentType = "text/csv";

        //                StringWriter sw = new StringWriter();

        //                sw.WriteLine("\"Name\",\"TransactionID\",\"Transaction Date\",\"Amount\",\"Mobile Number\",\"Payment Status\",\"TopUp Status\",\"Paid Type\",\"Email\",\"Payment Gateway\"");

        //                foreach (var line in TransReport)
        //                {

        //                    string tmsg = "";
        //                    if (line.payment_gateway == "DOKU")
        //                        tmsg = line.resultmsg;
        //                    else if (line.payment_gateway == "WIRECARD")
        //                        tmsg = line.transaction_state;
        //                    else
        //                        tmsg = line.payment_status;


        //                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"",
        //                                               line.cust_fname + " " + line.cust_lname,
        //                                               line.order_number,
        //                                               line.order_datetime,
        //                                               line.order_product_total,
        //                                               line.purchase_msisdn,
        //                                              tmsg,
        //                                               line.payment_status,
        //                                               line.payment_type,
        //                                               line.cust_email,
        //                                               line.payment_gateway
        //                                               ));
        //                }

        //                Response.Write(sw.ToString());
        //                Response.End();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _util_repo.ErrorLog_Txt(ex);

        //    }
        //}

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult MobileTopupTransaction()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("MobileTopupTransaction"))
                return RedirectToAction("Logout");
            try
            {
                long user_id = Convert.ToInt64(Session["beadmin"]);
                int role_id = Convert.ToInt32(Session["role_id"]);
                string gui_id = Guid.NewGuid().ToString();

                string svc_uid = ConfigurationManager.AppSettings["svc_uid"];
                string svc_pwd = ConfigurationManager.AppSettings["svc_pwd"];

                string svcUname = _util_repo.AES_ENCWR(svc_uid);
                string svcPwd = _util_repo.AES_ENCWR(svc_pwd);
                string menu_id = string.Empty;

                menu_id = _user_repo.get_MenuListByUserId(user_id, role_id);
                long rtn = _doku_client.save_doku_gui_access(svcUname, svcPwd, user_id, gui_id, menu_id);
                if (rtn > 0)
                {
                    return new PermanentRedirectResult(dokudomain + gui_id + "&user_id=" + user_id + "&report=SIMobileTopupTransaction");
                }
                return RedirectToAction("Login");
            }
            //int lSiteId = 0;


            //if (siteId == null)
            //    siteId = 5;

            //string sMsg = "";
            //int currentPageIndex = page.HasValue ? page.Value : 1;
            //IList<Payment_Transaction_model> oD = new List<Payment_Transaction_model>();
            //ViewData["transmerchantid"] = transmerchantid;
            //ViewData["mobile_no"] = mobile_no;
            //ViewData["ddlDokuStatus"] = ddlDokuStatus;
            //ViewData["ddlOrderstatus"] = ddlOrderstatus;
            //ViewData["sFrom"] = sFrom;
            //ViewData["sTo"] = sTo;
            //ViewData["siteId"] = siteId;
            //ViewData["sptype"] = sptype;

            //ViewBag.Orderstatuslist = FillOrderStatus();
            //ViewBag.Dokustatuslist = FillDokuStatus();
            ////ViewBag.PayTypelist = FillPayType();
            //try
            //{
            //    List<mobileModel> obj = new List<mobileModel>();
            //    obj = RetrunListOfProducts();
            //    ViewBag.mobileType = obj;

            //        lSiteId = Convert.ToInt32(siteId);
            //    string sTranscations = string.Empty;
            //    if (lSiteId > 0)
            //        sTranscations = _doku_client.get_order_transactionsByGateway(lSiteId, "TOPUP",sFrom, sTo,sptype);


            //    if (!string.IsNullOrEmpty(sTranscations))
            //    {
            //        oD = JsonConvert.DeserializeObject<List<Payment_Transaction_model>>(sTranscations).ToList();


            //        if (sptype == "DOKU")
            //        {
            //            if (!string.IsNullOrWhiteSpace(transmerchantid))
            //                oD = oD.Where(d => d.transidmerchant == transmerchantid).ToList();


            //            if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
            //                oD = oD.Where(d => d.resultmsg == ddlDokuStatus).ToList();
            //        }
            //        else if (sptype == "WIRECARD")
            //        {
            //            if (!string.IsNullOrWhiteSpace(transmerchantid))
            //                oD = oD.Where(d => d.request_id == transmerchantid).ToList();


            //            if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
            //                oD = oD.Where(d => d.transaction_state == ddlDokuStatus).ToList();
            //        }




            //        if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
            //            oD = oD.Where(d => d.payment_status == ddlOrderstatus).ToList();


            //        if (!string.IsNullOrWhiteSpace(mobile_no))
            //            oD = oD.Where(d => d.purchase_msisdn == mobile_no).ToList();

            //        if (oD.Count > 0)
            //            TempData["MobileTopupTransactions"] = oD;
            //    }
            //    oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

            //    if (oD.Count == 0)
            //        sMsg = "No Details found matching your search criteria!";

            //    ViewBag.Message = sMsg;
            //    if (Request.IsAjaxRequest())
            //        return PartialView("_Ajax_MobileTopupTransaction", oD);
            //    else
            //        return View(oD);
            //}
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
            return Content("Invalid Page Request");
        }

        //public void MobileTopupTransactionCSVList()
        //{

        //    try
        //    {

        //        if (TempData["MobileTopupTransactions"] != null)
        //        {
        //            List<Payment_Transaction_model> TransReport = new List<Payment_Transaction_model>();

        //            TransReport = (List<Payment_Transaction_model>)TempData["MobileTopupTransactions"];

        //            if (TransReport != null && TransReport.Count > 0)
        //            {
        //                TempData["MobileTopupTransactions"] = TransReport;

        //                string filename = "MobileTopupTransactions";

        //                Response.ClearContent();
        //                Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
        //                Response.ContentType = "text/csv";

        //                StringWriter sw = new StringWriter();

        //                sw.WriteLine("\"Name\",\"TransactionID\",\"Transaction Date\",\"Amount\",\"Mobile Number\",\"Payment Status\",\"TopUp Status\",\"Paid Type\",\"Email\"");

        //                foreach (var line in TransReport)
        //                {

        //                    string tmsg = "";
        //                    if (line.payment_gateway == "DOKU")
        //                        tmsg = line.resultmsg;
        //                    else if (line.payment_gateway == "WIRECARD")
        //                        tmsg = line.transaction_state;
        //                    else
        //                        tmsg = line.payment_status;
        //                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\"",
        //                                               line.cust_fname + " " + line.cust_lname,
        //                                               line.order_number,
        //                                               line.order_datetime,
        //                                               line.order_product_total,
        //                                               line.purchase_msisdn,
        //                                              tmsg,
        //                                               line.payment_status,
        //                                               line.payment_type,
        //                                               line.cust_email));
        //                }

        //                Response.Write(sw.ToString());
        //                Response.End();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _util_repo.ErrorLog_Txt(ex);

        //    }
        //}

        
        [SessionExpireFilter]
        public ActionResult BYPTransactions(string transmerchantid, string email, string ddlDokuStatus, string ddlOrderstatus, string sFrom, string sTo, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("BYPTransactions"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<DokuCareModel_New> oD = new List<DokuCareModel_New>();
            ViewData["transmerchantid"] = transmerchantid;
            ViewData["email"] = email;
            ViewData["ddlDokuStatus"] = ddlDokuStatus;
            ViewData["ddlOrderstatus"] = ddlOrderstatus;
            ViewData["sFrom"] = sFrom;
            ViewData["sTo"] = sTo;
            try
            {
                ViewBag.Orderstatuslist = FillOrderStatus();
                ViewBag.Dokustatuslist = FillDokuStatus();
                //ViewBag.PayTypelist = FillPayType();
                string sTranscations = _doku_client.get_doku_order_transactionsByPurchase(1, "BYP", sFrom, sTo);


                if (!string.IsNullOrEmpty(sTranscations))
                {
                    oD = JsonConvert.DeserializeObject<List<DokuCareModel_New>>(sTranscations).ToList();
                    if (!string.IsNullOrWhiteSpace(transmerchantid))
                        oD = oD.Where(d => d.doku.transidmerchant == transmerchantid).ToList();

                    if (!string.IsNullOrWhiteSpace(email))
                        oD = oD.Where(d => d.order.cust_email == email).ToList();


                    if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
                        oD = oD.Where(d => d.doku.resultmsg == ddlDokuStatus).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
                        oD = oD.Where(d => d.orderpayment.payment_status == ddlOrderstatus).ToList();



                    //if ((!string.IsNullOrWhiteSpace(sFrom)) && (!string.IsNullOrWhiteSpace(sTo)))
                    //{
                    //    DateTime dtFrom = DateTime.ParseExact(sFrom, "MM/dd/yyyy", null);
                    //    DateTime dtTo = DateTime.ParseExact(sTo, "MM/dd/yyyy", null);
                    //    oD = oD.Where(t => t.doku.created_on.Date >= dtFrom && t.doku.created_on <= dtTo).ToList();
                    //}

                    if (oD.Count > 0)
                        TempData["BYPTransactionList"] = oD;
                }
                oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

                if (oD.Count == 0)
                    sMsg = "No Details found matching your search criteria!";

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

        public void BYPTransactionCsvList()
        {
            try
            {
                if (TempData["BYPTransactionList"] != null)
                {
                    List<DokuCareModel_New> TransReport = new List<DokuCareModel_New>();

                    TransReport = (List<DokuCareModel_New>)TempData["BYPTransactionList"];

                    if (TransReport != null && TransReport.Count > 0)
                    {
                        TempData["BYPTransactionList"] = TransReport;

                        string filename = "BYPTransactions";

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"Name\",\"TransactionID\",\"Transaction Date\",\"Amount\",\"Mobile Number\",\"Payment Status\",\"TopUp Status\",\"Paid Type\",\"Email\"");

                        foreach (var line in TransReport)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                                                       line.order.cust_fname + " " + line.order.cust_lname,
                                                       line.order.order_number,
                                                       line.order.order_datetime,
                                                       line.order.order_product_total,
                                                       line.order.purchase_msisdn,
                                                       line.orderpayment.payment_status,
                                                       line.orderpayment.payment_type,
                                                       line.order.cust_email));
                        }

                        Response.Write(sw.ToString());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
        }

        public ActionResult CugTransactions(string transmerchantid, string mobile_no, string ddlDokuStatus, string ddlOrderstatus, string sFrom, string sTo, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("CugTransactions"))
                return RedirectToAction("Logout");

            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<DokuCareModel> oD = new List<DokuCareModel>();
            ViewData["transmerchantid"] = transmerchantid;
            ViewData["mobile_no"] = mobile_no;
            ViewData["ddlDokuStatus"] = ddlDokuStatus;
            ViewData["ddlOrderstatus"] = ddlOrderstatus;
            ViewData["sFrom"] = sFrom;
            ViewData["sTo"] = sTo;

            ViewBag.Orderstatuslist = FillOrderStatus();
            ViewBag.Dokustatuslist = FillDokuStatus();
            try
            {
                oD.Clear();
                string sTranscations = _doku_client.get_doku_order_transactionsByPurchase(1, "CUGTOPUP", sFrom, sTo);
                oD = JsonConvert.DeserializeObject<List<DokuCareModel>>(sTranscations).ToList();

                if (!string.IsNullOrWhiteSpace(transmerchantid))
                    oD = oD.Where(d => d.doku.transidmerchant == transmerchantid).ToList();

                if (!string.IsNullOrWhiteSpace(mobile_no))
                    oD = oD.Where(d => d.order.purchase_msisdn == mobile_no).ToList();

                if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
                    oD = oD.Where(d => d.doku.resultmsg == ddlDokuStatus).ToList();

                if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
                    oD = oD.Where(d => d.orderpayment.payment_status == ddlOrderstatus).ToList();

                if ((!string.IsNullOrWhiteSpace(sFrom)) && (!string.IsNullOrWhiteSpace(sTo)))
                {
                    DateTime dtFrom = Convert.ToDateTime(sFrom);
                    DateTime dtTo = Convert.ToDateTime(sTo).AddDays(1);
                    oD = oD.Where(t => t.doku.created_on.Date >= dtFrom && t.doku.created_on <= dtTo).ToList();
                }
                if (oD.Count > 0)
                    TempData["CugTransactions"] = oD;

                oD = oD.ToPagedList(currentPageIndex, defaultPageSize);


                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_CugTransactions", oD);
                else
                    return View(oD);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oD);
        }

        public void CugTransactionCsvList()
        {
            try
            {
                if (TempData["CugTransactions"] != null)
                {
                    List<DokuCareModel> TransReport = new List<DokuCareModel>();

                    TransReport = (List<DokuCareModel>)TempData["CugTransactions"];

                    if (TransReport != null && TransReport.Count > 0)
                    {
                        TempData["CugTransactions"] = TransReport;

                        string filename = "CugTransactions";

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"Name\",\"TransactionID\",\"Transaction Date\",\"Amount\",\"Mobile Number\",\"Payment Status\",\"TopUp Status\",\"Paid Type\",\"Email\"");

                        foreach (var line in TransReport)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\"",
                                                       line.order.cust_fname + " " + line.order.cust_lname,
                                                       line.order.order_number,
                                                       line.order.order_datetime,
                                                       line.order.order_product_total,
                                                       line.order.purchase_msisdn,
                                                       line.doku.resultmsg,
                                                       line.orderpayment.payment_status,
                                                       line.orderpayment.payment_type,
                                                       line.order.cust_email));
                        }

                        Response.Write(sw.ToString());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
        }

        #endregion

        #region Trasaction Logs

        private List<SelectListItem> GetApptransLogStatus()
        {

            List<SelectListItem> result = new List<SelectListItem>();
            var AppTrnStatus = (from a in _dbselfcare.application_transaction select new { a.status }).Distinct().ToList();
            if (AppTrnStatus.Count > 0)
            {
                foreach (var item in AppTrnStatus)
                {
                    var status = new SelectListItem { Text = item.status, Value = item.status };
                    result.Add(status);
                }
            }
            return result;
        }


        public ActionResult AppTransactionLogs(int? page, string ref_no, string mobile_no, string status)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("AppTransactionLogs"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<Application_Transaction> oATLogs = new List<Application_Transaction>();
            ViewData["ref_no"] = ref_no;
            ViewData["mobile_no"] = mobile_no;
            ViewData["status"] = status;

            try
            {

                ViewBag.Logstatus = GetApptransLogStatus();

                oATLogs = _care_repo.get_application_translogs();

                // oATLogs = oATLogs.ToPagedList(currentPageIndex, defaultPageSize);
                if (oATLogs.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(ref_no))
                        oATLogs = oATLogs.Where(b => b.reference == ref_no).ToList();

                    if (!string.IsNullOrWhiteSpace(mobile_no))
                        oATLogs = oATLogs.Where(b => b.msisdn == mobile_no).ToList();

                    if (!string.IsNullOrWhiteSpace(status))
                        oATLogs = oATLogs.Where(b => b.status == status).ToList();

                    oATLogs = oATLogs.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oATLogs.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "Not Found!";
                ViewBag.Message = sMsg;


                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_AppTransactionLogs", oATLogs);
                else
                    return View(oATLogs);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(oATLogs);
        }

        public List<SelectListItem> FillAllplans()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var bundles = (from b in _dbselfcare.bundles where b.isDelete == false && b.isPostpaid == false select new { b.Id, b.PlanName }).Distinct().ToList();
            if (bundles.Count > 0)
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    var item = new SelectListItem { Text = bundles[i].PlanName, Value = bundles[i].Id.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        public ActionResult PlanPurchaseHistory(long? ddlBundles, string user_name, string sFrom, string sTo, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");


            if (!Checkuserlevel("PlanPurchaseHistory"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<UserHistoryLogs> oUHLogs = new List<UserHistoryLogs>();

            ViewData["ddlBundles"] = ddlBundles;
            ViewData["user_name"] = user_name;
            ViewData["sFrom"] = sFrom;
            ViewData["sTo"] = sTo;
            try
            {
                ViewBag.bundleList = FillAllplans();


                oUHLogs = _care_repo.get_user_history().ToList();

                if (oUHLogs.Count > 0)
                {
                    if (ddlBundles != null && ddlBundles > 0)
                    {
                        long bundleId = 0;
                        if (long.TryParse(ddlBundles.ToString(), out bundleId))
                        {
                            oUHLogs = oUHLogs.Where(b => b.planId == bundleId).ToList();
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(user_name))
                        oUHLogs = oUHLogs.Where(b => b.userName.Trim().ToLower().Contains(user_name.Trim().ToLower())).ToList();



                    if (!string.IsNullOrEmpty(sFrom) && !string.IsNullOrEmpty(sTo))
                    {
                        DateTime dtFrom = Convert.ToDateTime(sFrom).Date;
                        DateTime dtTo = Convert.ToDateTime(sTo).Date.AddDays(1);
                        oUHLogs = oUHLogs.Where(b => b.createdDate >= dtFrom && b.createdDate <= dtTo).ToList();
                    }
                    
                    if (oUHLogs.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "Not Found!";
                ViewBag.Message = sMsg;

                oUHLogs = oUHLogs.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_PlanPurchaseHistory", oUHLogs);
                else
                    return View(oUHLogs);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oUHLogs);
        }

        public ActionResult YPlanTransactionLogs(int? page, string from_msisdn, string to_msisdn, string ddlExpiry)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("YPlanTransactionLogs"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<YourPlanTransModel> oYPLogs = new List<YourPlanTransModel>();

            ViewData["from_msisdn"] = from_msisdn;
            ViewData["to_msisdn"] = to_msisdn;
            ViewData["ddlExpiry"] = ddlExpiry;
            try
            {
                oYPLogs = _care_repo.get_your_plan_translog();

                if (oYPLogs.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(from_msisdn))
                        oYPLogs = oYPLogs.Where(b => b.from_msisdn == from_msisdn.Trim()).ToList();

                    if (!string.IsNullOrWhiteSpace(to_msisdn))
                        oYPLogs = oYPLogs.Where(b => b.to_msisdn == b.to_msisdn.Trim()).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlExpiry))
                    {
                        bool bStatus;
                        if (bool.TryParse(ddlExpiry, out bStatus))
                        {
                            oYPLogs = oYPLogs.Where(b => b.isExpired == bStatus).ToList();
                        }
                    }                    

                    if (oYPLogs.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "Not Found!";
                ViewBag.Message = sMsg;

                oYPLogs = oYPLogs.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_YPlanTransactionLogs", oYPLogs);
                else
                    return View(oYPLogs);
            }
            catch (Exception ex)
            {


            }
            return View();
        }

        #endregion

        #region Sim Reg

        public ActionResult SimRegCustomers(int? page)
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                IList<tbl_customer> customers = new List<tbl_customer>();

                customers = _UOW.customer_repo.Get(filter: (m => m.is_active == true && m.is_deleted == false)).ToList();
                if (customers.Count > 0)
                {
                    customers = customers.ToPagedList(currentPageIndex, defaultPageSize);

                    if (customers.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "Not found!";
                ViewBag.Message = sMsg;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_SimRegCustomers", customers);
                else
                    return View(customers);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        public ActionResult CheckSimRegStatus(long id)
        {
            var RegstatusID = _UOW.customer_repo.Get(filter: (m => m.cust_id == id)).Select(m => m.reg_status_id).FirstOrDefault();

            if (RegstatusID == 1)
                return RedirectToAction("UpdateSimReg", new { id = id });
            else if (RegstatusID == 2)
                return RedirectToAction("UpdateSimReplaceReg", new { id = id });

            return View();
        }

        [HttpGet]
        public ActionResult UpdateSimReg(long id)
        {
            try
            {
                var _obj = _sim_repo.get_new_sim_customers().Where(m => m.cust.cust_id == id).FirstOrDefault();

                if (_obj != null)
                {
                    return View(_obj);
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpPost]
        public ActionResult UpdateSimReg(update_sim_reg_model Item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    long reg_cust_id = _sim_repo.update_sim_reg_cust(Item);
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpGet]
        public ActionResult UpdateSimReplaceReg(long id)
        {
            try
            {
                var _obj = _sim_repo.get_replace_sim_customers().Where(m => m.cust.cust_id == id).FirstOrDefault();
                if (_obj != null)
                {
                    return View(_obj);
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpPost]
        public ActionResult UpdateSimReplaceReg(update_replace_sim_reg_model Item)
        {
            try
            {
                long reg_cust_id = _sim_repo.update_sim_replace_reg_cust(Item);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        #endregion

        #region Shopping Admin

        #region products

        public ActionResult Products(int? page, string sProduct, string sModelNo, string sCategory, string sBrand)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Products"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<all_products_model> oPM = new List<all_products_model>();
            ViewData["sProduct"] = sProduct;
            ViewData["sModelNo"] = sModelNo;
            ViewData["sCategory"] = sCategory;
            ViewData["sBrand"] = sBrand;

            try
            {
                oPM = _shop_repo.get_products().OrderByDescending(m => m.product.Product_ID).ToList();
                if (oPM.Count > 0)
                {
                    //ViewBag.prstatus = FillPromoStatus(oPM);

                    if (!string.IsNullOrWhiteSpace(sProduct))
                        oPM = oPM.Where(b => b.product.Product_Name.Trim().ToLower().Contains(sProduct.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(sModelNo))
                        oPM = oPM.Where(b => b.product.Model_No.Trim().ToLower().Contains(sModelNo.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(sCategory))
                        oPM = oPM.Where(b => b.category.Category_Name.Trim().ToLower().Contains(sCategory.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(sBrand))
                        oPM = oPM.Where(b => b.brand.Brand_Name.Trim().ToLower().Contains(sBrand.Trim().ToLower())).ToList();

                }

                oPM = oPM.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Products", oPM);
                else
                    return View(oPM);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(oPM);
        }

        public ActionResult AddProduct()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Products"))
                return RedirectToAction("Logout");

            all_products_model obj_product = new all_products_model();

            try
            {
                List<SelectListItem> categories = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false))
                                                   select new SelectListItem
                                                   {
                                                       Text = n.Category_Name,
                                                       Value = n.Category_ID.ToString()
                                                   }).ToList();
                ViewBag.categoryList = categories;
                List<SelectListItem> brands = (from n in _UOW.brand_repo.Get(filter: (c => c.IsDeleted == false))
                                               select new SelectListItem
                                               {
                                                   Text = n.Brand_Name,
                                                   Value = n.Brand_ID.ToString()
                                               }).ToList();
                ViewBag.brandList = brands;

                obj_product.product = new web_tbl_products();

                obj_product.product.is_topup = true;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_product);

        }

        [HttpPost]
        public ActionResult AddProduct(all_products_model obj_product, HttpPostedFileBase image_item)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            string Image_validation = string.Empty;
            try
            {
                List<SelectListItem> categories = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false))
                                                   select new SelectListItem
                                                   {
                                                       Text = n.Category_Name,
                                                       Value = n.Category_ID.ToString()
                                                   }).ToList();

                List<SelectListItem> brands = (from n in _UOW.brand_repo.Get(filter: (c => c.IsDeleted == false))
                                               select new SelectListItem
                                               {
                                                   Text = n.Brand_Name,
                                                   Value = n.Brand_ID.ToString()
                                               }).ToList();

                ViewBag.categoryList = categories;
                ViewBag.brandList = brands;
                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());

                if (obj_product != null)
                {
                    obj_product.product.Creation_Date = DateTime.Now;
                    obj_product.product.IsDeleted = false;
                    obj_product.product.Last_Update_Date = DateTime.Now;
                    obj_product.product.Created_By = admin_id;
                }

                if (image_item != null)
                {
                    string Extension = image_item.FileName.Remove(0, image_item.FileName.LastIndexOf('.'));
                    if (Extension != "")
                    {
                        if (Extension.ToLower() == ".jpg" || Extension.ToLower() == ".jpeg" || Extension.ToLower() == ".png" || Extension.ToLower() == ".gif")
                        {
                            _UOW.products_repo.Insert(obj_product.product);
                            _UOW.Save();

                            Image small_image = _util_repo.ResizeImage(image_item.InputStream, 120, 80);
                            Image medium_image = _util_repo.ResizeImage(image_item.InputStream, 280, 187);
                            Image large_image = _util_repo.ResizeImage(image_item.InputStream, 400, 268);


                            string small_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_small.png";
                            string medium_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_medium.png";
                            string large_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_large.png";

                            small_image.Save(small_image_url, System.Drawing.Imaging.ImageFormat.Png);
                            medium_image.Save(medium_image_url, System.Drawing.Imaging.ImageFormat.Png);
                            large_image.Save(large_image_url, System.Drawing.Imaging.ImageFormat.Png);

                            web_tbl_products_img obj_pro_img = new web_tbl_products_img();
                            obj_pro_img.img_small = obj_product.product.Product_ID + "_img_small.png";
                            obj_pro_img.img_medium = obj_product.product.Product_ID + "_img_medium.png";
                            obj_pro_img.img_large = obj_product.product.Product_ID + "_img_large.png";
                            obj_pro_img.Product_ID = obj_product.product.Product_ID;
                            _UOW.products_img_repo.Insert(obj_pro_img);
                            _UOW.Save();

                        }
                        else
                        {
                            Image_validation = "Only jpg,png,gif file formats are allowed to upload..";
                            ViewBag.Image_valid = Image_validation;
                        }
                    }
                }
                else
                {
                    _UOW.products_repo.Insert(obj_product.product);
                    _UOW.Save();

                    web_tbl_products_img obj_pro_img = new web_tbl_products_img();
                    obj_pro_img.img_small = "no_img_small.jpg";
                    obj_pro_img.img_medium = "no_img_medium.jpg";
                    obj_pro_img.img_large = "no_img_large.jpg";
                    obj_pro_img.Product_ID = obj_product.product.Product_ID;
                    _UOW.products_img_repo.Insert(obj_pro_img);
                    _UOW.Save();
                }

                if (obj_product.product.Product_ID > 0)
                {
                    obj_product.brand_conj.product_id = obj_product.product.Product_ID;
                    _UOW.product_brand_conj_repo.Insert(obj_product.brand_conj);
                    _UOW.Save();
                    if (obj_product.brand_conj.brand_id > 0)
                    {
                        obj_product.category_conj.product_id = obj_product.brand_conj.product_id;
                        _UOW.product_category_conj.Insert(obj_product.category_conj);
                        _UOW.Save();
                        if (obj_product.category_conj.Category_id > 0)
                            sMsg = "Product added successfully";
                        else
                            sMsg = "Failed to add!";
                    }

                }

                if (ViewBag.Image_valid == null)
                    ViewBag.Message = sMsg;

                return View(obj_product);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        public ActionResult EditProduct(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Products"))
                return RedirectToAction("Logout");

            string sMsg = string.Empty;
            try
            {
                List<SelectListItem> categories = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false))
                                                   select new SelectListItem
                                                   {
                                                       Text = n.Category_Name,
                                                       Value = n.Category_ID.ToString()
                                                   }).ToList();

                List<SelectListItem> brands = (from n in _UOW.brand_repo.Get(filter: (c => c.IsDeleted == false))
                                               select new SelectListItem
                                               {
                                                   Text = n.Brand_Name,
                                                   Value = n.Brand_ID.ToString()
                                               }).ToList();

                ViewBag.categoryList = categories;
                ViewBag.brandList = brands;

                all_products_model obj_products = new all_products_model();
                obj_products.product = _UOW.products_repo.Get(filter: (m => m.Product_ID == id && m.IsDeleted == false)).FirstOrDefault();
                obj_products.brand_conj = _UOW.product_brand_conj_repo.Get(filter: (c => c.product_id == id)).FirstOrDefault();
                obj_products.category_conj = _UOW.product_category_conj.Get(filter: (c => c.product_id == id)).FirstOrDefault();
                obj_products.product_img = _UOW.products_img_repo.Get(filter: (c => c.Product_ID == id)).FirstOrDefault();

                if (obj_products.product != null && obj_products.brand_conj != null && obj_products.category_conj != null)
                    return View(obj_products);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult EditProduct(all_products_model obj_product, HttpPostedFileBase image_item)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            string Image_validation = string.Empty;
            long product_id = 0;
            try
            {
                List<SelectListItem> categories = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false))
                                                   select new SelectListItem
                                                   {
                                                       Text = n.Category_Name,
                                                       Value = n.Category_ID.ToString()
                                                   }).ToList();

                List<SelectListItem> brands = (from n in _UOW.brand_repo.Get(filter: (c => c.IsDeleted == false))
                                               select new SelectListItem
                                               {
                                                   Text = n.Brand_Name,
                                                   Value = n.Brand_ID.ToString()
                                               }).ToList();

                ViewBag.categoryList = categories;
                ViewBag.brandList = brands;
                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());
                if (obj_product != null)
                {
                    obj_product.product.IsDeleted = false;
                    obj_product.product.Last_Update_Date = DateTime.Now;
                    obj_product.product.Last_Updated_By = admin_id;
                }


                if (image_item != null)
                {
                    string Extension = image_item.FileName.Remove(0, image_item.FileName.LastIndexOf('.'));
                    if (Extension != "")
                    {
                        if (Extension.ToLower() == ".jpg" || Extension.ToLower() == ".jpeg" || Extension.ToLower() == ".png" || Extension.ToLower() == ".gif")
                        {
                            _UOW.products_repo.Update(obj_product.product);
                            _UOW.Save();

                            Image small_image = _util_repo.ResizeImage(image_item.InputStream, 120, 80);
                            Image medium_image = _util_repo.ResizeImage(image_item.InputStream, 280, 187);
                            Image large_image = _util_repo.ResizeImage(image_item.InputStream, 400, 268);


                            string small_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_small.png";
                            string medium_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_medium.png";
                            string large_image_url = Server.MapPath("~/pro_images/") + obj_product.product.Product_ID + "_img_large.png";

                            if (System.IO.File.Exists(small_image_url))
                                System.IO.File.Delete(small_image_url);

                            if (System.IO.File.Exists(medium_image_url))
                                System.IO.File.Delete(medium_image_url);

                            if (System.IO.File.Exists(large_image_url))
                                System.IO.File.Delete(large_image_url);


                            small_image.Save(small_image_url, System.Drawing.Imaging.ImageFormat.Png);
                            medium_image.Save(medium_image_url, System.Drawing.Imaging.ImageFormat.Png);
                            large_image.Save(large_image_url, System.Drawing.Imaging.ImageFormat.Png);

                            var obj_pro_img = _UOW.products_img_repo.Get(filter: (p => p.Product_ID == obj_product.product.Product_ID)).FirstOrDefault();

                            obj_pro_img.img_small = obj_product.product.Product_ID + "_img_small.png";
                            obj_pro_img.img_medium = obj_product.product.Product_ID + "_img_medium.png";
                            obj_pro_img.img_large = obj_product.product.Product_ID + "_img_large.png";
                            obj_pro_img.Product_ID = obj_product.product.Product_ID;
                            _UOW.products_img_repo.Update(obj_pro_img);
                            _UOW.Save();
                            product_id = obj_product.product.Product_ID;
                        }
                        else
                        {
                            Image_validation = "Only jpg,png,gif file formats are allowed to upload..";
                            //ViewBag.Image_valid = Image_validation;
                            product_id = 0;
                        }
                    }
                }
                else
                {
                    _UOW.products_repo.Update(obj_product.product);
                    _UOW.Save();

                    product_id = obj_product.product.Product_ID;
                }

                if (product_id > 0)
                {
                    var obj_brand = _UOW.product_brand_conj_repo.Get(filter: (m => m.product_id == product_id)).FirstOrDefault(); ;

                    obj_brand.brand_id = obj_product.brand_conj.brand_id;
                    _UOW.product_brand_conj_repo.Update(obj_brand);
                    _UOW.Save();
                    if (obj_product.brand_conj.brand_id > 0)
                    {
                        var obj_category = _UOW.product_category_conj.Get(filter: (m => m.product_id == product_id)).FirstOrDefault(); ;

                        obj_category.Category_id = obj_product.category_conj.Category_id;

                        _UOW.product_category_conj.Update(obj_category);
                        _UOW.Save();
                        if (obj_product.category_conj.Category_id > 0)
                        {
                            obj_product.product_img = _UOW.products_img_repo.Get(filter: (m => m.Product_ID == obj_product.product.Product_ID)).FirstOrDefault();
                            sMsg = "Product updated successfully";
                        }
                        else
                            sMsg = "Failed to update!";
                    }

                }

                ViewBag.Image_valid = Image_validation;

                if (string.IsNullOrEmpty(Image_validation))
                    ViewBag.Message = sMsg;


                return View(obj_product);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        public ActionResult DeleteProduct(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Products"))
                return RedirectToAction("Logout");

            string sMsg = "";
            try
            {
                if (id > 0)
                {
                    var products = _UOW.products_repo.GetByID(id);
                    long admin_id = Convert.ToInt64(Session["beadmin"].ToString());
                    if (products != null)
                    {
                        products.IsDeleted = true;
                        products.Deleted_Date = DateTime.Now;
                        products.Deleted_By = admin_id;
                        _UOW.products_repo.Update(products);
                        _UOW.Save();
                        sMsg = "products deleted successfully";
                        return RedirectToAction("Products", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";

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

        #region related products

        public ActionResult RelatedProducts()
        {

            return View();
        }



        #endregion

        #region Category

        public ActionResult Category(int? page, string sCategory, string sParentCategory)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Category"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<main_category_model> obj_category = new List<main_category_model>();

            ViewData["sCategory"] = sCategory;
            ViewData["sParentCategory"] = sParentCategory;
            try
            {
                obj_category = _shop_repo.get_all_category().ToList();
                if (obj_category.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(sCategory))
                        obj_category = obj_category.Where(b => b.category_name.Trim().ToLower().Contains(sCategory.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(sParentCategory))
                        obj_category = obj_category.Where(b => b.parent_cat_name != null && b.parent_cat_name.Trim().ToLower().Contains(sParentCategory.Trim().ToLower())).ToList();


                    obj_category = obj_category.OrderBy(m => m.parent_cat_id).ToPagedList(currentPageIndex, defaultPageSize);

                    if (obj_category.Count == 0)
                        sMsg = "Not found!";
                }
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Category", obj_category);
                else
                    return View(obj_category);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_category);
        }

        public ActionResult AddCategory()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Category"))
                return RedirectToAction("Logout");
            try
            {
                List<SelectListItem> parent_category = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false && c.Parent_ID == 0))
                                                        select new SelectListItem
                                                        {
                                                            Text = n.Category_Name,
                                                            Value = n.Category_ID.ToString()
                                                        }).ToList();
                ViewBag.parentcategory = parent_category;

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(web_tbl_category obj_cat, HttpPostedFileBase image_item)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            string Image_validation = string.Empty;
            try
            {
                List<SelectListItem> parent_category = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false && c.Parent_ID == 0))
                                                        select new SelectListItem
                                                        {
                                                            Text = n.Category_Name,
                                                            Value = n.Category_ID.ToString()
                                                        }).ToList();
                ViewBag.parentcategory = parent_category;

                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());

                if (obj_cat.Parent_ID == null)
                {
                    obj_cat.Parent_ID = 0;
                    obj_cat.Created_By = admin_id;
                }

                if (image_item != null)
                {
                    string Extension = image_item.FileName.Remove(0, image_item.FileName.LastIndexOf('.'));
                    if (Extension != "")
                    {
                        if (Extension.ToLower() == ".jpg" || Extension.ToLower() == ".jpeg" || Extension.ToLower() == ".png" || Extension.ToLower() == ".gif")
                        {
                            Image small_image = _util_repo.ResizeImage(image_item.InputStream, 120, 80);

                            if (small_image != null)
                            {

                                obj_cat.Creation_Date = DateTime.Now;
                                obj_cat.Last_Update_Date = DateTime.Now;
                                obj_cat.IsDeleted = false;
                                _UOW.web_tbl_category_repo.Insert(obj_cat);
                                _UOW.Save();

                                string small_image_url = Server.MapPath("~/cat_images/" + obj_cat.Category_ID + "_img_cat.png");

                                if (System.IO.File.Exists(small_image_url))
                                    System.IO.File.Delete(small_image_url);

                                small_image.Save(small_image_url, System.Drawing.Imaging.ImageFormat.Png);

                                obj_cat.Category_Img = obj_cat.Category_ID + "_img_cat.png";
                                _UOW.web_tbl_category_repo.Update(obj_cat);
                                _UOW.Save();
                            }

                        }
                        else
                        {
                            Image_validation = "Only jpg,png,gif file formats are allowed to upload..";
                            ViewBag.Image_valid = Image_validation;
                        }
                    }
                }
                else
                {
                    obj_cat.Creation_Date = DateTime.Now;
                    obj_cat.Last_Update_Date = DateTime.Now;
                    obj_cat.IsDeleted = false;
                    _UOW.web_tbl_category_repo.Insert(obj_cat);
                    _UOW.Save();
                }

                if (obj_cat.Category_ID > 0)
                    sMsg = "Category added successfully";
                else
                    sMsg = "Failed to add!";

                if (ViewBag.Image_valid == null)
                    ViewBag.Message = sMsg;



            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_cat);
        }

        public ActionResult EditCategory(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Category"))
                return RedirectToAction("Logout");

            try
            {
                List<SelectListItem> parent_category = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false && c.Parent_ID == 0))
                                                        select new SelectListItem
                                                        {
                                                            Text = n.Category_Name,
                                                            Value = n.Category_ID.ToString()
                                                        }).ToList();
                ViewBag.parentcategory = parent_category;

                if (id > 0)
                {
                    var category = _UOW.web_tbl_category_repo.Get(filter: (m => m.Category_ID == id && m.IsDeleted == false)).FirstOrDefault();
                    if (category != null)
                        return View(category);
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult EditCategory(web_tbl_category obj_cat, HttpPostedFileBase image_item)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            string Image_validation = string.Empty;
            try
            {
                List<SelectListItem> parent_category = (from n in _UOW.web_tbl_category_repo.Get(filter: (c => c.IsDeleted == false && c.Parent_ID == 0))
                                                        select new SelectListItem
                                                        {
                                                            Text = n.Category_Name,
                                                            Value = n.Category_ID.ToString()
                                                        }).ToList();
                ViewBag.parentcategory = parent_category;

                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());

                var categorylist = _UOW.web_tbl_category_repo.GetByID(obj_cat.Category_ID);
                if (categorylist != null)
                {
                    categorylist.Category_Name = obj_cat.Category_Name;
                    categorylist.Category_Description = obj_cat.Category_Description;
                    categorylist.MetaDescription = obj_cat.MetaDescription;
                    categorylist.MetaKeyword = obj_cat.MetaKeyword;
                    categorylist.PageTitle = obj_cat.PageTitle;
                    categorylist.Last_Update_Date = DateTime.Now;
                    categorylist.Last_Updated_By = admin_id;

                    if (obj_cat.Parent_ID == null)
                        categorylist.Parent_ID = 0;
                    else
                        categorylist.Parent_ID = obj_cat.Parent_ID;

                }

                if (image_item != null)
                {
                    string Extension = image_item.FileName.Remove(0, image_item.FileName.LastIndexOf('.'));
                    if (Extension != "")
                    {
                        if (Extension.ToLower() == ".jpg" || Extension.ToLower() == ".jpeg" || Extension.ToLower() == ".png" || Extension.ToLower() == ".gif")
                        {
                            Image small_image = _util_repo.ResizeImage(image_item.InputStream, 120, 80);

                            string small_image_url = Server.MapPath("~/cat_images/") + obj_cat.Category_ID + "_img_cat.png";

                            if (System.IO.File.Exists(small_image_url))
                                System.IO.File.Delete(small_image_url);

                            small_image.Save(small_image_url, System.Drawing.Imaging.ImageFormat.Png);



                            categorylist.Category_Img = obj_cat.Category_ID + "_img_cat.png";
                            _UOW.web_tbl_category_repo.Update(categorylist);
                            _UOW.Save();

                        }
                        else
                        {
                            Image_validation = "Only jpg,png,gif file formats are allowed to upload..";
                            ViewBag.Image_valid = Image_validation;
                        }
                    }
                }
                else
                {
                    _UOW.web_tbl_category_repo.Update(categorylist);
                    _UOW.Save();
                }

                if (obj_cat.Category_ID > 0)
                    sMsg = "Category updated successfully";
                else
                    sMsg = "Failed to update!";

                if (ViewBag.Image_valid == null)
                    ViewBag.Message = sMsg;


                return View(obj_cat);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        public ActionResult DeleteCategory(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Category"))
                return RedirectToAction("Logout");

            string sMsg = "";
            try
            {
                if (id > 0)
                {
                    var category = _UOW.web_tbl_category_repo.GetByID(id);

                    long admin_id = Convert.ToInt64(Session["beadmin"].ToString());

                    if (category != null)
                    {
                        category.IsDeleted = true;
                        category.Deleted_Date = DateTime.Now;
                        category.Deleted_By = admin_id;
                        _UOW.web_tbl_category_repo.Update(category);
                        _UOW.Save();
                        sMsg = "Category deleted successfully";
                        return RedirectToAction("Category", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";

                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public JsonResult is_category_exist(string Category_Name, long? Category_ID)
        {
            bool IsValid = false;
            try
            {
                if (Category_ID != null && Category_ID > 0)
                {
                    var category_exist = _UOW.web_tbl_category_repo.Get(filter: m => m.Category_Name.Trim().ToLower() == Category_Name.Trim().ToLower() && m.Category_ID != Category_ID && m.IsDeleted == false).FirstOrDefault();
                    if (category_exist == null)
                        IsValid = true;
                }
                else
                {
                    var category_exist = _UOW.web_tbl_category_repo.Get(filter: m => m.Category_Name.Trim().ToLower() == Category_Name.Trim().ToLower() && m.IsDeleted == false).FirstOrDefault();
                    if (category_exist == null)
                        IsValid = true;
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return Json(IsValid);
        }



        #endregion

        #region Brands

        public ActionResult Brands(int? page, string sBrandName)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Brands"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<web_tbl_brand> obj_brands = new List<web_tbl_brand>();
            ViewData["sBrandName"] = sBrandName;
            try
            {
                obj_brands = _UOW.brand_repo.Get(filter: (m => m.IsDeleted == false)).ToList();
                if (obj_brands.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(sBrandName))
                        obj_brands = obj_brands.Where(b => b.Brand_Name.Trim().ToLower().Contains(sBrandName.Trim().ToLower())).ToList();

                    obj_brands = obj_brands.ToPagedList(currentPageIndex, defaultPageSize);

                    if (obj_brands.Count == 0)
                        sMsg = "Not found!";
                }
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Brands", obj_brands);
                else
                    return View(obj_brands);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_brands);
        }


        public ActionResult AddBrand()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Brands"))
                return RedirectToAction("Logout");

            try
            {
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult AddBrand(web_tbl_brand obj_brand)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = string.Empty;
            try
            {
                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());

                if (obj_brand != null)
                {
                    obj_brand.Creation_Date = DateTime.Now;
                    obj_brand.IsDeleted = false;
                    obj_brand.Last_Update_Date = DateTime.Now;
                    obj_brand.Created_By = admin_id;
                    _UOW.brand_repo.Insert(obj_brand);
                    _UOW.Save();
                    if (obj_brand.Brand_ID > 0)
                        sMsg = "brand added successfully";
                    else
                        sMsg = "Failed to add!";
                }
                ViewBag.Message = sMsg;

                return View(obj_brand);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        public ActionResult EditBrand(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Brands"))
                return RedirectToAction("Logout");

            try
            {
                if (id > 0)
                {
                    var obj_brands = _UOW.brand_repo.Get(filter: (m => m.Brand_ID == id && m.IsDeleted == false)).FirstOrDefault();
                    if (obj_brands != null)
                        return View(obj_brands);
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult EditBrand(web_tbl_brand obj_brand)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = string.Empty;
            try
            {
                long admin_id = Convert.ToInt64(Session["beadmin"].ToString());
                if (obj_brand != null)
                {
                    obj_brand.IsDeleted = false;
                    obj_brand.Last_Update_Date = DateTime.Now;
                    obj_brand.Last_Updated_By = admin_id;
                    _UOW.brand_repo.Update(obj_brand);
                    _UOW.Save();
                    if (obj_brand.Brand_ID > 0)
                        sMsg = "brand updated successfully";
                    else
                        sMsg = "Failed to update!";
                }
                ViewBag.Message = sMsg;

                return View(obj_brand);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        public ActionResult DeleteBrand(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Brands"))
                return RedirectToAction("Logout");

            string sMsg = "";
            try
            {
                if (id > 0)
                {
                    var brand = _UOW.brand_repo.GetByID(id);
                    long admin_id = Convert.ToInt64(Session["beadmin"].ToString());
                    if (brand != null)
                    {
                        brand.IsDeleted = true;
                        brand.Deleted_By = admin_id;
                        brand.Deleted_Date = DateTime.Now;
                        _UOW.brand_repo.Update(brand);
                        _UOW.Save();
                        sMsg = "Brand deleted successfully";
                        ViewBag.Message = sMsg;
                        return RedirectToAction("Brands", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";

                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public JsonResult is_brand_exist(string Brand_Name, long? Brand_ID)
        {
            bool IsValid = false;
            try
            {
                if (Brand_ID != null && Brand_ID > 0)
                {
                    var brand = _UOW.brand_repo.Get(filter: (m => m.Brand_Name.Trim().ToLower() == Brand_Name.Trim().ToLower() && m.Brand_ID != Brand_ID && m.IsDeleted == false)).FirstOrDefault();
                    if (brand == null)
                        IsValid = true;
                }
                else
                {
                    var brand = _UOW.brand_repo.Get(filter: (m => m.Brand_Name.Trim().ToLower() == Brand_Name.Trim().ToLower() && m.IsDeleted == false)).FirstOrDefault();
                    if (brand == null)
                        IsValid = true;
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return Json(IsValid);
        }


        #endregion

        #region Shopping users

        public ActionResult Users(int? page, string sName, string sEmail, string sCity)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Users"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<all_user_details> obj_users = new List<all_user_details>();
            ViewData["sName"] = sName;
            ViewData["sEmail"] = sEmail;
            ViewData["sCity"] = sCity;
            try
            {
                obj_users = _user_repo.get_all_users().ToList();

                if (obj_users.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(sName))
                    {
                        //obj_users = obj_users.Where(b => b.user_contact.first_name.Trim().ToLower().Contains(sName.Trim().ToLower()) || b.user_contact.last_name.Trim().ToLower().Contains(sName.Trim().ToLower())).ToList();

                        obj_users = (from n in obj_users
                                     let name = (n.user_contact.first_name + " " + n.user_contact.last_name).Trim().ToLower()
                                     where name.Contains(sName.Trim().ToLower())
                                     select n).ToList();

                    }




                    if (!string.IsNullOrWhiteSpace(sEmail))
                        obj_users = obj_users.Where(b => b.user_contact.email.Trim().ToLower().Contains(sEmail.Trim().ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(sCity))
                        obj_users = obj_users.Where(b => b.user_contact.city.Trim().ToLower().Contains(sCity.Trim().ToLower())).ToList();

                    obj_users = obj_users.ToPagedList(currentPageIndex, defaultPageSize);

                    if (obj_users.Count == 0)
                        sMsg = "Not found!";
                }
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Users", obj_users);
                else
                    return View(obj_users);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_users);
        }

        #endregion

        #region Shopping orders

        private List<SelectListItem> FillPaymentGateway(IList<all_orders_model> oP)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var res = (from b in oP select new { paygateway = b.order_pay.payment_gateway }).Distinct().ToList();
            for (int i = 0; i < res.Count; i++)
            {
                var item = new SelectListItem { Text = res[i].paygateway, Value = res[i].paygateway };
                result.Add(item);
            }
            return result;
        }

        public ActionResult Orders(int? page, string sOrderNo, string sPayGateway, string sEmail, string sCity, string sFromOrder, string sToOrder)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Orders"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<all_orders_model> obj_orders = new List<all_orders_model>();
            ViewData["sOrderNo"] = sOrderNo;
            ViewData["sPayGateway"] = sPayGateway;
            ViewData["sEmail"] = sEmail;
            ViewData["sCity"] = sCity;
            ViewData["sFromOrder"] = sFromOrder;
            ViewData["sToOrder"] = sToOrder;
            try
            {
                obj_orders = _shop_repo.get_search_orders(sOrderNo, sPayGateway, sEmail, sCity, sFromOrder, sToOrder);

                if (obj_orders.Count > 0)
                {
                    ViewBag.PayGateWayList = FillPaymentGateway(obj_orders);

                    obj_orders = obj_orders.ToPagedList(currentPageIndex, defaultPageSize);

                    if (obj_orders.Count == 0)
                        sMsg = "Not found!";
                }

                ViewBag.Message = sMsg;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_Orders", obj_orders);
                else
                    return View(obj_orders);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View(obj_orders);
        }


        public ActionResult PeiEzyOrders(string transmerchantid, string mobile_no, string ddlOrderstatus, string sFrom, string sTo, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");


            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<all_orders_model> oPT = new List<all_orders_model>();
            ViewData["transmerchantid"] = transmerchantid;
            ViewData["mobile_no"] = mobile_no;

            ViewData["ddlOrderstatus"] = ddlOrderstatus;
            ViewData["sFrom"] = sFrom;
            ViewData["sTo"] = sTo;

            ViewBag.Orderstatuslist = FillPeiezyOrderStatus();
            // ViewBag.Dokustatuslist = FillDokuStatus();
            //ViewBag.PayTypelist = FillPayType();
            try
            {


                oPT = _shop_repo.get_peiezy_orders();



                if (!string.IsNullOrWhiteSpace(transmerchantid))
                    oPT = oPT.Where(d => d.order_pay.payment_transaction_number == transmerchantid).ToList();

                if (!string.IsNullOrWhiteSpace(mobile_no))
                    oPT = oPT.Where(d => d.order_items[0].product_name == mobile_no).ToList();



                if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
                    oPT = oPT.Where(d => d.order_pay.payment_status == ddlOrderstatus).ToList();

                if ((!string.IsNullOrWhiteSpace(sFrom)) && (!string.IsNullOrWhiteSpace(sTo)))
                {
                    DateTime dtFrom = Convert.ToDateTime(sFrom);
                    DateTime dtTo = Convert.ToDateTime(sTo).AddDays(1);
                    oPT = oPT.Where(t => t.order_pay.payment_datetime.Date >= dtFrom && t.order_pay.payment_datetime.Date <= dtTo).ToList();
                }
                if (oPT.Count > 0)
                    TempData["PeiEzyTransactions"] = oPT;

                oPT = oPT.ToPagedList(currentPageIndex, defaultPageSize);

                if (oPT.Count == 0)
                    sMsg = "No Details found matching your search criteria!";

                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_PeiezyTransactions", oPT);
                else
                    return View(oPT);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
            return View(oPT);
        }

        public void PeiezyTransactionCsvList()
        {

            try
            {

                if (TempData["PeiEzyTransactions"] != null)
                {
                    List<all_orders_model> TransReport = new List<all_orders_model>();

                    TransReport = (List<all_orders_model>)TempData["PeiEzyTransactions"];

                    if (TransReport != null && TransReport.Count > 0)
                    {
                        TempData["PeiEzyTransactions"] = TransReport;

                        string filename = "InternetBankingTransactions";

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"TransactionID\",\"Transaction Date\",\"Amount\",\"MSISDN\",\"TopUp Status\",\"Paid Type\",\"Email\"");

                        foreach (var line in TransReport)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",

                                                       line.order_pay.payment_transaction_number,
                                                       line.order_pay.payment_date,
                                                       line.order_pay.payment_total,
                                                       line.order_items[0].product_name,
                                                       line.order_pay.payment_status,
                                                       line.order_pay.payment_type,
                                                       line.order_items[0].product_sku));
                        }

                        Response.Write(sw.ToString());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
        
        }

        private List<SelectListItem> FillPaymentStatus()
        {
            List<SelectListItem> lstpay = new List<SelectListItem>();
            var orderStatus = _UOW.shopping_order_payment_repo.Get().Select(o=>o.payment_status).Distinct().ToList();
            if (orderStatus.Count > 0)
            {
                foreach (string st in orderStatus)
                {
                    var item = new SelectListItem { Text = st, Value = st };
                    lstpay.Add(item);
                }
            }
            return lstpay;
        }

        private List<SelectListItem> FillDeliveryStatus()
        {
            List<SelectListItem> lstpay = new List<SelectListItem>();
            var delStatus = _UOW.shopping_order_repo.Get().Select(o => o.delivery_status).Distinct().ToList();
            if (delStatus.Count > 0)
            {
                foreach (bool bD in delStatus)
                {
                    string st = "";
                    if (bD == true)
                        st = "Delivered";
                    else
                        st = "Pending";
                    var item = new SelectListItem { Text = st, Value = bD.ToString() };
                    lstpay.Add(item);
                }
            }
            return lstpay;
        }

        public ActionResult OrderSummary(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            //all_orders_model order_details = new all_orders_model();
            try
            {
                ViewBag.ddlpaymentstatus = FillPaymentStatus();
                ViewBag.ddldeliverystatus = FillDeliveryStatus();
                var order_details = _shop_repo.get_order_summary(id);
               
                if (order_details != null)
                    return View(order_details);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        [HttpPost]
        public ActionResult OrderSummary(order_summary_model obj_order)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = string.Empty;
            ViewBag.ddlpaymentstatus = FillPaymentStatus();
            ViewBag.ddldeliverystatus = FillDeliveryStatus();
            try
            {
                var order = _UOW.shopping_order_repo.Get(filter: (m => m.order_id == obj_order.order.order_id)).FirstOrDefault();
                if (order != null)
                {
                    order.order_is_delivery = obj_order.order.order_is_delivery;
                    _UOW.shopping_order_repo.Update(order);
                    _UOW.Save();

                    var order_payment = _UOW.shopping_order_payment_repo.Get(filter: (m => m.order_id == order.order_id)).FirstOrDefault();
                    if (order_payment != null)
                    {
                        order_payment.payment_status = obj_order.order_pay.payment_status;
                        _UOW.shopping_order_payment_repo.Update(order_payment);
                        _UOW.Save();

                        sMsg = "Order updated successfully";
                        ViewBag.Message = sMsg;
                        return RedirectToAction("Orders", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
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

        #endregion

        #region USERS

        private List<SelectListItem> FillRoles()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var roles = _UOW.role_repo.Get(filter: r => r.id != 1 && r.id != 2).ToList();
            if (roles.Count > 0)
            {
                foreach (var r in roles)
                {
                    var item = new SelectListItem { Text = r.role_name, Value = r.id.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }
        private List<SelectListItem> FillUserStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var userStatus = _UOW.care_user_repo.Get(filter: r => r.id != 1).Select(m => m.is_active).Distinct().ToList();
            if (userStatus.Count > 0)
            {
                foreach (var r in userStatus)
                {
                    var item = new SelectListItem { Text = (r == true ? "Active" : "Inactive"), Value = r.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }


        public ActionResult CareUsers(long? ddlRoles, string ddlStatus, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            long B_id = Convert.ToInt64(Session["beadmin"]);
            int R_id = Convert.ToInt32(Session["role_id"]);
            if (R_id != 1)
            {
                if (!_user_repo.checkMenuAccess(B_id, R_id, "CareUsers"))
                    return RedirectToAction("Logout");
            }

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<web_tbl_care_user> users = new List<web_tbl_care_user>();
            ViewData["ddlRoles"] = ddlRoles;
            ViewData["ddlStatus"] = ddlStatus;
            try
            {
                ViewBag.vroles = FillRoles();
                ViewBag.vstatus = FillUserStatus();
                users = _UOW.care_user_repo.Get(filter: u => u.is_deleted == false && u.role_id!=1, orderBy: (b => b.OrderByDescending(o => o.id))).ToList();

                if (ddlRoles > 0 && ddlRoles != null)
                    users = users.Where(r => r.role_id == ddlRoles).ToList();
                if (!string.IsNullOrEmpty(ddlStatus))
                {
                    bool bStatus = false;
                    bool.TryParse(ddlStatus, out bStatus);
                    users = users.Where(r => r.is_active == bStatus).ToList();
                }

                users = users.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_CareUsers", users);
                else
                    return View(users);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(users);
        }

        public ActionResult CreateUser()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            Care_User_Model user = new Care_User_Model();
            try
            {

                ViewBag.vroles = FillRoles();
                user.MenuList = _user_repo.GetCareUserMenuList(null);

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(user);
        }

        [HttpPost]
        public ActionResult CreateUser(Care_User_Model user)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            try
            {
                ViewBag.vroles = FillRoles();
                if (user != null)
                {
                    web_tbl_care_user _obj = new web_tbl_care_user();
                    _obj.first_name = user.first_name;
                    _obj.last_name = user.last_name;
                    _obj.user_name = user.user_name;
                    _obj.user_pwd = user.user_pwd;
                    _obj.role_id = user.role_id;
                    _obj.contact_number = user.contact_number;
                    _obj.email_id = user.email_id;
                    _obj.creadted_on = DateTime.Now;
                    _obj.is_active = user.is_active;

                    _UOW.care_user_repo.Insert(_obj);
                    _UOW.Save();

                    if (_obj.id > 0)
                    {
                        // insert user_access
                        foreach (var useracc in user.MenuList)
                        {
                            if (useracc.active)
                            {
                                if (useracc.Menu_Id > 0)
                                {
                                    tbl_user_menu_conj objUserAccess = new tbl_user_menu_conj();
                                    objUserAccess.menu_id = Convert.ToInt32(useracc.Menu_Id);
                                    objUserAccess.user_id = _obj.id;
                                    _UOW.user_menu_conj_repo.Insert(objUserAccess);
                                    _UOW.Save();
                                }
                            }
                        }

                        var BS_Menu = user.MenuList.Where(f => f.Menu_Id < 0).ToList();
                        if (BS_Menu.Count > 0)
                        {
                            foreach (var item in BS_Menu)
                            {
                                if (item.active)
                                {
                                    var cMenu = _UOW.menu_repo.Get(f => f.parent_title == item.menu_name).ToList();
                                    if (cMenu.Count > 0)
                                    {
                                        foreach (var cSub in cMenu)
                                        {
                                            tbl_user_menu_conj oUAccess = new tbl_user_menu_conj();
                                            oUAccess.menu_id = Convert.ToInt32(cSub.id);
                                            oUAccess.user_id = _obj.id;
                                            _UOW.user_menu_conj_repo.Insert(oUAccess);
                                            _UOW.Save();
                                        }
                                    }
                                }
                            }
                        }

                        if (user.is_send_login)
                            EmailCareUser(_obj.user_name, _obj.email_id, _obj.first_name + " " + _obj.last_name, _obj.user_pwd);

                        sMsg = "User Created Successfully";
                        user.MenuList = _user_repo.GetCareUserMenuList(_obj.id);
                    }
                    else
                        sMsg = "Failed Create Care User";

                    ViewBag.Message = sMsg;
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(user);
        }

        public JsonResult UserExists(Care_User_Model user)
        {
            var userExists = _UOW.care_user_repo.Get(filter: u => u.user_name == user.user_name && u.role_id == user.role_id && u.is_deleted == false).ToList();
            if (userExists.Count > 0)
                return Json(false, JsonRequestBehavior.AllowGet);
            else
                return Json(true, JsonRequestBehavior.AllowGet);
        }



        public ActionResult EditUser(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            Care_User_Model _obj_user = new Care_User_Model();
            try
            {
                ViewBag.vroles = FillRoles();
                if (id > 0)
                {
                    var user = _UOW.care_user_repo.GetByID(id);
                    if (user != null)
                    {
                        _obj_user.id = user.id;
                        _obj_user.role_id = user.role_id;
                        _obj_user.user_name = user.user_name;
                        _obj_user.user_pwd = user.user_pwd;
                        _obj_user.email_id = user.email_id;
                        _obj_user.contact_number = user.contact_number;
                        _obj_user.first_name = user.first_name;
                        _obj_user.last_name = user.last_name;
                        _obj_user.is_active = user.is_active;
                        _obj_user.user_pwd = user.user_pwd;
                        _obj_user.confirm_pwd = user.user_pwd;
                    }
                    _obj_user.MenuList = _user_repo.GetCareUserMenuList(_obj_user.id);
                }
                else
                    _obj_user.MenuList = _user_repo.GetCareUserMenuList(null);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(_obj_user);
        }

        [HttpPost]
        public ActionResult EditUser(Care_User_Model _obj_user)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            string sMsg = string.Empty;
            try
            {
                ViewBag.vroles = FillRoles();
                if (_obj_user != null)
                {
                    web_tbl_care_user _obj = new web_tbl_care_user();
                    _obj = _UOW.care_user_repo.GetByID(_obj_user.id);
                    if (_obj != null)
                    {
                        _obj.role_id = _obj_user.role_id;
                        _obj.first_name = _obj_user.first_name;
                        _obj.last_name = _obj_user.last_name;
                        _obj.email_id = _obj_user.email_id;
                        _obj.contact_number = _obj_user.email_id;
                        _obj.is_active = _obj_user.is_active;
                        _obj.modified_on = DateTime.Now;
                        _obj.user_pwd = _obj_user.user_pwd;
                        _UOW.care_user_repo.Update(_obj);
                        _UOW.Save();

                        // Delete existing user_access_levels
                        var user_acc = _UOW.user_menu_conj_repo.Get(filter: f => f.user_id == _obj.id).ToList();
                        foreach (var item in user_acc)
                        {
                            _UOW.user_menu_conj_repo.Delete(item);
                            _UOW.Save();
                        }

                        // insert user_access
                        foreach (var useracc in _obj_user.MenuList)
                        {
                            if (useracc.active)
                            {
                                if (useracc.Menu_Id > 0)
                                {
                                    tbl_user_menu_conj objUserAccess = new tbl_user_menu_conj();
                                    objUserAccess.menu_id = Convert.ToInt32(useracc.Menu_Id);
                                    objUserAccess.user_id = _obj.id;
                                    _UOW.user_menu_conj_repo.Insert(objUserAccess);
                                    _UOW.Save();
                                }
                            }
                        }

                        var BS_Menu = _obj_user.MenuList.Where(f => f.Menu_Id < 0).ToList();
                        if (BS_Menu.Count > 0)
                        {
                            foreach (var item in BS_Menu)
                            {
                                if (item.active)
                                {
                                    var cMenu = _UOW.menu_repo.Get(f => f.parent_title == item.menu_name).ToList();
                                    if (cMenu.Count > 0)
                                    {
                                        foreach (var cSub in cMenu)
                                        {
                                            tbl_user_menu_conj oUAccess = new tbl_user_menu_conj();
                                            oUAccess.menu_id = Convert.ToInt32(cSub.id);
                                            oUAccess.user_id = _obj.id;
                                            _UOW.user_menu_conj_repo.Insert(oUAccess);
                                            _UOW.Save();
                                        }
                                    }
                                }
                            }
                        }

                        if (_obj.id > 0)
                        {
                            sMsg = "User Detail Updated Successfully";

                            if(_obj_user.is_send_login)
                            {
                                string name = _obj_user.first_name+" "+_obj_user.last_name;

                                EmailCareUser(_obj_user.user_name, _obj_user.email_id, name, _obj_user.user_pwd);
                            }
                        }
                        else
                            sMsg = "User Detail Updation is Falied";

                        _obj_user.MenuList = _user_repo.GetCareUserMenuList(_obj.id);
                        ViewBag.Message = sMsg;
                    }
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View(_obj_user);
        }

        public ActionResult DelUser(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string sMsg = "";
            try
            {
                if (id > 0)
                {
                    web_tbl_care_user user = new web_tbl_care_user();
                    user = _UOW.care_user_repo.GetByID(id);
                    if (user != null)
                    {
                        user.is_deleted = true;
                        _UOW.care_user_repo.Update(user);
                        _UOW.Save();
                    }
                    if (user.id > 0)
                    {
                        sMsg = "User deleted successfully";
                        return RedirectToAction("CareUsers", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";

                }
                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);

            }
            return View();
        }

        #region EmailUser(Method)

        public void EmailCareUser(string username, string _email, string _name, string _pwd)
        {
            string login_url = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "") + "/Admin/Login";

            XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
            XElement emailsubj = doc.Element("CareUserEmail_Subj");
            XElement emailBody = doc.Element("CareUserEmail_Body");
            string sData = emailBody.Value;
            sData = sData.Replace("#uname#", username).Replace("#pwd#", _pwd).Replace("#Name#", _name).Replace("#URL#", login_url);

            _util_repo.SendEmailMessage(_email, emailsubj.Value.Trim(), sData);
        }

        #endregion
        #endregion

        #region Import Staffs Topup

        public ActionResult ImportStaffsTopup()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("ImportStaffsTopup"))
                return RedirectToAction("Logout");

            List<StaffsTopupModel> objStaffsList = new List<StaffsTopupModel>();
            if (Session["staffs_data"] != null)
                objStaffsList = (List<StaffsTopupModel>)Session["staffs_data"];

            return View(objStaffsList);
        }

        [HttpPost]
        public ActionResult ImportStaffsTopup(HttpPostedFileBase StaffsTopupCsvfile)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            List<StaffsTopupModel> objStaffsList = new List<StaffsTopupModel>();
            try
            {
                if (StaffsTopupCsvfile != null)
                {
                    var extension = Path.GetExtension(StaffsTopupCsvfile.FileName ?? string.Empty);
                    if (extension.ToLower() == ".csv")
                    {
                        bool is_imported = false;
                        using (var reader = new StreamReader(StaffsTopupCsvfile.InputStream))
                        {
                            var SkipColumnName = 0;
                            var csv_items = reader.ReadToEnd().Split('\n');
                            var vp_item = from line in csv_items select line.Replace("\r", "").Split(',').ToList();

                            foreach (var item in vp_item)
                            {
                                StaffsTopupModel objStaffsModel = new StaffsTopupModel();
                                if (SkipColumnName > 0 && item.Count == 6)
                                {
                                    string current_msisdn = item[2];

                                    var check_alreadyAvailable = _UOW.staffs_topup_repo.Get(filter: T => T.msisdn_number == current_msisdn && T.is_deleted == false).LastOrDefault();

                                    if (check_alreadyAvailable != null)
                                    {
                                        DateTime check_date = Convert.ToDateTime(check_alreadyAvailable.created_on);

                                        if (check_date.Date != DateTime.Now.Date)
                                            check_alreadyAvailable = null;
                                    }

                                    if (check_alreadyAvailable == null)
                                    {
                                        objStaffsModel.description = "";

                                        if (!string.IsNullOrEmpty(item[0]))
                                            objStaffsModel.first_name = item[0];
                                        else
                                            objStaffsModel.description = "<div class='ifail'>First Name Required!</div>";

                                        if (!string.IsNullOrEmpty(item[1]))
                                            objStaffsModel.last_name = item[1];
                                        else
                                            objStaffsModel.description = "<div class='ifail'>Last Name Required!</div>";

                                        if (!string.IsNullOrEmpty(item[2]))
                                            objStaffsModel.msisdn_number = item[2];
                                        else
                                            objStaffsModel.description += "<div class='ifail'>MSISDN No Required!</div>";

                                        decimal amt = 0;
                                        if (decimal.TryParse(item[3], out amt))
                                            objStaffsModel.amount = amt;
                                        else
                                            objStaffsModel.description += "<div class='ifail'>Invalid Amount!</div>";

                                        string valid_email = item[5];
                                        if (!string.IsNullOrEmpty(valid_email))
                                        {
                                            bool bValEmail = IsValidEmailAddress(valid_email);
                                            if (bValEmail == false)
                                            {
                                                valid_email = "";
                                                objStaffsModel.description += "<div class='ifail'>Invalid Email!</div>";
                                            }
                                        }

                                        objStaffsModel.invoice = item[4];
                                        objStaffsModel.email = valid_email;
                                        objStaffsModel.created_on = DateTime.Now;
                                        objStaffsModel.is_recharged = false;

                                        if (string.IsNullOrEmpty(objStaffsModel.description))
                                        {
                                            //var objStaff = _UOW.staffs_topup_repo.Get(filter: s => s.invoice == objStaffsModel.invoice).FirstOrDefault();
                                            //if (objStaff == null)
                                            // {
                                            bm_staffs_topup stopup = new bm_staffs_topup();
                                            stopup.first_name = objStaffsModel.first_name;
                                            stopup.last_name = objStaffsModel.last_name;
                                            stopup.msisdn_number = objStaffsModel.msisdn_number;
                                            stopup.amount = (decimal)objStaffsModel.amount;
                                            stopup.invoice = objStaffsModel.invoice;
                                            stopup.email = objStaffsModel.email;
                                            stopup.created_on = DateTime.Now;
                                            stopup.is_recharged = false;
                                            stopup.is_active = true;
                                            stopup.is_deleted = false;

                                            _UOW.staffs_topup_repo.Insert(stopup);
                                            _UOW.Save();

                                            objStaffsModel.description = "<div class='isuccess'>Imported Successfully</div>";
                                            is_imported = true;
                                            // }
                                            //  else
                                            //      objStaffsModel.description = "<div class='ifail'>This Invoice already Exists!</div>";
                                        }

                                        objStaffsList.Add(objStaffsModel);
                                    }
                                    else
                                    {
                                        ViewBag.Msg = "Same data already imported. Please verify the file.";
                                    }
                                }

                                SkipColumnName++;
                            }
                        }

                        if (is_imported)
                        {
                            ViewBag.SuccessMsg = "Import Successful";
                        }
                        else if (ViewBag.Msg == null)
                        {
                            ViewBag.Msg = "No data imported. Please verify the file.";
                        }

                        Session["staffs_data"] = objStaffsList;
                    }
                    else
                    {
                        ViewBag.Msg = "File should be .csv format";
                    }
                }
                else
                {
                    ViewBag.Msg = "Please upload file";
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View(objStaffsList);
        }

        private bool IsValidEmailAddress(string emailAddress)
        {
            bool bRes = true;
            if (!string.IsNullOrEmpty(emailAddress))
            {
                string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                         @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(emailRegex);
                if (!re.IsMatch(emailAddress))
                {
                    bRes = false;
                }
            }
            return bRes;
        }
        #endregion

        #region Deduct Staffs Topup

        public ActionResult DeductStaffsTopup()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("DeductStaffsTopup"))
                return RedirectToAction("Logout");

            List<StaffsTopupModel> objStaffsList = new List<StaffsTopupModel>();
            if (Session["staffs_data"] != null)
                objStaffsList = (List<StaffsTopupModel>)Session["staffs_data"];

            return View(objStaffsList);
        }

        [HttpPost]
        public ActionResult DeductStaffsTopup(HttpPostedFileBase StaffsTopupCsvfile)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            List<StaffsTopupModel> objStaffsList = new List<StaffsTopupModel>();
            try
            {
                if (StaffsTopupCsvfile != null)
                {
                    var extension = Path.GetExtension(StaffsTopupCsvfile.FileName ?? string.Empty);
                    if (extension.ToLower() == ".csv")
                    {
                        bool is_imported = false;
                        using (var reader = new StreamReader(StaffsTopupCsvfile.InputStream))
                        {
                            var SkipColumnName = 0;
                            var csv_items = reader.ReadToEnd().Split('\n');
                            var vp_item = from line in csv_items select line.Replace("\r", "").Split(',').ToList();

                            foreach (var item in vp_item)
                            {
                                StaffsTopupModel objStaffsModel = new StaffsTopupModel();
                                if (SkipColumnName > 0 && item.Count == 2)
                                {
                                    objStaffsModel.description = "";


                                    if (!string.IsNullOrEmpty(item[0]))
                                        objStaffsModel.msisdn_number = item[0];
                                    else
                                        objStaffsModel.description += "<div class='ifail'>MSISDN No Required!</div>";

                                    decimal amt = 0;
                                    if (decimal.TryParse(item[1], out amt))
                                        objStaffsModel.amount = amt;
                                    else
                                        objStaffsModel.description += "<div class='ifail'>Invalid Amount!</div>";

                                    //objStaffsModel.invoice = item[4];
                                    //objStaffsModel.email = item[5];
                                    objStaffsModel.created_on = DateTime.Now;
                                    objStaffsModel.is_recharged = false;

                                    if (string.IsNullOrEmpty(objStaffsModel.description))
                                    {
                                        ////var objStaff = _UOW.staffs_topup_repo.Get(filter: s => s.invoice == objStaffsModel.invoice).FirstOrDefault();
                                        ////if (objStaff == null)
                                        //// {
                                        bm_staffs_topup stopup = new bm_staffs_topup();
                                        //stopup.first_name = objStaffsModel.first_name;
                                        //stopup.last_name = objStaffsModel.last_name;
                                        stopup.msisdn_number = objStaffsModel.msisdn_number;
                                        stopup.amount = (decimal)objStaffsModel.amount;
                                        //stopup.invoice = objStaffsModel.invoice;
                                        //stopup.email = objStaffsModel.email;
                                        stopup.created_on = DateTime.Now;
                                        stopup.is_recharged = false;
                                        stopup.is_active = true;
                                        stopup.is_deleted = false;

                                        //_UOW.staffs_topup_repo.Insert(stopup);
                                        //_UOW.Save();
                                        var success = _staffs_topup_repo.staff_deduction(stopup);
                                        if (success == true)
                                        {
                                            objStaffsModel.description = "<div class='isuccess'>Amount deducted Successfully</div>";
                                            is_imported = true;
                                        }
                                        // }
                                        //  else
                                        //      objStaffsModel.description = "<div class='ifail'>This Invoice already Exists!</div>";
                                    }

                                    objStaffsList.Add(objStaffsModel);
                                }

                                SkipColumnName++;
                            }
                        }

                        if (is_imported)
                        {
                            ViewBag.SuccessMsg = "Amount deducted Successfully";
                        }
                        else if (ViewBag.Msg == null)
                        {
                            ViewBag.Msg = "No data imported. Please verify the file.";
                        }

                        Session["staffs_data"] = objStaffsList;
                    }
                    else
                    {
                        ViewBag.Msg = "File should be .csv format";
                    }
                }
                else
                {
                    ViewBag.Msg = "Please upload file";
                }
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View(objStaffsList);
        }

        #endregion

        #region Staffs MSISDN

        public ActionResult staffs_msisdn(int? page, string staffs, string msisdn, string invoice, string sBrand)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("staffs_msisdn"))
                return RedirectToAction("Logout");
            try
            {
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewData["staffs"] = staffs;
                ViewData["msisdn"] = msisdn;
                ViewData["invoice"] = invoice;
                ViewData["sBrand"] = sBrand;
                IList<bm_staffs_topup> staffsMSISDN = new List<bm_staffs_topup>();

                staffsMSISDN = _UOW.staffs_topup_repo.Get(filter: S => S.is_deleted == false).ToList();
                if (staffsMSISDN.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(staffs))
                        staffsMSISDN = staffsMSISDN.Where(S => S.first_name.Contains(staffs) || S.last_name.Contains(staffs)).ToList();

                    if (!string.IsNullOrEmpty(msisdn))
                        staffsMSISDN = staffsMSISDN.Where(S => S.msisdn_number.Trim() == msisdn).ToList();

                    if (!string.IsNullOrEmpty(invoice))
                        staffsMSISDN = staffsMSISDN.Where(S => S.invoice.Trim() == invoice).ToList();
                }

                staffsMSISDN = staffsMSISDN.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_StaffsMSISDN", staffsMSISDN);
                else
                    return View(staffsMSISDN);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }

        #endregion

        #region Staff Process

        public ActionResult staff_process()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("staff_process"))
                return RedirectToAction("Logout");
            try
            {
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpPost]
        public ActionResult staff_process(string obj_staff)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {
                var success = _staffs_topup_repo.SendStaffsTopupEmail();

                if (success == true)
                    ViewBag.Msg = "Staffs Process Completed successfully";
                else
                    ViewBag.Msg = "No records found to process!";

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }


        #endregion

        #region Staffs MSISDN

        public ActionResult staffs_transactions(int? page, string sTransFrom, string sTransTo, string sMsisdn, string sInvoice, string sEmail)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("staffs_transactions"))
                return RedirectToAction("Logout");
            try
            {
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewData["sTransFrom"] = sTransFrom;
                ViewData["sTransTo"] = sTransTo;
                ViewData["sMsisdn"] = sMsisdn;
                ViewData["sInvoice"] = sInvoice;
                ViewData["sEmail"] = sEmail;
                IList<bm_staffs_trans> staffs_trans = new List<bm_staffs_trans>();

                staffs_trans = _UOW.staff_trans_repo.Get().ToList();
                if (staffs_trans.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(sMsisdn))
                        staffs_trans = staffs_trans.Where(S => S.msisdn_number == sMsisdn).ToList();

                    if (!string.IsNullOrEmpty(sInvoice))
                        staffs_trans = staffs_trans.Where(S => S.invoice_number == sInvoice).ToList();

                    if (!string.IsNullOrEmpty(sEmail))
                        staffs_trans = staffs_trans.Where(S => S.email == sEmail).ToList();

                    if (!string.IsNullOrEmpty(sTransFrom))
                    {
                        DateTime from_date = Convert.ToDateTime(sTransFrom);
                        staffs_trans = staffs_trans.Where(S => S.trans_date.Date >= from_date.Date).ToList();
                    }

                    if (!string.IsNullOrEmpty(sTransTo))
                    {
                        DateTime to_date = Convert.ToDateTime(sTransTo);
                        staffs_trans = staffs_trans.Where(S => S.trans_date.Date <= to_date.Date).ToList();
                    }


                }

                staffs_trans = staffs_trans.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_staffs_transactions", staffs_trans);
                else
                    return View(staffs_trans);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }

        #endregion

        #region menu

        public ActionResult menus(int? page)
        {

            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("menus"))
                return RedirectToAction("Logout");
            try
            {
                IList<Menu_Model> obj_menus = new List<Menu_Model>();

                string sMsg = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;

                var obj_menu = _user_repo.get_all_menu();
                obj_menus = obj_menu.ToPagedList(currentPageIndex, defaultPageSize);

                if (obj_menus.Count == 0)
                    sMsg = "Not found!";

                ViewBag.Message = sMsg;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_menus", obj_menus);
                else
                    return View(obj_menus);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }


        public ActionResult create_menu()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("menus"))
                return RedirectToAction("Logout");
            try
            {
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }

        [HttpPost]
        public ActionResult create_menu(tbl_menu obj_menu)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {
                string sMsg = "";
                if (obj_menu != null)
                {
                    _UOW.menu_repo.Insert(obj_menu);
                    _UOW.Save();

                    if (obj_menu.id > 0)
                        sMsg = "Menu added successfully";
                    else
                        sMsg = "Failed to add menu";

                    ViewBag.Message = sMsg;
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }

        public ActionResult update_menu(int id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("menus"))
                return RedirectToAction("Logout");
            try
            {
                var obj_menu = _UOW.menu_repo.Get(filter: s => s.id == id && s.is_deleted == false).FirstOrDefault();
                if (obj_menu != null)
                    return View(obj_menu);
            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }

        [HttpPost]
        public ActionResult update_menu(tbl_menu obj_menu)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            try
            {
                string sMsg = "";
                if (obj_menu != null)
                {
                    tbl_menu _obj = new tbl_menu();
                    _obj = _UOW.menu_repo.GetByID(obj_menu.id);
                    _obj.menu_name = obj_menu.menu_name;
                    _obj.page_name = obj_menu.page_name;
                    _obj.parent_title = obj_menu.parent_title;
                    _obj.is_active = obj_menu.is_active;
                    _UOW.menu_repo.Update(_obj);
                    _UOW.Save();

                    if (_obj.id > 0)
                        sMsg = "Menu updated successfully";
                    else
                        sMsg = "Failed to update menu";

                    ViewBag.Message = sMsg;
                }

            }
            catch (Exception ex)
            {
                 _util_repo.ErrorLog_Txt(ex);
            }

            return View();
        }

        public ActionResult delete_menu(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("menus"))
                return RedirectToAction("Logout");
            try
            {
                string sMsg = "";
                if (id > 0)
                {
                    var mainmenu = _UOW.menu_repo.Get(filter: s => s.is_deleted == false && s.id == id).FirstOrDefault();
                    if (mainmenu != null)
                    {
                        mainmenu.is_deleted = true;
                        _UOW.menu_repo.Update(mainmenu);
                        _UOW.Save();

                        sMsg = "Main menu deleted successfully";
                        return RedirectToAction("menus", "Admin");
                    }
                    else
                        sMsg = "Failed to delete!";
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

        #region CheckUserLevel

        private bool Checkuserlevel(string view_name)
        {
            bool bret = false;

            long B_id = Convert.ToInt64(Session["beadmin"]);
            int R_id = Convert.ToInt32(Session["role_id"]);
            if (R_id != 1)
            {
                if (_user_repo.checkMenuAccess(B_id, R_id, view_name))
                    bret = true;
            }
            else
                bret = true;
            return bret;
        }

        #endregion

        #region KYC Customers

        public ActionResult KYC_customers_List(int? page, string Mobile_no, string ddlProvince, string ddlDistrict)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<kyc_grid_list> ob = new List<kyc_grid_list>();
            try
            {
                ViewBag.ProvincialList = _kyc_repo.GetProvince();
                ViewBag.District = (from g in _kyc_repo.GetDistrict()
                                    select new SelectListItem
                                    {
                                        Value = g.DISTRICT_C.ToString(),
                                        Text = g.DISTRICT_N
                                    }).ToList();

                ob = _kyc_repo.getKycGridValue();

                if (!string.IsNullOrEmpty(Mobile_no))
                    ob = ob.Where(n => n.mobile_no == Mobile_no).ToList();

                if (!string.IsNullOrEmpty(ddlProvince) && (ddlProvince != "0"))
                {
                    string strProvince = _kyc_repo.GetProvinceNamebyProvinceID(Convert.ToInt32(ddlProvince));
                    ob = ob.Where(n => n.province == strProvince).ToList();
                }

                if (!string.IsNullOrEmpty(ddlDistrict) && (ddlDistrict != "0"))
                {
                    string strDistrict = _kyc_repo.GetDistrictNamebyDistrictID(Convert.ToInt32(ddlDistrict));
                    ob = ob.Where(n => n.town == strDistrict).ToList();
                }
                TempData["KYCCustomerList"] = ob;

                ob = ob.ToPagedList(currentPageIndex, defaultPageSize);



                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_KYC_CustomerList", ob);

                else
                    return View(ob);
            }

            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(ob);
            //return View();
        }

        public ActionResult KYC_SIM_Activation_Log(int? page, string MSISDN, string PUK, string SIM_No, string SimStatus)
        {

            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            bool sim_status = false;
            IList<tbl_kyc_simactivation_log> obj_sim_Activation_log = new List<tbl_kyc_simactivation_log>();
            try
            {
                obj_sim_Activation_log = _kyc_repo.get_Sim_Activation_Log_Details();

                ViewData["MSISDN"] = MSISDN;
                ViewData["PUK"] = PUK;
                ViewData["SIM_No"] = SIM_No;

                ViewData["SimStatus"] = SimStatus;


                if (!string.IsNullOrEmpty(MSISDN))
                    obj_sim_Activation_log = obj_sim_Activation_log.Where(n => n.msisdn_no == MSISDN).ToList();

                if (!string.IsNullOrEmpty(PUK))
                    obj_sim_Activation_log = obj_sim_Activation_log.Where(n => n.puk_code == PUK).ToList();

                if (!string.IsNullOrEmpty(SIM_No))
                    obj_sim_Activation_log = obj_sim_Activation_log.Where(n => n.sim_no == SIM_No).ToList();



                if (!string.IsNullOrEmpty(SimStatus))
                {
                    sim_status = (SimStatus == "Active") ? true : false;
                    obj_sim_Activation_log = obj_sim_Activation_log.Where(n => n.is_sim_active == sim_status).ToList();
                }

                TempData["kycSimActivationLog"] = obj_sim_Activation_log;

                int currentPageIndex = page.HasValue ? page.Value : 1;
                obj_sim_Activation_log = obj_sim_Activation_log.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_KYC_SimActivation_Log", obj_sim_Activation_log);

                else
                    return View(obj_sim_Activation_log);
            }

            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(obj_sim_Activation_log);
        }


        public ActionResult Update_Kyc_customer(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            kyc_Custome_Detail_Model kyc_cust_update_obj = new kyc_Custome_Detail_Model();
            try
            {

                if (id > 0)
                {
                    ViewBag.ProvincialList = _kyc_repo.GetProvince();
                    ViewBag.District = (from g in _kyc_repo.GetDistrict()
                                        select new SelectListItem
                                        {
                                            Value = g.DISTRICT_C.ToString(),
                                            Text = g.DISTRICT_N
                                        }).ToList();
                    kyc_cust_update_obj = _kyc_repo.getKYCCustomerDetailsById(id);
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(kyc_cust_update_obj);

        }
        [HttpPost]
        public ActionResult Update_Kyc_customer(kyc_Custome_Detail_Model kyc_cust_update_obj)
        {

            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            try
            {
                ViewBag.ProvincialList = _kyc_repo.GetProvince();
                ViewBag.District = (from g in _kyc_repo.GetDistrict()
                                    select new SelectListItem
                                    {
                                        Value = g.DISTRICT_C.ToString(),
                                        Text = g.DISTRICT_N
                                    }).ToList();

                if (kyc_cust_update_obj != null)
                {
                    int result = _kyc_repo.UpdateKyc_CustomerDetails(kyc_cust_update_obj);
                    if (result == 0)
                    {
                        ViewBag.Msg = "Updated Successfully";

                        // return RedirectToAction("KYC_Customers_List", "Admin");
                    }
                    if (result == 2)
                    {
                        ViewBag.Msg = "Please select any one identity details";
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(kyc_cust_update_obj);
        }

        public JsonResult DistrictByProvinceID(int id)
        {
            List<SelectListItem> obj = new List<SelectListItem>();
            List<DISTRICT_CODES> district = new List<DISTRICT_CODES>();
            district = _kyc_repo.GetDistrictByProvinceID(id);
            obj = (from g in district
                   select new SelectListItem
                   {
                       Text = g.DISTRICT_C.ToString(),
                       Value = g.DISTRICT_N

                   }).ToList();
            // obj.Insert(0, new SelectListItem { Text = "0", Value = "-- Choose District --" });
            JsonResult result = new JsonResult();
            result.Data = obj;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return result;

        }

        public void KYC_CustomerList_To_CSV()
        {
            try
            {
                if (TempData["KYCCustomerList"] != null)
                {

                    List<kyc_grid_list> obj_kyc_csv = new List<kyc_grid_list>();

                    obj_kyc_csv = (List<kyc_grid_list>)TempData["KYCCustomerList"];
                    TempData["KYCCustomerList"] = obj_kyc_csv;

                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment;filename=KYC_Customers_List.csv");
                    Response.ContentType = "text/csv";

                    StringWriter sw = new StringWriter();

                    sw.WriteLine("\"Customer Name\",\"Surname\",\"Sex\",\"Employed\",\"PO Box\",\"Town\",\"Province\",\"Suburb\",\"Email-ID\",\"Driving License Number\",\"Student ID Number\",\"Employee ID Number\",\"Nasfund ID Number\",\"Nambawan Super ID Number\",\"Other ID Number\",\"Customer Status\",\"Bmobile-Vodafone No_1\",\"Bmobile-Vodafone No_2\",\"Bmobile-Vodafone No_3\",\"Handset Model_1\",\"Handset Model_2\",\"Handset Model_3\",\"IMEI Number_1\",\"IMEI Number_2\",\"IMEI Number_3\",\"Purchase_Date\",\"Customer Signature\",\"Name Matched with ID\",\"Document Genuine\",\"Acknowledge date\",\"Signature\",\"DOB\",\"LT Reference\"");

                    foreach (var line in obj_kyc_csv)
                    {
                        string str_dob = Convert.ToDateTime(line.dob).ToShortDateString();
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\",\"{24}\",\"{25}\",\"{26}\",\"{27}\",\"{28}\",\"{29}\",\"{30}\",\"{31}\",\"{32}\"",
                                                   line.Name,
                                                   line.Surname,
                                                   line.Sex,
                                                   line.Is_employed,
                                                   line.pobox,
                                                   line.town,
                                                   line.province,
                                                   line.suburb,
                                                   line.email,
                                                   line.drv_lic,
                                                   line.student_id,
                                                   line.emp_id,
                                                   line.nasfund_id,
                                                   line.nbw_super_id,
                                                   line.other_id,
                                                   line.cust_status,
                                                   line.cust_mobile_no1,
                                                   line.cust_mobile_no2,
                                                   line.cust_mobile_no2,
                                                   line.cust_handset_model1,
                                                   line.cust_handset_model2,
                                                   line.cust_handset_model3,
                                                   line.cust_imei_no1,
                                                   line.cust_imei_no2,
                                                   line.cust_imei_no3,
                                                   line.Purchase_date,
                                                   line.Customersignature,
                                                   line.Is_nameMatched,
                                                   line.Is_documentGenuine,
                                                   line.Acknowledge_date,
                                                   line.is_officerSignature,
                                                   str_dob,
                                                   line.lt_ref
                                                   ));
                    }

                    Response.Write(sw.ToString());

                    Response.End();
                }
            }

            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

        }

        public void KYC_SIM_Activation_Log_to_List()
        {
            try
            {
                if (TempData["kycSimActivationLog"] != null)
                {
                    List<tbl_kyc_simactivation_log> obj_Sim_Activation_Log = new List<tbl_kyc_simactivation_log>();
                    obj_Sim_Activation_Log = (List<tbl_kyc_simactivation_log>)TempData["kycSimActivationLog"];

                    TempData["kycSimActivationLog"] = obj_Sim_Activation_Log;
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment;filename=KYC_SIM_Activation_Log.csv");
                    Response.ContentType = "text/csv";
                    StringWriter sw = new StringWriter();

                    sw.WriteLine("\"MSISDN Number\",\"PUK Code\",\"SIM Number\",\"Reference Number\",\"SIM Status\",\"Recorded on\",\"Activation Date\"");

                    foreach (var line in obj_Sim_Activation_Log)
                    {
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                                                   line.msisdn_no,
                                                   line.puk_code,
                                                   line.sim_no,
                                                   line.ref_no,
                                                   (line.is_sim_active == true) ? "Active" : "Inactive",
                                                   line.created_on,
                                                   line.activated_on


                                                   ));
                    }

                    Response.Write(sw.ToString());

                    Response.End();
                }
            }


            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

        }

        public ActionResult CreateKYCVersion()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public ActionResult CreateKYCVersion(tbl_kyc_version_update obj_kyc_version)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            int int_result = 1;
            try
            {
                if (obj_kyc_version != null)
                {
                    int_result = _kyc_repo.Create_KYC_Version(obj_kyc_version);
                    if (int_result == 0)
                        ViewBag.Message = "KYC Version Inserted Successfully";
                    else if (int_result == 2)
                        ViewBag.Message = "KYC Version Already Exist";
                    else
                        ViewBag.Message = "KYC Version not Inserted";
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(obj_kyc_version);
        }

        public ActionResult UpdateKYCVersion(long id)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            tbl_kyc_version_update obj_kyc_version = new tbl_kyc_version_update();
            try
            {
                if (id > 0)
                {
                    obj_kyc_version = _kyc_repo.Get_KYC_version_Details_By_ID(id);
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(obj_kyc_version);
        }

        [HttpPost]
        public ActionResult UpdateKYCVersion(tbl_kyc_version_update obj_kyc_version)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            int int_result = 1;
            try
            {
                if (obj_kyc_version != null)
                {
                    int_result = _kyc_repo.Update_KYC_Version(obj_kyc_version);
                    if (int_result == 0)
                        ViewBag.Message = "KYC Version Updated Successfully";
                    else
                        ViewBag.Message = "KYC Version not Updated";
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(obj_kyc_version);
        }

        public ActionResult KYC_Version_List(int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            IList<tbl_kyc_version_update> obj_kyc_version_list = new List<tbl_kyc_version_update>();
            try
            {
                int currentPageIndex = page.HasValue ? page.Value : 1;

                obj_kyc_version_list = _kyc_repo.Get_All_KYC_version_Details();

                obj_kyc_version_list = obj_kyc_version_list.ToPagedList(currentPageIndex, defaultPageSize);



                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_kyc_version_List", obj_kyc_version_list);

                else
                    return View(obj_kyc_version_list);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(obj_kyc_version_list);
        }

        public JsonResult check_kyc_version(decimal kyc_version, long ID)
        {
            bool bRet = false;

            bRet = _kyc_repo.Check_KYC_Version(kyc_version, ID);

            return Json(bRet);
        }

        public JsonResult delete_kyc_version(string id)
        {

            string status = "";


            status = _kyc_repo.delete_kyc_version(id).ToString();

            return Json(new { data = status });



        }

        public FileContentResult Get_KYC_Customer_Photo_By_ID(long id)
        {


            tbl_kyc_customer obj_kyc_customer = new tbl_kyc_customer();
            try
            {
                obj_kyc_customer = _kyc_repo.Get_KYC_Customer_Details_By_ID(id);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return new FileContentResult(obj_kyc_customer.photo_image, "image/jpeg");
        }

        #endregion

        #region top kad
        public ActionResult VerifyTOPKAD()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");


            return View();
        }
        [HttpPost]
        public ActionResult VerifyTopKad(top_kad_verify_model obj_top_kad)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");
            string dec_json_result = ""; bool check_limit = false;
            top_kad_return_model obj_return = new top_kad_return_model();
            decimal recharge_amount = 0;
            try
            {
                if (obj_top_kad != null)
                {
                    if (Session["role_id"].ToString() == "1")
                    {
                        dec_json_result = _care_repo.verify_topkad_serial(obj_top_kad);
                        if (!string.IsNullOrEmpty(dec_json_result))
                        {
                            obj_return = JsonConvert.DeserializeObject<top_kad_return_model>(dec_json_result);
                            if (obj_return != null && obj_return.resultcode == "0")
                            {
                                dec_json_result = _care_repo.top_up_topkad_serial(obj_top_kad);
                                obj_return = JsonConvert.DeserializeObject<top_kad_return_model>(dec_json_result);
                                if (obj_return != null && obj_return.resultcode == "0")
                                {
                                    if (obj_return.rechargedAmount != null)
                                    {
                                        recharge_amount = Convert.ToDecimal(obj_return.rechargedAmount);
                                        obj_top_kad.recharge_amount = recharge_amount;
                                        obj_top_kad.is_recharged = true;
                                    }
                                }
                                else
                                {
                                    obj_top_kad.recharge_amount = 0;
                                    obj_top_kad.is_recharged = false;
                                }
                            }
                            else
                            {
                                obj_top_kad.recharge_amount = recharge_amount;
                                obj_top_kad.is_recharged = false;
                            }

                            obj_top_kad.server_response = obj_return.resultcode;
                            obj_top_kad.server_reference = obj_return.reference;
                            obj_top_kad.sever_desc = obj_return.desc;
                            obj_top_kad.updated_by = Session["beadmin"].ToString();
                            XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/TopKadErrorCode.xml"));
                            XElement ErrorDesc = doc.Element("E" + obj_return.resultcode);
                            if (ErrorDesc != null)
                            {
                                if (obj_return.resultcode != null && obj_return.resultcode == "0")
                                    ViewBag.Msg = ErrorDesc.Value + " " + obj_return.rechargedAmount;
                                else
                                    ViewBag.Msg = ErrorDesc.Value;

                                obj_top_kad.sever_desc = ErrorDesc.Value;

                                if (obj_top_kad.sever_desc == null)
                                    obj_top_kad.sever_desc = obj_return.desc;
                                if (obj_top_kad.is_recharged == true)
                                    obj_top_kad.sever_desc += obj_return.rechargedAmount.ToString();
                            }
                            else
                                ViewBag.Msg = obj_return.desc;
                            int iresult = _care_repo.insert_top_kad_log(obj_top_kad);
                        }


                    }
                    else
                    {
                        string UserId = Session["beadmin"].ToString();
                        List<tbl_top_kad_log> obj_top_kad_list = new List<tbl_top_kad_log>();
                        obj_top_kad_list = _care_repo.get_top_kad_Log_List(UserId).Where(k => k.created_on.Year == DateTime.Now.Year && k.created_on.Month == DateTime.Now.Month && k.recharge_amount != null && k.is_recharged == true).ToList();
                        decimal sum_balance = obj_top_kad_list.Sum(k => k.recharge_amount);
                        if (sum_balance < kad_bal_limtit)
                        {
                            dec_json_result = _care_repo.verify_topkad_serial(obj_top_kad);
                            if (!string.IsNullOrEmpty(dec_json_result))
                            {
                                obj_return = JsonConvert.DeserializeObject<top_kad_return_model>(dec_json_result);
                                if (obj_return != null && obj_return.resultcode == "0")
                                {
                                    decimal top_up_amount = Convert.ToDecimal(obj_return.rechargedAmount);
                                    if ((top_up_amount + sum_balance) <= kad_bal_limtit)
                                    {
                                        dec_json_result = _care_repo.top_up_topkad_serial(obj_top_kad);
                                        obj_return = JsonConvert.DeserializeObject<top_kad_return_model>(dec_json_result);
                                        if (obj_return != null && obj_return.resultcode == "0")
                                        {
                                            if (obj_return.rechargedAmount != null)
                                            {
                                                recharge_amount = Convert.ToDecimal(obj_return.rechargedAmount);
                                                obj_top_kad.recharge_amount = recharge_amount;
                                                obj_top_kad.is_recharged = true;
                                            }
                                        }
                                        else
                                        {
                                            obj_top_kad.recharge_amount = 0;
                                            obj_top_kad.is_recharged = false;
                                        }
                                    }
                                    else
                                    {
                                        check_limit = true;
                                        obj_top_kad.recharge_amount = 0;
                                        obj_top_kad.is_recharged = false;
                                    }
                                }
                                else
                                {
                                    obj_top_kad.recharge_amount = recharge_amount;
                                    obj_top_kad.is_recharged = false;
                                }
                                obj_top_kad.server_response = obj_return.resultcode;
                                obj_top_kad.server_reference = obj_return.reference;
                                obj_top_kad.sever_desc = obj_return.desc;
                                obj_top_kad.updated_by = Session["beadmin"].ToString();
                                XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/TopKadErrorCode.xml"));
                                XElement ErrorDesc = doc.Element("E" + obj_return.resultcode);
                                if (ErrorDesc != null)
                                {
                                    if (obj_return.resultcode != null && obj_return.resultcode == "0")
                                        ViewBag.Msg = ErrorDesc.Value + " " + obj_return.rechargedAmount;
                                    else
                                        ViewBag.Msg = ErrorDesc.Value;

                                    obj_top_kad.sever_desc = ErrorDesc.Value;

                                    if (obj_top_kad.sever_desc == null)
                                        obj_top_kad.sever_desc = obj_return.desc;
                                    if (obj_top_kad.is_recharged == true)
                                        obj_top_kad.sever_desc += obj_return.rechargedAmount;
                                }
                                else
                                    ViewBag.Msg = obj_return.desc;
                                if (check_limit == true)
                                {
                                    recharge_amount = Convert.ToDecimal(obj_return.rechargedAmount);
                                    obj_top_kad.sever_desc = "Limit Exceeds";
                                    obj_top_kad.recharge_amount = recharge_amount;
                                    ViewBag.Msg = "Limit Exceed";
                                }
                                int iresult = _care_repo.insert_top_kad_log(obj_top_kad);
                            }

                        }
                        else
                            ViewBag.Msg = "Limit Exceed";

                    }
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View();
        }
        public void Top_Kad_Log_to_CSV()
        {
            if (Session["beadmin"] != null && TempData["top_kad_log"] != null)
            {

                List<tbl_top_kad_log> obj_top_kad_log = new List<tbl_top_kad_log>();

                obj_top_kad_log = (List<tbl_top_kad_log>)TempData["top_kad_log"];

                //obj_top_kad_log = _yplan_repo.get_top_kad_Log_List();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment;filename=TOPKADLOG.csv");
                Response.ContentType = "text/csv";
                StringWriter sw = new StringWriter();

                sw.WriteLine("\"MSISDN Number\",\"Serial Number\",\"Invoice Number\",\"Verify Date\",\"Server Description\",\"Amount\",\"Recharge Status\",\"User Name\"");

                foreach (var line in obj_top_kad_log)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                                               line.msisdn,
                                               line.serial_number,
                                               line.invoice,
                                               line.created_on,
                                               line.sever_desc,
                                               "K" + line.recharge_amount,
                                               (line.is_recharged == true) ? "Success" : "Failure",
                                               line.updated_by
                                               ));
                }

                Response.Write(sw.ToString());

                Response.End();
            }
            else
                RedirectToAction("Login");
        }

        public ActionResult TOPKADHistory(int? page, string serialno, string msisdn, string invoice, bool? recharge_status)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            IList<tbl_top_kad_log> obj_top_kad_log = new List<tbl_top_kad_log>();

            try
            {
                string User_Id = "";
                int currentPageIndex = page.HasValue ? page.Value : 1;
                //if (Session["beadmin"].ToString() != "1")
                //    User_Id = Session["beadmin"].ToString();

                obj_top_kad_log = _care_repo.get_top_kad_Log_List(User_Id);

                if (recharge_status != null)
                    obj_top_kad_log = obj_top_kad_log.Where(n => n.is_recharged == recharge_status).ToList();

                if (!string.IsNullOrEmpty(serialno))
                    obj_top_kad_log = obj_top_kad_log.Where(k => k.serial_number == serialno).ToList();

                if (!string.IsNullOrEmpty(msisdn))
                    obj_top_kad_log = obj_top_kad_log.Where(k => k.msisdn.Contains(msisdn)).ToList();

                if (!string.IsNullOrEmpty(invoice))
                    obj_top_kad_log = obj_top_kad_log.Where(k => k.invoice == invoice).ToList();

                obj_top_kad_log = obj_top_kad_log.ToPagedList(currentPageIndex, defaultPageSize);

                TempData["top_kad_log"] = obj_top_kad_log;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_TOPKAD_Log", obj_top_kad_log);
                else
                    return View(obj_top_kad_log);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(obj_top_kad_log);
        }
        #endregion

        #region FB_Vouchers

        [SessionExpireFilter]
        public ActionResult FB_Vouchers(int? page, string sl_no, string pin_no, string fb_voucher, bool? ddlStatus)
        {
            IList<web_tbl_fb_promotions> oFBVouchers = new List<web_tbl_fb_promotions>();
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("FB_Vouchers"))
                    return RedirectToAction("Logout");               

                int currentPageIndex = page.HasValue ? page.Value : 1;

                ViewData["sl_no"] = sl_no;
                ViewData["pin_no"] = pin_no;
                ViewData["fb_voucher"] = fb_voucher;
                ViewData["status"] = ddlStatus;

                oFBVouchers = _UOW.redeem_fb_promotions_repo.Get(filter: (p => p.is_deleted == false)).ToList();
                if (oFBVouchers != null && oFBVouchers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(sl_no))
                        oFBVouchers = oFBVouchers.Where(b => b.serial_number == sl_no).ToList();

                    if (!string.IsNullOrEmpty(pin_no))
                        oFBVouchers = oFBVouchers.Where(b => b.pin_number == pin_no).ToList();

                    if (!string.IsNullOrEmpty(fb_voucher))
                        oFBVouchers = oFBVouchers.Where(b => b.fb_vouchers == fb_voucher).ToList();

                    if (ddlStatus != null)
                        oFBVouchers = oFBVouchers.Where(b => b.is_active == ddlStatus).ToList();                                      
                }

                if (oFBVouchers.Count > 0)
                    TempData["FB_Vouchers"] = oFBVouchers;

                oFBVouchers = oFBVouchers.ToPagedList(currentPageIndex, defaultPageSize);  
              
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_FB_Vouchers", oFBVouchers);                

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            
            return View(oFBVouchers);
        }


        public void FB_VouchersCsvList()
        {
            try
            {
                if (TempData["FB_Vouchers"] != null)
                {
                    IList<web_tbl_fb_promotions> model = new List<web_tbl_fb_promotions>();
                    model = (List<web_tbl_fb_promotions>)TempData["FB_Vouchers"];
                    TempData["FB_Vouchers"] = model;
                    if (model.Count > 0)
                    {

                        string filename = "FB_VouchersReport";

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"Serial Number\",\"Pin Number\",\"FB Voucher\",\"Amount\",\"Status\",\"Created Date/Time\"");

                        foreach (var line in model)
                        {
                            string amt = "$ " + string.Format("{0:0.##}", line.amount);
                            string status = line.is_active == true ? "Active" : "Incative";
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                                                       line.serial_number,
                                                       line.pin_number,
                                                       line.fb_vouchers,
                                                       amt,
                                                       status,
                                                       line.created_on.ToString("dd/MM/yyyy")
                                                       ));
                        }

                        Response.Write(sw.ToString());

                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
        }
        #endregion

        #region Del_FB_Voucher

        public ActionResult Del_FB_Voucher(long id)
        {
            TempData["fb_del_msg"] = "Failed to delete voucher!";
            try
            {
                if (Session["beadmin"] != null && Checkuserlevel("FB_Vouchers"))
                {
                    bool result = _care_repo.Delete_FB_Voucher(id);
                    if (result)
                    {
                        TempData["fb_del_msg"] = "FB voucher deleted successfully";                        
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return RedirectToAction("FB_Vouchers", "Admin");
        }
        #endregion

        #region Import_FB_Vouchers

        public ActionResult Import_FB_Vouchers()
        {
            List<Import_FB_Vouchers> objImport_FB_Vouchers = new List<Import_FB_Vouchers>();

            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("FB_Vouchers"))
                    return RedirectToAction("Logout");    

                if (TempData["import_fb_voucher"] != null)
                {
                    objImport_FB_Vouchers = (List<Import_FB_Vouchers>)TempData["import_fb_voucher"];
                    TempData["import_fb_voucher"] = objImport_FB_Vouchers;
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(objImport_FB_Vouchers);
        }

        [HttpPost]
        public ActionResult Import_FB_Vouchers(HttpPostedFileBase Csvfile)
        {
            List<Import_FB_Vouchers> objImport_FB_Vouchers = new List<Import_FB_Vouchers>();
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("FB_Vouchers"))
                    return RedirectToAction("Logout");    

                if (Csvfile != null)
                {
                    var extension = Path.GetExtension(Csvfile.FileName ?? string.Empty);
                    if (extension.ToLower() == ".csv")
                    {
                        bool is_imported = false;
                        using (var reader = new StreamReader(Csvfile.InputStream))
                        {
                            var SkipColumnName = 0;
                            var csv_items = reader.ReadToEnd().Split('\n');
                            var fb_voucher_items = from line in csv_items select line.Replace("\r", "").Split(',').ToList();
                            if (fb_voucher_items.Count() > 0)
                            {
                                List<web_tbl_fb_promotions> objFBPromotionsList = _UOW.redeem_fb_promotions_repo.Get(filter: x => x.is_deleted == false).ToList();
                                foreach (var item in fb_voucher_items)
                                {
                                    Import_FB_Vouchers import_fb = new Import_FB_Vouchers();
                                    import_fb.fb_promotions = new web_tbl_fb_promotions();

                                    if (SkipColumnName > 0 && item.Count == 3)
                                    {
                                        import_fb.fb_promotions.serial_number = !string.IsNullOrEmpty(item[0]) ? item[0].Trim() : "";
                                        import_fb.fb_promotions.pin_number = !string.IsNullOrEmpty(item[1]) ? item[1].Trim() : "";
                                        string amt = !string.IsNullOrEmpty(item[2]) ? item[2].Trim() : "";
                                        decimal amount = 0;
                                        bool amt_res = decimal.TryParse(amt, out amount);
                                        import_fb.fb_promotions.amount = amount;

                                        if (!string.IsNullOrEmpty(import_fb.fb_promotions.serial_number) && !string.IsNullOrEmpty(import_fb.fb_promotions.pin_number) && amt_res)
                                        {
                                            web_tbl_fb_promotions objFBPromo = objFBPromotionsList.Where(x => x.serial_number == import_fb.fb_promotions.serial_number || x.pin_number == import_fb.fb_promotions.pin_number).FirstOrDefault();
                                            if (objFBPromo == null)
                                            {
                                                import_fb.fb_promotions.fb_vouchers = Gen_FB_VoucherNo();
                                                import_fb.fb_promotions.is_active = true;
                                                import_fb.fb_promotions.is_deleted = false;
                                                import_fb.fb_promotions.created_on = DateTime.Now;

                                                _UOW.redeem_fb_promotions_repo.Insert(import_fb.fb_promotions);
                                                _UOW.Save();
                                                objFBPromotionsList.Add(import_fb.fb_promotions);
                                                
                                                import_fb.is_valid = true;
                                                import_fb.msg = "<span class='grid-success'>Imported successfully</span>";

                                                is_imported = true;
                                               
                                            }
                                            else
                                            {
                                                import_fb.is_valid = false;
                                                import_fb.msg = "<span class='grid-fail'>Serial/Pin number already Exist!</span>";
                                            }
                                        }
                                        else
                                        {    
                                            import_fb.is_valid = false;
                                            import_fb.msg = "<span class='grid-fail'>Fields should not be empty!</span>";
                                        }

                                        objImport_FB_Vouchers.Add(import_fb);
                                    }

                                    SkipColumnName++;
                                }
                            }
                            else
                                ViewBag.Msg = "Records not found!";

                        }

                        if (is_imported)
                            ViewBag.SuccessMsg = "Import Successful";
                        else
                            ViewBag.Msg = "No data imported. Please verify the file.";
                    }
                    else
                        ViewBag.Msg = "File should be .csv format";
                }
                else
                    ViewBag.Msg = "Please upload file";

                TempData["import_fb_voucher"] = objImport_FB_Vouchers;
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(objImport_FB_Vouchers);
        }

        private string Gen_FB_VoucherNo()
        {
            string res = string.Empty;

            string ref_no = _util_repo.GetRandomString(10);
            web_tbl_fb_promotions obj = _UOW.redeem_fb_promotions_repo.Get(filter: (m => m.fb_vouchers == ref_no)).FirstOrDefault();
            if (obj != null)
            {
                res = Gen_FB_VoucherNo();
            }
            else
            {
                res = ref_no;
            }

            return res;
        }
        #endregion

        #region Check MSISDN

        public ActionResult Check_Msisdn()
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            return View();
        }

        public JsonResult Check_MsisdnNo(string msisdn_no)
        {
            int Res = -1;

            try
            {
                if (Session["beadmin"] != null)
                {
                    msisdn_no = "677" + msisdn_no;

                    Res = _care_repo.GetMsisdnStatus(msisdn_no);
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return Json(new { msisdn_status = Res }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Registration

        public ActionResult Registration(int? page, string fname, string lname, string msisdn,string email, string status)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            if (!Checkuserlevel("Registration"))
                return RedirectToAction("Logout");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<RegistrationReportModel> objRegistration = new List<RegistrationReportModel>();
            ViewData["fname"] = fname;
            ViewData["lname"] = lname;
            ViewData["msisdn"] = msisdn;
            ViewData["email"] = email;
            ViewData["status"] = status;
            try
            {
                objRegistration = _care_repo.GetRegistrationDetails();
                if (objRegistration.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(fname))
                        objRegistration = objRegistration.Where(x => x.Reg.FirstName.ToLower().Contains(fname.ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(lname))
                        objRegistration = objRegistration.Where(x => x.Reg.LastName.ToLower().Contains(lname.ToLower())).ToList();

                    if (!string.IsNullOrWhiteSpace(msisdn))
                    {
                        string msisdn_no = "677" + msisdn;
                        objRegistration = objRegistration.Where(x => x.Reg.MsisdnNumber == msisdn_no || x.MultipileSim.Where(S => S.MsisdnNumber == msisdn_no).Count() > 0).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        objRegistration = objRegistration.Where(x => x.Reg.Email == email).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        bool is_active = status == "1" ? true : false;
                        objRegistration = objRegistration.Where(x => x.Reg.isActive == is_active).ToList();
                    }

                    objRegistration = objRegistration.ToPagedList(currentPageIndex, defaultPageSize);
                }                

                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    //return PartialView("_Ajax_Registration", objRegistration);              
                    return PartialView("_Ajax_Registration1", objRegistration);   
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }

            return View(objRegistration);
        }
        #endregion

        #region special promotion
        public ActionResult SpecialPromotion(int? page, string f_name, string dob, string location)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<tbl_spc_promo_cust> obj_spc_promo = new List<tbl_spc_promo_cust>();
            try
            {
                ViewData["f_name"] = f_name;
                ViewData["location"] = location;
                ViewData["dob"] = dob;

                obj_spc_promo = _kyc_repo.getSpecialPromotion();
                if (!string.IsNullOrEmpty(f_name))
                    obj_spc_promo = obj_spc_promo.Where(p => p.l_name != null && p.l_name.ToLower().Trim().Contains(f_name.ToLower().Trim()) || p.f_name.ToLower().Trim().Contains(f_name.ToLower().Trim())).ToList();

                if (!string.IsNullOrEmpty(location))
                    obj_spc_promo = obj_spc_promo.Where(p => p.u_identification != null && p.u_identification.ToLower().Trim().Contains(location.ToLower().Trim())).ToList();


                if (!string.IsNullOrEmpty(dob))
                {
                    DateTime _dob = Convert.ToDateTime(dob);
                    obj_spc_promo = obj_spc_promo.Where(p => DateTime.Compare(p.dob.Date, _dob.Date) == 0).ToList();
                }

                TempData["SpecialPromotion"] = obj_spc_promo;
                obj_spc_promo = obj_spc_promo.ToPagedList(currentPageIndex, defaultPageSize);



                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSpecialPromotion", obj_spc_promo);

                else
                    return View(obj_spc_promo);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(obj_spc_promo);

        }
        public void SpecialPromotionToCSV()
        {
            try
            {
                if (TempData["SpecialPromotion"] != null)
                {

                    List<tbl_spc_promo_cust> obj_spc = new List<tbl_spc_promo_cust>();

                    obj_spc = (List<tbl_spc_promo_cust>)TempData["SpecialPromotion"];
                    TempData["KYCCustomerList"] = obj_spc;

                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment;filename=Special_Promotion_" + DateTime.Now.ToShortDateString() + ".csv");
                    Response.ContentType = "text/csv";

                    StringWriter sw = new StringWriter();

                    sw.WriteLine("\"First Name\",\"Last Name\",\"Date of Birth\",\"Location\"");

                    foreach (var line in obj_spc)
                    {
                        string str_dob = Convert.ToDateTime(line.dob).ToShortDateString();
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\"",
                                                line.f_name,
                                                line.l_name,
                                                line.dob.ToShortDateString(),
                                                line.u_identification
                                                   ));
                    }

                    Response.Write(sw.ToString());

                    Response.End();
                }
            }

            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

        }
        #endregion

        private List<SelectListItem> FillUsers()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var user_nm = _UOW.care_user_repo.Get(filter: m => m.is_active == true && m.is_deleted == false).ToList();
            if(user_nm.Count > 0)
            {
                foreach(var u in user_nm)
                {
                    var item = new SelectListItem { Text = u.user_name, Value = u.user_name };
                    result.Add(item);
                }
            }
            return result;
        }

        private List<SelectListItem> Filltrans_status()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var userStatus = _UOW.trans_plan_Repo.Get().Select(m => m.trans_status).Distinct().ToList();
            if (userStatus.Count > 0)
            {
                foreach (var r in userStatus)
                {
                    var item = new SelectListItem { Text = r.ToString(), Value = r.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }

        public ActionResult SISubscriptionplan(string ddlUsers, string transanumber, string msisdn, string dFrom, string dTo, string ddlStatus, int? page)
        {
            if (Session["beadmin"] == null)
                return RedirectToAction("Login");

            long B_id = Convert.ToInt64(Session["beadmin"]);
            int R_id = Convert.ToInt32(Session["role_id"]);
            if (R_id != 1)
            {
                if (!_user_repo.checkMenuAccess(B_id, R_id, "SISubscriptionplan"))
                    return RedirectToAction("Logout");
            }


            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<TransactionReportModel> obj_list = new List<TransactionReportModel>();
            ViewData["ddlUsers"] = ddlUsers;
            ViewData["transanumber"] = transanumber;
            ViewData["msisdn"] = msisdn;
            ViewData["dFrom"] = dFrom;
            ViewData["dTo"] = dTo;
            ViewData["ddlStatus"] = ddlStatus;
            try
            {
                ViewBag.vuser = FillUsers();
                ViewBag.vstatus = Filltrans_status();

                obj_list = _user_repo.get_list().ToList();



                if (!string.IsNullOrWhiteSpace(ddlUsers))
                    obj_list = obj_list.Where(r => r.user_name == ddlUsers).ToList();

                if (!string.IsNullOrWhiteSpace(transanumber))
                    obj_list = obj_list.Where(r => r.trans_number == transanumber).ToList();

                if (!string.IsNullOrWhiteSpace(msisdn))
                    obj_list = obj_list.Where(r => r.msisdn == msisdn).ToList();

                if ((!string.IsNullOrWhiteSpace(dFrom)) && (!string.IsNullOrWhiteSpace(dTo)))
                {
                    DateTime dtFrom = Convert.ToDateTime(dFrom);
                    DateTime dtTo = Convert.ToDateTime(dTo).AddDays(1);
                    obj_list = obj_list.Where(r => r.trans_initiated >= dtFrom.Date && r.trans_initiated < dtTo.Date).ToList();
                }
                if (!string.IsNullOrWhiteSpace(ddlStatus))
                    obj_list = obj_list.Where(r => r.trans_status == ddlStatus).ToList();



                if (obj_list.Count > 0)
                    TempData["SISubscriptionplan"] = obj_list;

                obj_list = obj_list.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_SISubscriptionplan", obj_list);
                else
                    return View(obj_list);

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(obj_list);

        }

        public void SISubscriptionplanCsvList()
        {
            try
            {
                if (TempData["SISubscriptionplan"] != null)
                {

                    List<TransactionReportModel> SubplanReport = new List<TransactionReportModel>();
                    SubplanReport = (List<TransactionReportModel>)TempData["SISubscriptionplan"];


                    if (SubplanReport != null && SubplanReport.Count > 0)
                    {
                        TempData["SISubscriptionplan"] = SubplanReport;

                        string filename = "SISubscriptionplan";

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"User Name\",\"Transaction Number\",\"MSISDN\",\"Plans\",\"Total Price\",\"Transaction Initiated Date\",\"Transaction Purchase Date\"");
                        foreach (var line in SubplanReport)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",

                                                     line.user_name,
                                                     line.trans_number,
                                                     line.msisdn,
                                                     line.plans,
                                                     line.total_price,
                                                     line.trans_initiated,
                                                     line.trans_purchase));
                        }

                        Response.Write(sw.ToString());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
        }

        public List<mobileModel> RetrunListOfProducts()
        {
            string xmlData = ConfigurationSettings.AppSettings["EmailTempPath"];
            xmlData = @xmlData + "MobileTrans.xml";
            DataSet ds = new DataSet();//Using dataset to read xml file  
            ds.ReadXml(xmlData);
            var products = new List<mobileModel>();
            products = (from rows in ds.Tables[0].AsEnumerable()
                        select new mobileModel
                        {
                            Type = rows[0].ToString(),
                            Value = rows[1].ToString()

                        }).ToList();
            return products;
        }

        #region Selfcare PlanPurchaseTransHistory

        public ActionResult SF_PlanPurchaseTransHistory(int? page, string pTransNo, string pMSISDN, long? planID, string planName, string pStatus, string sDt, string eDt, int? TypeId)
        {
            IList<tbl_web_plan_purchase_trans> objTrans = new List<tbl_web_plan_purchase_trans>();
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");

                if (!Checkuserlevel("SF_PlanPurchaseTransHistory"))
                    return RedirectToAction("Logout");

                int currentPageIndex = page.HasValue ? page.Value : 1;

                DateTime now = DateTime.Now;
                DateTime sFrom = new DateTime(now.Year, now.Month, 1);


                sDt = string.IsNullOrEmpty(sDt) ? sFrom.ToString("dd/MM/yyyy") : sDt;
                eDt = string.IsNullOrEmpty(eDt) ? now.ToString("dd/MM/yyyy") : eDt;

                //TypeId = TypeId ?? 1;
                ViewData["pTransNo"] = pTransNo;
                ViewData["pMSISDN"] = pMSISDN;
                ViewData["pPlanID"] = planID;
                ViewData["sPlanName"] = planName;
                ViewData["pStatus"] = pStatus;
                ViewData["sDt"] = sDt;
                ViewData["eDt"] = eDt;
                ViewData["TypeId"] = TypeId;


                objTrans = _UOW.plan_purchase_trans_Repo.Get().ToList();

                if (!string.IsNullOrEmpty(pTransNo))
                    objTrans = objTrans.Where(n => n.trans_number == pTransNo).ToList();

                if (!string.IsNullOrEmpty(pMSISDN))
                    objTrans = objTrans.Where(n => n.msisdn == pMSISDN).ToList();

                if (planID != null)
                    objTrans = objTrans.Where(n => n.plan_id == planID).ToList();

                if (!string.IsNullOrEmpty(planName))
                    objTrans = objTrans.Where(n => n.plan_name.Contains(planName)).ToList();

                if (!string.IsNullOrEmpty(pStatus))
                    objTrans = objTrans.Where(n => n.trans_status == pStatus).ToList();

                DateTime sdt;
                if (DateTime.TryParse(sDt, out sdt))
                    objTrans = objTrans.Where(n => n.created_on >= sdt).ToList();

                DateTime edt;
                if (DateTime.TryParse(eDt, out edt))
                {
                    edt = edt.AddDays(1).Date;
                    objTrans = objTrans.Where(n => n.created_on < edt).ToList();
                }

                if (TypeId != null)
                    objTrans = objTrans.Where(n => n.type_id != null && n.type_id == TypeId).ToList();

                TempData["SF_PlanPurchaseTransHis"] = objTrans;
                TempData["purchase_From"] = TypeId;
                objTrans = objTrans.ToPagedList(currentPageIndex, defaultPageSize);

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_SF_PlanPurchaseTransHistory", objTrans);
            }

            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }

            return View(objTrans);
        }

        public void SF_PlanPurchaseTransHis_CSV()
        {
            try
            {
                if (TempData["SF_PlanPurchaseTransHis"] != null)
                {
                    List<tbl_web_plan_purchase_trans> objTrans = (List<tbl_web_plan_purchase_trans>)TempData["SF_PlanPurchaseTransHis"];

                    if (objTrans != null && objTrans.Count > 0)
                    {
                        TempData.Keep("SF_PlanPurchaseTransHis");

                        string filename = string.Empty;

                        if (TempData["purchase_From"] != null)
                        {
                            if ((int)TempData["purchase_From"] == 1)
                            {
                                filename = "Web_PlanPurchaseHistory";
                            }
                            else if ((int)TempData["purchase_From"] == 2)
                            {
                                filename = "Android_PlanPurchaseHistory";
                            }
                            else if ((int)TempData["purchase_From"] == 3)
                            {
                                filename = "Iphone_PlanPurchaseHistory";
                            }

                        }
                        else
                        {
                            filename = "PlanPurchaseHistory";
                        }

                        Response.ClearContent();
                        Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".csv");
                        Response.ContentType = "text/csv";

                        StringWriter sw = new StringWriter();

                        sw.WriteLine("\"Transaction Number\",\"MSISDN\",\"Plan ID\",\"Plan Name\",\"Plan Price\",\"Transaction Initiated Date\",\"Transaction Purchase Date\",\"Status\",\"Type\"");

                        foreach (var line in objTrans)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\"",
                                                       line.trans_number,
                                                       line.msisdn,
                                                       line.plan_id,
                                                       line.plan_name,
                                                       line.plan_price,
                                                       string.Format("{0:dd/MM/yyyy}", line.created_on),
                                                       string.Format("{0:dd/MM/yyyy}", line.plan_purchased_on),
                                                       line.trans_status,
                                                       (line.type_id == 1) ? "Web" : (line.type_id == 2) ? "Android" : (line.type_id == 3) ? "Iphone" : "NA"));
                        }

                        Response.Write(sw.ToString());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);

            }
        }
        #endregion

        #region permanent redirect
        public class PermanentRedirectResult : ActionResult
        {
            public string Url { get; set; }

            public PermanentRedirectResult(string url)
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new ArgumentException("url is null or empty", "url");
                }
                this.Url = url;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
                context.HttpContext.Response.StatusCode = 301;
                context.HttpContext.Response.RedirectLocation = Url;
                context.HttpContext.Response.End();
            }
        }
        #endregion

        #region SelfProfile
        public ActionResult ProfileView()
        {
            try
            {
                if (Session["beadmin"] == null)
                    return RedirectToAction("Login");


                long user_id = Convert.ToInt64(Session["beadmin"]);

                var obj_user = _UOW.care_user_repo.Get(filter: x => x.is_active == true && x.is_deleted == false && x.id == user_id).FirstOrDefault();

                profilemodel objpro = new profilemodel();
                objpro.contact_number = obj_user.contact_number;
                objpro.email_id = obj_user.email_id;
                objpro.first_name = obj_user.first_name;
                objpro.last_name = obj_user.last_name;               
                objpro.rolename = _UOW.role_repo.Get(filter: x => x.id == obj_user.role_id).Select(x => x.role_name).FirstOrDefault();
                objpro.status = obj_user.is_active == true ? "Active" : "Inactive";                

                if (obj_user != null)
                {
                    return View(objpro);
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return Content("Invalid page");
        }
        #endregion

        #region Dispose Objects

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _care_repo.Dispose();
                _util_repo.Dispose();
                _shop_repo.Dispose();
                _sim_repo.Dispose();
                _user_repo.Dispose();
                _UOW.Dispose();
                _dbselfcare.Dispose();
                _staffs_topup_repo.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
