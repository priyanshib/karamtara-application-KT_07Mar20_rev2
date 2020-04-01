using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Karamtara_Application.Controllers
{

    public class CreateBOMController : BaseController
    {
        public CreateBOMDAL bomDal;
        public CreateBOMModel bomModel;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        // GET: CreateBOM
        [HttpGet]
        public ActionResult CreateBOM()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            int enqId = 0;
            if (Request.QueryString["EnqId"] != null)
            {
                enqId = Convert.ToInt32(Request.QueryString["EnqId"].ToString());
            }
            CreateBOMModel bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
            bomModel.SubAssemblyList = new List<SubAssemblyListModel>();
            bomModel.ComponentList = new List<ComponentModel>();
            bomModel.DisplayText = "";
            bomModel = bomDal.GetCurrentBomId(enqId);
            if (bomModel.CreateBOMHasRows)
            {
                var routeValues = new RouteValueDictionary { { "bomId", bomModel.BomId }, { "revNo", bomModel.RevisionNo } };
                RedirectToAction("EditBOM", "EditBOM", routeValues);
            }
            else
            {
                return View(bomModel);
            }
            return View(bomModel);
        }

        [HttpPost]
        public ActionResult BOMAutoComplete(string prefix)
        {
            List<BOMAutoComplete> autoComplete = new List<BOMAutoComplete>();
            bomDal = new CreateBOMDAL();
            autoComplete = bomDal.BOMAutoComplete(prefix);
            return Json(autoComplete);
        }

        [HttpPost]
        public ActionResult AssemblyAutoCompleteList(string prefix)
        {
            List<string> autoComplete = new List<string>();
            bomDal = new CreateBOMDAL();
            autoComplete = bomDal.GetAutoCompleteList(prefix);
            return Json(autoComplete);
        }

        [HttpGet]
        public ActionResult GetAssemblyDetails(int assmId)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            bomModel = bomDal.GetAssmProducts(assmId);
            return PartialView("~/Views/Shared/CreateBOM/_SubAssmDetails.cshtml", bomModel);
        }

        [HttpGet]
        public ActionResult EditProductGroup(int groupId)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            bomModel = bomDal.GetProdGroupList(groupId);
            // bomModel.DisplayText = assmName;
            return PartialView("~/Views/Shared/CreateBOM/_EditSubAssmList.cshtml", bomModel);
        }

        [HttpGet]
        public ActionResult GetProductGroupList(int groupId)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            bomModel = bomDal.GetProdGroupList(groupId);
            return PartialView("~/Views/Shared/CreateBOM/_SubList.cshtml", bomModel);
        }

        [HttpPost]
        public ActionResult SaveSubAsmChanges(FormCollection form)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            int status = 0;
            status = bomDal.SaveSubAssmChanges(form);
            return Json(status);

        }

        [HttpPost]
        public ActionResult UpdateBOMMaster(string assmName, int projId, int enqId)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            int status = 0;
            // bomDal.UpdateBOMTables(id, prodType);
            status = bomDal.InsertSubAsmBOM(assmName, projId, enqId);
            return Json(status);

        }

        [HttpPost]
        public ActionResult SaveBOMDetails(FormCollection form)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            int status = 0;
            var userId = GetCurrentUserId();
            status = bomDal.SaveBOMDetails(form, userId);
            return Json(status);

        }

        [HttpGet]
        public ActionResult GetProductDetails(int prodId, int prodType)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            bomModel = bomDal.GetProductDetails(prodId, prodType);
            bomModel.ProductType = prodType;
            return PartialView("~/Views/Shared/CreateBOM/_SubAssmDetails.cshtml", bomModel);
        }

        [HttpPost]
        public ActionResult SaveAssemblyChanges(FormCollection form)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            int status = 0;
            status = bomDal.SaveAssemblyChanges(form);
            return Json(status);

        }

        [HttpPost]
        public ActionResult PublishBOM(FormCollection form)
        {
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            int status = 0;
            var userId = GetCurrentUserId();
            status = bomDal.CreateBOMRevision(form,userId);
            return Json(status);

        }

        //[HttpGet]
        //public ActionResult ViewBOM(int enqId)
        //{
        //   // bomId = 2;
        //    bomModel = new CreateBOMModel();
        //    bomDal = new CreateBOMDAL();
        //    bomModel = bomDal.GetBomDetails(enqId);
        //    return View("ViewBOM", bomModel);

        //}

        [HttpGet]
        public ActionResult CreateRevision()
        {
            int enqId = 0;
            if (Request.QueryString["EnqId"] != null)
            {
                enqId = Convert.ToInt32(Request.QueryString["EnqId"].ToString());
            }
            bomModel = new CreateBOMModel();
            bomDal = new CreateBOMDAL();
            bomModel = bomDal.GetBomData_Revision(enqId);
            return View(bomModel);
        }

        [HttpGet]
        public ActionResult GetBomListForCloneSelection(int enqId, int bomId, int revId)
        {
            BOMListModel bomListModel = new BOMListModel();
            BOMListDAL bomListDal = new BOMListDAL();
            bomListModel = bomListDal.GetBOMList_Clone();
            bomListModel.EnquiryId = enqId;
            bomListModel.BomId = bomId;
            bomListModel.RevisionNo = revId;
            return PartialView("~/Views/Shared/CreateBOM/_CloneBomSelection.cshtml", bomListModel);
        }

        [HttpGet]
        public ActionResult ViewBOM(int enqId, int bomId, int revNo)
        {
            // bomId = 2;
            bomModel = new CreateBOMModel();
            BomRevisionDAL bomRevDAL = new BomRevisionDAL();
            bomModel = bomRevDAL.GetBomProjDetails(bomId, revNo, enqId);
            bomModel.MasterList = bomRevDAL.ViewBomData(bomId, revNo, enqId);
            bomModel.Summary = bomRevDAL.GetSummary(bomId, revNo);
            bomModel.BomId = bomId;
            bomModel.RevisionNo = revNo;
            bomModel.EnquiryId = enqId;
            return View("~/Views/CreateBOM/ViewBOM.cshtml", bomModel);
        }

        //[HttpGet]
        //public ActionResult GetSummary(int bomId, int revNo)
        //{
        //    //// bomId = 2;
        //    //bomModel = new CreateBOMModel();
        //    //BomRevisionDAL bomRevDAL = new BomRevisionDAL();
        //    //bomModel.MasterList = bomRevDAL.ViewBomData(bomId, revNo);
        //    //bomModel.BomId = bomId;
        //    //bomModel.RevisionNo = revNo;
        //    //bomModel.EnquiryId = enqId;
        //    //return View("~/Views/CreateBOM/ViewBOM.cshtml", null);
        //}

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    if (filterContext.ExceptionHandled)
        //    {
        //        return;
        //    }
        //    filterContext.Result = new ViewResult
        //    {
        //        ViewName = "~/Views/Shared/Error.cshtml"
        //    };
        //    filterContext.ExceptionHandled = true;
        //}

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
