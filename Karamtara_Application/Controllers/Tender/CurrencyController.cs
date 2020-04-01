using Karamtara_Application.DAL;
using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers.Tender
{
    public class CurrencyController : Controller
    {
        public CurrencyModel model;
        public CurrencyDAL dal = new CurrencyDAL();
        // GET: Master
        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Views/Tender/Currency/CreateCurrency.cshtml", new CurrencyModel());
        }

        [HttpPost]
        public ActionResult CreateMaster(string name)
        {
            model = new CurrencyModel() { Name = name };
            model = dal.CreateMaster(model);
            return Json(model);
        }

        public ActionResult GetMaster()
        {
            var result = dal.GetList();

            return View("~/Views/Tender/Currency/List.cshtml", result);
        }
        [HttpPost]
        public ActionResult DeleteMaster(int id)
        {
            model = new CurrencyModel() { Id = id};
            var result = dal.DeleteMaster(model);
            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        [HttpPost]
        public JsonResult SearchMasterByPrefix(string prefix)
        {
            var result = dal.GetList(prefix);
            return Json(result);
        }

        [HttpPost]
        public ActionResult GetMasterByPrefix(string prefix)
        {
            var result = dal.GetList(prefix);
            return PartialView("~/Views/Tender/Currency/List.cshtml", result);
        }
    }
}