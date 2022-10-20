using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using si_bmobile.DAL;
using System.Configuration;
using si_bmobile.Models;
using System.Text;
using si_bmobile.Utils;
using System.IO;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using System.Web.Routing;
using Newtonsoft.Json;
using MvcPaging;
using si_bmobile.dokuRef;


namespace si_bmobile.Controllers
{

    public class ShopController : Controller
    {
        private IShopRepository _shopping_repo;
        private IUserRepository _user_repo;
        private IUtilityRepository _utils_repo;
        public string sEncKey = ConfigurationManager.AppSettings["encKey"].ToString();
        string error_log_path = ConfigurationManager.AppSettings["error_log_path"].ToString();
        private int defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pgSize"]);
        private IcareRepository _care_repo;
       // private IdokuRepository _doku_repo;
        private IdokuServiceClient _doku_client;
        UnitOfWork _uow;
        public ShopController()
        {
           // this._doku_repo = new dokuRepository();
            this._uow = new UnitOfWork();
            this._shopping_repo = new ShopRepository();
            this._user_repo = new UserRepository();
            this._utils_repo = new UtilityRepository();
            this._care_repo = new careRepository();
            this._doku_client = new IdokuServiceClient();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Products", "Shop");
        }

        #region Login

        public ActionResult login()
        {
            try
            {
                ViewBag.StepsView = Session["shopcart"] == null ? "display: none;" : "display: block;";
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult login(LoginModel logindetails)
        {
            try
            {
                ViewBag.StepsView = Session["shopcart"] == null ? "display: none;" : "display: block;";

                //logindetails.Pwd = _utils_repo.AES_DEC("W4wmKe7yhNcwlYhafHxLXw==");
                logindetails.Pwd = _utils_repo.AES_ENC(logindetails.Pwd);

                web_tbl_shopping_user_contact user = _user_repo.shop_user_login(logindetails);
                if (user != null && user.user_id > 0)
                {
                    Session["ShoppingUserID"] = user.user_id;
                    string name = user.first_name + " " + user.last_name;
                    Session["UName"] = name;
                    FormsAuthentication.SetAuthCookie(name, true);

                    if (Session["shopcart"] != null)
                        return RedirectToAction("Confirm");
                    else
                        return RedirectToAction("Products");
                }
                else
                {
                    ViewBag.Message = "Invalid Username/Password!";
                    return View(logindetails);
                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            return View(logindetails);
        }

        #endregion

        #region LogOut

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
        #endregion

        #region Register

        public ActionResult Register()
        {
            try
            {
                ViewBag.StepsView = Session["shopcart"] == null ? "display: none;" : "display: block;";
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Register(web_tbl_shopping_user_contact user_details)
        {
            try
            {
                ViewBag.StepsView = Session["shopcart"] == null ? "display: none;" : "display: block;";

                web_tbl_shopping_user obj_user = new web_tbl_shopping_user();

                obj_user.user_allows_contact = true;
                _uow.shopping_user_repo.Insert(obj_user);
                _uow.Save();

                if (obj_user != null && obj_user.user_id > 0)
                {
                    user_details.user_id = obj_user.user_id;
                    user_details.IsShopping = true;
                    user_details.pwd = _utils_repo.AES_ENC(user_details.pwd);

                    _uow.shopping_user_contact_repo.Insert(user_details);
                    _uow.Save();

                    if (user_details != null && user_details.contact_id > 0)
                    {
                        string new_user_id = Convert.ToString(user_details.user_id);
                        user_details.enc_id = _utils_repo.AES_ENC(new_user_id);
                        _uow.shopping_user_contact_repo.Update(user_details);
                        _uow.Save();
                        if (user_details != null && user_details.contact_id > 0)
                        {
                            XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                            XElement emailsubj = doc.Element("SendConfirmationMail_Subj");
                            XElement emailBody = doc.Element("SendConfirmationMail_body");
                            string sData = emailBody.Value;
                            sData = sData.Replace("#Name#", user_details.first_name + " " + user_details.last_name).Replace("#LoginID#", user_details.email);
                            string hostURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                            sData = sData.Replace("#ActivationURL#", hostURL + "/Shop/AccountActivation/" + user_details.enc_id);
                            _utils_repo.SendEmailMessage(user_details.email, emailsubj.Value.Trim(), sData);

                            //ViewBag.Message = "Email id already exist";
                            return RedirectToAction("Thankyou", "Shop", new { Message = "Register" });
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Unable to register!";
                        return View(user_details);
                    }
                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }


        #endregion

        #region my account

        [HttpGet]
        public ActionResult UserProfile()
        {
            try
            {
                if (Session["ShoppingUserID"] == null)
                    return RedirectToAction("Login");

                long userid = Convert.ToInt64(Session["ShoppingUserID"]);
                var userdetails = _user_repo.get_all_users().Where(m => m.user_contact.user_id == userid).FirstOrDefault();
                if (userdetails != null)
                    return View(userdetails);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult UserProfile(all_user_details userdetails)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");
            try
            {
                if (userdetails != null)
                {
                    //Userdetails.pwd = _utils_repo.AES_ENC(Userdetails.pwd);
                    userdetails.user_contact.IsShopping = true;
                    _uow.shopping_user_contact_repo.Update(userdetails.user_contact);
                    _uow.Save();

                    if (userdetails.user_contact.user_id > 0)
                        return RedirectToAction("Thankyou", "Shop", new { Message = "Update" });
                }


            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }

            return View(userdetails);
        }

        #endregion

        #region ChangePassword

        public ActionResult ChangePwd()
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public ActionResult ChangePwd(ChangePwdModel obj_pwd)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");
            try
            {
                if (ModelState.IsValid)
                {
                    obj_pwd.UserId = long.Parse(Session["ShoppingUserID"].ToString());
                    long user_id = _user_repo.change_password(obj_pwd);

                    if (user_id == 0)
                        ViewBag.Message = "Invalid Password!";
                    else
                        ViewBag.Message = "Your Password has changed successfully";
                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(obj_pwd);
        }

        #endregion

        #region Thankyou

        public ActionResult Thankyou(string Message)
        {
            try
            {
                ViewBag.Message = Message;
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        #region IsEmailAvailable

        public JsonResult IsEmailAvailable(string email)
        {
            bool IsValid = false;
            try
            {
                IsValid = _user_repo.is_email_available(email);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return Json(IsValid, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region AccountActivation

        public ActionResult AccountActivation(string id)
        {
            try
            {
                bool Res = _user_repo.shop_user_email_activation(id);

                if (Res)
                    return RedirectToAction("Thankyou", "Shop", new { Message = "AccountConfirmation" });
                else
                    return RedirectToAction("Thankyou", "Shop", new { Message = "ActivationFailed" });
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        #region ResetPassword

        public ActionResult ResetPassword(string id)
        {
            try
            {
                string enc_id = HttpUtility.UrlDecode(id.Trim());
                ResetModel user = new ResetModel();
                user.Enc_Id = enc_id;
                return View(user);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetModel details)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    long userid = _user_repo.reset_password(details);

                    if (userid > 0)
                        return RedirectToAction("Thankyou", "Shop", new { Message = "ResetPassword" });
                    else
                        ViewBag.Message = "Invalid user details or password!";

                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(details);
        }

        #endregion

        #region ForgotPassword

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            try
            {
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotModel user)
        {
            try
            {
                string pwd = string.Empty;
                long userid = _user_repo.forgot_password(user.Email, out pwd);

                if (userid > 0)
                {
                    var UserInfo = _user_repo.get_all_users().Where(m => m.user_contact.user_id == userid).FirstOrDefault();

                    string EnEnc_Id = UserInfo.user_contact.enc_id;

                    XElement doc = XElement.Load(Server.MapPath("~/EmailTemplate/email_template.xml"));
                    XElement emailsubj = doc.Element("resetpwd_subj");
                    XElement emailBody = doc.Element("resetpwd_body");
                    string sData = emailBody.Value;
                    sData = sData.Replace("#Name#", UserInfo.user_contact.first_name + " " + UserInfo.user_contact.last_name).Replace("#LoginID#", UserInfo.user_contact.email).Replace("#Password#", pwd);
                    string hostURL = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "");
                    sData = sData.Replace("#ActivationURL#", hostURL + "/Shop/ResetPassword/" + EnEnc_Id);
                    _utils_repo.SendEmailMessage(UserInfo.user_contact.email, emailsubj.Value.Trim(), sData);

                    return RedirectToAction("Thankyou", "Shop", new { Message = "ForgotPassword" });
                }
                else
                    ViewBag.Message = "Email id not exist!";

            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(user);
        }

        #endregion

        #region MyOrders

        public ActionResult MyOrders(int? page, string sOrderNo, string sEmail, string sCity, string sFromOrder, string sToOrder)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");

            string sMsg = "";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            IList<all_orders_model> obj_orders = new List<all_orders_model>();
            try
            {
                long User_Id = long.Parse(Session["ShoppingUserID"].ToString());

                ViewData["sOrderNo"] = sOrderNo;
                ViewData["sEmail"] = sEmail;
                ViewData["sCity"] = sCity;
                ViewData["sFromOrder"] = sFromOrder;
                ViewData["sToOrder"] = sToOrder;

                obj_orders = _shopping_repo.get_my_shopping_order(User_Id).OrderByDescending(O => O.order.order_id).ToList();
                if (obj_orders.Count > 0)
                {
                    if (!string.IsNullOrEmpty(sOrderNo))
                        obj_orders = obj_orders.Where(b => b.order.order_number == sOrderNo).ToList();

                    if (!string.IsNullOrEmpty(sEmail))
                        obj_orders = obj_orders.Where(b => b.ordered_user.email.Trim().ToLower().Contains(sEmail.Trim().ToLower())).ToList();

                    if (!string.IsNullOrEmpty(sCity))
                        obj_orders = obj_orders.Where(b => b.ordered_user.city.Trim().ToLower().Contains(sCity.Trim().ToLower())).ToList();

                    if (!string.IsNullOrEmpty(sFromOrder))
                    {
                        DateTime FromOrder = Convert.ToDateTime(sFromOrder);
                        obj_orders = obj_orders.Where(b => b.order.order_datetime.Date >= FromOrder.Date).ToList();
                    }
                    if (!string.IsNullOrEmpty(sToOrder))
                    {
                        DateTime ToOrder = Convert.ToDateTime(sToOrder);

                        obj_orders = obj_orders.Where(b => b.order.order_datetime.Date <= ToOrder.Date).ToList();
                    }

                    obj_orders = obj_orders.OrderByDescending(m => m.order.order_id).ToPagedList(currentPageIndex, defaultPageSize);

                    if (obj_orders.Count == 0)
                        sMsg = "Not found!";

                    if (Request.IsAjaxRequest())
                        return PartialView("_Ajax_MyOrders", obj_orders);
                    else
                        return View(obj_orders);

                }

                ViewBag.Message = sMsg;
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        #region MyOrderSummary

        public ActionResult MyOrderSummary(long id)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");

            try
            {
                long User_Id = long.Parse(Session["ShoppingUserID"].ToString());

                var my_order = _shopping_repo.get_order_summary(id, User_Id);

                Session["shopcart"] = null;

                if (User_Id != my_order.order.user_id)
                    return RedirectToAction("MyOrders");

                return View(my_order);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        #region _CartSummaryPartial

        public ActionResult _CartSummaryPartial()
        {
            try
            {
                List<cartpanel_model> OIList = new List<cartpanel_model>();
                if (Session["shopcart"] != null)
                    OIList = (List<cartpanel_model>)Session["shopcart"];

                int CartCount = OIList.Count();

                ViewBag.CartCount = CartCount;

                return PartialView();
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
                return View();
            }
        }

        #endregion

        #region Products

        public ActionResult Products()
        {
            ShopProductsModel SPModel = new ShopProductsModel();
            try
            {
                SPModel.CategoryList = _shopping_repo.get_category();
                SPModel.BrandList = _shopping_repo.get_product_brand();
                SPModel.PriceList = _shopping_repo.get_product_price();
                SPModel.Products = _shopping_repo.get_products().Where(f => f.product.IsFeatured == true).ToList();

            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(SPModel);

        }

        public ActionResult _Ajax_Product(long? cid, long? bid, int? prid, string skey)
        {
            List<all_products_model> obj_products = new List<all_products_model>();
            try
            {
                long Category_Id = cid == null ? 0 : (long)cid;
                long Brand_Id = bid == null ? 0 : (long)bid;
                int PriceRange_Id = prid == null ? 0 : (int)prid;

                obj_products = _shopping_repo.get_search_products(Category_Id, Brand_Id, PriceRange_Id, skey);



            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return PartialView(obj_products);

        }

        #endregion

        #region ProductInfoPage

        public ActionResult ProductInfo(long Id)
        {
            try
            {
                if (Session["ShoppingUserID"] != null)
                    ViewBag.acrt_pinfo = "display:block;";
                else
                    ViewBag.acrt_pinfo = "display:none;";

                var products = _shopping_repo.get_product_byId(Id);


                return View(products);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);

            }
            return View();
        }
        #endregion

        #region ProductInfoPopup

        public ActionResult ProductInfoPartial(long Id)
        {
            try
            {
                var products = _shopping_repo.get_product_byId(Id);

                return PartialView("_ProductInfoPartial", products);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return PartialView("_ProductInfoPartial");
        }
        #endregion

        #region Confirm

        public ActionResult CancelOrder()
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");

            try
            {
                if (Session["shopcart"] != null)
                    Session.Remove("shopcart");
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return RedirectToAction("Products");
        }

        private List<SelectListItem> gettopuplist()
        {
            string[] TopupItems = null;

            var topups = ConfigurationManager.AppSettings["Shop_SIMBuy"].ToString();
            TopupItems = topups.Split(',');

            List<SelectListItem> obj_topup = new List<SelectListItem>();

            foreach (var item in TopupItems)
            {
                obj_topup.Add(new SelectListItem { Value = item, Text = "$ " + item });
            }
            return obj_topup;
        }

        public JsonResult CheckTerms(bool Terms)
        {
            bool bRet = false;

            if (Terms == true)
                bRet = true;


            return Json(bRet, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Confirm()
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");
            ConfirmModel confirmmodel = new ConfirmModel();
            try
            {
                if (Session["shopcart"] == null)
                    return RedirectToAction("Products");

                List<web_tbl_payment_mode> pModes = new List<web_tbl_payment_mode>();
                pModes = _uow.payment_mode_repo.Get(filter: p => p.IsActive == true).ToList();

                List<cartpanel_model> cart_items = (List<cartpanel_model>)Session["shopcart"];

                if (pModes.Count > 0)
                    confirmmodel.PaymentModes = pModes;

                if (cart_items.Count > 0)
                {
                    confirmmodel.Order_Items = cart_items;
                    return View(confirmmodel);
                }
                else
                    return RedirectToAction("Products");

            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View(confirmmodel);
        }

        [HttpPost]
        public ActionResult Confirm(ConfirmModel Obj, string cmd)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");
            try
            {
                if (Session["shopcart"] == null)
                    return RedirectToAction("Products");

                List<cartpanel_model> new_cart_items = new List<cartpanel_model>();

                List<cartpanel_model> final_cart_items = new List<cartpanel_model>();

                List<cartpanel_model> cart_items = (List<cartpanel_model>)Session["shopcart"];

                decimal total_amount = 0;
                int pro_count = 0;
                int sucess_count = 0;

                if (cart_items.Count > 0)
                {
                    foreach (cartpanel_model items in cart_items)
                    {
                        pro_count++;

                        items.sub_product_price = (items.product_price * items.product_qty);

                        total_amount += items.sub_product_price;

                        if (!string.IsNullOrEmpty(Request.Form["topupAmt" + items.product_id].ToString()) && items.is_topup == true)
                        {
                            items.topup_amt = Convert.ToDecimal(Request.Form["topupAmt" + items.product_id].ToString());

                            items.sub_topup_price = (Convert.ToDecimal(items.topup_amt) * items.sim_qty);

                            total_amount += items.sub_topup_price;

                            List<SelectListItem> selecteditem_topup = new List<SelectListItem>();

                            #region selected topup amt

                            if (items.TopupAmtlist.Count > 0 && items.topup_amt > 0)
                            {
                                foreach (SelectListItem topup_item in items.TopupAmtlist)
                                {
                                    if (topup_item.Value == items.topup_amt.ToString())
                                        topup_item.Selected = true;
                                    else
                                        topup_item.Selected = false;

                                    selecteditem_topup.Add(topup_item);
                                }
                            }

                            #endregion

                            items.TopupAmtlist = selecteditem_topup;
                            new_cart_items.Add(items);
                            sucess_count++;
                        }
                        else
                        {
                            new_cart_items.Add(items);
                            sucess_count++;
                        }

                    }

                    if (new_cart_items.Count > 0)
                    {

                        Obj.Order_Items = new_cart_items;
                        Session["shopcart"] = new_cart_items;
                    }

                }


                if (cmd.ToLower() == "continue shopping")
                {
                    return RedirectToAction("Products");
                }
                else if (cmd.ToLower() == "pay now")
                {
                    #region Pay now

                    if (ModelState.IsValid)
                    {
                        if (Obj.Terms == true)
                        {
                            int PaymentMode = Obj.PayModeId;

                            if (sucess_count != pro_count)
                            {
                                ViewBag.FailMessage = "Please select sim topup charges for required product";
                                return View(Obj);
                            }

                            long UserID = Convert.ToInt64(Session["ShoppingUserID"].ToString());

                            if (Obj.PayModeId == 2)
                            {
                                #region Pay by credit card

                                long order_id = 0;
                                long payment_order_id = 0;

                                //Session["seDKUOrderid"] = orderid.ToString();
                                PaymentEntModel pay = new PaymentEntModel();

                                
                                //pay.fname = oTP.pay.fname;
                                //pay.lname = oTP.pay.lname;
                                //pay.paid_for_msisdn = sResMSISDN;
                                //pay.primary_msisdn = oTP.pay.primary_msisdn;

                                pay.payment_mode = PaymentMode;
                                pay.paid_userid = UserID;
                                pay.sess_id = Session.SessionID;
                                pay.paid_amount = total_amount.ToString("#0.00");

                                pay.payment_gateway = "DOKU";
                                pay.payment_type = "CREDITCARD";

                                web_tbl_shopping_user_contact user_details = new web_tbl_shopping_user_contact();

                                user_details = _uow.shopping_user_contact_repo.Get(filter: m => m.contact_id == UserID).FirstOrDefault();

                                if (user_details != null)
                                    pay.user_details = JsonConvert.SerializeObject(user_details);
                                pay.email = user_details.email;
                                pay.fname = user_details.first_name;
                                pay.lname = user_details.last_name;

                                List<tbl_doku_order_item> order_items = new List<tbl_doku_order_item>();

                                #region add cart items

                                if (new_cart_items.Count > 0)
                                {
                                    foreach (var item in new_cart_items)
                                    {
                                        tbl_doku_order_item o_item = new tbl_doku_order_item();

                                        o_item.product_id = item.product_id;
                                        o_item.product_name = item.product_name;
                                        o_item.purchase_desc = "SHOP";
                                        o_item.product_qty = item.product_qty;
                                        o_item.product_price = item.product_price;

                                        order_items.Add(o_item);

                                        if (item.is_topup == true)
                                        {
                                            tbl_doku_order_item topup_item = new tbl_doku_order_item();

                                            topup_item.product_id = -1;
                                            topup_item.product_name = "TOPUP FOR " + item.product_name;
                                            topup_item.purchase_desc = "SHOP";
                                            topup_item.product_qty = item.product_qty;
                                            topup_item.product_price = item.topup_amt;

                                            order_items.Add(topup_item);
                                        }

                                    }
                                }

                                #endregion

                                if (order_items.Count > 0)
                                    pay.order_items = order_items.ToArray();

                                order_id = _doku_client.save_selfcare_order(pay);

                                if (order_id > 0)
                                    payment_order_id = _doku_client.save_doku_order_payment(pay, order_id, 2);


                                temp_orders_model oT = new temp_orders_model();
                                oT.Id = 0;
                                oT.Order_Id = order_id;
                                oT.Session_Id = Session.SessionID;
                                oT.SiteId = 2;

                                string sJStemp = JsonConvert.SerializeObject(oT);
                                long TempOrderId = _doku_client.save_temp_order(sJStemp);

                                if (TempOrderId > 0)
                                {
                                    Session.Remove("shopcart");
                                    string dk_process_url = ConfigurationManager.AppSettings["dk_ProcessURL"].ToString();
                                    return Redirect(dk_process_url + "/" + oT.Session_Id);
                                }
                               

                                #endregion

                            }
                            else
                            {
                                long orderid = _shopping_repo.save_shopping_order(UserID, total_amount, new_cart_items, PaymentMode);

                                if (orderid > 0)
                                {
                                    web_tbl_shopping_order OModel = new web_tbl_shopping_order();

                                    OModel = _uow.shopping_order_repo.Get(filter: (m => m.order_id == orderid)).FirstOrDefault();

                                    General oG = new Utils.General();
                                    Random rn = new Random();
                                    string _payment_refno = oG.RandomString(rn, 8);

                                    if (Obj.PayModeId == 3)
                                    {
                                        #region Pay by Ezy
                                        //Session["seEztAmt"] =  total_amt.ToString();
                                        //Session["seEztRef"] = _payment_refno;
                                        Session["seEztOrederid"] = orderid.ToString();

                                        ViewBag.reURL = "/ShopEzy/Ezy";
                                        return View(Obj);
                                        //return RedirectToAction("ShopEzy", "Ezy", oEM);
                                        //return RedirectToAction("OrderSummary", new { id = orderid });
                                        #endregion

                                    }
                                    else if (Obj.PayModeId == 1)
                                    {
                                        #region Pay by branch

                                        // _utils_repo.SendEmailMessage(ordered_user.email, emailsubj.Value.Trim(), sData);
                                        return RedirectToAction("OrderSummary", new { id = orderid });

                                        #endregion

                                    }

                                }

                            }




                        }
                        else
                            ViewBag.TermsMsg = "Please accept terms and conditions";
                    }

                    #endregion
                }


            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);

            }
            return View(Obj);
        }

        #endregion

        #region OrderSummary

        public ActionResult OrderSummary(long id)
        {
            if (Session["ShoppingUserID"] == null)
                return RedirectToAction("Login");
            try
            {
                order_summary_model my_order = _shopping_repo.get_order_summary(id);

                if (my_order != null)
                    my_order.order_topup = _uow.order_topup_repo.Get(filter: t => t.order_id == my_order.order.order_id).ToList();

                long User_Id = long.Parse(Session["ShoppingUserID"].ToString());

                Session["shopcart"] = null;

                if (User_Id != my_order.order.user_id)
                    return RedirectToAction("Products");

                Session["shopcart"] = null;

                return View(my_order);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        #endregion

        #region CartPopup

        public ActionResult CartPanel(long? Id)
        {
            //if (Session["ShoppingUserID"] == null)
            //    return RedirectToAction("Login");
            try
            {
                List<cartpanel_model> OIList = new List<cartpanel_model>();
                bool flag = false;
                if (Session["shopcart"] != null)
                    OIList = (List<cartpanel_model>)Session["shopcart"];

                if (Session["ShoppingUserID"] == null && OIList.Count == 1)
                    Session.Remove("shopcart");

                if (OIList.Count > 0 && Id != null)
                {
                    foreach (var listitem in OIList)
                    {
                        if (listitem.product_id == Id)
                        {
                            listitem.product_qty = listitem.product_qty + 1;

                            listitem.sub_product_price = (listitem.product_price * listitem.product_qty);

                            if (listitem.is_topup == true)
                            {
                                listitem.sim_qty = listitem.sim_qty + 1;
                                if (listitem.topup_amt > 0)
                                    listitem.sub_topup_price = (listitem.topup_amt * listitem.sim_qty);

                            }

                            flag = true;
                        }
                    }
                }

                if (!flag && Id != null)
                {
                    long produc_id = Convert.ToInt64(Id);
                    var obj_products = _shopping_repo.get_product_byId(produc_id);
                    cartpanel_model cart_item = new cartpanel_model();
                    cart_item.product_id = obj_products.product.Product_ID;
                    cart_item.product_price = (decimal)obj_products.product.Product_Price;
                    cart_item.product_name = obj_products.product.Product_Name;
                    cart_item.product_qty = 1;
                    cart_item.product_sku = obj_products.product.SKU_Number;
                    cart_item.product_image = obj_products.product_img.img_small;
                    cart_item.product_model = obj_products.product.Model_No;
                    cart_item.is_topup = obj_products.product.is_topup;
                    cart_item.sub_product_price = cart_item.product_price;
                    if (cart_item.is_topup == true)
                    {
                        cart_item.sim_qty = 1;
                        cart_item.TopupAmtlist = gettopuplist();
                    }

                    OIList.Add(cart_item);
                }

                Session["shopcart"] = OIList;

                return PartialView("_ProductPartial", OIList);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        public ActionResult UpdateProduct_Qunatity(long PId, int qty)
        {
            try
            {
                List<cartpanel_model> OIList = new List<cartpanel_model>();
                cartpanel_model Ord_item = new cartpanel_model();
                decimal Sub_total_price = 0;
                if (Session["shopcart"] != null)
                    OIList = (List<cartpanel_model>)Session["shopcart"];
                if (qty == 0)
                    qty = 1;

                foreach (var listitem in OIList)
                {
                    if (listitem.product_id == PId)
                    {
                        listitem.product_qty = qty;

                        if (listitem.is_topup == true)
                            listitem.sim_qty = qty;
                        if (listitem.topup_amt > 0)
                            listitem.sub_topup_price = (listitem.topup_amt * listitem.sim_qty);

                        listitem.sub_product_price = listitem.product_price * listitem.product_qty;
                        Sub_total_price = listitem.product_price * listitem.product_qty;
                        //listitem.total_product_price = listitem.Sub_total_price;
                        Ord_item = listitem;
                    }
                }

                Session["shopcart"] = OIList;

                return Json(new { Status = true, price = Sub_total_price, qty = Ord_item.product_qty }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteProduct(long id)
        {
            try
            {
                List<cartpanel_model> OIList = new List<cartpanel_model>();

                if (Session["shopcart"] != null)
                    OIList = (List<cartpanel_model>)Session["shopcart"];

                var objProduct = OIList.Where(P => P.product_id == id).FirstOrDefault();
                if (objProduct != null)
                {
                    OIList.Remove(objProduct);
                    return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
                }

                Session["shopcart"] = OIList;

                return Json(new { Status = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion


        public ActionResult testZoom()
        {
            return View();
        }


        #region Dispose Repo
        /// <summary>
        /// Dispose Repository objects
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _shopping_repo.Dispose();
            _user_repo.Dispose();
            _utils_repo.Dispose();
            base.Dispose(disposing);
        }
        #endregion
    }

}

