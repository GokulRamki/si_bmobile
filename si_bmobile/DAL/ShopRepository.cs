using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using si_bmobile.Models;
using System.Configuration;

using System.IO;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using si_bmobile.Utils;
using System.Text;
using Newtonsoft.Json;
using si_bmobile.dokuRef;


namespace si_bmobile.DAL
{
    public class ShopRepository : IShopRepository, IDisposable
    {

        bmtestshopDBEntities _db = new bmtestshopDBEntities();
        private IUtilityRepository _utils_repo;
        private IdokuServiceClient _dokuclient;
        UnitOfWork _uow;
        public ShopRepository()
        {

            this._utils_repo = new UtilityRepository();
            this._uow = new UnitOfWork();
            this._dokuclient = new IdokuServiceClient();
        }

        public List<category_model> get_category()
        {
            List<category_model> obj_categories = new List<category_model>();
            CategoryModel main_category = new CategoryModel();

            obj_categories = (from parent_cat in _db.category.AsEnumerable()
                              where parent_cat.IsDeleted == false && parent_cat.Parent_ID == 0
                              select new category_model
                              {
                                  parent_category = parent_cat,
                                  parent_product_count = get_productparentcategory_count(parent_cat.Category_ID),
                                  sub_category = get_sub_category(parent_cat.Category_ID)
                              }).GroupBy(cust => cust.parent_category.Category_ID).Select(grp => grp.First()).Where(m => m.parent_product_count > 0).ToList();

            return obj_categories;
        }

        private List<sub_category_model> get_sub_category(long? parent_cat_id)
        {
            var sub_categories = (from cat in _db.category.AsEnumerable()
                                  join cat_conj in _db.product_category_conj.AsEnumerable() on cat.Category_ID equals cat_conj.Category_id
                                  join pro in _db.products.AsEnumerable() on cat_conj.product_id equals pro.Product_ID
                                  where pro.IsDeleted == false && cat.IsDeleted == false && cat.Parent_ID == parent_cat_id
                                  select new sub_category_model
                                  {
                                      category = cat,
                                      sub_category_count = get_productcategory_count(cat.Category_ID)
                                  }).GroupBy(cust => cust.category.Category_ID).Select(grp => grp.First()).Where(m => m.sub_category_count > 0).ToList();
            return sub_categories;
        }

        private int get_productparentcategory_count(long? parent_cat_id)
        {
            int iret = 0;

            var parent_categories = _db.category.Where(p => p.Category_ID == parent_cat_id && p.IsDeleted == false).ToList();

            var parent_product_count = (from parent_cat in parent_categories.AsEnumerable()
                                        join cat_conj in _db.product_category_conj.AsEnumerable() on parent_cat.Category_ID equals cat_conj.Category_id
                                        join pro in _db.products.AsEnumerable() on cat_conj.product_id equals pro.Product_ID
                                        where pro.IsDeleted == false
                                        select cat_conj).Count();

            if (parent_product_count > 0)
                iret = parent_product_count;

            var product_count2 = (from parent_cat in parent_categories.AsEnumerable()
                                  join cat in _db.category.AsEnumerable() on parent_cat.Category_ID equals cat.Parent_ID
                                  join cat_conj in _db.product_category_conj on cat.Category_ID equals cat_conj.Category_id
                                  join pro in _db.products on cat_conj.product_id equals pro.Product_ID
                                  where pro.IsDeleted == false && cat.IsDeleted == false
                                  select cat_conj).Count();

            if (product_count2 > 0)
                iret += product_count2;

            return iret;
        }

        private int get_productcategory_count(long category_id)
        {
            var product_count = (from cat in _db.category
                                 join cat_conj in _db.product_category_conj on cat.Category_ID equals cat_conj.Category_id
                                 join pro in _db.products on cat_conj.product_id equals pro.Product_ID
                                 where pro.IsDeleted == false && cat.IsDeleted == false && cat.Category_ID == category_id
                                 select cat_conj).Count();

            return product_count;
        }

        public List<all_products_model> get_products()
        {
            var obj_products = (from brand in _db.brands
                                join brand_conj in _db.product_brand_conj on brand.Brand_ID equals brand_conj.brand_id
                                join pro in _db.products on brand_conj.product_id equals pro.Product_ID
                                join pro_img in _db.products_img on pro.Product_ID equals pro_img.Product_ID
                                join cat_conj in _db.product_category_conj on pro.Product_ID equals cat_conj.product_id
                                join cat in _db.category on cat_conj.Category_id equals cat.Category_ID
                                where pro.IsDeleted == false && cat.IsDeleted == false && brand.IsDeleted == false
                                select new all_products_model
                                {
                                    brand = brand,
                                    category = cat,
                                    brand_conj = brand_conj,
                                    category_conj = cat_conj,
                                    product = pro,
                                    product_img = pro_img
                                }).ToList();

            return obj_products;
        }

        public all_products_model get_product_byId(long product_id)
        {
            var obj_products = (from brand in _db.brands
                                join brand_conj in _db.product_brand_conj on brand.Brand_ID equals brand_conj.brand_id
                                join pro in _db.products on brand_conj.product_id equals pro.Product_ID
                                join pro_img in _db.products_img on pro.Product_ID equals pro_img.Product_ID
                                join cat_conj in _db.product_category_conj on pro.Product_ID equals cat_conj.product_id
                                join cat in _db.category on cat_conj.Category_id equals cat.Category_ID
                                where pro.IsDeleted == false && cat.IsDeleted == false && brand.IsDeleted == false && pro.Product_ID == product_id
                                select new all_products_model
                                {
                                    brand_conj = brand_conj,
                                    category_conj = cat_conj,
                                    product = pro,
                                    product_img = pro_img
                                }).FirstOrDefault();

            return obj_products;
        }

