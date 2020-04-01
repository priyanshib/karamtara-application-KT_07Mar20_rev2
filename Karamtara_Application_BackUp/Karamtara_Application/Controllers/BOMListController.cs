using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class BOMListController : Controller
    {
        public BOMListDAL bomListDal;
        public BOMListModel bomListModel;
        // GET: BOMList

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

            bomListModel = new BOMListModel();
            bomListDal = new BOMListDAL();
            bomListModel = bomListDal.GetBOMList();
            return View("BomList",bomListModel);
        }

        public ActionResult SearchBOMList(string prefix)
        {
            bomListModel = new BOMListModel();
            bomListDal = new BOMListDAL();
            bomListModel = bomListDal.SearchBOMList(prefix);
            return PartialView("~/Views/Shared/BOMList/_BomList.cshtml", bomListModel);
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = this.RedirectToAction("GenError", "Error");
            filterContext.ExceptionHandled = true;
        }
    }
}