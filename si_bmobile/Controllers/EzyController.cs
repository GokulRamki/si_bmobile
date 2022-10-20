using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using si_bmobile.Models;
using System.Configuration;
using si_bmobile.DAL;
using System.Xml.Linq;
using bmdoku.bmkuRef;
using Newtonsoft.Json;
using si_bmobile.Utils;
using System.Net;

namespace si_bmobile.Controllers
{
    public class EzyController : Controller
    {
        //
        // GET: /Ezy/

        UnitOfWork _UOW;

        private EasyRecharge _easyRecharge;
        private IUtilityRepository _util_repo;
        private IShopRepository _Shop_repo;
        private string dk_encry_pwd;
        private string dk_plain_pwd;
        private string dk_merchantid;
        private string dk_username;
        private string dk_keycode;
        public EzyController()
        {
            this._UOW = new UnitOfWork();
            this._util_repo = new UtilityRepository();
            this._easyRecharge = new EasyRecharge();
            this.dk_merchantid = ConfigurationManager.AppSettings["pei_merchantid"].ToString();
            this.dk_username = ConfigurationManager.AppSettings["pei_username"].ToString();
            this.dk_plain_pwd = ConfigurationManager.AppSettings["pei_plain_pwd"].ToString();
            this.dk_encry_pwd = ConfigurationManager.AppSettings["pei_encry_pwd"].ToString();
            this.dk_keycode = ConfigurationManager.AppSettings["pei_keycode"].ToString();
            this._Shop_repo = new ShopRepository();
        }


