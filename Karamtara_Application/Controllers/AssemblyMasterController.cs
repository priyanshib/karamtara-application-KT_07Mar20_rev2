using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using System.Net;

namespace Karamtara_Application.Controllers
{
    public class AssemblyMasterController : BaseController
    {
        public AssemblyMasterModel assmModel;
        public AssemblyDAL assmDAL;
        public CommonDAL _comDal;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        public int GetUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }
        // GET: AssemblyMaster
        [HttpGet]
        public ActionResult AssemblyMaster()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            _comDal = new CommonDAL();
            assmModel.AssemblyList = new List<AssemblyMasterModel>();

            assmModel = assmDAL.GetAssemblyProducts();
            assmModel.AssemblyList = assmDAL.GetAssemblyList();
            assmModel.AssemblyList = assmModel.AssemblyList.OrderBy(x => x.AssemblyName).ToList();
            assmModel.UnitList = _comDal.GetUnitList();

            return View(assmModel);
        }

        public ActionResult AssemblyMasterWithId(int assmId)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            _comDal = new CommonDAL();
            assmModel.AssemblyList = new List<AssemblyMasterModel>();
            assmModel = assmDAL.GetAssemblyProducts(assmId);
            assmModel.AssemblyList = assmDAL.GetAssemblyList().OrderBy(x => x.AssemblyCode).ThenBy(y=>y.AssemblyName).ToList();
            assmModel.UnitList = _comDal.GetUnitList();
            //assmModel.AssemblyList = assmModel.AssemblyList.OrderBy(x => x.AssemblyName).ToList();
            return View("AssemblyMaster", assmModel);
        }

        [HttpPost]
        public ActionResult AssmAutocomplete(string prefix)
        {
            List<string> autoCompleteList = new List<string>();
            assmDAL = new AssemblyDAL();
            autoCompleteList = assmDAL.GetAutoCompleteList(prefix);
            return Json(autoCompleteList);
        }

        [HttpPost]
        public ActionResult AssmSearchList(string searchText)
        {
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            assmModel.AssemblyList = assmDAL.SearchSelectList(searchText).OrderBy(x => x.AssemblyCode).ThenBy(y => y.AssemblyName).ToList();
            return PartialView("~/Views/Shared/Assembly/_AssemblyList.cshtml", assmModel);
        }

        [HttpPost]
        public ActionResult CreateAssembly(FormCollection form)
        {
            List<HttpPostedFileBase> drawingFiles = new List<HttpPostedFileBase>();
            if (Request != null)
            {
                drawingFiles = Request.Files.GetMultiple("txtDrawingFileName").ToList();
            }
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            var assmId = assmDAL.CreateAssembly(form, drawingFiles);
            //if(assmModel == null)
            //TempData["CreateAssSuccess"] = 1;
            return RedirectToAction("AssemblyMasterWithId", new { assmId = assmId });
            //return Json(assmModel.AssemblyId, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult RefreshAssemblyList()
        {
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            assmModel.AssemblyList = assmDAL.GetAssemblyList();
            return PartialView("~/Views/Shared/Assembly/_AssemblyList.cshtml", assmModel);
        }

        [HttpGet]
        public ActionResult GetAssmProducts(int assmId)
        {
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            assmModel = assmDAL.GetAssemblyProducts(assmId);
            return PartialView("~/Views/Shared/Assembly/_AssmProducts.cshtml", assmModel);
        }

        [HttpGet]
        public ActionResult GetAssmList()
        {
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            assmModel.AssemblyList = assmDAL.GetAssemblyList();
            assmModel.SubAssemblyList = new List<SubAssemblyListModel>();
            return PartialView("~/Views/Shared/Assembly/_AssemblyList.cshtml", assmModel);

        }
        [HttpGet]
        public ActionResult AssmProdEditGet(int data)
         {
            assmModel = new AssemblyMasterModel();
            assmDAL = new AssemblyDAL();
            _comDal = new CommonDAL();
            assmModel = assmDAL.GetAssemblyProductsAutocomplete(data);
            assmModel.UnitList = _comDal.GetUnitList();
            return PartialView("~/Views/Shared/Assembly/_AssmLinkProducts.cshtml", assmModel);
        }

        [HttpPost]
        public ActionResult AssmProductsEditSave(FormCollection form)
        {
            assmDAL = new AssemblyDAL();
            var status = assmDAL.EditAssembly(form);
            int assemblyId = Convert.ToInt32(form["AssmId"]);
            if (assemblyId > 0)
            {
                var assmModel = assmDAL.GetAssemblyProducts(assemblyId);
                return PartialView("~/Views/Shared/Assembly/_AssmProducts.cshtml", assmModel);
            }
            else
                return Json(false, JsonRequestBehavior.AllowGet);
            
        }

        [HttpPost]
        public ActionResult GetAssemblyDetailsSearch(string prefix)
        {
            assmDAL = new AssemblyDAL();
            var data = assmDAL.GetSubAssembliesAndComponentsBySearch(prefix).MasterList;
            return Json(data);
            //return PartialView("~/Views/Shared/Assembly/_AssmLinkProducts.cshtml", data);
        }

        // [HttpPost]
        // public ActionResult GetSubAssmCodeAutoComp(string prefix)
        //{
        //     List<string> autoCompleteList = new List<string>();
        //     assmDAL = new AssemblyDAL();
        //     autoCompleteList = assmDAL.GetAutoComp_SubAssmCode(prefix);
        //     return Json(autoCompleteList);
        // }

        //[HttpPost]
        //public ActionResult GetSubAssmTNameAutoComp(string prefix)
        //{
        //    List<string> autoCompleteList = new List<string>();
        //    assmDAL = new AssemblyDAL();
        //    autoCompleteList = assmDAL.GetAutoComp_TName(prefix);
        //    return Json(autoCompleteList);
        //}

        //[HttpGet]
        //public ActionResult GetSubAssmRow(string code)
        //{
        //    AssemblyMasterModel subAssmModel = new AssemblyMasterModel();
        //    assmDAL = new AssemblyDAL();
        //    subAssmModel = assmDAL.GetSubAssmRowDetails(code);
        //    var dataObj = new { Code = subAssmModel.AssemblyCode, Name = subAssmModel.AssemblyName, TName = subAssmModel.AssmTechName, SubAssmId=subAssmModel.AssemblyId };
        //    return Json(dataObj,JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult CodeAutoComplete(string searchText)
        {
            assmDAL = new AssemblyDAL();
            List<string> autoCompleteList = new List<string>();
            autoCompleteList = assmDAL.AutoCompleteCodeList(searchText);
            return Json(autoCompleteList);
        }

        [HttpPost]
        public ActionResult AssmAttachEditSave(FormCollection form)
        {
            var isRemove = form["IsRemove"];
            var assId = form["AssmId"];
            if (isRemove == "true")
            {
                assmModel = new AssemblyMasterModel();
                assmDAL = new AssemblyDAL();
                int temp = assmDAL.deleteFile(assId);
            }
            else
            {
                List<HttpPostedFileBase> drawingFiles = new List<HttpPostedFileBase>();
                if (Request != null)
                {
                    drawingFiles = Request.Files.GetMultiple("image").ToList();
                }
                assmModel = new AssemblyMasterModel();
                assmDAL = new AssemblyDAL();
                assmModel = assmDAL.uploadFile(form, drawingFiles);
            }
            //TempData["CreateAssSuccess"] = 1;
            //return PartialView("~/Views/Shared/Assembly/_AssmLinkProducts.cshtml");
            return RedirectToAction("AssemblyMaster");
        }


        public ActionResult DownloadFile(int assmId, string fileName)
        {
            assmDAL = new AssemblyDAL();
            var fileBytes = assmDAL.DocumentDownload(assmId, fileName, out string newName);

            if (fileBytes == null)
                return HttpNotFound();

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, newName);
        }


        //public ActionResult DownloadFile(FormCollection form,string filename)
        //{
        //    var assmId = form["AssmId"];
        //    string remoteUri = "http://www.contoso.com/library/homepage/images/";
        //    string fileName = "ms-banner.gif", myStringWebResource = null;

        //    // Create a new WebClient instance.
        //    using (WebClient myWebClient = new WebClient())
        //    {
        //        myStringWebResource = remoteUri + fileName;
        //        // Download the Web resource and save it into the current filesystem folder.
        //        myWebClient.DownloadFile(myStringWebResource, fileName);
        //    }
        //    return RedirectToAction("AssemblyMaster");
        //}
    }
}
