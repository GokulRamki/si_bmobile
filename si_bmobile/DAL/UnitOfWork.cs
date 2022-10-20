using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using si_bmobile.Models;

namespace si_bmobile.DAL
{
    public class UnitOfWork : DbContext, IDisposable
    {
        private bmtestshopDBEntities context = new bmtestshopDBEntities();

        #region doku

        #region DOKU SELFCARE

        //private GenericRepository<doku_selfcare> DOKU_SELFCARE;
        //public GenericRepository<doku_selfcare> doku_selfcare_repo
        //{
        //    get
        //    {

        //        if (this.DOKU_SELFCARE == null)
        //        {
        //            this.DOKU_SELFCARE = new GenericRepository<doku_selfcare>(context);
        //        }
        //        return DOKU_SELFCARE;
        //    }
        //}

        #endregion

        #region SELFCARE ORDER

        //private GenericRepository<web_tbl_selfcare_order> SELFCARE_ORDER;
        //public GenericRepository<web_tbl_selfcare_order> selfcare_order_repo
        //{
        //    get
        //    {

        //        if (this.SELFCARE_ORDER == null)
        //        {
        //            this.SELFCARE_ORDER = new GenericRepository<web_tbl_selfcare_order>(context);
        //        }
        //        return SELFCARE_ORDER;
        //    }
        //}

        #endregion

        #region SELFCARE ORDER ITEM

        //private GenericRepository<web_tbl_selfcare_order_item> SELFCARE_ORDER_ITEM;
        //public GenericRepository<web_tbl_selfcare_order_item> selfcare_order_item_repo
        //{
        //    get
        //    {

        //        if (this.SELFCARE_ORDER_ITEM == null)
        //        {
        //            this.SELFCARE_ORDER_ITEM = new GenericRepository<web_tbl_selfcare_order_item>(context);
        //        }
        //        return SELFCARE_ORDER_ITEM;
        //    }
        //}

        #endregion

        #region SELFCARE ORDER PAYMENT

        //private GenericRepository<web_tbl_selfcare_payment> SELFCARE_ORDER_PAYMENT;
        //public GenericRepository<web_tbl_selfcare_payment> selfcare_order_payment_repo
        //{
        //    get
        //    {

        //        if (this.SELFCARE_ORDER_PAYMENT == null)
        //        {
        //            this.SELFCARE_ORDER_PAYMENT = new GenericRepository<web_tbl_selfcare_payment>(context);
        //        }
        //        return SELFCARE_ORDER_PAYMENT;
        //    }
        //}

        #endregion

        #endregion

        #region shopping

        #region DOKU SHOP

        private GenericRepository<doku_shop> DOKU_SHOP;
        public GenericRepository<doku_shop> doku_shop_repo
        {
            get
            {

                if (this.DOKU_SHOP == null)
                {
                    this.DOKU_SHOP = new GenericRepository<doku_shop>(context);
                }
                return DOKU_SHOP;
            }
        }

        #endregion

        #region PRO BRAND CONJ

        private GenericRepository<web_tbl_product_brand_conj> PRO_BRAND_CONJ;
        public GenericRepository<web_tbl_product_brand_conj> product_brand_conj_repo
        {
            get
            {

                if (this.PRO_BRAND_CONJ == null)
                {
                    this.PRO_BRAND_CONJ = new GenericRepository<web_tbl_product_brand_conj>(context);
                }
                return PRO_BRAND_CONJ;
            }
        }

        #endregion


        #region PRO BRAND CONJ

        private GenericRepository<web_tbl_product_category_conj> web_product_category_conj;
        public GenericRepository<web_tbl_product_category_conj> product_category_conj
        {
            get
            {

                if (this.web_product_category_conj == null)
                {
                    this.web_product_category_conj = new GenericRepository<web_tbl_product_category_conj>(context);
                }
                return web_product_category_conj;
            }
        }

        #endregion

        #region PRO BRAND CONJ

        private GenericRepository<web_tbl_payment_mode> web_payment_mode;
        public GenericRepository<web_tbl_payment_mode> payment_mode_repo
        {
            get
            {

                if (this.web_payment_mode == null)
                {
                    this.web_payment_mode = new GenericRepository<web_tbl_payment_mode>(context);
                }
                return web_payment_mode;
            }
        }

