using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Karamtara_Application.DAL
{
    public class BOMListDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public BOMListModel GetBOMList()
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetBOMList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                BOMListModel masterModel = new BOMListModel();
                List<BOMListModel> bomList = new List<BOMListModel>();
                List<ProjectEnquiryModel> dataList = new List<ProjectEnquiryModel>();
                List<ProjectEnquiryModel> innerDataList = new List<ProjectEnquiryModel>();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BOMListModel model = new BOMListModel();
                        model.EnquiryId= Convert.ToInt32(ds.Tables[0].Rows[i]["EnquiryId"]);
                        model.BomId = Convert.ToInt32(ds.Tables[0].Rows[i]["BomId"]);
                        model.RevisionNo = Convert.ToInt32(ds.Tables[0].Rows[i]["RevisionNo"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsPublished"]);
                        model.UserId = Convert.ToInt32(ds.Tables[0].Rows[i]["UserId"]);
                        bomList.Add(model);
                    }
                }
                if(ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ProjectEnquiryModel model = new ProjectEnquiryModel();
                        model.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        //model.RevNo = Convert.ToInt32(ds.Tables[1].Rows[i]["RevisionNo"]);
                        //model.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        model.ProjectName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectName"]);
                        model.ProjectNo = Convert.ToString(ds.Tables[1].Rows[i]["TenderFileNo"]);
                        model.EndCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["EndCustName"]);
                        model.ProjectDueDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ProjectDueDate"]);
                        model.ProjectCreateDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["CreateDate"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[1].Rows[i]["IsPublished"]);
                        model.EnquiryType = Convert.ToString(ds.Tables[1].Rows[i]["Type"]);
                        model.ProjectStatus = Convert.ToString(ds.Tables[1].Rows[i]["ProjectStatus"]);
                        //model.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        //model.EpcCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        //model.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        //model.EnquiryDuteDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        //model.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        //model.BomId = Convert.ToInt32(ds.Tables[1].Rows[i]["BomId"]);
                        dataList.Add(model);
                    }
                }
                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        ProjectEnquiryModel model = new ProjectEnquiryModel();
                        model.ProjectId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProjectId"]);
                        model.RevNo = Convert.ToInt32(ds.Tables[2].Rows[i]["RevisionNo"]);
                        model.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        //model.ProjectName = Convert.ToString(ds.Tables[2].Rows[i]["ProjectName"]);
                        //model.ProjectNo = Convert.ToString(ds.Tables[2].Rows[i]["TenderFileNo"]);
                        //model.EndCustomerName = Convert.ToString(ds.Tables[2].Rows[i]["EndCustName"]);
                        //model.ProjectDueDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["ProjectDueDate"]);
                        //model.ProjectCreateDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["CreateDate"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[2].Rows[i]["IsPublished"]);
                        //model.EnquiryType = Convert.ToString(ds.Tables[2].Rows[i]["Type"]);
                        //model.ProjectStatus = Convert.ToString(ds.Tables[2].Rows[i]["ProjectStatus"]);
                        model.EnquiryStatus = Convert.ToString(ds.Tables[2].Rows[i]["EnquiryStatus"]);
                        model.EpcCustomerName = Convert.ToString(ds.Tables[2].Rows[i]["CustomerName"]);
                        model.EnquiryDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["EnquiryDate"]);
                        model.EnquiryDuteDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["ExpiryDate"]);
                        model.Country = Convert.ToString(ds.Tables[2].Rows[i]["Country"]);
                        model.BomId = Convert.ToInt32(ds.Tables[2].Rows[i]["BomId"]);
                        model.IsLatestRevision = Convert.ToBoolean(ds.Tables[2].Rows[i]["IsLatestRevision"]);
                        model.TNumber = Convert.ToString(ds.Tables[2].Rows[i]["TNumber"]);
                        model.BOMSource=Convert.ToString(ds.Tables[2].Rows[i]["source"]);

                        innerDataList.Add(model);
                    }
                }

                masterModel.BomList = bomList;
                masterModel.DataList = dataList;
                masterModel.InnerDataList = innerDataList;
                return masterModel;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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

        public BOMListModel GetBOMList_Clone()
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetBOMList_Clone", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                BOMListModel masterModel = new BOMListModel();
                List<BOMListModel> bomList = new List<BOMListModel>();
                List<ProjectEnquiryModel> dataList = new List<ProjectEnquiryModel>();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BOMListModel model = new BOMListModel();
                        model.EnquiryId = Convert.ToInt32(ds.Tables[0].Rows[i]["EnquiryId"]);
                        model.BomId = Convert.ToInt32(ds.Tables[0].Rows[i]["BomId"]);
                        model.RevisionNo = Convert.ToInt32(ds.Tables[0].Rows[i]["RevisionNo"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsPublished"]);
                        model.UserId = Convert.ToInt32(ds.Tables[0].Rows[i]["UserId"]);
                        bomList.Add(model);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ProjectEnquiryModel model = new ProjectEnquiryModel();
                        model.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        model.RevNo = Convert.ToInt32(ds.Tables[1].Rows[i]["RevisionNo"]);
                        model.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        model.ProjectName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectName"]);
                        model.ProjectNo = Convert.ToString(ds.Tables[1].Rows[i]["TenderFileNo"]);
                        model.EndCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["EndCustName"]);
                        model.ProjectDueDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ProjectDueDate"]);
                        model.ProjectCreateDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["CreateDate"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[1].Rows[i]["IsPublished"]);
                        model.EnquiryType = Convert.ToString(ds.Tables[1].Rows[i]["Type"]);
                        model.ProjectStatus = Convert.ToString(ds.Tables[1].Rows[i]["ProjectStatus"]);
                        model.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        model.EpcCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        model.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        model.EnquiryDuteDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        model.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        model.BomId = Convert.ToInt32(ds.Tables[1].Rows[i]["BomId"]);
                        dataList.Add(model);
                    }
                }
                masterModel.BomList = bomList;
                masterModel.DataList = dataList;
                return masterModel;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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

        public BOMListModel SearchBOMList(string prefix)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetBomListWithSearch", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                BOMListModel masterModel = new BOMListModel();
                List<BOMListModel> bomList = new List<BOMListModel>();
                List<ProjectEnquiryModel> dataList = new List<ProjectEnquiryModel>();
                List<ProjectEnquiryModel> innerDataList = new List<ProjectEnquiryModel>();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BOMListModel model = new BOMListModel();
                        model.EnquiryId = Convert.ToInt32(ds.Tables[0].Rows[i]["EnquiryId"]);
                        model.BomId = Convert.ToInt32(ds.Tables[0].Rows[i]["BomId"]);
                        model.RevisionNo = Convert.ToInt32(ds.Tables[0].Rows[i]["RevisionNo"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsPublished"]);
                        model.UserId = Convert.ToInt32(ds.Tables[0].Rows[i]["UserId"]);
                        bomList.Add(model);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ProjectEnquiryModel model = new ProjectEnquiryModel();
                        model.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProjectId"]);
                        model.ProjectName = Convert.ToString(ds.Tables[1].Rows[i]["ProjectName"]);
                        model.ProjectNo = Convert.ToString(ds.Tables[1].Rows[i]["TenderFileNo"]);
                        model.EndCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["EndCustName"]);
                        model.ProjectDueDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ProjectDueDate"]);
                        model.ProjectCreateDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["CreateDate"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[1].Rows[i]["IsPublished"]);
                        model.EnquiryType = Convert.ToString(ds.Tables[1].Rows[i]["Type"]);
                        model.ProjectStatus = Convert.ToString(ds.Tables[1].Rows[i]["ProjectStatus"]);
                        //model.EnquiryStatus = Convert.ToString(ds.Tables[1].Rows[i]["EnquiryStatus"]);
                        //model.EpcCustomerName = Convert.ToString(ds.Tables[1].Rows[i]["CustomerName"]);
                        //model.EnquiryDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["EnquiryDate"]);
                        //model.EnquiryDuteDate = Convert.ToDateTime(ds.Tables[1].Rows[i]["ExpiryDate"]);
                        //model.Country = Convert.ToString(ds.Tables[1].Rows[i]["Country"]);
                        //model.BomId = Convert.ToInt32(ds.Tables[1].Rows[i]["BomId"]);
                        //model.RevNo = Convert.ToInt32(ds.Tables[1].Rows[i]["RevisionNo"]);
                        //model.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[i]["EnquiryId"]);
                        dataList.Add(model);
                    }
                }
                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        ProjectEnquiryModel model = new ProjectEnquiryModel();
                        model.ProjectId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProjectId"]);
                        model.RevNo = Convert.ToInt32(ds.Tables[2].Rows[i]["RevisionNo"]);
                        model.EnquiryId = Convert.ToInt32(ds.Tables[2].Rows[i]["EnquiryId"]);
                        model.IsPublished = Convert.ToBoolean(ds.Tables[2].Rows[i]["IsPublished"]);
                        model.EnquiryStatus = Convert.ToString(ds.Tables[2].Rows[i]["EnquiryStatus"]);
                        model.EpcCustomerName = Convert.ToString(ds.Tables[2].Rows[i]["CustomerName"]);
                        model.EnquiryDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["EnquiryDate"]);
                        model.EnquiryDuteDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["ExpiryDate"]);
                        model.Country = Convert.ToString(ds.Tables[2].Rows[i]["Country"]);
                        model.BomId = Convert.ToInt32(ds.Tables[2].Rows[i]["BomId"]);
                        model.IsLatestRevision = Convert.ToBoolean(ds.Tables[2].Rows[i]["IsLatestRevision"]);
                        model.TNumber = Convert.ToString(ds.Tables[2].Rows[i]["TNumber"]);
                        //model.ProjectName = Convert.ToString(ds.Tables[2].Rows[i]["ProjectName"]);
                        //model.ProjectNo = Convert.ToString(ds.Tables[2].Rows[i]["TenderFileNo"]);
                        //model.EndCustomerName = Convert.ToString(ds.Tables[2].Rows[i]["EndCustName"]);
                        //model.ProjectDueDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["ProjectDueDate"]);
                        //model.ProjectCreateDate = Convert.ToDateTime(ds.Tables[2].Rows[i]["CreateDate"]);
                        //model.EnquiryType = Convert.ToString(ds.Tables[2].Rows[i]["Type"]);
                        //model.ProjectStatus = Convert.ToString(ds.Tables[2].Rows[i]["ProjectStatus"]);
                        innerDataList.Add(model);
                    }
                }

                masterModel.BomList = bomList;
                masterModel.DataList = dataList;
                masterModel.InnerDataList = innerDataList;
                return masterModel;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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