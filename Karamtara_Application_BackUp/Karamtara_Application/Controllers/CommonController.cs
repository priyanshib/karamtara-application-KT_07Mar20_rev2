using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class CommonController : Controller
    {
        CommonDAL comDAL;
        // GET: Common

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }
        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            return View();
        }

        [NonAction]
        public int GetUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }

        [HttpPost]
        public ActionResult DeleteMasterData(int id, int type)
        {
            comDAL = new CommonDAL();
            var userId = GetUserId();
            var result = comDAL.DeleteMasterData(id, type, userId);

            if (result > 0)
                return Json(true);
            else
                return Json(false);
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