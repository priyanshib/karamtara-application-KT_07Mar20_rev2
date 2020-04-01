using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.DAL
{
    public class EnquiryDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public EnquiryModel GetEnquiryDetails(int userId)
        {
            EnquiryModel enquiryModel = new EnquiryModel();
            List<EnquiryModel> enqList = new List<EnquiryModel>();
            List<CustomerEnquiryModel> custEnqList = new List<CustomerEnquiryModel>();
            List<TechnicalQueryModel> techQueryList = new List<TechnicalQueryModel>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_GetEnquiryList", connection);
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        EnquiryModel enqModel = new EnquiryModel();
                        enqModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectId"]);
                        enqModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[i]["ProjectName"]);
                        enqModel.TenderFileNo = Convert.ToString(ds.Tables[0].Rows[i]["TenderFileNo"]);
                        enqModel.EnquiryType = Convert.ToString(ds.Tables[0].Rows[i]["EnqType"]);
                        enqModel.EndCustName = Convert.ToString(ds.Tables[0].Rows[i]["EndCustName"]);
                        enqModel.EnqDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDate"]);
                        enqModel.EnqDueDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDueDate"]);
                        enqModel.StatusDesc = Convert.ToString(ds.Tables[0].Rows[i]["StatusDesc"]);
                        enqModel.EnquiryAttachmentName = Convert.ToString(ds.Tables[0].Rows[i]["Attachment"]);
                        enqModel.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsPublished"]);
                        enqModel.EnquiryTypeId = enqModel.EnquiryType == "Domestic" ? 1 : 2;
                        enqList.Add(enqModel);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        CustomerEnquiryModel custEnquiry = new CustomerEnquiryModel();
                        custEnquiry.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        custEnquiry.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        custEnquiry.EpCCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        custEnquiry.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        custEnquiry.ExpiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        custEnquiry.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        custEnquiry.BoqFileName = Convert.ToString(ds.Tables[1].Rows[i]["BoqFile"]);
                        custEnquiry.ProjectSpecFileName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectSpecFile"]);
                        custEnquiry.OtherFileName = Convert.ToString(ds.Tables[1].Rows[i]["OtherFile"]);
                        custEnquiry.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        custEnquiry.CanCreateBOM = Convert.ToBoolean(ds.Tables[1].Rows[i]["CanCreateBOM"]);
                        custEnquiry.IsPublished = Convert.ToBoolean(ds.Tables[1].Rows[i]["IsPublished"]);
                        custEnquiry.TechnicalQuery = new List<TechnicalQueryModel>();
                        custEnqList.Add(custEnquiry);
                    }
                }
                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        //technicalQuery.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                        technicalQuery.Answer = Convert.ToString(ds.Tables[2].Rows[i]["Answer"]);
                        techQueryList.Add(technicalQuery);
                    }
                }
                custEnqList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y=> y.EnquiryId == x.EnquiryId)));
                //custEnquiry.TechnicalQuery = techQueryList;
                enqList.ForEach(x => x.StatusDesc = (custEnqList.Where(y => y.ProjectId == x.ProjectId).All(z => z.IsPublished) ?
                "Published" : (custEnqList.Where(y => y.ProjectId == x.ProjectId).Any(z => z.IsPublished) ? "Partially Published" : "New")));

                enquiryModel.EnquiryList = enqList;
                enquiryModel.CustomerList = custEnqList;
                return enquiryModel;
            }
            catch (Exception ex)
            {
                return enquiryModel;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<CountryModel> GetCountries()
        {
            List<CountryModel> countryList = new List<CountryModel>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_GetCountries", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        CountryModel model = new CountryModel();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.CountryName= Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        countryList.Add(model);
                    }
                }
                return countryList;
            }
            catch (Exception ex)
            {
                return countryList;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public EnquiryModel GetEnquiryDetailsWithId(int projectId)
        {
            EnquiryModel enquiryModel = new EnquiryModel();
            List<CustomerEnquiryModel> custEnqList = new List<CustomerEnquiryModel>();
            List<EnquiryModel> enqList = new List<EnquiryModel>();
            List<TechnicalQueryModel> techQueryList = new List<TechnicalQueryModel>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_GetEnquiry", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projectId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    EnquiryModel enqModel = new EnquiryModel();
                    enqModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[0]["ProjectId"]);
                    enqModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                    enqModel.TenderFileNo = Convert.ToString(ds.Tables[0].Rows[0]["TenderFileNo"]);
                    enqModel.EnquiryType = Convert.ToString(ds.Tables[0].Rows[0]["EnqType"]);
                    enqModel.EndCustName = Convert.ToString(ds.Tables[0].Rows[0]["EndCustName"]);
                    enqModel.EnqDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EnqDate"]);
                    enqModel.EnqDueDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EnqDueDate"]);
                    enqModel.StatusDesc = Convert.ToString(ds.Tables[0].Rows[0]["StatusDesc"]);
                    enqModel.LineNumber = Convert.ToInt32(ds.Tables[0].Rows[0]["LinesNo"]);
                    enqModel.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsPublished"] ?? 0);
                    enqModel.EnquiryAttachmentName = Convert.ToString(ds.Tables[0].Rows[0]["Attachment"]);
                    enqModel.Summary = Convert.ToString(ds.Tables[0].Rows[0]["Summary"]);
                    enqModel.EnquiryTypeId = enqModel.EnquiryType == "Domestic" ? 1 : 2;
                    enquiryModel = enqModel;
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        CustomerEnquiryModel custEnquiry = new CustomerEnquiryModel();
                        custEnquiry.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        custEnquiry.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        custEnquiry.EpCCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        custEnquiry.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        custEnquiry.ExpiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        custEnquiry.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        custEnquiry.BoqFileName = Convert.ToString(ds.Tables[1].Rows[i]["BoqFile"]);
                        custEnquiry.ProjectSpecFileName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectSpecFile"]);
                        custEnquiry.OtherFileName = Convert.ToString(ds.Tables[1].Rows[i]["OtherFile"]);
                        custEnquiry.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        custEnquiry.CountryId = Convert.ToInt32(ds.Tables[1].Rows[i]["CountryId"]);
                        custEnqList.Add(custEnquiry);
                        enquiryModel.CustomerList = new List<CustomerEnquiryModel>();
                        enquiryModel.CustomerList.AddRange(custEnqList);
                    }
                }

                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        //technicalQuery.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                        technicalQuery.Answer = Convert.ToString(ds.Tables[2].Rows[i]["Answer"]);
                        techQueryList.Add(technicalQuery);
                    }
                }
                enquiryModel.CustomerList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                return enquiryModel;
            }
            catch (Exception ex)
            {
                return enquiryModel;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public MediaFileNamesModel GetFileNamesList(int projectId)
        {
            MediaFileNamesModel mediaFiles = new MediaFileNamesModel();
            mediaFiles.BoqFileName = new List<string>();
            mediaFiles.ProjectSpecFileNames = new List<string>();
            mediaFiles.OtherFileNames = new List<string>();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd = new SqlCommand("sp_GetFileNames", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projectId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        mediaFiles.ProjectAttachmentName = Convert.ToString(ds.Tables[0].Rows[0]["Attachment"] ?? string.Empty);
                    else
                        mediaFiles.ProjectAttachmentName = string.Empty;
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        mediaFiles.BoqFileName.Add(Convert.ToString(ds.Tables[1].Rows[i]["BoqFile"] ?? string.Empty));
                        mediaFiles.ProjectSpecFileNames.Add(Convert.ToString(ds.Tables[1].Rows[i]["ProjectSpecFile"] ?? string.Empty));
                        mediaFiles.OtherFileNames.Add(Convert.ToString(ds.Tables[1].Rows[i]["OtherFile"] ?? string.Empty));
                    }
                }
                return mediaFiles;
            }
            catch (Exception ex)
            {
                return mediaFiles;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public EnquiryModel CreateEnquiry(int userId, bool IsPublished, FormCollection form, List<HttpPostedFileBase> boqFiles, List<HttpPostedFileBase> projSpecFiles, List<HttpPostedFileBase> otherFiles, HttpPostedFileBase enqAttachment = null)
        {
            EnquiryModel enqModel = new EnquiryModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int projId = 0;
            try
            {
                string tenderDeptFileNo = form["TenderFileNo"];
                string projectName = form["ProjectName"];
                string enqType = form["EnquiryTypeId"];
                string endCustName = form["EndCustName"];
                string enqCreateDate = form["EnqDate"];
                string enqDueDate = form["EnqDueDate"];
                string projSpecs = form["ProjectSpecification"] ?? string.Empty;
                string lineNo = form["LineNumber"] ?? string.Empty;
                string fileAttachment = enqAttachment == null ? string.Empty : enqAttachment.FileName;
                string projSummary = form["ProjectSummary"];

                cmd = new SqlCommand("sp_CreateEnquiry", connection);
                cmd.Parameters.Add(new SqlParameter("@TenderFileNo", tenderDeptFileNo));
                cmd.Parameters.Add(new SqlParameter("@ProjectName", projectName));
                cmd.Parameters.Add(new SqlParameter("@EnquiryType", enqType));
                cmd.Parameters.Add(new SqlParameter("@EndCustName", endCustName));
                cmd.Parameters.Add(new SqlParameter("@EnqDate", DateTime.TryParseExact(enqCreateDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqDate) ? enqDate : SqlDateTime.MinValue.Value));
                cmd.Parameters.Add(new SqlParameter("@EnqDueDate", DateTime.TryParseExact(enqDueDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqOutDate) ? enqOutDate : SqlDateTime.MinValue.Value));
                cmd.Parameters.Add(new SqlParameter("@ProjSpecification", projSpecs));
                int lineNum = 0;
                cmd.Parameters.Add(new SqlParameter("@LineNo", Int32.TryParse(lineNo, out lineNum) ? lineNum : 0));
                cmd.Parameters.Add(new SqlParameter("@ProjSummary", projSummary ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@ProjAttachmentName", fileAttachment ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.Parameters.Add(new SqlParameter("@ProjStatus", IsPublished ? 4 : 1));
                cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished ? 1: 0));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                projId = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                //save attachment

                DocumentDAL docDal = new DocumentDAL();
                var attachmentName = docDal.SaveEnquiryAttachment(projId, enqAttachment);

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_SaveEnquiryAttachment", connection);

                cmd.Parameters.Add(new SqlParameter("@projectId", projId));
                cmd.Parameters.Add(new SqlParameter("@attachmentName", attachmentName));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                int status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (projId > 0)
                    AddEpcCustomers(form, projId, boqFiles, projSpecFiles, otherFiles);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return enqModel;
        }

        public int AddEpcCustomers(FormCollection form, int projId, List<HttpPostedFileBase> boqFiles, List<HttpPostedFileBase> projectSpecFiles, List<HttpPostedFileBase> otherFiles)
        {
            //int enqId = 0;
            string epcCustName = form["EPCCustomerName"];
            string custEnqDate = form["EnquiryDate"];
            string custExpDate = form["ExpiryDate"];
            string Country = form["Country"];

            int custAdd = 0;
            try
            {
                List<String> epcNameList = new List<String>(epcCustName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> custEnqDateList = new List<String>(custEnqDate.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> custExpDateList = new List<String>(custExpDate.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> countries = new List<string>(Country.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                for (int i = 0; i < epcNameList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    cmd = new SqlCommand("sp_AddEPCCustomer", connection);
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", projId));
                    cmd.Parameters.Add(new SqlParameter("@CustName", epcNameList.ElementAtOrDefault(i) != null ? epcNameList[i] : string.Empty));
                    cmd.Parameters.Add(new SqlParameter("@EnqDate", DateTime.TryParseExact((custEnqDateList.ElementAtOrDefault(i) != null ? custEnqDateList[i] : null), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqDate) ? enqDate : SqlDateTime.MinValue.Value));
                    cmd.Parameters.Add(new SqlParameter("@ExpDate", DateTime.TryParseExact((custExpDateList.ElementAtOrDefault(i) != null ? custExpDateList[i] : null), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expDate) ? expDate : SqlDateTime.MinValue.Value));
                    cmd.Parameters.Add(new SqlParameter("@Country", countries.ElementAtOrDefault(i) != null ? Convert.ToInt32(countries[i]) : 0));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    int enqId = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();

                    var boqFile = boqFiles.ElementAtOrDefault(i) != null ? boqFiles[i] : null;
                    var proSpecFile = projectSpecFiles.ElementAtOrDefault(i) != null ? projectSpecFiles[i] : null;
                    var otherFile = otherFiles.ElementAtOrDefault(i) != null ? otherFiles[i] : null;

                    DocumentDAL docDal = new DocumentDAL();
                    var names = docDal.SaveAllCustomerAttachements(projId, enqId, boqFile, proSpecFile, otherFile);
                    connection = new SqlConnection(connectionString);
                    cmd = new SqlCommand();
                    cmd = new SqlCommand("sp_AttachFiles", connection);
                    cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                    cmd.Parameters.Add(new SqlParameter("@BoqFile", names[0]));
                    cmd.Parameters.Add(new SqlParameter("@ProjectSpecFile", names[1]));
                    cmd.Parameters.Add(new SqlParameter("@OtherFile", names[2]));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    int status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return custAdd;
        }

        public int EditEnquiry(int userId, FormCollection form, bool IsPublished, List<HttpPostedFileBase> boqFiles, List<HttpPostedFileBase> projSpecFiles, List<HttpPostedFileBase> otherFiles, HttpPostedFileBase enqAttachment = null)
        {
            EnquiryModel enqModel = new EnquiryModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                int projId = Convert.ToInt32(form["ProjectId"]);
                string tenderDeptFileNo = form["TenderFileNo"];
                string projectName = form["ProjectName"];
                string enqType = form["EnquiryTypeId"];
                string endCustName = form["EndCustName"];
                string enqCreateDate = form["EnqDate"];
                string enqDueDate = form["EnqDueDate"];
                string projSpecs = form["ProjectSpecification"] ?? string.Empty;
                string lineNo = form["LineNumber"] ?? string.Empty;
                string projAttachmentName = form["EnquiryAttachmentName"];
                //string fileAttachment = enqAttachment == null ? string.Empty : enqAttachment.FileName;
                string projSummary = form["ProjectSummary"] ?? string.Empty;
                var model = GetFileNamesList(projId);

                cmd = new SqlCommand("sp_EditEnquiry", connection);
                cmd.Parameters.Add(new SqlParameter("@ProjectId", projId));
                cmd.Parameters.Add(new SqlParameter("@TenderFileNo", tenderDeptFileNo));
                cmd.Parameters.Add(new SqlParameter("@ProjectName", projectName));
                cmd.Parameters.Add(new SqlParameter("@EnquiryType", enqType));
                cmd.Parameters.Add(new SqlParameter("@EndCustName", endCustName));
                cmd.Parameters.Add(new SqlParameter("@EnqDate", DateTime.TryParseExact(enqCreateDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqDate) ? enqDate : SqlDateTime.MinValue.Value));
                cmd.Parameters.Add(new SqlParameter("@EnqDueDate", DateTime.TryParseExact(enqCreateDate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqOutDate) ? enqOutDate : SqlDateTime.MinValue.Value));
                cmd.Parameters.Add(new SqlParameter("@ProjSpecification", projSpecs));
                int lineNum = 0;
                cmd.Parameters.Add(new SqlParameter("@LineNo", Int32.TryParse(lineNo, out lineNum) ? lineNum : 0));
                cmd.Parameters.Add(new SqlParameter("@ProjSummary", projSummary ?? string.Empty));
                //cmd.Parameters.Add(new SqlParameter("@ProjAttachmentName", fileAttachment ?? string.Empty));
                cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished ? 1 : 0));
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.Parameters.Add(new SqlParameter("@ProjStatus", IsPublished ? 4 : 1));

                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var stat = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                //save attachment
                
                //DocumentDAL docDal = new DocumentDAL();
                var attachmentName = EditProjAttachment(projId, projAttachmentName, enqAttachment, DocumentType.EA, model.ProjectAttachmentName);
                    //docDal.SaveEnquiryAttachment(projId, enqAttachment);
                
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_SaveEnquiryAttachment", connection);

                cmd.Parameters.Add(new SqlParameter("@projectId", projId));
                cmd.Parameters.Add(new SqlParameter("@attachmentName", attachmentName));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                int status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (projId > 0)
                    EditEpcCustomers(form, projId, boqFiles, projSpecFiles, otherFiles, model);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return 0;
        }

        public int EditEpcCustomers(FormCollection form, int projId, List<HttpPostedFileBase> boqFiles, List<HttpPostedFileBase> projectSpecFiles, List<HttpPostedFileBase> otherFiles, MediaFileNamesModel mediaNames)
        {
            string enqIds = form["x.EnquiryId"];
            string epcCustName = form["x.EPCCustomerName"];
            string custEnqDate = form["x.EnquiryDate"];
            string custExpDate = form["x.ExpiryDate"];
            string boqName = form["x.BoqFileName"];
            string psName = form["x.ProjectSpecFileName"];
            string otherName = form["x.OtherFileName"];
            string country = form["x.CountryId"];

            int custAdd = 0;
            try
            {
                List<String> epcNameList = new List<String>(epcCustName.Split(',')).ToList();
                List<String> custEnqDateList = new List<String>(custEnqDate.Split(',')).ToList();
                List<String> custExpDateList = new List<String>(custExpDate.Split(',')).ToList();
                List<String> enquiryIds = new List<String>(enqIds.Split(',')).ToList();
                List<String> boqFileNames = new List<String>(boqName.Split(',')).ToList();
                List<String> proSpecFilesNames = new List<String>(psName.Split(',')).ToList();
                List<String> otherFileNames = new List<String>(otherName.Split(',')).ToList();
                List<String> countries = new List<String>(country.Split(',')).ToList();

                var newBoqNames = EditMedia(enquiryIds, projId, boqFileNames, boqFiles, DocumentType.Boq, mediaNames.BoqFileName);
                var newPsNames = EditMedia(enquiryIds, projId, proSpecFilesNames, projectSpecFiles, DocumentType.PS, mediaNames.ProjectSpecFileNames);
                var newOtherNames = EditMedia(enquiryIds, projId, otherFileNames, otherFiles, DocumentType.Oth, mediaNames.OtherFileNames);

                //edit customers

                for (int i = 0; i < enquiryIds.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    cmd = new SqlCommand("sp_EditCustomer", connection);
                    cmd.Parameters.Add(new SqlParameter("@ProjectId", projId));
                    cmd.Parameters.Add(new SqlParameter("@enquiryId", Convert.ToInt32(enquiryIds[i])));
                    cmd.Parameters.Add(new SqlParameter("@CustName", epcNameList.ElementAtOrDefault(i) != null ? epcNameList[i] : string.Empty));
                    cmd.Parameters.Add(new SqlParameter("@EnqDate", DateTime.TryParseExact(custEnqDateList[i], "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqDate) ? enqDate : SqlDateTime.MinValue.Value));
                    cmd.Parameters.Add(new SqlParameter("@ExpDate", DateTime.TryParseExact(custExpDateList[i], "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expDate) ? expDate : SqlDateTime.MinValue.Value));
                    cmd.Parameters.Add(new SqlParameter("@BoqFile", newBoqNames.ElementAtOrDefault(i) != null ? newBoqNames[i] : string.Empty));
                    cmd.Parameters.Add(new SqlParameter("@PsFile", newPsNames.ElementAtOrDefault(i) != null ? newPsNames[i] : string.Empty));
                    cmd.Parameters.Add(new SqlParameter("@OtherFile", newOtherNames.ElementAtOrDefault(i) != null ? newOtherNames[i] : string.Empty));
                    cmd.Parameters.Add(new SqlParameter("@CountryId", countries.ElementAtOrDefault(i) != null ? countries[i] : string.Empty));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    int status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }

                //Adding new customers

                if(epcNameList.Count > 0)
                {
                    var startIndex = 0;
                    var count = enquiryIds.Count();
                    epcNameList.RemoveRange(startIndex, count );
                    custEnqDateList.RemoveRange(startIndex, count);
                    custExpDateList.RemoveRange(startIndex, count);
                    boqFiles.RemoveRange(startIndex, count);
                    projectSpecFiles.RemoveRange(startIndex, count);
                    otherFiles.RemoveRange(startIndex, count);
                    countries.RemoveRange(startIndex, count);

                    for (int i = 0; i < epcNameList.Count; i++)
                    {
                        connection = new SqlConnection(connectionString);
                        SqlCommand cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_AddEPCCustomer", connection);
                        cmd.Parameters.Add(new SqlParameter("@ProjectId", projId));
                        cmd.Parameters.Add(new SqlParameter("@CustName", epcNameList.ElementAtOrDefault(i) != null ? epcNameList[i] : string.Empty));
                        cmd.Parameters.Add(new SqlParameter("@EnqDate", DateTime.TryParseExact((custEnqDateList.ElementAtOrDefault(i) != null ? custEnqDateList[i] : null), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime enqDate) ? enqDate : SqlDateTime.MinValue.Value));
                        cmd.Parameters.Add(new SqlParameter("@ExpDate", DateTime.TryParseExact((custExpDateList.ElementAtOrDefault(i) != null ? custExpDateList[i] : null), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expDate) ? expDate : SqlDateTime.MinValue.Value));
                        cmd.Parameters.Add(new SqlParameter("@Country", countries.ElementAtOrDefault(i) != null ? Convert.ToInt32(countries[i]) : 0));
                        cmd.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        int enqId = Convert.ToInt32(cmd.ExecuteScalar());
                        connection.Close();

                        var boqFile = boqFiles.ElementAtOrDefault(i) != null ? boqFiles[i] : null;
                        var proSpecFile = projectSpecFiles.ElementAtOrDefault(i) != null ? projectSpecFiles[i] : null;
                        var otherFile = otherFiles.ElementAtOrDefault(i) != null ? otherFiles[i] : null;

                        DocumentDAL docDal = new DocumentDAL();
                        var names = docDal.SaveAllCustomerAttachements(projId, enqId, boqFile, proSpecFile, otherFile);
                        connection = new SqlConnection(connectionString);
                        cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_AttachFiles", connection);
                        cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                        cmd.Parameters.Add(new SqlParameter("@BoqFile", names[0]));
                        cmd.Parameters.Add(new SqlParameter("@ProjectSpecFile", names[1]));
                        cmd.Parameters.Add(new SqlParameter("@OtherFile", names[2]));
                        cmd.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        int status = Convert.ToInt32(cmd.ExecuteScalar());
                        connection.Close();
                    }
                }
                
            }
            catch (Exception ex)
            {

            }
            return custAdd;
        }

        public bool SubmitTechnicalQuery(int id, string query)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AddTechnicalQuery", connection);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.Parameters.Add(new SqlParameter("@query", query));
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            int status = Convert.ToInt32(cmd.ExecuteScalar());
            connection.Close();

            if (status > 0)
                return true;
            else
                return false;
        }

        public List<string> EditMedia(List<string> enquiryIds, int projectId, List<string> fileNames, List<HttpPostedFileBase> files, DocumentType docType, List<string> currentFileNames)
        {
            List<string> names = new List<string>();
            try
            {
                for (int i = 0; i < enquiryIds.Count; i++)
                {
                    var enquiryId = Convert.ToInt32(enquiryIds[i]);
                    var projId = projectId;
                    var currentName = currentFileNames[i];
                    string newFileName = SaveOrUpdateDocument(projId, enquiryId, files[i], docType);
                    if (string.IsNullOrEmpty(newFileName))
                    {
                        var fileName = fileNames[i];
                        if (currentName != fileName)
                        {
                            newFileName = fileName;
                            RemoveFile(projectId, enquiryId, currentName);
                        }
                        else
                            newFileName = fileName;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currentName))
                            RemoveFile(projectId, enquiryId, currentName);
                    }

                    names.Add(newFileName);
                }
                return names;
            }
            catch (Exception ex)
            {
                return names;
            }
        }

        public string EditProjAttachment(int projectId, string fileName, HttpPostedFileBase file, DocumentType docType, string currentFileName)
        {
            try
            {
                DocumentDAL docDAL = new DocumentDAL();
                var projId = projectId;
                string newFileName = docDAL.SaveEnquiryAttachment(projId, file);
                if (string.IsNullOrEmpty(newFileName))
                {
                    if (currentFileName != fileName)
                    {
                        newFileName = fileName;
                        RemoveProjAttachmentFile(projectId, currentFileName);
                    }
                    else
                        newFileName = fileName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentFileName))
                        RemoveProjAttachmentFile(projectId, currentFileName);
                }

                return newFileName;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public bool RemoveFile(int projectId, int enquiryId, string oldFileName)
        {
            string path = string.Format("~/Documents/Project_{0}/Customer_{1}", projectId, enquiryId);
            //Delete file if old exists
            var filePath = Path.Combine(HostingEnvironment.MapPath(path), oldFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return true;
        }

        public bool RemoveProjAttachmentFile(int projectId, string oldFileName)
        {
            string path = string.Format("~/Documents/Project_{0}", projectId);
            //Delete file if old exists
            var filePath = Path.Combine(HostingEnvironment.MapPath(path), oldFileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return true;
        }

        public string SaveOrUpdateDocument(int projectId, int enquiryId, HttpPostedFileBase file, DocumentType documentType = DocumentType.Doc)
        {
            string fileName = string.Empty;
            string path = string.Format("~/Documents/Project_{0}/Customer_{1}", projectId, enquiryId);

            try
            {
                if (file == null || file.ContentLength <= 0)
                    return string.Empty;

                Directory.CreateDirectory(HostingEnvironment.MapPath(path));
                fileName = file.FileName;
                var extension = Path.GetExtension(fileName);
                fileName = fileName.Replace(extension, "");
                fileName = Regex.Replace(fileName, @"[^0-9a-zA-Z]+", "-");
                fileName = fileName.Substring(0, fileName.Length > 10 ? 10 : fileName.Length - 1);
                fileName = documentType.ToString() + DateTime.UtcNow.ToString("ddMMyyyyHHmmss") + Guid.NewGuid().ToString().Substring(0, 5) + extension;
                path = Path.Combine(HostingEnvironment.MapPath(path), fileName);
                file.SaveAs(path);

                return fileName;
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }

        public int CreateBOMId(int enqId,int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int bomId = 0;
            try
            {
                cmd = new SqlCommand("sp_CreateBomId", connection);
                cmd.Parameters.Add(new SqlParameter("@enquiryId", enqId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                bomId = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return bomId;
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<TechnicalQueryModel> GetTechnicalQueryList(int enqId)
        {
            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            try
            {
                cmd = new SqlCommand("sp_getTechQuery", connection);
                cmd.Parameters.Add(new SqlParameter("@enquiryId", enqId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                //bomId = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[0].Rows[i]["EnquiryId"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[0].Rows[i]["Query"]);
                        technicalQuery.Answer = Convert.ToString(ds.Tables[0].Rows[i]["Answer"]);
                        queryModels.Add(technicalQuery);
                    }
                }
                return queryModels;
            }
            catch (Exception ex)
            {
                return queryModels;
            }
        }
         
        public int SubTechnicalQueryAnswer(List<string> techAnswer, List<string> techAnswerId, int enqId, out bool allAnswered)
        {
            allAnswered = false;
            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                int n = techAnswer.Count;
                int m = techAnswerId.Count;
                int status = 0;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        if (i == j)
                        {
                            int id = Convert.ToInt32(techAnswerId[j]);
                            string answer = techAnswer[i];
                            cmd = new SqlCommand("sp_subTechQueryAns", connection);
                            cmd.Parameters.Add(new SqlParameter("Id",id));
                            cmd.Parameters.Add(new SqlParameter("Answer", answer));
                            cmd.CommandType = CommandType.StoredProcedure;
                            connection.Open();
                            status = Convert.ToInt32(cmd.ExecuteScalar());
                            connection.Close();
                        }
                    }
                }

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_CheckAllQueriesAnswered", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                connection.Open();
                allAnswered = Convert.ToInt32(cmd.ExecuteScalar()) > 0 ? true : false;
                connection.Close();

                return status;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<MasterModel> GetDownloadedExcel(int ProductGroupId)
        {
            //List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            List<MasterModel> masterModels = new List<MasterModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            try
            {
                //cmd = new SqlCommand("sp_getTable", connection);
                cmd = new SqlCommand("sp_GetProductGroupHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@ProductGroupId", ProductGroupId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel master = new MasterModel();
                        master.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        master.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        master.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        master.TechnicalName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        master.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        master.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
                        master.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        master.Grade = Convert.ToString(ds.Tables[0].Rows[i]["Grade"]);
                        master.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        master.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        master.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        master.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        master.ParentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ParentId"]);
                        master.MasterType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        master.IsRelated = Convert.ToBoolean(ds.Tables[0].Rows[i]["Related"]);
                        masterModels.Add(master);
                    }
                }
                return masterModels;
            }
            catch (Exception ex)
            {
                return masterModels;
            }
        }

        int rowIndex = 1;
        ExcelRange cell;
        ExcelFill fill;
        Border border;

        public byte[] GetExcel(List<MasterModel> masterModels)
        {
            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";
                var sheet = excelPackage.Workbook.Worksheets.Add("MasterModel");
                sheet.Name = "Master Model";
                sheet.Column(2).Width = 15;
                sheet.Column(4).Width = 30;
                sheet.Column(5).Width = 20;
                sheet.Column(7).Width = 50;
                sheet.Column(9).Width = 15;
                sheet.Column(11).Width = 15;
                sheet.Column(12).Width = 15;

                #region Report Header
                sheet.Cells[rowIndex, 1, rowIndex, 15].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "400 KV DOUBLE SUSPENSION INSULATOR STRING FOR QUAD MOOSE ACSR CONDUCTOR";
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 20;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 15].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "DRG. NO. KI/400KV-DSI(Q).240/02";
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 2;
                #endregion

                #region Table Header
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "Id";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 2];
                cell.Value = "CatalogueNo";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 3];
                cell.Value = "Code";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 4];
                cell.Value = "TechnicalName";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 5];
                cell.Value = "Name";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 6];
                cell.Value = "Quantity";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 7];
                cell.Value = "Material";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 8];
                cell.Value = "Grade";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 9];
                cell.Value = "DrawingNo";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 10];
                cell.Value = "Size";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 11];
                cell.Value = "UnitGrWt";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 12];
                cell.Value = "UnitNetWt";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 13];
                cell.Value = "ParentId";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 14];
                cell.Value = "Type";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 15];
                cell.Value = "Related";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                rowIndex = rowIndex + 1;
                #endregion

                #region Table body
                if (masterModels.Count > 0)
                {
                    foreach (MasterModel masterModel in masterModels)
                    {
                        cell = sheet.Cells[rowIndex, 1];
                        cell.Value = masterModel.Id;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 2];
                        cell.Value = masterModel.CatalogueNo;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 3];
                        cell.Value = masterModel.Code;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 4];
                        cell.Value = masterModel.TechnicalName;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 5];
                        cell.Value = masterModel.Name;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 6];
                        cell.Value = masterModel.Quantity;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 7];
                        cell.Value = masterModel.Material;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 8];
                        cell.Value = masterModel.Grade;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 9];
                        cell.Value = masterModel.DrawingNo;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 10];
                        cell.Value = masterModel.Size;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 11];
                        cell.Value = masterModel.UnitGrWt;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 12];
                        cell.Value = masterModel.UnitNetWt;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 13];
                        cell.Value = masterModel.ParentId;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 14];
                        cell.Value = masterModel.MasterType;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell = sheet.Cells[rowIndex, 15];
                        cell.Value = masterModel.IsRelated;
                        //cell.Style.Font.Bold = true;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.White);
                        border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                        rowIndex = rowIndex + 1;
                    }
                }

                return excelPackage.GetAsByteArray();
                #endregion
            }
        }

        public bool DeleteProject(int projectId, int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_DeleteProject", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projectId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public EnquiryModel SearchEnquiries(string prefix)
        {
            EnquiryModel enquiryModel = new EnquiryModel();
            List<EnquiryModel> enqList = new List<EnquiryModel>();
            List<CustomerEnquiryModel> custEnqList = new List<CustomerEnquiryModel>();
            List<TechnicalQueryModel> techQueryList = new List<TechnicalQueryModel>();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_SearchEnquiries", connection);
                cmd.Parameters.Add(new SqlParameter("@search", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        EnquiryModel enqModel = new EnquiryModel();
                        enqModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectId"]);
                        enqModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[i]["ProjectName"]);
                        enqModel.TenderFileNo = Convert.ToString(ds.Tables[0].Rows[i]["TenderFileNo"]);
                        enqModel.EnquiryType = Convert.ToString(ds.Tables[0].Rows[i]["EnqType"]);
                        enqModel.EndCustName = Convert.ToString(ds.Tables[0].Rows[i]["EndCustName"]);
                        enqModel.EnqDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDate"]);
                        enqModel.EnqDueDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDueDate"]);
                        enqModel.StatusDesc = Convert.ToString(ds.Tables[0].Rows[i]["StatusDesc"]);
                        enqModel.EnquiryAttachmentName = Convert.ToString(ds.Tables[0].Rows[i]["Attachment"]);
                        enqModel.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsPublished"]);
                        enqModel.EnquiryTypeId = enqModel.EnquiryType == "Domestic" ? 1 : 2;
                        enqList.Add(enqModel);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        CustomerEnquiryModel custEnquiry = new CustomerEnquiryModel();
                        custEnquiry.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        custEnquiry.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        custEnquiry.EpCCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        custEnquiry.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        custEnquiry.ExpiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        custEnquiry.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        custEnquiry.BoqFileName = Convert.ToString(ds.Tables[1].Rows[i]["BoqFile"]);
                        custEnquiry.ProjectSpecFileName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectSpecFile"]);
                        custEnquiry.OtherFileName = Convert.ToString(ds.Tables[1].Rows[i]["OtherFile"]);
                        custEnquiry.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        custEnquiry.CanCreateBOM = Convert.ToBoolean(ds.Tables[1].Rows[i]["CanCreateBOM"]);
                        custEnquiry.TechnicalQuery = new List<TechnicalQueryModel>();
                        custEnqList.Add(custEnquiry);
                    }
                }
                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                        technicalQuery.Answer = Convert.ToString(ds.Tables[2].Rows[i]["Answer"]);
                        techQueryList.Add(technicalQuery);
                    }
                }
                custEnqList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                enquiryModel.EnquiryList = enqList;
                enquiryModel.CustomerList = custEnqList;
                return enquiryModel;
            }
            catch (Exception ex)
            {
                return enquiryModel;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public bool PublishProject(int projectId, int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_PublishProject", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projectId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool PublishEnquiry(int enqId, int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_PublishEnquiry", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public AssignToModel GetAssignModal(int enquiryId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            List<UserAssignModel> assignee = new List<UserAssignModel>();
            AssignToModel atm = new AssignToModel();
            try
            {
                cmd = new SqlCommand("sp_GetAssignee", connection);
                cmd.Parameters.Add(new SqlParameter("@enquiryId", enquiryId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    atm.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[0]["ProjectId"]);
                    atm.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                    atm.EnquiryId = enquiryId;
                    atm.CustomerName = Convert.ToString(ds.Tables[0].Rows[0]["CustomerName"]);
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        UserAssignModel uam = new UserAssignModel();

                        uam.UserId = Convert.ToInt32(ds.Tables[1].Rows[i]["UserId"]);
                        uam.Name = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        uam.Designation = Convert.ToString(ds.Tables[1].Rows[i]["Type"]);
                        uam.IsSelected = Convert.ToBoolean(ds.Tables[1].Rows[i]["IsSelected"]);
                        assignee.Add(uam);
                    }
                }

                atm.Users = assignee;
                return atm;
            }
            catch (Exception ex)
            {
                return atm;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public bool SetAssignee(FormCollection form)
        {
            var enquiryId = Convert.ToInt32(string.IsNullOrEmpty(form["EnquiryId"]) ? "0" : form["EnquiryId"]);
            var selections = form["user.IsSelected"] ?? string.Empty;
            if (enquiryId > 0)
            {
                var dataModel = GetAssignModal(enquiryId);

                connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                try
                {
                    cmd = new SqlCommand("sp_SetEnquiryAssignee", connection);
                    cmd.Parameters.Add(new SqlParameter("@enquiryId", enquiryId));
                    cmd.Parameters.Add(new SqlParameter("@userIds", selections));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    var status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();

                    EmailsToAssignee(dataModel, selections);

                    if (status > 0)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
                return false;
        }

        public void EmailsToAssignee(AssignToModel model, string selections)
        {
            var oldSelections = model.Users.Where(x => x.IsSelected == true).Select(y => y.UserId).ToList();
            var selectionList = selections.Split(',').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList();
            var userIds = selectionList.Except(oldSelections);
            var data = string.Join(",", userIds);

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            List<string> emailIds = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetEmailIds", connection);
                cmd.Parameters.Add(new SqlParameter("@userIds", data));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        emailIds.Add(Convert.ToString(ds.Tables[0].Rows[i]["EmailId"]));
                    }
                }

                if(emailIds.Count > 0)
                {
                    string host = string.Empty;
                    string port = string.Empty;
                    if (HttpContext.Current != null)
                    {
                        host = HttpContext.Current.Request.Url.Host;
                        port = HttpContext.Current.Request.Url.Port.ToString();
                    }
                    StringBuilder emailBody = new StringBuilder();
                    emailBody.Append("<p>Hello,</p>");
                    emailBody.Append("<br/>");
                    emailBody.Append("<p>You have been assigned a new Enquiry. Following are it's details</p>");
                    emailBody.Append("<br/>");
                    emailBody.Append("<p>Project Name : " + model.ProjectName + " <p>");
                    emailBody.Append("<br/><br/>");
                    emailBody.Append("<p>Customer Name : " + model.CustomerName + " <p>");
                    emailBody.Append("<br/><br/>");
                    emailBody.Append("<p>Regards,<p>");
                    emailBody.Append("<br/>");
                    emailBody.Append("Karamtara");
                    string subject = "New Enquiry Assigned";

                    EmailService emailService = new EmailService();
                    emailService.SendEmailAsync(emailIds, subject, emailBody.ToString());
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public FilterData GetFilterList()
        {
            FilterData filter = new FilterData();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();

            filter.Columns = new List<FilterColumn>();
            filter.Orders = new List<Orders>();
            connection = new SqlConnection(connectionString);
            
            try
            {
                cmd = new SqlCommand("sp_GetFilterLists", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        FilterColumn col = new FilterColumn();
                        col.ColumnId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        col.ColumnName = Convert.ToString(ds.Tables[0].Rows[i]["ModelColumnName"]);
                        filter.Columns.Add(col);
                    }
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        Orders order = new Orders();
                        order.OrderId = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        order.Order = Convert.ToString(ds.Tables[1].Rows[i]["OrderName"]);
                        filter.Orders.Add(order);
                    }
                }

                return filter;
            }
            catch (Exception ex)
            {
                return filter;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public EnquiryModel FilterDataSelection(int columnId, int orderId, int userId)
        {
            var enqModel = new EnquiryModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_SetFilter", connection);
                cmd.Parameters.Add(new SqlParameter("@ColumnId", columnId));
                cmd.Parameters.Add(new SqlParameter("@OrderId", orderId));
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                enqModel = GetEnquiryDetails(userId);
                enqModel.Filter = GetFilterList();
                return enqModel;
            }
            catch (Exception ex)
            {
                return enqModel;
            }
        }
    }
}