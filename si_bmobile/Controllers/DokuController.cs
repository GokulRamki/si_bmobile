using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using bemobile.Models;
using System.Configuration;
using bemobile.DAL;
using bemobile.Utils;
using Newtonsoft.Json;
using MvcPaging;
using bemobile.GetPowerRef;
using bemobile.esipayRef;
using System.Xml.Linq;

namespace bemobile.Controllers
{
    public class DokuController : Controller
    {
        //
        // GET: /Doku/
        private IShopRepository _shopping_repo;
        private IUtilityRepository _utils_repo;
        private IcareRepository _care_repo;
        private IPlanRepository _plan_repo;
        private UnitOfWork UOW;
        private IdokuRepository _doku_repo;
        private IesipayServiceClient _esipay;
        private ISIRepository _bmsi_repo;


        string sAdmName;
        string sAdmPwd;
        private int defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pgSize"]);
        private string gpDisplay;
        private string ToShopCC;
        #region Params for GetPower
        private string gp_svc_uid = ConfigurationManager.AppSettings["gp_svc_uid"].ToString();
        private string gp_svc_pwd = ConfigurationManager.AppSettings["gp_svc_pwd"].ToString();
        private string gp_java_uid = ConfigurationManager.AppSettings["gp_java_uid"].ToString();
        private string gp_java_pwd = ConfigurationManager.AppSettings["gp_java_pwd"].ToString();
        private string gp_merc_id = ConfigurationManager.AppSettings["gp_merc_id"].ToString();
        private string gp_merc_uname = ConfigurationManager.AppSettings["gp_merc_uname"].ToString();
        private string gp_merc_pwd = ConfigurationManager.AppSettings["gp_merc_pwd"].ToString();
        private string gp_merc_keycode = ConfigurationManager.AppSettings["gp_merc_keycode"].ToString();
        #endregion

        public DokuController()
        {
            this._shopping_repo = new ShopRepository();
            this._utils_repo = new UtilityRepository();
            this._care_repo = new careRepository();
            this.UOW = new UnitOfWork();
            this._doku_repo = new dokuRepository();
            this._plan_repo = new PlanRepository();
            this._esipay = new IesipayServiceClient();
            this._bmsi_repo = new SIRepository();
            this.sAdmName = ConfigurationManager.AppSettings["admuname"];
            this.sAdmPwd = ConfigurationManager.AppSettings["admpwd"];
            this.gpDisplay = "display:none";
            this.ToShopCC = ConfigurationManager.AppSettings["ToShopCC"].ToString();
        }

