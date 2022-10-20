using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using si_bmobile.Models;
using System.Configuration;
using si_bmobile.Utils;
using Newtonsoft.Json;

namespace si_bmobile.DAL
{
    public class UserRepository : IUserRepository, IDisposable
    {
        bmtestshopDBEntities _db;
        private IUtilityRepository _utils_repo;
        UnitOfWork _uow;
        int topup_id;
        int mobiletopup_id;

        public UserRepository()
        {

            this._utils_repo = new UtilityRepository();
            this._uow = new UnitOfWork();
            this._db = new bmtestshopDBEntities();
            this.topup_id = Convert.ToInt32(ConfigurationManager.AppSettings["topup_menuid"]);
            this.mobiletopup_id = Convert.ToInt32(ConfigurationManager.AppSettings["mobiletopup_menuid"]);
        }

        #region new

        public IEnumerable<all_user_details> get_all_users()
        {
            var users = (from u in _db.shopping_user
                         join uc in _db.shopping_user_contact on u.user_id equals uc.user_id
                         select new all_user_details
                         {
                             user = u,
                             user_contact = uc
                         }).ToList();

            return users;
        }

        public bool shop_user_email_activation(string Enc_Id)
        {
            bool res = false;
            var UserDetails = (from U in _db.shopping_user
                               join UC in _db.shopping_user_contact on U.user_id equals UC.user_id
                               where UC.enc_id == Enc_Id
                               select U).FirstOrDefault();
            if (UserDetails.is_approved == false)
            {
                UserDetails.is_approved = true;
                _uow.shopping_user_repo.Update(UserDetails);
                _uow.Save();
                res = true;
            }
            return res;
        }

        public web_tbl_shopping_user_contact shop_user_login(LoginModel logindetails)
        {
            var user_details = (from U in _db.shopping_user
                                join UC in _db.shopping_user_contact on U.user_id equals UC.user_id
                                where U.is_approved == true && UC.email.ToLower().Trim() == logindetails.Email && UC.pwd.ToLower().Trim() == logindetails.Pwd
                                select UC).FirstOrDefault();
            return user_details;
        }

        public bool is_email_available(string email_id)
        {
            long UserId = (from U in _db.shopping_user_contact where U.email == email_id select U.user_id).FirstOrDefault();
            if (UserId > 0)
                return false;
            else
                return true;
        }

        public bool is_msisdn_available(string msisdn)
        {
            long UserId = (from U in _db.shopping_user_contact where U.mobile_number.Trim() == msisdn.Trim() select U.user_id).FirstOrDefault();
            if (UserId > 0)
                return false;
            else
                return true;
        }

        public long change_password(ChangePwdModel details)
        {
            long userid = 0;
            details.OldPassword = _utils_repo.AES_ENC(details.OldPassword);
            details.NewPassword = _utils_repo.AES_ENC(details.NewPassword);

            var user = (from U in _db.shopping_user
                        join UC in _db.shopping_user_contact on U.user_id equals UC.user_id
                        where U.is_approved == true && U.user_id == details.UserId && UC.pwd == details.OldPassword
                        select UC).FirstOrDefault();

            if (user != null)
            {
                userid = user.user_id;
                user.pwd = details.NewPassword;
                _uow.shopping_user_contact_repo.Update(user);
                _uow.Save();
            }

            return userid;
        }

        public long reset_password(ResetModel details)
        {
            long userid = 0;
            details.OldPassword = _utils_repo.AES_ENC(details.OldPassword);
            details.NewPassword = _utils_repo.AES_ENC(details.NewPassword);
            var user = (from U in _db.shopping_user
                        join UC in _db.shopping_user_contact on U.user_id equals UC.user_id
                        where U.is_approved == true && UC.enc_id == details.Enc_Id && UC.pwd == details.OldPassword
                        select UC).FirstOrDefault();

            if (user != null)
            {
                userid = user.user_id;
                user.pwd = details.NewPassword;
                _uow.shopping_user_contact_repo.Update(user);
                _uow.Save();
            }

            return userid;
        }

