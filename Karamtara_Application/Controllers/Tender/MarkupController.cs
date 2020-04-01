using Karamtara_Application.DAL;
using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers.Tender
{
    public class MarkupController : BaseController
    {
        public MarkupModel mModel;
        public MarkupDAL mDAL = new MarkupDAL();
        // GET: Master
        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Views/Tender/Markup/CreateMarkup.cshtml", new MarkupModel());
        }

        [HttpPost]
        public ActionResult CreateMaster(string desc, decimal value)
        {
            mModel = new MarkupModel() { Description = desc, Value = value };
            mModel = mDAL.CreateMaster(mModel);
            return Json(mModel);
        }

        public ActionResult GetMaster()
        {
            var result = mDAL.GetList();

            return View("~/Views/Tender/Markup/List.cshtml", result);
        }
        [HttpPost]
        public ActionResult DeleteMaster(int id)
        {
            mModel = new MarkupModel() { Id = id};
            var result = mDAL.DeleteMaster(mModel);
            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpPost]
        public JsonResult SearchMasterByPrefix(string prefix)
        {
            var result = mDAL.GetList(prefix);
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetMasterByPrefix(string prefix)
        {
            var result = mDAL.GetList(prefix);
            return PartialView("~/Views/Tender/Markup/List.cshtml", result);
        }
    }
}