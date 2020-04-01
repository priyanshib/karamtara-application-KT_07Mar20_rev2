using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class ComponentMasterController : BaseController
    {
        public SubAssemblyDAL subAssmDAL;

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
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult ComponentMaster(string prefix = "")
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            subAssmDAL = new SubAssemblyDAL();
            ComponentModel component = new ComponentModel();
            var unitMasters = new List<UnitMaster>();
            component.ComponentList = subAssmDAL.getComponenetMaterData(out unitMasters, prefix);
            component.UnitList = unitMasters;
            component.UnitId = 2;
            component.ComponentList = component.ComponentList.OrderBy(x => x.ComponentName).ToList();
            return View("ComponentMaster", component);
        }

        public ActionResult GetComponentMasterList(string searchText = "", bool clearResult = false)
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            subAssmDAL = new SubAssemblyDAL();
            ComponentModel component = new ComponentModel();
            var unitMasters = new List<UnitMaster>();
            component.ComponentList = subAssmDAL.getComponenetMaterData(out unitMasters, (clearResult ? string.Empty : searchText));
            component.UnitList = unitMasters;
            component.UnitId = 2;
            component.ComponentList = component.ComponentList.OrderBy(x => x.ComponentName).ToList();
            return PartialView("~/Views/Shared/ComponentMaster/_ComponentMasterList.cshtml", component);
        }

        [HttpPost]
        public ActionResult CreateComponents(FormCollection form)
        {
            subAssmDAL = new SubAssemblyDAL();
            var result = subAssmDAL.CreateComponents(form);

            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpGet]
        public ActionResult GetComponentMaster(int ComponentId)
        {
            subAssmDAL = new SubAssemblyDAL();
            ComponentModel component = new ComponentModel();
            component = subAssmDAL.GetComponentMasterdata(ComponentId);
            component.GalvanizedMaterialList = subAssmDAL.GetGalvanizedMaterialList();
            return PartialView("~/Views/Shared/ComponentMaster/_EditComponents.cshtml", component);
        }

        [HttpPost]
        public ActionResult DeleteComponentMaster(int ComponentId)
        {
            subAssmDAL = new SubAssemblyDAL();
            var userId = GetUserId();
            var result = subAssmDAL.DeleteComponenetMaster(ComponentId, userId);

            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpPost]
        public ActionResult SubmitCompMaster(ComponentModel model)
        {
            subAssmDAL = new SubAssemblyDAL();
            ComponentModel component = new ComponentModel();
            var unitMasters = new List<UnitMaster>();

            int addCount = 0;
            addCount = subAssmDAL.SubmitComponentMaster(model);

            //component.ComponentList = subAssmDAL.getComponenetMaterData(out unitMasters);
            //component.UnitList = unitMasters;
            return RedirectToAction("ComponentMaster");
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
