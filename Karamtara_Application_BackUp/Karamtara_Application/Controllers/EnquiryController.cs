using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.Controllers
{
    public class EnquiryController : Controller
    {
        public EnquiryDAL enqDAL;
        public EnquiryModel enqModel;
        private int UserId;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
            {
                UserId = userInfo.UserId;
                return true;
            }
            else
                return false;
        }

        public int GetCurrentUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }

        [HttpGet]
        public ActionResult CreateEnquiry()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            enqDAL = new EnquiryDAL();
            enqModel = new EnquiryModel();
            var filterData = enqDAL.GetFilterList();
            enqModel = enqDAL.GetEnquiryDetails(UserId);
            enqModel.CountryList = enqDAL.GetCountries();
            enqModel.Filter = filterData;
            return View("Enquiry", enqModel);
        }

        [HttpPost]
        //[Route]
        public ActionResult CreateEnquiry(FormCollection form, bool IsPublished = false)
        {
            List<HttpPostedFileBase> boqFiles = new List<HttpPostedFileBase>();
            List<HttpPostedFileBase> projSpecFiles = new List<HttpPostedFileBase>();
            List<HttpPostedFileBase> otherFiles = new List<HttpPostedFileBase>();
            HttpPostedFileBase enqAttachment = null;

            if (Request != null)
            {
                boqFiles = Request.Files.GetMultiple("BoqFile").ToList();
                projSpecFiles = Request.Files.GetMultiple("ProjectSpecFile").ToList();
                otherFiles = Request.Files.GetMultiple("OtherFile").ToList();
                enqAttachment = Request.Files.Get("EnquiryAttachment");
            }

            enqDAL = new EnquiryDAL();
            enqModel = new EnquiryModel();
            var userId = GetCurrentUserId();
            enqModel = enqDAL.CreateEnquiry(userId, IsPublished, form, boqFiles, projSpecFiles, otherFiles, enqAttachment);

            return RedirectToAction("CreateEnquiry");
        }

        [HttpPost]
        public JsonResult SubmitTechnicalQuery(int id, string query)
        {
            enqDAL = new EnquiryDAL();
            var result = enqDAL.SubmitTechnicalQuery(id, query);
            //enqModel = enqDAL.GetEnquiryDetails();
            return Json(result);
        }

        [HttpGet]
        public ActionResult EditProject(int projectId)
        {
            enqDAL = new EnquiryDAL();
            enqModel = new EnquiryModel();
            enqModel = enqDAL.GetEnquiryDetailsWithId(projectId);
            enqModel.CountryList = enqDAL.GetCountries();
            enqModel.IsEdit = true;
            return PartialView("~/Views/Shared/Enquiry/_EditEnquiry.cshtml", enqModel);
        }

        [HttpPost]
        public ActionResult EditProject(FormCollection form, bool IsPublished = false)
        {
            List<HttpPostedFileBase> boqFiles = new List<HttpPostedFileBase>();
            List<HttpPostedFileBase> projSpecFiles = new List<HttpPostedFileBase>();
            List<HttpPostedFileBase> otherFiles = new List<HttpPostedFileBase>();
            HttpPostedFileBase enqAttachment = null;
            if (Request != null)
            {
                boqFiles = Request.Files.GetMultiple("BoqFile").ToList();
                projSpecFiles = Request.Files.GetMultiple("ProjectSpecFile").ToList();
                otherFiles = Request.Files.GetMultiple("OtherFile").ToList();
                enqAttachment = Request.Files.Get("EnquiryAttachment");
            }

            enqDAL = new EnquiryDAL();
            var userId = GetCurrentUserId();
            int status = enqDAL.EditEnquiry(userId, form, IsPublished, boqFiles, projSpecFiles, otherFiles, enqAttachment);

            return RedirectToAction("CreateEnquiry");
        }

        public ActionResult CancelProjectData(FormCollection form, bool IsPublished = false)
        {
            enqDAL = new EnquiryDAL();
            var userId = GetCurrentUserId();
           // int status = enqDAL.CancelEnquiry(userId, form, IsPublished);

            return PartialView("~/Views/Shared/Enquiry/_EditEnquiry.cshtml", enqModel);
        }

        [HttpGet]
        public ActionResult CreateBOMId(int enqId)
        {
            enqDAL = new EnquiryDAL();
            int status = 0;
            var userId = GetCurrentUserId();
            status = enqDAL.CreateBOMId(enqId, userId);
            return Json(status,JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetTechQuery(int enqId)
        {
            EnquiryModel enqModel = new EnquiryModel();
            enqDAL = new EnquiryDAL();
            enqModel.TechQueryList = enqDAL.GetTechnicalQueryList(enqId);
            //return Json(enqModel);
            return PartialView("~/Views/Shared/Enquiry/_technicalQuery.cshtml", enqModel);
        }

        [HttpPost]
        public ActionResult SubmitTechQuery(FormCollection form)
        {
            EnquiryModel enqModel = new EnquiryModel();
            var answers = form["x.Answer"];
            string pattern = "~!,";
            var AnsId = form["x.Id"];

            if (answers != null && answers != "~!")
            {
                if (AnsId != null)
                {
                    var enqId = Convert.ToInt32(form["EnquiryId"]);
                    var techAnswer = answers.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                    techAnswer = techAnswer.Select(x => x = x.Replace("~!", "")).ToList();
                    var techAnswerId = AnsId.Split(new string[] { "," }, StringSplitOptions.None).ToList();
                    techAnswerId = techAnswerId.Select(x => x = x.Replace(",", "")).ToList();

                    enqDAL = new EnquiryDAL();
                    int status = enqDAL.SubTechnicalQueryAnswer(techAnswer, techAnswerId, enqId, out bool allAnswered);
                    var returnObject = new { Status = status, AllAnswered = allAnswered };
                    if (status != 0)
                    {
                        return Json(returnObject);
                    }
                    else
                    {
                        return Json(returnObject);
                    }
                }
            }
            else
            {
                return Json(false);
            }
            return Json(false);
        }

        public ActionResult ExcelDownload()
        {
            return View("Excel_download");
        }

        public ActionResult DownloadExcel()
        {
            enqDAL = new EnquiryDAL();
            Response.Clear();
            Response.BinaryWrite(enqDAL.GetExcel(GetDownloadExcel()));
            //var dt = enqDAL.GetDownloadedExcel(1);
            Response.AddHeader("content-disposition", "attachment;filename=MasterModel.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.Flush();
            Response.End();
            //GridView gv = new GridView();
            //gv.DataSource = dt;
            //gv.DataBind();
            //Response.Clear();
            //Response.AddHeader("content-disposition", "attachment;filename=MasterModel.xls");
            //Response.ContentType = "application/vnd.ms-excel";
            //StringWriter sw = new StringWriter();
            //HtmlTextWriter hw = new HtmlTextWriter(sw);
            //gv.RenderControl(hw);
            //Response.Output.Write(sw.ToString());
            //Response.End();


            return View();
        }

        public List<MasterModel> GetDownloadExcel()
        {
            List<MasterModel> masterModels = new List<MasterModel>();
            MasterModel masterModel = new MasterModel();
            enqDAL = new EnquiryDAL();
            var dt = enqDAL.GetDownloadedExcel(1);
            return dt;
        }

        public ActionResult DeleteProject(int projectId)
        {
            enqDAL = new EnquiryDAL();
            var userId = GetCurrentUserId();
            enqDAL.DeleteProject(projectId, userId);
            return RedirectToAction("CreateEnquiry");
        }

        public ActionResult SearchEnquiries(string prefix)
        {
            EnquiryModel enqModel = new EnquiryModel();
            enqDAL = new EnquiryDAL();
            enqModel = enqDAL.SearchEnquiries(prefix);
            return PartialView("~/Views/Shared/Enquiry/_EnquiryList.cshtml", enqModel);
        }

        public ActionResult PublishProject(int projectId)
        {
            var userId = GetCurrentUserId();
            enqDAL = new EnquiryDAL();
            var result = enqDAL.PublishProject(projectId, userId);
            return Json(result);
        }

        public ActionResult PublishEnquiry(int enqId)
        {
            var userId = GetCurrentUserId();
            enqDAL = new EnquiryDAL();
            var result = enqDAL.PublishEnquiry(enqId, userId);
            return Json(result);
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

        public ActionResult GetAssignModal(int enquiryId)
        {
            AssignToModel model = new AssignToModel();
            enqDAL = new EnquiryDAL();
            model = enqDAL.GetAssignModal(enquiryId);
            return PartialView("~/Views/Shared/Enquiry/_AssignView.cshtml", model);
        }

        [HttpPost]
        public ActionResult AssignEnquiry(FormCollection form)
        {
            enqDAL = new EnquiryDAL();
            bool status = enqDAL.SetAssignee(form);
            return Json(status);
        }

        public ActionResult DownloadEnquiryDocs(int projectId, int enquiryId, string docType, string file)
        {
            DocumentDAL docDAL = new DocumentDAL();

            if (!Enum.TryParse(docType, out DocumentType doc))
                return HttpNotFound();

            var fileBytes = docDAL.DocumentDownload(projectId, enquiryId, doc, file, out string fileName);

            if(fileBytes == null)
                return HttpNotFound();

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult EnquiryFilter(int columnId, int orderId)
        {
            enqDAL = new EnquiryDAL();
            var userId = GetCurrentUserId();
            var data = enqDAL.FilterDataSelection(columnId, orderId, userId);
            return PartialView("~/Views/Shared/Enquiry/_EnquiryList.cshtml", data);
        }
    }
}