using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            LogoutWithoutReturn();
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(LoginModel loginCred)
        {
            try
            {
                LoginDAL dal = new LoginDAL();
                var result = dal.CheckLogin(loginCred);
                Session["UserData"] = result;
                var temp = (UserModel)Session["UserData"];
                var data = Json(result, JsonRequestBehavior.AllowGet);
                return data;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public ActionResult Logout()
        {
            Session["UserData"] = null;
            return RedirectToAction("Index");
        }

        [NonAction]
        public void LogoutWithoutReturn()
        {
            Session["UserData"] = null;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml"
            };
            filterContext.ExceptionHandled = true;
        }
    }
}