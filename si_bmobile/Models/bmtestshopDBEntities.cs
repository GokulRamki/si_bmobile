using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace si_bmobile.Models
{
    public class bmtestshopDBEntities : DbContext
    {
        //public DbSet<doku_selfcare> doku_selfccare { get; set; }

        //public DbSet<web_tbl_selfcare_order> selfcare_order { get; set; }

        //public DbSet<web_tbl_selfcare_order_item> selfcare_order_item { get; set; }

        //public DbSet<web_tbl_selfcare_payment> selfcare_order_payment { get; set; }

        public DbSet<web_tbl_promotions> promotions { get; set; }
        public DbSet<web_tbl_fb_promotions> redeem_fb_promotion { get; set; }
        public DbSet<web_tbl_product_brand_conj> product_brand_conj { get; set; }
        public DbSet<web_tbl_product_category_conj> product_category_conj { get; set; }
        public DbSet<web_tbl_products> products { get; set; }
        public DbSet<web_tbl_products_img> products_img { get; set; }
        public DbSet<web_tbl_related_products> related_products { get; set; }
        public DbSet<web_tbl_category> category { get; set; }
        public DbSet<web_tbl_price_range> price_range { get; set; }
        public DbSet<web_tbl_brand> brands { get; set; }

        public DbSet<doku_shop> doku_shop { get; set; }
        public DbSet<web_tbl_shopping_order> shopping_order { get; set; }
        public DbSet<web_tbl_shopping_order_item> shopping_order_item { get; set; }
        public DbSet<web_tbl_shopping_order_payment> shopping_order_payment { get; set; }
        public DbSet<web_tbl_shopping_shipping_matrix> shopping_shipping_matrix { get; set; }
        public DbSet<web_tbl_shopping_surcharge> shopping_surcharge { get; set; }
        public DbSet<web_tbl_shopping_user> shopping_user { get; set; }
        public DbSet<web_tbl_shopping_user_contact> shopping_user_contact { get; set; }
        public DbSet<web_tbl_shopping_user_contact_type> shopping_user_contact_type { get; set; }
        public DbSet<web_tbl_payment_mode> payment_mode { get; set; }
        
        public DbSet<temp_yplans> temp_yplans { get; set; }

        public DbSet<web_tbl_sim_details> sim_details { get; set; }
        public DbSet<tbl_customer> customer { get; set; }
        public DbSet<tbl_customer_acknowlegement> customer_acknowlegement { get; set; }
        public DbSet<tbl_customer_identification> customer_identification { get; set; }
        public DbSet<tbl_customer_mobile_no> customer_mobile_no { get; set; }
        public DbSet<tbl_customer_purchase> customer_purchase { get; set; }
        public DbSet<tbl_customer_reg_status> customer_reg_status { get; set; }
        public DbSet<tbl_customer_retailer_use> customer_retailer_use { get; set; }
        public DbSet<tbl_retailers> retailers { get; set; }

        public DbSet<web_tbl_care_user> care_users { get; set; }
        public DbSet<web_tbl_role> roles { get; set; }


        public DbSet<bm_staffs_topup> staffs_topup { get; set; }

        public DbSet<temp_orders> temp_orders { get; set; }
        public DbSet<bm_staffs_deduction_trans> staffs_deduction_trans { get; set; }
        public DbSet<bm_staffs_trans> staffs_trans { get; set; }

        public DbSet<tbl_menu> menu { get; set; }

        public DbSet<tbl_user_menu_conj> user_menu_conj { get; set; }

        public DbSet<tbl_order_topup> order_topup { get; set; }
       // public DbSet<tbl_email_template> email_template { get; set; }

        public DbSet<tbl_transaction_fees> transaction_fees { get; set; }

        public DbSet<tbl_kyc_customer> kyc_customer_Details { get; set; }

        public DbSet<tbl_kyc_customer_identification> kyc_customer_Identification { get; set; }

        public DbSet<tbl_kyc_customer_purchase> kyc_customer_Purchase { get; set; }

        public DbSet<tbl_kyc_customer_acknowlegement> kyc_customer_acknowledgement { get; set; }

        public DbSet<tbl_kyc_simactivation_log> kyc_Sim_Activation_Log_tbl { get; set; }


        public DbSet<PROVINCIAL_CODES> province_codes { get; set; }

        public DbSet<DISTRICT_CODES> district_codes { get; set; }

        public DbSet<tbl_kyc_version_update> kyc_version_update { get; set; }

        public DbSet<tbl_top_kad_log> top_kad_db_log { get; set; }
        public DbSet<tbl_kyc_taccode> tac_code { get; set; }

        public DbSet<web_tbl_fb_attempt_log> fb_attempt_log { get; set; }

        public DbSet<tbl_spc_promo_cust> spc_promo_cust { get; set; }

        public DbSet<tbl_evd_plan_transaction> plan_transaction { get; set; }
        public DbSet<tbl_evd_plan_details> plan_details { get; set; }

        public DbSet<tbl_web_plan_purchase_trans> web_plan_purchase_trans { get; set; }
        public DbSet<tbl_bm_topup_value> topup_value { get; set; }
    }
}