        public long forgot_password(string email, out string pwd)
        {
            long userid = 0;
            pwd = string.Empty;
            var user = (from U in _db.shopping_user
                        join UC in _db.shopping_user_contact on U.user_id equals UC.user_id
                        where U.is_approved == true && UC.email.ToLower().Trim() == email
                        select UC).FirstOrDefault();
            if (user != null)
            {
                userid = user.user_id;

                Random r = new Random();
                string password = _utils_repo.GetRandomString(6);
                pwd = password;
                user.pwd = _utils_repo.AES_ENC(password);
                _uow.shopping_user_contact_repo.Update(user);
                _uow.Save();
                //SendMail(user);
            }
            return userid;
        }

        #endregion


        public IEnumerable<Menu_Model> get_all_menu()
        {
            List<Menu_Model> menu_list = new List<Menu_Model>();

            var menus = (from M in _db.menu
                         where M.is_deleted == false
                         select new Menu_Model
                         {
                             Menu_Id = M.id,
                             menu_name = M.menu_name,
                             parent_title = M.parent_title,
                             active = M.is_active
                         }).ToList();

            menu_list = menus;

            return menu_list;
        }


        public Care_User_Model GetUserById(long id)
        {
            Care_User_Model _obj_user = new Care_User_Model();

            var user = _db.care_users.Where(f => f.id == id).FirstOrDefault();
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
            }

            return _obj_user;
        }

        public List<Menu_Model> GetCareUserMenuList(long? U_id)
        {
            List<Menu_Model> objAccess = new List<Menu_Model>();
            objAccess = (from M in _db.menu
                         where M.is_active == true && M.is_deleted == false
                         select new Menu_Model
                         {
                             Menu_Id = M.id,
                             menu_name = M.menu_name,
                             parent_title = M.parent_title,
                             active = false
                         }).ToList();

            //if (objAccess.Count > 0)
            //{
            //    var bypmenu = objAccess.Where(f => f.parent_title == "BYP").Select(s => s.Menu_Id);
            //    objAccess.RemoveAll(r => bypmenu.Contains(r.Menu_Id));

            //    var simenu = objAccess.Where(f => f.parent_title == "Shopping Items").Select(s => s.Menu_Id);
            //    objAccess.RemoveAll(r => simenu.Contains(r.Menu_Id));

            //    objAccess.Add(new Menu_Model { Menu_Id = -1, menu_name = "BYP" });
            //    objAccess.Add(new Menu_Model { Menu_Id = -2, menu_name = "Shopping Items" });

            //}

            if (U_id != null)
            {
               // var user_access = _db.user_menu_conj.Where(U => U.user_id == U_id).ToList();

                var u_access = (from U in _db.care_users
                                join UM in _db.user_menu_conj on U.id equals UM.user_id
                                join M in _db.menu on UM.menu_id equals M.id
                                where UM.user_id == U_id && M.is_active == true && M.is_deleted == false
                                select new
                                {
                                    UM.menu_id,
                                    UM.user_id,
                                    M.menu_name,
                                    M.parent_title
                                }).ToList();

                foreach (var acc in objAccess)
                {
                    foreach (var useracc in u_access)
                    {
                        if (acc.Menu_Id == useracc.menu_id)
                            acc.active = true;
                        else if (acc.Menu_Id < 0)
                        {
                            if (acc.menu_name == useracc.parent_title)
                                acc.active = true;
                        }
                    }
                }
            }

            return objAccess;
        }

