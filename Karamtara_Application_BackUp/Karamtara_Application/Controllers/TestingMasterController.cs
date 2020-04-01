using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class TestingMasterController : Controller
    {
        public TestDAL testDal;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            var model = new TestMasterModel();
            testDal = new TestDAL();
            model.TestList = testDal.GetAllTests();
            return View("TestingMaster", model);
        }

        public ActionResult GetAssembliesAutoComplete(string prefix)
        {
            testDal = new TestDAL();
            var data = testDal.GetAssembliesAutoComplete(prefix);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitTest(int ProductId,int Type,int BOMId, List<string> values)
        {
            var model = new TestMasterModel();
            testDal = new TestDAL();
            var value = "";
            int temp = 0;
            //var temp = new  List<char>();
            if (values != null)
            {
                for (int i = 0; i < values.Count; i = i + 6)
                {
                    value = values[i];
                    var data = testDal.SubmitTestData(ProductId, Type, BOMId, value);
                    temp = data;
                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            return Json(temp, JsonRequestBehavior.AllowGet);
            //model.TestList = testDal.GetAllTests();
            //return View("TestingMaster", model);
            //return View();
        }

        [HttpPost]
        public ActionResult CreateTests(FormCollection form)
        {
            testDal = new TestDAL();
            var result = testDal.CreateTests(form);

            if (result > 0)
                return Json(true);
            else
                return Json(false);
        }

        //public ActionResult AddTestMaster()
        //{
        //    var testDal = new SubAssemblyDAL();
        //    var data = testDal.GetComponents();
        //    return Json(data.ComponenetList, JsonRequestBehavior.AllowGet);
        //    //var model = new TestMasterModel();
        //    //testDal = new TestDAL();
        //    //model = testDal.GetTestMasterdata();
        //    //return PartialView("~/Views/Shared/TestMaster/_AddTests.cshtml", model);
        //    //var result = testDal.CreateComponents(form);

        //    //if (result > 0)
        //    //    return Json(true);
        //    //else
        //    //    return Json(false);
        //}
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