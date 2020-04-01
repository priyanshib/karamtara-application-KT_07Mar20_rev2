using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class ProductGroupDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public ProductGroupModel GetProductGroupData()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ProductGroupModel prodGroupModel = new ProductGroupModel();
            List<ProductGroupTypeMS> groupTypeList = new List<ProductGroupTypeMS>();
            List<LineVoltageMS> lineVoltageList = new List<LineVoltageMS>();
            List<UTSMS> utsList = new List<UTSMS>();
            List<BundleTypeMS> bundleList = new List<BundleTypeMS>();
            List<ProductGroupListModel> groupDetailsList = new List<ProductGroupListModel>();
            List<ConductorTypeMaster> conductorTypeList = new List<ConductorTypeMaster>();
            try
            {
                cmd = new SqlCommand("sp_GetProductGroupData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ProductGroupTypeMS groupTypeModel = new ProductGroupTypeMS();

                        groupTypeModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        groupTypeModel.ProductGroupType = Convert.ToString(ds.Tables[0].Rows[i]["GroupType"]);
                        groupTypeList.Add(groupTypeModel);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        LineVoltageMS lineVoltageModel = new LineVoltageMS();

                        lineVoltageModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        lineVoltageModel.LineVoltage = Convert.ToString(ds.Tables[1].Rows[i]["LineVoltage"]);
                        lineVoltageList.Add(lineVoltageModel);
                    }
                }
                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        UTSMS utsModel = new UTSMS();

                        utsModel.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        utsModel.UTSValue = Convert.ToString(ds.Tables[2].Rows[i]["UTSValue"]);
                        utsList.Add(utsModel);
                    }
                }
                if (ds.Tables[3] != null)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        BundleTypeMS bundleTypeModel = new BundleTypeMS();

                        bundleTypeModel.Id = Convert.ToInt32(ds.Tables[3].Rows[i]["Id"]);
                        bundleTypeModel.BundleType = Convert.ToString(ds.Tables[3].Rows[i]["BundleType"]);
                        bundleList.Add(bundleTypeModel);
                    }
                }
                if (ds.Tables[4] != null)
                {
                    for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                    {
                        ProductGroupListModel groupList = new ProductGroupListModel();
                        groupList.ProductGroupId= Convert.ToInt32(ds.Tables[4].Rows[i]["Id"]);
                        groupList.ProductGroupCode= Convert.ToString(ds.Tables[4].Rows[i]["Code"]);
                        groupList.ProductGroupName = Convert.ToString(ds.Tables[4].Rows[i]["Name"]);
                        groupList.LastUpdateDate = Convert.ToDateTime(ds.Tables[4].Rows[i]["LastUpdated"]);
                        groupList.Summary = Convert.ToString(ds.Tables[4].Rows[i]["Summary"]);
                        groupDetailsList.Add(groupList);
                    }
                }
                if (ds.Tables[5] != null)
                {
                    for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                    {
                        ConductorTypeMaster conductorModel = new ConductorTypeMaster();
                        conductorModel.Id = Convert.ToInt32(ds.Tables[5].Rows[i]["Id"]);
                        conductorModel.ConductorType = Convert.ToString(ds.Tables[5].Rows[i]["ConductorType"]);
                        conductorTypeList.Add(conductorModel);
                    }
                }

                prodGroupModel.ProductGroupTypeList = groupTypeList;
                prodGroupModel.LineVoltageList = lineVoltageList;
                prodGroupModel.UtsValueList = utsList;
                prodGroupModel.BundleTypeList = bundleList;
                prodGroupModel.ProductGroupList = groupDetailsList;
                prodGroupModel.ConductorTypeList = conductorTypeList;

                return prodGroupModel;
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

        public int CreateProductGroup(FormCollection form)
        {
            int insertCount = 0;
            try
            {
                string pattern = ",";
                var groupName = form["ProdGroupName"];
                var groupCode = form["ProdGroupCode"];
                var groupTypeId = form["ProductGroupTypeId"];
                var lineVoltageId = form["LineVoltageId"];
                var utsValueId = form["UtsValueId"];
                var bundleTypeId = form["BundleTypeId"];
                var groupSummary = form["Summary"];
                var drawingNo = form["DrawingNo"] ?? string.Empty;
                var conductorId = form["ConductorTypeId"];
                
                var assmIdList = form["AssemId"] ?? string.Empty;
                var assmCodeList = form["AssemblyCode"] ?? string.Empty;
                var assmNameList = form["AssemblyName"] ?? string.Empty;
                var assmTechNameList = form["TechnicalName"] ?? string.Empty;
                var quantity = form["Quantity"] ?? string.Empty;
                var typeString= form["ObjectType"] ?? string.Empty;


                var assmIds = assmIdList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var assmCodes = assmCodeList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var assmNames = assmNameList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var assmTechs = assmTechNameList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var pgQuantities =  quantity.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var typeList = typeString.Split(new string[] { pattern }, StringSplitOptions.None).ToList();

                connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("sp_CreateProductGroup", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@GroupName", groupName));
                cmd.Parameters.Add(new SqlParameter("@Code", groupCode));
                cmd.Parameters.Add(new SqlParameter("@Type", Convert.ToInt32(groupTypeId)));
                cmd.Parameters.Add(new SqlParameter("@LineVoltage", Convert.ToInt32(lineVoltageId)));
                cmd.Parameters.Add(new SqlParameter("@UTS", Convert.ToInt32(1)));
                cmd.Parameters.Add(new SqlParameter("@BundleType", Convert.ToInt32(bundleTypeId)));
                cmd.Parameters.Add(new SqlParameter("@Summary", groupSummary));
                cmd.Parameters.Add(new SqlParameter("@DrawingNo", drawingNo));
                cmd.Parameters.Add(new SqlParameter("@ConductorType",Convert.ToString(conductorId)));
                connection.Open();
                using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        cmd.Transaction = trans;
                        int groupId = Convert.ToInt32(cmd.ExecuteScalar());
                        if (groupId < 0)
                            throw new Exception();
;
                        for (int i = 0; i < assmIds.Count; i++)
                        {
                            if(!string.IsNullOrEmpty(assmIds[i]))
                            {
                                cmd = new SqlCommand("sp_AddProductGroupRelation", connection);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@GroupId", groupId));
                                cmd.Parameters.Add(new SqlParameter("@ObjectId", assmIds.ElementAtOrDefault(i) != null ? Convert.ToInt32(assmIds[i]) : 0));
                                cmd.Parameters.Add(new SqlParameter("@Qty", pgQuantities.ElementAtOrDefault(i) != null ? (string.IsNullOrEmpty(pgQuantities[i]) ? 1 : (Convert.ToInt32(pgQuantities[i]) > 1 ? Convert.ToInt32(pgQuantities[i]) : 1)) : 1));
                                cmd.Parameters.Add(new SqlParameter("@Type", typeList.ElementAtOrDefault(i) != null ? Convert.ToInt32(typeList[i]) : 0));
                                cmd.Transaction = trans;
                                insertCount = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        trans.Commit();
                        connection.Close();
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return insertCount;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return insertCount;

        }

        public ProductGroupModel GetProductGroupHierarchyById(int groupId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ProductGroupModel model = new ProductGroupModel();
            model.MasterList = new List<MasterModel>();
            //model.AssemblyList = new List<AssemblyMasterModel>();
            //model.SubAssemblyList = new List<SubAssemblyListModel>();
            //model.ComponentList = new List<ComponentModel>();

            try
            {
                cmd = new SqlCommand("sp_GetProductGroupHierarchyById", connection);
                cmd.Parameters.Add(new SqlParameter("@GroupId", groupId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    model.ProductGroupId = groupId;
                    model.ProductGroupName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
                    model.ProductGroupCode= Convert.ToString(ds.Tables[0].Rows[0]["Code"]);
                    model.GroupType = Convert.ToString(ds.Tables[0].Rows[0]["Type"]);
                    model.LineVoltage = Convert.ToString(ds.Tables[0].Rows[0]["LineVoltage"]);
                    model.UTS = Convert.ToString(ds.Tables[0].Rows[0]["UTSValue"]);
                    model.BundleType = Convert.ToString(ds.Tables[0].Rows[0]["BundleType"]);
                    model.Summary = Convert.ToString(ds.Tables[0].Rows[0]["Summary"]);
                    model.DrawingNo = Convert.ToString(ds.Tables[0].Rows[0]["DrawingNo"]);
                    model.Conductor = Convert.ToString(ds.Tables[0].Rows[0]["ConductorType"]);
                    model.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalGrWt"]);
                    model.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalNetWt"]);
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        MasterModel masterModel = new MasterModel();
                        masterModel.SrNo = Convert.ToString(ds.Tables[1].Rows[i]["SrNo"]);
                        masterModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        masterModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"]);
                        masterModel.Name = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        masterModel.Quantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        masterModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        masterModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        masterModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        masterModel.ParentId = Convert.ToInt32(ds.Tables[1].Rows[i]["ParentId"]);
                        masterModel.IsRelated = Convert.ToBoolean(ds.Tables[1].Rows[i]["Related"]);
                        masterModel.MasterType = Convert.ToInt32(ds.Tables[1].Rows[i]["Type"]);
                        masterModel.Material = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                        masterModel.Code = Convert.ToString(ds.Tables[1].Rows[i]["Code"]);
                        masterModel.TechnicalName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"]);
                        masterModel.Grade = Convert.ToString(ds.Tables[1].Rows[i]["Grade"]);
                        masterModel.DrawingNo = Convert.ToString(ds.Tables[1].Rows[i]["DrawingNo"]);
                        masterModel.TotalUnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalGrWt"]);
                        masterModel.TotalUnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalNetWt"]);
                        model.MasterList.Add(masterModel);
                    }
                }

                return model;
            }
            catch(Exception ex)
            {
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

        public List<MasterModel> GetAssembliesAutoComplete(string prefix) 
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            var masterList = new List<MasterModel>();

            try
            {
                cmd = new SqlCommand("sp_GetSubAssmAndCompBySearch", connection);
                cmd.Parameters.Add(new SqlParameter("@searchText", prefix));
                cmd.Parameters.Add(new SqlParameter("@Type", 3));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel masterModel = new MasterModel();
                        masterModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        masterModel.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        masterModel.MasterType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        masterModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        masterModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        masterModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        masterModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        masterModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        masterModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        masterModel.TechnicalName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        masterList.Add(masterModel);
                    }
                }

                return masterList;
            }
            catch (Exception ex)
            {
                return masterList;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public ProductGroupModel GetRelatedAssemblies(int groupId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ProductGroupModel model = new ProductGroupModel();
            model.MasterList = new List<MasterModel>();

            try
            {
                cmd = new SqlCommand("sp_GetRelatedAssemblies", connection);
                cmd.Parameters.Add(new SqlParameter("@GroupId", groupId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    model.ProductGroupId = groupId;
                    model.ProductGroupName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
                    model.ProductGroupCode = Convert.ToString(ds.Tables[0].Rows[0]["Code"]);
                    model.GroupType = Convert.ToString(ds.Tables[0].Rows[0]["Type"]);
                    model.LineVoltage = Convert.ToString(ds.Tables[0].Rows[0]["LineVoltage"]);
                    model.UTS = Convert.ToString(ds.Tables[0].Rows[0]["UTSValue"]);
                    model.BundleType = Convert.ToString(ds.Tables[0].Rows[0]["BundleType"]);
                    model.Summary = Convert.ToString(ds.Tables[0].Rows[0]["Summary"]);
                    model.DrawingNo = Convert.ToString(ds.Tables[0].Rows[0]["DrawingNo"]);
                    model.Conductor = Convert.ToString(ds.Tables[0].Rows[0]["ConductorType"]);
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        MasterModel mModel = new MasterModel();
                        mModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        mModel.Code = Convert.ToString(ds.Tables[1].Rows[i]["Code"]);
                        mModel.Name = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        mModel.TechnicalName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"]);
                        mModel.Quantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        mModel.MasterType = Convert.ToInt32(ds.Tables[1].Rows[i]["Type"]);
                        mModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"]);
                        model.MasterList.Add(mModel);
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
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

        public int EditAssembly(FormCollection form)
        {
            int insertCount = 0;
            int assmId = 0;
            try
            {
                string pattern = ",";
                assmId = Convert.ToInt32(form["AssemblyId"]);
                //var assemblyCode = form["AssemblyCode"];
                //var assmTypeId = form["AssemblyTypeId"];
                //var lineVoltageId = form["LineVoltageId"];
                //var utsValueId = form["UtsValueId"];
                //var bundleTypeId = form["BundleTypeId"];
                //var assmSummary = form["Summary"];

                var assmIdList = form["editAssmId"] ?? string.Empty;
                var assmCodeList = form["editAssmCode"] ?? string.Empty;
                var assmNameList = form["editAssmName"] ?? string.Empty;
                var assmTechNameList = form["editAssmTech"] ?? string.Empty;
                var quantityList = form["editQty"] ?? string.Empty;
                var typeList = form["ObjectType"] ?? string.Empty;

                var assmIds = assmIdList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                //var assmCodes = assmCodeList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                //var assmNames = assmNameList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                //var assmTechs = assmTechNameList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var quantities = quantityList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                var types = typeList.Split(new string[] { pattern }, StringSplitOptions.None).ToList();

                List<int> AssmList = new List<int>();
                List<int> sAList = new List<int>();
                List<int> cList = new List<int>();

                connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(string.Format("Select ProductGroupId from ProductGroupMaster where ProductGroupId = {0}", assmId), connection);
                cmd.CommandType = CommandType.Text;
                connection.Open();
                int groupId = Convert.ToInt32(cmd.ExecuteScalar());
                if (groupId > 0)
                {
                    using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                    {
                        try
                        {
                            for (int i = 0; i < assmIds.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(assmIds[i]))
                                {
                                    cmd = new SqlCommand("sp_AddProductGroupRelation", connection);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@GroupId", groupId));
                                    cmd.Parameters.Add(new SqlParameter("@ObjectId", assmIds.ElementAtOrDefault(i) != null ? Convert.ToInt32(assmIds[i]) : 0));
                                    cmd.Parameters.Add(new SqlParameter("@Qty", quantities.ElementAtOrDefault(i) != null && !string.IsNullOrEmpty(quantities[i]) ? Convert.ToInt32(quantities[i]) : 1));
                                    cmd.Parameters.Add(new SqlParameter("@Type", types.ElementAtOrDefault(i) != null ? Convert.ToInt32(types[i]) : 0));
                                    cmd.Transaction = trans;
                                    insertCount = Convert.ToInt32(cmd.ExecuteScalar());

                                    if (Convert.ToInt32(types[i]) == 2)
                                        AssmList.Add(Convert.ToInt32(assmIds[i]));
                                    else if (Convert.ToInt32(types[i]) == 3)
                                        sAList.Add(Convert.ToInt32(assmIds[i]));
                                    else if (Convert.ToInt32(types[i]) == 4)
                                        cList.Add(Convert.ToInt32(assmIds[i]));
                                }
                            }

                            cmd = new SqlCommand("sp_DeleteOldProductGroupDetails", connection);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@GroupId", groupId));
                            cmd.Parameters.Add(new SqlParameter("@AssmIds", string.Join(",", AssmList)));
                            cmd.Parameters.Add(new SqlParameter("@SubAssmIds", string.Join(",", sAList)));
                            cmd.Parameters.Add(new SqlParameter("@CompIds", string.Join(",", cList)));
                            cmd.Transaction = trans;
                            var status = Convert.ToInt32(cmd.ExecuteScalar());

                            trans.Commit();
                            connection.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return assmId;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return assmId;
        }

        public ProductGroupModel GetFilteredProductGroups(int groupTypeId, int lineVoltageId, int conductorType, int bundleTypeId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ProductGroupModel groupModel = new ProductGroupModel();
            groupModel.ProductGroupList = new List<ProductGroupListModel>();
            try
            {
                cmd = new SqlCommand("sp_GetFilteredProductGroups", connection);
                cmd.Parameters.Add(new SqlParameter("@aTId", groupTypeId));
                cmd.Parameters.Add(new SqlParameter("@lVId", lineVoltageId));
                cmd.Parameters.Add(new SqlParameter("@utsId", 1));
                cmd.Parameters.Add(new SqlParameter("@bTId", bundleTypeId));
                cmd.Parameters.Add(new SqlParameter("@ctId", conductorType));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ProductGroupListModel model = new ProductGroupListModel();

                        model.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.ProductGroupName = Convert.ToString(ds.Tables[0].Rows[i]["GroupName"]);
                        model.ProductGroupCode = Convert.ToString(ds.Tables[0].Rows[i]["GroupCode"]);
                        groupModel.ProductGroupList.Add(model);
                    }
                }
                return groupModel;
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
    }
}