using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace si_bmobile.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/
       
        public ActionResult TechIssue()
        {
            Session.Clear();
            return View();
        }
       

        public ActionResult BadRequest()
        {
            return View();
        }

        public ActionResult Forbidden()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult SessionTimeout()
        {
            return View();
        }
    }
}
