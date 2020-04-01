using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class SignupController : Controller
    {
        // GET: Signup
        public ActionResult Index()
        {
            return View("~/Views/Signup/Signup.cshtml");
        }

        public ActionResult SignupUser(UserModel model)
        {
            SignupDAL dal = new SignupDAL();
            if(!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword) && !string.IsNullOrEmpty(model.SignUpCode))
            {
                if(model.Password != model.ConfirmPassword)
                {
                    return Json(-3);
                }

                var status = dal.Signup(model);

                if (status == 0)
                    return Json(0);
                if (status < 0)
                    return Json(-1);
                else
                    return Json(1);
            }
            else
                return Json(-4);
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