using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class BOMListController : BaseController
    {
        public BOMListDAL bomListDal;
        public BOMListModel bomListModel;
        // GET: BOMList

        [NonAction]
        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }


        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            bomListModel = new BOMListModel();
            bomListDal = new BOMListDAL();
            var userId = GetCurrentUserId();
            bomListModel = bomListDal.GetBOMList(userId);
            return View("BomList", bomListModel);
        }

        public ActionResult SearchBOMList(string prefix)
        {
            bomListModel = new BOMListModel();
            bomListDal = new BOMListDAL();
            var userId = GetCurrentUserId();
            bomListModel = bomListDal.SearchBOMList(prefix, userId);
            return PartialView("~/Views/Shared/BOMList/_BomList.cshtml", bomListModel);
        }
        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    if (filterContext.ExceptionHandled)
        //    {
        //        return;
        //    }
        //    filterContext.Result = this.RedirectToAction("GenError", "Error");
        //    filterContext.ExceptionHandled = true;
        //}
    }
}