        public List<MainMenuModel> GetUserMenu(long U_id)
        {
            List<MainMenuModel> _obj = new List<MainMenuModel>();

            var sel_menu = (from M in _db.menu
                            join A in _db.user_menu_conj on M.id equals A.menu_id
                            join U in _db.care_users on A.user_id equals U.id
                            where U.id == U_id
                            select M).ToList();


            var normal_menu = sel_menu.Where(f => (string.IsNullOrEmpty(f.parent_title))).ToList();


            var title_menu = sel_menu.Where(f => (!string.IsNullOrEmpty(f.parent_title))).Select(s => s.parent_title).Distinct().ToList();

            foreach (var s_item in normal_menu)
            {
                MainMenuModel _Mobj = new MainMenuModel();

                if (string.IsNullOrEmpty(s_item.parent_title))
                {
                    _Mobj.title = s_item.menu_name;
                    _Mobj.menus = new List<menumodel>();
                    _Mobj.menus.Add(new menumodel { menu_id = s_item.id, menu_name = s_item.menu_name, page_name = s_item.page_name });
                }
                _obj.Add(_Mobj);
            }

            for (int i = 0; i < title_menu.Count; i++)
            {
                MainMenuModel tobj = new MainMenuModel();
                string title_name = title_menu[i].ToString().ToLower();
                var Parent_menus = sel_menu.Where(f => (!string.IsNullOrEmpty(f.parent_title))).ToList();
                var T_obj = Parent_menus.Where(f => f.parent_title.ToLower() == title_name).ToList();
                if (T_obj.Count > 0)
                {
                    tobj.menus = new List<menumodel>();
                    foreach (var item in T_obj)
                    {
                        tobj.title = title_menu[i].ToString();
                        tobj.menus.Add(new menumodel { menu_id = item.id, menu_name = item.menu_name, page_name = item.page_name });
                    }
                }
                _obj.Add(tobj);
            }
            return _obj;


        }

        public bool checkMenuAccess(long user_id, int role_id, string viewname)
        {
            bool ret_val = false;
            var _obj = (from U in _db.care_users
                        join R in _db.roles on U.role_id equals R.id
                        join A in _db.user_menu_conj on U.id equals A.user_id
                        join M in _db.menu on A.menu_id equals M.id
                        where U.id == user_id && R.id == role_id && U.is_deleted == false && U.is_active == true
                        select M).ToList();
            if (_obj.Count > 0)
            {
                foreach (var item in _obj)
                {
                    if (item.page_name == viewname)
                        ret_val = true;
                }
            }

            return ret_val;

        }


        //public List<Menu_Model> GetCareUserMenusList(long? U_id)
        //{
        //    var objAccess = (from M in _db.menu
        //                     where M.is_active == true && M.is_deleted == false
        //                     select new Menu_Model
        //                     {
        //                         Menu_Id = M.id,
        //                         menu_name = M.menu_name,
        //                         parent_title = M.parent_title,
        //                         active = false
        //                     }).ToList();
        //    List<Menu_Model> oMenu = new List<Menu_Model>();

        //    if (objAccess.Count > 0)
        //    {
        //        foreach (var item in objAccess)
        //        {
        //            if (item.parent_title != "BYP" && item.parent_title.Trim() != "ShoppingItems")
        //                oMenu.Add(item);
        //        }
        //        var bypmenu = objAccess.Where(f => f.parent_title.Trim() == "BYP").ToList();
        //        if (bypmenu.Count > 0)
        //        {
        //            Menu_Model bMenu = new Menu_Model();
        //            bMenu.Menu_Id = 0;
        //            bMenu.menu_name = "BYP";
        //            bMenu.parent_title = "BYP";
        //            foreach (var byp in bypmenu)
        //            {
        //                bMenu.menu_id = bMenu.menu_id + byp.Menu_Id.ToString()+"/";
        //            }
        //            bMenu.menu_id = bMenu.menu_id.Substring(0, bMenu.menu_id.Length - 1);

        //            oMenu.Add(bMenu);
        //        }