        public ActionResult ShopEzy()
        {
            EzyModel oEM = new EzyModel();
            try
            {

                if (Session["subscriber"] == null)
                    return RedirectToAction("Login");

                else
                {
                    long order_id = 0;
                    if (Session["seEztOrederid"] != null)
                        order_id = Convert.ToInt64(Session["seEztOrederid"]);

                    web_tbl_shopping_order_payment order_payment = new web_tbl_shopping_order_payment();

                    order_payment = _UOW.shopping_order_payment_repo.Get(filter: p => p.order_id == order_id).FirstOrDefault();

                    if (order_payment != null)
                    {
                        oEM.ezt_amount = order_payment.payment_total.ToString("#0.00");
                        oEM.ezt_refnumber = order_payment.payment_transaction_number;
                        oEM.ezt_orderinfo = order_id.ToString();
                        oEM.ezt_user_key = ConfigurationManager.AppSettings["user_key"].ToString();
                        oEM.ezt_keycode = ConfigurationManager.AppSettings["keycode"].ToString();
                        oEM.ezt_secure_secret = ConfigurationManager.AppSettings["secure_secret"].ToString();
                        oEM.ezt_return_url = ConfigurationManager.AppSettings["returnURL"].ToString();
                        oEM.ezt_cancel_url = ConfigurationManager.AppSettings["cancelURL"].ToString();
                        oEM.ezt_logo_url = ConfigurationManager.AppSettings["logoURL"].ToString();
                        oEM.paymentURL = ConfigurationManager.AppSettings["mkpaymentURL"].ToString();
                        if (!string.IsNullOrEmpty(order_payment.payment_receipt_no) != null)
                            oEM.ezt_txn_data1 = order_payment.payment_receipt_no;
                        
                    }
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return View(oEM);
        }


        public ActionResult EzyTopup()
        {
            if (Session["subscriber"] == null)
                return RedirectToAction("Login", "Care");

            long lOrderId = 0;
            string id = Request.QueryString["ezt_OrderInfo"].ToString();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    bool isNumeric = long.TryParse(id, out lOrderId);
                    if (isNumeric)
                    {
                        web_tbl_shopping_order order = new web_tbl_shopping_order();
                        order = _UOW.shopping_order_repo.Get(filter: r => r.order_id == lOrderId && r.order_is_delivery == false).FirstOrDefault();

                        web_tbl_shopping_order_item order_item = new web_tbl_shopping_order_item();
                        order_item = _UOW.shopping_order_item_repo.Get(filter: oi => oi.order_id == lOrderId).FirstOrDefault();

                        if (order != null && order_item != null)
                        {

                            string ValidateMerch = validate_merchant(order_item.product_name, order.order_total.ToString());
                            if (ValidateMerch != "")
                            {
                                string Recharge = recharge_msisdn(order_item.product_name, order_item.product_price.ToString(), ValidateMerch);

                                var opRes = JsonConvert.DeserializeObject<RechargeOPModel>(Recharge);
                                web_tbl_shopping_order_payment order_payment = new web_tbl_shopping_order_payment();
                                order_payment = _UOW.shopping_order_payment_repo.Get(filter: p => p.order_id == lOrderId).FirstOrDefault();
                                if (order_payment != null)
                                {

                                    if (opRes.resultCode == 0)
                                    {
                                        #region TOPUP SUCCESS

                                        order_payment.payment_response = "UpdateSuccess#ref:"+opRes.reference;
                                        //                                        order_payment.payment_receipt_no = 
                                        order_payment.payment_status = "APPROVED";

                                        _UOW.shopping_order_payment_repo.Update(order_payment);
                                        _UOW.Save();

                                        web_tbl_shopping_order order_delivery = new web_tbl_shopping_order();
                                        order_delivery = _UOW.shopping_order_repo.GetByID(lOrderId);
                                        if (order_delivery != null)
                                        {
                                            order_delivery.order_is_delivery = true;
                                            _UOW.shopping_order_repo.Update(order_delivery);
                                            _UOW.Save();
                                        }
                                        EzyOrderModel oEOM = new EzyOrderModel();
                                        oEOM = _Shop_repo.GetEzyOrderDetails(lOrderId);
                                        if (oEOM != null)
                                        {
                                            SendOrderSummary_email(oEOM);
                                        }


                                        return RedirectToAction("OrderSummary", new { id = lOrderId.ToString() });
                                        #endregion

                                    }
                                    else
                                    {
                                        order_payment.payment_response = opRes.resultCode.ToString() + "#" + "RechargeFailed";
                                        order_payment.payment_receipt_no = opRes.reference;
                                        _UOW.shopping_order_payment_repo.Update(order_payment);
                                        _UOW.Save();

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
            return Content("Invalid Request");
        }



        #region For Recharge APIs

        /// <summary>
        /// Validate Merchant using the config details
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <returns>json format string</returns>
        public string validate_merchant(string msisdn, string amount)
        {
            string sRes = "";



            RechargeModel obj_regcharge = new RechargeModel();
            obj_regcharge.keycode = dk_keycode;
            obj_regcharge.username = dk_username;
            obj_regcharge.password = dk_plain_pwd;
            obj_regcharge.msisdn = msisdn;
            obj_regcharge.amount = amount;

            var data = JsonConvert.SerializeObject(obj_regcharge, Formatting.Indented);

            var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.ValidateMerchant(dk_merchantid, _util_repo.AES_JENC(data)));

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
        public string recharge_msisdn(string msisdn, string amount, string data)
        {
            string sRes = "";

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


                    var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.RechargeMsisdn(dk_merchantid, encry_data));

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

        #region Order Summary

        public ActionResult OrderSummary(string id)
        {

            if (Session["subscriber"] == null)
                return RedirectToAction("Login", "Care");

            EzyOrderModel oEOM = new EzyOrderModel();
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    long orderId = Convert.ToInt64(id);


                    oEOM = _Shop_repo.GetEzyOrderDetails(orderId);

                    if (oEOM != null)
                    {
                        if (oEOM.eo != null && oEOM.eoi.Count > 0 && oEOM.eop != null)
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
            return View(oEOM);
        }

        #endregion

        private void SendOrderSummary_email(EzyOrderModel oEOM)
        {
            try
            {
                if (oEOM.eo != null)
                {
                    #region Doku

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

                    //oRDM.dkuCare.doku = UOW.doku_repo.Get(filter: o => o.order_id == oRDM.dkuCare.order.order_id).FirstOrDefault();

                    sData = sData.Replace("#Trans_Merchant_Id#", oEOM.eop.payment_transaction_number.ToString()).Replace("#payment_date#", oEOM.eo.order_datetime.ToString());

                    string stot_qty = "";
                    if (oEOM.eoi.Count > 0)
                    {
                        decimal dtotal = 0;
                        long qty = 0;
                        for (int i = 0; i < oEOM.eoi.Count; i++)
                        {
                            string description = "";
                            int sno = 0;

                            long quantity = oEOM.eoi[i].product_qty;
                            string sUnits = "";
                            qty += oEOM.eoi[i].product_qty;
                            stot_qty = qty.ToString();

                            if (oEOM.eoi[i].product_id == -11)
                            {
                                description = "RECHARGE DONE FOR " + oEOM.eoi[i].product_name;
                                sData = sData.Replace("##gpshow##", "display:none !important;display:none;").Replace("#Custom_txt#", "Quantity");
                                sData = sData.Replace("##trandisp##", "display:none !important;display:none;");
                                sData = sData.Replace("#Custom_Pricetxt#", "Price");
                                sData = sData.Replace("#Custom_TotalPricetxt#", "Total Price");
                            }

                            //else if (oRDM.dkuCare.orderitems[i].purchase_desc == "SHOP")
                            //{

                            //}

                            sno = i + 1;
                            // qty = qty + quantity;
                            dtotal = oEOM.eo.order_product_total;

                            sMain = sMain.Replace("#sno#", sno.ToString()).Replace("#description#", description).Replace("#quantity#", qty.ToString()).Replace("#product_price#", oEOM.eoi[i].product_price.ToString("#0.00")).Replace("#product_total#", (oEOM.eo.order_product_total).ToString("#0.00"));

                            sData += sMain;
                            sMain = emailMain.Value;
                        }

                        sMain_Total = sMain_Total.Replace("##trandisp##", "display:none !important;display:none;");
                        sMain_Total = sMain_Total.Replace("#total_qty#", stot_qty).Replace("#Order_total#", oEOM.eo.order_product_total.ToString("#0.00"));

                        sData += sMain_Total;

                        if (!string.IsNullOrEmpty(oEOM.eoi[0].product_sku))
                            _util_repo.SendEmailMessage(oEOM.eoi[0].product_sku, emailsubj.Value.Trim(), sData);//


                    }

                    #endregion
                }

            }
            catch (System.Exception ex)
            {
                //Skip if Email fails 
                _util_repo.ErrorLog_Txt(ex);
            }
        }

        #region Dispose Repo
        /// <summary>
        /// Dispose Repository objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _UOW.Dispose();
            base.Dispose(disposing);
        }
        #endregion

    }
}