        #endregion


        #region PRODUCTS

        private GenericRepository<web_tbl_products> PRODUCTS;
        public GenericRepository<web_tbl_products> products_repo
        {
            get
            {

                if (this.PRODUCTS == null)
                {
                    this.PRODUCTS = new GenericRepository<web_tbl_products>(context);
                }
                return PRODUCTS;
            }
        }

        #endregion

        #region PRODUCTS IMG

        private GenericRepository<web_tbl_products_img> web_products_img;
        public GenericRepository<web_tbl_products_img> products_img_repo
        {
            get
            {

                if (this.web_products_img == null)
                {
                    this.web_products_img = new GenericRepository<web_tbl_products_img>(context);
                }
                return web_products_img;
            }
        }

        #endregion

        #region RELATED PRODUCTS

        private GenericRepository<web_tbl_related_products> web_related_products;
        public GenericRepository<web_tbl_related_products> related_products_repo
        {
            get
            {

                if (this.web_related_products == null)
                {
                    this.web_related_products = new GenericRepository<web_tbl_related_products>(context);
                }
                return web_related_products;
            }
        }

        #endregion

        #region CATEGORY

        private GenericRepository<web_tbl_category> web_category;
        public GenericRepository<web_tbl_category> web_tbl_category_repo
        {
            get
            {
                if (this.web_category == null)
                {
                    this.web_category = new GenericRepository<web_tbl_category>(context);
                }
                return web_category;
            }
        }

        #endregion

        #region PRICE RANGE

        private GenericRepository<web_tbl_price_range> web_price_range;
        public GenericRepository<web_tbl_price_range> price_range_repo
        {
            get
            {
                if (this.web_price_range == null)
                {
                    this.web_price_range = new GenericRepository<web_tbl_price_range>(context);
                }
                return web_price_range;
            }
        }

        #endregion

        #region Brand

        private GenericRepository<web_tbl_brand> web_brand;
        public GenericRepository<web_tbl_brand> brand_repo
        {
            get
            {
                if (this.web_brand == null)
                {
                    this.web_brand = new GenericRepository<web_tbl_brand>(context);
                }
                return web_brand;
            }
        }

        #endregion

        #region shopping order

        private GenericRepository<web_tbl_shopping_order> web_shopping_order;
        public GenericRepository<web_tbl_shopping_order> shopping_order_repo
        {
            get
            {
                if (this.web_shopping_order == null)
                {
                    this.web_shopping_order = new GenericRepository<web_tbl_shopping_order>(context);
                }
                return web_shopping_order;
            }
        }

        #endregion

        #region shopping order item

        private GenericRepository<web_tbl_shopping_order_item> web_shopping_order_item;
        public GenericRepository<web_tbl_shopping_order_item> shopping_order_item_repo
        {
            get
            {
                if (this.web_shopping_order_item == null)
                {
                    this.web_shopping_order_item = new GenericRepository<web_tbl_shopping_order_item>(context);
                }
                return web_shopping_order_item;
            }
        }

        #endregion

        #region shopping order payment

        private GenericRepository<web_tbl_shopping_order_payment> web_shopping_order_payment;
        public GenericRepository<web_tbl_shopping_order_payment> shopping_order_payment_repo
        {
            get
            {
                if (this.web_shopping_order_payment == null)
                {
                    this.web_shopping_order_payment = new GenericRepository<web_tbl_shopping_order_payment>(context);
                }
                return web_shopping_order_payment;
            }
        }

        #endregion

        #region shopping shipping matrix

        private GenericRepository<web_tbl_shopping_shipping_matrix> web_shopping_shipping_matrix;
        public GenericRepository<web_tbl_shopping_shipping_matrix> shopping_shipping_matrix_repo
        {
            get
            {
                if (this.web_shopping_shipping_matrix == null)
                {
                    this.web_shopping_shipping_matrix = new GenericRepository<web_tbl_shopping_shipping_matrix>(context);
                }
                return web_shopping_shipping_matrix;
            }
        }

        #endregion

        #region shipping surcharge

