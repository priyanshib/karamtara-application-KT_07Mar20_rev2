using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Karamtara_Application.DAL
{
    public class RawMaterialPricingDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<RawMaterialPricingModel> GetRawPricingList(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<RawMaterialPricingModel> list = new List<RawMaterialPricingModel>();
            try
            {
                cmd = new SqlCommand("sp_GetRawMaterialPricingDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderId", tndId);
                cmd.Parameters.AddWithValue("@TenderRevId", tndRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                int count = 1;
                foreach (DataRow item in dt.Rows)
                {
                    RawMaterialPricingModel model = new RawMaterialPricingModel();
                    model.MaterialGroup = item["MaterialGroup"].ToString();
                    model.MaterialDesc = item["MaterialDescription"].ToString();
                    model.Price = Convert.ToDouble(item["Price"].ToString());
                    model.Id = Convert.ToInt32(item["Id"]);
                    model.SrNo = count;
                    list.Add(model);
                    count++;
                }
            }
            catch (Exception)
            {

            }
            return list;
        }

        public List<RawMaterialReportModel> GetRawMaterialPricingListForTender(int tndId, out List<int> Revisions)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<RawMaterialReportModel> list = new List<RawMaterialReportModel>();
            Revisions = new List<int>();
            try
            {
                cmd = new SqlCommand("sp_GetRawMaterialPricingForAllRevisions", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                var tablesCount = ds.Tables.Count;
                if(ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {
                        RawMaterialReportModel model = new RawMaterialReportModel();
                        model.RawMaterialId = Convert.ToInt32(ds.Tables[0].Rows[j]["RawMaterialId"]);
                        model.RawMaterialName = Convert.ToString(ds.Tables[0].Rows[j]["MaterialDescription"]);
                        model.GroupName = Convert.ToString(ds.Tables[0].Rows[j]["Material"]);
                        model.Pricing = new List<RMRevPricingModel>();
                        list.Add(model);
                    }
                }

                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int j = 0; j < ds.Tables[1].Rows.Count; j++)
                    {
                        Revisions.Add(Convert.ToInt32(ds.Tables[1].Rows[j]["TndRevId"]));
                    }
                }

                for (int i = 2; i < tablesCount; i++)
                {
                    if(ds.Tables[i] != null && ds.Tables[i].Rows.Count > 0)
                    {
                        for (int k = 0; k < ds.Tables[i].Rows.Count; k++)
                        {
                            RMRevPricingModel model = new RMRevPricingModel();
                            model.RawMaterialId = Convert.ToInt32(ds.Tables[i].Rows[k]["RawMaterialId"]);
                            model.TenderRevId = Convert.ToInt32(ds.Tables[i].Rows[k]["RevisionId"]);
                            model.Price = Convert.ToDecimal(ds.Tables[i].Rows[k]["Price"]);
                            list.Where(x => x.RawMaterialId == model.RawMaterialId).FirstOrDefault().Pricing.Add(model);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return list;
        }

        public bool SaveRawPricing(RawMaterialPricingDetail list)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            dt = getDataTable(list);
            try
            {
                cmd = new SqlCommand("sp_SaveRawMaterialPricing", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@dataValue", dt);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public DataTable getDataTable(RawMaterialPricingDetail list)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[4] {
                    new DataColumn("Id", typeof(int)),
                    new DataColumn("Price",typeof(double)),
                    new DataColumn("TndId",typeof(double)),
                    new DataColumn("TndRevNo",typeof(double))});

            for (int i = 0; i < list.RawMaterialList.Count; i++)
            {
                dt.Rows.Add(list.RawMaterialList[i].Id, list.RawMaterialList[i].Price, list.RawMaterialList[i].TndId, list.RawMaterialList[i].TndRevNo);
            }

            return dt;
        }
    }
}