using Karamtara_Application.DAL;
using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class ManagePasswordController : Controller
    {
        public ManagePassDAL managePass = new ManagePassDAL();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string emailId)
        {
            int status = 0;
            if (string.IsNullOrEmpty(emailId))
                return Json(-2, JsonRequestBehavior.AllowGet);

            try
            {
                status = managePass.forgotPassword(emailId);

                if (status == 1)
                    managePass.GetUserId(emailId);

                if (status == 1)
                    return Json(status);
                else
                    return Json(status);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult ResetPassword(string UserId)
        {
            try
            {
                ResetPasswordModel model = new ResetPasswordModel();
                model = managePass.GetMailId(UserId);
                if (string.IsNullOrEmpty(model.Receiver))
                {
                    return View("InvalidView");
                }
                else
                    return View(model);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(string UserId, string newPassword, string ConfirmPassword, string ResetPassCode)
        {
            int IsPasswordChange = 0;
            ResetPasswordModel model = new ResetPasswordModel();
            try
            {
                if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(ConfirmPassword))
                    return Json(-1);

                if (newPassword != ConfirmPassword)
                    return Json(-2);

                if (newPassword.Length < 8)
                    return Json(-3);

                var hashedPassword = PasswordHasher(newPassword);
                IsPasswordChange = managePass.ResetPassword(UserId, newPassword, hashedPassword, ResetPassCode);
                return Json(IsPasswordChange);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string PasswordHasher(string password)
        {
            PasswordHash hasher = new PasswordHash(password);
            var hashedArray = hasher.ToArray();
            return Convert.ToBase64String(hashedArray);
        }

    }
}