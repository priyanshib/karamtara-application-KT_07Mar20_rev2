using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{

    public class BOMRevisionController : Controller
    {
        public BomRevisionDAL bomRevDAL;
        public CreateBOMModel bomModel;
        public MasterModel masterModel;
        public CreateBOMDAL bomDal;
        public BOMRevisionModel bomRevModel;
        int enqId1 = 0;
        int bomId1 = 0;
        int revNo1 = 0;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }
        // GET: BOMRevision
        [HttpGet]
        public ActionResult GetRevisionData()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");
            masterModel = new MasterModel();
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            int bomId = 0;
            int revId = 0;
            int enqId = 0;
            if (Request.QueryString["enqId"] != null)
            {
                enqId = Convert.ToInt32(Request.QueryString["enqId"].ToString());
            }
            if (Request.QueryString["bomId"] != null)
            {
                bomId = Convert.ToInt32(Request.QueryString["bomId"].ToString());
            }
            if (Request.QueryString["revNo"] != null)
            {
                revId = Convert.ToInt32(Request.QueryString["revNo"].ToString());
            }
            var userId = GetCurrentUserId();
            bomModel = bomRevDAL.GetCurrentRevDetails(bomId, enqId, userId, "BOM");
            bomModel.OldRevisionNo = revId;
            //   if (bomId > 0)
            bomModel.MasterList = bomRevDAL.GetBomData(bomId, bomModel.OldRevisionNo, enqId, bomModel.RevisionNo, "BOM", bomModel.IsTemp);
            bomModel.BomType = "Revision";
            return View("BOMRevision", bomModel);
        }

        public ActionResult GetRevisionDataFromMaster()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");
            masterModel = new MasterModel();
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            int bomId = 0;
            int revId = 0;
            int enqId = 0;
            if (Request.QueryString["enqId"] != null)
            {
                enqId = Convert.ToInt32(Request.QueryString["enqId"].ToString());
            }
            if (Request.QueryString["bomId"] != null)
            {
                bomId = Convert.ToInt32(Request.QueryString["bomId"].ToString());
            }
            if (Request.QueryString["revNo"] != null)
            {
                revId = Convert.ToInt32(Request.QueryString["revNo"].ToString());
            }
            var userId = GetCurrentUserId();
            bomModel = bomRevDAL.GetCurrentRevDetails(bomId, enqId, userId, "Master");
            bomModel.OldRevisionNo = revId;
            //   if (bomId > 0)
            bomModel.MasterList = bomRevDAL.GetBomData(bomId, bomModel.OldRevisionNo, enqId, bomModel.RevisionNo, "Master", bomModel.IsTemp);
            return View("BOMRevision", bomModel);
        }

        [HttpGet]
        public ActionResult ViewBOM(int enqId, int bomId, int revNo)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            // bomId = 2;
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            bomModel = bomRevDAL.GetBomProjDetails(bomId, revNo, enqId);
            bomModel.MasterList = bomRevDAL.ViewBomData(bomId, revNo, enqId);
            bomModel.Summary = bomRevDAL.GetSummary(bomId, revNo);
            bomModel.BomId = bomId;
            bomModel.RevisionNo = revNo;
            bomModel.EnquiryId = enqId;

            return View("~/Views/CreateBOM/ViewBOM.cshtml", bomModel);
        }

        [HttpGet]
        public ActionResult GetSubAssmData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            //int prodId, int type, int parentId, int related, int bomId, int revNo
            bomRevModel = bomRevDAL.GetSubAssmData(prodId, bomId, revNo, pgId, assmId, subAssmId);
            bomRevModel.BomId = bomId;
            bomRevModel.RevisionNo = revNo;
            return PartialView("~/Views/Shared/BOMRevision/_EditProducts.cshtml", bomRevModel);

        }

        [HttpGet]
        public ActionResult GetAssmData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            //int prodId, int type, int parentId, int related, int bomId, int revNo
            bomRevModel = bomRevDAL.GetAssmData(prodId, bomId, revNo, pgId, assmId, subAssmId);
            bomRevModel.BomId = bomId;
            bomRevModel.RevisionNo = revNo;
            return PartialView("~/Views/Shared/BOMRevision/_EditProducts.cshtml", bomRevModel);

        }

        [HttpGet]
        public ActionResult GetPGData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            //int prodId, int type, int parentId, int related, int bomId, int revNo
            bomRevModel = bomRevDAL.GetPGData(prodId, bomId, revNo, pgId, assmId, subAssmId);
            bomRevModel.BomId = bomId;
            bomRevModel.RevisionNo = revNo;
            return PartialView("~/Views/Shared/BOMRevision/_EditProducts.cshtml", bomRevModel);

        }
        [HttpGet]
        public ActionResult RemoveSubAssmProd(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.RemoveSubAssmData(prodId, prodType, bomId, revNo, parentId, parentType, subAssmId, assmId, pgId);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult RemoveAssmProd(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.RemoveSubAssmData(prodId, prodType, bomId, revNo, parentId, parentType, subAssmId, assmId, pgId);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult RemovePgProd(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.RemoveSubAssmData(prodId, prodType, bomId, revNo, parentId, parentType, subAssmId, assmId, pgId);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveSubAssmChanges(FormCollection form)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.SaveSubAssmChanges(form);
            return Json(status);
        }

        [HttpPost]
        public ActionResult SaveAssmChanges(FormCollection form)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.SaveAssemblyChanges(form);
            return Json(status);
        }

        [HttpPost]
        public ActionResult SavePGChanges(FormCollection form)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.SavePGChanges(form);
            return Json(status);
        }

        [HttpPost]
        public ActionResult PublishRevision(int bomId, int revNo, string tNumber)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            var userId = GetCurrentUserId();
            status = bomRevDAL.PublishBOMRevision(bomId, revNo, tNumber, userId);
            return Json(status);
        }

        [HttpPost]
        public ActionResult CancelRevision(int bomId, int revNo)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 0;
            status = bomRevDAL.CancelBOMRevision(bomId, revNo);
            return Json(status);
        }

        public ActionResult DownloadExcel(int bomId, int revNo, int enqId)
        {
            bomRevDAL = new BomRevisionDAL();
            Response.Clear();
            Response.BinaryWrite(bomRevDAL.GetExcel(GetDownloadExcel(bomId, revNo, enqId), enqId));
            //var dt = enqDAL.GetDownloadedExcel(1);
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment;filename=MasterModel.xls");

            Response.ContentType = "application/vnd.ms-excel";
            Response.AppendHeader("content-disposition", "attachment; filename=BomFile.xlsx");

            Response.Flush();
            Response.End();
            return View();
        }

        [NonAction]
        public List<MasterModel> GetDownloadExcel(int bomId, int revNo, int enqId)
        {
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            bomModel.MasterList = bomRevDAL.ViewBomData(bomId, revNo, enqId);
            //return dt;
            return bomModel.MasterList;
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
        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }

        [HttpPost]
        public ActionResult AddNewComponent(int bomId, int revNo, int prodId, int prodType, string TNumber)
        {
            bomRevModel = new BOMRevisionModel();
            bomRevDAL = new BomRevisionDAL();
            int status = 1;
            status = bomRevDAL.AddNewComponent(bomId, revNo, prodId, prodType, TNumber);
            return Json(status);
        }

        [HttpGet]
        public ActionResult GetEditRevisionData(int bomId, int revNo)
        {
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            bomModel = bomRevDAL.GetEditDetails(bomId, revNo);
            bomModel.MasterList = bomRevDAL.GetEdit_RevisionData(bomId, revNo);
            bomModel.BomId = bomId;
            bomModel.RevisionNo = revNo;
           // bomModel.IsEdit = true;
            return View("BOMRevision", bomModel);
        }

        [HttpGet]
        public ActionResult RefreshFromMaster(int bomId, int revNo)
        {
            bomModel = new CreateBOMModel();
            bomRevDAL = new BomRevisionDAL();
            bomModel.MasterList = bomRevDAL.RefreshDataFromMaster(bomId, revNo);
            bomModel.IsEdit = false;
            return View("BOMRevision", bomModel);
        }
    }
}
