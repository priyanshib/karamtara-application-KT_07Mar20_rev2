using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Karamtara_Application.DAL;
using Karamtara_Application.Models;

namespace Karamtara_Application.Controllers
{
    public class MasterController : Controller
    {
        public MasterDataModel mModel;
        public MasterDAL mDAL;
        // GET: Master
        [HttpGet]
        public ActionResult Index()
        {
            mModel = new MasterDataModel();
            if (IsTender())
            {
                mModel.MasterTypeList = mModel.MasterTypeList.Where(x => x.IsTender == true).ToList();
            }
            else
            {
                mModel.MasterTypeList = mModel.MasterTypeList.Where(x => x.IsTender == false).ToList();
            }
            return View("Index", mModel);
        }

        [HttpPost]
        public ActionResult CreateMaster(string name, int type, int id)
        {
            mModel = new MasterDataModel() { Name = name, Type = type, Id = id };
            mDAL = new MasterDAL();
            mModel = mDAL.CreateMaster(mModel);
            //if (result > 0)
            //    return Json(true);
            //else
            //    return Json(false);
            return Json(mModel);
        }

        public ActionResult GetMaster(int type)
        {
            mDAL = new MasterDAL();
            var result = mDAL.GetListbyType(type);

            return View("List", result);
        }
        [HttpPost]
        public ActionResult DeleteMaster(int id, int type)
        {
            mModel = new MasterDataModel() { Id = id, Type = type };
            mDAL = new MasterDAL();
            var result = mDAL.DeleteMaster(mModel);
            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpPost]
        public JsonResult SearchMasterByPrefix(string prefix, int type)
        {
            mDAL = new MasterDAL();
            var result = mDAL.GetListbyType(type, prefix);
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetMasterByPrefix(string prefix, int type)
        {
            mDAL = new MasterDAL();
            var result = mDAL.GetListbyType(type, prefix);
            return PartialView("List", result);
        }

        public bool IsTender()
        {
            bool isTender = false;
            var userInfo = (Karamtara_Application.Models.UserModel)Session["UserData"];
            if (userInfo != null)
            {
                switch (userInfo.UserType.ToLower())
                {
                    case "tender":
                        {
                            isTender = true;
                            break;
                        }
                    case "tender admin":
                        {
                            isTender = true;
                            break;
                        }
                }
            }
            return isTender;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = this.RedirectToAction("GenError", "Error");
            filterContext.ExceptionHandled = true;
        }
    }
}