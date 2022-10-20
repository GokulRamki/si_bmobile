using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using bemobile.Models;
using bemobile.Utils;
using System.Configuration;
using System.Text;
using bmdoku.bmkuRef;
using Newtonsoft.Json;
using bemobile.dokuRef;

namespace bemobile.DAL
{

    public class dokuRepository:IdokuRepository,IDisposable
    {
        UnitOfWork UOW;

        bmtestshopDBEntities _db;
        private EasyRecharge _easyRecharge;
        private IUtilityRepository _util_repo;
        private IdokuServiceClient _doku_client;

        //private string dk_encry_pwd;
        //private string dk_plain_pwd;
        //private string dk_merchantid;
        //private string dk_username;
        //private string dk_keycode;
        //private string png_uname;
        //private string png_pwd;
        //private string png_keycode;
        //private string png_merch_id;

        private string gp_merchantid;
        private string gp_username;
        private string gp_keycode;
        private string gp_password;
        
        public dokuRepository()
        {
            this._easyRecharge = new EasyRecharge();
            this._util_repo = new UtilityRepository();
            this._doku_client = new IdokuServiceClient();

            //this.dk_merchantid = ConfigurationManager.AppSettings["dk_merchantid"].ToString();
            //this.dk_username = ConfigurationManager.AppSettings["dk_username"].ToString();
            //this.dk_plain_pwd = ConfigurationManager.AppSettings["dk_plain_pwd"].ToString();
            //this.dk_encry_pwd = ConfigurationManager.AppSettings["dk_encry_pwd"].ToString();
            //this.dk_keycode = ConfigurationManager.AppSettings["dk_keycode"].ToString();
            //this.png_uname = ConfigurationManager.AppSettings["png_uname"].ToString();
            //this.png_pwd = ConfigurationManager.AppSettings["png_pwd"].ToString();
            //this.png_keycode = ConfigurationManager.AppSettings["png_keycode"].ToString();
            //this.png_merch_id = ConfigurationManager.AppSettings["png_merch_id"].ToString();

            this.gp_merchantid = ConfigurationManager.AppSettings["gp_merc_id"].ToString();
            this.gp_username = ConfigurationManager.AppSettings["gp_merc_uname"].ToString();
            this.gp_password = ConfigurationManager.AppSettings["gp_merc_pwd"].ToString();
            this.gp_keycode = ConfigurationManager.AppSettings["sms_merc_keycode"].ToString();
          

            this.UOW = new UnitOfWork();
            this._db = new bmtestshopDBEntities();
        }



        #region Generic Selfcare Order




        #endregion


        #region SelfCare Order


        private string UniqueTransactionNumber(string rnstring)
        {
            string res = "";
            var order_payment = UOW.selfcare_order_payment_repo.Get(filter: p => p.payment_transaction_number == rnstring).ToList();
            if (order_payment.Count > 0)
            {
                res = _util_repo.GetRandomNumber(8);
                res = UniqueTransactionNumber(res);
            }
            else
                res = rnstring;
            return res;

        }

        /// <summary>
        /// Save selfcare order details
        /// </summary>
        /// <param name="oTP"></param>
        /// <returns></returns>
        public long SaveSelfcareOrder(Payment_EntModel oTP)
        {
            long retOrderId = 0;
            web_tbl_selfcare_order oSO = new web_tbl_selfcare_order();
            oSO.order_id = 0;
            oSO.order_number = Convert.ToInt64(UniqueTransactionNumber(_util_repo.GetRandomNumber(8)));
            oSO.order_product_total = Convert.ToDecimal(oTP.paid_amount);
            oSO.order_freight_total = 0;
            oSO.order_surcharge = 0;
            oSO.order_datetime = DateTime.Now;
            oSO.user_id = oTP.paid_userid;
            oSO.cust_fname = oTP.fname;
            oSO.cust_lname = oTP.lname;
            oSO.cust_email = oTP.email;
            oSO.cust_mobile_number = oTP.primary_msisdn;
            oSO.payment_mode = oTP.payment_mode;
            oSO.purchase_msisdn = oTP.paid_for_msisdn;
            UOW.selfcare_order_repo.Insert(oSO);
            UOW.Save();
            if (oSO.order_id > 0)
            {
                web_tbl_selfcare_order_item oSI = new web_tbl_selfcare_order_item();
                oSI.order_id = oSO.order_id;
                oSI.order_item_id = 0;
                oSI.product_id = Convert.ToInt64(oTP.product_id);
                oSI.product_name = oTP.product_name;
                oSI.product_price = Convert.ToDecimal(oTP.paid_amount);
                oSI.product_qty = 1;
                oSI.product_shipping_matrix_id = 0;
                oSI.purchase_desc = oTP.purchse_desc;
                UOW.selfcare_order_item_repo.Insert(oSI);
                UOW.Save();
                if (oSI.order_item_id > 0)
                {
                    web_tbl_selfcare_payment oSP = new web_tbl_selfcare_payment();
                    oSP.order_id = oSO.order_id;
                    oSP.payment_datetime = DateTime.Now;
                    oSP.payment_gateway = oTP.payment_gateway;
                    oSP.payment_id = 0;
                    oSP.payment_receipt_no = oTP.receiptno;
                    oSP.payment_response = "";
                    oSP.payment_status = "PENDING";
                    oSP.payment_total = Convert.ToDecimal(oTP.paid_amount);
                    oSP.payment_transaction_number = oSO.order_number.ToString();
                    oSP.payment_type = oTP.payment_type;
                    UOW.selfcare_order_payment_repo.Insert(oSP);
                    UOW.Save();
                    retOrderId = oSP.order_id;
                }
            }
            return retOrderId;
        }

