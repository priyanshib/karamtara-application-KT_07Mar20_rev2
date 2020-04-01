using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class FreightChargesDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public DataSet GetFreightChargesList(int bomId, int revId, int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetLineFreightDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@revId", revId);
                cmd.Parameters.AddWithValue("@bomId", bomId);
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);

                connection.Close();
            }
            catch (Exception)
            {

            }
            return ds;
        }

        public bool SaveFreightCharges(string values, int bomId, int revId, int tndId, int tndRevNo, string lineTruckDt, string lineContingency, 
            string lineTotFreights, string lineLoadingFactors, string lineUnitFreight)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveLineFreightDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@values", values);
                cmd.Parameters.AddWithValue("@revId", revId);
                cmd.Parameters.AddWithValue("@bomId", bomId);
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                cmd.Parameters.AddWithValue("@lineTruckDt", lineTruckDt);
                cmd.Parameters.AddWithValue("@lineContingency", lineContingency);
                cmd.Parameters.AddWithValue("@lineFreights", lineTotFreights);
                cmd.Parameters.AddWithValue("@lineLoadingFactors", lineLoadingFactors);
                cmd.Parameters.AddWithValue("@lineUnitFreight", lineUnitFreight);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception ex)
            {

            }
            return status > 0 ? true : false;
        }
    }
}