        private GenericRepository<web_tbl_shopping_surcharge> web_shopping_surcharge;
        public GenericRepository<web_tbl_shopping_surcharge> shopping_surcharge_repo
        {
            get
            {
                if (this.web_shopping_surcharge == null)
                {
                    this.web_shopping_surcharge = new GenericRepository<web_tbl_shopping_surcharge>(context);
                }
                return web_shopping_surcharge;
            }
        }

        #endregion

        #region shopping user

        private GenericRepository<web_tbl_shopping_user> web_shopping_user;
        public GenericRepository<web_tbl_shopping_user> shopping_user_repo
        {
            get
            {
                if (this.web_shopping_user == null)
                {
                    this.web_shopping_user = new GenericRepository<web_tbl_shopping_user>(context);
                }
                return web_shopping_user;
            }
        }

        #endregion

        #region shopping user contact

        private GenericRepository<web_tbl_shopping_user_contact> web_shopping_user_contact;
        public GenericRepository<web_tbl_shopping_user_contact> shopping_user_contact_repo
        {
            get
            {
                if (this.web_shopping_user_contact == null)
                {
                    this.web_shopping_user_contact = new GenericRepository<web_tbl_shopping_user_contact>(context);
                }
                return web_shopping_user_contact;
            }
        }

        #endregion

        #region shopping user contact type

        private GenericRepository<web_tbl_shopping_user_contact_type> web_shopping_user_contact_type;
        public GenericRepository<web_tbl_shopping_user_contact_type> shopping_user_contact_type_repo
        {
            get
            {
                if (this.web_shopping_user_contact_type == null)
                {
                    this.web_shopping_user_contact_type = new GenericRepository<web_tbl_shopping_user_contact_type>(context);
                }
                return web_shopping_user_contact_type;
            }
        }

        #endregion

        #region temp yplans

        private GenericRepository<temp_yplans> temp_yplans;
        public GenericRepository<temp_yplans> temp_yplans_repo
        {
            get
            {
                if (this.temp_yplans == null)
                {
                    this.temp_yplans = new GenericRepository<temp_yplans>(context);
                }
                return temp_yplans;
            }
        }

        #endregion

        #endregion

        #region sim reg customer

        #region sim details

        private GenericRepository<web_tbl_sim_details> web_sim_details;
        public GenericRepository<web_tbl_sim_details> sim_details_repo
        {
            get
            {

                if (this.web_sim_details == null)
                {
                    this.web_sim_details = new GenericRepository<web_tbl_sim_details>(context);
                }
                return web_sim_details;
            }
        }

        #endregion

        #region customer

        private GenericRepository<tbl_customer> customer;
        public GenericRepository<tbl_customer> customer_repo
        {
            get
            {

                if (this.customer == null)
                {
                    this.customer = new GenericRepository<tbl_customer>(context);
                }
                return customer;
            }
        }

        #endregion

        #region customer acknowlegement

        private GenericRepository<tbl_customer_acknowlegement> customer_acknowlegement;
        public GenericRepository<tbl_customer_acknowlegement> customer_acknowlegement_repo
        {
            get
            {

                if (this.customer_acknowlegement == null)
                {
                    this.customer_acknowlegement = new GenericRepository<tbl_customer_acknowlegement>(context);
                }
                return customer_acknowlegement;
            }
        }

        #endregion

        #region customer identification

        private GenericRepository<tbl_customer_identification> customer_identification;
        public GenericRepository<tbl_customer_identification> customer_identification_repo
        {
            get
            {

                if (this.customer_identification == null)
                {
                    this.customer_identification = new GenericRepository<tbl_customer_identification>(context);
                }
                return customer_identification;
            }
        }

        #endregion

        #region customer mobile no

        private GenericRepository<tbl_customer_mobile_no> customer_mobile_no;
        public GenericRepository<tbl_customer_mobile_no> customer_mobile_no_repo
        {
            get
            {

                if (this.customer_mobile_no == null)
                {
                    this.customer_mobile_no = new GenericRepository<tbl_customer_mobile_no>(context);
                }
                return customer_mobile_no;
            }
        }

        #endregion

        #region  customer purchase

        private GenericRepository<tbl_customer_purchase> customer_purchase;
        public GenericRepository<tbl_customer_purchase> customer_purchase_repo
        {
            get
            {

                if (this.customer_purchase == null)
                {
                    this.customer_purchase = new GenericRepository<tbl_customer_purchase>(context);
                }
                return customer_purchase;
            }
        }