        /// <summary>
        /// save doku payment order
        /// </summary>
        /// <param name="oTP"></param>
        /// <param name="order_id"></param>
        /// <returns></returns>
        public long SaveDokuSelfcareTrans(Payment_EntModel oTP, long order_id, int site_id)
        {
            long retVal = 0;
            doku_selfcare oDS = new doku_selfcare();
            oDS.id = 0;
            oDS.order_id = order_id;
            string stransidmerchant = "";
            if (site_id == 1)
            {
                var order = UOW.selfcare_order_repo.GetByID(order_id);
                stransidmerchant = order.order_number.ToString();
            }
            else if (site_id == 2)
            {
                var order_payment = UOW.shopping_order_payment_repo.Get(filter:s=>s.order_id==order_id).FirstOrDefault();
                stransidmerchant = order_payment.payment_transaction_number;
            }
            else if (site_id == 3)
            {
                var order = UOW.selfcare_order_repo.GetByID(order_id);
                stransidmerchant = order.order_number.ToString();
            }

            oDS.transidmerchant = stransidmerchant;
            oDS.customer_id = oTP.paid_userid;
            oDS.totalamount =oTP.paid_amount;
            string sFormationwords = oDS.totalamount + ConfigurationManager.AppSettings["dk_MALLID"].ToString() + ConfigurationManager.AppSettings["dk_Shared_key"].ToString() + oDS.transidmerchant + ConfigurationManager.AppSettings["dk_Currency"].ToString();
            oDS.words = _util_repo.CalculateSha1(sFormationwords, Encoding.Default).ToLower();
            oDS.statustype = "";
            oDS.response_code = "";
            oDS.approvalcode = "";
            oDS.resultmsg = "PENDING";
            oDS.payment_channel =ConfigurationManager.AppSettings["dk_paymentChannel"];
            oDS.paymentcode = "0";
            oDS.session_id =oTP.sess_id;
            oDS.bank_issuer = "";
            oDS.mcn = "";
            oDS.payment_date_time = DateTime.Now.ToString("yyyyMMddHHmmss");
            oDS.verifyid = "";
            oDS.verifyscore = "0";
            oDS.verifystatus = "";
            oDS.site_id = site_id;
            oDS.created_on = DateTime.Now;
            UOW.doku_selfcare_repo.Insert(oDS);
            UOW.Save();
            retVal = oDS.id;
            return retVal;
        }

        #endregion

        #region For Selfcare Doku Orders
        public List<DokuCareModel> GetDokuOrderTransaction()
        {
            List<DokuCareModel> resTrans = new List<DokuCareModel>();
        
            var transactions = (from so in _db.selfcare_order
                                join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                                join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                                where dk.site_id==1
                                select new DokuCareModel
                                {
                                    order = so,
                                    orderpayment = sop,
                                    doku = dk
                                }).OrderByDescending(s => s.order.order_id).ToList();

            if (transactions.Count > 0)
            {
                double dBSP=0;
                double dDep = 0;
                double dAmt = 0;
                foreach (var item in transactions)
                {
                    DokuCareModel dkuCare = new DokuCareModel();
                    dkuCare.order = item.order;
                    dkuCare.orderitems = GetOrderItems(item.order.order_id);
                    dkuCare.orderpayment = item.orderpayment;
                    dkuCare.doku = item.doku;
                    dAmt = Convert.ToDouble(item.order.order_product_total);
                    dBSP = CalcBSPCommission(dAmt);
                    dDep = dAmt - dBSP;
                    dkuCare.amt = dAmt;
                    dkuCare.bsp_amt = dBSP;
                    dkuCare.dep_amt = dDep;
                    resTrans.Add(dkuCare);
                }
               
            }
           // resTrans = resTrans.OrderByDescending(r => r.order.order_id).ToList();
            return resTrans;
        }