        public List<all_products_model> get_search_products(long? cat_id, long? brand_id, long? price_range_id, string skey)
        {
            var obj_products = (from brand in _db.brands
                                join brand_conj in _db.product_brand_conj on brand.Brand_ID equals brand_conj.brand_id
                                join pro in _db.products on brand_conj.product_id equals pro.Product_ID
                                join pro_img in _db.products_img on pro.Product_ID equals pro_img.Product_ID
                                join cat_conj in _db.product_category_conj on pro.Product_ID equals cat_conj.product_id
                                join cat in _db.category on cat_conj.Category_id equals cat.Category_ID

                                where pro.IsDeleted == false && cat.IsDeleted == false && brand.IsDeleted == false
                                select new all_products_model
                                {
                                    brand_conj = brand_conj,
                                    category_conj = cat_conj,
                                    product = pro,
                                    product_img = pro_img
                                }).ToList();

            if (cat_id > 0)
            {
                var parent_category = _db.category.Where(c => c.Parent_ID == cat_id && c.IsDeleted == false).ToList();

                var subcategory_products = (from products in obj_products
                                            join parent_cat in parent_category on products.category_conj.Category_id equals parent_cat.Category_ID
                                            select products).ToList();

                obj_products = obj_products.Where(m => m.category_conj.Category_id == cat_id).ToList();

                if (subcategory_products.Count > 0)
                    obj_products.AddRange(subcategory_products);

            }

            if (brand_id > 0)
                obj_products = obj_products.Where(m => m.brand_conj.brand_id == brand_id).ToList();

            if (price_range_id > 0)
            {
                var price_range_detail = _db.price_range.Where(m => m.ID == price_range_id).FirstOrDefault();

                if (price_range_detail != null)
                    obj_products = obj_products.Where(m => m.product.Product_Price >= price_range_detail.RangeFrom && m.product.Product_Price <= price_range_detail.RangeTo).ToList();
            }

            return obj_products;
        }

        public List<CategoryModel> get_product_category(long? cat_id)
        {
            List<CategoryModel> obj_categories = new List<CategoryModel>();
            CategoryModel main_category = new CategoryModel();
            if (cat_id == 0)
            {
                var parent_categories = _db.category.Where(p => p.Parent_ID == 0).ToList();

                obj_categories = (from parent_cat in parent_categories.AsEnumerable()
                                  join cat in _db.category.AsEnumerable() on parent_cat.Category_ID equals cat.Parent_ID
                                  where cat.IsDeleted == false
                                  select new CategoryModel
                                  {
                                      parent_category = parent_cat,
                                      parent_product_count = get_productparentcategory_count(cat.Parent_ID)
                                  }).GroupBy(cust => cust.parent_category.Category_ID).Select(grp => grp.First()).ToList();
            }
            else
            {
                var parent_categories = _db.category.Where(p => p.Category_ID == cat_id && p.Parent_ID == 0).ToList();

                if (parent_categories.Count > 0)
                {
                    var parent_categort_list = (from parent_cat in parent_categories.AsEnumerable()
                                                join cat in _db.category.AsEnumerable() on parent_cat.Category_ID equals cat.Parent_ID
                                                where cat.IsDeleted == false
                                                select new CategoryModel
                                                {
                                                    parent_category = parent_cat,
                                                    parent_product_count = get_productparentcategory_count(cat.Parent_ID)
                                                }).GroupBy(cust => cust.parent_category.Category_ID).Select(grp => grp.First()).FirstOrDefault();

                    obj_categories.Add(parent_categort_list);

                    var sub_categories = (from cat in _db.category.AsEnumerable()
                                          join cat_conj in _db.product_category_conj.AsEnumerable() on cat.Category_ID equals cat_conj.Category_id
                                          join pro in _db.products.AsEnumerable() on cat_conj.product_id equals pro.Product_ID
                                          where pro.IsDeleted == false && cat.IsDeleted == false && cat.Parent_ID == cat_id
                                          select new CategoryModel
                                          {
                                              category = cat,
                                              product_count = get_productcategory_count(cat.Category_ID)
                                          }).GroupBy(cust => cust.category.Category_ID).Select(grp => grp.First()).ToList();

                    obj_categories.AddRange(sub_categories);

                }
                else
                {
                    var category = _db.category.Where(p => p.Category_ID == cat_id).FirstOrDefault();
                    var parent_category = _db.category.Where(p => p.Category_ID == category.Parent_ID).ToList();

                    main_category = (from parent_cat in parent_category.AsEnumerable()
                                     join cat in _db.category.AsEnumerable() on parent_cat.Category_ID equals cat.Parent_ID
                                     join cat_conj in _db.product_category_conj.AsEnumerable() on cat.Category_ID equals cat_conj.Category_id
                                     join pro in _db.products.AsEnumerable() on cat_conj.product_id equals pro.Product_ID
                                     where pro.IsDeleted == false && cat.IsDeleted == false
                                     select new CategoryModel
                                     {
                                         parent_category = parent_cat,
                                         parent_product_count = get_productparentcategory_count(cat.Parent_ID)
                                     }).GroupBy(cust => cust.parent_category.Category_ID).Select(grp => grp.First()).FirstOrDefault();

                    if (main_category != null)
                    {
                        obj_categories.Add(main_category);

                        var sub_categories = (from cat in _db.category.AsEnumerable()
                                              join cat_conj in _db.product_category_conj.AsEnumerable() on cat.Category_ID equals cat_conj.Category_id
                                              join pro in _db.products.AsEnumerable() on cat_conj.product_id equals pro.Product_ID
                                              where pro.IsDeleted == false && cat.IsDeleted == false && cat.Category_ID == cat_id
                                              select new CategoryModel
                                              {
                                                  category = cat,
                                                  product_count = get_productcategory_count(cat.Category_ID)
                                              }).GroupBy(cust => cust.category.Category_ID).Select(grp => grp.First()).ToList();

                        obj_categories.AddRange(sub_categories);
                    }

                }

            }
            return obj_categories;
        }

