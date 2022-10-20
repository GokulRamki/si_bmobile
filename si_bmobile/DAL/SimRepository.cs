using si_bmobile.Models;
using si_bmobile.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.DAL
{
    public class SimRepository: ISimRepository, IDisposable
    {
        bmtestshopDBEntities _db = new bmtestshopDBEntities();
        private IUtilityRepository _utils_repo;
        UnitOfWork _uow;
        public SimRepository()
        {
            this._utils_repo = new UtilityRepository();
            this._uow = new UnitOfWork();
        }

        public bool save_sim_contact_details(ActiveSimModel active_sim)
        {
            bool res = false;

            web_tbl_sim_details sim_det = new web_tbl_sim_details();
            sim_det.sim_number = active_sim.sim_number;
            sim_det.first_name = active_sim.first_name;
            sim_det.last_name = active_sim.last_name;
            sim_det.email = active_sim.email;
            sim_det.address1 = active_sim.address1;
            sim_det.address2 = active_sim.address2;
            sim_det.driving_lic = active_sim.driving_lic;
            sim_det.status = active_sim.status;
            sim_det.created_on = DateTime.Now;
            _uow.sim_details_repo.Insert(sim_det);
            _uow.Save();
            if (sim_det.id > 0)
                res = true;

            return res;
        }


        public List<sim_reg_model> get_new_sim_customers()
        {
            List<sim_reg_model> customer_list = new List<sim_reg_model>();
            sim_reg_model customerdetails = new sim_reg_model();

            var customers = _db.customer.Where(c => c.is_active == true && c.is_deleted == false).ToList();
            foreach (var customer in customers)
            {
                customerdetails.cust.address = customer.address;
                customerdetails.cust.cust_id = customer.cust_id;
                customerdetails.cust.email = customer.email;
                customerdetails.cust.is_active = customer.is_active;
                customerdetails.cust.is_deleted = customer.is_deleted;
                customerdetails.cust.is_employed = customer.is_employed;
                customerdetails.cust.name = customer.name;
                customerdetails.cust.reg_status_id = customer.reg_status_id;
                customerdetails.cust.sex = customer.sex;

                //customerdetails.cust = customer;
                customerdetails.cust_ack = _db.customer_acknowlegement.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customerdetails.cust_iden = _db.customer_identification.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customerdetails.cust_purchase = _db.customer_purchase.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                //customerdetails.cust_ack = _db.customer_acknowlegement.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customer_list.Add(customerdetails);
            }

            return customer_list;
        }

        public List<replace_sim_reg_model> get_replace_sim_customers()
        {
            List<replace_sim_reg_model> customer_list = new List<replace_sim_reg_model>();
            replace_sim_reg_model customerdetails = new replace_sim_reg_model();

            var customers = _db.customer.Where(c => c.is_active == true && c.is_deleted == false).ToList();
            foreach (var customer in customers)
            {
                //customerdetails.cust.address = customer.address;
                customerdetails.cust.cust_id = customer.cust_id;
                //customerdetails.cust.email = customer.email;
                customerdetails.cust.is_active = customer.is_active;
                customerdetails.cust.is_deleted = customer.is_deleted;
                //customerdetails.cust.is_employed = customer.is_employed;
                customerdetails.cust.name = customer.name;
                customerdetails.cust.reg_status_id = customer.reg_status_id;
                //customerdetails.cust.sex = customer.sex;

                //customerdetails.cust = customer;
                customerdetails.cust_ack = _db.customer_acknowlegement.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customerdetails.cust_iden = _db.customer_identification.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customerdetails.cust_mob = _db.customer_mobile_no.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                //customerdetails.cust_ack = _db.customer_acknowlegement.Where(m => m.cust_id == customer.cust_id).FirstOrDefault();
                customer_list.Add(customerdetails);
            }

            return customer_list;
        }


        public long save_sim_reg_cust(sim_reg_model Item)
        {
            long customer_id = 0;

            if (Item.cust != null)
            {
                tbl_customer customer = new tbl_customer();
                customer.address = Item.cust.address;
                customer.cust_id = Item.cust.cust_id;
                customer.email = Item.cust.email;
                customer.is_employed = Item.cust.is_employed;
                customer.name = Item.cust.name;
                customer.sex = Item.cust.sex;
                customer.reg_status_id = 1;
                customer.created_on = DateTime.Now;
                customer.is_active = false;
                customer.is_deleted = false;
                customer.modified_on = DateTime.Now;
                _uow.customer_repo.Insert(customer);
                _uow.Save();
                if (Item.cust_ack != null && customer.cust_id > 0)
                {
                    Item.cust_ack.cust_id = customer.cust_id;
                    _uow.customer_acknowlegement_repo.Insert(Item.cust_ack);
                    _uow.Save();

                    if (Item.cust_iden != null && Item.cust_ack.ack_id > 0)
                    {
                        Item.cust_iden.cust_id = Item.cust_ack.cust_id;
                        _uow.customer_identification_repo.Insert(Item.cust_iden);
                        _uow.Save();
                        if (Item.cust_purchase != null && Item.cust_iden.cust_identity_id > 0)
                        {
                            Item.cust_purchase.cust_id = Item.cust_iden.cust_id;
                            _uow.customer_purchase_repo.Insert(Item.cust_purchase);
                            _uow.Save();
                            if (Item.cust_purchase.cust_pur_id > 0)
                                customer_id = Item.cust_purchase.cust_id;

                        }
                    }
                }
            }

            return customer_id;
        }

        public long save_sim_replace_reg_cust(replace_sim_reg_model Item)
        {
            long customer_id = 0;

            if (Item.cust != null)
            {
                tbl_customer customer = new tbl_customer();
                customer.cust_id = Item.cust.cust_id;
                customer.name = Item.cust.name;
                customer.mobile_no = Item.cust.mobile_no;
                customer.reg_status_id = 2;
                customer.created_on = DateTime.Now;
                customer.is_active = false;
                customer.is_deleted = false;
                customer.modified_on = DateTime.Now;
                _uow.customer_repo.Insert(customer);
                _uow.Save();
                if (Item.cust_ack != null && customer.cust_id > 0)
                {
                    Item.cust_ack.cust_id = customer.cust_id;
                    _uow.customer_acknowlegement_repo.Insert(Item.cust_ack);
                    _uow.Save();

                    if (Item.cust_iden != null && Item.cust_ack.ack_id > 0)
                    {
                        Item.cust_iden.cust_id = Item.cust_ack.cust_id;
                        _uow.customer_identification_repo.Insert(Item.cust_iden);
                        _uow.Save();
                        if (Item.cust_mob != null && Item.cust_iden.cust_identity_id > 0)
                        {
                            Item.cust_mob.cust_id = Item.cust_iden.cust_id;
                            _uow.customer_mobile_no_repo.Insert(Item.cust_mob);
                            _uow.Save();

                            if (Item.cust_mob.cust_mob_id > 0)
                                customer_id = Item.cust_mob.cust_id;
                        }
                    }
                }
            }


            return customer_id;
        }


        public long update_sim_reg_cust(update_sim_reg_model Item)
        {
            long customer_id = 0;

            if (Item.cust != null)
            {
                tbl_customer customer = new tbl_customer();
                customer.address = Item.cust.address;
                customer.cust_id = Item.cust.cust_id;
                customer.email = Item.cust.email;
                customer.is_employed = Item.cust.is_employed;
                customer.name = Item.cust.name;
                customer.sex = Item.cust.sex;
                customer.reg_status_id = 1;
                //customer.is_active = false;
                customer.is_deleted = false;
                customer.modified_on = DateTime.Now;
                long cust_id = update_customer(customer);

                if (Item.cust_ack != null && cust_id > 0)
                {
                    Item.cust_ack.cust_id = cust_id;
                    cust_id = update_cust_ack(Item.cust_ack);

                    if (Item.cust_iden != null && cust_id > 0)
                    {
                        Item.cust_ack.cust_id = cust_id;
                        cust_id = update_cust_Ident(Item.cust_iden);

                        if (Item.cust_purchase != null && cust_id > 0)
                        {
                            Item.cust_purchase.cust_id = cust_id;
                            cust_id = update_cust_purchase(Item.cust_purchase);
                            if (Item.retailerUser != null && cust_id > 0)
                            {
                                customer_id = cust_id;
                            }
                            else
                                customer_id = cust_id;

                        }
                        else
                            customer_id = cust_id;
                    }
                    else
                        customer_id = cust_id;
                }
                else
                    customer_id = cust_id;
            }

            return customer_id;
        }

        public long update_sim_replace_reg_cust(update_replace_sim_reg_model Item)
        {
            long customer_id = 0;

            if (Item.cust != null)
            {
                tbl_customer customer = new tbl_customer();
                customer.cust_id = Item.cust.cust_id;
                customer.name = Item.cust.name;
                customer.mobile_no = Item.cust.mobile_no;
                customer.reg_status_id = 2;
                //customer.is_active = false;
                customer.is_deleted = false;
                customer.modified_on = DateTime.Now;
                long cust_id = update_customer(customer);

                if (Item.cust_ack != null && cust_id > 0)
                {
                    Item.cust_ack.cust_id = cust_id;
                    cust_id = update_cust_ack(Item.cust_ack);

                    if (Item.cust_iden != null && cust_id > 0)
                    {
                        Item.cust_ack.cust_id = cust_id;
                        cust_id = update_cust_Ident(Item.cust_iden);

                        if (Item.cust_mob != null && cust_id > 0)
                        {
                            Item.cust_mob.cust_id = cust_id;
                            cust_id = update_cust_mobiles(Item.cust_mob);
                            if (Item.retailerUser != null && cust_id > 0)
                            {
                                cust_id = update_cust_ret_use(Item.retailerUser);
                                customer_id = cust_id;
                            }
                            else
                                customer_id = cust_id;

                        }
                        else
                            customer_id = cust_id;
                    }
                    else
                        customer_id = cust_id;
                }
                else
                    customer_id = cust_id;
            }


            return customer_id;
        }



        private long update_customer(tbl_customer obj_customer)
        {
            long customer_id = 0;
            if (obj_customer != null)
            {
                var cust = _db.customer.Where(P => P.cust_id == obj_customer.cust_id).FirstOrDefault();
                if (cust != null)
                {
                    cust.name = obj_customer.name;
                    cust.reg_status_id = obj_customer.reg_status_id;
                    cust.sex = obj_customer.sex;
                    cust.address = obj_customer.address;
                    cust.created_on = obj_customer.created_on;
                    cust.cust_id = obj_customer.cust_id;
                    cust.deleted_on = obj_customer.deleted_on;
                    cust.email = obj_customer.email;
                    cust.is_active = obj_customer.is_active;
                    cust.is_deleted = obj_customer.is_deleted;
                    cust.is_employed = obj_customer.is_employed;
                    cust.mobile_no = obj_customer.mobile_no;
                    cust.modified_on = obj_customer.modified_on;

                    _uow.customer_repo.Update(cust);
                    _uow.Save();
                    customer_id = cust.cust_id;
                }
            }
            return customer_id;
        }

        private long update_cust_Ident(tbl_customer_identification cust_iden)
        {
            long customer_id = 0;
            if (cust_iden != null)
            {
                var ident = _db.customer_identification.Where(P => P.cust_identity_id == cust_iden.cust_identity_id).FirstOrDefault();
                if (ident != null)
                {
                    ident.is_drv_lic = cust_iden.is_drv_lic;
                    ident.drv_lic = cust_iden.drv_lic;
                    ident.is_student_id = cust_iden.is_student_id;
                    ident.student_id = cust_iden.student_id;
                    ident.is_emp_id = cust_iden.is_emp_id;
                    ident.emp_id = cust_iden.emp_id;
                    ident.is_nasfund_id = cust_iden.is_nasfund_id;
                    ident.nasfund_id = cust_iden.nasfund_id;
                    ident.is_nbw_super_id = cust_iden.is_nbw_super_id;
                    ident.nbw_super_id = cust_iden.nbw_super_id;
                    ident.is_other_id = cust_iden.is_other_id;
                    ident.other_id = cust_iden.other_id;

                    _uow.customer_identification_repo.Update(ident);
                    _uow.Save();
                    customer_id = ident.cust_id;
                }
            }
            return customer_id;
        }

        private long update_cust_mobiles(tbl_customer_mobile_no cust_mob)
        {
            long customer_id = 0;
            if (cust_mob != null)
            {
                var mob = _db.customer_mobile_no.Where(P => P.cust_mob_id == cust_mob.cust_mob_id).FirstOrDefault();
                if (mob != null)
                {
                    mob.cust_mobile_no4 = cust_mob.cust_mobile_no4;
                    mob.cust_mobile_no1 = cust_mob.cust_mobile_no1;
                    mob.cust_mobile_no2 = cust_mob.cust_mobile_no2;
                    mob.cust_mobile_no3 = cust_mob.cust_mobile_no3;
                    mob.cust_id = cust_mob.cust_id;

                    _uow.customer_mobile_no_repo.Update(mob);
                    _uow.Save();
                    customer_id = mob.cust_id;

                }
            }
            return customer_id;
        }

        private long update_cust_purchase(tbl_customer_purchase obj_customer)
        {
            long customer_id = 0;
            if (obj_customer != null)
            {
                var cust_pur = _db.customer_purchase.Where(P => P.cust_pur_id == obj_customer.cust_pur_id).FirstOrDefault();
                if (cust_pur != null)
                {
                    cust_pur.cust_handset_model1 = obj_customer.cust_handset_model1;
                    cust_pur.cust_handset_model2 = obj_customer.cust_handset_model2;
                    cust_pur.cust_handset_model3 = obj_customer.cust_handset_model3;
                    cust_pur.cust_id = obj_customer.cust_id;
                    cust_pur.cust_imei_no1 = obj_customer.cust_imei_no1;
                    cust_pur.cust_imei_no2 = obj_customer.cust_imei_no2;
                    cust_pur.cust_imei_no3 = obj_customer.cust_imei_no3;
                    cust_pur.cust_mobile_no1 = obj_customer.cust_mobile_no1;
                    cust_pur.cust_mobile_no2 = obj_customer.cust_mobile_no2;
                    cust_pur.cust_mobile_no3 = obj_customer.cust_mobile_no3;
                    cust_pur.cust_status = obj_customer.cust_status;
                    //cust_pur.is_existing_cust = obj_customer.is_existing_cust;
                    //cust_pur.is_new_cust = obj_customer.is_new_cust;

                    _uow.customer_purchase_repo.Update(cust_pur);
                    _uow.Save();
                    customer_id = cust_pur.cust_id;
                }
            }
            return customer_id;
        }

        private long update_cust_ack(tbl_customer_acknowlegement obj_customer)
        {
            long customer_id = 0;
            if (obj_customer != null)
            {
                var cust_ack = _db.customer_acknowlegement.Where(P => P.ack_id == obj_customer.ack_id).FirstOrDefault();
                if (cust_ack != null)
                {
                    //cust_ack.ack_id = obj_customer.ack_id;
                    cust_ack.cust_ack_date = obj_customer.cust_ack_date;
                    cust_ack.cust_id = obj_customer.cust_id;
                    cust_ack.is_original_sign = obj_customer.is_original_sign;

                    _uow.customer_acknowlegement_repo.Update(cust_ack);
                    _uow.Save();
                    customer_id = obj_customer.cust_id;
                }
            }
            return customer_id;
        }

        private long update_cust_ret_use(tbl_customer_retailer_use obj_customer)
        {
            long customer_id = 0;
            if (obj_customer != null)
            {
                var cust_ret = _db.customer_retailer_use.Where(P => P.cust_ret_id == obj_customer.cust_ret_id).FirstOrDefault();
                if (cust_ret != null)
                {
                    cust_ret.cust_id = obj_customer.cust_id;
                    cust_ret.ret_id = obj_customer.ret_id;
                    cust_ret.is_doc_expired = obj_customer.is_doc_expired;
                    cust_ret.is_match_id = obj_customer.is_match_id;
                    cust_ret.is_retailer_sign = obj_customer.is_retailer_sign;
                    cust_ret.payment_receipt = obj_customer.payment_receipt;
                    cust_ret.sale_person_name = obj_customer.sale_person_name;
                    cust_ret.modified_on = obj_customer.modified_on;

                    _uow.customer_retailer_use_repo.Update(cust_ret);
                    _uow.Save();
                    customer_id = cust_ret.cust_id;

                }
                else
                {
                    tbl_customer_retailer_use new_cust_ret = new tbl_customer_retailer_use();
                    //new_cust_ret.id = obj_customer.custwithret_id;
                    new_cust_ret.cust_id = obj_customer.cust_id;
                    new_cust_ret.ret_id = obj_customer.ret_id;
                    new_cust_ret.is_doc_expired = obj_customer.is_doc_expired;
                    new_cust_ret.is_match_id = obj_customer.is_match_id;
                    new_cust_ret.is_retailer_sign = obj_customer.is_retailer_sign;
                    new_cust_ret.payment_receipt = obj_customer.payment_receipt;
                    new_cust_ret.sale_person_name = obj_customer.sale_person_name;
                    new_cust_ret.modified_on = obj_customer.modified_on;

                    _uow.customer_retailer_use_repo.Insert(new_cust_ret);
                    _uow.Save();
                    customer_id = new_cust_ret.cust_id;

                }
            }
            return customer_id;
        }



        #region Dispose Objects


        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
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