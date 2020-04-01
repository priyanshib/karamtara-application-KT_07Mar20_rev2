using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace Karamtara_Application.DAL
{
    public class MarkupPricingDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public DataSet GetMarkupPricingList(int bomId, int revId, int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetMarkupPricingDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@revId", revId);
                cmd.Parameters.AddWithValue("@bomId", bomId);
                cmd.Parameters.AddWithValue("@tenderId", tenderId);
                cmd.Parameters.AddWithValue("@tenderRevNo", tenderRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
            }
            catch (Exception ex)
            {

            }
            return ds;
        }

        public bool SaveMasterPricing(string values, int bomId, int revId, int tndId, int tndRevNo, string marginValues,
            string developement, string finalTotalArray, string percToUnitCostArray, string travelLB,string testing,
            string interestRate, string finSalesDays, string finMfgDays, string intSavingAdvDays, string intSavingAdvMnths,string testingRemarks,string travelLBValues)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveMarkupPricing", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@values", values);
                cmd.Parameters.AddWithValue("@marginValues", marginValues);
                cmd.Parameters.AddWithValue("@developement", developement);
                cmd.Parameters.AddWithValue("@finalTotalArray", finalTotalArray);
                cmd.Parameters.AddWithValue("@percToUnitCostArray", percToUnitCostArray);
                cmd.Parameters.AddWithValue("@travelLB", travelLB);
                cmd.Parameters.AddWithValue("@revId", revId);
                cmd.Parameters.AddWithValue("@bomId", bomId);
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                cmd.Parameters.AddWithValue("@testing", testing);
                cmd.Parameters.AddWithValue("@interestRate", interestRate);
                cmd.Parameters.AddWithValue("@finSalesDays", finSalesDays);
                cmd.Parameters.AddWithValue("@finMfgDaysf", finMfgDays);
                cmd.Parameters.AddWithValue("@intSavingAdvDays", intSavingAdvDays);
                cmd.Parameters.AddWithValue("@intSavingAdvMnths", intSavingAdvMnths);
                cmd.Parameters.AddWithValue("@testingRemarks", testingRemarks);
                cmd.Parameters.AddWithValue("@travelLBValues", travelLBValues);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception ex)
            {

            }
            return status > 0 ? true : false;
        }

        public List<dynamic> ToDynamicList(DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }
    }
}