        private static string CalculateSha1(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSha1 =
            new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(
                cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");

            return hash;
        }

        public ActionResult test()
        {
            PostpaidModel pm = new PostpaidModel();
            pm.cheque_num = "FINV-00008319-14";
            pm.payment_amount = "1.00";
            pm.entity_id = "41739148";
            string spp = _care_repo.PurchasePostpaid(pm);
            string token = Math.Abs(1.00).ToString();
            string units = "";
            string op = "";
            // GetPowerProcessRes("67576542265", "2809478", "1", 1, out token, out units, out op);

            //List<DokuCareModel> r = new List<DokuCareModel>();
            //r = _doku_repo.GetDokuOrderTransaction();
            //SendGPSMS("67576342468", Math.Round(1.00).ToString(), "2809478", "12365498795465423", "1.3", "4564654214");
            return View();
        }

        public ActionResult ShopDku()
        {

            DokuPostModel oDM = new DokuPostModel();
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");

            if (Session["seDKUOrderid"] != null)
            {

                //long order_id = Convert.ToInt64("20155");
               long order_id = Convert.ToInt64(Session["seDKUOrderid"]);
                var dokuTrans = UOW.doku_selfcare_repo.Get(filter: d => d.order_id == order_id && d.site_id == 2).FirstOrDefault();
                var orderTrans = UOW.shopping_order_repo.GetByID(order_id);
                var orderItem = UOW.shopping_order_item_repo.Get(filter: d => d.order_id == order_id).OrderBy(o=>o.product_id).ToList();
                var orderPayment = UOW.shopping_order_payment_repo.Get(filter: d => d.order_id == order_id).FirstOrDefault();

                web_tbl_shopping_user_contact UserDetails = new web_tbl_shopping_user_contact();

                if(orderPayment!=null && !string.IsNullOrEmpty(orderPayment.payment_response))
                    UserDetails = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(orderPayment.payment_response);

                if (dokuTrans != null && orderTrans != null && orderItem.Count > 0 && UserDetails != null)
                {
                    oDM.ACTIONURL = ConfigurationManager.AppSettings["dk_Form_Action"].ToString();
                    oDM.MALLID = ConfigurationManager.AppSettings["dk_MALLID"].ToString();
                    oDM.CHAINMERCHANT = ConfigurationManager.AppSettings["dk_chainMerchant"].ToString();
                    oDM.AMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                    oDM.PURCHASEAMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                    oDM.TRANSIDMERCHANT = orderPayment.payment_transaction_number.ToString();
                    oDM.CURRENCY = ConfigurationManager.AppSettings["dk_Currency"].ToString();
                    string sFormationwrd = oDM.AMOUNT + oDM.MALLID + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + oDM.TRANSIDMERCHANT + oDM.CURRENCY;
                    oDM.WORDS = CalculateSha1(sFormationwrd, Encoding.Default).ToLower();
                    oDM.REQUESTDATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                    oDM.PAYMENTCHANNEL = ConfigurationManager.AppSettings["dk_paymentChannel"].ToString();
                    oDM.PURCHASECURRENCY = ConfigurationManager.AppSettings["dk_PurchaseCurrency"].ToString();
                    oDM.SESSIONID = dokuTrans.session_id;
                    oDM.ADDITIONALDATA = "shop";
                    //   oDM.CUSTOMERID = dokuTrans.customer_id.ToString();
                    oDM.NAME = UserDetails.first_name + " " + UserDetails.last_name;
                    oDM.EMAIL = UserDetails.email;

                    string line_items = "";
                    foreach (var item in orderItem)
                    {
                      var sub_pro_price =  CalculateTotal(item.product_qty, item.product_price.ToString());

                      line_items += item.product_name.Trim() + "," + item.product_price.ToString("#0.00").Trim() + "," + item.product_qty.ToString().Trim() + "," + sub_pro_price.Trim() + ";";

                    }

                    oDM.BASKET = line_items;
                }
            }
            return View(oDM);
        }

        public ActionResult CareDku()
        {

            DokuPostModel oDM = new DokuPostModel();
            try
            {
                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");

                if (Session["seDKUOrderid"] != null)
                {
                    long order_id = Convert.ToInt64(Session["seDKUOrderid"]);
                    var dokuTrans = UOW.doku_selfcare_repo.Get(filter: d => d.order_id == order_id && d.site_id == 1).FirstOrDefault();
                    var orderTrans = UOW.selfcare_order_repo.GetByID(order_id);
                    var orderItem = UOW.selfcare_order_item_repo.Get(filter: d => d.order_id == order_id).ToList();
                    if (dokuTrans != null && orderTrans != null && orderItem.Count > 0)
                    {

                        oDM.ACTIONURL = ConfigurationManager.AppSettings["dk_Form_Action"].ToString();
                        oDM.MALLID = ConfigurationManager.AppSettings["dk_MALLID"].ToString();
                        oDM.CHAINMERCHANT = ConfigurationManager.AppSettings["dk_chainMerchant"].ToString();
                        oDM.AMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                        oDM.PURCHASEAMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                        oDM.TRANSIDMERCHANT = orderTrans.order_number.ToString();
                        oDM.CURRENCY = ConfigurationManager.AppSettings["dk_Currency"].ToString();
                        string sFormationwrd = oDM.AMOUNT + oDM.MALLID + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + oDM.TRANSIDMERCHANT + oDM.CURRENCY;
                        oDM.WORDS = CalculateSha1(sFormationwrd, Encoding.Default).ToLower();
                        oDM.REQUESTDATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                        oDM.PAYMENTCHANNEL = ConfigurationManager.AppSettings["dk_paymentChannel"].ToString();
                        oDM.PURCHASECURRENCY = ConfigurationManager.AppSettings["dk_PurchaseCurrency"].ToString();
                        oDM.SESSIONID = dokuTrans.session_id;
                        oDM.ADDITIONALDATA = "selfcare";
                        //   oDM.CUSTOMERID = dokuTrans.customer_id.ToString();
                        oDM.NAME = orderTrans.cust_fname + " " + orderTrans.cust_lname;
                        oDM.EMAIL = orderTrans.cust_email;



                        string line_items = "";

                        string bypitems = "";
                        foreach (var item in orderItem)
                        {
                            if (item.purchase_desc.ToLower() == "topup")
                                line_items += "TOPUP FOR " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                            else if (item.purchase_desc.ToLower() == "postpaid_pdf_bill")
                                line_items += "POST PAID ACCOUNT PAYMENT FOR SERVICE " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                            else if (item.purchase_desc.ToLower() == "getpower")
                                line_items += "Easypay for meter no: " + item.product_name + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                            else if (item.purchase_desc.ToLower() == "byp" || item.purchase_desc.ToLower() == "promotion")
                            {
                                bypitems += item.product_name + " / ";
                                line_items = bypitems + "," + oDM.AMOUNT + "," + item.product_qty + "," + oDM.AMOUNT + ";";
                            }
                            else
                                line_items += item.product_name + "," + item.product_price.ToString("#0.00") + "," + item.product_qty + "," + CalculateTotal(item.product_qty, item.product_price.ToString()) + ";";
                        }


                        oDM.BASKET = line_items;
                    }
                }
            }
            catch (Exception ex)
            {

                _utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku error", ex.Message + "<br>" + ex.StackTrace);
            }

            return View(oDM);
        }

        public ActionResult ProcessDoku(string id)
        {

            DokuPostModel oDM = new DokuPostModel();
            try
            {
                string sproSessId = "";
                if (!string.IsNullOrEmpty(id))
                {
                    sproSessId = id;

                    var tempOrder = UOW.temp_orders_repo.Get(filter: t => t.Session_Id == sproSessId).LastOrDefault();
                    long orderId = 0;
                    int iSiteId = 0;

                    if (tempOrder != null)
                    {
                        orderId = Convert.ToInt64(tempOrder.Order_Id);
                        iSiteId = Convert.ToInt32(tempOrder.SiteId);
                    }

                    if (orderId != 0 && iSiteId != 0)
                    {
                        long order_id = orderId;
                        var dokuTrans = UOW.doku_selfcare_repo.Get(filter: d => d.order_id == order_id && d.site_id == iSiteId).FirstOrDefault();
                        var orderTrans = UOW.selfcare_order_repo.GetByID(order_id);
                        var orderItem = UOW.selfcare_order_item_repo.Get(filter: d => d.order_id == order_id).ToList();
                        if (dokuTrans != null && orderTrans != null && orderItem.Count > 0)
                        {
                            oDM.ACTIONURL = ConfigurationManager.AppSettings["dk_Form_Action"].ToString();
                            oDM.MALLID = ConfigurationManager.AppSettings["dk_MALLID"].ToString();
                            oDM.CHAINMERCHANT = ConfigurationManager.AppSettings["dk_chainMerchant"].ToString();
                            oDM.AMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                            oDM.PURCHASEAMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                            oDM.TRANSIDMERCHANT = orderTrans.order_number.ToString();
                            oDM.CURRENCY = ConfigurationManager.AppSettings["dk_Currency"].ToString();
                            string sFormationwrd = oDM.AMOUNT + oDM.MALLID + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + oDM.TRANSIDMERCHANT + oDM.CURRENCY;
                            oDM.WORDS = CalculateSha1(sFormationwrd, Encoding.Default).ToLower();
                            oDM.REQUESTDATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                            oDM.PAYMENTCHANNEL = ConfigurationManager.AppSettings["dk_paymentChannel"].ToString();
                            oDM.PURCHASECURRENCY = ConfigurationManager.AppSettings["dk_PurchaseCurrency"].ToString();
                            oDM.SESSIONID = dokuTrans.session_id;
                            oDM.ADDITIONALDATA = "selfcare";
                            //   oDM.CUSTOMERID = dokuTrans.customer_id.ToString();
                            oDM.NAME = orderTrans.cust_fname + " " + orderTrans.cust_lname;
                            oDM.EMAIL = orderTrans.cust_email;

                            string line_items = "";

                            foreach (var item in orderItem)
                            {

                                if (item.purchase_desc.ToLower() == "cugtopup")
                                    line_items += "TOPUP FOR " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                                else if (item.purchase_desc.ToLower() == "getpower")
                                    line_items += "GETPOWER FOR " + item.product_name + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                                else if (item.purchase_desc.ToLower() == "sitopup")
                                    line_items += "TOPUP FOR " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";
                                else if (item.purchase_desc.ToLower() == "bmtopup")
                                    line_items += "BMOBILE TOPUP FOR " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";

                            }
                            oDM.BASKET = line_items;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku error(PROCESS DOKU)", ex.Message + "<br>" + ex.StackTrace);
            }

            return View(oDM);
        }

        private string CalculateTotal(long qty, string amount)
        {
            string res = "";
            if (qty != 0 && amount != "")
                res = (Convert.ToDouble(qty) * Convert.ToDouble(amount)).ToString("#0.00");
            return res;
        }

        private string DataIdentify(string stransmerId)
        {
            string sRes = "";

            var dkuselfcare = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == stransmerId).FirstOrDefault();
            if (dkuselfcare != null)
            {
                if (dkuselfcare.site_id == 1)
                    sRes = "selfcare";
                else if (dkuselfcare.site_id == 3)
                    sRes = "cug";
            }
            var dkushop = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == stransmerId && d.site_id == 2).FirstOrDefault();
            if (dkushop != null)
            {
                if (dkushop.site_id == 2)
                    sRes = "shop";
            }
            return sRes;
        }


        public ActionResult notify()
        {
            DokuRDModel oRDM = new DokuRDModel();
            doku_selfcare oD = new doku_selfcare();
            try
            {
                if (Request.Form["TRANSIDMERCHANT"] != null)
                {
                    string transidmerchant = Request.Form["TRANSIDMERCHANT"].ToString();
                    string words = Request.Form["WORDS"].ToString();
                    string sSessId = Request.Form["SESSIONID"].ToString();

                    string sADATA = DataIdentify(transidmerchant);
                    string myWORDS = "";
                    if (sADATA == "selfcare")
                    {
                        #region Selfcare Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 1).FirstOrDefault();
                            if (doku != null)
                            {
                                string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (myWORDS == Request.Form["WORDS"])
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();

                                    DokuCareModel dokucare = new DokuCareModel();
                                    dokucare.order = order;
                                    dokucare.orderitems = orderitems;
                                    dokucare.doku = oD;
                                    if (oD != null && order != null && orderitems.Count > 0)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = Request.Form["PAYMENTDATETIME"];
                                        oD.paymentcode = Request.Form["PAYMENTCODE"];
                                        oD.response_code = Request.Form["RESPONSECODE"].ToString();
                                        oD.resultmsg = Request.Form["RESULTMSG"].ToString();

                                        oD.statustype = Request.Form["STATUSTYPE"].ToString();

                                        oD.verifyid = Request.Form["VERIFYID"].ToString();
                                        oD.verifyscore = Request.Form["VERIFYSCORE"];
                                        oD.verifystatus = Request.Form["VERIFYSTATUS"].ToString();

                                        oD.approvalcode = Request.Form["APPROVALCODE"].ToString();
                                        oD.mcn = Request.Form["MCN"].ToString();

                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                if (orderitems[0].purchase_desc == "GETPOWER")
                                                {
                                                    #region GetPower

                                                    string token = string.Empty;
                                                    string units = string.Empty;
                                                    bool bRes = GetPowerProcess(order.purchase_msisdn, orderitems[0].product_name, Math.Round(orderitems[0].product_price).ToString(), Convert.ToInt16(order.user_id.ToString()), out token, out units);
                                                    if (bRes == true)
                                                    {
                                                        #region Update GetPower Success
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = units;
                                                            op.payment_receipt_no = token;
                                                            op.payment_status = "APPROVED";

                                                            dokucare.orderpayment = op;
                                                            oRDM.dkuCare = dokucare;

                                                            if (oRDM.dkuCare != null)
                                                            {
                                                                //SendGPSMS(order.cust_mobile_number, Math.Round(orderitems[0].product_price).ToString(), orderitems[0].product_name, token, units, transidmerchant);
                                                                SendOrderSummary_email(oRDM);
                                                            }

                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = token + "#" + "RechargeFailed";
                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                        }
                                                    }

                                                    #endregion
                                                }
                                                else if (orderitems[0].purchase_desc == "BYP" || orderitems[0].purchase_desc == "PROMOTION")
                                                {
                                                    #region Buy BYP,PROMOTIONS

                                                    var byp = orderitems.Where(b => b.purchase_desc == "BYP").ToList();
                                                    var promo = orderitems.Where(b => b.purchase_desc == "PROMOTION").ToList();
                                                    PurchasePlanModel ppM = new PurchasePlanModel();
                                                    List<Purchasedenomination> pDenoIds = new List<Purchasedenomination>();
                                                    List<Purchasedpromotion> pPromoIds = new List<Purchasedpromotion>();
                                                    if (byp.Count > 0)
                                                    {
                                                        foreach (var bi in byp)
                                                        {
                                                            Purchasedenomination pd = new Purchasedenomination();
                                                            pd.denomination_id = bi.product_id;
                                                            pd.plan_name = bi.product_name;
                                                            pd.price = Convert.ToDouble(bi.product_price);
                                                            pDenoIds.Add(pd);
                                                        }
                                                    }
                                                    if (promo.Count > 0)
                                                    {
                                                        foreach (var pi in promo)
                                                        {
                                                            Purchasedpromotion pp = new Purchasedpromotion();
                                                            pp.promotion_id = pi.product_id;
                                                            pp.plan_name = pi.product_name;
                                                            pp.price = Convert.ToDouble(pi.product_price);
                                                            pPromoIds.Add(pp);

                                                        }
                                                    }

                                                    ppM.from_msisdn = order.cust_mobile_number;
                                                    ppM.isMobile = "1";
                                                    ppM.purchase_msisdn = order.purchase_msisdn;
                                                    ppM.pDenomIds = pDenoIds;
                                                    ppM.PromotionIds = pPromoIds;
                                                    ppM.tot_amt = order.order_product_total;
                                                    var order_payment_byp = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    if (order_payment_byp != null)
                                                        ppM.isTopUp = Convert.ToBoolean(order_payment_byp.payment_receipt_no);

                                                    int iResBYP = _plan_repo.purchase_plan(ppM);

                                                    if (iResBYP == 0)
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_receipt_no = iResBYP.ToString();
                                                            op.payment_status = "APPROVED";

                                                            dokucare.orderpayment = op;
                                                            oRDM.dkuCare = dokucare;

                                                            if (oRDM.dkuCare != null)
                                                                SendOrderSummary_email(oRDM);
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = iResBYP + "#" + "BuyBYP/PromotionFailed";
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                    }

                                                    #endregion

                                                }
                                                else if (orderitems[0].purchase_desc == "TOPUP")
                                                {
                                                    #region EasyRecharge For TopUP

                                                    string ValidateMerch = _doku_repo.validate_merchant(order.purchase_msisdn, sAmount);
                                                    if (ValidateMerch != "")
                                                    {
                                                        string Recharge = _doku_repo.recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                        var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                        if (opRes.resultCode == 0)
                                                        {



                                                            #region TOPUP SUCCESS
                                                            var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                            var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                            if (op != null)
                                                            {
                                                                op.payment_response = "UpdateSuccess";
                                                                op.payment_receipt_no = opRes.reference;
                                                                op.payment_status = "APPROVED";

                                                                dokucare.orderpayment = op;
                                                                oRDM.dkuCare = dokucare;

                                                                if (oRDM.dkuCare != null)
                                                                    SendOrderSummary_email(oRDM);
                                                            }
                                                            else
                                                                op.payment_response = "OrderNotFound";

                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                            #endregion

                                                        }
                                                        else
                                                        {
                                                            var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                            var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                            if (op != null)
                                                            {
                                                                op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                                op.payment_receipt_no = opRes.reference;
                                                                UOW.selfcare_order_payment_repo.Update(op);
                                                                UOW.Save();
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (orderitems[0].purchase_desc == "Postpaid_Pdf_bill")
                                                {
                                                    #region Postpaid Stuff

                                                    var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    if (op != null)
                                                    {
                                                        PostpaidModel pm = new PostpaidModel();
                                                        pm.cheque_num = op.payment_receipt_no;
                                                        pm.payment_amount = order.order_product_total.ToString();
                                                        pm.entity_id = orderitems[0].product_name;
                                                        string sPurchase = _care_repo.PurchasePostpaid(pm);
                                                        if (sPurchase == "1")
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_status = "APPROVED";
                                                        }
                                                        else
                                                        {
                                                            op.payment_response = "PPfailed#" + sPurchase;
                                                            op.payment_status = "PENDING";
                                                        }
                                                        dokucare.orderpayment = op;
                                                        oRDM.dkuCare = dokucare;

                                                        if (oRDM.dkuCare != null)
                                                            SendOrderSummary_email(oRDM);
                                                    }
                                                    else
                                                        op.payment_response = "OrderNotFound";

                                                    UOW.selfcare_order_payment_repo.Update(op);
                                                    UOW.Save();




                                                    #endregion
                                                }
                                            }

                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _utils_repo.ErrorLog_Txt(ex);
                            //_utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error testselfcare(notify page)", ex.Message + "<br>" + ex.StackTrace + "<br>");
                        }
                        #endregion
                    }
                    else if (sADATA == "shop")
                    {
                        #region Shop Notify

                        try
                        {
                            var dokuShop = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 2).FirstOrDefault();
                            if (dokuShop != null)
                            {
                                string myFormationwrd = Convert.ToDecimal(dokuShop.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (myWORDS == Request.Form["WORDS"])
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(dokuShop.id);
                                    long _lorder_id = oD.order_id;
                                    var orderSh = UOW.shopping_order_repo.GetByID(_lorder_id);
                                    var orderitemsSh = UOW.shopping_order_item_repo.Get(filter: oi => oi.order_id == _lorder_id).OrderBy(o => o.product_id).ToList();

                                    DokuShopModel obj_dokushop = new DokuShopModel();
                                    obj_dokushop.sorder = orderSh;
                                    obj_dokushop.sorderitems = orderitemsSh;
                                    obj_dokushop.sdoku = oD;

                                    if (oD != null && orderSh != null && orderitemsSh.Count > 0)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = Request.Form["PAYMENTDATETIME"];
                                        oD.paymentcode = Request.Form["PAYMENTCODE"];
                                        oD.response_code = Request.Form["RESPONSECODE"].ToString();
                                        oD.resultmsg = Request.Form["RESULTMSG"].ToString();
                                        oD.statustype = Request.Form["STATUSTYPE"].ToString();
                                        oD.verifyid = Request.Form["VERIFYID"].ToString();
                                        oD.verifyscore = Request.Form["VERIFYSCORE"];
                                        oD.verifystatus = Request.Form["VERIFYSTATUS"].ToString();
                                        oD.approvalcode = Request.Form["APPROVALCODE"].ToString();
                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {

                                            var order_payment = UOW.shopping_order_payment_repo.Get(filter: p => p.order_id == _lorder_id).FirstOrDefault();
                                            //var op = UOW.shopping_order_payment_repo.GetByID(order_payment.payment_id);
                                            if (order_payment != null)
                                            {
                                                //op.payment_response = "UpdateSuccess";
                                                order_payment.payment_receipt_no = oD.approvalcode;
                                                order_payment.payment_status = "APPROVED";

                                                obj_dokushop.sorderpayment = order_payment;
                                                oRDM.dkuShop = obj_dokushop;

                                                if (oRDM.dkuShop != null)
                                                    SendOrderSummary_email(oRDM);
                                            }
                                            //else
                                            //    order_payment.payment_response = "OrderNotFound";

                                            UOW.shopping_order_payment_repo.Update(order_payment);
                                            UOW.Save();
                                        }
                                        else
                                        {
                                            #region CC Payment failed

                                            #endregion
                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _utils_repo.ErrorLog_Txt(ex);
                            //_utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error testselfcare(notify page)", ex.Message + "<br>" + ex.StackTrace);
                        }
                        #endregion
                    }
                    else if (sADATA == "cug")
                    {
                        #region cug Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 3).FirstOrDefault();
                            if (doku != null)
                            {


                                string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (myWORDS == Request.Form["WORDS"])
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();
                                    if (oD != null && order != null)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = Request.Form["PAYMENTDATETIME"];
                                        oD.paymentcode = Request.Form["PAYMENTCODE"];
                                        oD.response_code = Request.Form["RESPONSECODE"].ToString();
                                        oD.resultmsg = Request.Form["RESULTMSG"].ToString();
                                        oD.statustype = Request.Form["STATUSTYPE"].ToString();
                                        oD.verifyid = Request.Form["VERIFYID"].ToString();
                                        oD.verifyscore = Request.Form["VERIFYSCORE"];
                                        oD.verifystatus = Request.Form["VERIFYSTATUS"].ToString();
                                        oD.approvalcode = Request.Form["APPROVALCODE"].ToString();
                                        oD.mcn = Request.Form["MCN"].ToString();
                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                string ValidateMerch = _doku_repo.validate_merchant(order.purchase_msisdn, sAmount);
                                                if (ValidateMerch != "")
                                                {
                                                    string Recharge = _doku_repo.recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);
                                                    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);
                                                    if (opRes.resultCode == 0)
                                                    {
                                                        #region CUG TOPUP
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_receipt_no = opRes.reference;
                                                            op.payment_status = "APPROVED";
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                            op.payment_receipt_no = opRes.reference;
                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                        }
                                                    }
                                                }


                                            }

                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _utils_repo.ErrorLog_Txt(ex);
                            //_utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error testselfcare(notify page)", ex.Message + "<br>" + ex.StackTrace);
                        }
                        #endregion
                    }
                    else if (sADATA == "bmtopup")
                    {
                        #region bmtopup Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 5).FirstOrDefault();
                            if (doku != null)
                            {
                                string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (myWORDS == Request.Form["WORDS"])
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();
                                    if (oD != null && order != null)
                                    {
                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = Request.Form["PAYMENTDATETIME"];
                                        oD.paymentcode = Request.Form["PAYMENTCODE"];
                                        oD.response_code = Request.Form["RESPONSECODE"].ToString();
                                        oD.resultmsg = Request.Form["RESULTMSG"].ToString();

                                        oD.statustype = Request.Form["STATUSTYPE"].ToString();

                                        oD.verifyid = Request.Form["VERIFYID"].ToString();
                                        oD.verifyscore = Request.Form["VERIFYSCORE"];
                                        oD.verifystatus = Request.Form["VERIFYSTATUS"].ToString();

                                        oD.approvalcode = Request.Form["APPROVALCODE"].ToString();
                                        oD.mcn = Request.Form["MCN"].ToString();

                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                if (orderitems[0].product_id == 675) // For PNG Topup 
                                                {
                                                    #region For PNG Topup

                                                    //string ValidateMerch = _doku_repo.bmpng_validate_merchant(order.purchase_msisdn, sAmount);
                                                    //if (!string.IsNullOrEmpty(ValidateMerch))

                                                    //    string Recharge = _doku_repo.bmpng_recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                    //    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                    //    if (opRes.resultCode == 0)
                                                    //    {
                                                    //        #region BMOBILE TOPUP
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = "UpdateSuccess";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            op.payment_status = "APPROVED";
                                                    //        }
                                                    //        else
                                                    //        {
                                                    //            op.payment_response = "OrderNotFound";
                                                    //        }

                                                    //        UOW.selfcare_order_payment_repo.Update(op);
                                                    //        UOW.Save();

                                                    //        #endregion
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            UOW.selfcare_order_payment_repo.Update(op);
                                                    //            UOW.Save();
                                                    //        }
                                                    //    }
                                                    //}
                                                    #endregion
                                                }
                                                else if (orderitems[0].product_id == 677) // For SI Topup 
                                                {
                                                    #region For SI Topup

                                                    //string ValidateMerch = _bmsi_repo.bmsi_validate_merchant(order.purchase_msisdn, sAmount);
                                                    //if (!string.IsNullOrEmpty(ValidateMerch))
                                                    //{
                                                    //    string Recharge = _bmsi_repo.bmsi_recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                    //    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                    //    if (opRes.resultCode == 0)
                                                    //    {
                                                    //        #region BMOBILE TOPUP
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = "UpdateSuccess";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            op.payment_status = "APPROVED";
                                                    //        }
                                                    //        else
                                                    //        {
                                                    //            op.payment_response = "OrderNotFound";
                                                    //        }

                                                    //        UOW.selfcare_order_payment_repo.Update(op);
                                                    //        UOW.Save();

                                                    //        #endregion
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            UOW.selfcare_order_payment_repo.Update(op);
                                                    //            UOW.Save();
                                                    //        }
                                                    //    }
                                                    //}
                                                    #endregion
                                                }
                                            }
                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _utils_repo.ErrorLog_Txt(ex);
                            //_utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error testselfcare(notify page)", ex.Message + "<br>" + ex.StackTrace);
                        }
                        #endregion
                    }
                    //else if (sADATA == "siselfcare")
                    //{
                    //   #region  SI Selfcare Notify
                    //    SI_Selfcare_notify(oD, transidmerchant, myWORDS);
                    //    #endregion
                    //}



                    #region FOR SUCCESS RESPONSE / EMAIL RESPONSE
                    if (Request.Form["RESPONSECODE"] == "0000")
                        Response.Write("CONTINUE");

                    string response = "MALLID " + Request.Form["MALLID"] + "<br>";
                    response += "CHAINMERCHANT " + Request.Form["CHAINMERCHANT"] + "<br>" + "AMOUNT " + Request.Form["AMOUNT"] + "<br>" + "PURCHASEAMOUNT " + Request.Form["PURCHASEAMOUNT"] + "<br>" + "TRANSIDMERCHANT " + Request.Form["TRANSIDMERCHANT"] + "<br>" + "CURRENCY " + Request.Form["CURRENCY"] + "<br>";
                    response += "WORDS " + Request.Form["WORDS"] + "<br>" + "REQUESTDATETIME " + Request.Form["REQUESTDATETIME"] + "<br>" + "PAYMENTCHANNEL " + Request.Form["PAYMENTCHANNEL"] + "<br>" + "PURCHASECURRENCY " + Request.Form["PURCHASECURRENCY"] + "<br>" + "SESSIONID " + Request.Form["SESSIONID"] + "<br>";
                    response += "ADDITIONALDATA " + Request.Form["ADDITIONALDATA"] + "<br>" + "NAME " + Request.Form["NAME"] + "<br>" + "EMAIL " + Request.Form["EMAIL"] + "<br>" + "BASKET " + Request.Form["BASKET"] + "<br>";
                    response += "MYWords " + myWORDS + "<br>" + "RESULTMSg " + Request.Form["RESULTMSG"] + "<br>";
                    response += "Response code " + Request.Form["RESPONSECODE"] + "<br>";
                    response += "This is for " + sADATA;
                    //_utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku notify response", response);
                    #endregion



                }
                else
                {
                    Response.Write("STOP");
                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex);
                //_utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error testselfcare(notify page)", ex.Message + "<br>" + ex.StackTrace);
            }
            return View(oD);
        }

