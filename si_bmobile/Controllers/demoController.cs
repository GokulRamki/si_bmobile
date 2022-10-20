using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using si_bmobile.Models;

namespace si_bmobile.Controllers
{
    public class demoController : Controller
    {
        //
        // GET: /demo/
       // bemobileEntities _dbcontext = new bemobileEntities();
        public ActionResult Index()
        {
            //var test = (from b in _dbcontext.bundles select new { b.PlanName, b.Price, b.Size }).ToList();
            return View();
        }

     
    }
}
