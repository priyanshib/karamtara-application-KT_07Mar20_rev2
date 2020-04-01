using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Karamtara_Application.DAL.Tender
{
    public class TenderListDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public TenderListModel GetTenderList(int tenderType)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderListModel tenderModel = new TenderListModel();
            tenderModel.ProjectList = new List<ProjectEnquiryModel>();
            tenderModel.CustomerList = new List<TenderEnquiryModel>();
            try
            {
                cmd = new SqlCommand("sp_GetTenderList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tenderType", tenderType);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ProjectEnquiryModel peModel = new ProjectEnquiryModel();
                        peModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectId"]);
                        peModel.ProjectNo = Convert.ToString(ds.Tables[0].Rows[i]["TenderFileNo"]);
                        peModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[i]["Projectname"]);
                        peModel.ProjectCreateDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreateDate"]);
                        peModel.EnquiryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDate"]);
                        peModel.ProjectDueDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDueDate"]);
                        tenderModel.ProjectList.Add(peModel);
                    }
                }

                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        {
                            TenderEnquiryModel teModel = new TenderEnquiryModel();
                            teModel.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                            teModel.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                            teModel.TenderId = Convert.ToInt32(ds.Tables[1].Rows[i]["TenderId"]);
                            teModel.TenderRevisionId = Convert.ToInt32(ds.Tables[1].Rows[i]["TenderRevisionNo"]);
                            teModel.CustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                            teModel.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                            teModel.DueDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                            teModel.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                            teModel.Status = Convert.ToString(ds.Tables[1].Rows[i]["Status"]);
                            teModel.BomId = Convert.ToInt32(ds.Tables[1].Rows[i]["BomId"]);
                            teModel.BomRevisionId = Convert.ToInt32(ds.Tables[1].Rows[i]["RevisionNo"]);
                            teModel.MaxTndRevNo = Convert.ToInt32(ds.Tables[1].Rows[i]["MaxTndRevNo"]);
                            teModel.TenderType = Convert.ToInt32(ds.Tables[1].Rows[i]["TenderType"]);
                            if(teModel.TenderType == 1)
                            {
                                teModel.TenderTypeName = "Karamtara";
                            }
                            else if(teModel.TenderType == 2)
                            {
                                teModel.TenderTypeName = "ISELFA";
                            }
                            tenderModel.CustomerList.Add(teModel);
                        }
                    }
                }

                return tenderModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return tenderModel;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public TenderListModel GetTenderListWithSearch(string searchText)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderListModel tenderModel = new TenderListModel();
            tenderModel.ProjectList = new List<ProjectEnquiryModel>();
            tenderModel.CustomerList = new List<TenderEnquiryModel>();
            try
            {
                cmd = new SqlCommand("sp_GetTenderListWithSearch", connection);
                cmd.Parameters.Add(new SqlParameter("@searchText", searchText));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ProjectEnquiryModel peModel = new ProjectEnquiryModel();
                        peModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectId"]);
                        peModel.ProjectNo = Convert.ToString(ds.Tables[0].Rows[i]["TenderFileNo"]);
                        peModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[i]["Projectname"]);
                        peModel.ProjectCreateDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["CreateDate"]);
                        peModel.EnquiryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDate"]);
                        peModel.ProjectDueDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["EnqDueDate"]);
                        tenderModel.ProjectList.Add(peModel);
                    }
                }

                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        {
                            TenderEnquiryModel teModel = new TenderEnquiryModel();
                            teModel.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                            teModel.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                            teModel.TenderId = Convert.ToInt32(ds.Tables[1].Rows[i]["TenderId"]);
                            teModel.TenderRevisionId = Convert.ToInt32(ds.Tables[1].Rows[i]["TenderRevisionNo"]);
                            teModel.CustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                            teModel.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                            teModel.DueDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                            teModel.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                            teModel.Status = Convert.ToString(ds.Tables[1].Rows[i]["Status"]);
                            teModel.BomId = Convert.ToInt32(ds.Tables[1].Rows[i]["BomId"]);
                            teModel.BomRevisionId = Convert.ToInt32(ds.Tables[1].Rows[i]["RevisionNo"]);
                            teModel.MaxTndRevNo = Convert.ToInt32(ds.Tables[1].Rows[i]["MaxTndRevNo"]);
                            tenderModel.CustomerList.Add(teModel);
                        }
                    }
                }

                return tenderModel;
            }
            catch (Exception)
            {
                return tenderModel;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<TndCompareModel> GetTndCompareList(int enqType)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            List<TndCompareModel> compareList = new List<TndCompareModel>();
            try
            {
                cmd = new SqlCommand("sp_GetCompareList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@enqType", enqType);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TndCompareModel compareModel = new TndCompareModel();
                        compareModel.ProjectName = Convert.ToString(dt.Rows[i]["ProjectName"]);
                        compareModel.CustomerName = Convert.ToString(dt.Rows[i]["CustomerName"]);
                        compareModel.BomId = Convert.ToInt32(dt.Rows[i]["BomId"]);
                        compareModel.BomRevisionNo = Convert.ToInt32(dt.Rows[i]["RevisionNo"]);
                        compareModel.TenderId = Convert.ToInt32(dt.Rows[i]["TenderId"]);
                        compareModel.TenderRevNo = Convert.ToInt32(dt.Rows[i]["TenderRevisionNo"]);
                        compareList.Add(compareModel);
                    }
                }
                return compareList;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }
    }
}