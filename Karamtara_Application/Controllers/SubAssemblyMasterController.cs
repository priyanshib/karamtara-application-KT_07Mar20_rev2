using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Linq;

namespace Karamtara_Application.Controllers
{
    public class SubAssemblyMasterController : BaseController
    {
        public SubAssemblyDAL subAssmDAL;
        public int CatId;

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

        // GET: ProductMaster
        [HttpGet]
        public ActionResult SubAssemblyMaster()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            SubAssemblyMasterModel prodMsModel = new SubAssemblyMasterModel();
            prodMsModel.SubAssemblyList = new List<SubAssemblyListModel>();
            subAssmDAL = new SubAssemblyDAL();
            CommonDAL commonDal = new CommonDAL();
            prodMsModel = subAssmDAL.GetSumAssemblyMasterData();
            prodMsModel.UnitList = commonDal.GetUnitList();
            prodMsModel.SubAssemblyList = prodMsModel.SubAssemblyList.OrderBy(x => x.SubAssemblyName).ToList();
            prodMsModel.CategoryId = 1;

            return View(prodMsModel);
        }

        [HttpPost]
        public ActionResult CheckIfCodeOrCatNoExists(string codeOrCatNum, int type)
        {
            CommonDAL comDAL = new CommonDAL();
            var result = comDAL.CheckIfCodeOrCatNumExists(codeOrCatNum, type);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateSubAssembly()
        {
            var temp = new SubAssemblyDAL();
            var commonDal = new CommonDAL();
            var data = temp.GetComponents();
            data.UnitList = commonDal.GetUnitList();
            return PartialView("~/Views/Shared/SubAssemblyMaster/_SubAssmCreate.cshtml", data);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CreateSubAssembly(FormCollection form)
        {
            subAssmDAL = new SubAssemblyDAL();
            var result = subAssmDAL.CreateSubAssembly(form);
            return Json(result);
            //return RedirectToAction("SubAssemblyMaster");
        }

        public JsonResult GetComponents()
        {
            var temp = new SubAssemblyDAL();
            var data = temp.GetComponents();
            return Json(data.ComponenetList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetComponentList(int subAssId)
        {
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            subAssmDAL = new SubAssemblyDAL();
            CommonDAL commonDal = new CommonDAL();
            subAssmModel = subAssmDAL.GetComponentDetails(subAssId);
            subAssmModel.UnitList = commonDal.GetUnitList();
            subAssmModel.RenderPartialView = true;
            return PartialView("~/Views/Shared/SubAssemblyMaster/_SubAssmDetailsEdit.cshtml", subAssmModel);
        }

        public ActionResult GetSubAssemblyList()
        {
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            subAssmDAL = new SubAssemblyDAL();
            subAssmModel = subAssmDAL.GetSubAssemblyList();
            subAssmModel.RenderPartialView = true;
            return PartialView("~/Views/Shared/SubAssemblyMaster/_SubAssmList.cshtml", subAssmModel);
        }

        [HttpPost]
        public ActionResult EditSubAssembly(FormCollection form)

        {
            string prodId = form["ProductId"];
            SubAssemblyMasterModel prodMSModel = new SubAssemblyMasterModel();
            subAssmDAL = new SubAssemblyDAL();
            int addCount = 0;
            addCount = subAssmDAL.EditSubAssembly(form);
            if (addCount > 0)
                return Json(addCount);
            else if (addCount == 0)
                return Json(addCount);
            else
                return Json(addCount);
        }

        [HttpPost]
        public ActionResult DeleteSubAssembly(int subAssmId)
        {
            subAssmDAL = new SubAssemblyDAL();
            var userId = GetUserId();
            var result = subAssmDAL.DeleteSubAssembly(subAssmId, userId);

            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        public ActionResult SubAssmSearchList(string searchText, bool clearResult = false)
        {
            subAssmDAL = new SubAssemblyDAL();
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            subAssmModel.SubAssemblyList = subAssmDAL.GetSubAssemblyList(clearResult ? string.Empty : searchText).SubAssemblyList.OrderBy(x => x.CatalogueNo).ThenBy(y => y.SubAssemblyName).ToList();
            return PartialView("~/Views/Shared/SubAssemblyMaster/_SubAssmList.cshtml", subAssmModel);
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
    }

    //    [HttpPost]
    //    public ActionResult CreateComponents(FormCollection form)
    //    {
    //        subAssmDAL = new SubAssemblyDAL();
    //        var result = subAssmDAL.CreateComponents(form);

    //        if (result > 0)
    //            return Json(true);
    //        else
    //            return Json(false);
    //    }

    //    public ActionResult ComponentMaster()
    //    {
    //        subAssmDAL = new SubAssemblyDAL();
    //        ComponentModel component = new ComponentModel();
    //        component.ComponentList = subAssmDAL.getComponenetMaterData();
    //        return View("ComponentMaster", component);
    //    }

    //    [HttpPost]
    //    public ActionResult UpdateComponenetMaster(int ComponentId, string ComponentName, string RawMaterialId, string Size, int Qty, string UnitGrWt, string UnitNetWt, int GalvanizedRequired, int GalvanizedMaterial, string DrawingNo, string MaterialGrade)
    //    {
    //        subAssmDAL = new SubAssemblyDAL();
    //        ComponentModel component = new ComponentModel();
    //        int addCount = 0;
    //        //  ComponentId, ComponentName, RawMaterialId, Size, Qty, UnitGrWt, UnitNetWt, GalvanizedRequired, GalvanizedMaterial, DrawingNo, MaterialGrade
    //        //addCount = subAssmDAL.SubmitComponentMaster(ComponentId, ComponentName, RawMaterialId, Size, Qty, UnitGrWt, UnitNetWt, GalvanizedRequired, GalvanizedMaterial, DrawingNo, MaterialGrade);
    //        if (addCount > 0)
    //            return Json(true);
    //        else
    //            return Json(false);
    //    }

    //    [HttpGet]
    //    public ActionResult GetComponentMaster(int ComponentId)
    //    {

    //        subAssmDAL = new SubAssemblyDAL();
    //        ComponentModel component = new ComponentModel();
    //        component = subAssmDAL.GetComponentMasterdata(ComponentId); /*UnitGrWt, UnitNetWt, DrawingNo, Size, MaterialGrade, RawMaterialId, GalvanizedRequired*/
    //        return PartialView("~/Views/Shared/SubAssemblyMaster/_EditComponents.cshtml", component);
    //    }

    //    [HttpPost]
    //    public ActionResult SubmitCompMaster(ComponentModel model)
    //    {
    //        subAssmDAL = new SubAssemblyDAL();
    //        ComponentModel component = new ComponentModel();
    //        int addCount = 0;
    //        //  ComponentId, ComponentName, RawMaterialId, Size, Qty, UnitGrWt, UnitNetWt, GalvanizedRequired, GalvanizedMaterial, DrawingNo, MaterialGrade
    //        addCount = subAssmDAL.SubmitComponentMaster(model);
    //        component.ComponentList = subAssmDAL.getComponenetMaterData();


    //        return View("ComponentMaster", component);
    //    }

    //    [HttpPost]
    //    public ActionResult DeleteComponentMaster(int ComponentId)
    //    {
    //        subAssmDAL = new SubAssemblyDAL();
    //        var result = subAssmDAL.DeleteComponenetMaster(ComponentId);

    //        if (result > 0)
    //            return Json(true);
    //        else
    //            return Json(false);
    //    }
    //    public ActionResult EditSubAssm(int prodId, int parentId, int related, int bomId, int revNo)
    //    {
    //        bomRevModel = new BOMRevisionModel();
    //        bomRevDAL = new BomRevisionDAL();
    //        //int prodId, int type, int parentId, int related, int bomId, int revNo
    //        bomRevModel = bomRevDAL.EditSubAssmData(prodId, bomId, revNo, related, parentId);
    //        bomRevModel.BomId = bomId;
    //        bomRevModel.RevisionNo = revNo;
    //        return PartialView("~/Views/Shared/BOMRevision/_EditProducts.cshtml", bomRevModel);

    //    }
    //    public ActionResult GetAllProductsAutoComplete()
    //    {
    //        var prodDal = new SubAssemblyMasterDAL();
    //        var data = prodDal.GetProducts();
    //        return Json(data, JsonRequestBehavior.AllowGet);
    //    }

    //    [HttpGet]
    //    public ActionResult CreateProductGet(int id)
    //    {
    //        try
    //        {
    //            SubAssemblyMasterModel prodMSModel = new SubAssemblyMasterModel();
    //            prodMSModel.ComponenetList = new System.Collections.Generic.List<ComponentModel>();
    //            prodMSModel.CatalogueNo = string.Empty;
    //            prodMSModel.SubAssemblyName = string.Empty;
    //            prodMSModel.CategoryId = id;
    //            return PartialView("_SubAssmDetailsEdit", prodMSModel);
    //            //ProductMasterModel prodMsModel = new ProductMasterModel();
    //            //prodMsDAL = new ProductMasterDAL();
    //            //prodMsModel = prodMsDAL.GetProductMasterData();
    //            //prodMsModel.SubProductList = new System.Collections.Generic.List<SubProductModel>();
    //            //prodMsModel.CategoryId = CatId;
    //            //// prodMsModel.autoTest1 = new List<string> { "abc", "def", "ghi" };
    //            //return View("ProductMaster", prodMsModel);
    //            //  return RedirectToAction("ProductMaster");

    //        }
    //        catch (Exception ex)
    //        {
    //            return View();
    //        }

    //    }

    //    public ActionResult GetProductList_Cat(int catId)
    //    {
    //        SubAssemblyMasterModel prodMSModel = new SubAssemblyMasterModel();
    //        subAssmDAL = new SubAssemblyMasterDAL();
    //        prodMSModel.CategoryId = catId;
    //        prodMSModel.SubAssemblyList = subAssmDAL.GetProductListFromCat(catId);
    //        CatId = catId;
    //        return PartialView("_ProdMSProdList", prodMSModel);
    //    }

    //    [HttpGet]
    //    public ActionResult CreateProdTest()
    //    {
    //        int id = Convert.ToInt32(Request.QueryString["CatId"]);
    //        SubAssemblyMasterModel prodMsModel = new SubAssemblyMasterModel();
    //        subAssmDAL = new SubAssemblyMasterDAL();
    //        prodMsModel = subAssmDAL.GetProductMasterData();
    //        prodMsModel.SubAssemblyList = subAssmDAL.GetProductListFromCat(id);
    //        prodMsModel.ComponenetList = new System.Collections.Generic.List<ComponentModel>();
    //        prodMsModel.CategoryId = id;
    //        TempData["CreateProd_Cat"] = id;
    //        return View("SubAssemblyMaster", prodMsModel);
    //    }

    //    public ActionResult SaveComponents(FormCollection form)
    //    {
    //        SubAssemblyMasterModel prodMSModel = new SubAssemblyMasterModel();
    //        subAssmDAL = new SubAssemblyMasterDAL();
    //        int addCount = 0;
    //        addCount = subAssmDAL.AddComponents(form, CatId);
    //        return View();
    //    }
    //}
}