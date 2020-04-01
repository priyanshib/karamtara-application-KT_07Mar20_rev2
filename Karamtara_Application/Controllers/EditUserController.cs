using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Karamtara_Application.Models;
using Karamtara_Application.DAL;

namespace Karamtara_Application.Controllers
{
    public class EditUserController : Controller
    {
        private int UserId;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
            {
                UserId = userInfo.UserId;
                return true;
            }
            else
                return false;
        }

        // GET: EditUser
        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            EditUserDAL EUser = new EditUserDAL();
            EditUserDetails details = new EditUserDetails();
            int Id = GetCurrentUserId();
            details = EUser.GetUser(Id);
            return View(details);
        }

        [HttpPost]
        public ActionResult SaveDetails(EditUserDetails user)
        {
            EditUserDAL editDAL = new EditUserDAL();
            int success = editDAL.UpdateDetails(user);

            if (success > 0)
            {
                TempData["Status"] = "Success";
                if(!string.IsNullOrEmpty(user.Password) && !string.IsNullOrEmpty(user.ConfirmPassword) && string.Equals(user.Password, user.ConfirmPassword))
                TempData["PasswordChanged"] = true;
            }
            else
            {
                TempData["Status"] = "Failed";
            }
            return RedirectToAction("Index");
        }

        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }
    }
}