        #endregion

        #region customer reg status

        private GenericRepository<tbl_customer_reg_status> customer_reg_status;
        public GenericRepository<tbl_customer_reg_status> customer_reg_status_repo
        {
            get
            {

                if (this.customer_reg_status == null)
                {
                    this.customer_reg_status = new GenericRepository<tbl_customer_reg_status>(context);
                }
                return customer_reg_status;
            }
        }

        #endregion

        #region customer retailer use

        private GenericRepository<tbl_customer_retailer_use> customer_retailer_use;
        public GenericRepository<tbl_customer_retailer_use> customer_retailer_use_repo
        {
            get
            {

                if (this.customer_retailer_use == null)
                {
                    this.customer_retailer_use = new GenericRepository<tbl_customer_retailer_use>(context);
                }
                return customer_retailer_use;
            }
        }

        #endregion

        #region retailers

        private GenericRepository<tbl_retailers> retailers;
        public GenericRepository<tbl_retailers> retailers_repo
        {
            get
            {

                if (this.retailers == null)
                {
                    this.retailers = new GenericRepository<tbl_retailers>(context);
                }
                return retailers;
            }
        }

        #endregion

        #endregion

        #region Promotions

        private GenericRepository<web_tbl_promotions> web_promotions;
        public GenericRepository<web_tbl_promotions> promotions_repo
        {
            get
            {

                if (this.web_promotions == null)
                {
                    this.web_promotions = new GenericRepository<web_tbl_promotions>(context);
                }
                return web_promotions;
            }
        }

        #endregion

        #region REDEEM FB PROMOTIONS

        private GenericRepository<web_tbl_fb_promotions> REDEEM_FB_PROMOTIONS;
        public GenericRepository<web_tbl_fb_promotions> redeem_fb_promotions_repo
        {
            get
            {

                if (this.REDEEM_FB_PROMOTIONS == null)
                {
                    this.REDEEM_FB_PROMOTIONS = new GenericRepository<web_tbl_fb_promotions>(context);
                }
                return REDEEM_FB_PROMOTIONS;
            }
        }

        #endregion

        #region CARE USERS


        private GenericRepository<web_tbl_care_user> CARE_USER;
        public GenericRepository<web_tbl_care_user> care_user_repo
        {
            get
            {

                if (this.CARE_USER == null)
                {
                    this.CARE_USER = new GenericRepository<web_tbl_care_user>(context);
                }
                return CARE_USER;
            }
        }

        private GenericRepository<web_tbl_role> ROLE;
        public GenericRepository<web_tbl_role> role_repo
        {
            get
            {

                if (this.ROLE == null)
                {
                    this.ROLE = new GenericRepository<web_tbl_role>(context);
                }
                return ROLE;
            }
        }

        #endregion

        #region staffs_topup

        private GenericRepository<bm_staffs_topup> STAFFS_TOPUP;
        public GenericRepository<bm_staffs_topup> staffs_topup_repo
        {
            get
            {

                if (this.STAFFS_TOPUP == null)
                {
                    this.STAFFS_TOPUP = new GenericRepository<bm_staffs_topup>(context);
                }
                return STAFFS_TOPUP;
            }
        }

        #endregion

        #region temp_orders

        private GenericRepository<temp_orders> temp_orders;
        public GenericRepository<temp_orders> temp_orders_repo
        {
            get
            {

                if (this.temp_orders == null)
                {
                    this.temp_orders = new GenericRepository<temp_orders>(context);
                }
                return temp_orders;
            }
        }

        #endregion

        #region bm_staffs_trans

        private GenericRepository<bm_staffs_trans> staff_trans;
        public GenericRepository<bm_staffs_trans> staff_trans_repo
        {
            get
            {
                if (this.staff_trans == null)
                    this.staff_trans = new GenericRepository<bm_staffs_trans>(context);

                return staff_trans;
            }
        }

        #endregion

        #region bm_staffs_deduction_trans

        private GenericRepository<bm_staffs_deduction_trans> staffs_deduction_trans;
        public GenericRepository<bm_staffs_deduction_trans> staffs_deduction_trans_repo
        {
            get
            {
                if (this.staffs_deduction_trans == null)
                    this.staffs_deduction_trans = new GenericRepository<bm_staffs_deduction_trans>(context);

                return staffs_deduction_trans;
            }
        }

