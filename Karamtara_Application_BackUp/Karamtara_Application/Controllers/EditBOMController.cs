using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class EditBOMController : Controller
    {
        public EditBOMDAL editBOMDAL;
        public CreateBOMModel bomModel;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        [HttpGet]
        public ActionResult EditBOM(int bomId, int revNo)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            bomModel = new CreateBOMModel();
            editBOMDAL = new EditBOMDAL();
            bomModel = editBOMDAL.GetEditBomData(bomId, revNo);
            return View("EditBOM", bomModel);
        }

        [HttpPost]
        public ActionResult RemoveProduct(int bomId,int revNo,int prodId,int prodType)
        {
            bomModel = new CreateBOMModel();
            editBOMDAL = new EditBOMDAL();
            int status = 0;
            status = editBOMDAL.RemoveProduct(bomId, revNo, prodId, prodType);
            return Json(status);
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