        public List<BrandModel> get_product_brand()
        {
            var brandlist = (from brand in _db.brands.AsEnumerable()
                             join brand_conj in _db.product_brand_conj.AsEnumerable() on brand.Brand_ID equals brand_conj.brand_id
                             join pro in _db.products.AsEnumerable() on brand_conj.product_id equals pro.Product_ID
                             where brand.IsDeleted == false && pro.IsDeleted == false
                             select new BrandModel
                             {
                                 brand = brand,
                                 product_count = get_productbrand_count(brand.Brand_ID)
                             }).GroupBy(cust => cust.brand.Brand_ID).Select(grp => grp.First()).Where(m => m.product_count > 0).ToList();

            return brandlist;
        }

        private int get_productbrand_count(long brand_id)
        {
            var product_count = (from brand in _db.brands
                                 join brand_conj in _db.product_brand_conj on brand.Brand_ID equals brand_conj.brand_id
                                 join pro in _db.products on brand_conj.product_id equals pro.Product_ID
                                 join cat_conj in _db.product_category_conj on pro.Product_ID equals cat_conj.product_id
                                 join cat in _db.category on cat_conj.Category_id equals cat.Category_ID
                                 where brand.IsDeleted == false && pro.IsDeleted == false && cat.IsDeleted == false && brand.Brand_ID == brand_id
                                 select brand_conj).Distinct().Count();

            return product_count;
        }

        public List<PriceModel> get_product_price()
        {
            List<PriceModel> obj_price_list = new List<PriceModel>();

            var price_range_list = _db.price_range.ToList();
            foreach (var item in price_range_list)
            {

                var productcount = (from brand in _db.brands
                                    join brand_conj in _db.product_brand_conj on brand.Brand_ID equals brand_conj.brand_id
                                    join pro in _db.products on brand_conj.product_id equals pro.Product_ID
                                    join pro_img in _db.products_img on pro.Product_ID equals pro_img.Product_ID
                                    join cat_conj in _db.product_category_conj on pro.Product_ID equals cat_conj.product_id
                                    join cat in _db.category on cat_conj.Category_id equals cat.Category_ID
                                    where pro.IsDeleted == false && cat.IsDeleted == false && brand.IsDeleted == false
                                    && pro.Product_Price >= item.RangeFrom && pro.Product_Price <= item.RangeTo
                                    select pro).Count();
                if (productcount > 0)
                {
                    obj_price_list.Add(new PriceModel
                    {
                        ID = item.ID,
                        Description = item.Description,
                        RangeFrom = item.RangeFrom,
                        RangeTo = item.RangeTo,
                        product_count = productcount,
                    });
                }
            }

            obj_price_list = obj_price_list.Where(p => p.product_count > 0).ToList();

            return obj_price_list;
        }

        public IEnumerable<web_tbl_category> get_category_tree(long Category_Id)
        {
            List<web_tbl_category> category_treelist = new List<web_tbl_category>();

            var objCartegories = (from cat in _db.category
                                  where cat.Category_ID == Category_Id && cat.IsDeleted == false
                                  select cat).ToList();
            foreach (web_tbl_category pcategory in objCartegories)
            {
                category_treelist.Add(pcategory);
                sub_category(pcategory.Category_ID, category_treelist, objCartegories);
            }
            return category_treelist;
        }

        private IEnumerable<web_tbl_category> sub_category(long PCategory_Id, List<web_tbl_category> category_treelst, List<web_tbl_category> categories_source)
        {
            var objCartegoy = categories_source.Where(C => C.Parent_ID == PCategory_Id).ToList();
            foreach (web_tbl_category scategory in objCartegoy)
            {
                category_treelst.Add(scategory);
                if (scategory.Parent_ID > 0)
                    sub_category(scategory.Category_ID, category_treelst, categories_source);
            }

            return category_treelst;
        }

        private string UniqueTransactionNumber(string rnstring)
        {
            string res = "";
            var order_payment = _uow.shopping_order_payment_repo.Get(filter: p => p.payment_transaction_number == rnstring).ToList();
            if (order_payment.Count > 0)
            {
                res = _utils_repo.GetRandomNumber(8);
                res = UniqueTransactionNumber(res);
            }
            else
                res = rnstring;
            return res;

        }



        public IEnumerable<main_category_model> get_all_category()
        {
            List<main_category_model> category_treelist = new List<main_category_model>();

            var objCartegories = (from cat in _db.category
                                  where cat.IsDeleted == false && cat.Parent_ID == 0
                                  select new main_category_model
                                  {
                                      category_id = cat.Category_ID,
                                      category_name = cat.Category_Name,
                                  }).ToList();

            category_treelist.AddRange(objCartegories);

            var sub_cartegories = (from cat in _db.category
                                   where cat.IsDeleted == false && cat.Parent_ID > 0
                                   select new main_category_model
                                   {
                                       category_id = cat.Category_ID,
                                       category_name = cat.Category_Name,
                                       parent_cat_id = _db.category.Where(m => m.Category_ID == cat.Parent_ID).FirstOrDefault().Category_ID,
                                       parent_cat_name = _db.category.Where(m => m.Category_ID == cat.Parent_ID).FirstOrDefault().Category_Name,
                                   }).ToList();

            category_treelist.AddRange(sub_cartegories);

            return category_treelist;
        }

