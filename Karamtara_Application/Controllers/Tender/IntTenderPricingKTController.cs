using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class IntTenderPricingKTController : BaseController
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


        // GET: TenderStructure
        public ActionResult Index()
        {
            tndDetailsModel = new TenderDetailsModel();
            return View("~/Views/Tender/TenderPricing/TenderPricing.cshtml", tndDetailsModel);
        }

        [HttpPost]
        public ActionResult LoadRMPricing(int tndId, int tndRevNo)
        {
            var result = rmpDAL.GetRawPricingList(tndId, tndRevNo);
            RawMaterialPricingDetail rmModel = new RawMaterialPricingDetail();
            rmModel.RawMaterialList = result;
            rmModel.TndId = tndId;
            rmModel.TndRevNo = tndRevNo;
            return PartialView("~/Views/Shared/Tender/TenderPricing/_RawMaterialPrice.cshtml", rmModel);
        }


        public JsonResult SaveRMPricing(RawMaterialPricingDetail list)
        {
            var result = rmpDAL.SaveRawPricing(list);
            return Json(result);
        }

        [HttpPost]
        public ActionResult LoadMarkupPricing(int tndId, int tndRevNo)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            var result = intDetailsDAL.GetMarkupPricingList(tndId, tndRevNo);

            return PartialView("~/Views/Shared/IntTender/_MarkupPrice.cshtml", result);
        }

        public JsonResult SaveMarkupPricing(FormCollection form)
        {
            var result = intDetailsDAL.SaveMarkupPricing(form);
            return Json(result);
        }

        [HttpPost]
        public ActionResult LoadFreightCharges(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetFreightChargesList(tndId, tndRevNo);
            return PartialView("~/Views/Shared/IntTender/_FreightCharges.cshtml", result);
        }
        [HttpPost]
        public JsonResult SaveFreightCharges(FormCollection form)
        {
            var result = intDetailsDAL.SaveFreightCharges(form);
            return Json(result);
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
            return View("~/Views/intTender/IntTenderPricing_KT.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult ViewTenderDetailsDom(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
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
            tndDetailsModel.IsEdit = false;
            return View("~/Views/intTender/IntTenderPricing_KT.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult TndDom()
        {
            //tndDetailsModel = new TenderDetailsModel();
            //tndDetailsDAL = new TenderDetailsDAL();
            //tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            //tndDetailsModel.BomId = bomId;
            //tndDetailsModel.EnquiryId = enqId;
            //tndDetailsModel.RevisionNo = revNo;
            return View("~/Views/Tender/TenderPricing/TenderPricing.cshtml");
        }

        public JsonResult SaveTenderQty(string qtyDetails, int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();

            var result = intDetailsDAL.SaveTenderQty(tndId, tndRevId, qtyDetails);
            if (result)
            {
                var userId = GetCurrentUserId();
                tndDetailsDAL.InsertAuditTrial(userId, tndId, tndRevId, "Save");
            }
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetBGData(int tndId, int tndRevNo)
        {
            TenderBGModel bgModel = new TenderBGModel();
            tndDetailsDAL = new TenderDetailsDAL();
            bgModel.TenderNo = tndId;
            bgModel.TenderRevisionNo = tndRevNo;
            ViewData["tendorNo"] = tndId;
            ViewData["tendorRevNo"] = tndRevNo;
            var list = tndDetailsDAL.GetBGData(tndId, tndRevNo);
            return PartialView("~/Views/Shared/Tender/TenderPricing/_BGCommision.cshtml", list);
        }

        [HttpPost]
        public ActionResult SaveBGData(FormCollection form)
        {
            TenderBGModel bgModel = new TenderBGModel();
            tndDetailsDAL = new TenderDetailsDAL();
            UserModel userInfo = new UserModel();
            userInfo = (UserModel)Session["UserData"];
            var result = tndDetailsDAL.SaveBGData(form, userInfo.UserId);
            return Json(result);
        }

        [HttpPost]
        public ActionResult SaveCosts(string unitCost, int tndNo, int tndRevNo, string salesCost, string exWorks, string lineUnitCost)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var status = tndDetailsDAL.SaveUnitCost(unitCost, tndNo, tndRevNo, salesCost, exWorks, lineUnitCost);
            return Json(status);
        }

        [HttpPost]
        public ActionResult GetTestLineRelation(ParameterModel parameter)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var tndStrModel = tndDetailsDAL.GetTestRelationData(parameter);
            return PartialView("~/Views/Shared/TestMaster/_TestLineRelation.cshtml", tndStrModel);
        }

        [HttpPost]
        public ActionResult SaveTestLineRelation(TenderStructureModel model)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var result = tndDetailsDAL.SaveTestLineRelation(model);
            return Json(result);
            //return PartialView("~/Views/Shared/TestMaster/_TestLineRelation.cshtml", tndStrModel);
        }

        [HttpPost]
        public ActionResult LoadTestPricing(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetTestPricingList(tndId, tndRevNo);
            return PartialView("~/Views/Shared/Tender/TenderPricing/_TestingMasterPrice.cshtml", data);
        }

        [HttpPost]
        public ActionResult SaveTestPricing(TestMasterModel model)
        {
            var testDal = new TestDAL();
            var data = testDal.SaveTestMasterPricingList(model);
            return Json(data);
        }

        [HttpPost]
        public ActionResult GetFinalPrices(int tndId, int tndRevNo)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsModel = tndDetailsDAL.GetFinalPrices(tndId, tndRevNo);
            return PartialView("~/Views/Shared/Tender/TenderPricing/_FinalPrices.cshtml", tndDetailsModel);
        }

        [HttpPost]
        public ActionResult PublishTender(int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            int publishStatus = 0;
            publishStatus = tndDetailsDAL.PublishTender(tndId, tndRevId);
            if (publishStatus > 0)
            {
                var userId = GetCurrentUserId();
                tndDetailsDAL.InsertAuditTrial(userId, tndId, tndRevId, "Publish");
            }
            return Json(publishStatus);
        }

        [HttpPost]
        public ActionResult GetAuditTrial(int tndId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsModel.AuditTrialList = tndDetailsDAL.GetAuditTrialDetails(tndId);
            return PartialView("~/Views/Shared/Tender/TenderPricing/_AuditTrial.cshtml", tndDetailsModel);
        }

        [HttpPost]
        public ActionResult LoadTextDetails(int tndId, int tndRevNo)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsModel.TextList = tndDetailsDAL.GetTextDetails(tndId, tndRevNo);
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            return PartialView("~/Views/Shared/IntTender/_Text.cshtml", tndDetailsModel);
        }

        public JsonResult SaveTextDetails(string message, int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.SaveTextDetails(tndId, tndRevNo, message);
            return Json(result);
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
        public ActionResult LoadCurrency(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetCurrencyList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            return PartialView("~/Views/Shared/IntTender/_Currency.cshtml", result);
        }

        public JsonResult SaveCurrency(string currencies, int tndId, int tndRevNo, decimal conversionRate, int CurrId)
        {
            var result = intDetailsDAL.SaveCurrency(tndId, tndRevNo, currencies, conversionRate, CurrId);
            return Json(result);
        }

        [HttpPost]
        public ActionResult LoadAssignPort(int tndId, int tndRevNo)
        {
            currencyDAL = new CurrencyDAL();
            var portDAL = new PortDAL();
            AssignPortModel assignPortModel = portDAL.GetPorts(tndId, tndRevNo);
            assignPortModel.TndId = tndId;
            assignPortModel.TndRevNo = tndRevNo;
            assignPortModel.CurrencyList = currencyDAL.GetList();
            return PartialView("~/Views/Shared/IntTender/_AssignPort.cshtml", assignPortModel);
        }

        [HttpPost]
        public ActionResult SavePortDetails(FormCollection assignPortForm)
        {
            int userId = GetCurrentUserId();
            var result = intDetailsDAL.SavePortDetails(assignPortForm, userId);
            return Json(result);
        }

        [HttpPost]
        public JsonResult CreateTender(int enqId, int bomId, int revNo)
        {
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            int userId = GetCurrentUserId();
            var tndId = intDetailsDAL.CreateTender(enqId, bomId, revNo, 1, userId);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = 1;
            tndDetailsModel.IsEdit = true;
            return Json(tndDetailsModel);
        }

        [HttpPost]
        public JsonResult CheckPublish(int tndId, int tndRevId)
        {
            MarkupModel model = intDetailsDAL.CheckPublish(tndId, tndRevId);
            return Json(model);
        }

    }
}