using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using bemobile.Models;
using bemobile.DAL;
using bemobile.Utils;
using System.Web.Routing;
using bemobile.Filters;
using System.Configuration;
using MvcPaging;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;

namespace bemobile.Controllers
{
    public class CustomerCareController : Controller
    {
        //
        // GET: /CustomerCare/
       
             UnitOfWork _UOW;
        private IdokuRepository _doku_repo;
        private IUtilityRepository _util_repo;
        private int defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pgRptSize"]);
        public CustomerCareController()
        {
            this._UOW = new UnitOfWork();
            this._doku_repo = new dokuRepository();
            this._util_repo = new UtilityRepository();
        }
       
        public ActionResult Index()
        {
            return RedirectToAction("Login", "CustomerCare");
        }
        public ActionResult Login()
        {
            web_tbl_care_user oCU = new web_tbl_care_user();

            return View(oCU);
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "CustomerCare");
        }

        [HttpPost]
        public ActionResult Login(web_tbl_care_user oCU)
        {
            var Authenticate = _UOW.care_user_repo.Get(filter: a => a.user_name == oCU.user_name && a.user_pwd == oCU.user_pwd && a.is_active == true && a.is_deleted == false && a.role_id == 4).ToList();
            if (Authenticate.Count > 0)
            {
                Session["cc_user_id"] = Authenticate[0].id;
                Session["cc_role_id"] = Authenticate[0].role_id;
                return RedirectToAction("TopupTransactions");
            }
            else
            {
                ViewBag.Message = "Login Failed!";
            }
            return View(oCU);

        }
        private List<SelectListItem> FillOrderStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = _UOW.selfcare_order_payment_repo.Get().ToList();
            if (res.Count > 0)
            {
                var status = (from s in res select new { s.payment_status }).Distinct().ToList();
                for (int i = 0; i < status.Count; i++)
                {
                    var item = new SelectListItem { Text = status[i].payment_status, Value = status[i].payment_status.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }
        private List<SelectListItem> FillDokuStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = _UOW.doku_selfcare_repo.Get().ToList();
            if (res.Count > 0)
            {
                var status = (from s in res select new { s.resultmsg }).Distinct().ToList();
                for (int i = 0; i < status.Count; i++)
                {
                    var item = new SelectListItem { Text = status[i].resultmsg, Value = status[i].resultmsg.ToString() };
                    result.Add(item);
                }
            }
            return result;
        }
        public ActionResult TopupTransactions(string transmerchantid, string mobile_no, string ddlDokuStatus, string ddlOrderstatus, string sFrom, string sTo, int? page)
        {
            if (Session["cc_user_id"] == null)
                return RedirectToAction("Login","CustomerCare");

            string sMsg = "";
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
            //ViewBag.PayTypelist = FillPayType();
            try
            {

                oD = _doku_repo.GetDokuOrderTransaction().Where(t => t.orderitems[0].purchase_desc == "TOPUP").ToList();
                if (oD.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(transmerchantid))
                        oD = oD.Where(d => d.doku.transidmerchant == transmerchantid).ToList();

                    if (!string.IsNullOrWhiteSpace(mobile_no))
                        oD = oD.Where(d => d.order.purchase_msisdn == mobile_no).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlDokuStatus))
                        oD = oD.Where(d => d.doku.resultmsg == ddlDokuStatus).ToList();

                    if (!string.IsNullOrWhiteSpace(ddlOrderstatus))
                        oD = oD.Where(d => d.doku.resultmsg == ddlOrderstatus).ToList();

                    if ((!string.IsNullOrWhiteSpace(sFrom)) && (!string.IsNullOrWhiteSpace(sTo)))
                    {
                        DateTime dtFrom = Convert.ToDateTime(sFrom);
                        DateTime dtTo = Convert.ToDateTime(sTo).AddDays(1);
                        oD = oD.Where(t => t.doku.created_on.Date >= dtFrom && t.doku.created_on <= dtTo).ToList();
                    }
                    if (oD.Count > 0)
                        TempData["TopupTransactions"] = oD;
                    oD = oD.ToPagedList(currentPageIndex, defaultPageSize);

                    if (oD.Count == 0)
                        sMsg = "No Details found matching your search criteria!";
                }
                else
                    sMsg = "No Records found!";
                ViewBag.Message = sMsg;
                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_TopupTransactions", oD);
                else
                    return View(oD);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);

            }
            return View(oD);
        }

        public void TopupTransactionCsvList()
        {
            try
            {
                if (TempData["TopupTransactions"] != null)
                {
                    List<DokuCareModel> TransReport = new List<DokuCareModel>();

                    TransReport = (List<DokuCareModel>)TempData["TopupTransactions"];

                    if (TransReport != null && TransReport.Count > 0)
                    {
                        TempData["TopupTransactions"] = TransReport;

                        string filename = "TopupTransactions";

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
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);

            }
        }
    }
}
