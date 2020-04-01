using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using static Karamtara_Application.HelperClass.Flags;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Karamtara_Application.DAL
{
    public class MasterDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public MasterDataModel CreateMaster(MasterDataModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_SaveMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@id", dataModel.Id));
                cmd.Parameters.Add(new SqlParameter("@type", dataModel.Type));
                cmd.Parameters.Add(new SqlParameter("@name", dataModel.Name));
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

        public int DeleteMaster(MasterDataModel dataModel)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_DeleteMasters", connection);
                cmd.Parameters.Add(new SqlParameter("@type", dataModel.Type));
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

        public List<MasterDataModel> GetListbyType(int type)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MasterDataModel> list = new List<MasterDataModel>();
            try
            {
                cmd = new SqlCommand("sp_getMasterList", connection);
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    MasterDataModel model = new MasterDataModel();
                    model.Name = item["Name"].ToString();
                    model.Id = Convert.ToInt32(item["Id"]);
                    model.Delete = Convert.ToBoolean(item["Delete"]);
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

        public List<MasterDataModel> GetListbyType(int type, string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MasterDataModel> list = new List<MasterDataModel>();
            try
            {
                cmd = new SqlCommand("sp_getMasterListByPrefix", connection);
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    MasterDataModel model = new MasterDataModel();
                    model.Name = item["Name"].ToString();
                    model.Id = Convert.ToInt32(item["Id"]);
                    model.Delete = Convert.ToBoolean(item["Delete"]);
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