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
    public class EnquiryDAL
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
                        //custEnquiry.CanCreteTender = Convert.ToBoolean(ds.Tables[1].Rows[i]["CanCreateTender"]);
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
                        technicalQuery.Answers.AddRange(ansModels.Where(x => x.QueryId == technicalQuery.Id).ToList());
                        techQueryList.Add(technicalQuery);
                    }
                }
                custEnqList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                //custEnqList = custEnqList.Where(x => x.IsPublished).ToList();

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

        public EnquiryModel SearchEnquiries(string prefix, int userId)
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
                //if (ds.Tables[2] != null)
                //{
                //    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                //    {
                //        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                //        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                //        technicalQuery.Query = Convert.ToString(ds.Tables[2].Rows[i]["Query"]);
                //        technicalQuery.Answer = Convert.ToString(ds.Tables[2].Rows[i]["Answer"]);
                //        techQueryList.Add(technicalQuery);
                //    }
                //}
                custEnqList.ForEach(x => x.TechnicalQuery.AddRange(techQueryList.Where(y => y.EnquiryId == x.EnquiryId)));
                //custEnqList = custEnqList.Where(x => x.IsPublished).ToList();

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
                    atm.EnquiryId = enquiryId;
                    atm.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[0]["ProjectId"]);
                    atm.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                    atm.TenderFileNo = Convert.ToString(ds.Tables[0].Rows[0]["TenderFileNo"]);
                    atm.EndCustomerName = Convert.ToString(ds.Tables[0].Rows[0]["EndCustName"]);
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
            catch (Exception)
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
                catch (Exception)
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

                if (emailIds.Count > 0)
                {
                    
                    string emailBody = PopulateBody(model);
                    string subject = "New Enquiry Assigned";

                    EmailService emailService = new EmailService();
                    emailService.SendEmailAsync(emailIds, subject, emailBody);
                }
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
        }


        private string PopulateBody(AssignToModel model)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/enquiryAsigned.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{tenderFileNo}", model.TenderFileNo);
            body = body.Replace("{projectName}", model.ProjectName);
            body = body.Replace("{endCustName}", model.EndCustomerName);
            body = body.Replace("{epcCustName}", model.CustomerName);
            return body;
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
    }
        
}