        private double CalcBSPCommission(double dAmt)
        {
            double dRes = 0;
            double dPerc = Convert.ToDouble(ConfigurationManager.AppSettings["bspPercentage"]);
            dRes = (dAmt * dPerc) / 100;
            return dRes;
        }

        public DokuCareModel GetDokuOrder(long OrderId)
        {
            DokuCareModel Order = new DokuCareModel();
            var Orders = (from so in _db.selfcare_order
                     join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                     join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                     where dk.order_id == OrderId && dk.site_id==1
                     select new DokuCareModel 
                     {
                         order = so,
                         orderpayment = sop,
                         doku = dk
                     }).FirstOrDefault();
            if (Orders !=null)
            {
                Order.order=Orders.order;
                Order.orderpayment=Orders.orderpayment;
                Order.doku=Orders.doku;
                Order.orderitems = GetOrderItems(Orders.order.order_id);
            }
            return Order;

        }

        public List<DokuCareModel> GetCUGDokuOrderTransaction()
        {
            List<DokuCareModel> resTrans = new List<DokuCareModel>();

            var transactions = (from so in _db.selfcare_order
                                join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                                join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                                where dk.site_id == 3
                                select new DokuCareModel
                                {
                                    order = so,
                                    orderpayment = sop,
                                    doku = dk
                                }).OrderByDescending(s => s.order.order_id).ToList();

            if (transactions.Count > 0)
            {
                double dBSP = 0;
                double dDep = 0;
                double dAmt = 0;
                foreach (var item in transactions)
                {
                    DokuCareModel dkuCare = new DokuCareModel();
                    dkuCare.order = item.order;
                    dkuCare.orderitems = GetOrderItems(item.order.order_id);
                    dkuCare.orderpayment = item.orderpayment;
                    dkuCare.doku = item.doku;
                    dAmt = Convert.ToDouble(item.order.order_product_total);
                    dBSP = CalcBSPCommission(dAmt);
                    dDep = dAmt - dBSP;
                    dkuCare.amt = dAmt;
                    dkuCare.bsp_amt = dBSP;
                    dkuCare.dep_amt = dDep;
                    resTrans.Add(dkuCare);
                }

            }
            // resTrans = resTrans.OrderByDescending(r => r.order.order_id).ToList();
            return resTrans;
        }

        public DokuCareModel GetDokuOrderCUG(long OrderId)
        {
            DokuCareModel Order = new DokuCareModel();
            var Orders = (from so in _db.selfcare_order
                          join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                          join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                          where dk.order_id == OrderId && dk.site_id == 3
                          select new DokuCareModel
                          {
                              order = so,
                              orderpayment = sop,
                              doku = dk
                          }).FirstOrDefault();
            if (Orders != null)
            {
                Order.order = Orders.order;
                Order.orderpayment = Orders.orderpayment;
                Order.doku = Orders.doku;
                Order.orderitems = GetOrderItems(Orders.order.order_id);
            }
            return Order;

        }

        private List<web_tbl_selfcare_order_item> GetOrderItems(long orderId)
        {
            List<web_tbl_selfcare_order_item> resOrderItems = new List<web_tbl_selfcare_order_item>();
            var orderitems = UOW.selfcare_order_item_repo.Get(filter: o => o.order_id == orderId).ToList();
            if (orderitems.Count > 0)
            {
                resOrderItems = orderitems;
            }
            return resOrderItems;
        }