        #endregion

        #region main_menu

        private GenericRepository<tbl_menu> menu;
        public GenericRepository<tbl_menu> menu_repo
        {
            get
            {
                if (this.menu == null)
                    this.menu = new GenericRepository<tbl_menu>(context);

                return menu;
            }
        }

        #endregion

        #region user_menu_conj

        private GenericRepository<tbl_user_menu_conj> user_menu_conj;
        public GenericRepository<tbl_user_menu_conj> user_menu_conj_repo
        {
            get
            {
                if (this.user_menu_conj == null)
                    this.user_menu_conj = new GenericRepository<tbl_user_menu_conj>(context);

                return user_menu_conj;
            }
        }

        #endregion

        #region order topup

        private GenericRepository<tbl_order_topup> order_topup;
        public GenericRepository<tbl_order_topup> order_topup_repo
        {
            get
            {
                if (this.order_topup == null)
                    this.order_topup = new GenericRepository<tbl_order_topup>(context);

                return order_topup;
            }
        }

        #endregion
        
        #region Email Template
        //private GenericRepository<tbl_email_template> email_template;
        //public GenericRepository<tbl_email_template> email_template_repo
        //{
        //    get
        //    {
        //        if (this.email_template == null)
        //            this.email_template = new GenericRepository<tbl_email_template>(context);

        //        return email_template;
        //    }
        //}
        #endregion

        #region transaction_fees

        private GenericRepository<tbl_transaction_fees> transaction_fees;
        public GenericRepository<tbl_transaction_fees> transaction_fees_repo
        {
            get
            {
                if (this.transaction_fees == null)
                    this.transaction_fees = new GenericRepository<tbl_transaction_fees>(context);

                return transaction_fees;
            }
        }
        #endregion

        #region KYC

        #region Customer Details
        private GenericRepository<tbl_kyc_customer> kyc_Customer;
        public GenericRepository<tbl_kyc_customer> kyc_Customer_Repo
        {
            get
            {

                if (this.kyc_Customer == null)
                {
                    this.kyc_Customer = new GenericRepository<tbl_kyc_customer>(context);
                }
                return kyc_Customer;
            }
        }
        #endregion

        #region KYC Sim activation log
        private GenericRepository<tbl_kyc_simactivation_log> kyc_Sim_Activition_log;
        public GenericRepository<tbl_kyc_simactivation_log> kyc_Sim_Activition_log_Repo
        {
            get
            {

                if (this.kyc_Sim_Activition_log == null)
                {
                    this.kyc_Sim_Activition_log = new GenericRepository<tbl_kyc_simactivation_log>(context);
                }
                return kyc_Sim_Activition_log;
            }
        }
        #endregion

        #region Customer Purchase
        private GenericRepository<tbl_kyc_customer_purchase> kyc_Cust_Purchase;
        public GenericRepository<tbl_kyc_customer_purchase> kyc_Cust_Purchase_Repo
        {
            get
            {

                if (this.kyc_Cust_Purchase == null)
                {
                    this.kyc_Cust_Purchase = new GenericRepository<tbl_kyc_customer_purchase>(context);
                }
                return kyc_Cust_Purchase;
            }
        }
        #endregion

        #region Customer Identification
        private GenericRepository<tbl_kyc_customer_identification> kyc_Cust_identification;
        public GenericRepository<tbl_kyc_customer_identification> kyc_Cust_identification_Repo
        {
            get
            {

                if (this.kyc_Cust_identification == null)
                {
                    this.kyc_Cust_identification = new GenericRepository<tbl_kyc_customer_identification>(context);
                }
                return kyc_Cust_identification;
            }
        }
        #endregion

        #region Customer acknowlegement
        private GenericRepository<tbl_kyc_customer_acknowlegement> kyc_Cust_acknowledge;
        public GenericRepository<tbl_kyc_customer_acknowlegement> kyc_Cust_acknowledge_Repo
        {
            get
            {

                if (this.kyc_Cust_acknowledge == null)
                {
                    this.kyc_Cust_acknowledge = new GenericRepository<tbl_kyc_customer_acknowlegement>(context);
                }
                return kyc_Cust_acknowledge;
            }
        }
        #endregion

