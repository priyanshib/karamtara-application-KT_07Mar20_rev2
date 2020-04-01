using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.HelperClass;
using Karamtara_Application.Models.Tender;
using System.Data;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers.Tender
{
    public class ReportController : BaseController
    {
        ReportDAL reportDAL;
        RawMaterialPricingDAL rmpDAL = new RawMaterialPricingDAL();
        IntTenderDetailsDAL intDetailsDAL = new IntTenderDetailsDAL();
        MarkupPricingDAL mDAL = new MarkupPricingDAL();
        FreightChargesDAL fDAL = new FreightChargesDAL();
        public TenderDetailsDAL tndDetailsDAL;
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TenderTonnage(int tndId)
        {
            reportDAL = new ReportDAL();
            var result = reportDAL.GetTenderTonnage(tndId);
            return PartialView("~/Views/Shared/Report/TenderReport.cshtml", result);
        }

        public ActionResult ProductGrpTonnage(int tndId, int tndRevNo)
        {
            reportDAL = new ReportDAL();
            var result = reportDAL.GetProductGrpTonnage(tndId, tndRevNo);
            return PartialView("~/Views/Shared/Report/ProductGroupwiseReport.cshtml", result);
        }

        public ActionResult GetTndRawMaterialPricing(int tndId)
        {
            reportDAL = new ReportDAL();
            var result = reportDAL.GetTndRawMaterialPricing(tndId);
            return PartialView("~/Views/Shared/Report/TndRevRMPricingReport.cshtml", result);
        }

        /// <summary>
        /// Domestic view details method
        /// </summary>
        /// <param name="bomId"></param>
        /// <param name="bomRevId"></param>
        /// <param name="tenderId"></param>
        /// <param name="tenderRevId"></param>
        /// <returns></returns>
        public ActionResult DownloadTenderPricingData(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadTenderPricingData(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadTenderPricingCustomerData(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadTenderPricingCustomerDataG(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadInternationalTenderPricingCustomerData(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadInternationalTenderPricingCustomerDataG(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadIntTenderPricingDataK(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadInternationalTenderPricingDataK(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadIntPricingDataKT(int enqId, int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadInternationalTenderPricingDataK(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadIntTenderPricingDataI(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadInternationalTenderPricingDataI(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DownloadIntTenderPricingCustomerDataI(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DownloadInternationalTenderPricingCustomerDataIG(bomId, bomRevId, tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult GetTenderTonnageReport(int tenderId)
        {
            Response.Clear();
            ReportDAL dal = new ReportDAL();
            var fileBytes = dal.GetTenderTonnageReport(tenderId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender tonnage report.xlsx");

        }

        public ActionResult GetTenderProductReport(int tenderId, int tenderRevId)
        {
            Response.Clear();
            ReportDAL dal = new ReportDAL();
            var fileBytes = dal.GetTenderTonnageProductWise(tenderId, tenderRevId);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender product report.xlsx");
        }

        public ActionResult GetCompQtyLevelReport(int bomId, int revNo)
        {
            Response.Clear();
            ReportDAL dal = new ReportDAL();
            var fileBytes = dal.GetBOMComponentQtyReport(bomId, revNo);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Component Quantity Level Report.xlsx");
        }

        public ActionResult DomTenderRevPricing(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return View("~/Views/Report/Domestic/TenderPricing.cshtml", tndDetailsModel);
        }

        public ActionResult DomTenderRevPricingPartial(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            return PartialView("~/Views/Report/Domestic/_TenderPricing.cshtml", tndDetailsModel);
        }

        public ActionResult TenderRevPricing(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return View("~/Views/Report/IntIselfa/IntTenderRevPricingReport_I.cshtml", tndDetailsModel);
        }

        public ActionResult TenderRevPricingPartial(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            return PartialView("~/Views/Report/IntIselfa/_IntTenderRevPricingReport_I.cshtml", tndDetailsModel);
        }

        public ActionResult TenderRevPricingKT(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            //ViewData["IsRevisionCreated"] = true;
            return View("~/Views/Report/IntKaramtara/IntTenderRevPricingReport_KT.cshtml", tndDetailsModel);
        }

        public ActionResult TenderRevPricingPartialKT(int enqId, int bomId, int revNo, int tndId, int tndRevNo)
        {
            var tndDetailsModel = new TenderDetailsModel();
            var tndDetailsDAL = new TenderDetailsDAL();
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            tndDetailsModel.IsEdit = true;
            return PartialView("~/Views/Report/IntKaramtara/_IntTenderRevPricingReport_KT.cshtml", tndDetailsModel);
        }

        public ActionResult RMPricing(int tndId, int tndRevNo)
        {
            var result = rmpDAL.GetRawPricingList(tndId, tndRevNo);
            RawMaterialPricingDetail rmModel = new RawMaterialPricingDetail();
            rmModel.RawMaterialList = result;
            rmModel.TndId = tndId;
            rmModel.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/Common/RawMaterial/_RMPrice.cshtml", rmModel);
        }

        public ActionResult RMPricingIteration(int tndId, int tndRevNo)
        {
            var result = rmpDAL.GetRawPricingList(tndId, tndRevNo);
            RawMaterialPricingDetail rmModel = new RawMaterialPricingDetail();
            rmModel.RawMaterialList = result;
            rmModel.TndId = tndId;
            rmModel.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/Common/RawMaterial/_RMPriceIteration.cshtml", rmModel);
        }

        [HttpPost]
        public ActionResult BGCommision(int tndId, int tndRevNo)
        {
            TenderBGModel bgModel = new TenderBGModel();
            tndDetailsDAL = new TenderDetailsDAL();
            bgModel.TenderNo = tndId;
            bgModel.TenderRevisionNo = tndRevNo;
            bgModel.List = tndDetailsDAL.GetBGData(tndId, tndRevNo);
            return PartialView("~/Views/Report/Common/BGCommision/_BGCommision.cshtml", bgModel);
        }

        [HttpPost]
        public ActionResult BGCommisionIteration(int tndId, int tndRevNo)
        {
            TenderBGModel bgModel = new TenderBGModel();
            tndDetailsDAL = new TenderDetailsDAL();
            bgModel.TenderNo = tndId;
            bgModel.TenderRevisionNo = tndRevNo;
            bgModel.List = tndDetailsDAL.GetBGData(tndId, tndRevNo);
            return PartialView("~/Views/Report/Common/BGCommision/_BGCommisionIteration.cshtml", bgModel);
        }

        [HttpPost]
        public ActionResult MarkupPricing(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetMarkupPricingList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/Markup/_Markup.cshtml", result);
        }

        [HttpPost]
        public ActionResult MarkupPricingIteration(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetMarkupPricingList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/Markup/_MarkupIteration.cshtml", result);
        }

        [HttpPost]
        public ActionResult Freight(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetFreightChargesList(tndId, tndRevNo);
            result.TndRevNo = tndRevNo;
            result.TndId = tndId;
            return PartialView("~/Views/Report/IntIselfa/Freight/_Freight.cshtml", result);
        }

        [HttpPost]
        public ActionResult FreightIteration(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetFreightChargesList(tndId, tndRevNo);
            result.TndRevNo = tndRevNo;
            result.TndId = tndId;
            return PartialView("~/Views/Report/IntIselfa/Freight/_FreightIteration.cshtml", result);
        }

        [HttpPost]
        public ActionResult Currency(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetCurrencyList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/Currency/_Currency.cshtml", result);
        }

        [HttpPost]
        public ActionResult CurrencyIteration(int tndId, int tndRevNo)
        {
            var result = intDetailsDAL.GetCurrencyList(tndId, tndRevNo);
            result.TndId = tndId;
            result.TndRevNo = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/Currency/_CurrencyIteration.cshtml", result);
        }

        [HttpPost]
        public ActionResult TestPricing(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetIntTestPricingList(tndId, tndRevNo);
            data.TenderId = tndId;
            data.TenderRevisionId = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/TestPrice/_Test.cshtml", data);
        }

        [HttpPost]
        public ActionResult TestPricingIteration(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetIntTestPricingList(tndId, tndRevNo);
            data.TenderId = tndId;
            data.TenderRevisionId = tndRevNo;
            return PartialView("~/Views/Report/IntIselfa/TestPrice/_TestIteration.cshtml", data);
        }

        [HttpPost]
        public ActionResult DomMarkup(int bomId, int revId, int tndId, int tndRevNo)
        {
            var result = mDAL.GetMarkupPricingList(bomId, revId, tndId, tndRevNo);
            if (result.Tables.Count > 1)
            {
                TempData["TenderLineValues"] = mDAL.ToDynamicList(result.Tables[1]);
            }
            //return PartialView("~/Views/Report/Tender/Markup/_MarkupPrice.cshtml", result.Tables[0]);
            return PartialView("~/Views/Report/Domestic/Markup/_Markup.cshtml", result.Tables[0]);
        }

        [HttpPost]
        public ActionResult DomMarkupIteration(int bomId, int revId, int tndId, int tndRevNo)
        {
            var result = mDAL.GetMarkupPricingList(bomId, revId, tndId, tndRevNo);
            ViewData["tendorRevNo"] = tndRevNo;
            if (result.Tables.Count > 1)
            {
                TempData["TenderLineValues"] = mDAL.ToDynamicList(result.Tables[1]);
            }
            //return PartialView("~/Views/Report/Tender/Markup/_MarkupPrice.cshtml", result.Tables[0]);
            return PartialView("~/Views/Report/Domestic/Markup/_MarkupIteration.cshtml", result.Tables[0]);
        }

        [HttpPost]
        public ActionResult DomFreight(int bomId, int revId, int tndId, int tndRevNo)
        {
            var result = fDAL.GetFreightChargesList(bomId, revId, tndId, tndRevNo);
            ViewData["tendorRevNo"] = tndRevNo;
            if (result.Tables.Count > 1)
            {
                TempData["MaxDestinationCount"] = result.Tables[result.Tables.Count - 1].Rows[0]["MaxDestinationCount"].ToString();
                result.Tables.RemoveAt(result.Tables.Count - 1);
                TempData["TruckMetricTypes"] = new SelectList(result.Tables[result.Tables.Count - 1].AsDataView(), "id", "name");
                result.Tables.RemoveAt(result.Tables.Count - 1);
            }
            return PartialView("~/Views/Report/Domestic/Freight/_Freight.cshtml", result);
        }

        [HttpPost]
        public ActionResult DomTestPricing(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetTestPricingList(tndId, tndRevNo);
            data.TenderId = tndId;
            data.TenderRevisionId = tndRevNo;
            return PartialView(@"~\Views\Report\Domestic\Testing\_Testing.cshtml", data);
        }

        [HttpPost]
        public ActionResult DomTestPricingIteration(int tndId, int tndRevNo)
        {
            var testDal = new TestDAL();
            var data = testDal.GetTestPricingList(tndId, tndRevNo);
            data.TenderId = tndId;
            data.TenderRevisionId = tndRevNo;
            return PartialView(@"~\Views\Report\Domestic\Testing\_TestingIteration.cshtml", data);
        }

        [HttpPost]
        public ActionResult DomFinalPrices(int tndId, int tndRevNo)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var tndDetailsModel = new TenderDetailsModel();
            tndDetailsModel = tndDetailsDAL.GetFinalPrices(tndId, tndRevNo);
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            return PartialView(@"~\Views\Report\Domestic\FinalPrices\_FinalPrices.cshtml", tndDetailsModel);
        }

        [HttpPost]
        public ActionResult DomFinalPricesIteration(int tndId, int tndRevNo)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var tndDetailsModel = new TenderDetailsModel();
            tndDetailsModel = tndDetailsDAL.GetFinalPrices(tndId, tndRevNo);
            tndDetailsModel.TenderId = tndId;
            tndDetailsModel.TenderRevisionNo = tndRevNo;
            return PartialView(@"~\Views\Report\Domestic\FinalPrices\_FinalPricesIteration.cshtml", tndDetailsModel);
        }

        public ActionResult IntTenderCompareRevisionK(int bomId, int revId, int tndId, int tndRevNo)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.IntTenderCompareRevisionK(bomId, revId, tndId, tndRevNo);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }
        public ActionResult IntTenderCompareRevisionI(int bomId, int revId, int tndId, int tndRevNo)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.IntTenderCompareRevisionI(bomId, revId, tndId, tndRevNo);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
        }

        public ActionResult DomTenderCompareRevision(int bomId, int revId, int tndId, int tndRevNo)
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
            //if (tndRevNo > 1)
            //{


                Response.Clear();
                Excel dal = new Excel();
                var fileBytes = dal.DomTenderCompareRevision(bomId, revId, tndId, tndRevNo);
                return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Details.xlsx");
            //}
            //else
            //{
                
            //    return null;
            //}
            
        }
        public ActionResult DomDiffTenderCompare(int firstTndId, int firstTndRevNo, int otherTndId, int otherTndRevNo)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.DomDiffTenderComparison(firstTndId, firstTndRevNo, otherTndId, otherTndRevNo);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "Tender Comparison.xlsx");
        }

        public ActionResult IntDiffTenderCompare(int firstTndId, int firstTndRevNo, int otherTndId, int otherTndRevNo)
        {
            Response.Clear();
            Excel dal = new Excel();
            var fileBytes = dal.IntDiffTenderComparison(firstTndId, firstTndRevNo, otherTndId, otherTndRevNo);
            return File(fileBytes, System.Net.Mime.DispositionTypeNames.Attachment, "International Tender Comparison.xlsx");
        }
    }
}