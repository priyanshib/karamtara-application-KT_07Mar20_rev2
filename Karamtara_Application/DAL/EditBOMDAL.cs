using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Karamtara_Application.DAL
{
    public class EditBOMDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public CreateBOMModel GetEditBomData(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                CreateBOMModel bomModel = new CreateBOMModel();
                bomModel.MasterList = new List<MasterModel>();
                cmd = new SqlCommand("sp_GetEditBomData", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                List<BomMasterModel> bomMasterList = new List<BomMasterModel>();
                List<int> prodIdList = new List<int>();
                List<int> prodTypeList = new List<int>();
                int delStatus = 0;
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BomMasterModel bomMsModel = new BomMasterModel();
                        bomMsModel.ProductId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        bomMsModel.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        bomMsModel.ProductName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        bomMsModel.BomId = bomId;
                        bomMsModel.RevisionNo = revNo;
                        bomMasterList.Add(bomMsModel);
                        int prodId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        int prodType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        prodIdList.Add(prodId);
                        prodTypeList.Add(prodType);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    bomModel.EnquiryId = Convert.ToInt32(ds.Tables[1].Rows[0]["EnquiryId"]);
                    bomModel.IsPublished = Convert.ToBoolean(ds.Tables[1].Rows[0]["IsPublished"]);
                    bomModel.ProjectName = Convert.ToString(ds.Tables[1].Rows[0]["ProjectName"]);
                    bomModel.ProjectId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProjectId"]);
                    bomModel.CustomerName = Convert.ToString(ds.Tables[1].Rows[0]["CustomerName"]);
                    bomModel.BomId = Convert.ToInt32(ds.Tables[1].Rows[0]["BOMId"]);
                    bomModel.RevisionNo = Convert.ToInt32(ds.Tables[1].Rows[0]["RevisionNo"]);
                    bomModel.TNumber = Convert.ToString(ds.Tables[1].Rows[0]["TNumber"]);
                    bomModel.Bom = Convert.ToString(ds.Tables[1].Rows[0]["BomType"]);
                    bomModel.BomSource = Convert.ToString(ds.Tables[1].Rows[0]["BomSource"]);
                }
                bomModel.BomMasterList = bomMasterList;
                if (bomModel.BomSource.ToLower().Equals("master"))
                {
                    delStatus = DeleteOldDataFromBOMTable(bomId, revNo);
                    if (delStatus > 0)
                        for (int i = 0; i < prodIdList.Count; i++)
                        {
                            UpdateBOMTablesWithMasterData(bomId, revNo, prodIdList[i], prodTypeList[i]);
                        }
                }
                return bomModel;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public int RemoveProduct(int bomId, int revNo, int prodId, int prodType)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_BOM_RemoveProduct", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int status = 0;
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return 0;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public int UpdateBOMTablesWithMasterData(int bomId, int revNo, int prodId, int prodType)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_UpdateBOMTables", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revId", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int bomIds = 0;
                bomIds = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return bomIds;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public int DeleteOldDataFromBOMTable(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_DeleteOldDataFromBomTables", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int status = 0;
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }
    }
}