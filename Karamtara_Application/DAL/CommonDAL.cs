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
    public class CommonDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public bool CheckIfCodeOrCatNumExists(string data, int type)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_CheckIfCodeOrCatNumExists", connection);
                cmd.Parameters.Add(new SqlParameter("@data", data));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var result = cmd.ExecuteScalar();
                connection.Close();
                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                return true;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int DeleteMasterData(int id, int type, int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd = new SqlCommand("sp_DeleteMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@id", id));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
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

        public List<UnitMaster> GetUnitList()
        {
            connection = new SqlConnection(connectionString);

            DataSet ds = new DataSet();
            List<UnitMaster> unitList = new List<UnitMaster>();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();

            try
            {
                cmd = new SqlCommand("sp_GetUnitList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        UnitMaster uModel = new UnitMaster();
                        uModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        uModel.UnitName = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        unitList.Add(uModel);
                    }
                }
                return unitList;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return unitList;

        }
    }
}