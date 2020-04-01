using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class BomRevisionDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<MasterModel> GetBomData(int bomId, int oldRevId, int enqId, int currRevId, string copyFrom, bool isTemp)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int newRevNo = 0;
            // if (isTemp.Equals("False") || isTemp.Equals("false"))
            {
                if (copyFrom.Equals("Master"))
                {
                    newRevNo = InsertOldBOMData_FromMaster(bomId, oldRevId, enqId, currRevId);
                }
                else if (copyFrom.Equals("BOM"))
                {
                    newRevNo = InsertData_FromBOM(bomId, oldRevId, enqId, currRevId);
                }
            }
            try
            {
                cmd = new SqlCommand("Sp_GetBOMHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", newRevNo));
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
                        msModel.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]) ?? string.Empty;
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

        public int InsertOldBOMData_FromMaster(int bomId, int oldRevId, int enqId, int currRevId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_InsertOldBomData", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@oldRevId", oldRevId));
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@newRevNo", currRevId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                List<int> prodIdList = new List<int>();
                int revNo = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    int prodId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                    int type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                    revNo = Convert.ToInt32(ds.Tables[0].Rows[i]["RevisionNo"]);
                    UpdateBOMTables_FromMaster(prodId, type, bomId, revNo);
                }
                // int newRevisionNo = Convert.ToInt32(ds.Tables[1].Rows[0]["RevisionNo"]);
                return revNo;
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

        public int UpdateBOMTables_FromMaster(int prodId, int prodType, int bomId, int revId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_UpdateBOMTables", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revId", revId));
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

        public CreateBOMModel GetCurrentRevDetails(int bomId, int enqId, int userId, string bomSource)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_GetCurrentRevDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.Parameters.Add(new SqlParameter("@bomSource", bomSource));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                int revNo = 0;
                int isTemp = 0;
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    revNo = Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionNo"]);
                    isTemp = Convert.ToInt32(ds.Tables[0].Rows[0]["Temp"]);
                    bomModel.TNumber = Convert.ToString(ds.Tables[0].Rows[0]["TNumber"]) ?? string.Empty;
                    bomModel.BomId = bomId;
                    bomModel.EnquiryId = enqId;
                    bomModel.RevisionNo = revNo;
                    bomModel.IsTemp = Convert.ToBoolean(isTemp);
                }
                bomModel.IsEdit = false;
                return bomModel;
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

        public List<MasterModel> ViewBomData(int bomId, int revId, int enqId)
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
                        msModel.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]) ?? string.Empty;
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
                        msModel.TotalUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitGrWt"]);
                        msModel.TotalUnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitNetWt"]);
                        msModel.WastagePercentage = Convert.ToDecimal(ds.Tables[0].Rows[i]["WastagePerc"]);
                        msModel.CalculatedUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["CalculatedGrWt"]);
                        msModel.PrimaryId = Convert.ToInt32(ds.Tables[0].Rows[i]["PrimaryId"]);
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

        public List<MasterModel> ViewTenderBomData(int bomId, int revId, int enqId,int tndNo,int tndRevNo)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("Sp_GetTenderBOMHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revId));
                cmd.Parameters.Add(new SqlParameter("@tndNo", tndNo));
                cmd.Parameters.Add(new SqlParameter("@tndRevNo", tndRevNo));
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
                        msModel.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]) ?? string.Empty;
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
                        msModel.TotalUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitGrWt"]);
                        msModel.TotalUnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitNetWt"]);
                        msModel.WastagePercentage = Convert.ToDecimal(ds.Tables[0].Rows[i]["WastagePerc"]);
                        msModel.CalculatedUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["CalculatedGrWt"]);
                        msModel.PrimaryId = Convert.ToInt32(ds.Tables[0].Rows[i]["PrimaryId"]);
                        msModel.GalCost = Convert.ToDecimal(ds.Tables[0].Rows[i]["GalCost"]);
                        msModel.BlackCost = Convert.ToDecimal(ds.Tables[0].Rows[i]["BlackCost"]);
                        msModel.CostPerPiece = Convert.ToDecimal(ds.Tables[0].Rows[i]["CostPerPiece"]);
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

        public BOMRevisionModel GetSubAssmData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            BOMRevisionModel revModel = new BOMRevisionModel();
            List<MasterModel> masterList = new List<MasterModel>();
            try
            {
                cmd = new SqlCommand("sp_BomRev_GetSubAssmDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                string prodName = string.Empty;
                string catNo = string.Empty;

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel model = new MasterModel();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        model.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        model.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        model.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        model.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatNo"]);
                        model.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        model.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        model.Grade = Convert.ToString(ds.Tables[0].Rows[i]["Grade"]);
                        model.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        model.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        model.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        model.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        model.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        masterList.Add(model);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    revModel.ProductId = Convert.ToInt32(ds.Tables[1].Rows[0]["SubAssemblyId"]);
                    revModel.ProductName = Convert.ToString(ds.Tables[1].Rows[0]["ItemName"]);
                    revModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[0]["CatalogueNo"]);
                    revModel.Type = Convert.ToInt32(ds.Tables[1].Rows[0]["ParentType"]);
                    revModel.SubAssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["SubAssemblyId"]);
                    revModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["AssemblyId"]);
                    revModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProductGroupId"]);
                }
                revModel.MasterList = masterList;
                return revModel;
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

        public SummaryModel GetSummary(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            SummaryModel summary = new SummaryModel();
            List<DataModel> dmList = new List<DataModel>();
            try
            {
                cmd = new SqlCommand("sp_GetSummary", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revId", revNo));

                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        DataModel dm = new DataModel();
                        dm.Name = Convert.ToString(ds.Tables[1].Rows[i]["GroupName"]);
                        dm.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalUnitGrWt"]);
                        dm.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalUnitNetWt"]);
                        dmList.Add(dm);
                    }
                }

                summary.data = dmList;
                return summary;
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

        public BomAuditModel GetBomAudit(int bomId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            BomAuditModel model = new BomAuditModel();
            model.AuditList = new List<BomAuditModel>();
            try
            {
                cmd = new SqlCommand("sp_GetBomAuditTrial", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));

                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BomAuditModel mod = new BomAuditModel();
                        mod.UserName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        mod.Timestamp = Convert.ToDateTime(ds.Tables[0].Rows[i]["EditedDate"]);
                        mod.Version = Convert.ToString(ds.Tables[0].Rows[i]["Version"]);
                        mod.BomId = bomId;
                        mod.BomRevId = Convert.ToInt32(ds.Tables[0].Rows[i]["BomRevId"]);
                        model.AuditList.Add(mod);
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return model;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public BOMRevisionModel GetAssmData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            BOMRevisionModel revModel = new BOMRevisionModel();
            List<MasterModel> masterList = new List<MasterModel>();
            try
            {
                cmd = new SqlCommand("sp_BomRev_GetAssmDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel model = new MasterModel();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        model.Material = Convert.ToString(ds.Tables[0].Rows[i]["material"]);
                        model.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        model.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        model.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatNo"]);
                        model.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        model.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        model.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        model.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        model.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        model.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        masterList.Add(model);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    revModel.ProductId = Convert.ToInt32(ds.Tables[1].Rows[0]["AssemblyId"]);
                    revModel.ProductName = Convert.ToString(ds.Tables[1].Rows[0]["TechnicalName"]);
                    revModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[0]["CatalogueNo"]);
                    revModel.Type = Convert.ToInt32(ds.Tables[1].Rows[0]["ParentType"]);
                    revModel.SubAssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["SubAssemblyId"]);
                    revModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["AssemblyId"]);
                    revModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProductGroupId"]);
                }
                revModel.MasterList = masterList;
                return revModel;
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

        public BOMRevisionModel GetPGData(int prodId, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            BOMRevisionModel revModel = new BOMRevisionModel();
            List<MasterModel> masterList = new List<MasterModel>();
            try
            {
                cmd = new SqlCommand("sp_BomRev_GetPGDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel model = new MasterModel();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        model.Material = Convert.ToString(ds.Tables[0].Rows[i]["material"]) ?? string.Empty;
                        model.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        model.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        model.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatNo"]) ?? string.Empty;
                        model.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]) ?? string.Empty;
                        model.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        model.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        model.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        model.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        model.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        masterList.Add(model);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    revModel.ProductId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProductGroupId"]);
                    revModel.ProductName = Convert.ToString(ds.Tables[1].Rows[0]["GroupName"]);
                    revModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[0]["CatalogueNo"]);
                    revModel.Type = Convert.ToInt32(ds.Tables[1].Rows[0]["ParentType"]);
                    revModel.SubAssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["SubAssemblyId"]);
                    revModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["AssemblyId"]);
                    revModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProductGroupId"]);
                }
                revModel.MasterList = masterList;
                return revModel;
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

        public int RemoveSubAssmData(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_RemoveSubAssmProducts", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@parentId", parentId));
                // cmd.Parameters.Add(new SqlParameter("@parentType", parentType));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
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

        public int RemoveAssmData(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_RemoveAssmProducts", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@parentId", parentId));
                // cmd.Parameters.Add(new SqlParameter("@parentType", parentType));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
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

        public int RemovePGData(int prodId, int prodType, int bomId, int revNo, int parentId, int parentType, int subAssmId, int assmId, int pgId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_RemovePGProducts", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@parentId", parentId));
                // cmd.Parameters.Add(new SqlParameter("@parentType", parentType));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
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

        public int SaveSubAssmChanges(FormCollection form)
        {
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["revNo"]);
            int prodId = Convert.ToInt32(form["ProdId"]);
            int prodPgId = Convert.ToInt32(form["PgId"]);
            int prodAssmId = Convert.ToInt32(form["AssmId"]);
            int prodSubAssmId = Convert.ToInt32(form["SubAssmId"]);
            int prodCompId = Convert.ToInt32(form["CompId"]);
            int prodType = Convert.ToInt32(form["ProdType"]);

            string itemPgId = form["ItemPGId"];
            string itemAssmId = form["ItemAssmId"];
            string itemSubAssmId = form["ItemSubAssmId"];

            string itemId = form["ItemId"];
            string itemQty = form["ItemQty"];
            string itemType = form["ItemType"];
            int status = 0;
            try
            {
                List<string> itemIdList = new List<string>(itemId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemQtyList = new List<string>(itemQty.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemTypeList = new List<string>(itemType.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                List<string> itemPgIdList = new List<string>(itemPgId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemAssmIdList = new List<string>(itemAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemSubAssmIdList = new List<string>(itemSubAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                for (int i = 0; i < itemIdList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    cmd = new SqlCommand("sp_BomRev_SaveSubAssm", connection);
                    cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                    cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                    cmd.Parameters.Add(new SqlParameter("@ProdId", prodId));
                    cmd.Parameters.Add(new SqlParameter("@ItemId", itemIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@ItemQty", itemQtyList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemType", itemTypeList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemPgId", itemPgIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemAssmId", itemAssmIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemSubAssmId", itemSubAssmIdList[i]));
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public int SaveAssemblyChanges(FormCollection form)
        {
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["revNo"]);
            int prodId = Convert.ToInt32(form["ProdId"]);
            int prodPgId = Convert.ToInt32(form["PgId"]);
            int prodAssmId = Convert.ToInt32(form["AssmId"]);
            int prodSubAssmId = Convert.ToInt32(form["SubAssmId"]);
            int prodCompId = Convert.ToInt32(form["CompId"]);
            int prodType = Convert.ToInt32(form["ProdType"]);
            string itemId = form["ItemId"];
            string itemQty = form["ItemQty"];
            string itemType = form["ItemType"];

            string itemPgId = form["ItemPGId"];
            string itemAssmId = form["ItemAssmId"];
            string itemSubAssmId = form["ItemSubAssmId"];
            int status = 0;
            try
            {
                List<string> itemIdList = new List<string>(itemId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemQtyList = new List<string>(itemQty.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemTypeList = new List<string>(itemType.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                List<string> itemPgIdList = new List<string>(itemPgId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemAssmIdList = new List<string>(itemAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemSubAssmIdList = new List<string>(itemSubAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                for (int i = 0; i < itemIdList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    cmd = new SqlCommand("sp_BomRev_SaveAssembly", connection);
                    cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                    cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                    cmd.Parameters.Add(new SqlParameter("@ProdId", prodId));
                    cmd.Parameters.Add(new SqlParameter("@ItemId", itemIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@ItemQty", itemQtyList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemType", itemTypeList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemPgId", itemPgIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemAssmId", itemAssmIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemSubAssmId", itemSubAssmIdList[i]));
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int SavePGChanges(FormCollection form)
        {
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["revNo"]);
            int prodId = Convert.ToInt32(form["ProdId"]);
            int prodPgId = Convert.ToInt32(form["PgId"]);
            int prodAssmId = Convert.ToInt32(form["AssmId"]);
            int prodSubAssmId = Convert.ToInt32(form["SubAssmId"]);
            int prodCompId = Convert.ToInt32(form["CompId"]);
            int prodType = Convert.ToInt32(form["ProdType"]);
            string itemId = form["ItemId"];
            string itemQty = form["ItemQty"];
            string itemType = form["ItemType"];
            string itemPgId = form["ItemPGId"];
            string itemAssmId = form["ItemAssmId"];
            string itemSubAssmId = form["ItemSubAssmId"];
            int status = 0;
            try
            {
                List<string> itemIdList = new List<string>(itemId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemQtyList = new List<string>(itemQty.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemTypeList = new List<string>(itemType.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                List<string> itemPgIdList = new List<string>(itemPgId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemAssmIdList = new List<string>(itemAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> itemSubAssmIdList = new List<string>(itemSubAssmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                for (int i = 0; i < itemIdList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    cmd = new SqlCommand("sp_BomRev_SavePG", connection);
                    cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                    cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                    cmd.Parameters.Add(new SqlParameter("@ProdId", prodId));
                    cmd.Parameters.Add(new SqlParameter("@ItemId", itemIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@ItemQty", itemQtyList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemType", itemTypeList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemPgId", itemPgIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemAssmId", itemAssmIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@itemSubAssmId", itemSubAssmIdList[i]));
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public int InsertData_FromBOM(int bomId, int oldRevId, int enqId, int currRevId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_InsertOldBomData", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@oldRevId", oldRevId));
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@newRevNo", currRevId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                List<int> prodIdList = new List<int>();
                int currRevNumber = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    int prodId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                    int type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                    currRevNumber = Convert.ToInt32(ds.Tables[0].Rows[i]["RevisionNo"]);
                    int oldRevNumber = Convert.ToInt32(ds.Tables[0].Rows[i]["OldRevNo"]);
                    UpdateBOMTablesFromBOM(prodId, type, bomId, currRevNumber, oldRevNumber);
                }
                //int newRevisionNo = Convert.ToInt32(ds.Tables[1].Rows[0]["RevisionNo"]);
                return currRevNumber;
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

        public int UpdateBOMTablesFromBOM(int prodId, int prodType, int bomId, int newRevId, int oldRevid)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_UpdateBOMTables_FromBOM", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@newRevId", newRevId));
                cmd.Parameters.Add(new SqlParameter("@oldRevId", oldRevid));
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

        public int PublishBOMRevision(int bomId, int revNo, string tNumber, int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_BomRevisionPublish", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@tNumber", tNumber));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                int status = 0;
                status = Convert.ToInt32(cmd.ExecuteScalar());
                if (status > 0)
                {
                    SendBOMCreatedEmail(bomId, revNo);
                }
                connection.Close();
                CreateBOMDAL dal = new CreateBOMDAL();
                dal.SaveBomAuditTrial(bomId, revNo, userId, false);
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

        public int CancelBOMRevision(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_DelBomRevData", connection);
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

        int rowIndex = 1;
        ExcelRange cell;
        ExcelFill fill;
        Border border;
        public byte[] GetExcel(List<MasterModel> masterModels, int enqId, int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            string ProjectName = "";
            string EndCustomerName = "";
            string CustomerName = "";
            string tndFileNo = string.Empty;
            string publishedBy = string.Empty;
            StringBuilder sbAssigned = new StringBuilder();
            string BOMassignedTo = string.Empty;
            try
            {
                cmd = new SqlCommand("sp_GetBOMProjDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                    EndCustomerName = Convert.ToString(ds.Tables[0].Rows[0]["EndCustName"]);
                    CustomerName = Convert.ToString(ds.Tables[0].Rows[0]["CustomerName"]);
                    tndFileNo = Convert.ToString(ds.Tables[0].Rows[0]["TenderFileNo"]);
                    publishedBy = Convert.ToString(ds.Tables[0].Rows[0]["PublishedBy"]);
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        string assigned = Convert.ToString(ds.Tables[1].Rows[i]["AssignedTo"]);
                        sbAssigned.Append(assigned);
                        sbAssigned.Append(", ");
                    }
                }

                char[] charsToTrim = { ',', ' ' };
                BOMassignedTo = Convert.ToString(sbAssigned).Trim(charsToTrim);
            }
            catch (Exception)
            {
            }

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";
                var sheet = excelPackage.Workbook.Worksheets.Add("MasterModel");
                sheet.Name = "Master Model";
                sheet.Column(2).Width = 15;
                sheet.Column(3).Width = 20;
                sheet.Column(4).Width = 30;
                sheet.Column(5).Width = 40;
                sheet.Column(7).Width = 20;
                sheet.Column(8).Width = 30;
                sheet.Column(9).Width = 30;
                sheet.Column(10).Width = 40;
                sheet.Column(11).Width = 40;
                //sheet.Column(12).Width = 30;
                //sheet.Column(13).Width = 25;

                #region Report Header
                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "Project Name :" + ProjectName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 20;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "End Customer Name :" + EndCustomerName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "EPC Customer Name :" + CustomerName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "Tender File Name :" + tndFileNo;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "BOM Assigned to :" + BOMassignedTo;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 1;

                sheet.Cells[rowIndex, 1, rowIndex, 13].Merge = true;
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "BOM Published By:" + publishedBy;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 15;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                rowIndex = rowIndex + 2;


                #endregion

                #region Table Header
                cell = sheet.Cells[rowIndex, 1];
                cell.Value = "Sr.No";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 2];
                cell.Value = "Catalogue No";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 3];
                cell.Value = "Code";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 4];
                cell.Value = "Item Name";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 5];
                cell.Value = "Raw Material";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 6];
                cell.Value = "Size";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 7];
                cell.Value = "Quantity (Nos)";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 8];
                cell.Value = "Unit Gross Weight (Kg)";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 9];
                cell.Value = "Unit Net Weight (Kg)";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 10];
                cell.Value = "Total Unit Gross Weight (Kg)";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                cell = sheet.Cells[rowIndex, 11];
                cell.Value = "Total Unit Net Weight (Kg)";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(Color.LightGray);
                border = cell.Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                //cell = sheet.Cells[rowIndex, 12];
                //cell.Value = "Drawing No";
                //cell.Style.Font.Bold = true;
                //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //fill = cell.Style.Fill;
                //fill.PatternType = ExcelFillStyle.Solid;
                //fill.BackgroundColor.SetColor(Color.LightGray);
                //border = cell.Style.Border;
                //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                //cell = sheet.Cells[rowIndex, 13];
                //cell.Value = "Material Grade";
                //cell.Style.Font.Bold = true;
                //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //fill = cell.Style.Fill;
                //fill.PatternType = ExcelFillStyle.Solid;
                //fill.BackgroundColor.SetColor(Color.LightGray);
                //border = cell.Style.Border;
                //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                rowIndex = rowIndex + 1;
                #endregion

                #region Table body
                if (masterModels.Count > 0)
                {
                    //int foreachCount = 1;
                    //int charCount = 97;
                    //int capCount = 65;
                    foreach (MasterModel masterModel in masterModels)
                    {
                        if (masterModel.IsRelated)
                        {
                            cell = sheet.Cells[rowIndex, 1];
                            cell.Value = masterModel.SrNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 2];
                            cell.Value = masterModel.CatalogueNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 3];
                            cell.Value = masterModel.Code;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 4];
                            cell.Value = masterModel.Name;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 5];
                            cell.Value = masterModel.Material;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 6];
                            cell.Value = masterModel.Size;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 7];
                            cell.Value = masterModel.Quantity;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 8];
                            cell.Value = masterModel.UnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 9];
                            cell.Value = masterModel.UnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 10];
                            cell.Value = masterModel.TotalUnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 11];
                            cell.Value = masterModel.TotalUnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 12];
                            //cell.Value = masterModel.DrawingNo;
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 13];
                            //cell.Value = "";
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //foreachCount++;
                            //capCount = 65;
                            //charCount = 97;

                            rowIndex = rowIndex + 1;
                        }
                        else if (masterModel.Type == 2)
                        {
                            cell = sheet.Cells[rowIndex, 1];
                            cell.Value = masterModel.SrNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 2];
                            cell.Value = masterModel.CatalogueNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 3];
                            cell.Value = masterModel.Code;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 4];
                            cell.Value = masterModel.Name;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 5];
                            cell.Value = masterModel.Material;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 6];
                            cell.Value = masterModel.Size;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 7];
                            cell.Value = masterModel.Quantity;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 8];
                            cell.Value = masterModel.UnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 9];
                            cell.Value = masterModel.UnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 10];
                            cell.Value = masterModel.TotalUnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 11];
                            cell.Value = masterModel.TotalUnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 12];
                            //cell.Value = masterModel.DrawingNo;
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 13];
                            //cell.Value = "";
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //capCount++;
                            //charCount = 97;

                            rowIndex = rowIndex + 1;
                        }
                        else if (masterModel.Type == 3)
                        {
                            cell = sheet.Cells[rowIndex, 1];
                            cell.Value = masterModel.SrNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 2];
                            cell.Value = masterModel.CatalogueNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 3];
                            cell.Value = masterModel.Code;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 4];
                            cell.Value = masterModel.Name;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 5];
                            cell.Value = masterModel.Material;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 6];
                            cell.Value = masterModel.Size;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 7];
                            cell.Value = masterModel.Quantity;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 8];
                            cell.Value = masterModel.UnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 9];
                            cell.Value = masterModel.UnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 10];
                            cell.Value = masterModel.TotalUnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 11];
                            cell.Value = masterModel.TotalUnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 12];
                            //cell.Value = masterModel.DrawingNo;
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 13];
                            //cell.Value = "";
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //charCount++;

                            rowIndex = rowIndex + 1;
                        }
                        else
                        {
                            cell = sheet.Cells[rowIndex, 1];
                            cell.Value = masterModel.SrNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 2];
                            cell.Value = masterModel.CatalogueNo;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 3];
                            cell.Value = masterModel.Code;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 4];
                            cell.Value = masterModel.Name;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 5];
                            cell.Value = masterModel.Material;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 6];
                            cell.Value = masterModel.Size;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 7];
                            cell.Value = masterModel.Quantity;
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 8];
                            cell.Value = masterModel.UnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 9];
                            cell.Value = masterModel.UnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 10];
                            cell.Value = masterModel.TotalUnitGrWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            cell = sheet.Cells[rowIndex, 11];
                            cell.Value = masterModel.TotalUnitNetWt.ToString("N3");
                            //cell.Style.Font.Bold = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.White);
                            border = cell.Style.Border;
                            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 12];
                            //cell.Value = masterModel.DrawingNo;
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            //cell = sheet.Cells[rowIndex, 13];
                            //cell.Value = "";
                            ////cell.Style.Font.Bold = true;
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            //fill = cell.Style.Fill;
                            //fill.PatternType = ExcelFillStyle.Solid;
                            //fill.BackgroundColor.SetColor(Color.White);
                            //border = cell.Style.Border;
                            //border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                            rowIndex = rowIndex + 1;
                        }

                    }
                }
                #endregion

                return excelPackage.GetAsByteArray();

            }
        }

        public CreateBOMModel GetBomProjDetails(int bomId, int revNo, int enqId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetBOMProjDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                CreateBOMModel bOMModel = new CreateBOMModel();
                bOMModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"]);
                bOMModel.CustomerName = Convert.ToString(ds.Tables[0].Rows[0]["CustomerName"]);
                bOMModel.TNumber = Convert.ToString(ds.Tables[0].Rows[0]["TNumber"]);
                bOMModel.PublishedBy = Convert.ToString(ds.Tables[0].Rows[0]["PublishedBy"]);
                List<string> assignedList = new List<string>();
                StringBuilder strbAssignNames = new StringBuilder();
                char[] charsToTrim = { ',', ' ' };
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {

                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        string assignTo = Convert.ToString(ds.Tables[1].Rows[i]["AssignedTo"]);
                        strbAssignNames.Append(assignTo);
                        strbAssignNames.Append(", ");
                    }

                }
                bOMModel.AssignedToNames = Convert.ToString(strbAssignNames).Trim(charsToTrim);
                return bOMModel;
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
        public void SendBOMCreatedEmail(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<string> receiverList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetMailList_BOM", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                string projName = string.Empty;
                string custName = string.Empty;
                string tndFileNo = string.Empty;

                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string email = Convert.ToString(ds.Tables[0].Rows[i]["EmailId"]);
                        receiverList.Add(email);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    projName = Convert.ToString(ds.Tables[1].Rows[0]["ProjectName"]);
                    custName = Convert.ToString(ds.Tables[1].Rows[0]["CustomerName"]);
                    tndFileNo = Convert.ToString(ds.Tables[1].Rows[0]["TenderFileNo"]);
                }
                if (receiverList != null && receiverList.Count > 0)
                {
                    Send_BOMCreated_Email(receiverList, projName, custName, tndFileNo, revNo);
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

        public void Send_BOMCreated_Email(List<string> receivers, string projName, string custName, string tndFileNo, int revNo)
        {
            List<string> body = new List<string>();
            for (int i = 0; i < receivers.Count; i++)
            {
                string host = string.Empty;
                string port = string.Empty;
                if (HttpContext.Current != null)
                {
                    host = HttpContext.Current.Request.Url.Host;
                    port = HttpContext.Current.Request.Url.Port.ToString();
                }
                string emailBody = PopulateBody(projName, custName, tndFileNo, revNo);

                body.Add(Convert.ToString(emailBody));
            }
            string subject = "Karamtara - New BOM Published";
            EmailService emailService = new EmailService();
            emailService.SendDifferentEmailsAsync(receivers, subject, body);
        }

        private string PopulateBody(string projName, string custName, string tndFileNo, int revNo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/BOM-Published.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{tenderFileNo}", tndFileNo);
            body = body.Replace("{projectName}", projName);
            body = body.Replace("{epcCustomerName}", custName);
            body = body.Replace("{revisionNo}", Convert.ToString(revNo));
            return body;
        }

        public int AddNewComponent(int bomId, int revNo, int prodId, int prodType, string tNumber)
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                SqlTransaction trans;
                trans = connection.BeginTransaction(IsolationLevel.Snapshot);
                CreateBOMDAL createBomDal = new CreateBOMDAL();
                int bomMasterStatus = 0;
                int updateTableStatus = 0;
                bomMasterStatus += createBomDal.InsertData_BomMaster(prodId, prodType, bomId, revNo, tNumber, connection, trans);
                updateTableStatus += createBomDal.UpdateBOMTables(prodId, prodType, bomId, revNo, connection, trans);
                return 1;
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

        public List<MasterModel> GetEdit_RevisionData(int bomId, int revNo)
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
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
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
                        msModel.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]) ?? string.Empty;
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

        public CreateBOMModel GetEditDetails(int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetBOMEditDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                CreateBOMModel bOMModel = new CreateBOMModel();
                bOMModel.BomId = bomId;
                bOMModel.RevisionNo = revNo;
                bOMModel.BomType = Convert.ToString(ds.Tables[0].Rows[0]["BomType"]);
                bOMModel.BomSource = Convert.ToString(ds.Tables[0].Rows[0]["BomSource"]);
                if (bOMModel.BomType.ToLower().Equals("revision") || bOMModel.BomType.ToLower().Equals("clone"))
                {
                    if (bOMModel.BomSource.ToLower().Equals("master"))
                    {
                        bOMModel.IsEdit = true;
                    }
                    else
                    {
                        bOMModel.IsEdit = false;
                    }
                }
                return bOMModel;
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

        public List<MasterModel> RefreshDataFromMaster(int bomId, int revNo)
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
                delStatus = DeleteOldDataFromBOMTable(bomId, revNo);
                // if (delStatus > 0)
                for (int i = 0; i < prodIdList.Count; i++)
                {
                    UpdateBOMTables_FromMaster(prodIdList[i], prodTypeList[i], bomId, revNo);
                }
                bomModel.MasterList = GetEdit_RevisionData(bomId, revNo);
                return bomModel.MasterList;
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

        public int RemoveProdGrpData(int prodId, int prodType, int bomId, int revNo, int pgId, int assmId, int subAssmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_RemovePgProds", connection);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@pgId", pgId));
                cmd.Parameters.Add(new SqlParameter("@assmId", assmId));
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
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

        public int SaveBOMChanges(FormCollection form)
        {
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["RevisionNo"]);

            string wastePerc = form["WastagePercentage"];
            string calcUnitGrWt = form["CalculatedUnitGrWt"];
            string primaryId = form["PrimaryId"];
            int status = 0;
            try
            {
                List<string> wastePercList = new List<string>(wastePerc.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> calcUnitGrWtList = new List<string>(calcUnitGrWt.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> primaryIdList = new List<string>(primaryId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                for (int i = 0; i < wastePercList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    cmd = new SqlCommand("sp_SaveCompWastage", connection);
                    cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                    cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                    cmd.Parameters.Add(new SqlParameter("@primaryId", primaryIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@wastagePerc", wastePercList[i]));
                    cmd.Parameters.Add(new SqlParameter("@calcGrWt", calcUnitGrWtList[i]));
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception)
            {
                return 0;
            }

        }
    }
}