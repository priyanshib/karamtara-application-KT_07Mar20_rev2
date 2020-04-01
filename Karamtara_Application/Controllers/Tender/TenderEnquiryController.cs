using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.Controllers
{
    [OutputCache(Duration = 0)]
    public class TenderEnquiryController : Controller
    {
        public TenderEnquiryDAL enqDAL;
        public EnquiryModel enqModel;
        public EnquiryCommonDAL commonDAL;
        private int UserId;
        private UserModel _user;

        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
            {
                _user = userInfo;
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
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult CreateEnquiry()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");

            if (_user.UserType.ToLower() == "design" || _user.UserType.ToLower() == "design admin")
                return RedirectToAction("EnquiryList", "Enquiry");

            enqDAL = new TenderEnquiryDAL();
            enqModel = new EnquiryModel();
            commonDAL = new EnquiryCommonDAL();
            
            var filterData = commonDAL.GetFilterList(UserId);
            enqModel = enqDAL.GetEnquiryDetails(UserId);
            enqModel.CountryList = enqDAL.GetCountries();
            enqModel.ColumnId = filterData.Columns.Where(x => x.IsSelected).Select(y => y.ColumnId).FirstOrDefault();
            enqModel.OrderId = filterData.Orders.Where(x => x.IsSelected).Select(y => y.OrderId).FirstOrDefault();
            enqModel.Filter = filterData;

            return View("~/Views/Tender/Enquiry/TenderEnquiry.cshtml", enqModel);
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0)]
        //[Route]
        public ActionResult CreateEnquiry(FormCollection form, bool IsPublished = false)
        {
            EnquiryMediaFiles files = new EnquiryMediaFiles();
            if (Request != null)
            {
                files.BOQFiles= Request.Files.GetMultiple("BoqFile").ToList();
                files.ProjectSpecificationFiles = Request.Files.GetMultiple("ProjectSpecFile").ToList();
                files.OtherAttachmentFiles = Request.Files.GetMultiple("OtherFile").ToList();
                files.ProjectAttachment= Request.Files.Get("EnquiryAttachment");
            }

            enqDAL = new TenderEnquiryDAL();
            enqModel = new EnquiryModel();
            var userId = GetCurrentUserId();
            enqModel = enqDAL.CreateEnquiry(userId, IsPublished, form, files);

            return RedirectToAction("CreateEnquiry");
        }

        [HttpPost]
        [Route]
        public JsonResult SubmitTechnicalQuery(/*int id, string query*/)
        {
            commonDAL = new EnquiryCommonDAL();
            TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
            var abc = Request.Files;
            var tempData = Request.Files.GetMultiple("file").ToList();
            var id = Convert.ToInt32(Request.Params.Get("id"));
            var query = Request.Params.Get("comment").ToString();
            var userId = GetCurrentUserId();
            technicalQuery = commonDAL.SendMailList(userId);
            var result = commonDAL.SubmitTechnicalQuery(id, query, tempData, userId, technicalQuery);
            return Json(result);
        }

        [HttpGet]
        public ActionResult EditProject(int projectId)
        {
            enqDAL = new TenderEnquiryDAL();
            enqModel = new EnquiryModel();
            enqModel = enqDAL.GetEnquiryDetailsWithId(projectId);
            enqModel.CountryList = enqDAL.GetCountries();
            enqModel.IsEdit = true;
            return PartialView("~/Views/Tender/Shared/EnquiryMaster/_EditEnquiry.cshtml", enqModel);
        }

        [HttpPost]
        public ActionResult EditProject(FormCollection form, bool IsPublished = false)
        {
            //List<HttpPostedFileBase> boqFiles = new List<HttpPostedFileBase>();
            //List<HttpPostedFileBase> projSpecFiles = new List<HttpPostedFileBase>();
            //List<HttpPostedFileBase> otherFiles = new List<HttpPostedFileBase>();
            //HttpPostedFileBase enqAttachment = null;
            EnquiryMediaFiles files = new EnquiryMediaFiles();
            if (Request != null)
            {
                files.BOQFiles = Request.Files.GetMultiple("BoqFile").ToList();
                files.ProjectSpecificationFiles = Request.Files.GetMultiple("ProjectSpecFile").ToList();
                files.OtherAttachmentFiles = Request.Files.GetMultiple("OtherFile").ToList();
                files.ProjectAttachment = Request.Files.Get("EnquiryAttachment");
            }

            enqDAL = new TenderEnquiryDAL();
            var userId = GetCurrentUserId();
            int status = enqDAL.EditEnquiry(userId, form, IsPublished, files);

            return RedirectToAction("CreateEnquiry");
        }

        public ActionResult CancelProjectData(FormCollection form, bool IsPublished = false)
        {
            enqDAL = new TenderEnquiryDAL();
            var userId = GetCurrentUserId();
           // int status = enqDAL.CancelEnquiry(userId, form, IsPublished);

            return PartialView("~/Views/Tender/Shared/EnquiryMaster/_EditEnquiry.cshtml", enqModel);
        }

        [HttpGet]
        public ActionResult CreateBOMId(int enqId)
        {
            enqDAL = new TenderEnquiryDAL();
            int status = 0;
            var userId = GetCurrentUserId();
            status = enqDAL.CreateBOMId(enqId, userId);
            return Json(status,JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult GetTechQuery(int enqId)
        {
            EnquiryModel enqModel = new EnquiryModel();
            commonDAL = new EnquiryCommonDAL();

            enqModel.TechQueryList = commonDAL.GetTechnicalQueryList(enqId);
            return PartialView("~/Views/Tender/Shared/EnquiryMaster/_technicalQuery.cshtml", enqModel);
        }

        [HttpPost]
        public ActionResult SubmitTechQuery()
        {
            TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
            EnquiryModel enqModel = new EnquiryModel();
            commonDAL = new EnquiryCommonDAL();

            var abc = Request.Files;
            var tempData = Request.Files.GetMultiple("file").ToList();
            var AnsId = Convert.ToInt32(Request.Params.Get("id"));
            var answers = Request.Params.Get("Answer").ToString();
            var query = Request.Params.Get("Query").ToString();
            var enqId = Request.Params.Get("EnquiryId").ToString();
            var userId = GetCurrentUserId(); 
            technicalQuery = commonDAL.SendMailList(userId);
            technicalQuery.Query = query;

            int status = commonDAL.SubTechnicalQueryAnswer(AnsId,answers,Convert.ToInt32(enqId), tempData, userId, technicalQuery, out bool allAnswered);
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

        public ActionResult ExcelDownload()
        {
            return View("Excel_download");
        }

        public ActionResult DownloadExcel()
        {
            commonDAL = new EnquiryCommonDAL();

            Response.Clear();
            Response.BinaryWrite(commonDAL.GetExcel(GetDownloadExcel()));
            Response.AddHeader("content-disposition", "attachment;filename=MasterModel.xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.Flush();
            Response.End();
            return View();
        }

        public List<MasterModel> GetDownloadExcel()
        {
            commonDAL = new EnquiryCommonDAL();
            var dt = commonDAL.GetDownloadedExcel(1);
            return dt;
        }

        public ActionResult DeleteProject(int projectId)
        {
            enqDAL = new TenderEnquiryDAL();
            var userId = GetCurrentUserId();
            enqDAL.DeleteProject(projectId, userId);
            return RedirectToAction("CreateEnquiry");
        }

        public ActionResult SearchEnquiries(string prefix)
        {
            EnquiryModel enqModel = new EnquiryModel();
            enqDAL = new TenderEnquiryDAL();
            commonDAL = new EnquiryCommonDAL();

            var userId = GetCurrentUserId();
            enqModel = enqDAL.SearchEnquiries(prefix, userId);
            enqModel.Filter = commonDAL.GetFilterList(userId);
            enqModel.ColumnId = enqModel.Filter.Columns.Where(x => x.IsSelected).Select(y => y.ColumnId).FirstOrDefault();
            enqModel.OrderId = enqModel.Filter.Orders.Where(x => x.IsSelected).Select(y => y.OrderId).FirstOrDefault();

            return PartialView("~/Views/Tender/Shared/EnquiryMaster/_EnquiryList.cshtml", enqModel);
        }

        public ActionResult PublishProject(int projectId)
        {
            var userId = GetCurrentUserId();
            enqDAL = new TenderEnquiryDAL();
            var result = enqDAL.PublishProject(projectId, userId);
            return Json(result);
        }

        public ActionResult PublishEnquiry(int enqId)
        {
            var userId = GetCurrentUserId();
            enqDAL = new TenderEnquiryDAL();
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

        public ActionResult SendTechQueryMail(string filename)
        {
            email_send();
            return View();
        }

        public void email_send()

        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("manisha.g@mobinexttech.com");
            mail.To.Add("saranya.s@mobinexttech.com");
            mail.Subject = "Test Mail - 1";
            mail.Body = "mail with attachment";

            System.Net.Mail.Attachment attachment;

			attachment = new System.Net.Mail.Attachment("d:/Karamtara Application/karamtara-v3.pdf"); //c:/ textfile.txt
            mail.Attachments.Add(attachment);
            
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("manisha.g@mobinexttech.com", "mmbhalkar");
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);

        }

        public ActionResult DownloadAttachTechDocs(int enquiryId, string file)
        {
            DocumentDAL docDAL = new DocumentDAL();
            commonDAL = new EnquiryCommonDAL();
            string ResponeFileName = commonDAL.getResponseFileName(file);
            //if (!Enum.TryParse(docType, out DocumentType doc))
            //    return HttpNotFound();

            var fileBytes = docDAL.QueryDocumentDownload(enquiryId,  file, ResponeFileName, out string fileName);

            if (fileBytes == null)
                return HttpNotFound();

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult DownloadAttachDocs(int enquiryId, string file)
        {
            DocumentDAL docDAL = new DocumentDAL();
            commonDAL = new EnquiryCommonDAL();
            string QueryFileName = commonDAL.getQueryFileName(file);

            var fileBytes = docDAL.QueryAttachDocumentDownload(enquiryId, file, QueryFileName, out string fileName);

            if (fileBytes == null)
                return HttpNotFound();

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult EnquiryFilter(int columnId, int orderId)
        {
            enqDAL = new TenderEnquiryDAL();
            var userId = GetCurrentUserId();
            var data = enqDAL.FilterDataSelection(columnId, orderId, userId);
            return PartialView("~/Views/Tender/Shared/EnquiryMaster/_EnquiryList.cshtml", data);
        }
    }
}