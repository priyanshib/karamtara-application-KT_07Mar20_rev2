using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class TenderStructureController : Controller
    {

        // GET: TenderStructure
        public ActionResult Index()
        {
            TenderStructureModel model = new TenderStructureModel();
         
               
            return View("~/Views/Tender/TenderStructure/TenderStructure.cshtml",model);
        }

        [HttpPost]
        public ActionResult SaveLine()
        {
            int status = 0;
            return Json(status);
        }

        [HttpPost]
        public ActionResult SaveLot()
        {
            int status = 0;
            return Json(status);
        }

        [HttpPost]
        public ActionResult SavePackage()
        {
            int status = 0;
            return Json(status);
        }
    }
}