        public List<DokuCareModel> GetSIDokuOrderTransaction()
        {
            List<DokuCareModel> resTrans = new List<DokuCareModel>();

            var transactions = (from so in _db.selfcare_order
                                join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                                join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                                where dk.site_id == 4
                                select new DokuCareModel
                                {
                                    order = so,
                                    orderpayment = sop,
                                    doku = dk
                                }).OrderByDescending(s => s.order.order_id).ToList();

            if (transactions.Count > 0)
            {
                double dBSP = 0;
                double dDep = 0;
                double dAmt = 0;
                foreach (var item in transactions)
                {
                    DokuCareModel dkuCare = new DokuCareModel();
                    dkuCare.order = item.order;
                    dkuCare.orderitems = GetOrderItems(item.order.order_id);
                    dkuCare.orderpayment = item.orderpayment;
                    dkuCare.doku = item.doku;
                    dAmt = Convert.ToDouble(item.order.order_product_total);
                    dBSP = CalcBSPCommission(dAmt);
                    dDep = dAmt - dBSP;
                    dkuCare.amt = dAmt;
                    dkuCare.bsp_amt = dBSP;
                    dkuCare.dep_amt = dDep;
                    resTrans.Add(dkuCare);
                }

            }
            // resTrans = resTrans.OrderByDescending(r => r.order.order_id).ToList();
            return resTrans;
        }

        public DokuCareModel GetDokuOrderSI(long OrderId)
        {
            DokuCareModel Order = new DokuCareModel();
            var Orders = (from so in _db.selfcare_order
                          join sop in _db.selfcare_order_payment on so.order_id equals sop.order_id
                          join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                          where dk.order_id == OrderId && dk.site_id == 4
                          select new DokuCareModel
                          {
                              order = so,
                              orderpayment = sop,
                              doku = dk
                          }).FirstOrDefault();
            if (Orders != null)
            {
                Order.order = Orders.order;
                Order.orderpayment = Orders.orderpayment;
                Order.doku = Orders.doku;
                Order.orderitems = GetOrderItems(Orders.order.order_id);
            }
            return Order;

        }

        #endregion

        #region For Shopping Doku Orders
        public List<DokuShopModel> GetDokuShopOrderTransaction()
        {
            List<DokuShopModel> resTrans = new List<DokuShopModel>();
            var transactions = (from so in _db.shopping_order
                                join sop in _db.shopping_order_payment on so.order_id equals sop.order_id
                                join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                                where dk.site_id == 2
                                select new DokuShopModel
                                {
                                    sorder = so,
                                    sorderpayment = sop,
                                    sdoku = dk
                                }).OrderByDescending(s => s.sorder.order_id).ToList();

            if (transactions.Count > 0)
            {
                foreach (var item in transactions)
                {
                    DokuShopModel dkuShop = new DokuShopModel();
                    dkuShop.sorder = item.sorder;
                    dkuShop.sorderitems = GetShopOrderItems(item.sorder.order_id);
                    dkuShop.sorderpayment = item.sorderpayment;
                    dkuShop.sdoku = item.sdoku;
                    resTrans.Add(dkuShop);
                }
            }
            return resTrans;
        }

        public DokuShopModel GetShopDokuOrder(long OrderId)
        {
            DokuShopModel Order = new DokuShopModel();
            var Orders = (from so in _db.shopping_order
                     join sop in _db.shopping_order_payment on so.order_id equals sop.order_id
                     join dk in _db.doku_selfccare on so.order_id equals dk.order_id
                     //join su in _db.shopping_user_contact on so.user_id equals su.user_id
                     where dk.order_id == OrderId && dk.site_id == 2
                     select new DokuShopModel
                     {
                         sorder = so,
                         sorderpayment = sop,
                         sdoku = dk,
                         //suser=su
                     }).FirstOrDefault();
            if (Orders != null)
            {
                Order.sdoku = Orders.sdoku;
                Order.sorder = Orders.sorder;
                Order.sorderitems = GetShopOrderItems(Orders.sorder.order_id);
                Order.sorderpayment = Orders.sorderpayment;
                //Order.suser = Orders.suser;
            }
            return Order;

        }


     

        private List<web_tbl_shopping_order_item> GetShopOrderItems(long orderId)
        {
            List<web_tbl_shopping_order_item> resOrderItems = new List<web_tbl_shopping_order_item>();
            var orderitems = UOW.shopping_order_item_repo.Get(filter: o => o.order_id == orderId).ToList();
            if (orderitems.Count > 0)
            {
                resOrderItems = orderitems;
            }
            return resOrderItems;
        }
        #endregion

        #region For Recharge APIs

        /// <summary>
        /// Validate Merchant using the config details
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <returns>json format string</returns>
        public string validate_merchant(string msisdn,string amount)
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

        #region For Recharge APIs for bmobile Topup

