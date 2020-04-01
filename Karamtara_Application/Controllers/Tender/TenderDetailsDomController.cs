using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models.Tender;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers.Tender
{
    //this controller not being used
    public class TenderDetailsDomController : BaseController
    {
        public TenderDetailsModel tndDetailsModel;
        public TenderDetailsDAL tndDetailsDAL;
        // GET: TenderDetails
        [HttpGet]
        public ActionResult TenderDetailsDom(int enqId, int bomId, int revNo, int tndId = 1, int tndRevNo = 1)
        {
            tndDetailsModel = new TenderDetailsModel();
            tndDetailsDAL = new TenderDetailsDAL();
            //  tndDetailsModel = tndDetailsDAL.GetTenderDetails(enqId, bomId, revNo);
            tndDetailsModel = tndDetailsDAL.GetBomProdDetails(bomId, revNo, tndId, tndRevNo);
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.EnquiryId = enqId;
            tndDetailsModel.RevisionNo = revNo;
            return View("~/Views/Tender/TenderDetailsDom/TenderDetailsDom.cshtml", tndDetailsModel);
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
            return View("~/Views/Tender/TenderDetailsDom/TenderDetailsDom.cshtml");
        }

        public JsonResult SaveLineQty(string values)
        {
            tndDetailsDAL = new TenderDetailsDAL();
            var result = tndDetailsDAL.SaveLineQty(values,"");
            return Json(result);
        }

       
    }
}