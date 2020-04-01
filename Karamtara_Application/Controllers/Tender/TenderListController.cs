using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers.Tender
{
    public class TenderListController : Controller
    {
        public TenderListDAL tendListDal;
        // GET: TenderList

        [NonAction]
        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        [NonAction]
        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }

        public ActionResult GetDomesticTenders()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            tendListDal = new TenderListDAL();
            var data = tendListDal.GetTenderList(1);
            return View("~/Views/Tender/TenderList/DomTenderList.cshtml", data);
        }

        public ActionResult SearchDomesticTenderList(string searchText)
        {
            tendListDal = new TenderListDAL();
            var data = tendListDal.GetTenderListWithSearch(searchText);
            return PartialView("~/Views/Shared/Tender/TenderList/_DomTenderList.cshtml", data);
        }

        public ActionResult GetInternationTenders()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            tendListDal = new TenderListDAL();
            var data = tendListDal.GetTenderList(2);
            return View("~/Views/Tender/TenderList/IntlTenderList.cshtml", data);
        }

        public ActionResult SearchInternationalTenderList(string searchText)
        {
            tendListDal = new TenderListDAL();
            var data = tendListDal.GetTenderListWithSearch(searchText);
            return PartialView("~/Views/Shared/Tender/TenderList/_IntlTenderList.cshtml", data);
        }

        [HttpGet]
        public ActionResult GetDOMTndCompareList()
        {
            TenderListDAL listDAL = new TenderListDAL();
            TenderListModel listModel= new TenderListModel();
            int enqType = 1;
            listModel.TndCompareList = listDAL.GetTndCompareList(enqType);
            return PartialView("~/Views/Shared/Tender/TenderList/_TenderCompareList.cshtml", listModel);
        }

        [HttpGet]
        public ActionResult GetIntTndCompareList()
        {
            TenderListDAL listDAL = new TenderListDAL();
            TenderListModel listModel = new TenderListModel();
            int enqType = 2;
            listModel.TndCompareList = listDAL.GetTndCompareList(enqType);
            return PartialView("~/Views/Shared/Tender/TenderList/_TenderCompareList.cshtml", listModel);
        }
    }
}