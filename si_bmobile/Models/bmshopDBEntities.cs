using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace bemobile.Models
{
    public class bmshopDBEntities : DbContext
    {
        public DbSet<doku_selfcare> doku_selfccare { get; set; }

        public DbSet<web_tbl_selfcare_order> selfcare_order { get; set; }

        public DbSet<web_tbl_selfcare_order_item> selfcare_order_item { get; set; }

        public DbSet<web_tbl_selfcare_payment> selfcare_order_payment { get; set; }

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


    }
}