using si_bmobile.DAL;
using si_bmobile.Models;
using si_bmobile.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace si_bmobile.Controllers
{
    public class HomeController : Controller
    {

        private IUtilityRepository _util_repo;
        private IShopRepository _shop_repo;
        private ISimRepository _sim_repo;
        UnitOfWork _uow;
        public HomeController()
        {
            this._util_repo = new UtilityRepository();
            this._shop_repo = new ShopRepository();
            this._sim_repo = new SimRepository();
            this._uow = new UnitOfWork();
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return RedirectToAction("Login", "Care");
        }

        [HttpGet]
        public ActionResult SimRegistration()
        {
            try
            {
                sim_reg_model Item = new sim_reg_model();
                Item.cust_iden.is_drv_lic = false;
                Item.cust_iden.is_student_id = false;
                Item.cust_iden.is_emp_id = false;
                Item.cust_iden.is_nasfund_id = false;
                Item.cust_iden.is_nbw_super_id = false;
                Item.cust_iden.is_other_id = false;
                return View(Item);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult SimRegistration(sim_reg_model Item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Cust_SimRegModel RegDetails = new Cust_SimRegModel();
                    //RegDetails.cust_ack = Item.cust_ack;
                    //RegDetails.cust_iden = Item.cust_iden;
                    //RegDetails.cust_purchase = Item.cust_purchase;
                    //RegDetails.customer = new Cust_RegModel();

                    //RegDetails.customer.address = Item.address;
                    //RegDetails.customer.cust_id = Item.cust_id;
                    //RegDetails.customer.email = Item.email;
                    //RegDetails.customer.is_employed = Item.is_employed;
                    //RegDetails.customer.name = Item.name;
                    //RegDetails.customer.sex = Item.sex;
                    //long reg_cust_id = _shop_repo.Save_SimRegCustomer(RegDetails);
                    long reg_cust_id = _sim_repo.save_sim_reg_cust(Item);
                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpGet]
        public ActionResult SimReplaceRegistration()
        {
            try
            {
                replace_sim_reg_model Item = new replace_sim_reg_model();
                Item.cust_iden.is_drv_lic = false;
                Item.cust_iden.is_student_id = false;
                Item.cust_iden.is_emp_id = false;
                Item.cust_iden.is_nasfund_id = false;
                Item.cust_iden.is_nbw_super_id = false;
                Item.cust_iden.is_other_id = false;
                return View(Item);
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }

        [HttpPost]
        public ActionResult SimReplaceRegistration(replace_sim_reg_model Item)
        {
            try
            {
                //Cust_SimRegModel RegDetails = new Cust_SimRegModel();

                //RegDetails.cust_ack = Item.cust_ack;
                //RegDetails.cust_iden = Item.cust_iden;
                //RegDetails.cust_mob = Item.cust_mob;
                //RegDetails.customer = new Cust_RegModel();

                //RegDetails.customer.name = Item.name;
                //RegDetails.customer.mobile_no = Item.mobile_no;
                //RegDetails.customer.reg_status_id = 2;
                //RegDetails.customer.created_on = DateTime.Now;
                //RegDetails.customer.is_active = false;
                //RegDetails.customer.is_deleted = false;
                //RegDetails.customer.modified_on = DateTime.Now;
                //long reg_cust_id = _shop_repo.Save_SimRegCustomer(RegDetails);

                long reg_cust_id = _sim_repo.save_sim_replace_reg_cust(Item);


            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return View();
        }



    }
}
