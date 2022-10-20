using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using si_bmobile.Models;

namespace si_bmobile.DAL
{
    interface IShopRepository : IDisposable
    {

       

        List<all_products_model> get_products();

        all_products_model get_product_byId(long product_id);

        List<all_products_model> get_search_products(long? cat_id, long? brand_id, long? price_range_id, string skey);


        List<BrandModel> get_product_brand();

        List<PriceModel> get_product_price();

        List<category_model> get_category();

        IEnumerable<web_tbl_category> get_category_tree(long Category_Id);

        IEnumerable<main_category_model> get_all_category();

        long save_shopping_order(long user_id, decimal amount, List<cartpanel_model> _order_item, int paymentmode);

        long save_shopping_order(long user_id, decimal amount, List<cartpanel_model> _order_item, int paymentmode, string sPur_type);

        IEnumerable<all_orders_model> get_all_orders();

        List<all_orders_model> get_search_orders(string sOrderNo, string sPayGateway, string sEmail, string sCity, string sFromOrder, string sToOrder);

        order_summary_model get_order_summary(long order_id);

        //List<order_item_model> get_orderitemswith_products(List<web_tbl_shopping_order_item> items);

        //List<orderlist_model> get_shopping_orders(long user_id);

        List<all_orders_model> get_my_shopping_order(long user_id);

        order_summary_model get_order_summary(long order_id, long user_id);

        List<all_orders_model> get_shopping_orders();

        List<all_orders_model> get_peiezy_orders();

        EzyOrderModel GetEzyOrderDetails(long order_id);
       

    }
}
