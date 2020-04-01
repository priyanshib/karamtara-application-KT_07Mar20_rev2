using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class IntTenderPricingController : BaseController
    {
        RawMaterialPricingDAL rmpDAL = new RawMaterialPricingDAL();
        MarkupPricingDAL mDAL = new MarkupPricingDAL();
        FreightChargesDAL fDAL = new FreightChargesDAL();
        public TenderDetailsModel tndDetailsModel;
        public TenderDetailsDAL tndDetailsDAL;
        public CurrencyDAL currencyDAL;
        public IntTenderDetailsDAL intDetailsDAL = new IntTenderDetailsDAL();
        private int UserId;
        private UserModel _user;

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


        // GET: TenderStructure
        public ActionResult Index()
        {
            tndDetailsModel = new TenderDetailsModel();
            return View("~/Views/Tender/TenderPricing/TenderPricing.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult TenderDetailsDom(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {

            if (!UserExist())
                return RedirectToAction("Index", "Login");
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            //  tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return View("~/Views/intTender/IntTenderPricing_I.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult TenderDetailsDomPartial(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            //  tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return PartialView("~/Views/intTender/IntTenderPricing_I.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult ViewTenderDetailsDom(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            //  tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = false;
            return View("~/Views/intTender/IntTenderPricing_I.cshtml", tndDetailsModel);
        }

        [HttpPost]
        public ActionResult LoadCurrency(int tndId, int tndRevNo, int tndType)
        {
            var result = intDetailsDAL.GetCurrencyList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            result.TenderType = tndType;
            return PartialView("~/Views/Shared/IntTender/_Currency_I.cshtml", result);
        }

        [HttpPost]
        public JsonResult CreateTender(int enqId, int bomId, int revNo)
        {
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            int userId = GetCurrentUserId();
            var tndId = intDetailsDAL.CreateTender(enqId, bomId, revNo, 2, userId);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = 1;
            tndDetailsModel.IsEdit = true;
            return Json(tndDetailsModel);
        }

        [HttpPost]
        public ActionResult CreateTenderRev(int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            int publishStatus = 0;
            UserModel userInfo = new UserModel();
            userInfo = (UserModel)Session["UserData"];
            TempData["IsRevisionCreated"] = true;
            publishStatus = intDetailsDAL.CreateTenderRevision(tndId, tndRevId, userInfo.UserId);
            return Json(publishStatus);
        }

        [HttpPost]
        public ActionResult CancelTenderRev(int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var publishStatus = intDetailsDAL.CancelTenderRev(tndId, tndRevId);
            return Json(publishStatus);
        }

        [HttpPost]
        public JsonResult CheckPublish(int tndId, int tndRevId)
        {
            MarkupModel model = intDetailsDAL.CheckPublish(tndId, tndRevId);
            return Json(model);
        }

        [HttpPost]
        public ActionResult LoadTestPricing(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetIntTestPricingList(tndId, tndRevNo);
            return PartialView("~/Views/Shared/IntTender/_TestingMasterPrice.cshtml", data);
        }

        [HttpPost]
        public ActionResult SaveTestPricing(TestMasterModel model)
        {
            var testDal = new TestDAL();
            var data = testDal.SaveIntTestMasterPricingList(model);
            return Json(data);
        }

        [HttpPost]
        public ActionResult SaveContainerValues(int tndId, int tndRevNo, decimal dividingFactor20Ft, decimal dividingFactor40Ft, int considered20FtCntr, int considered40FtCntr,
            int dollarsPerCnt40Ft, int dollarsPerCnt20Ft)
        {
            var data = intDetailsDAL.SaveContainerValues(tndId, tndRevNo, dividingFactor20Ft, dividingFactor40Ft, considered20FtCntr, considered40FtCntr,
                dollarsPerCnt40Ft, dollarsPerCnt20Ft);
            return Json(data);
        }

        [HttpPost]
        public ActionResult SaveTenderValue(int tndId, int tndRevNo, string key,decimal value)
        {
            var result = intDetailsDAL.SaveTenderValue(tndId, tndRevNo, key, value);
            return Json(result);
        }

    }
}