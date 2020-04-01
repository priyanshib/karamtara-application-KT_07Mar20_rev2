using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class CurrencyDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public CurrencyModel CreateMaster(CurrencyModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_SaveCurrencyMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@Name", dataModel.Name));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                dataModel.Id = Convert.ToInt32(dt.Rows[0]["Id"]);
                dataModel.Message = dt.Rows[0]["Message"].ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
            }

            return dataModel;
        }

        public int DeleteMaster(CurrencyModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_DeleteCurrencyMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@id", dataModel.Id));
                cmd.CommandType = CommandType.StoredProcedure;
                //adapter.SelectCommand = cmd;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception ex)
            {
            }

            return status;
        }

        public List<CurrencyModel> GetList()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<CurrencyModel> list = new List<CurrencyModel>();
            try
            {
                cmd = new SqlCommand("sp_GetCurrencyMasterList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    CurrencyModel model = new CurrencyModel();
                    model.Name = item["Name"].ToString();
                    model.Id = Convert.ToInt32(item["Id"]);
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

        public List<CurrencyModel> GetList(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<CurrencyModel> list = new List<CurrencyModel>();
            try
            {
                cmd = new SqlCommand("sp_GetCurrencyMasterListByPrefix", connection);
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    CurrencyModel model = new CurrencyModel();
                    model.Name = item["Name"].ToString();
                    model.Id = Convert.ToInt32(item["Id"]);
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

    }
}