using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class RawMaterialMasterController : Controller
    {
        public RMMasterModel rmModel;
        public RMMasterDAL rmDAL;
        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }
        // GET: RawMaterialMaster
        [HttpGet]
        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            rmModel = new RMMasterModel();
            rmDAL = new RMMasterDAL();
            rmModel = rmDAL.GetRMDetails();
            //rmModel.RawMaterialList.Insert(0, new RawMaterialModel { Material = "0", MaterialDesc = "Select RawMaterial Code" });
            return View("RawMaterialMaster", rmModel);
        }

        [HttpPost]
        public ActionResult SaveMaterial1(string name, string code, string type)
        {
            rmModel = new RMMasterModel();
            rmDAL = new RMMasterDAL();
            //rmModel = rmDAL.SaveRawMaterial(name, code,type);
            return View();
        }
        [HttpPost]
        public ActionResult CreateRawMaterial(string materialName, int groupId)
        {
            rmModel = new RMMasterModel() { MaterialName = materialName, GroupId = groupId };
            rmDAL = new RMMasterDAL();
            var result = rmDAL.SaveRawMaterial(rmModel);
            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpPost]
        public ActionResult AutoComplete(string prefix)
        {
            rmDAL = new RMMasterDAL();
            List<string> autoCompleteList = new List<string>();
            autoCompleteList = rmDAL.AutoCompleteList(prefix);
            return Json(autoCompleteList);
        }
        public ActionResult SearchRawMaterial(string searchText)
        {
            rmDAL = new RMMasterDAL();
            rmModel = new RMMasterModel();
            rmModel = rmDAL.GetRMSearchData(searchText);
            return PartialView("_RMList", rmModel);
        }
        public JsonResult GetAllRawMaterials(string prefix)
        {
            rmDAL = new RMMasterDAL();
            var data = rmDAL.GetRMDetails().RawMaterialList;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllRawMaterialGroups(string prefix)
        {
            rmDAL = new RMMasterDAL();
            var data = rmDAL.GetRMTypes(prefix);
            return Json(data, JsonRequestBehavior.AllowGet);
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
}