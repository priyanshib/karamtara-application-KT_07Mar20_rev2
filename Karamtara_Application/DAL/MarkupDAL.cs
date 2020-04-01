using Karamtara_Application.HelperClass;
using Karamtara_Application.Models.Tender;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using static Karamtara_Application.HelperClass.Flags;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Karamtara_Application.DAL
{
    public class MarkupDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public MarkupModel CreateMaster(MarkupModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_SaveMarkupMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@desc", dataModel.Description));
                cmd.Parameters.Add(new SqlParameter("@value", dataModel.Value));
                cmd.CommandType = CommandType.StoredProcedure;
                //adapter.SelectCommand = cmd;
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                //status = Convert.ToInt32(cmd.ExecuteScalar());
                dataModel.Id = Convert.ToInt32(dt.Rows[0]["Id"]);
                dataModel.Message = dt.Rows[0]["Message"].ToString();
                connection.Close();
            }
            catch (Exception ex)
            {
            }

            return dataModel;
        }

        public int DeleteMaster(MarkupModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_DeleteMarkupMaster", connection);
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

        public List<MarkupModel> GetList()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MarkupModel> list = new List<MarkupModel>();
            try
            {
                cmd = new SqlCommand("sp_getMarkupMasterList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    MarkupModel model = new MarkupModel();
                    model.Description = item["Description"].ToString();
                    model.Value = Convert.ToDecimal(item["Value"]);
                    model.Id = Convert.ToInt32(item["Id"]);
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

        public List<MarkupModel> GetList(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MarkupModel> list = new List<MarkupModel>();
            try
            {
                cmd = new SqlCommand("sp_getMarkupMasterListByPrefix", connection);
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    MarkupModel model = new MarkupModel();
                    model.Description = item["Description"].ToString();
                    model.Value = Convert.ToDecimal(item["Value"]);
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