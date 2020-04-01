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
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.DAL
{
    public class TenderEnquiryDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        EnquiryCommonDAL commonDAL;

        public EnquiryModel GetEnquiryDetails(int userId)
        {
            EnquiryModel enquiryModel = new EnquiryModel();
            List<EnquiryModel> enqList = new List<EnquiryModel>();
            List<CustomerEnquiryModel> custEnqList = new List<CustomerEnquiryModel>();
            List<TechnicalQueryModel> techQueryList = new List<TechnicalQueryModel>();
            List<TechnicalAnsModel> ansModels = new List<TechnicalAnsModel>();
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

                if (ds.Tables[3] != null)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        TechnicalAnsModel ansModel = new TechnicalAnsModel();
                        ansModel.Id = Convert.ToInt32(ds.Tables[3].Rows[i]["Id"]);
                        ansModel.QueryId = Convert.ToInt32(ds.Tables[3].Rows[i]["QueryId"]);
                        ansModel.Answer = Convert.ToString(ds.Tables[3].Rows[i]["Answer"]);
                        ansModel.ResponseFileName = Convert.ToString(ds.Tables[3].Rows[i]["AttachedFileName"]);
                        ansModel.ResponseAttachment = Convert.ToString(ds.Tables[3].Rows[i]["Attachment"]);
                        ansModels.Add(ansModel);
                    }
                }

                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.Answers = new List<TechnicalAnsModel>();
                        technicalQuery.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                        technicalQuery.QueryFileName = Convert.ToString(ds.Tables[2].Rows[i]["QueryFileName"]);
                        technicalQuery.QueryAttachment = Convert.ToString(ds.Tables[2].Rows[i]["QueryAttachment"]);
                        technicalQuery.Answers.AddRange(ansModels.Where(x => x.QueryId == technicalQuery.Id).ToList());
                        techQueryList.Add(technicalQuery);
                    }
                }
                custEnqList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                //custEnquiry.TechnicalQuery = techQueryList;
                //enqList.ForEach(x => x.StatusDesc = (custEnqList.Where(y => y.ProjectId == x.ProjectId).All(z => z.IsPublished) ?
                //"Published" : (custEnqList.Where(y => y.ProjectId == x.ProjectId).Any(z => z.IsPublished) ? "Partially Published" : "New")));

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
                        model.CountryName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        countryList.Add(model);
                    }
                }
                return countryList;
            }
            catch (Exception)
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
            List<TechnicalAnsModel> ansModels = new List<TechnicalAnsModel>();
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

                if (ds.Tables[3] != null)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        TechnicalAnsModel ansModel = new TechnicalAnsModel();
                        ansModel.Id = Convert.ToInt32(ds.Tables[3].Rows[i]["Id"]);
                        ansModel.QueryId = Convert.ToInt32(ds.Tables[3].Rows[i]["QueryId"]);
                        ansModel.Answer = Convert.ToString(ds.Tables[3].Rows[i]["Answer"]);
                        ansModel.ResponseFileName = Convert.ToString(ds.Tables[3].Rows[i]["AttachedFileName"]);
                        ansModel.ResponseAttachment = Convert.ToString(ds.Tables[3].Rows[i]["Attachment"]);
                        ansModels.Add(ansModel);
                    }
                }

                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.Answers = new List<TechnicalAnsModel>();
                        technicalQuery.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                        technicalQuery.QueryFileName = Convert.ToString(ds.Tables[2].Rows[i]["QueryFileName"]);
                        technicalQuery.QueryAttachment = Convert.ToString(ds.Tables[2].Rows[i]["QueryAttachment"]);
                        technicalQuery.Answers.AddRange(ansModels.Where(x => x.QueryId == technicalQuery.Id).ToList());
                        techQueryList.Add(technicalQuery);
                    }
                }
                //enquiryModel.CustomerList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                return enquiryModel;
            }
            catch (Exception)
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
            catch (Exception)
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

        public EnquiryModel CreateEnquiry(int userId, bool IsPublished, FormCollection form, EnquiryMediaFiles files)
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
                string projSummary = form["ProjectSummary"];
                //files.ProjectSpecificationFiles
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
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.Parameters.Add(new SqlParameter("@ProjStatus", IsPublished ? 4 : 1));
                cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished ? 1 : 0));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                projId = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                //save attachment

                DocumentDAL docDal = new DocumentDAL();
                var attachmentName = docDal.SaveEnquiryAttachment(projId, files.ProjectAttachment);

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_SaveEnquiryAttachment", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projId));
                cmd.Parameters.Add(new SqlParameter("@attachmentName", attachmentName));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                int status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (projId > 0)
                    AddEpcCustomers(form, projId, userId, IsPublished, files);
            }
            catch (Exception)
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

        public int AddEpcCustomers(FormCollection form, int projId, int userId, bool IsPublished, EnquiryMediaFiles files)
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
                List<int> enquiryIds = new List<int>();

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
                    cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                    cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished));

                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    int enqId = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();

                    if (enqId > 0)
                        enquiryIds.Add(enqId);

                    var boqFile = files.BOQFiles != null && files.BOQFiles.ElementAtOrDefault(i) != null ? files.BOQFiles[i] : null;
                    var proSpecFile = files.ProjectSpecificationFiles != null && files.ProjectSpecificationFiles.ElementAtOrDefault(i) != null ? files.ProjectSpecificationFiles[i] : null;
                    var otherFile = files.OtherAttachmentFiles != null && files.OtherAttachmentFiles.ElementAtOrDefault(i) != null ? files.OtherAttachmentFiles[i] : null;

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

                if (IsPublished)
                    GetAndSendPublishEmailData(projId, enquiryIds, userId);
            }
            catch (Exception)
            {

            }
            return custAdd;
        }

        public int EditEnquiry(int userId, FormCollection form, bool IsPublished, EnquiryMediaFiles files)
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
                //docDal.SaveEnquiryAttachment(projId, enqAttachment);
                var attachmentName = EditProjAttachment(projId, projAttachmentName, files.ProjectAttachment, DocumentType.EA, model.ProjectAttachmentName);

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_SaveEnquiryAttachment", connection);

                cmd.Parameters.Add(new SqlParameter("@projectId", projId));
                cmd.Parameters.Add(new SqlParameter("@attachmentName", attachmentName));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                int status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (projId > 0)
                    EditEpcCustomers(form, projId, IsPublished, userId, files, model);
            }
            catch (Exception)
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

        public int EditEpcCustomers(FormCollection form, int projId, bool IsPublished, int userId, EnquiryMediaFiles files, MediaFileNamesModel mediaNames)
        {
            string enqIds = form["x.EnquiryId"];
            string epcCustName = form["x.EPCCustomerName"];
            string custEnqDate = form["x.EnquiryDate"];
            string custExpDate = form["x.ExpiryDate"];
            string boqName = form["x.BoqFileName"];
            string psName = form["x.ProjectSpecFileName"];
            string otherName = form["x.OtherFileName"];
            string country = form["x.CountryId"];
            string boqIds = form["RemovedBoqIds"];
            string psIds = form["RemovedPsIds"];
            string oaIds = form["RemovedOaIds"];
            List<int> publishedEnqIds = new List<int>();
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
                List<int> removedBoqIds = boqIds.Split(',').Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)).Select(Int32.Parse).ToList();
                List<int> removedPsIds = psIds.Split(',').Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)).Select(Int32.Parse).ToList();
                List<int> removedOaIds = oaIds.Split(',').Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)).Select(Int32.Parse).ToList();

                var newBoqNames = EditMedia(enquiryIds, projId, boqFileNames, files.BOQFiles, DocumentType.Boq, mediaNames.BoqFileName, removedBoqIds);
                var newPsNames = EditMedia(enquiryIds, projId, proSpecFilesNames, files.ProjectSpecificationFiles, DocumentType.PS, mediaNames.ProjectSpecFileNames, removedPsIds);
                var newOtherNames = EditMedia(enquiryIds, projId, otherFileNames, files.OtherAttachmentFiles, DocumentType.Oth, mediaNames.OtherFileNames, removedOaIds);

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
                    cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                    cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    int status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                    if (status > 0)
                        publishedEnqIds.Add(Convert.ToInt32(enquiryIds[i]));
                }

                //Adding new customers
                if (epcNameList.Count > 0)
                {
                    var startIndex = 0;
                    var count = enquiryIds.Count();
                    epcNameList.RemoveRange(startIndex, count);
                    custEnqDateList.RemoveRange(startIndex, count);
                    custExpDateList.RemoveRange(startIndex, count);
                    files.BOQFiles.RemoveRange(startIndex, count);
                    files.ProjectSpecificationFiles.RemoveRange(startIndex, count);
                    files.OtherAttachmentFiles.RemoveRange(startIndex, count);
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
                        cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                        cmd.Parameters.Add(new SqlParameter("@IsPublished", IsPublished));
                        cmd.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        int enqId = Convert.ToInt32(cmd.ExecuteScalar());
                        connection.Close();
                        if (IsPublished)
                            publishedEnqIds.Add(enqId);

                        var boqFile = files.BOQFiles != null && files.BOQFiles.ElementAtOrDefault(i) != null ? files.BOQFiles[i] : null;
                        var proSpecFile = files.ProjectSpecificationFiles != null && files.ProjectSpecificationFiles.ElementAtOrDefault(i) != null ? files.ProjectSpecificationFiles[i] : null;
                        var otherFile = files.OtherAttachmentFiles != null && files.OtherAttachmentFiles.ElementAtOrDefault(i) != null ? files.OtherAttachmentFiles[i] : null;

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
                GetAndSendPublishEmailData(projId, publishedEnqIds, userId);
            }
            catch (Exception)
            {

            }
            return custAdd;
        }

        public List<string> EditMedia(List<string> enquiryIds, int projectId, List<string> actualNames, List<HttpPostedFileBase> files, DocumentType docType, List<string> dbFileNames, List<int> removedIds)
        {
            var tempFiles = files;
            List<string> names = new List<string>();
            try
            {
                for (int i = 0; i < enquiryIds.Count; i++)
                {
                    var enquiryId = Convert.ToInt32(enquiryIds[i]);
                    var projId = projectId;

                    var fileName = files.ElementAtOrDefault(i) != null && files[i] != null && files[i].ContentLength > 0 ? files[i].FileName : string.Empty;

                    if (string.IsNullOrEmpty(fileName)) //no incoming file
                    {
                        if (removedIds.Contains(enquiryId)) //removed
                        {
                            var oldFileName = dbFileNames[i];
                            RemoveFile(projectId, enquiryId, oldFileName);
                            names.Add("");
                        }
                        else if (actualNames.ElementAtOrDefault(i) != null && !string.IsNullOrEmpty(actualNames[i]) &&
                            !string.IsNullOrEmpty(dbFileNames[i]) && actualNames[i] == dbFileNames[i])
                        {
                            names.Add(dbFileNames[i]);
                        }
                        else
                        {
                            names.Add("");
                        }
                    }
                    else //new file added or changed
                    {
                        fileName = SaveOrUpdateDocument(projId, enquiryId, files[i], docType);
                        names.Add(fileName);
                    }

                    //if (string.IsNullOrEmpty(newFileName))


                    //    var currentName = dbFileNames[i];
                    //string newFileName = SaveOrUpdateDocument(projId, enquiryId, files[i], docType);
                    //if (string.IsNullOrEmpty(newFileName))
                    //{
                    //    var fileName = fileNames[i];
                    //    if (currentName != fileName)
                    //    {
                    //        newFileName = fileName;
                    //        RemoveFile(projectId, enquiryId, currentName);
                    //    }
                    //    else
                    //        newFileName = fileName;
                    //}
                    //else
                    //{
                    //    if (!string.IsNullOrEmpty(currentName))
                    //        RemoveFile(projectId, enquiryId, currentName);
                    //}


                }
                return names;
            }
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
            {
                return fileName;
            }
        }

        public int CreateBOMId(int enqId, int userId)
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
            catch (Exception)
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
            catch (Exception)
            {
                return false;
            }
        }

        public EnquiryModel SearchEnquiries(string prefix, int userId)
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
            catch (Exception)
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
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            List<int> enqIds = new List<int>();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_PublishProject", connection);
                cmd.Parameters.Add(new SqlParameter("@projectId", projectId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    status = Convert.ToInt32(ds.Tables[0].Rows[0]["Status"]);
                }

                if (status > 0)
                {
                    if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        {
                            enqIds.Add(Convert.ToInt32(ds.Tables[1].Rows[i]["Ids"]));
                        }
                    }
                    GetAndSendPublishEmailData(projectId, enqIds, userId);
                }
                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                if (status > 0)
                    return true;
                else
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
                var list = new List<int>();
                list.Add(enqId);
                GetAndSendPublishEmailData(0, list, userId);
                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public EnquiryModel FilterDataSelection(int columnId, int orderId, int userId)
        {
            var enqModel = new EnquiryModel();
            commonDAL = new EnquiryCommonDAL();
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
                enqModel.Filter = commonDAL.GetFilterList(userId);

                return enqModel;
            }
            catch (Exception)
            {
                return enqModel;
            }
        }

        public void GetAndSendPublishEmailData(int projectId, List<int> enquiryIds, int userId)
        {
            if (enquiryIds.Count == 0)
                return;

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            EnquiryMail model = new EnquiryMail();
            List<string> customerNames = new List<string>();
            List<string> ccUsers = new List<string>();
            List<UserEmail> userEmails = new List<UserEmail>();
            try
            {
                cmd = new SqlCommand("sp_GetEnquiryDetailsForEmail", connection);
                cmd.Parameters.Add(new SqlParameter("@projId", projectId));
                cmd.Parameters.Add(new SqlParameter("@enqIds", string.Join(",", enquiryIds)));
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    model.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                    model.EndCustName = Convert.ToString(ds.Tables[0].Rows[0]["EndCustName"]);
                    model.PublisherName = Convert.ToString(ds.Tables[0].Rows[0]["Publisher"]);
                    model.PublisherDesignation = Convert.ToString(ds.Tables[0].Rows[0]["Designation"]);
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        customerNames.Add(Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]));
                    }
                }

                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        UserEmail uEmail = new UserEmail();
                        uEmail.Name = Convert.ToString(ds.Tables[2].Rows[i]["Name"]);
                        uEmail.EmailId = Convert.ToString(ds.Tables[2].Rows[i]["EmailId"]);
                        userEmails.Add(uEmail);
                    }
                }

                if (ds.Tables[3] != null)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        ccUsers.Add(Convert.ToString(ds.Tables[3].Rows[i]["EmailId"]));
                    }
                }
                model.CustomerNames = customerNames;
                model.UserEmails = userEmails;
                SendPublishEmails(model, ccUsers);
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

        public void SendPublishEmails(EnquiryMail dataModel,List<string> ccUsers)
        {
            string host = string.Empty;
            string port = string.Empty;
            if (HttpContext.Current != null)
            {
                host = HttpContext.Current.Request.Url.Host;
                port = HttpContext.Current.Request.Url.Port.ToString();
            }
            string emailBody = PopulateBody(dataModel);
            string subject = "Enquiry Published";

            EmailService emailService = new EmailService();
            emailService.SendEmailAsync(dataModel.UserEmails.Select(x => x.EmailId).ToList(), subject, emailBody.ToString(), ccUsers);
        }

        private string PopulateBody(EnquiryMail dataModel)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/enquiryPublished.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{nameOfUser}", dataModel.PublisherName);
            body = body.Replace("{designation}", dataModel.PublisherDesignation);
            body = body.Replace("{projectName}", dataModel.ProjectName);
            body = body.Replace("{endCustomerName}", dataModel.EndCustName);
            string forLoop = string.Empty;
            for (int i = 0; i < dataModel.CustomerNames.Count; i++)
            {
                forLoop = forLoop + (" <li> " + dataModel.CustomerNames[i] + " </li> ");
            }

            body = body.Replace("{ReplaceThisByForLoop}", forLoop);
            return body;
        }
    }
}


