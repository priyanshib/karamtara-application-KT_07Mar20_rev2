using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Karamtara_Application.DAL
{
    public class BOMCloneDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public BOMRevisionModel bomRevModel;

        public BOMRevisionModel GetCloneData(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            BOMRevisionModel bomRevModel = new BOMRevisionModel();
            try
            {
                cmd = new SqlCommand("sp_GetBomCloneData", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                revNo = 0;
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {

                }
                return null;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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
        public BOMRevisionModel InsertAndGetCloneData(int enqId, int fromBomId, int frmRevId, int toBomId, int toRevNo, int userId,string refreshFrom)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            BOMRevisionModel bomRevModel = new BOMRevisionModel();
            try
            {
                cmd = new SqlCommand("sp_CreateCloneBomId", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.Parameters.Add(new SqlParameter("@toBomId", toBomId));
                cmd.Parameters.Add(new SqlParameter("@toRevNo", toRevNo));
                cmd.Parameters.Add(new SqlParameter("@bomSource", refreshFrom));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    bomRevModel.EnquiryId = Convert.ToInt32(ds.Tables[0].Rows[0]["EnquiryId"]);
                    bomRevModel.BomId = Convert.ToInt32(ds.Tables[0].Rows[0]["BomId"]);
                    bomRevModel.RevisionNo = Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionNo"]);
                }
                InsertBomMasterTable(fromBomId, bomRevModel.BomId, frmRevId, bomRevModel.RevisionNo,refreshFrom);
                return bomRevModel;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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

        public void InsertBomMasterTable(int fromBomId, int toBomId, int frmRevNo, int toRevNo,string refreshBom)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            BOMRevisionModel bomRevModel = new BOMRevisionModel();
            try
            {
                cmd = new SqlCommand("sp_InsertBomMasterFrmBom", connection);
                cmd.Parameters.Add(new SqlParameter("@fromBomId", fromBomId));
                cmd.Parameters.Add(new SqlParameter("@toBomId", toBomId));
                cmd.Parameters.Add(new SqlParameter("@frmRevNo", frmRevNo));
                cmd.Parameters.Add(new SqlParameter("@toRevNo", toRevNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        int prodId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                        int prodType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        if (refreshBom.Equals("BOM"))
                        {
                            Clone_MasterTables_FromBOM(prodId, prodType, fromBomId, toBomId, frmRevNo, toRevNo);
                        }
                        else 
                        {
                            Clone_fromMasterTables(prodId, prodType, fromBomId, toBomId, frmRevNo, toRevNo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public int Clone_MasterTables_FromBOM(int prodId, int prodType, int fromBomId, int toBomId, int frmRevNo, int toRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_Clone_MasterTables", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@fromBomId", fromBomId));
                cmd.Parameters.Add(new SqlParameter("@toBomId", toBomId));
                cmd.Parameters.Add(new SqlParameter("@fromRevId", frmRevNo));
                cmd.Parameters.Add(new SqlParameter("@toRevId", toRevNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int bomIds = 0;
                bomIds = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return bomIds;
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

        public List<MasterModel> GetMasterData(int bomId, int revId)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("Sp_GetBOMHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                List<MasterModel> masterList = new List<MasterModel>();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel msModel = new MasterModel();
                        msModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        msModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]) ?? string.Empty;
                        msModel.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]) ?? string.Empty;
                        msModel.TechnicalName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]) ?? string.Empty;
                        msModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]) ?? string.Empty;
                        msModel.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
                        msModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        msModel.Grade = Convert.ToString(ds.Tables[0].Rows[i]["Grade"]) ?? string.Empty;
                        msModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]) ?? string.Empty;
                        msModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]) ?? string.Empty;
                        msModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        msModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        msModel.ParentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ParentId"]);
                        msModel.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        msModel.IsRelated = Convert.ToBoolean(ds.Tables[0].Rows[i]["Related"]);
                        msModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        msModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        msModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        msModel.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        masterList.Add(msModel);
                    }
                }
                return masterList;

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
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

        public int Clone_fromMasterTables(int prodId,int prodType, int fromBomId, int toBomId, int frmRevNo, int toRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_Clone_InsertMaster_FromMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@fromBomId", fromBomId));
                cmd.Parameters.Add(new SqlParameter("@toBomId", toBomId));
                cmd.Parameters.Add(new SqlParameter("@fromRevId", frmRevNo));
                cmd.Parameters.Add(new SqlParameter("@toRevId", toRevNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int bomIds = 0;
                bomIds = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return bomIds;
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

    }
}