        public long save_shopping_order(long user_id, decimal amount, List<cartpanel_model> _order_item, int paymentmode)
        {
            long iRes = 0;
            try
            {
                web_tbl_shopping_order _SO = new web_tbl_shopping_order();
                _SO.order_number = "0";
                _SO.order_product_total = amount;
                _SO.user_id = user_id;
                _SO.order_freight_total = 0;
                _SO.order_surcharge = 0;
                _SO.order_datetime = DateTime.Now;
                _SO.order_city_id = 0;
                _SO.order_store_id = 1;
                _SO.order_is_delivery = false;
                _SO.payment_mode = paymentmode;
                _SO.order_id = 0;

                _SO.order_total = amount;


                _SO.order_number = user_id.ToString()+ DateTime.Now.ToString("yyyyMMddssfff");

                _uow.shopping_order_repo.Insert(_SO);
                _uow.Save();
                if (_SO.order_id > 0)
                {
                    //if (_order_topup_item.Count > 0)
                    //{
                    //    decimal total_topup_amt = 0;
                    //    bool bRes = false;
                    //    save_order_topup(_order_topup_item, _SO.order_id, out bRes, out total_topup_amt);

                    //    if (bRes == true && total_topup_amt > 0)
                    //    {
                    //        _SO.order_product_total = _SO.order_product_total + total_topup_amt;
                    //        amount = amount + total_topup_amt;
                    //        _uow.shopping_order_repo.Update(_SO);
                    //        _uow.Save();
                    //    }
                    //}

                    bool bOrder_itemRes = false;

                    if (_order_item.Count > 0)
                    {
                        bOrder_itemRes = save_order_item(_order_item, _SO.order_id);
                    }

                    if (bOrder_itemRes == true && _SO.order_id > 0 && _SO.user_id > 0)
                    {

                        var ordered_user = _uow.shopping_user_contact_repo.Get(filter: (m => m.user_id == _SO.user_id)).FirstOrDefault();

                        string sRes_user = JsonConvert.SerializeObject(ordered_user);

                        var mode = _uow.payment_mode_repo.GetByID(paymentmode);
                        string pg_type = "";
                        if (mode.ID == 1)
                            pg_type = "CASH";
                        else if (mode.ID == 3)
                            pg_type = "EZYTRANS";
                        //else if (mode.ID == 2)
                        //    pg_type = "CREDITCARD";

                        web_tbl_shopping_order_payment _SOP = new web_tbl_shopping_order_payment();

                        _SOP.order_id = _SO.order_id;
                        _SOP.payment_datetime = DateTime.Now;
                        _SOP.payment_total = amount;
                        _SOP.payment_transaction_number = UniqueTransactionNumber(_utils_repo.GetRandomNumber(8));
                        _SOP.payment_gateway = pg_type;
                        _SOP.payment_status = "PENDING";
                        _SOP.payment_type = mode.Payment_Mode;
                        _SOP.payment_response = sRes_user;

                        _uow.shopping_order_payment_repo.Insert(_SOP);
                        _uow.Save();
                        if (_SOP.payment_id > 0)
                            iRes = _SO.order_id;
                    }


                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return iRes;

        }


        public long save_shopping_order(long user_id, decimal amount, List<cartpanel_model> _order_item, int paymentmode, string sPur_type)
        {
            long iRes = 0;
            try
            {
                web_tbl_shopping_order _SO = new web_tbl_shopping_order();
                _SO.order_number = "0";
                _SO.order_product_total = amount;
                _SO.user_id = user_id;
                _SO.order_freight_total = 0;
                _SO.order_surcharge = 0;
                _SO.order_datetime = DateTime.Now;
                _SO.order_city_id = 0;
                _SO.order_store_id = 1;
                _SO.order_is_delivery = false;
                _SO.delivery_status = false;
                _SO.payment_mode = paymentmode;
                _SO.order_id = 0;

                _SO.order_total = amount;


                _SO.order_number =user_id.ToString()+ DateTime.Now.ToString("yyyyMMddssfff");

                _uow.shopping_order_repo.Insert(_SO);
                _uow.Save();
                if (_SO.order_id > 0)
                {
                    //if (_order_topup_item.Count > 0)
                    //{
                    //    decimal total_topup_amt = 0;
                    //    bool bRes = false;
                    //    save_order_topup(_order_topup_item, _SO.order_id, out bRes, out total_topup_amt);

                    //    if (bRes == true && total_topup_amt > 0)
                    //    {
                    //        _SO.order_product_total = _SO.order_product_total + total_topup_amt;
                    //        amount = amount + total_topup_amt;
                    //        _uow.shopping_order_repo.Update(_SO);
                    //        _uow.Save();
                    //    }
                    //}

                    bool bOrder_itemRes = false;

                    if (_order_item.Count > 0)
                    {
                        bOrder_itemRes = save_order_item(_order_item, _SO.order_id);
                    }

                    if (bOrder_itemRes == true && _SO.order_id > 0 && _SO.user_id > 0)
                    {

                        var ordered_user = _uow.shopping_user_contact_repo.Get(filter: (m => m.user_id == _SO.user_id)).FirstOrDefault();

                        string sRes_user = "";
                        if (ordered_user != null)
                            sRes_user = JsonConvert.SerializeObject(ordered_user);

                        var mode = _uow.payment_mode_repo.GetByID(paymentmode);
                        string pg_type = "";
                        if (mode.ID == 1)
                            pg_type = "CASH";
                        else if (mode.ID == 3)
                            pg_type = "EZYTRANS";
                        //else if (mode.ID == 2)
                        //    pg_type = "CREDITCARD";

                        web_tbl_shopping_order_payment _SOP = new web_tbl_shopping_order_payment();

                        _SOP.order_id = _SO.order_id;
                        _SOP.payment_datetime = DateTime.Now;
                        _SOP.payment_total = amount;
                        _SOP.payment_transaction_number = UniqueTransactionNumber(_utils_repo.GetRandomNumber(8));
                        _SOP.payment_gateway = pg_type;
                        _SOP.payment_status = "PENDING";
                        _SOP.payment_type = mode.Payment_Mode;
                        _SOP.payment_response = sRes_user;
                        _SOP.payment_receipt_no = sPur_type;
                        _uow.shopping_order_payment_repo.Insert(_SOP);
                        _uow.Save();
                        if (_SOP.payment_id > 0)
                            iRes = _SO.order_id;
                    }


                }
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return iRes;

        }

        private bool save_order_item(List<cartpanel_model> _order_item, long order_id)
        {
            bool bRes = false;
            try
            {
                int _count = 0;
                int _success_count = 0;
                foreach (var item in _order_item)
                {
                    web_tbl_shopping_order_item order_item = new web_tbl_shopping_order_item();
                    _count++;
                    order_item.order_id = order_id;
                    order_item.product_id = item.product_id;
                    order_item.product_name = item.product_name;
                    order_item.product_price = item.product_price;
                    order_item.product_qty = item.product_qty;
                    order_item.product_sku = item.product_sku;
                    //order_item.product_sku = item.product_model;
                    order_item.product_shipping_matrix_id = 1;
                    _uow.shopping_order_item_repo.Insert(order_item);
                    _uow.Save();

                    if (order_item.order_id > 0 && item.is_topup == true && item.topup_amt > 0 && item.sim_qty > 0)
                    {
                        web_tbl_shopping_order_item topup_item = new web_tbl_shopping_order_item();

                        order_item.order_id = order_id;
                        order_item.product_id = item.product_id;
                        order_item.product_name = "SIM Topup Charges";
                        order_item.product_price = item.topup_amt;
                        order_item.product_qty = item.sim_qty;
                        //order_item.product_sku = item.product_sku;
                        order_item.product_shipping_matrix_id = 0;
                        _uow.shopping_order_item_repo.Insert(order_item);
                        _uow.Save();
                    }

                    if (order_item.order_id > 0)
                        _success_count++;

                }

                if (_count == _success_count)
                    bRes = true;
            }
            catch (Exception ex)
            {
                _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return bRes;
        }

        //private void save_order_topup(List<TopupProductModel> _order_topup_item, long order_id, out bool result, out decimal total_topup)
        //{
        //    result = false;
        //    total_topup = 0;
        //    try
        //    {
        //        int topup_count = 0;
        //        int topupsuccess_count = 0;
        //        decimal topupamt = 0;

        //        foreach (var topup_item in _order_topup_item)
        //        {
        //            topup_count++;
        //            tbl_order_topup orderTP = new tbl_order_topup();
        //            orderTP.order_id = order_id;
        //            orderTP.product_id = topup_item.product_id;
        //            orderTP.topup_amount = topup_item.topup_amt;
        //            _uow.order_topup_repo.Insert(orderTP);
        //            _uow.Save();
        //            if (orderTP.id > 0)
        //            {
        //                topupsuccess_count++;
        //                topupamt += orderTP.topup_amount;
        //            }
        //        }

        //        if (topup_count == topupsuccess_count)
        //        {
        //            total_topup = topupamt;
        //            result = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _utils_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
        //    }
        //}


        public IEnumerable<all_orders_model> get_all_orders()
        {
            var orders = (from o in _db.shopping_order
                          join op in _db.shopping_order_payment on o.order_id equals op.order_id
                          join su in _db.shopping_user on o.user_id equals su.user_id
                          join suc in _db.shopping_user_contact on su.user_id equals suc.user_id
                          where su.is_approved == true
                          select new all_orders_model
                          {
                              order = o,
                              order_pay = op,
                              ordered_user = suc
                          }).ToList();
            return orders;
        }

        public IEnumerable<all_orders_model> get_all_orders_withdoku()
        {
            var orders = (from o in _db.shopping_order
                          join op in _db.shopping_order_payment on o.order_id equals op.order_id
                          join su in _db.shopping_user on o.user_id equals su.user_id
                          join suc in _db.shopping_user_contact on su.user_id equals suc.user_id
                         // join doku in _db.doku_selfccare on o.order_id equals doku.order_id
                          where su.is_approved == true //&& doku.site_id == 2
                          select new all_orders_model
                          {
                              order = o,
                              order_pay = op,
                              ordered_user = suc
                              //doku_selfcare = doku
                          }).ToList();
            return orders;
        }

        public List<all_orders_model> get_search_orders(string sOrderNo, string sPayGateway, string sEmail, string sCity, string sFromOrder, string sToOrder)
        {
            List<all_orders_model> obj_orders = new List<all_orders_model>();

            //obj_orders = (from o in _db.shopping_order
            //              join op in _db.shopping_order_payment on o.order_id equals op.order_id
            //              join su in _db.shopping_user on o.user_id equals su.user_id
            //              join suc in _db.shopping_user_contact on su.user_id equals suc.user_id
            //              //join doku in _db.doku_selfccare.AsEnumerable() on o.order_id equals doku.order_id into 
            //              //doku_new from d in doku_new.DefaultIfEmpty()
            //              where su.is_approved == true
            //              select new all_orders_model
            //              {
            //                  order = o,
            //                  order_pay = op,
            //                  ordered_user = suc,
            //                  doku_selfcare = _db.doku_selfccare.Where(m => m.site_id == 2 && m.order_id == o.order_id).FirstOrDefault()

            //              }).ToList();

            obj_orders = get_shopping_orders();


            if (obj_orders.Count > 0)
            {

                if (!string.IsNullOrEmpty(sPayGateway))
                    obj_orders = obj_orders.Where(b => b.order_pay.payment_gateway == sPayGateway).ToList();

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
            }

            return obj_orders.OrderByDescending(s => s.order.order_id).ToList();
        }



        public order_summary_model get_order_summary_old(long order_id)
        {
            order_summary_model order_details = new order_summary_model();

            order_details.order = _uow.shopping_order_repo.Get(filter: (m => m.order_id == order_id)).FirstOrDefault();
            order_details.order_pay = _uow.shopping_order_payment_repo.Get(filter: (m => m.order_id == order_id)).FirstOrDefault();

            if (order_details.order_pay != null && !string.IsNullOrEmpty(order_details.order_pay.payment_response))
                order_details.ordered_user = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(order_details.order_pay.payment_response);

            order_details.order_items = (from SOI in _db.shopping_order_item
                                         join P in _db.products on SOI.product_id equals P.Product_ID
                                         join PIMG in _db.products_img on P.Product_ID equals PIMG.Product_ID
                                         where SOI.order_id == order_id
                                         select new order_item_model
                                         {
                                             order_id = SOI.order_id,
                                             order_item_id = SOI.order_item_id,
                                             product_id = SOI.product_id,
                                             product_name = SOI.product_name,
                                             product_price = SOI.product_price,
                                             product_qty = SOI.product_qty,
                                             product_shipping_matrix_id = SOI.product_shipping_matrix_id,
                                             product_sku = SOI.product_sku,
                                             product_img = PIMG.img_medium,
                                             product_model = P.Model_No,
                                         }).ToList();
            return order_details;
        }

        public order_summary_model get_order_summary(long order_id)
        {
            order_summary_model order_details = new order_summary_model();

            all_orders_model orders = new all_orders_model();

            orders = get_shopping_orders().Where(m => m.order.order_id == order_id).FirstOrDefault();

            if (orders != null)
            {
                order_details.order_items = new List<order_item_model>();
                order_details.order = new web_tbl_shopping_order();
                order_details.order_pay = new web_tbl_shopping_order_payment();
                order_details.ordered_user = new web_tbl_shopping_user_contact();

                order_details.order = orders.order;
                order_details.order_pay = orders.order_pay;
                order_details.ordered_user = orders.ordered_user;

                //if (order_details.order_pay != null && !string.IsNullOrEmpty(order_details.order_pay.payment_response))
                //    order_details.ordered_user = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(order_details.order_pay.payment_response);

                foreach (var item in orders.order_items)
                {
                    order_item_model o_item = new order_item_model();

                    if (item != null && item.product_id > 0)
                    {
                        var product = _uow.products_repo.Get(filter: m => m.Product_ID == item.product_id).FirstOrDefault();

                        if (product != null)
                        {
                            o_item.product_id = product.Product_ID;
                            o_item.product_sku = product.SKU_Number;
                            o_item.product_model = product.Model_No;

                            var product_img = _uow.products_img_repo.Get(filter: m => m.Product_ID == item.product_id).FirstOrDefault();

                            if (product_img != null)
                                o_item.product_img = product_img.img_medium;
                        }
                    }
                    else
                        o_item.product_sku = item.product_sku;

                    o_item.order_id = item.order_id;
                    o_item.order_item_id = item.order_item_id;
                    //o_item.product_id = item.product_id;
                    o_item.product_name = item.product_name;
                    o_item.product_price = item.product_price;
                    o_item.product_qty = item.product_qty;
                    o_item.product_shipping_matrix_id = item.product_shipping_matrix_id;



                    order_details.order_items.Add(o_item);

                }

            }

            return order_details;
        }


        public order_summary_model get_order_summary(long order_id, long user_id)
        {
            order_summary_model order_details = new order_summary_model();

            all_orders_model orders = new all_orders_model();

            orders = get_my_shopping_order(user_id).Where(m => m.order.order_id == order_id).FirstOrDefault();

            if (orders != null)
            {
                order_details.order_items = new List<order_item_model>();
                order_details.order = new web_tbl_shopping_order();
                order_details.order_pay = new web_tbl_shopping_order_payment();
                order_details.ordered_user = new web_tbl_shopping_user_contact();

                order_details.order = orders.order;
                order_details.order_pay = orders.order_pay;
                order_details.ordered_user = orders.ordered_user;

                //if (order_details.order_pay != null && !string.IsNullOrEmpty(order_details.order_pay.payment_response))
                //    order_details.ordered_user = JsonConvert.DeserializeObject<web_tbl_shopping_user_contact>(order_details.order_pay.payment_response);

                foreach (var item in orders.order_items)
                {
                    order_item_model o_item = new order_item_model();

                    if (item != null && item.product_id >0)
                    {
                        var product = _uow.products_repo.Get(filter: m => m.Product_ID == item.product_id).FirstOrDefault();

                        if (product != null)
                        {
                            o_item.product_id = product.Product_ID;
                            o_item.product_sku = product.SKU_Number;
                            o_item.product_model = product.Model_No;

                            var product_img = _uow.products_img_repo.Get(filter: m => m.Product_ID == item.product_id).FirstOrDefault();

                            if (product_img != null)
                                o_item.product_img = product_img.img_medium;
                        }
                    }
                    else
                        o_item.product_sku = item.product_sku;

                    o_item.order_id = item.order_id;
                    o_item.order_item_id = item.order_item_id;
                    //o_item.product_id = item.product_id;
                    o_item.product_name = item.product_name;
                    o_item.product_price = item.product_price;
                    o_item.product_qty = item.product_qty;
                    o_item.product_shipping_matrix_id = item.product_shipping_matrix_id;



                    order_details.order_items.Add(o_item);

                }

            }

            return order_details;
        }

        public List<all_orders_model> get_my_shopping_order(long user_id)
        {
            List<all_orders_model> _orderdetails = new List<all_orders_model>();

            _orderdetails = (from O in _db.shopping_order.AsEnumerable()
                             join U in _db.shopping_user_contact.AsEnumerable() on O.user_id equals U.user_id
                             join OP in _db.shopping_order_payment.AsEnumerable() on O.order_id equals OP.order_id
                             //where O.user_id == user_id
                             select new all_orders_model
                             {
                                 order = O,
                                 order_items = _db.shopping_order_item.Where(m => m.order_id == O.order_id).ToList(),
                                 ordered_user = U,
                                 order_pay = OP
                                 //ordered_topup = _db.order_topup.Where(P => P.order_id == O.order_id).FirstOrDefault()
                             }).ToList();

            string sRes = _dokuclient.get_shopping_orderby_userid(user_id);

            if (!string.IsNullOrEmpty(sRes))
            {
                List<DokuCareModel> obj_doku_shop_order = new List<DokuCareModel>();

                obj_doku_shop_order = JsonConvert.DeserializeObject<List<DokuCareModel>>(sRes);

                if (obj_doku_shop_order != null)
                {

                    long max_order_id = _orderdetails.Max(m => m.order.order_id);

                    foreach (var item in obj_doku_shop_order)
                    {
                        all_orders_model doku_order = new all_orders_model();

                        max_order_id++;

                        doku_order.order = new web_tbl_shopping_order();
                        doku_order.order_items = new List<web_tbl_shopping_order_item>();
                        doku_order.order_pay = new web_tbl_shopping_order_payment();
                        doku_order.ordered_user = new web_tbl_shopping_user_contact();

                        doku_order.ordered_user = _uow.shopping_user_contact_repo.GetByID(item.order.user_id);

                        doku_order.order.order_id = max_order_id;

                        doku_order.order.order_datetime = item.order.order_datetime;
                        doku_order.order.order_freight_total = item.order.order_freight_total;
                        doku_order.order.order_number = item.order.order_number;
                        doku_order.order.order_product_total = item.order.order_product_total;
                        doku_order.order.order_surcharge = item.order.order_surcharge;
                        doku_order.order.payment_mode = item.order.payment_mode;
                        doku_order.order.user_id = item.order.user_id;
                        doku_order.order.order_total = item.order.order_product_total;

                        foreach (var order_item in item.orderitems)
                        {
                            web_tbl_shopping_order_item o_item = new web_tbl_shopping_order_item();

                            o_item.order_id = max_order_id;
                            o_item.order_item_id = order_item.order_item_id;
                            o_item.order_item_id = order_item.product_id;
                            o_item.product_name = order_item.product_name;
                            o_item.product_price = order_item.product_price;
                            o_item.product_qty = order_item.product_qty;
                            o_item.order_item_id = order_item.product_shipping_matrix_id;
                            o_item.product_id = order_item.product_id;

                            doku_order.order_items.Add(o_item);
                        }

                        doku_order.order_pay.order_id = max_order_id;
                        doku_order.order_pay.payment_datetime = item.orderpayment.payment_datetime;
                        doku_order.order_pay.payment_gateway = item.orderpayment.payment_gateway;
                        doku_order.order_pay.payment_id = item.orderpayment.payment_id;
                        doku_order.order_pay.payment_receipt_no = item.orderpayment.payment_receipt_no;
                        doku_order.order_pay.payment_response = item.orderpayment.payment_response;
                        doku_order.order_pay.payment_status = item.orderpayment.payment_status;
                        doku_order.order_pay.payment_total = item.orderpayment.payment_total;
                        doku_order.order_pay.payment_transaction_number = item.orderpayment.payment_transaction_number;
                        doku_order.order_pay.payment_type = item.orderpayment.payment_type;


                        _orderdetails.Add(doku_order);

                    }
                }

            }


            return _orderdetails;
        }

        public List<all_orders_model> get_peiezy_orders()
        {
            List<all_orders_model> _orderdetails = new List<all_orders_model>();

            _orderdetails = (from O in _db.shopping_order.AsEnumerable()
                             join U in _db.shopping_user_contact.AsEnumerable() on O.user_id equals U.user_id
                             join OP in _db.shopping_order_payment.AsEnumerable() on O.order_id equals OP.order_id
                             where OP.payment_gateway == "EZYTRANS"
                             select new all_orders_model
                             {
                                 order = O,
                                 order_items = _db.shopping_order_item.Where(m => m.order_id == O.order_id).ToList(),
                                 ordered_user = U,
                                 order_pay = OP,
                             }).ToList();
            return _orderdetails;
        }

        public List<all_orders_model> get_shopping_orders()
        {
            List<all_orders_model> _orderdetails = new List<all_orders_model>();

            _orderdetails = (from O in _db.shopping_order.AsEnumerable()
                             join U in _db.shopping_user_contact.AsEnumerable() on O.user_id equals U.user_id
                             join OP in _db.shopping_order_payment.AsEnumerable() on O.order_id equals OP.order_id
                             select new all_orders_model
                             {
                                 order = O,
                                 order_items = _db.shopping_order_item.Where(m => m.order_id == O.order_id).ToList(),
                                 ordered_user = U,
                                 order_pay = OP,
                             }).ToList();

            string sRes = _dokuclient.get_doku_order_transactionsBySite(2);

            if (!string.IsNullOrEmpty(sRes))
            {
                List<DokuCareModel> obj_doku_shop_order = new List<DokuCareModel>();

                obj_doku_shop_order = JsonConvert.DeserializeObject<List<DokuCareModel>>(sRes);

                if (obj_doku_shop_order != null)
                {

                    long max_order_id = _orderdetails.Max(m => m.order.order_id);

                    foreach (var item in obj_doku_shop_order)
                    {
                        all_orders_model doku_order = new all_orders_model();

                        max_order_id++;

                        doku_order.order = new web_tbl_shopping_order();
                        doku_order.order_items = new List<web_tbl_shopping_order_item>();
                        doku_order.order_pay = new web_tbl_shopping_order_payment();
                        doku_order.ordered_user = new web_tbl_shopping_user_contact();

                        doku_order.ordered_user = _uow.shopping_user_contact_repo.GetByID(item.order.user_id);

                        doku_order.order.order_id = max_order_id;

                        doku_order.order.order_datetime = item.order.order_datetime;
                        doku_order.order.order_freight_total = item.order.order_freight_total;
                        doku_order.order.order_number = item.order.order_number;
                        doku_order.order.order_product_total = item.order.order_product_total;
                        doku_order.order.order_surcharge = item.order.order_surcharge;
                        doku_order.order.payment_mode = item.order.payment_mode;
                        doku_order.order.user_id = item.order.user_id;
                        doku_order.order.order_total = item.order.order_product_total;

                        foreach (var order_item in item.orderitems)
                        {
                            web_tbl_shopping_order_item o_item = new web_tbl_shopping_order_item();

                            o_item.order_id = max_order_id;
                            o_item.order_item_id = order_item.order_item_id;
                            o_item.order_item_id = order_item.product_id;
                            o_item.product_name = order_item.product_name;
                            o_item.product_price = order_item.product_price;
                            o_item.product_qty = order_item.product_qty;
                            o_item.order_item_id = order_item.product_shipping_matrix_id;
                            o_item.product_id = order_item.product_id;

                            doku_order.order_items.Add(o_item);

                        }

                        doku_order.order_pay.order_id = max_order_id;
                        doku_order.order_pay.payment_datetime = item.orderpayment.payment_datetime;
                        doku_order.order_pay.payment_gateway = item.orderpayment.payment_gateway;
                        doku_order.order_pay.payment_id = item.orderpayment.payment_id;
                        doku_order.order_pay.payment_receipt_no = item.orderpayment.payment_receipt_no;
                        doku_order.order_pay.payment_response = item.orderpayment.payment_response;
                        doku_order.order_pay.payment_status = item.orderpayment.payment_status;
                        doku_order.order_pay.payment_total = item.orderpayment.payment_total;
                        doku_order.order_pay.payment_transaction_number = item.orderpayment.payment_transaction_number;
                        doku_order.order_pay.payment_type = item.orderpayment.payment_type;


                        _orderdetails.Add(doku_order);

                    }
                }

            }


            return _orderdetails;
        }


        //private void get_doku_shop_order(long user_id)
        //{
        //}


        //public List<orderlist_model> get_shopping_orders(long user_id)
        //{
        //    List<orderlist_model> obj_orders = new List<orderlist_model>();

        //    obj_orders = (from O in _db.shopping_order
        //                  join U in _db.shopping_user_contact on O.user_id equals U.user_id
        //                  join OP in _db.shopping_order_payment on O.order_id equals OP.order_id
        //                  where O.user_id == user_id
        //                  select new orderlist_model
        //                   {
        //                       city = U.city,
        //                       deliver_status = O.order_is_delivery == true ? "Delivered" : " Not Delivered",
        //                       email = U.email,
        //                       first_name = U.first_name,
        //                       last_name = U.last_name,
        //                       order_datetime = O.order_datetime,
        //                       order_id = O.order_id,
        //                       order_number = O.order_number,
        //                       order_total = O.order_product_total,
        //                       payment_mode = OP.payment_type,
        //                       payment_receipt_no = OP.payment_receipt_no,
        //                       payment_status = OP.payment_status,

        //                   }).ToList();

        //    string sRes = _dokuclient.get_shopping_orderby_userid(user_id);

        //    if (!string.IsNullOrEmpty(sRes))
        //    {
        //        List<DokuCareModel> obj_doku_shop_order = new List<DokuCareModel>();

        //        List<orderlist_model> obj_doku_orders = new List<orderlist_model>();

        //        web_tbl_shopping_user_contact user_details = new web_tbl_shopping_user_contact();

        //        obj_doku_shop_order = JsonConvert.DeserializeObject<List<DokuCareModel>>(sRes);

        //        if (obj_doku_shop_order != null)
        //        {
        //            user_details = _uow.shopping_user_contact_repo.GetByID(user_id);

        //            long max_order_id = obj_orders.Max(m => m.order_id);

        //            foreach (var item in obj_doku_shop_order)
        //            {
        //                orderlist_model doku_order = new orderlist_model();

        //                max_order_id++;

        //                doku_order.order_id = max_order_id;
        //                doku_order.city = user_details.city;
        //                //deliver_status = O.orderpayment. == true ? "Delivered" : " Not Delivered",
        //                doku_order.email = user_details.email;
        //                doku_order.first_name = user_details.first_name;
        //                doku_order.last_name = user_details.last_name;
        //                doku_order.order_datetime = item.order.order_datetime;
        //                doku_order.order_id = item.order.order_id;
        //                doku_order.order_number = item.order.order_number;
        //                doku_order.order_total = item.order.order_product_total;
        //                doku_order.payment_mode = item.orderpayment.payment_type;
        //                doku_order.payment_receipt_no = item.orderpayment.payment_receipt_no;
        //                doku_order.payment_status = item.orderpayment.payment_status;

        //                obj_orders.Add(doku_order);
        //            }
        //        }

        //    }



        //    return obj_orders;
        //}

        public EzyOrderModel GetEzyOrderDetails(long order_id)
        {
            EzyOrderModel result = new EzyOrderModel();
            if (order_id > 0)
            {
                result.eo = _uow.shopping_order_repo.GetByID(order_id);
                if (result.eo != null)
                {
                    result.eoi = _uow.shopping_order_item_repo.Get(o => o.order_id == order_id).ToList();
                    if (result.eoi.Count > 0)
                        result.eop = _uow.shopping_order_payment_repo.Get(op => op.order_id == order_id).FirstOrDefault();
                }
            }
            return result;
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