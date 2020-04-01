using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Karamtara_Application.Models;

namespace Karamtara_Application.DAL
{
    public class MasterDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public int SaveMaster(MasterDataModel model)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            int status = 0;
            string TableName = "";

            if (model.Type == 1)
                TableName = "PGGroupType";
            else if (model.Type == 2)
                TableName = "LineVoltageMS";
            else if (model.Type == 3)
                TableName = "ConductorTypeMaster";

            try
            {
                cmd = new SqlCommand("sp_InsertIntoMasters", connection);
                cmd.Parameters.Add(new SqlParameter("@TableName", TableName));
                cmd.Parameters.Add(new SqlParameter("@Name", model.Name));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<string> GetMasterTypes(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adp = new SqlDataAdapter();
            List<string> Masterlist = new List<string>();

            try
            {
                cmd = new SqlCommand("", connection);
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand = cmd;
                connection.Open();
                adp.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        Masterlist.Add(ds.Tables[0].Rows[i][""].ToString());
                    }
                }
                return Masterlist;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<MasterListModel> GetMasterList(int Mastertype)
        {
            connection = new SqlConnection(connectionString);
            List<MasterListModel> modelList = new List<MasterListModel>();

            string TableName = "";

            if (Mastertype == 1)
                TableName = "PGGroupType";
            else if (Mastertype == 2)
                TableName = "LineVoltageMS";
            else if (Mastertype == 3)
                TableName = "ConductorTypeMaster";

            SqlCommand cmd = new SqlCommand("select * from " + TableName + " where IsActive = 1", connection);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            connection.Open();
            adp.Fill(ds);
            connection.Close();

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    MasterListModel model = new MasterListModel();
                    model.Id = Convert.ToInt32(ds.Tables[0].Rows[i][0]);
                    model.Name = ds.Tables[0].Rows[i][1].ToString();
                    model.TableName = TableName;
                    modelList.Add(model);
                }
            }

            return modelList;
        }

        public int DeleteFromMaster(int Id, string TableName)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd = new SqlCommand("sp_deleteFromMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@Id", Id));
                cmd.Parameters.Add(new SqlParameter("@TableName", TableName));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                return status;
            }
            catch (Exception ex)
            {
                return status;
            }

        }



    }
}