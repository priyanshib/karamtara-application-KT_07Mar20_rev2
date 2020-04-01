using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class ComponentMasterController : Controller
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
        public ActionResult ComponentMaster()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            subAssmDAL = new SubAssemblyDAL();
            ComponentModel component = new ComponentModel();
            component.ComponentList = subAssmDAL.getComponenetMaterData();
            component.ComponentList = component.ComponentList.OrderBy(x => x.ComponentName).ToList();
            return View("ComponentMaster", component);
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
            int addCount = 0;
            //  ComponentId, ComponentName, RawMaterialId, Size, Qty, UnitGrWt, UnitNetWt, GalvanizedRequired, GalvanizedMaterial, DrawingNo, MaterialGrade
            addCount = subAssmDAL.SubmitComponentMaster(model);
            component.ComponentList = subAssmDAL.getComponenetMaterData();
            return RedirectToAction("ComponentMaster");
            //return View("ComponentMaster", component);
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
