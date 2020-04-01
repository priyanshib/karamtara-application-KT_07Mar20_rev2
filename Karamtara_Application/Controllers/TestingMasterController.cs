using Karamtara_Application.DAL;
using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class TestingMasterController : BaseController
    {
        public TestDAL testDal;

        [NonAction]
        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        [NonAction]
        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }

        public ActionResult Index()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            var model = new TestMasterModel();
            testDal = new TestDAL();
            model = testDal.GetTestMasterData();
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

            var model = testDal.GetTestList();
            
            return Json(new
            {
                Status = result,
                AjaxReturn = ViewToString.RenderRazorViewToString(this, "~/Views/Shared/TestMaster/_TestList.cshtml", model)
        });
        }

        #region Test and BOM relation

        public ActionResult TestRelation(int bomId = 1, int revNo = 1)
        {
            var userId = GetCurrentUserId();
            testDal = new TestDAL();
            var data = testDal.GetMasterRelationData(bomId, revNo, userId);
            return View("BOMTestDetails", data);
        }

        [HttpPost]
        public ActionResult GetTestDataForProduct(ParameterModel param)
        {
            testDal = new TestDAL();
            var data = testDal.GetTestDetails(param);
            return PartialView("~/Views/Shared/TestMaster/_BOMTestRelation.cshtml", data);
        }

        [HttpPost]
        public ActionResult SubmitTestRelation(TestMasterModel model)
        {
            testDal = new TestDAL();
            var result = testDal.UpdateTestDetails(model);
            return Json(result);
        }

        #endregion

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