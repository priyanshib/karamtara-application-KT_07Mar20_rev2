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
    }
}