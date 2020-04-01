using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var userInfo = (Karamtara_Application.Models.UserModel)Session["UserData"];
            if (userInfo != null)
                return RedirectToAction("EnquiryList", "Enquiry", null);
            else
                return RedirectToAction("Index", "Login", null);
        }
    }
}