using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class ProductGroupController : BaseController
    {
        public ProductGroupDAL assmDal;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }
        // GET: Assembly
        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            ProductGroupModel assmModel = new ProductGroupModel();
            assmDal = new ProductGroupDAL();
            assmModel = assmDal.GetProductGroupData();
            assmModel.AssemblyList = new List<AssemblyMasterModel>();
            assmModel.MasterList = new List<MasterModel>();
            assmModel.ProductGroupList = assmModel.ProductGroupList.OrderBy(x => x.ProductGroupName).ToList();
            if (assmModel.ProductGroupList != null && assmModel.ProductGroupList.Count > 0)
            {
                var assmDetails = assmDal.GetProductGroupHierarchyById(assmModel.ProductGroupList.FirstOrDefault().ProductGroupId);
                assmModel.MasterList = assmDetails.MasterList;
                assmModel.ProductGroupId = assmDetails.ProductGroupId;
                assmModel.ProductGroupCode = assmDetails.ProductGroupCode;
                assmModel.Summary = assmDetails.Summary;
                assmModel.GroupType = assmDetails.GroupType;
                assmModel.UTS = assmDetails.UTS;
                assmModel.BundleType = assmDetails.BundleType;
                assmModel.LineVoltage = assmDetails.LineVoltage;
                assmModel.ProductGroupName = assmDetails.ProductGroupName;
                assmModel.Conductor = assmDetails.Conductor;
                assmModel.UnitGrWt = assmDetails.UnitGrWt;
                assmModel.UnitNetWt = assmDetails.UnitNetWt;
                assmModel.ConductorName = assmDetails.ConductorName;
                assmModel.BundleSpacing = assmDetails.BundleSpacing;
            }
            return View("ProductGroup", assmModel);
        }

        [HttpPost]
        public ActionResult CreateProductGroup(FormCollection form)
        {
            assmDal =new ProductGroupDAL();
            int status = 0;
            status= assmDal.CreateProductGroup(form);
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductGroupHierarchyById(int groupId)
        {
            assmDal = new ProductGroupDAL();
            var model = assmDal.GetProductGroupHierarchyById(groupId);
            return PartialView("~/Views/Shared/ProductGroup/_ProductGroupListDetails.cshtml", model);
        }

        public ActionResult GetAssembliesAutoComplete(string prefix)
        {
            assmDal = new ProductGroupDAL();
            var data = assmDal.GetAssembliesAutoComplete(prefix);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEditProductGroupData(int groupId)
        {
            assmDal = new ProductGroupDAL();
            var assmModel = assmDal.GetRelatedAssemblies(groupId);
            return PartialView("~/Views/Shared/ProductGroup/_EditProductGroup.cshtml", assmModel);
        }

        [HttpPost]
        public ActionResult EditAssembly(FormCollection form)
        {
            assmDal = new ProductGroupDAL();
            var assemblyId = assmDal.EditAssembly(form);
            TempData["EditSuccess"] = 1;
            return RedirectToAction("Index");
        }

       public ActionResult GetFilteredProductGroups(int groupTypeId, int lineVoltageId, int conductorType, int bundleTypeId)
       {
            assmDal = new ProductGroupDAL();
            var modelData = assmDal.GetFilteredProductGroups(groupTypeId, lineVoltageId, conductorType, bundleTypeId);
            return PartialView("~/Views/Shared/ProductGroup/_ProductGroupList.cshtml", modelData);
       }

        public ActionResult GetConductorNames(string prefix)
        {
            assmDal = new ProductGroupDAL();
            var data = assmDal.GetConductorNames(prefix);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

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
    }
}