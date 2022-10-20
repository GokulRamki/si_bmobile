using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace si_bmobile
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "Products",
            //    "Shop/Products/{id}/{cid}/{bid}/{prid}/{skey}",
            //    new { controller = "Shop", action = "Products", id = UrlParameter.Optional, cid = UrlParameter.Optional, bid = UrlParameter.Optional, prid = UrlParameter.Optional, skey = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );            
        }
    }
}