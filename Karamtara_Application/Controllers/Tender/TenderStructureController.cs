using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System.Web.Mvc;
using System.Web.Routing;

namespace Karamtara_Application.Controllers
{
    public class TenderStructureController : BaseController
    {
        public TenderStructureDAL strDal;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        // GET: TenderStructure
        public ActionResult Index(int enqId,int bomId,int revNo)
         {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            TenderStructureModel tndStrModel = new TenderStructureModel();
            strDal = new TenderStructureDAL();
            var userId = GetCurrentUserId();
            bool strExists = strDal.CheckIfStrExists(enqId, bomId, revNo);
            tndStrModel = strDal.CreateTenderId(enqId, bomId, revNo, userId);
            if (strExists)
            {
                var routeValues = new RouteValueDictionary { {"enqId",tndStrModel.EnquiryId },{ "bomId",tndStrModel.BomId},
                    { "revNo",tndStrModel.RevisionNo},{"tndId",tndStrModel.TenderId },{"tndRevNo",tndStrModel.TenderRevisionNo } };
                return RedirectToAction("TenderDetailsDom", "TenderPricing", routeValues);
            }
            else
            {
                return View("~/Views/Tender/TenderStructure/TenderStructure.cshtml", tndStrModel);
            }
        }

        [HttpPost]
        public ActionResult SaveStructure(FormCollection formData)
        {
            int status = 0;
            strDal = new TenderStructureDAL();
            var userId = GetCurrentUserId();
            status = strDal.CreateStructure(formData,userId);
            if(status > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpGet]
        public ActionResult GetDetailStr(int enqId,int tndId)
        {
            TenderStructureModel tndStrModel = new TenderStructureModel();
            strDal = new TenderStructureDAL();
            tndStrModel = strDal.GetStructureDetails(enqId,tndId);
            return PartialView("~/Views/Shared/Tender/TenderStructure/_DetailedStructure.cshtml", tndStrModel);
        }

        [HttpPost]
        public ActionResult SaveStrDetails(FormCollection formData)
        {
            TenderStructureModel tndStrModel = new TenderStructureModel();
            strDal = new TenderStructureDAL();
            var userId = GetCurrentUserId();
            int status = strDal.SaveTenderDetails(formData,userId);
            return Json(status);
        }

        [HttpGet]
        public ActionResult GetEditStructreDetails(int enqId)
        {
            TenderStructureModel tndStrModel = new TenderStructureModel();
            strDal = new TenderStructureDAL();
            tndStrModel = strDal.GetEditTenderStrDetails(enqId);
            tndStrModel.EnquiryId = enqId;
            return View("~/Views/Tender/TenderStructure/EditTenderStructure.cshtml", tndStrModel);

        }

        [HttpPost]
        public ActionResult UpdateTenderStr(FormCollection formData)
        {
            TenderStructureModel tndStrModel = new TenderStructureModel();
            strDal = new TenderStructureDAL();
            int status = strDal.UpdateTenderStr(formData);
            return Json(status);
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

    }
}