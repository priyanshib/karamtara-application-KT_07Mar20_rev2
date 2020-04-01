using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Karamtara_Application.DAL
{
    public class EnquiryCommonDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        int rowIndex = 1;
        ExcelRange cell;
        ExcelFill fill;
        Border border;

        public FilterData GetFilterList(int userId)
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
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
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
                        col.IsSelected = Convert.ToBoolean(ds.Tables[0].Rows[i]["Selected"]);
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
                        order.IsSelected = Convert.ToBoolean(ds.Tables[1].Rows[i]["Selected"]);
                        filter.Orders.Add(order);
                    }
                }

                return filter;
            }
            catch (Exception)
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
        //common
        public bool SubmitTechnicalQuery(int id, string query, List<HttpPostedFileBase> QueryFile, int UserId, TechnicalQueryModel technicalQuery)
        {
            var Filenames = string.Empty;
            var QueryActFile = string.Empty;
            DocumentDAL docDal = new DocumentDAL();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            if (QueryFile != null && QueryFile.Count > 0)
            {
                var queryFile = QueryFile.FirstOrDefault();
                Filenames = docDal.SaveQueryFileAttachements(queryFile, id);
                QueryActFile = Convert.ToString(QueryFile.FirstOrDefault().FileName);
            }
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd = new SqlCommand("sp_AddTechnicalQuery", connection);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.Parameters.Add(new SqlParameter("@query", query));
            cmd.Parameters.Add(new SqlParameter("@UserId", UserId));
            cmd.Parameters.Add(new SqlParameter("@QueryFile", Filenames));
            cmd.Parameters.Add(new SqlParameter("@QueryActFileName", QueryActFile));
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            int status = Convert.ToInt32(cmd.ExecuteScalar());
            connection.Close();

            if (status > 0)
            {
                technicalQuery.Query = query;
                connection = new SqlConnection(connectionString);
                //SqlCommand cmd = new SqlCommand();
                cmd = new SqlCommand("sp_TechnicalQueryAskBy", connection);
                cmd.Parameters.Add(new SqlParameter("@UserId", UserId));
                cmd.Parameters.Add(new SqlParameter("@enqId", id));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    technicalQuery.AskedBy = Convert.ToString(ds.Tables[0].Rows[0]["FirstName"]);
                    technicalQuery.LastName = Convert.ToString(ds.Tables[0].Rows[0]["LastName"]);
                    technicalQuery.Designation = Convert.ToString(ds.Tables[0].Rows[0]["Designation"]);
                }

                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    technicalQuery.TenderFileNo = Convert.ToString(ds.Tables[1].Rows[0]["TenderFileNo"]);
                    technicalQuery.ProjectName = Convert.ToString(ds.Tables[1].Rows[0]["ProjectName"]);
                    technicalQuery.CustomerName = Convert.ToString(ds.Tables[1].Rows[0]["EpcCustomer"]);
                }
                SendQueryAttachmentMail(technicalQuery, id, Filenames);
                return true;
            }
            else
                return false;
        }

        //common
        public List<TechnicalQueryModel> GetTechnicalQueryList(int enqId)
        {
            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            List<TechnicalAnsModel> ansModels = new List<TechnicalAnsModel>();
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
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        TechnicalAnsModel ansModel = new TechnicalAnsModel();
                        ansModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        ansModel.QueryId = Convert.ToInt32(ds.Tables[1].Rows[i]["QueryId"]);
                        ansModel.Answer = Convert.ToString(ds.Tables[1].Rows[i]["Answer"]);
                        ansModel.ReplyBy = Convert.ToString(ds.Tables[1].Rows[i]["ReplyBy"]);
                        ansModel.ReplyDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ReplyDate"]);
                        ansModel.ResponseAttachment = Convert.ToString(ds.Tables[1].Rows[i]["Attachment"]);
                        ansModel.ResponseFileName = Convert.ToString(ds.Tables[1].Rows[i]["AttachedFileName"]);
                        ansModels.Add(ansModel);
                    }
                }

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
                        technicalQuery.Answers = new List<TechnicalAnsModel>();
                        technicalQuery.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        technicalQuery.EnquiryId = Convert.ToInt32(ds.Tables[0].Rows[i]["EnquiryId"]);
                        technicalQuery.Query = Convert.ToString(ds.Tables[0].Rows[i]["Query"]);
                        technicalQuery.QueryAttachment = Convert.ToString(ds.Tables[0].Rows[i]["QueryAttachment"]);
                        technicalQuery.AskedBy = Convert.ToString(ds.Tables[0].Rows[i]["AskedBy"]);
                        technicalQuery.QueryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AskedDate"]);
                        technicalQuery.Answers.AddRange(ansModels.Where(x => x.QueryId == technicalQuery.Id).ToList());
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

        //common
        public int SubTechnicalQueryAnswer(int QueryId, string answer, int enqId, List<HttpPostedFileBase> queryRespFiles, int UserId, TechnicalQueryModel technicalQuery, out bool allAnswered)
        {
            allAnswered = false;
            var ResponseActFile = string.Empty;

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            var fileName = string.Empty;
            DocumentDAL docDal = new DocumentDAL();
            if (queryRespFiles != null && queryRespFiles.Count > 0)
            {
                var queryFile = queryRespFiles.FirstOrDefault();
                fileName = docDal.SaveQueryRespFileAttachements(queryFile, enqId);
                ResponseActFile = Convert.ToString(queryRespFiles.FirstOrDefault().FileName);
            }

            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                int AnsId = 0;
                if (answer != null)
                {
                    cmd = new SqlCommand("sp_subTechQueryAns", connection);
                    cmd.Parameters.Add(new SqlParameter("@Id", QueryId));
                    cmd.Parameters.Add(new SqlParameter("@Answer", answer));
                    cmd.Parameters.Add(new SqlParameter("@UserId", UserId));
                    cmd.Parameters.Add(new SqlParameter("@QueryRespFile", fileName));
                    cmd.Parameters.Add(new SqlParameter("@RespActFileName", ResponseActFile));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    AnsId = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                    if (AnsId != 0)
                    {
                        technicalQuery.Answer = answer;
                        connection = new SqlConnection(connectionString);
                        //SqlCommand cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_GetQuererAndAnswererDetails", connection);
                        cmd.Parameters.Add(new SqlParameter("@queryId", QueryId));
                        cmd.Parameters.Add(new SqlParameter("@ansId", AnsId));
                        cmd.CommandType = CommandType.StoredProcedure;
                        adapter.SelectCommand = cmd;
                        connection.Open();
                        adapter.Fill(ds);
                        connection.Close();
                        if (ds.Tables[0] != null && ds.Tables[0].Rows[0] != null)
                        {
                            technicalQuery.AskedBy = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
                            technicalQuery.Designation = Convert.ToString(ds.Tables[0].Rows[0]["Designation"]);
                        }

                        if (ds.Tables[0] != null && ds.Tables[0].Rows[1] != null)
                        {
                            technicalQuery.ReplyBy = Convert.ToString(ds.Tables[0].Rows[1]["Name"]);
                            technicalQuery.ReplierDesignation = Convert.ToString(ds.Tables[0].Rows[1]["Designation"]);
                        }

                        if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                        {
                            technicalQuery.TenderFileNo = Convert.ToString(ds.Tables[1].Rows[0]["TenderFileNo"]);
                            technicalQuery.ProjectName = Convert.ToString(ds.Tables[1].Rows[0]["ProjectName"]);
                            technicalQuery.CustomerName = Convert.ToString(ds.Tables[1].Rows[0]["EpcCustomer"]);
                        }
                        SendQueryResponseAttachmentMail(technicalQuery, enqId, fileName);
                    }
                }

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_CheckAllQueriesAnswered", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                connection.Open();
                allAnswered = Convert.ToInt32(cmd.ExecuteScalar()) > 0 ? true : false;
                connection.Close();

                return AnsId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //common
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
            catch (Exception)
            {
                return masterModels;
            }
        }

        //common
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

        //common
        public string getQueryFileName(string File)
        {
            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            string QueryFileName = string.Empty;
            try
            {
                cmd = new SqlCommand("sp_getQueryFileName", connection);
                cmd.Parameters.Add(new SqlParameter("@FileName", File));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                QueryFileName = Convert.ToString(cmd.ExecuteScalar());
                connection.Close();
                return QueryFileName;
            }
            catch (Exception)
            {
                return QueryFileName;
            }
        }

        //common
        public string getResponseFileName(string File)
        {
            List<TechnicalQueryModel> queryModels = new List<TechnicalQueryModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            string QueryFileName = string.Empty;
            try
            {
                cmd = new SqlCommand("sp_getResponseFileName", connection);
                cmd.Parameters.Add(new SqlParameter("@FileName", File));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                QueryFileName = Convert.ToString(cmd.ExecuteScalar());
                connection.Close();
                return QueryFileName;
            }
            catch (Exception)
            {
                return QueryFileName;
            }
        }

        //common
        public TechnicalQueryModel SendMailList(int userId)
        {
            TechnicalQueryModel technicalQuery = new TechnicalQueryModel();
            technicalQuery.ToMailList = new List<String>();
            technicalQuery.CcMailList = new List<String>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_getUserMailId", connection);
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                string UserMailId = Convert.ToString(cmd.ExecuteScalar());
                connection.Close();
                technicalQuery.UserMailId = UserMailId;

                cmd = new SqlCommand("sp_getToAndCcMailList", connection);
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        technicalQuery.ToMailList.Add(Convert.ToString(ds.Tables[0].Rows[i]["ToMailIds"]));
                    }
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        technicalQuery.CcMailList.Add(Convert.ToString(ds.Tables[1].Rows[i]["CcMialIds"]));
                    }
                }
            }
            catch (Exception)
            {

            }

            return technicalQuery;
        }

        //common
        public void SendQueryAttachmentMail(TechnicalQueryModel technicalQuery, int enquiryId, string file)
        {
            string host = string.Empty;
            string port = string.Empty;
            if (HttpContext.Current != null)
            {
                host = HttpContext.Current.Request.Url.Host;
                port = HttpContext.Current.Request.Url.Port.ToString();
            }

            string emailBody = PopulateBodyForQuery(technicalQuery);
            string subject = "Karamtara Application: Technical Query";

            var extension = Path.GetExtension(file);
            Attachment attachment;
            if (!string.IsNullOrEmpty(file))
            {
                string path = HostingEnvironment.MapPath(string.Format("~/Documents/TechnicalQuery/{0}/{1}", enquiryId, file));
                attachment = new System.Net.Mail.Attachment(path);
                attachment.Name = "Attachment" + extension;
                emailBody = emailBody.Replace("{pfaComment}", "Also, PFA the file related to the query");
            }
            else
            {
                attachment = null;
                emailBody = emailBody.Replace("{pfaComment}", "");
            }
            EmailService emailService = new EmailService();
            emailService.SendEmailWithAttachment(emailBody, subject, attachment, technicalQuery.ToMailList, technicalQuery.CcMailList);
        }

        private string PopulateBodyForQuery(TechnicalQueryModel technicalQuery)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/query.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{querer}", string.Format("{0} {1} ({2})", technicalQuery.AskedBy, technicalQuery.LastName, technicalQuery.Designation));
            body = body.Replace("{queryValue}", technicalQuery.Query);
            body = body.Replace("{tenderFileNo}", technicalQuery.TenderFileNo);
            body = body.Replace("{projName}", technicalQuery.ProjectName);
            body = body.Replace("{epcCustName}", technicalQuery.CustomerName);
            return body;
        }

        //common
        public void SendQueryResponseAttachmentMail(TechnicalQueryModel technicalQuery, int enquiryId, string file)
        {
            string host = string.Empty;
            string port = string.Empty;
            if (HttpContext.Current != null)
            {
                host = HttpContext.Current.Request.Url.Host;
                port = HttpContext.Current.Request.Url.Port.ToString();
            }

            string emailBody = PopulateBodyForResponse(technicalQuery);
            string subject = "Karamtara Application: Technical Query Answer";

            var extension = Path.GetExtension(file);
            System.Net.Mail.Attachment attachment;
            if (!string.IsNullOrEmpty(file))
            {
                string path = HostingEnvironment.MapPath(string.Format("~/Documents/TechnicalQueryResponse/{0}/{1}", enquiryId, file));
                attachment = new System.Net.Mail.Attachment(path);
                attachment.Name = "Attachment" + extension;
                emailBody = emailBody.Replace("{pfaComment}", "Also, PFA the file related to the response");
            }
            else
            {
                attachment = null;
                emailBody = emailBody.Replace("{pfaComment}", "");
            }
            EmailService emailService = new EmailService();
            emailService.SendEmailWithAttachment(emailBody, subject, attachment, technicalQuery.ToMailList, technicalQuery.CcMailList);
        }

        private string PopulateBodyForResponse(TechnicalQueryModel technicalQuery)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/queryResponse.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{querer}", string.Format("{0} ({1})", technicalQuery.AskedBy, technicalQuery.Designation));
            body = body.Replace("{answerer}", string.Format("{0} ({1})", technicalQuery.ReplyBy, technicalQuery.ReplierDesignation));
            body = body.Replace("{queryValue}", technicalQuery.Query);
            body = body.Replace("{answerValue}", technicalQuery.Answer);
            body = body.Replace("{tenderFileNo}", technicalQuery.TenderFileNo);
            body = body.Replace("{projName}", technicalQuery.ProjectName);
            body = body.Replace("{epcCustName}", technicalQuery.CustomerName);
            return body;
        }
    }
}