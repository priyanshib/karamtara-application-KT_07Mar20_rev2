using Karamtara_Application.DAL;
using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class UserController : BaseController
    {
        private UserDAL userDAL;
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

            userDAL = new UserDAL();
            UserModel userModel = new UserModel();
            var userList = userDAL.GetAllUsers();
            userModel.UserList = userList;
            userModel.UserTypeList = userDAL.GetUserTypes();
            return View("/Views/User/UserView.cshtml", userModel);
        }

        [HttpPost]
        public ActionResult CreateUsers(FormCollection form)
        {
            userDAL = new UserDAL();
            List<string> failed = new List<string>();
            var status = userDAL.CreateUser(form, out failed);
            //var failedIds = string.Join(" ,!~ ", failed);
            if(failed.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                int count = 1;
                foreach (var fail in failed)
                {
                    sb.Append(string.Format("{0}. {1}", count, fail));
                    sb.AppendLine();
                    count++;
                }
                return Json(sb.ToString());
            }
            else
            {
                return Json(true);
            }
        }

        [HttpPost]
        public ActionResult ChangeUserActivation(int userId, bool IsEnabled)
        {
            userDAL = new UserDAL();
            UserModel userModel = new UserModel();

            var status = userDAL.ChangeUserActivation(userId, IsEnabled);

            if(status > 0)
                userModel.UserList = userDAL.GetAllUsers();

            return Json(new
            {
                Status = status,
                AjaxReturn = ViewToString.RenderRazorViewToString(this, "~/Views/Shared/User/_UserList.cshtml", userModel)
            });
        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    if (filterContext.ExceptionHandled)
        //    {
        //        return;
        //    }
        //    filterContext.Result = new ViewResult
        //    {
        //        ViewName = "~/Views/Shared/Error.cshtml"
        //    };
        //    filterContext.ExceptionHandled = true;
        //}
    }
}