        #region Province
        private GenericRepository<PROVINCIAL_CODES> Province_codes;
        public GenericRepository<PROVINCIAL_CODES> Province_codes_Repo
        {
            get
            {

                if (this.Province_codes == null)
                {
                    this.Province_codes = new GenericRepository<PROVINCIAL_CODES>(context);
                }
                return Province_codes;
            }
        }
        #endregion

        #region District
        private GenericRepository<DISTRICT_CODES> District_Codes;
        public GenericRepository<DISTRICT_CODES> District_Codes_Repo
        {
            get
            {

                if (this.District_Codes == null)
                {
                    this.District_Codes = new GenericRepository<DISTRICT_CODES>(context);
                }
                return District_Codes;
            }
        }
        #endregion

        #region KYC Version Update
        private GenericRepository<tbl_kyc_version_update> kyc_version_update;
        public GenericRepository<tbl_kyc_version_update> kyc_version_update_Repo
        {
            get
            {

                if (this.kyc_version_update == null)
                {
                    this.kyc_version_update = new GenericRepository<tbl_kyc_version_update>(context);
                }
                return kyc_version_update;
            }
        }
        #endregion


        #region tac code
        private GenericRepository<tbl_kyc_taccode> tbl_Tac_code;
        public GenericRepository<tbl_kyc_taccode> tbl_tac_code_Repo
        {
            get
            {

                if (this.tbl_Tac_code == null)
                {
                    this.tbl_Tac_code = new GenericRepository<tbl_kyc_taccode>(context);
                }
                return tbl_Tac_code;
            }
        }
        #endregion
        #endregion
        
        #region top kad log

        private GenericRepository<tbl_top_kad_log> top_kad_log;
        public GenericRepository<tbl_top_kad_log> top_kad_log_repo
        {
            get
            {
                if (this.top_kad_log == null)
                    this.top_kad_log = new GenericRepository<tbl_top_kad_log>(context);

                return top_kad_log;
            }
        }

        #endregion       

        #region fb_attempt_log

        private GenericRepository<web_tbl_fb_attempt_log> fb_attempt_log;
        public GenericRepository<web_tbl_fb_attempt_log> fb_attempt_log_repo
        {
            get
            {
                if (this.fb_attempt_log == null)
                    this.fb_attempt_log = new GenericRepository<web_tbl_fb_attempt_log>(context);

                return fb_attempt_log;
            }
        }

        #endregion       

        #region special promotion

        private GenericRepository<tbl_spc_promo_cust> spc_promo_cust_repo;
        public GenericRepository<tbl_spc_promo_cust> spc_promo_cust_Repo
        {
            get
            {
                if (this.spc_promo_cust_repo == null)
                    this.spc_promo_cust_repo = new GenericRepository<tbl_spc_promo_cust>(context);

                return spc_promo_cust_repo;
            }
        }

        #endregion       

        #region transaction plan
        private GenericRepository<tbl_evd_plan_transaction> trans_plan_repo;
        public GenericRepository<tbl_evd_plan_transaction> trans_plan_Repo
        {
            get
            {
                if (this.trans_plan_repo == null)
                    this.trans_plan_repo = new GenericRepository<tbl_evd_plan_transaction>(context);
                return trans_plan_repo;
            }
        }
        #endregion

        #region plan details

        private GenericRepository<tbl_evd_plan_details> plan_details_repo;
        public GenericRepository<tbl_evd_plan_details> plan_details_Repo
        {
            get
            {
                if (this.plan_details_repo == null)
                    this.plan_details_repo = new GenericRepository<tbl_evd_plan_details>(context);
                return plan_details_repo;
            }
        }
        #endregion

        #region plan_purchase_trans
        private GenericRepository<tbl_web_plan_purchase_trans> plan_purchase_trans;
        public GenericRepository<tbl_web_plan_purchase_trans> plan_purchase_trans_Repo
        {
            get
            {

                if (this.plan_purchase_trans == null)
                {
                    this.plan_purchase_trans = new GenericRepository<tbl_web_plan_purchase_trans>(context);
                }
                return plan_purchase_trans;
            }
        }
        #endregion

        #region Db Save

        public void Save()
        {
            context.SaveChanges();
        }

        #endregion

        #region Disposal

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
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