using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class CreateBOMDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        SqlTransaction sqlTrans;

        public List<BOMAutoComplete> BOMAutoComplete(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_BOMAutoComplete", connection);
                cmd.Parameters.Add(new SqlParameter("@SearchText", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                List<BOMAutoComplete> autoList = new List<BOMAutoComplete>();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        BOMAutoComplete model = new BOMAutoComplete();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.TName = Convert.ToString(ds.Tables[0].Rows[i]["TName"]);
                        model.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        model.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        autoList.Add(model);
                    }
                }
                return autoList;
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

        public List<string> GetAutoCompleteList(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            List<string> assmList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetAssemblyAutoComplete", connection);
                cmd.Parameters.Add(new SqlParameter("@SearchText", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string assmName = Convert.ToString(ds.Tables[0].Rows[i]["AssemblyName"]);
                        assmList.Add(assmName);
                    }
                }
                return assmList;
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

        public CreateBOMModel GetAssmProducts(int assmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_GetAssemblyDetails_BOM", connection);
                cmd.Parameters.Add(new SqlParameter("@AssmName", assmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                CreateBOMModel bomModel = new CreateBOMModel();
                List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
                List<ComponentModel> compList = new List<ComponentModel>();
                List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
                string assmName = string.Empty;
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        SubAssemblyListModel subAssmModel = new SubAssemblyListModel();
                        subAssmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                        subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["ProductName"]);
                        subAssmModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        subAssmModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        subAssmModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        subAssmModel.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
                        subAssmList.Add(subAssmModel);
                    }
                }

                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProductId"]);
                        compModel.SubAssemblyId = Convert.ToInt32(ds.Tables[1].Rows[i]["SubAssmId"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[1].Rows[i]["ComponentName"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        compModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        compModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"]);
                        compList.Add(compModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        AssemblyMasterModel assemblyModel = new AssemblyMasterModel();
                        assemblyModel.AssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["AssemblyId"]);
                        assemblyModel.AssemblyName = Convert.ToString(ds.Tables[2].Rows[i]["AssemblyName"]);
                        assemblyModel.AssemblyCode = Convert.ToString(ds.Tables[2].Rows[i]["AssemblyCode"]);
                        assemblyModel.AssmTechName = Convert.ToString(ds.Tables[2].Rows[i]["TechnicalName"]);
                        assmName = Convert.ToString(ds.Tables[2].Rows[0]["AssemblyName"]);
                        assmList.Add(assemblyModel);
                    }
                }
                bomModel.DisplayText = assmName;
                bomModel.AssemblyList = assmList;
                bomModel.SubAssemblyList = subAssmList;
                bomModel.ComponentList = compList;
                bomModel.ProductType = 2;
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

        public CreateBOMModel GetProdGroupList(int groupId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
            try
            {
                cmd = new SqlCommand("sp_PG_AssemblyList_BOM", connection);
                cmd.Parameters.Add(new SqlParameter("@Text", groupId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssmId"]);
                        assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[i]["AssmCode"]);
                        assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        assmList.Add(assmModel);
                    }
                    bomModel.AssemblyList = assmList;
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    bomModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[0]["ProductGroupId"]);
                    bomModel.DisplayText = Convert.ToString(ds.Tables[1].Rows[0]["GroupName"]);
                }
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
        public int SaveSubAssmChanges(FormCollection form)
        {
            string subAsmId = form["item.SubAssemblyId"];
            string subAsmCode = form["item.SubAssemblyCode"];
            string tName = form["item.SubAssmTechName"];
            string name = form["item.SubAssemblyName"];
            int AssemblyId = Convert.ToInt32(form["AssemblyId"]);
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                List<string> idList = new List<string>(subAsmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> codeList = new List<String>(subAsmCode.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> tNameList = new List<String>(tName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> nameList = new List<String>(name.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter();
                DataSet ds = new DataSet();
                dt.Columns.AddRange(new DataColumn[7] {
                    new DataColumn("ProjectId", typeof(int)),
                    new DataColumn("EnquiryId",typeof(int)),
                    new DataColumn("AssemblyId", typeof(int)),
                    new DataColumn("SubAssemblyId", typeof(int)),
                    new DataColumn("SubAsmCode", typeof(string)),
                    new DataColumn("TechnicalName", typeof(string)),
                    new DataColumn("Name", typeof(string))});

                for (int i = 0; i < codeList.Count; i++)
                {
                    dt.Rows.Add(1, 1, AssemblyId, idList[i], codeList[i], tNameList[i], nameList[i]);
                }
                cmd = new SqlCommand("sp_BomSaveSubAsmChanges", connection);
                cmd.Parameters.Add(new SqlParameter("@dataValue", dt));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                //da.SelectCommand = cmd;
                //da.Fill(ds);
                int status = 0;
                //status = Convert.ToInt32(ds.Tables[0].Rows[0]["RowsChanged"]);
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

        public int InsertSubAsmBOM(string assmName, int projId, int enqId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            List<string> assmList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_InsertSubAsm_BOM", connection);
                cmd.Parameters.Add(new SqlParameter("@ProjectId", projId));
                cmd.Parameters.Add(new SqlParameter("@EnquiryId", enqId));
                cmd.Parameters.Add(new SqlParameter("@AssmName", assmName));
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

        public CreateBOMModel GetDetails(string assmName, int type)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            List<AssemblyMasterModel> subAssmList = new List<AssemblyMasterModel>();
            try
            {
                cmd = new SqlCommand("sp_GetSubAssemblyList_BOM", connection);
                cmd.Parameters.Add(new SqlParameter("@Text", assmName));
                cmd.Parameters.Add(new SqlParameter("@Type", type));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AssemblyMasterModel subAssmModel = new AssemblyMasterModel();
                        subAssmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssmId"]);
                        subAssmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[i]["SubAssmCode"]);
                        subAssmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        subAssmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        subAssmList.Add(subAssmModel);
                    }
                    bomModel.AssemblyList = subAssmList;
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    bomModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[0]["AssemblyId"]);
                }
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

        public int SaveBOMDetails(FormCollection form, int userId)
        {
            int enquiryId = Convert.ToInt32(form["EnquiryId"]);
            int bomId = Convert.ToInt32(form["BomId"]);
            int revId = Convert.ToInt32(form["RevisionNo"]);
            string prodId = form["hidId"];
            string type = form["hidType"];
            string productId = form["productId"];
            string prodName = form["prodName"];
            string TNumber = form["TNumber"];
            int status = 0;
            int errorCount = 0;
            try
            {
                List<string> prodIdList = new List<string>(prodId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<string> prodNameList = new List<string>(prodName.Split(',')).ToList();
                List<string> typeList = new List<string>(type.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                connection = new SqlConnection(connectionString);


                try
                {
                    connection.Open();
                    sqlTrans = connection.BeginTransaction(IsolationLevel.Snapshot);


                    for (int i = 0; i < prodIdList.Count; i++)
                    {
                        errorCount = InsertData_BomMaster(Convert.ToInt32(prodIdList[i]), Convert.ToInt32(typeList[i]), bomId, revId, TNumber, connection, sqlTrans);
                        if (errorCount < 0)
                        {
                            sqlTrans.Rollback();
                            break;
                        }

                        errorCount = UpdateBOMTables(Convert.ToInt32(prodIdList[i]), Convert.ToInt32(typeList[i]), bomId, revId, connection, sqlTrans);
                        if (errorCount < 0)
                        {
                            sqlTrans.Rollback();
                            break;
                        }

                    }

                    sqlTrans.Commit();
                    status = 1;
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    status = 0;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }

            }
            catch (Exception ex)
            {
                return status;
            }
            SaveBomAuditTrial(bomId, revId, userId, true);

            return status;
        }

        public int SaveBomAuditTrial(int bomId, int revId, int userId, bool IsSaved)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            cmd = new SqlCommand("sp_SaveBomAuditTrial", connection);
            cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
            cmd.Parameters.Add(new SqlParameter("@bomRevId", revId));
            cmd.Parameters.Add(new SqlParameter("@userId", userId));
            cmd.Parameters.Add(new SqlParameter("@isSaved", IsSaved));
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            int status = 0;
            status = Convert.ToInt32(cmd.ExecuteScalar());
            connection.Close();
            return status;
        }

        public CreateBOMModel GetProductDetails(int prodId, int prodType)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            List<ComponentModel> compList = new List<ComponentModel>();
            List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
            try
            {
                cmd = new SqlCommand("sp_GetProductDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@ProdId", prodId));
                cmd.Parameters.Add(new SqlParameter("@ProdType", prodType));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                string prodName = string.Empty;
                if (prodType.Equals(3))
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            SubAssemblyListModel subAssmModel = new SubAssemblyListModel();
                            subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
                            subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["ProductName"]);
                            subAssmModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                            subAssmModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterialId"]);
                            subAssmModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                            subAssmModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["NetUnitWt"]);
                            subAssmList.Add(subAssmModel);
                        }
                        bomModel.SubAssemblyList = subAssmList;
                    }
                    if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {

                        for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        {
                            ComponentModel compModel = new ComponentModel();
                            compModel.SubAssemblyId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProductId"]);
                            compModel.ComponentId = Convert.ToInt32(ds.Tables[1].Rows[i]["SubProductId"]);
                            compModel.ComponentName = Convert.ToString(ds.Tables[1].Rows[i]["ComponentName"]);
                            compModel.RawMaterialId = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                            compModel.Qty = Convert.ToInt32(ds.Tables[1].Rows[i]["SubProdQty"]);
                            compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                            compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                            compModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                            compList.Add(compModel);
                        }
                        bomModel.ComponentList = compList;
                    }
                    if (ds.Tables.Count > 0 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                    {
                        prodName = Convert.ToString(ds.Tables[2].Rows[0]["Name"]);
                    }
                }
                if (prodType.Equals(4))
                {
                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            ComponentModel subProdModel = new ComponentModel();
                            subProdModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                            subProdModel.ComponentName = Convert.ToString(ds.Tables[0].Rows[i]["ComponentName"]);
                            subProdModel.RawMaterialId = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                            subProdModel.Qty = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                            subProdModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                            subProdModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                            subProdModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                            compList.Add(subProdModel);
                        }
                        bomModel.ComponentList = compList;
                    }
                    if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        prodName = Convert.ToString(ds.Tables[1].Rows[0]["ComponentName"]);
                    }

                }
                bomModel.DisplayText = prodName;
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

        public CreateBOMModel GetCurrentBomId(int enqId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_GetCurrentBOMId", connection);
                cmd.Parameters.Add(new SqlParameter("@EnquiryId", enqId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    bomModel.EnquiryId = enqId;
                    bomModel.BomId = Convert.ToInt32(ds.Tables[0].Rows[0]["BomId"].ToString());
                    bomModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[0]["ProjectId"].ToString());
                    bomModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[0]["ProjectName"].ToString());
                    bomModel.CustomerName = Convert.ToString(ds.Tables[0].Rows[0]["CustomerName"].ToString());
                    bomModel.RevisionNo = Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionNo"].ToString());
                    bomModel.IsNewBom = true;
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    // int hasRows = Convert.ToInt32*
                    bomModel.CreateBOMHasRows = Convert.ToBoolean(ds.Tables[1].Rows[0]["BOMMasterHasRows"]);
                }
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

        public int SaveAssemblyChanges(FormCollection form)
        {
            string subAsmId = form["item.SubAssemblyId"];
            string subAsmCode = form["item.SubAssemblyCode"];
            string tName = form["item.SubAssmTechName"];
            string name = form["item.SubAssemblyName"];
            int AssemblyId = Convert.ToInt32(form["AssemblyId"]);
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;
            try
            {
                List<string> subIdList = new List<string>(subAsmId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> codeList = new List<String>(subAsmCode.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> tNameList = new List<String>(tName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> nameList = new List<String>(name.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                for (int i = 0; i < subIdList.Count(); i++)
                {
                    cmd = new SqlCommand("sp_BOMSaveAssemblyChanges", connection);
                    cmd.Parameters.Add(new SqlParameter("@AssmId", AssemblyId));
                    cmd.Parameters.Add(new SqlParameter("@SubAssmId", subIdList[i]));
                    cmd.Parameters.Add(new SqlParameter("@SubAssmCode", codeList[i]));
                    cmd.Parameters.Add(new SqlParameter("@SubAssmName", nameList[i]));
                    cmd.Parameters.Add(new SqlParameter("@TechnicalName", tNameList[i]));

                    cmd.CommandType = CommandType.StoredProcedure;
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
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int UpdateBOMTables(int prodId, int prodType, int bomId, int revId, SqlConnection con, SqlTransaction Sqltrans)
        {
            //connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            try
            {
                cmd = new SqlCommand("sp_UpdateBOMTables", con);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@prodType", prodType));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revId", revId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                cmd.Transaction = Sqltrans;
                int bomIds = 0;
                //con.Open();
                bomIds = Convert.ToInt32(cmd.ExecuteScalar());
                return bomIds;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return 1;
            }
            finally
            {
                //if (connection != null && connection.State == ConnectionState.Open)
                //{
                //    connection.Close();
                //}
            }
        }

        public int CreateBOMRevision(FormCollection form, int userId)
        {
            int enquiryId = Convert.ToInt32(form["EnquiryId"]);
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["RevisionNo"]);
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel assmModel = new CreateBOMModel();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_BomPublish", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                if (status > 0)
                {
                    SendBOMCreatedEmail(bomId, revNo);
                    SaveBomAuditTrial(bomId, revNo, userId, false);
                }
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

        public CreateBOMModel GetBomDetails(int bomId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                //List<ProductGroupModel> groupList = new List<ProductGroupModel>();
                //List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
                //List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
                //List<ComponentModel> compList = new List<ComponentModel>();
                CreateBOMModel bomModel = new CreateBOMModel();
                bomModel.MasterList = new List<MasterModel>();
                cmd = new SqlCommand("Sp_GetBOMHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", bomId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                //cmd.Parameters.Add(new SqlParameter("@revNo", revId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
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
                        bomModel.MasterList.Add(msModel);
                    }
                }
                //if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                //{
                //    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                //    {
                //        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                //        assmModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[i]["AssemblyId"].ToString());
                //        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[i]["SubAssmId"].ToString());
                //        assmModel.AssemblyCode = Convert.ToString(ds.Tables[1].Rows[i]["SubAssmCode"].ToString());
                //        assmModel.AssemblyName = Convert.ToString(ds.Tables[1].Rows[i]["Name"].ToString());
                //        assmModel.AssmTechName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"].ToString());
                //        assmModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"].ToString());
                //        assmList.Add(assmModel);
                //    }
                //}
                //if (ds.Tables.Count > 0 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                //{
                //    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                //    {
                //        SubAssemblyListModel subAssmModel = new SubAssemblyListModel();
                //        subAssmModel.AssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["SubAssmId"].ToString());
                //        subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProductId"].ToString());
                //        subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[2].Rows[i]["ProductName"].ToString());
                //        subAssmModel.CategoryId = Convert.ToInt32(ds.Tables[2].Rows[i]["CategoryId"].ToString());
                //        subAssmModel.Quantity = Convert.ToInt32(ds.Tables[2].Rows[i]["Quantity"].ToString());
                //        subAssmModel.CatalogueNo = Convert.ToString(ds.Tables[2].Rows[i]["CatalogueNo"].ToString());
                //        subAssmModel.Size = Convert.ToString(ds.Tables[2].Rows[i]["Size"].ToString());
                //        subAssmModel.UnitGrWt = Convert.ToDecimal(ds.Tables[2].Rows[i]["UnitGrWt"].ToString());
                //        subAssmModel.UnitNetWt = Convert.ToDecimal(ds.Tables[2].Rows[i]["UnitNetWt"].ToString());
                //        subAssmList.Add(subAssmModel);
                //    }
                //}
                //if (ds.Tables.Count > 0 && ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                //{
                //    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                //    {
                //        ComponentModel compModel = new ComponentModel();
                //        compModel.SubAssemblyId = Convert.ToInt32(ds.Tables[3].Rows[i]["ProductId"].ToString());
                //        compModel.ComponentId = Convert.ToInt32(ds.Tables[3].Rows[i]["SubProductId"].ToString());
                //        compModel.ComponentName = Convert.ToString(ds.Tables[3].Rows[i]["ComponentName"].ToString());
                //        compModel.Qty = Convert.ToInt32(ds.Tables[3].Rows[i]["Quantity"].ToString());
                //        compModel.CatalogueNo = Convert.ToString(ds.Tables[3].Rows[i]["CatalogueNo"].ToString());
                //        compModel.Size = Convert.ToString(ds.Tables[3].Rows[i]["Size"].ToString());
                //        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[3].Rows[i]["UnitGrWt"].ToString());
                //        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[3].Rows[i]["UnitNetWt"].ToString());
                //        compList.Add(compModel);
                //    }
                //}
                //bomModel.ProductGroupList = groupList;
                //bomModel.AssemblyList = assmList;
                //bomModel.SubAssemblyList = subAssmList;
                //bomModel.ComponentList = compList;
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

        public CreateBOMModel ViewBOMDetails(int bomId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CreateBOMModel bomModel = new CreateBOMModel();
            try
            {
                List<ProductGroupModel> groupList = new List<ProductGroupModel>();
                List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
                List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
                List<ComponentModel> compList = new List<ComponentModel>();
                List<BomMasterModel> bomMasterList = new List<BomMasterModel>();
                cmd = new SqlCommand("sp_GetBOMDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", bomId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ProductGroupModel groupModel = new ProductGroupModel();
                        groupModel.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"].ToString());
                        groupModel.ProductGroupName = Convert.ToString(ds.Tables[0].Rows[i]["AssemblyName"].ToString());
                        groupModel.ProductGroupCode = Convert.ToString(ds.Tables[0].Rows[i]["AssemblyCode"].ToString());
                        groupModel.GroupType = Convert.ToString(ds.Tables[0].Rows[i]["AssemblyType"].ToString());
                        groupModel.LineVoltage = Convert.ToString(ds.Tables[0].Rows[i]["LineVoltage"].ToString());
                        groupModel.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"].ToString());
                        groupModel.BundleType = Convert.ToString(ds.Tables[0].Rows[i]["BundleType"].ToString());
                        groupList.Add(groupModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                        assmModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[i]["AssemblyId"].ToString());
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[1].Rows[i]["SubAssmId"].ToString());
                        assmModel.AssemblyCode = Convert.ToString(ds.Tables[1].Rows[i]["SubAssmCode"].ToString());
                        assmModel.AssemblyName = Convert.ToString(ds.Tables[1].Rows[i]["Name"].ToString());
                        assmModel.AssmTechName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"].ToString());
                        assmModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"].ToString());
                        assmList.Add(assmModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        SubAssemblyListModel subAssmModel = new SubAssemblyListModel();
                        subAssmModel.AssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["SubAssmId"].ToString());
                        subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProductId"].ToString());
                        subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[2].Rows[i]["ProductName"].ToString());
                        subAssmModel.CategoryId = Convert.ToInt32(ds.Tables[2].Rows[i]["CategoryId"].ToString());
                        subAssmModel.Quantity = Convert.ToInt32(ds.Tables[2].Rows[i]["Quantity"].ToString());
                        subAssmModel.CatalogueNo = Convert.ToString(ds.Tables[2].Rows[i]["CatalogueNo"].ToString());
                        subAssmModel.Size = Convert.ToString(ds.Tables[2].Rows[i]["Size"].ToString());
                        subAssmModel.UnitGrWt = Convert.ToDecimal(ds.Tables[2].Rows[i]["UnitGrWt"].ToString());
                        subAssmModel.UnitNetWt = Convert.ToDecimal(ds.Tables[2].Rows[i]["UnitNetWt"].ToString());
                        subAssmList.Add(subAssmModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.SubAssemblyId = Convert.ToInt32(ds.Tables[3].Rows[i]["ProductId"].ToString());
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[3].Rows[i]["SubProductId"].ToString());
                        compModel.ComponentName = Convert.ToString(ds.Tables[3].Rows[i]["ComponentName"].ToString());
                        compModel.Qty = Convert.ToInt32(ds.Tables[3].Rows[i]["Quantity"].ToString());
                        compModel.CatalogueNo = Convert.ToString(ds.Tables[3].Rows[i]["CatalogueNo"].ToString());
                        compModel.Size = Convert.ToString(ds.Tables[3].Rows[i]["Size"].ToString());
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[3].Rows[i]["UnitGrWt"].ToString());
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[3].Rows[i]["UnitNetWt"].ToString());
                        compList.Add(compModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[4] != null && ds.Tables[4].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                    {
                        BomMasterModel bomMasterModel = new BomMasterModel();
                        bomMasterModel.ProductId = Convert.ToInt32(ds.Tables[4].Rows[i]["ProductId"].ToString());
                        bomMasterModel.Type = Convert.ToInt32(ds.Tables[3].Rows[i]["Type"].ToString());
                        bomMasterModel.BomId = Convert.ToInt32(ds.Tables[3].Rows[i]["BomId"].ToString());
                        bomMasterList.Add(bomMasterModel);
                    }
                }
                bomModel.ProductGroupList = groupList;
                bomModel.AssemblyList = assmList;
                bomModel.SubAssemblyList = subAssmList;
                bomModel.ComponentList = compList;
                bomModel.BomMasterList = bomMasterList;
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


        public int InsertData_BomMaster(int prodId, int type, int bomId, int revId, string tNumber, SqlConnection con, SqlTransaction Sqltrans)
        {
            //connection = new SqlConnection(connectionString);
            // connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;

            try
            {
                cmd = new SqlCommand("sp_InsertData_BomMaster", con);
                cmd.Parameters.Add(new SqlParameter("@prodId", prodId));
                cmd.Parameters.Add(new SqlParameter("@type", type));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revId));
                cmd.Parameters.Add(new SqlParameter("@tNumber", tNumber));
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = Sqltrans;
                  //con.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                //  connection.Close();

                //sqlTrans.Commit();
                //status = 1;

                return status;

            }
            catch (Exception ex)
            {

                return status;
            }
            finally
            {
                //if (connection.State == ConnectionState.Open)
                //    connection.Close();
            }
        }

        public CreateBOMModel GetBomData_Revision(int enqId)
        {
            CreateBOMModel bomModel = new CreateBOMModel();
            return bomModel;
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
                //StringBuilder emailBody = new StringBuilder();
                //emailBody.Append("<p>Hello Sir/Madam,</p>");
                //emailBody.Append("<br/>");
                //emailBody.Append("<p>A new BOM is published by the design Team. Following are the details:</p>");
                //emailBody.Append("<br/>");
                //emailBody.Append(string.Format("ProjectName: {0}", projName));
                //emailBody.Append("<br/>");
                //emailBody.Append(string.Format("EPC Customer Name: {0}", custName));
                //emailBody.Append("<br/>");
                //emailBody.Append(string.Format("Tender File Name: {0}", tndFileNo));
                //emailBody.Append("<br/>");
                //emailBody.Append(string.Format("Revision No: {0}", revNo.ToString()));
                //emailBody.Append("<br/><br/>");
                //emailBody.Append("<br/>");
                //emailBody.Append("<br/>");
                //emailBody.Append("<br/>");
                //emailBody.Append("<br/><br/>");
                //emailBody.Append("<p>Regards,<p>");
                //emailBody.Append("<br/>");
                //emailBody.Append("Karamtara");

                body.Add(emailBody);
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
    }
}