        public ActionResult notifytest()
        {
            DokuRDModel oRDM = new DokuRDModel();
            doku_selfcare oD = new doku_selfcare();
            try
            {
                if (true)
                {
                    string transidmerchant = "76387918";// Request.Form["TRANSIDMERCHANT"].ToString();
                    string words = "";// Request.Form["WORDS"].ToString();
                    string sSessId = Session.SessionID;

                    string sADATA = DataIdentify(transidmerchant);
                    string myWORDS = "";
                    if (sADATA == "selfcare")
                    {

                        #region Selfcare Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 1).FirstOrDefault();
                            if (doku != null)
                            {
                                //  string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + doku.resultmsg + doku.verifystatus + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                //  myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (true)
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();

                                    DokuCareModel dokucare = new DokuCareModel();
                                    dokucare.order = order;
                                    dokucare.orderitems = orderitems;
                                    dokucare.doku = oD;
                                    if (oD != null && order != null && orderitems.Count > 0)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = doku.payment_date_time;
                                        oD.paymentcode = doku.paymentcode;
                                        oD.response_code = "0000";
                                        oD.resultmsg = "SUCCESS";

                                        oD.statustype = doku.statustype;

                                        oD.verifyid = doku.verifyid;
                                        oD.verifyscore = doku.verifyscore;
                                        oD.verifystatus = "testing";

                                        oD.approvalcode = doku.approvalcode;
                                        oD.mcn = doku.mcn;

                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                if (orderitems[0].purchase_desc == "GETPOWER")
                                                {
                                                    #region GetPower

                                                    string token = string.Empty;
                                                    string units = string.Empty;
                                                    bool bRes = GetPowerProcess(order.purchase_msisdn, orderitems[0].product_name, Math.Round(orderitems[0].product_price).ToString(), Convert.ToInt16(order.user_id.ToString()), out token, out units);
                                                    if (bRes == true)
                                                    {
                                                        #region Update GetPower Success
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = units;
                                                            op.payment_receipt_no = token;
                                                            op.payment_status = "APPROVED";

                                                            dokucare.orderpayment = op;
                                                            oRDM.dkuCare = dokucare;

                                                            if (oRDM.dkuCare != null)
                                                                SendOrderSummary_email(oRDM);

                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = token + "#" + "RechargeFailed";
                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                        }
                                                    }

                                                    #endregion
                                                }
                                                else if (orderitems[0].purchase_desc == "BYP" || orderitems[0].purchase_desc == "PROMOTION")
                                                {
                                                    #region Buy BYP,PROMOTIONS

                                                    var byp = orderitems.Where(b => b.purchase_desc == "BYP").ToList();
                                                    var promo = orderitems.Where(b => b.purchase_desc == "PROMOTION").ToList();
                                                    PurchasePlanModel ppM = new PurchasePlanModel();
                                                    List<Purchasedenomination> pDenoIds = new List<Purchasedenomination>();
                                                    List<Purchasedpromotion> pPromoIds = new List<Purchasedpromotion>();
                                                    if (byp.Count > 0)
                                                    {
                                                        foreach (var bi in byp)
                                                        {
                                                            Purchasedenomination pd = new Purchasedenomination();
                                                            pd.denomination_id = bi.product_id;
                                                            pd.plan_name = bi.product_name;
                                                            pd.price = Convert.ToDouble(bi.product_price);
                                                            pDenoIds.Add(pd);
                                                        }
                                                    }
                                                    if (promo.Count > 0)
                                                    {
                                                        foreach (var pi in promo)
                                                        {
                                                            Purchasedpromotion pp = new Purchasedpromotion();
                                                            pp.promotion_id = pi.product_id;
                                                            pp.plan_name = pi.product_name;
                                                            pp.price = Convert.ToDouble(pi.product_price);
                                                            pPromoIds.Add(pp);

                                                        }
                                                    }

                                                    ppM.from_msisdn = order.cust_mobile_number;
                                                    ppM.isMobile = "1";
                                                    ppM.purchase_msisdn = order.purchase_msisdn;
                                                    ppM.pDenomIds = pDenoIds;
                                                    ppM.PromotionIds = pPromoIds;
                                                    ppM.tot_amt = order.order_product_total;
                                                    var order_payment_byp = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    if (order_payment_byp != null)
                                                        ppM.isTopUp = Convert.ToBoolean(order_payment_byp.payment_receipt_no);

                                                    int iResBYP = 0;// _plan_repo.purchase_plan(ppM);

                                                    if (iResBYP == 0)
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_receipt_no = iResBYP.ToString();
                                                            op.payment_status = "APPROVED";

                                                            dokucare.orderpayment = op;
                                                            oRDM.dkuCare = dokucare;

                                                            if (oRDM.dkuCare != null)
                                                                SendOrderSummary_email(oRDM);
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = iResBYP + "#" + "BuyBYP/PromtionFailed";
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                    }

                                                    #endregion

                                                }
                                                else if (orderitems[0].purchase_desc == "TOPUP")
                                                {
                                                    #region EasyRecharge For TopUP

                                                    string ValidateMerch = "";// _doku_repo.validate_merchant(order.purchase_msisdn, sAmount);
                                                    if (ValidateMerch == "")
                                                    {
                                                        RechargeOPModel t = new RechargeOPModel();
                                                        t.resultCode = 0;

                                                        string Recharge = JsonConvert.SerializeObject(t); //_doku_repo.recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                        var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                        if (opRes.resultCode == 0)
                                                        {



                                                            #region TOPUP SUCCESS
                                                            var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                            var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                            if (op != null)
                                                            {
                                                                op.payment_response = "UpdateSuccess";
                                                                op.payment_receipt_no = opRes.reference;
                                                                op.payment_status = "APPROVED";

                                                                dokucare.orderpayment = op;
                                                                oRDM.dkuCare = dokucare;

                                                                if (oRDM.dkuCare != null)
                                                                    SendOrderSummary_email(oRDM);
                                                            }
                                                            else
                                                                op.payment_response = "OrderNotFound";

                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                            #endregion

                                                        }
                                                        else
                                                        {
                                                            var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                            var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                            if (op != null)
                                                            {
                                                                op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                                op.payment_receipt_no = opRes.reference;
                                                                UOW.selfcare_order_payment_repo.Update(op);
                                                                UOW.Save();
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (orderitems[0].purchase_desc == "Postpaid_Pdf_bill")
                                                {
                                                    #region Postpaid Stuff

                                                    var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    if (op != null)
                                                    {
                                                        PostpaidModel pm = new PostpaidModel();
                                                        pm.cheque_num = op.payment_receipt_no;
                                                        pm.payment_amount = order.order_product_total.ToString();
                                                        string sPurchase = "1";// _care_repo.PurchasePostpaid(pm);
                                                        if (sPurchase == "1")
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_status = "APPROVED";
                                                        }
                                                        else
                                                        {
                                                            op.payment_response = "PPfailed#" + sPurchase;
                                                            op.payment_status = "PENDING";
                                                        }
                                                        dokucare.orderpayment = op;
                                                        oRDM.dkuCare = dokucare;

                                                        if (oRDM.dkuCare != null)
                                                            SendOrderSummary_email(oRDM);
                                                    }
                                                    else
                                                        op.payment_response = "OrderNotFound";

                                                    UOW.selfcare_order_payment_repo.Update(op);
                                                    UOW.Save();




                                                    #endregion
                                                }
                                            }

                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _utils_repo.ErrorLog_Txt(ex);
                            // Response.Write("Message : " + ex.Message + "<br>" + "Stacktrace : " + ex.StackTrace);
                        }
                        #endregion

                    }
                    else if (sADATA == "shop")
                    {
                        #region Shop Notify
                        try
                        {
                            var dokuShop = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 2).FirstOrDefault();
                            if (dokuShop != null)
                            {

                                //  string myFormationwrd = Convert.ToDecimal(dokuShop.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                //  myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (true)
                                {

                                    oD = UOW.doku_selfcare_repo.GetByID(dokuShop.id);
                                    long _lorder_id = oD.order_id;
                                    var orderSh = UOW.shopping_order_repo.GetByID(_lorder_id);
                                    var orderitemsSh = UOW.shopping_order_item_repo.Get(filter: oi => oi.order_id == _lorder_id).OrderBy(o=>o.product_id).ToList();
                                    //var orderTopup = UOW.order_topup_repo.Get(filter: t => t.order_id == _lorder_id).ToList();

                                    DokuShopModel obj_dokushop = new DokuShopModel();
                                    obj_dokushop.sorder = orderSh;
                                    obj_dokushop.sorderitems = orderitemsSh;
                                    obj_dokushop.sdoku = oD;

                                    //if (orderTopup != null)
                                    //    obj_dokushop.sordertopup = orderTopup;

                                    if (oD != null && orderSh != null && orderitemsSh.Count > 0)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = dokuShop.payment_date_time;
                                        oD.paymentcode = dokuShop.paymentcode;
                                        oD.response_code = "0000";
                                        oD.resultmsg = "SUCCESS";
                                        oD.statustype = dokuShop.statustype;
                                        oD.verifyid = dokuShop.verifyid;
                                        oD.verifyscore = dokuShop.verifyscore;
                                        oD.verifystatus = dokuShop.verifystatus;
                                        oD.approvalcode = dokuShop.approvalcode;
                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {

                                            var order_payment = UOW.shopping_order_payment_repo.Get(filter: p => p.order_id == _lorder_id).FirstOrDefault();
                                            //var op = UOW.shopping_order_payment_repo.GetByID(order_payment.payment_id);
                                            if (order_payment != null)
                                            {
                                                //op.payment_response = "UpdateSuccess";
                                                order_payment.payment_receipt_no = oD.approvalcode;
                                                order_payment.payment_status = "APPROVED";

                                                obj_dokushop.sorderpayment = order_payment;
                                                oRDM.dkuShop = obj_dokushop;

                                                if (oRDM.dkuShop != null)
                                                    SendOrderSummary_email(oRDM);
                                            }
                                            //else
                                            //    order_payment.payment_response = "OrderNotFound";

                                            UOW.shopping_order_payment_repo.Update(order_payment);
                                            UOW.Save();
                                        }
                                        else
                                        {
                                            #region CC Payment failed

                                            #endregion
                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write("Message : " + ex.Message + "<br>" + "Statcktrace : " + ex.StackTrace);
                        }
                        #endregion

                    }
                    else if (sADATA == "cug")
                    {

                        #region cug Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 3).FirstOrDefault();
                            if (doku != null)
                            {


                                //string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                // myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (true)
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();
                                    if (oD != null && order != null)
                                    {

                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = doku.payment_date_time;
                                        oD.paymentcode = doku.paymentcode;
                                        oD.response_code = "0000";
                                        oD.resultmsg = "SUCCESS";
                                        oD.statustype = doku.statustype;
                                        oD.verifyid = doku.verifyid;
                                        oD.verifyscore = doku.verifyscore;
                                        oD.verifystatus = doku.verifystatus;
                                        oD.approvalcode = doku.approvalcode;
                                        oD.mcn = doku.mcn;
                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                string ValidateMerch = "test";// _doku_repo.validate_merchant(order.purchase_msisdn, sAmount);
                                                if (ValidateMerch != "")
                                                {
                                                    RechargeOPModel test = new RechargeOPModel();
                                                    test.resultCode = 0;
                                                    string Recharge = JsonConvert.SerializeObject(test); //_doku_repo.recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);
                                                    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);
                                                    if (opRes.resultCode == 0)
                                                    {
                                                        #region CUG TOPUP
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = "UpdateSuccess";
                                                            op.payment_receipt_no = opRes.reference;
                                                            op.payment_status = "APPROVED";
                                                        }
                                                        else
                                                            op.payment_response = "OrderNotFound";

                                                        UOW.selfcare_order_payment_repo.Update(op);
                                                        UOW.Save();
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                        if (op != null)
                                                        {
                                                            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                            op.payment_receipt_no = opRes.reference;
                                                            UOW.selfcare_order_payment_repo.Update(op);
                                                            UOW.Save();
                                                        }
                                                    }
                                                }


                                            }

                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write("Message : " + ex.Message + "<br>" + "Statcktrace : " + ex.StackTrace);
                        }
                        #endregion
                    }
                    else if (sADATA == "bmtopup")
                    {
                        #region bmtopup Notify
                        try
                        {
                            var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 5).FirstOrDefault();
                            if (doku != null)
                            {
                                //string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                                // myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                                if (true)
                                {
                                    oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                                    long order_id = oD.order_id;
                                    var order = UOW.selfcare_order_repo.GetByID(order_id);
                                    var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();
                                    if (oD != null && order != null)
                                    {
                                        oD.order_id = oD.order_id;
                                        oD.payment_date_time = doku.payment_date_time;
                                        oD.paymentcode = doku.paymentcode;
                                        oD.response_code = "0000";
                                        oD.resultmsg = "SUCCESS";

                                        oD.statustype = doku.statustype;

                                        oD.verifyid = doku.verifyid;
                                        oD.verifyscore = doku.verifyscore;
                                        oD.verifystatus = doku.verifystatus;

                                        oD.approvalcode = doku.approvalcode;
                                        oD.mcn = doku.mcn;

                                        UOW.doku_selfcare_repo.Update(oD);
                                        UOW.Save();

                                        if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                                        {
                                            if (orderitems.Count > 0)
                                            {
                                                string sAmount = oD.totalamount;

                                                if (orderitems[0].product_id == 675) // For PNG Topup 
                                                {
                                                    #region For PNG Topup

                                                    //string ValidateMerch = _doku_repo.bmpng_validate_merchant(order.purchase_msisdn, sAmount);
                                                    //if (!string.IsNullOrEmpty(ValidateMerch))

                                                    //    string Recharge = _doku_repo.bmpng_recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                    //    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                    //    if (opRes.resultCode == 0)
                                                    //    {
                                                    //        #region BMOBILE TOPUP
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = "UpdateSuccess";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            op.payment_status = "APPROVED";
                                                    //        }
                                                    //        else
                                                    //        {
                                                    //            op.payment_response = "OrderNotFound";
                                                    //        }

                                                    //        UOW.selfcare_order_payment_repo.Update(op);
                                                    //        UOW.Save();

                                                    //        #endregion
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            UOW.selfcare_order_payment_repo.Update(op);
                                                    //            UOW.Save();
                                                    //        }
                                                    //    }
                                                    //}
                                                    #endregion
                                                }
                                                else if (orderitems[0].product_id == 677) // For SI Topup 
                                                {
                                                    #region For SI Topup

                                                    //string ValidateMerch = _bmsi_repo.bmsi_validate_merchant(order.purchase_msisdn, sAmount);
                                                    //if (!string.IsNullOrEmpty(ValidateMerch))
                                                    //{
                                                    //    string Recharge = _bmsi_repo.bmsi_recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                                    //    var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                                    //    if (opRes.resultCode == 0)
                                                    //    {
                                                    //        #region BMOBILE TOPUP
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = "UpdateSuccess";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            op.payment_status = "APPROVED";
                                                    //        }
                                                    //        else
                                                    //        {
                                                    //            op.payment_response = "OrderNotFound";
                                                    //        }

                                                    //        UOW.selfcare_order_payment_repo.Update(op);
                                                    //        UOW.Save();

                                                    //        #endregion
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    //        var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    //        if (op != null)
                                                    //        {
                                                    //            op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                    //            op.payment_receipt_no = opRes.reference;
                                                    //            UOW.selfcare_order_payment_repo.Update(op);
                                                    //            UOW.Save();
                                                    //        }
                                                    //    }
                                                    //}
                                                    #endregion
                                                }
                                            }
                                        }
                                        else
                                        {

                                            #region CC PAYMENT Failed
                                            //nothing to update
                                            #endregion

                                        }
                                    }
                                }
                                else
                                {
                                    Response.Write("STOP");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Response.Write("Message : " + ex.Message + "<br>" + "Statcktrace : " + ex.StackTrace);
                        }
                        #endregion
                    }
                    //else if (sADATA == "siselfcare")
                    //{
                    //   #region  SI Selfcare Notify
                    //    SI_Selfcare_notify(oD, transidmerchant, myWORDS);
                    //    #endregion
                    //}



                    #region FOR SUCCESS RESPONSE / EMAIL RESPONSE
                    if (Request.Form["RESPONSECODE"] == "0000")
                        Response.Write("CONTINUE");

                    string response = "MALLID " + Request.Form["MALLID"] + "<br>";
                    response += "CHAINMERCHANT " + Request.Form["CHAINMERCHANT"] + "<br>" + "AMOUNT " + Request.Form["AMOUNT"] + "<br>" + "PURCHASEAMOUNT " + Request.Form["PURCHASEAMOUNT"] + "<br>" + "TRANSIDMERCHANT " + Request.Form["TRANSIDMERCHANT"] + "<br>" + "CURRENCY " + Request.Form["CURRENCY"] + "<br>";
                    response += "WORDS " + Request.Form["WORDS"] + "<br>" + "REQUESTDATETIME " + Request.Form["REQUESTDATETIME"] + "<br>" + "PAYMENTCHANNEL " + Request.Form["PAYMENTCHANNEL"] + "<br>" + "PURCHASECURRENCY " + Request.Form["PURCHASECURRENCY"] + "<br>" + "SESSIONID " + Request.Form["SESSIONID"] + "<br>";
                    response += "ADDITIONALDATA " + Request.Form["ADDITIONALDATA"] + "<br>" + "NAME " + Request.Form["NAME"] + "<br>" + "EMAIL " + Request.Form["EMAIL"] + "<br>" + "BASKET " + Request.Form["BASKET"] + "<br>";
                    response += "MYWords " + myWORDS + "<br>" + "RESULTMSg " + Request.Form["RESULTMSG"] + "<br>";
                    response += "Response code " + Request.Form["RESPONSECODE"] + "<br>";
                    response += "This is for " + sADATA;
                    //_utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku notify response", response);
                    #endregion



                }
                else
                {
                    Response.Write("STOP");
                }
            }
            catch (Exception ex)
            {
                Response.Write("Message : " + ex.Message + "<br>" + "Stacktrace : " + ex.StackTrace);
            }
            return View();
        }

        private void SendOrderSummary_email(DokuRDModel oRDM)
        {
            try
            {
                if (oRDM.dkuCare != null)
                {
                    #region Care

                    //var email_template = UOW.email_template_repo.Get(e=>e.Id==1 && e.IsActive==true).FirstOrDefault();
                    //XElement doc = XElement.Parse(email_template.template_content);
                    XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                    XElement emailsubj = doc.Element("subj_ordersummary");
                    XElement emailBody = doc.Element("body_ordersummary");
                    XElement emailMain = doc.Element("main_ordersummary");
                    XElement emailMain_Total = doc.Element("main_total_ordersummary");
                    string sData = emailBody.Value;
                    string sMain = emailMain.Value;
                    string sMain_Total = emailMain_Total.Value;
                    string sbmobileurl = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                    sData = sData.Replace("#bmobile_url#", sbmobileurl).Replace("#Trans_Merchant_Id#", oRDM.dkuCare.doku.transidmerchant.ToString()).Replace("#payment_date#", oRDM.dkuCare.order.order_datetime.ToString()).Replace("#token_num#", oRDM.dkuCare.orderpayment.payment_receipt_no);

                    string stot_qty = "";
                    if (oRDM.dkuCare.orderitems.Count > 0)
                    {
                        decimal dtotal = 0;
                        long qty = 0;
                        for (int i = 0; i < oRDM.dkuCare.orderitems.Count; i++)
                        {
                            string description = "";
                            int sno = 0;

                            long quantity = oRDM.dkuCare.orderitems[i].product_qty;
                            string sUnits = "";
                            //qty = 0;
                            stot_qty = qty.ToString();
                            if (oRDM.dkuCare.orderitems[i].purchase_desc == "TOPUP")
                            {
                                description = "RECHARGE DONE FOR " + oRDM.dkuCare.order.purchase_msisdn;
                                sData = sData.Replace("##gpshow##", gpDisplay).Replace("#Custom_txt#", "Quantity");
                            }
                            else if (oRDM.dkuCare.orderitems[i].purchase_desc == "Postpaid_Pdf_bill")
                            {
                                description = "POST PAID ACCOUNT PAYMENT FOR SERVICE " + oRDM.dkuCare.order.purchase_msisdn;
                                sData = sData.Replace("##gpshow##", gpDisplay).Replace("#Custom_txt#", "Quantity");
                            }
                            else if (oRDM.dkuCare.orderitems[i].purchase_desc == "GETPOWER")
                            {
                                description = "Easypay for meter no: " + oRDM.dkuCare.orderitems[i].product_name;
                                sUnits = oRDM.dkuCare.orderpayment.payment_response;
                                sData = sData.Replace("##gpshow##", "display:block;");
                                sData = sData.Replace("#Custom_txt#", "Units");
                                stot_qty = "";
                            }
                            else if (oRDM.dkuCare.orderitems[i].purchase_desc == "BYP" || oRDM.dkuCare.orderitems[i].purchase_desc == "PROMOTION")
                            {
                                description = oRDM.dkuCare.orderitems[i].product_name;
                                sData = sData.Replace("##gpshow##", gpDisplay).Replace("#Custom_txt#", "Quantity");
                            }
                            sno = i + 1;
                            qty = qty + quantity;
                            dtotal = oRDM.dkuCare.order.order_product_total;
                            if (oRDM.dkuCare.orderitems[i].purchase_desc == "GETPOWER")
                            {
                                sMain = sMain.Replace("#sno#", sno.ToString()).Replace("#description#", description).Replace("#quantity#", sUnits).Replace("#product_price#", oRDM.dkuCare.orderitems[i].product_price.ToString("#0.00")).Replace("#product_total#", CalculateTotal(oRDM.dkuCare.orderitems[i].product_qty, oRDM.dkuCare.orderitems[i].product_price.ToString()));
                            }
                            else
                            {
                                sMain = sMain.Replace("#sno#", sno.ToString()).Replace("#description#", description).Replace("#quantity#", quantity.ToString()).Replace("#product_price#", oRDM.dkuCare.orderitems[i].product_price.ToString("#0.00")).Replace("#product_total#", CalculateTotal(oRDM.dkuCare.orderitems[i].product_qty, oRDM.dkuCare.orderitems[i].product_price.ToString()));
                            }
                            sData += sMain;
                            sMain = emailMain.Value;
                        }
                        sMain_Total = sMain_Total.Replace("#total_qty#", stot_qty).Replace("#Order_total#", oRDM.dkuCare.order.order_product_total.ToString("#0.00"));
                        sData += sMain_Total;

                        if (!string.IsNullOrEmpty(oRDM.dkuCare.order.cust_email))
                            _utils_repo.SendEmailMessage(oRDM.dkuCare.order.cust_email, emailsubj.Value.Trim(), sData);

                    }

                    #endregion

                }
                else if (oRDM.dkuShop != null)
                {
                    #region Shop

                    StringBuilder sb = new StringBuilder();
                    XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                    XElement emailRow = doc.Element("OrderSummary_Row");
                    string tblRow = emailRow.Value;
                    //Send Email Receipt
                    XElement emailsubj = doc.Element("OrderSummary_Subj");
                    XElement emailBody = doc.Element("OrderSummary_Body");
                    string sData = emailBody.Value;
                    string Email_id = string.Empty;
                    decimal dtotal = 0;
                    //long qty = 0;
                    if (oRDM.dkuShop.sorderitems.Count > 0)
                    {
                        int i = 0;
                        foreach (web_tbl_shopping_order_item order_item in oRDM.dkuShop.sorderitems)
                        {
                            i = i + 1;

                            long quantity = order_item.product_qty;
                            var Sub_total_price = (order_item.product_price * order_item.product_qty);

                            dtotal += Sub_total_price;

                            var product = UOW.products_repo.Get(filter: p => p.Product_ID == order_item.product_id).FirstOrDefault();

                            string product_model=" ";
                            if (product != null && order_item.product_name != "SIM Topup Charges")
                                product_model = product.Model_No;

                            string eRow = tblRow;
                            eRow = eRow.Replace("#SNo#", i.ToString()).Replace("#ProductName#", order_item.product_name).Replace("#ModelNo#", product_model);
                            eRow = eRow.Replace("#UnitPrice#", order_item.product_price.ToString()).Replace("#Qty#", order_item.product_qty.ToString()).Replace("#SubTotal#", Sub_total_price.ToString());

                            sb.Append(eRow);

                            //if (i == 0)
                            //{
                            //    var OModel = UOW.shopping_order_repo.Get(filter: (m => m.order_id == oRDM.dkuShop.sorder.order_id)).FirstOrDefault();
                            //    var ordered_user = UOW.shopping_user_contact_repo.Get(filter: (m => m.user_id == oRDM.dkuShop.sorder.user_id)).FirstOrDefault();

                            //    sData = sData.Replace("#TransactionNo#", oRDM.dkuShop.sorderpayment.payment_transaction_number.ToString()).Replace("#OrderDate#", oRDM.dkuShop.sorder.order_datetime.ToString("dd/MM/yyyy hh:mm tt"));
                            //    sData = sData.Replace("#Name#", ordered_user.first_name + " " + ordered_user.last_name).Replace("#Email#", ordered_user.email);
                            //    sData = sData.Replace("#MobileNo#", ordered_user.mobile_number);
                            //    sData = sData.Replace("#CityPostCode#", ordered_user.city + " - " + ordered_user.postcode);
                            //    sData = sData.Replace("#MobileNo#", ordered_user.mobile_number).Replace("#Country#", ordered_user.country); ;
                            //    sData = sData.Replace("#Address1#", ordered_user.address1).Replace("#Address2#", ordered_user.address2).Replace("#OrderItems#", sb.ToString());
                            //    sData = sData.Replace("#CartSubtotal#", dtotal.ToString());
                            //    string hostURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                            //    sData = sData.Replace("#URL#", hostURL);
                            //    Email_id = ordered_user.email;
                            //}
                        }

                        var OModel = UOW.shopping_order_repo.Get(filter: (m => m.order_id == oRDM.dkuShop.sorder.order_id)).FirstOrDefault();

                        web_tbl_shopping_user_contact ordered_user = new web_tbl_shopping_user_contact();

                        if (oRDM.dkuShop.sorderpayment != null && !string.IsNullOrEmpty(oRDM.dkuShop.sorderpayment.payment_response))
                            ordered_user = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(oRDM.dkuShop.sorderpayment.payment_response);

                        if (ordered_user != null)
                        {
                            sData = sData.Replace("#TransactionNo#", oRDM.dkuShop.sorderpayment.payment_transaction_number.ToString()).Replace("#OrderDate#", oRDM.dkuShop.sorder.order_datetime.ToString("dd/MM/yyyy hh:mm tt"));
                            sData = sData.Replace("#Name#", ordered_user.first_name + " " + ordered_user.last_name).Replace("#Email#", ordered_user.email);
                            sData = sData.Replace("#MobileNo#", ordered_user.mobile_number);
                            sData = sData.Replace("#CityPostCode#", ordered_user.city + " - " + ordered_user.postcode);
                            sData = sData.Replace("#MobileNo#", ordered_user.mobile_number).Replace("#Country#", ordered_user.country); ;
                            sData = sData.Replace("#Address1#", ordered_user.address1).Replace("#Address2#", ordered_user.address2).Replace("#OrderItems#", sb.ToString());
                            sData = sData.Replace("#CartSubtotal#", OModel.order_total.ToString("#0.00")).Replace("#Payment_status#", "SUCCESS");
                            string hostURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                            sData = sData.Replace("#URL#", hostURL);
                            Email_id = ordered_user.email;

                            if (!string.IsNullOrEmpty(Email_id))
                                _utils_repo.SendEmailMessage(Email_id, ToShopCC, emailsubj.Value.Trim(), sData);
                            //_utils_repo.SendEmailMessage(Email_id, emailsubj.Value.Trim(), sData);
                        }
                    }



                    #endregion

                }
            }
            catch (Exception ex)
            {
                //Skip if Email fails
            }
        }

        public ActionResult redirect()
        {
            DokuRDModel oRDM = new DokuRDModel();
            DokuShopModel oDSM = new DokuShopModel();
            DokuCareModel oDCM = new DokuCareModel();

            try
            {

                //string transidmerchant = "23777389";
                if (Request.Form["TRANSIDMERCHANT"] != null)
                {
                    string transidmerchant = Request.Form["TRANSIDMERCHANT"].ToString();
                    string words = Request.Form["WORDS"].ToString();
                    string sSessId = Request.Form["SESSIONID"].ToString();
                    string sADATA = DataIdentify(transidmerchant);

                    if (sADATA == "selfcare")
                    {
                        #region Selfcare

                        var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 1).FirstOrDefault();
                        if (doku != null)
                        {

                            string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["STATUSCODE"] + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                            string myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();


                            string response = "AMOUNT " + Request.Form["AMOUNT"] + "TRANSIDMERCHANT " + Request.Form["TRANSIDMERCHANT"] + "<br>";
                            response += "WORDS " + Request.Form["WORDS"] + "<br>" + "STATUSCODE " + Request.Form["STATUSCODE"] + "<br>" + "PAYMENTCHANNEL " + Request.Form["PAYMENTCHANNEL"] + "<br>" + "SESSIONID " + Request.Form["SESSIONID"] + "<br>";
                            response += "PAYMENTCODE " + Request.Form["PAYMENTCODE"] + "<br>";
                            response += "Mywords " + myWORDS;


                            //_utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku redirect", response);

                            if (myWORDS == Request.Form["WORDS"] && Request.Form["STATUSCODE"] == "0000")
                            {

                                oDCM = _doku_repo.GetDokuOrder(doku.order_id);

                                if (oDCM != null)
                                {
                                    oRDM.dkuCare = oDCM;
                                }
                                if (doku.resultmsg == "SUCCESS")
                                {

                                    ViewBag.TransID = transidmerchant;
                                    ViewBag.TAmount = Convert.ToDouble(doku.totalamount).ToString("#0.00");
                                    ViewBag.dSuccess = "";
                                    ViewBag.dFail = "display:none;";


                                }
                                else
                                {
                                    ViewBag.dSuccess = "display:none;";
                                    ViewBag.dFail = "";
                                }
                            }
                            else
                            {
                                ViewBag.dSuccess = "display:none;";
                                ViewBag.dFail = "";
                            }
                        }
                        #endregion
                    }
                    else if (sADATA == "shop")
                    {
                        #region Shopping
                        var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 2).FirstOrDefault();
                        if (doku != null)
                        {
                            string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["STATUSCODE"] + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                            string myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                            if (myWORDS == Request.Form["WORDS"] && Request.Form["STATUSCODE"] == "0000")
                            {
                                oDSM = _doku_repo.GetShopDokuOrder(doku.order_id);

                                if (oDSM.sorderpayment != null && !string.IsNullOrEmpty(oDSM.sorderpayment.payment_response))
                                    oDSM.suser = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(oDSM.sorderpayment.payment_response);

                                if (oDSM != null)
                                    oRDM.dkuShop = oDSM;

                                if (doku.resultmsg == "SUCCESS")
                                {
                                    ViewBag.TransID = Request.Form["TRANSIDMERCHANT"].ToString();
                                    ViewBag.TAmount = Convert.ToDouble(doku.totalamount).ToString("#0.00");
                                    ViewBag.dSuccess = "";
                                    ViewBag.dFail = "display:none;";
                                    //SendOrderSummary_email(oRDM);
                                }
                                else
                                {
                                    ViewBag.dSuccess = "display:none;";
                                    ViewBag.dFail = "";
                                }
                            }
                            else
                            {
                                ViewBag.dSuccess = "display:none;";
                                ViewBag.dFail = "";
                            }
                        }

                        #endregion
                    }
                    else if (sADATA == "cug")
                    {
                        #region CUG
                        var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 3).FirstOrDefault();
                        if (doku != null)
                        {

                            string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["STATUSCODE"] + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                            string myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();


                            string response = "AMOUNT " + Request.Form["AMOUNT"] + "TRANSIDMERCHANT " + Request.Form["TRANSIDMERCHANT"] + "<br>";
                            response += "WORDS " + Request.Form["WORDS"] + "<br>" + "STATUSCODE " + Request.Form["STATUSCODE"] + "<br>" + "PAYMENTCHANNEL " + Request.Form["PAYMENTCHANNEL"] + "<br>" + "SESSIONID " + Request.Form["SESSIONID"] + "<br>";
                            response += "PAYMENTCODE " + Request.Form["PAYMENTCODE"] + "<br>";
                            response += "Mywords " + myWORDS;

                            //_utils_repo.SendEmailMessageFROMGMAIL("raghu@twinkletech.com", "doku redirect", response);

                            if (myWORDS == Request.Form["WORDS"] && Request.Form["STATUSCODE"] == "0000")
                            {

                                string cug_return_url = ConfigurationManager.AppSettings["cug_ReturnURL"].ToString();


                                return new RedirectResult(cug_return_url + "/" + doku.order_id);
                            }
                            else
                            {
                                string cug_return_url = ConfigurationManager.AppSettings["cug_ReturnURL"].ToString();
                                return new RedirectResult(cug_return_url + "/" + doku.order_id);
                            }
                        }
                        #endregion
                    }
                    else if (sADATA == "siselfcare")
                    {
                        //#region SI Selfcare
                        //var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 4).FirstOrDefault();
                        //return Redirect(cug_return_url + "/" +doku.order_id);
                    }
                }

            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex);
                // _utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error test(redirect page)", ex.Message + "<br>" + ex.StackTrace);
            }
            return View(oRDM);
        }

        #region Login

        public ActionResult Login()
        {
            AdminModel oAM = new AdminModel();


            if (Session["beadmin"] != null)
                Session.Clear();
            return View(oAM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminModel oAM)
        {
            try
            {
                string sMsg = "";
                if (oAM.Username == sAdmName && oAM.Password == sAdmPwd)
                {
                    Session["beDoku"] = sAdmPwd;
                    return RedirectToAction("DokuTransactions");
                }
                else
                    sMsg = "Login Failed!";

                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        private List<SelectListItem> FillSiteStatus(IList<doku_selfcare> oB)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var res = (from b in oB select new { sStatus = b.site_id == 1 ? "Selfcare" : (b.site_id == 2 ? "Shopping" : ""), isactive = b.site_id }).Distinct().ToList();
            for (int i = 0; i < res.Count; i++)
            {
                var item = new SelectListItem { Text = res[i].sStatus, Value = res[i].isactive.ToString() };
                result.Add(item);
            }
            return result;
        }

        public ActionResult DokuTransactions(int? page, string sFromOrder, string sToOrder, int? ddlDokuSite)
        {
            if (Session["beDoku"] == null)
                return RedirectToAction("Login");
            IList<doku_selfcare> objdokuselfcare = new List<doku_selfcare>();
            string sMsg = "";
            ViewData["sFromOrder"] = sFromOrder;
            ViewData["sToOrder"] = sToOrder;
            ViewData["ddlDokuSite"] = ddlDokuSite;
            try
            {
                objdokuselfcare = UOW.doku_selfcare_repo.Get(orderBy: d => d.OrderByDescending(o => o.created_on)).ToList();
                int currentPageIndex = page.HasValue ? page.Value : 1;

                ViewBag.sitelist = FillSiteStatus(objdokuselfcare);
                if (objdokuselfcare.Count > 0)
                {

                    if (ddlDokuSite > 0)
                        objdokuselfcare = objdokuselfcare.Where(b => b.site_id == ddlDokuSite).ToList();

                    if (!string.IsNullOrEmpty(sFromOrder))
                    {
                        DateTime FromOrder = Convert.ToDateTime(sFromOrder);
                        objdokuselfcare = objdokuselfcare.Where(b => b.created_on.Date >= FromOrder.Date).ToList();
                    }

                    if (!string.IsNullOrEmpty(sToOrder))
                    {
                        DateTime ToOrder = Convert.ToDateTime(sToOrder);
                        objdokuselfcare = objdokuselfcare.Where(b => b.created_on.Date <= ToOrder.Date).ToList();
                    }

                    objdokuselfcare = objdokuselfcare.ToPagedList(currentPageIndex, defaultPageSize);

                    if (objdokuselfcare.Count == 0)
                        sMsg = "No Details found matching your search criteria!";

                }
                else
                    sMsg = "Not found!";

                ViewBag.Message = sMsg;

                if (Request.IsAjaxRequest())
                    return PartialView("_Ajax_DokuTransactions", objdokuselfcare);
                else
                    return View(objdokuselfcare);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);

            }
            return View(objdokuselfcare);
        }

        #region GetPowerProcess

        private bool GetPowerProcess(string msisdn_no, string meter_no, string amount, int userid, out string token, out string units)
        {
            bool bRes = false;
            token = string.Empty;
            units = string.Empty;
            string myi = "";
            try
            {

                GetPowerModelIP buypower = new GetPowerModelIP();
                buypower.sUserName = gp_java_uid;
                buypower.sPassword = gp_java_pwd;
                buypower.sMeterNo = meter_no;
                buypower.dAmount = Convert.ToInt64(amount);
                buypower.sMSISDN = msisdn_no;
                buypower.sKeyCode = gp_merc_keycode;
                buypower.sIPAddress = "172.16.23.186";
                buypower.iProcessMode = 0;
                buypower.user_id = userid;


                string ipdata = JsonConvert.SerializeObject(buypower);
                string enc_ipdata = _utils_repo.AES_ENCWR(ipdata);

                string enc_uid = _utils_repo.AES_ENCWR(gp_svc_uid);
                string enc_pwd = _utils_repo.AES_ENCWR(gp_svc_pwd);
                string ResCode = _esipay.GetTokenNumber(enc_uid, enc_pwd, _utils_repo.AES_DECWR(enc_ipdata));

                myi = "#rescode : " + ResCode + "#input : " + _utils_repo.AES_DECWR(enc_ipdata);
                if (!string.IsNullOrEmpty(ResCode))
                {
                    string xmldata = ResCode;
                    //using (System.IO.StreamReader sr = new System.IO.StreamReader(Server.MapPath("~/EmailTemplate/gettoken.xml")))
                    //{
                    //    xmldata = sr.ReadToEnd();
                    //}

                    if (!string.IsNullOrEmpty(xmldata))
                    {
                        System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                        xDoc.LoadXml(xmldata);
                        System.Xml.XmlNode node = xDoc.DocumentElement.SelectSingleNode("//success");
                        if (node.InnerText.Trim() == "1")
                        {
                            System.Xml.XmlNodeList xNodelst = xDoc.SelectNodes("//suprima/thin-client/vend/token");
                            if (xNodelst.Count > 0)
                            {
                                System.Xml.XmlNode xNode = xNodelst.Item(0);
                                for (int i = 0; i < xNode.ChildNodes.Count; i++)
                                {
                                    if (xNode.ChildNodes[i].Name.ToLower() == "tk50")
                                    {
                                        units = xNode.ChildNodes[i].InnerText.Trim();
                                        bRes = true;
                                    }
                                    else if (xNode.ChildNodes[i].Name.ToLower() == "tk60")
                                    {
                                        token = xNode.ChildNodes[i].InnerText.Trim();
                                        bRes = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    token = ResCode.ToString();
                if (bRes == true)
                {

                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace + myi);
            }
            return bRes;
        }

        #endregion


        private void SendGPSMS(string msisdn, string amt, string meterno, string token, string units, string transid)
        {
            try
            {
                XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                XElement smsBody = doc.Element("GetPowerSMS");
                string sData = smsBody.Value;
                sData = sData.Replace("##gp_paidamt##", "K " + Convert.ToDouble(amt).ToString("#0.00").Trim()).Replace("##gp_meterno##", meterno.Trim()).Replace("##gp_token##", token.Trim()).Replace("##gp_units##", units.Trim()).Replace("##trans_id##", transid.Trim());
                sData = sData.Trim();
                string sRes = _doku_repo.SendSMS(msisdn.Trim(), sData.Trim());
            }
            catch (Exception ex)
            {

            }
        }
        private void SI_Selfcare_notify(doku_selfcare oD, string transidmerchant, string myWORDS)
        {
            #region SI Selfcare Notify
            try
            {
                var doku = UOW.doku_selfcare_repo.Get(filter: d => d.transidmerchant == transidmerchant && d.site_id == 4).FirstOrDefault();
                if (doku != null)
                {


                    string myFormationwrd = Convert.ToDecimal(doku.totalamount).ToString("#0.00") + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + transidmerchant + Request.Form["RESULTMSG"].ToString() + Request.Form["VERIFYSTATUS"].ToString() + ConfigurationManager.AppSettings["dk_Currency"].ToString();
                    myWORDS = CalculateSha1(myFormationwrd, Encoding.Default).ToLower();

                    if (myWORDS == Request.Form["WORDS"])
                    {
                        oD = UOW.doku_selfcare_repo.GetByID(doku.id);
                        long order_id = oD.order_id;
                        var order = UOW.selfcare_order_repo.GetByID(order_id);
                        var orderitems = UOW.selfcare_order_item_repo.Get(filter: oi => oi.order_id == order_id).ToList();
                        if (oD != null && order != null && orderitems.Count > 0)
                        {

                            oD.order_id = oD.order_id;
                            oD.payment_date_time = Request.Form["PAYMENTDATETIME"];
                            oD.paymentcode = Request.Form["PAYMENTCODE"];
                            oD.response_code = Request.Form["RESPONSECODE"].ToString();
                            oD.resultmsg = Request.Form["RESULTMSG"].ToString();

                            oD.statustype = Request.Form["STATUSTYPE"].ToString();

                            oD.verifyid = Request.Form["VERIFYID"].ToString();
                            oD.verifyscore = Request.Form["VERIFYSCORE"];
                            oD.verifystatus = Request.Form["VERIFYSTATUS"].ToString();

                            oD.approvalcode = Request.Form["APPROVALCODE"].ToString();
                            oD.mcn = Request.Form["MCN"].ToString();

                            UOW.doku_selfcare_repo.Update(oD);
                            UOW.Save();

                            if (oD.response_code == "0000" && oD.resultmsg == "SUCCESS")
                            {
                                if (orderitems.Count > 0)
                                {
                                    string sAmount = oD.totalamount;

                                    if (orderitems[0].purchase_desc == "SITOPUP")
                                    {
                                        #region EasyRecharge

                                        string ValidateMerch = _doku_repo.validate_merchant(order.purchase_msisdn, sAmount);
                                        if (ValidateMerch != "")
                                        {
                                            string Recharge = _doku_repo.recharge_msisdn(order.purchase_msisdn, sAmount, ValidateMerch);

                                            var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);

                                            if (opRes.resultCode == 0)
                                            {

                                                if (orderitems[0].purchase_desc == "SITOPUP")
                                                {
                                                    #region SI TOPUP
                                                    var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                    var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                    if (op != null)
                                                    {
                                                        op.payment_response = "UpdateSuccess";
                                                        op.payment_receipt_no = opRes.reference;
                                                        op.payment_status = "APPROVED";
                                                    }
                                                    else
                                                        op.payment_response = "OrderNotFound";

                                                    UOW.selfcare_order_payment_repo.Update(op);
                                                    UOW.Save();
                                                    #endregion
                                                }
                                            }
                                            else
                                            {
                                                var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();
                                                var op = UOW.selfcare_order_payment_repo.GetByID(order_payment.payment_id);
                                                if (op != null)
                                                {
                                                    op.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                                    op.payment_receipt_no = opRes.reference;
                                                    UOW.selfcare_order_payment_repo.Update(op);
                                                    UOW.Save();
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }

                            }
                            else
                            {

                                #region CC PAYMENT Failed
                                //nothing to update
                                #endregion

                            }
                        }
                    }
                    else
                    {
                        Response.Write("STOP");
                    }
                }
            }
            catch (Exception ex)
            {
                _utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error(notify page)", ex.Message + "<br>" + ex.StackTrace);
            }
            #endregion
        }


        public ActionResult SIProcessDoku(string id)
        {

            DokuPostModel oDM = new DokuPostModel();
            try
            {
                string sproSessId = "";
                if (!string.IsNullOrEmpty(id))
                {
                    sproSessId = id;

                    var tempOrder = UOW.temp_orders_repo.Get(filter: t => t.Session_Id == sproSessId).LastOrDefault();
                    long orderId = 0;
                    int iSiteId = 0;

                    if (tempOrder != null)
                    {
                        orderId = Convert.ToInt64(tempOrder.Order_Id);
                        iSiteId = Convert.ToInt32(tempOrder.SiteId);
                    }

                    if (orderId != 0 && iSiteId != 0)
                    {
                        long order_id = orderId;
                        var dokuTrans = UOW.doku_selfcare_repo.Get(filter: d => d.order_id == order_id && d.site_id == iSiteId).FirstOrDefault();
                        var orderTrans = UOW.selfcare_order_repo.GetByID(order_id);
                        var orderItem = UOW.selfcare_order_item_repo.Get(filter: d => d.order_id == order_id).ToList();
                        if (dokuTrans != null && orderTrans != null && orderItem.Count > 0)
                        {
                            oDM.ACTIONURL = ConfigurationManager.AppSettings["dk_Form_Action"].ToString();
                            oDM.MALLID = ConfigurationManager.AppSettings["dk_MALLID"].ToString();
                            oDM.CHAINMERCHANT = ConfigurationManager.AppSettings["dk_chainMerchant"].ToString();
                            oDM.AMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                            oDM.PURCHASEAMOUNT = Convert.ToDecimal(dokuTrans.totalamount).ToString("#0.00");
                            oDM.TRANSIDMERCHANT = orderTrans.order_number.ToString();
                            oDM.CURRENCY = ConfigurationManager.AppSettings["dk_SICurrency"].ToString();
                            string sFormationwrd = oDM.AMOUNT + oDM.MALLID + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + oDM.TRANSIDMERCHANT + oDM.CURRENCY;
                            oDM.WORDS = CalculateSha1(sFormationwrd, Encoding.Default).ToLower();
                            oDM.REQUESTDATETIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                            oDM.PAYMENTCHANNEL = ConfigurationManager.AppSettings["dk_paymentChannel"].ToString();
                            oDM.PURCHASECURRENCY = ConfigurationManager.AppSettings["dk_SIPurchaseCurrency"].ToString();
                            oDM.SESSIONID = dokuTrans.session_id;
                            oDM.ADDITIONALDATA = "siselfcare";
                            //   oDM.CUSTOMERID = dokuTrans.customer_id.ToString();
                            oDM.NAME = orderTrans.cust_fname + " " + orderTrans.cust_lname;
                            oDM.EMAIL = orderTrans.cust_email;

                            string line_items = "";

                            foreach (var item in orderItem)
                            {

                                if (item.purchase_desc.ToLower() == "sitopup")
                                    line_items += "TOPUP FOR " + orderTrans.purchase_msisdn + "," + oDM.AMOUNT + "," + item.product_qty + "," + CalculateTotal(item.product_qty, oDM.AMOUNT) + ";";

                            }
                            oDM.BASKET = line_items;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _utils_repo.SendEmailMessageFROMGMAIL(ConfigurationManager.AppSettings["bugEmailTo"].ToString(), "doku error(SI PROCESS DOKU)", ex.Message + "<br>" + ex.StackTrace);
            }

            return View(oDM);
        }

    }
}
