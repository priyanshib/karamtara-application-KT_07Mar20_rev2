using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Karamtara_Application.Models;
using Karamtara_Application.DAL;

namespace Karamtara_Application.Controllers
{
    public class MasterController : Controller
    {
        MasterDataModel MasterModel;
        MasterDAL MasterDal;

        // GET: Master
        public ActionResult Index(MasterDataModel model)
        {
            return View("Master", model);
        }

        [HttpPost]
        public ActionResult CreateMaster(string name, int type)
        {
            MasterModel = new MasterDataModel() { Name = name, Type = type };
            MasterDal = new MasterDAL();
            var result = MasterDal.SaveMaster(MasterModel);
            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        public JsonResult GetAllMaster(string prefix)
        {
            MasterDAL MDal = new MasterDAL();
            var data = MDal.GetMasterTypes(prefix);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult MasterList(int type)
        {
            MasterDAL MDal = new MasterDAL();
            MasterListModel model = new MasterListModel();
            model.MasterDataList = MDal.GetMasterList(type);
            return PartialView("_MasterPartial", model);
        }

        public ActionResult DeleteMaster(int MasterId, string TableName)
        {
            MasterDAL MDal = new MasterDAL();
            int result = 0;
            result = MDal.DeleteFromMaster(MasterId, TableName);

            return Json(result);
        }


    }
}