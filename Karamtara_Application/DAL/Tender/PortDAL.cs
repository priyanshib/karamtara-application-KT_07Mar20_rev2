using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Karamtara_Application.DAL.Tender
{
    public class PortDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public AssignPortModel GetPorts(int tenderId, int tenderRevId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            AssignPortModel portModel = new AssignPortModel();
            portModel.DomPortList = new List<AssignPortModel>();
            portModel.IntlPortList = new List<AssignPortModel>();
            try
            {
                cmd = new SqlCommand("sp_GetPortDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@tndId", tenderId));
                cmd.Parameters.Add(new SqlParameter("@tndRevId", tenderRevId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                foreach (DataRow item in dt.Rows)
                {
                    AssignPortModel model = new AssignPortModel();
                    model.PortName = Convert.ToString(item["PortName"]);
                    model.PortId = Convert.ToInt32(item["PortId"]);
                    model.CurrencyId = Convert.ToInt32(item["CurrencyId"]);
                    model.CurrencyName = Convert.ToString(item["CurrencyName"]);
                    model.PortTypeName = Convert.ToString(item["PortDesc"]);
                    if (model.PortTypeName.ToLower() == "domestic")
                        portModel.DomPortList.Add(model);
                    else
                        portModel.IntlPortList.Add(model);
                }
            }
            catch (Exception ex)
            {
                return portModel;

            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return portModel;
        }
    }
}