        //        var SImenu = objAccess.Where(f => f.parent_title.Trim() == "Shopping Items").ToList();
        //        if (SImenu.Count > 0)
        //        {
        //            Menu_Model sMenu = new Menu_Model();
        //            sMenu.Menu_Id = 0;
        //            sMenu.menu_name = "Shopping Items";
        //            sMenu.parent_title = "Shopping Items";
        //            foreach (var si in SImenu)
        //            {
        //                sMenu.menu_id = sMenu.menu_id + si.Menu_Id.ToString() + "/";
        //            }
        //            sMenu.menu_id = sMenu.menu_id.Substring(0, sMenu.menu_id.Length - 1);

        //            oMenu.Add(sMenu);
        //        }
        //    }

        //    if (U_id != null)
        //    {
        //        var user_access = _db.user_menu_conj.Where(U => U.user_id == U_id).ToList();
        //        foreach (var acc in oMenu)
        //        {
        //            foreach (var useracc in user_access)
        //            {
        //                if (acc.Menu_Id == useracc.menu_id)
        //                    acc.active = true;

        //            }
        //        }
        //    }

        //    return objAccess;
        //}

        public IList<TransactionReportModel> get_list()
        {
            IList<SISubscriptionModel> obj_list = new List<SISubscriptionModel>();
            var list = (from t in _db.plan_transaction
                        join p in _db.plan_details on t.Id equals p.transaction_id 
                        join w in _db.care_users on t.user_id equals w.id
                       
                        select new SISubscriptionModel
                        {
                              et=t,
                              pd=p,
                              c=w
  
                        }).ToList();

            IList<TransactionReportModel> obj_report = new List<TransactionReportModel>();
            var res = (from m in list
                       select new 
                       {
                           user_name = m.c.user_name,
                           trans_number = m.et.trans_number,
                           msisdn = m.et.msisdn,
                           total_price = m.et.total_price,
                           trans_initiated = m.et.created_on,
                           trans_purchase = m.et.plan_purchased_on,
                           transaction_id = m.pd.transaction_id,
                           trans_status=m.et.trans_status 
                       }
                       ).Select(d => new {d.user_name,d.trans_number,d.msisdn,d.total_price,d.trans_initiated,d.trans_purchase,d.transaction_id,d.trans_status}).Distinct().ToList();

            foreach(var item in res)
            {
                TransactionReportModel obj = new TransactionReportModel();
                obj.msisdn = item.msisdn;
                obj.user_name = item.user_name;
                obj.trans_number = item.trans_number;
                obj.trans_initiated = item.trans_initiated;
                obj.trans_purchase = item.trans_purchase;
                obj.total_price = item.total_price;
                obj.transaction_id = item.transaction_id;
                obj.trans_status = item.trans_status;

                string splandetails = "";
                var plandetails=list.Where(p=>p.pd.transaction_id==obj.transaction_id).ToList();
                if (plandetails.Count > 0)
                {
                    foreach (var plan in plandetails)
                    {
                        splandetails += plan.pd.plan_name + "-$ " + plan.pd.plan_price + ";";

                    }
                    obj.plans = splandetails;
                    obj_report.Add(obj);
                }
            }
            return obj_report;



            //string op = JsonConvert.SerializeObject(res);



            //obj_report = res;
            
           // obj_list = list;
           // return obj_list;
        }

        #region user authenticated menulist

        public string get_MenuListByUserId(long id, int role)
        {
            string rtn = string.Empty;

            List<int> obj_m = new List<int>();
            if (role != 1)
            {
                var sel_menu = (from M in _db.menu
                                join A in _db.user_menu_conj on M.id equals A.menu_id
                                join U in _db.care_users on A.user_id equals U.id
                                where U.id == id
                                select M).Select(x => x.id).ToList();
                obj_m = sel_menu;
            }
            else
            {

                obj_m.Add(topup_id);
                obj_m.Add(mobiletopup_id);

            }

            rtn = JsonConvert.SerializeObject(obj_m);

            return rtn;
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
                    _db.Dispose();
                    _uow.Dispose();
                    _utils_repo.Dispose();
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