        /// <summary>
        /// Validate Merchant using the config details
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <returns>json format string</returns>
        public string bmpng_validate_merchant(string msisdn, string amount)
        {
            string sRes = "";
            RechargeModel obj_regcharge = new RechargeModel();
            obj_regcharge.keycode = png_keycode;
            obj_regcharge.username = png_uname;
            obj_regcharge.password = png_pwd;
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
        public string bmpng_recharge_msisdn(string msisdn, string amount, string data)
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

        #region BYP Order
        /// <summary>
        /// Save byp selfcare order details
        /// </summary>
        /// <param name="oTP"></param>
        /// <returns></returns>
        public long SaveBYPOrder(Payment_EntModel oTP)
        {
            long retOrderId = 0;
            web_tbl_selfcare_order oSO = new web_tbl_selfcare_order();
            oSO.order_id = 0;
            oSO.order_number = Convert.ToInt64(_util_repo.GetRandomNumber(8));
            oSO.order_product_total = Convert.ToDecimal(oTP.paid_amount);
            oSO.order_freight_total = 0;
            oSO.order_surcharge = 0;
            oSO.order_datetime = DateTime.Now;
            oSO.user_id = oTP.paid_userid;
            oSO.cust_fname = oTP.fname;
            oSO.cust_lname = oTP.lname;
            oSO.cust_email = oTP.email;
            oSO.cust_mobile_number = oTP.primary_msisdn;
            oSO.payment_mode = oTP.payment_mode;
            oSO.purchase_msisdn = oTP.paid_for_msisdn;
            UOW.selfcare_order_repo.Insert(oSO);
            UOW.Save();
            if (oSO.order_id > 0)
            {
                long ret_orderitemId = 0;
                foreach (var item in oTP.order_items)
                {
                    web_tbl_selfcare_order_item oSI = new web_tbl_selfcare_order_item();
                    oSI.order_id = oSO.order_id;
                    oSI.order_item_id = 0;
                    oSI.product_id = Convert.ToInt64(item.product_id);
                    oSI.product_name = item.product_name;
                    oSI.product_price = Convert.ToDecimal(item.product_price);
                    oSI.product_qty = Convert.ToInt32(item.product_qty);
                    oSI.product_shipping_matrix_id = 0;
                    oSI.purchase_desc = item.purchase_desc;
                    UOW.selfcare_order_item_repo.Insert(oSI);
                    UOW.Save();
                    ret_orderitemId = oSI.order_item_id;
                }
                if (ret_orderitemId > 0)
                {
                    web_tbl_selfcare_payment oSP = new web_tbl_selfcare_payment();
                    oSP.order_id = oSO.order_id;
                    oSP.payment_datetime = DateTime.Now;
                    oSP.payment_gateway = oTP.payment_gateway;
                    oSP.payment_id = 0;
                    oSP.payment_receipt_no = oTP.receiptno;
                    oSP.payment_response = "";
                    oSP.payment_status = "PENDING";
                    oSP.payment_total = Convert.ToDecimal(oTP.paid_amount);
                    oSP.payment_transaction_number = oSO.order_number.ToString();
                    oSP.payment_type = oTP.payment_type;
                    UOW.selfcare_order_payment_repo.Insert(oSP);
                    UOW.Save();
                    retOrderId = oSP.order_id;
                }
            }
            return retOrderId;
        }

        #endregion

        public string RedeemFBPromotion_GetVoucher(RedeemFBPromotionsModel RedeemFB)
        {
            string _iRes = string.Empty;

            FBPromotionInputModel model = new FBPromotionInputModel();
            model.username = "bmselfcare";
            model.password = "bmselfcare";
            model.keycode = "45678";
            model.serialNo = RedeemFB.serial_number;
            model.amount = "0";

            var data = JsonConvert.SerializeObject(model, Formatting.Indented);

            _iRes = _easyRecharge.getVoucher("111", data);


            return _iRes;
        }


        #region SMS API


        public string SendSMS(string sMSISDN, string sMessage)
        {
            string sRes = "";
            try
            {
                string sMerchantId = gp_merchantid;
                SMSModel sms = new SMSModel();
                sms.username = gp_username;
                sms.password = gp_password;
                sms.keycode = gp_keycode;
                sms.msisdn = sMSISDN;
                sms.message = sMessage;
                string data = JsonConvert.SerializeObject(sms);
                sRes = _easyRecharge.sendSMS(sMerchantId,_util_repo.AES_ENCWR(data));
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            sRes = _util_repo.AES_DECWR(sRes);
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
                    _easyRecharge.Dispose();
                    UOW.Dispose();
                    
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