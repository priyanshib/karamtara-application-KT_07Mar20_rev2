using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System.Data;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class TenderPricingController : BaseController
    {
        RawMaterialPricingDAL rmpDAL = new RawMaterialPricingDAL();
        MarkupPricingDAL mDAL = new MarkupPricingDAL();
        FreightChargesDAL fDAL = new FreightChargesDAL();
        public TenderDetailsModel tndDetailsModel;
        public TenderDetailsDAL tndDetailsDAL;

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
        public ActionResult LoadMarkupPricing(int bomId, int revId, int tndId, int tndRevNo)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            var result = mDAL.GetMarkupPricingList(bomId, revId, tndId, tndRevNo);
            if (result.Tables.Count > 1)
            {
                TempData["TenderLineValues"] = mDAL.ToDynamicList(result.Tables[1]);
                TempData["TenderType"] = result.Tables[2].Rows[0][0].ToString();
            }
            return PartialView("~/Views/Shared/Tender/TenderPricing/_MarkupPrice.cshtml", result.Tables[0]);
        }

        public JsonResult SaveMarkupPricing(string values, int bomId, int revId, int tndId, int tndRevNo, string marginValues,
            string developement, string finalTotalArray, string percToUnitCostArray, string travelLB, string testing,
            string interestRate, string finSalesDays, string finMfgDays, string intSavingAdvDays, string intSavingAdvMnths, string testingRemarks, string travelLBValues)
        {
            var result = mDAL.SaveMasterPricing(values, bomId, revId, tndId, tndRevNo, marginValues, developement, finalTotalArray, percToUnitCostArray, travelLB,
                testing, interestRate, finSalesDays, finMfgDays, intSavingAdvDays, intSavingAdvMnths, testingRemarks, travelLBValues);
            return Json(result);
        }

        [HttpPost]
        public ActionResult LoadFreightCharges(int bomId, int revId, int tndId, int tndRevNo)
        {
            var result = fDAL.GetFreightChargesList(bomId, revId, tndId, tndRevNo);
            if (result.Tables.Count > 1)
            {
                result.Tables.RemoveAt(result.Tables.Count - 1);
                TempData["TruckMetricTypes"] = new SelectList(result.Tables[result.Tables.Count - 1].AsDataView(), "id", "name");
                result.Tables.RemoveAt(result.Tables.Count - 1);
            }
            return PartialView("~/Views/Shared/Tender/TenderPricing/_FreightCharges.cshtml", result);
        }

        public JsonResult SaveFreightCharges(string values, int bomId, int revId, int tndId, int tndRevNo, string lineTruckDt,
            string lineContingency, string lineTotFreights, string lineLoadingFactors, string lineUnitFreight)
        {
            var result = fDAL.SaveFreightCharges(values, bomId, revId, tndId, tndRevNo, lineTruckDt, lineContingency, lineTotFreights, lineLoadingFactors, lineUnitFreight);
            return Json(result);
        }

        [HttpGet]
        public ActionResult TenderDetailsDom(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            //if (!UserExist())
            //    return RedirectToAction("Index", "Login");
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            //  tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.TenderStrName = tndDetailsDAL.GetTenderStrDetails(tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return View("~/Views/Tender/TenderPricing/TenderPricing.cshtml", tndDetailsModel);
        }

        [HttpGet]
        public ActionResult ViewTenderDetailsDom(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            //if (!UserExist())
            //    return RedirectToAction("Index", "Login");
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
            return View("~/Views/Tender/TenderPricing/TenderPricing.cshtml", tndDetailsModel);
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

        public JsonResult SaveLineQty(string values, string grWt, int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();

            var result = tndDetailsDAL.SaveLineQty(values, grWt);
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
            publishStatus = tndDetailsDAL.CreateTenderRevision(tndId, tndRevId, userInfo.UserId);
            return Json(publishStatus);
        }

        [HttpPost]
        public ActionResult CancelTenderRev(int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var publishStatus = tndDetailsDAL.CancelTenderRev(tndId, tndRevId);
            return Json(publishStatus);
        }

        [HttpPost]
        public JsonResult CheckPublish(int tndId, int tndRevId)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            MarkupModel model = tndDetailsDAL.CheckPublish(tndId, tndRevId);
            return Json(model);
        }

        [HttpPost]
        public ActionResult SaveFinalPrices(int tndId, int tndRevNo,decimal GstValue)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var publishStatus = tndDetailsDAL.SaveTenderValue(tndId, tndRevNo, "GSTPercentage", GstValue);
            return Json(publishStatus);
        }

    }
}