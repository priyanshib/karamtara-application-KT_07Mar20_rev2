using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.DAL.Tender
{
    public class TenderDetailsDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public TenderDetailsModel GetTenderDetails(int enqId, int bomId, int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderDetailsModel tndDetailsModel = new TenderDetailsModel();
            List<ProductGroupModel> pgList = new List<ProductGroupModel>();
            List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
            List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetBomDetails_Tender", connection);
                cmd.Parameters.Add(new SqlParameter("@EnqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@BomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@RevNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        tndDetailsModel.ProjectId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProjectId"]);
                        tndDetailsModel.ProjectName = Convert.ToString(ds.Tables[0].Rows[i]["ProjectName"]);
                        tndDetailsModel.EndCustName = Convert.ToString(ds.Tables[0].Rows[i]["EndCustName"]);
                        tndDetailsModel.EPCCustName = Convert.ToString(ds.Tables[0].Rows[i]["EPCCustName"]);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ProductGroupModel pgModel = new ProductGroupModel();
                        pgModel.ProductGroupId = Convert.ToInt32(ds.Tables[1].Rows[i]["ProductGroupId"]);
                        pgModel.ProductGroupName = Convert.ToString(ds.Tables[1].Rows[i]["GroupName"]);
                        pgList.Add(pgModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["AssemblyId"]);
                        assmModel.AssemblyName = Convert.ToString(ds.Tables[2].Rows[i]["AssemblyName"]);
                        assmModel.AssmTechName = Convert.ToString(ds.Tables[2].Rows[i]["TechnicalName"]);
                        assmModel.ProductGroupId = Convert.ToInt32(ds.Tables[2].Rows[i]["ProductGroupId"]);
                        assmList.Add(assmModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        SubAssemblyListModel subAssmModel = new SubAssemblyListModel();
                        subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[3].Rows[i]["AssemblyId"]);
                        subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[3].Rows[i]["ItemName"]);
                        subAssmModel.AssemblyId = Convert.ToInt32(ds.Tables[3].Rows[i]["AssemblyId"]);
                        subAssmModel.ProductGroupId = Convert.ToInt32(ds.Tables[3].Rows[i]["ProductGroupId"]);
                        subAssmList.Add(subAssmModel);
                    }
                }
                if (ds.Tables.Count > 0 && ds.Tables[4] != null && ds.Tables[4].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[4].Rows[i]["ComponentId"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[4].Rows[i]["ComponentName"]);
                        compModel.SubAssemblyId = Convert.ToInt32(ds.Tables[4].Rows[i]["SubAssemblyId"]);
                        compModel.AssemblyId = Convert.ToInt32(ds.Tables[4].Rows[i]["AssemblyId"]);
                        compModel.ProductGroupId = Convert.ToInt32(ds.Tables[4].Rows[i]["ProductGroupId"]);
                        compList.Add(compModel);
                    }
                }
                //List<TenderBomModel> tndBomList = new List<TenderBomModel>();
                //foreach(var pg in pgList)
                //{
                //    TenderBomModel tndBomMdl = new TenderBomModel();
                //    tndBomMdl.ProductId = pg.ProductGroupId;
                //    tndBomMdl.ProductName = pg.ProductGroupName;
                //    tndBomMdl.ParentId = bomId;
                //    tndBomMdl.IsRelated = true;
                //}
                tndDetailsModel.ProductGroupList = pgList;
                tndDetailsModel.AssemblyList = assmList;
                tndDetailsModel.SubAssemblyList = subAssmList;
                tndDetailsModel.ComponentList = compList;

                return tndDetailsModel;
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

        public TenderDetailsModel GetBomProdDetails(int bomId, int revNo, int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderDetailsModel tndDetailsModel = new TenderDetailsModel();
            List<MasterModel> msProdList = new List<MasterModel>();
            List<LineStructure> lineList = new List<LineStructure>();
            List<IntTndQuantity> tndQty = new List<IntTndQuantity>();
            List<TenderLineValues> tndLineValuesList = new List<TenderLineValues>();
            tndDetailsModel.TndPortDetails = new List<TenderPortNames>();
            tndDetailsModel.CurrencyList = new List<CurrencyModel>();
            tndDetailsModel.TndPortFrtDetails = new List<TenderPortDetails>();
            tndDetailsModel.TndMarkupDetails = new List<MarkupDetails>();
            tndDetailsModel.IntTndValues = new List<IntTenderDetails>();
            tndDetailsModel.RevList = new List<TenderRevisions>();
            tndDetailsModel.BomId = bomId;
            tndDetailsModel.RevisionNo = revNo;
            tndDetailsModel.TenderId = tenderId;
            try
            {
                cmd = new SqlCommand("sp_GetTenderHierarchy", connection);
                cmd.Parameters.Add(new SqlParameter("@BomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@RevNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@tndNo", tenderId));
                cmd.Parameters.Add(new SqlParameter("@tndRevNo", tenderRevNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                cmd.CommandTimeout = 0;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel msModel = new MasterModel();
                        msModel.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]);
                        msModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        msModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        msModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        msModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        msModel.TotalUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitGrWt"]);
                        msModel.TotalUnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalUnitNetWt"]);
                        msModel.IsDirectChild = Convert.ToBoolean(ds.Tables[0].Rows[i]["DirectChild"]);
                        msModel.IsRelated = Convert.ToBoolean(ds.Tables[0].Rows[i]["Related"]);
                        msModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        msModel.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        msModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        msModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        msModel.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        msModel.UnitCost = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitCost"]);
                        msModel.PrimaryId = Convert.ToInt32(ds.Tables[0].Rows[i]["PrimaryId"]);
                        msModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]) ?? "-";
                        msModel.Unit = Convert.ToString(ds.Tables[0].Rows[i]["Unit"]) ?? string.Empty;
                        msModel.CalculatedUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["CalculatedGrWt"]);
                        msModel.TotalCalcUnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalCalculatedGrWt"]);
                        msProdList.Add(msModel);
                    }
                    tndDetailsModel.MasterList = msProdList;
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        LineStructure str = new LineStructure();
                        str.LineId = Convert.ToInt32(ds.Tables[1].Rows[i]["LineId"]);
                        str.LineName = Convert.ToString(ds.Tables[1].Rows[i]["LineName"]);
                        lineList.Add(str);
                    }
                    tndDetailsModel.LineList = lineList;
                }
                if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    tndDetailsModel.UnitCost = Convert.ToDecimal(ds.Tables[2].Rows[0]["UnitCost"]);
                }

                if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        TenderLineValues lineVals = new TenderLineValues();
                        lineVals.LineId = Convert.ToInt32(ds.Tables[3].Rows[i]["LineId"]);
                        lineVals.Description = Convert.ToString(ds.Tables[3].Rows[i]["Description"]);
                        if (ds.Tables[3].Rows[i]["Value"] != DBNull.Value)
                        {
                            lineVals.Values = Convert.ToDecimal(ds.Tables[3].Rows[i]["Value"]);
                        }
                        tndLineValuesList.Add(lineVals);
                    }
                    tndDetailsModel.TndLineValuesList = tndLineValuesList;
                }
                if (ds.Tables[4] != null && ds.Tables[4].Rows.Count > 0)
                {
                    tndDetailsModel.ProjectName = Convert.ToString(ds.Tables[4].Rows[0]["ProjectName"]);
                    tndDetailsModel.EPCCustName = Convert.ToString(ds.Tables[4].Rows[0]["CustomerName"]);
                    tndDetailsModel.EndCustName = Convert.ToString(ds.Tables[4].Rows[0]["EndCustName"]);
                    tndDetailsModel.TenderFileNo = Convert.ToString(ds.Tables[4].Rows[0]["TenderFileNo"]);
                }

                if (ds.Tables[5] != null && ds.Tables[5].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[5].Rows.Count; i++)
                    {
                        IntTndQuantity qty = new IntTndQuantity();
                        qty.PrimaryId = Convert.ToInt32(ds.Tables[5].Rows[i]["PrimaryId"]);
                        qty.Quantity = Convert.ToInt32(ds.Tables[5].Rows[i]["Quantity"]);
                        qty.BO = Convert.ToDecimal(ds.Tables[5].Rows[i]["BO"]);
                        qty.TypeId = Convert.ToInt32(ds.Tables[5].Rows[i]["Type"]);
                        tndQty.Add(qty);
                    }
                    tndDetailsModel.LineList = lineList;
                }

                if (ds.Tables.Count > 6 && ds.Tables[6] != null && ds.Tables[6].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[6].Rows.Count; i++)
                    {
                        TenderPortNames portNames = new TenderPortNames();
                        portNames.CurrencyName = ds.Tables[6].Rows[i]["CurrencyName"].ToString();
                        portNames.PortName = ds.Tables[6].Rows[i]["PortName"].ToString();
                        portNames.Id = Convert.ToInt32(ds.Tables[6].Rows[i]["Id"]);
                        portNames.IsActive = Convert.ToBoolean(ds.Tables[6].Rows[i]["IsActive"]);
                        portNames.Type = Convert.ToInt32(ds.Tables[6].Rows[i]["PortType"]);
                        portNames.CurrencyValue = Convert.ToDecimal(ds.Tables[6].Rows[i]["CurrencyValue"]);
                        portNames.SeaFreight = Convert.ToDecimal(ds.Tables[6].Rows[i]["SeaFreight"]);
                        portNames.PackingPercentage = Convert.ToDecimal(ds.Tables[6].Rows[i]["PackingPercentage"]);
                        tndDetailsModel.TndPortDetails.Add(portNames);
                    }
                }

                if (ds.Tables.Count > 7 && ds.Tables[7] != null && ds.Tables[7].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[7].Rows.Count; i++)
                    {
                        CurrencyModel model = new CurrencyModel();
                        model.Id = Convert.ToInt32(ds.Tables[7].Rows[i]["CurrencyId"]);
                        model.Name = ds.Tables[7].Rows[i]["CurrencyName"].ToString();
                        model.Value = Convert.ToDecimal(ds.Tables[7].Rows[i]["Value"]);
                        model.DisplayInView = Convert.ToBoolean(ds.Tables[7].Rows[i]["DisplayInView"]);
                        tndDetailsModel.CurrencyList.Add(model);
                    }
                }

                if (ds.Tables.Count > 8 && ds.Tables[8] != null && ds.Tables[8].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[8].Rows.Count; i++)
                    {
                        MarkupDetails markup = new MarkupDetails();
                        markup.MarkupId = Convert.ToInt32(ds.Tables[8].Rows[i]["MarkupId"]);
                        markup.IndiaVal = ds.Tables[8].Rows[i]["India"].ToString();
                        markup.ItalyVal = ds.Tables[8].Rows[i]["Italy"].ToString();
                        markup.BOVal = ds.Tables[8].Rows[i]["BO"].ToString();
                        tndDetailsModel.TndMarkupDetails.Add(markup);
                    }
                }

                if (ds.Tables.Count > 9 && ds.Tables[9] != null && ds.Tables[9].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[9].Rows.Count; i++)
                    {
                        TenderPortDetails portDetails = new TenderPortDetails();
                        portDetails.Description = ds.Tables[9].Rows[i]["Description"].ToString();
                        portDetails.PortTypeName = ds.Tables[9].Rows[i]["PortDesc"].ToString();
                        portDetails.PortId = Convert.ToInt32(ds.Tables[9].Rows[i]["PortId"]);
                        portDetails.Data = Convert.ToDecimal(ds.Tables[9].Rows[i]["Data"]);
                        portDetails.Cost = Convert.ToDecimal(ds.Tables[9].Rows[i]["Cost"]);
                        tndDetailsModel.TndPortFrtDetails.Add(portDetails);
                    }
                }

                if (ds.Tables.Count > 10 && ds.Tables[10] != null && ds.Tables[10].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[10].Rows.Count; i++)
                    {
                        IntTenderDetails tenderDetails = new IntTenderDetails();
                        tenderDetails.Description = ds.Tables[10].Rows[i]["Description"].ToString();
                        tenderDetails.Value = Convert.ToDecimal(ds.Tables[10].Rows[i]["Value"]);
                        tndDetailsModel.IntTndValues.Add(tenderDetails);
                    }
                }

                if (ds.Tables.Count > 11 && ds.Tables[11] != null && ds.Tables[11].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[11].Rows.Count; i++)
                    {
                        TenderRevisions revisions = new TenderRevisions();
                        revisions.RevisionNo = Convert.ToInt32(ds.Tables[11].Rows[i]["TenderRevNo"]);
                        revisions.TenderType = Convert.ToInt32(ds.Tables[11].Rows[i]["TenderType"]);
                        revisions.CreatedByName = Convert.ToString(ds.Tables[11].Rows[i]["CreatedByName"]);
                        revisions.PublishedDate = ds.Tables[11].Rows[i]["PublishedDate"] == DBNull.Value ? (DateTime?) null : Convert.ToDateTime(ds.Tables[11].Rows[i]["PublishedDate"]);
                        tndDetailsModel.RevList.Add(revisions);
                    }
                    tndDetailsModel.TenderType = Convert.ToInt32(ds.Tables[11].Rows[0]["TenderType"]);
                }
                
                tndDetailsModel.LineQtyList = getTenderLineQtyList(bomId, revNo, tenderId, tenderRevNo);
                tndDetailsModel.IntTndQtyList = tndQty;
                tndDetailsModel.TenderRevisionNo = tenderRevNo;
                
                var intTenderDetailsDAL = new IntTenderDetailsDAL();
                tndDetailsModel.CurrencyData = intTenderDetailsDAL.GetCurrencyList(tenderId, tenderRevNo);

                return tndDetailsModel;
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

        public List<dynamic> getTenderLineQtyList(int bomId, int revNo, int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<dynamic> list = new List<dynamic>();
            try
            {
                cmd = new SqlCommand("sp_GetTenderLineQtyList", connection);
                cmd.Parameters.Add(new SqlParameter("@BomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@RevNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@tenderId", tenderId));
                cmd.Parameters.Add(new SqlParameter("@tenderRevNo", tenderRevNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();

                foreach (DataRow row in dt.Rows)
                {
                    dynamic dynamic = new ExpandoObject();
                    dynamic.Id = Convert.ToInt32(row["Id"].ToString());
                    dynamic.ProductId = Convert.ToInt32(row["ProductId"]);
                    dynamic.Type = Convert.ToString(row["Type"]);
                    dynamic.ProductGroupId = Convert.ToInt32(row["ProductGroupId"]);
                    dynamic.AssemblyId = Convert.ToInt32(row["AssemblyId"]);
                    dynamic.SubAssemblyId = Convert.ToInt32(row["SubAssemblyId"]);
                    dynamic.ComponentId = Convert.ToInt32(row["ComponentId"]);
                    dynamic.LineId = Convert.ToInt32(row["LineId"]);
                    dynamic.Quantity = Convert.ToInt32(row["Quantity"]);
                    list.Add(dynamic);
                }

                return list;
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

        public bool SaveLineQty(string values, string grWtValues)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveTndLineQuantity", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@values", values);
                cmd.Parameters.AddWithValue("@grWtValues", grWtValues);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SaveBGData(FormCollection form, int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                DataTable dataTable = GetBGDataTable(form);

                cmd = new SqlCommand("sp_SaveTndBGDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderNo", Convert.ToInt32(form["TenderNo"]));
                cmd.Parameters.AddWithValue("@TenderRevisionNo", Convert.ToInt32(form["TenderRevisionNo"]));
                cmd.Parameters.AddWithValue("@DeliveryMonth", Convert.ToDecimal(form["deliveryMonths"]));
                cmd.Parameters.AddWithValue("@ContractValue", Convert.ToDecimal(form["contractValue"]));
                cmd.Parameters.AddWithValue("@PerformancePeriod", Convert.ToDecimal(form["performancePeriod"]));
                cmd.Parameters.AddWithValue("@GracePeriod", Convert.ToDecimal(form["gracePeriod"]));
                cmd.Parameters.AddWithValue("@BGDetails", dataTable);
                cmd.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                //SqlDataAdapter sda = new SqlDataAdapter(cmd);
                status = Convert.ToInt32(cmd.ExecuteScalar());
                //sda.Fill(dt);
                connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        public List<TenderBGModel> GetBGData(int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            TenderBGModel tenderBG = new TenderBGModel();
            List<TenderBGModel> list = new List<TenderBGModel>();

            try
            {
                cmd = new SqlCommand("sp_GetTndBGDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderNo", tenderId);
                cmd.Parameters.AddWithValue("@TenderRevisionNo", tenderRevNo);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();

                foreach (DataRow row in dt.Rows)
                {
                    tenderBG = new TenderBGModel();
                    tenderBG.ContractValue = Convert.ToDecimal(row["ContractValue"]);
                    tenderBG.DeliveryMonth = Convert.ToDecimal(row["DeliveryMonth"]);
                    tenderBG.TenderNo = Convert.ToInt32(row["TenderNo"]);
                    tenderBG.TenderRevisionNo = Convert.ToInt32(row["TenderRevisionNo"]);
                    tenderBG.BGCostPercentage = Convert.ToDecimal(row["BGCostPercentage"]);
                    tenderBG.BGPercentage = Convert.ToDecimal(row["BGPercentage"]);
                    tenderBG.CommisionPercentage = Convert.ToDecimal(row["CommisionPercentage"]);
                    tenderBG.BGMonth = Convert.ToDecimal(row["BGMonth"]);
                    tenderBG.BGType = Convert.ToInt32(row["BGType"]); ;
                    tenderBG.BGAmount = Convert.ToDecimal(row["BGAmount"]);
                    tenderBG.BGTypeString = Convert.ToString(row["BGTypeString"]);
                    tenderBG.PerformancePeriod = Convert.ToDecimal(row["PerformancePeriod"]);
                    tenderBG.GracePeriod = Convert.ToDecimal(row["GracePeriod"]);
                    list.Add(tenderBG);
                }

            }
            catch (Exception)
            {

            }
            return list;

        }

        public List<List<TenderBGModel>> GetBGDataForAllRevisions(int tenderId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            TenderBGModel tenderBG = new TenderBGModel();
            List<TenderBGModel> list = new List<TenderBGModel>();
            List<List<TenderBGModel>> bgList = new List<List<TenderBGModel>>();

            try
            {
                cmd = new SqlCommand("sp_GetTndBGDetailsForAllRevisions", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderNo", tenderId);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(ds);
                connection.Close();

                for(int i = 0; i < ds.Tables.Count; i++)
                {
                    list = new List<TenderBGModel>();
                    if (ds.Tables[i] != null && ds.Tables[i].Rows.Count > 0)
                    {
                        for (int j = 0; j < ds.Tables[i].Rows.Count; j++)
                        {
                            tenderBG = new TenderBGModel();
                            tenderBG.ContractValue = Convert.ToDecimal(ds.Tables[i].Rows[j]["ContractValue"]);
                            tenderBG.DeliveryMonth = Convert.ToDecimal(ds.Tables[i].Rows[j]["DeliveryMonth"]);
                            tenderBG.TenderNo = Convert.ToInt32(ds.Tables[i].Rows[j]["TenderNo"]);
                            tenderBG.TenderRevisionNo = Convert.ToInt32(ds.Tables[i].Rows[j]["TenderRevisionNo"]);
                            tenderBG.BGCostPercentage = Convert.ToDecimal(ds.Tables[i].Rows[j]["BGCostPercentage"]);
                            tenderBG.BGPercentage = Convert.ToDecimal(ds.Tables[i].Rows[j]["BGPercentage"]);
                            tenderBG.CommisionPercentage = Convert.ToDecimal(ds.Tables[i].Rows[j]["CommisionPercentage"]);
                            tenderBG.BGMonth = Convert.ToDecimal(ds.Tables[i].Rows[j]["BGMonth"]);
                            tenderBG.BGType = Convert.ToInt32(ds.Tables[i].Rows[j]["BGType"]); ;
                            tenderBG.BGAmount = Convert.ToDecimal(ds.Tables[i].Rows[j]["BGAmount"]);
                            tenderBG.BGTypeString = Convert.ToString(ds.Tables[i].Rows[j]["BGTypeString"]);
                            tenderBG.PerformancePeriod = Convert.ToDecimal(ds.Tables[i].Rows[j]["PerformancePeriod"]);
                            tenderBG.GracePeriod = Convert.ToDecimal(ds.Tables[i].Rows[j]["GracePeriod"]);
                            list.Add(tenderBG);
                        }
                    }
                    bgList.Add(list);
                }
            }
            catch (Exception ex)
            {

            }
            return bgList;
        }

        public DataTable GetBGDataTable(FormCollection form)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[6] {
                    new DataColumn("BGType", typeof(int)),
                    new DataColumn("BGMonth",typeof(decimal)),
                    new DataColumn("CommisionPercentage", typeof(decimal)),
                    new DataColumn("BGPercentage", typeof(decimal)),
                    new DataColumn("BGAmount", typeof(decimal)),
                    new DataColumn("BGCostPercentage", typeof(decimal))
            });

            dt.Rows.Add(1, Convert.ToDecimal(form["AdvMonth"]), Convert.ToDecimal(form["AdvPercentageComm"]), Convert.ToDecimal(form["AdvPercentageBG"]),
                Convert.ToDecimal(form["AdvBGAmt"]), Convert.ToDecimal(form["ADVTotalBGPercentage"]));
            dt.Rows.Add(2, Convert.ToDecimal(form["PfmMonth"]), Convert.ToDecimal(form["PfmPercentageComm"]), Convert.ToDecimal(form["PfmPercentageBG"]),
                Convert.ToDecimal(form["PfmBGAmt"]), Convert.ToDecimal(form["PfmTotalBGPercentage"]));
            dt.Rows.Add(3, Convert.ToDecimal(form["RetMonth"]), Convert.ToDecimal(form["RetPercentageComm"]), Convert.ToDecimal(form["RetPercentageBG"]),
                Convert.ToDecimal(form["RetBGAmt"]), Convert.ToDecimal(form["RetTotalBGPercentage"]));

            return dt;
        }

        public int SaveUnitCost(string unitCosts, int tndNo, int tndRevNo, string salesCost, string exWorks, string lineUnitCost)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_SaveTenderCosts", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndNo", tndNo);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                cmd.Parameters.AddWithValue("@desc", "UnitCost");
                cmd.Parameters.AddWithValue("@salesCost", salesCost);
                cmd.Parameters.AddWithValue("@exWorks", exWorks);
                cmd.Parameters.AddWithValue("@lineUnitCost", lineUnitCost);
                cmd.Parameters.AddWithValue("@value", Convert.ToDecimal(unitCosts));
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception)
            {
                return status;
            }

        }

        #region testrelation

        public TenderStructureModel GetTestRelationData(ParameterModel param)
        {
            TenderStructureModel strModel = new TenderStructureModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            strModel.LineList = new List<StructureDetails>();
            strModel.TestNames = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetTestLineRelationData", connection);
                cmd.Parameters.Add(new SqlParameter("@bomId", param.BomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", param.RevId));
                cmd.Parameters.Add(new SqlParameter("@prodId", param.Id));
                cmd.Parameters.Add(new SqlParameter("@typeId", param.ProdType));
                cmd.Parameters.Add(new SqlParameter("@compId", param.ComponentId));
                cmd.Parameters.Add(new SqlParameter("@subId", param.SubAssemblyId));
                cmd.Parameters.Add(new SqlParameter("@asmId", param.AssemblyId));
                cmd.Parameters.Add(new SqlParameter("@pgId", param.ProductGroupId));
                cmd.Parameters.Add(new SqlParameter("@finalId", param.PrimaryId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string test = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        strModel.TestNames.Add(test);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        StructureDetails structModel = new StructureDetails();
                        structModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        structModel.Name = Convert.ToString(ds.Tables[1].Rows[i]["StrName"]);
                        structModel.IsSelected = Convert.ToBoolean(ds.Tables[1].Rows[i]["Selected"]);
                        structModel.TestQuantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        structModel.Price = Convert.ToDecimal(ds.Tables[1].Rows[i]["Price"]);
                        if (!structModel.IsSelected)
                            structModel.TestQuantity = 0;
                        strModel.LineList.Add(structModel);
                    }
                }
                if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    strModel.ProductIdentityId = Convert.ToInt32(ds.Tables[2].Rows[0]["Id"]);
                    strModel.ProductName = Convert.ToString(ds.Tables[2].Rows[0]["ProductName"]);
                    strModel.ProdType = Convert.ToInt32(ds.Tables[2].Rows[0]["TypeId"]);
                }
                strModel.TenderId = param.TenderId;
                strModel.TenderRevisionNo = param.TenderRevisionId;
                strModel.ProductIdentityId = param.PrimaryId;
                strModel.BomId = param.BomId;
                strModel.RevisionNo = param.RevId;
                strModel.ProdType = param.ProdType;
                return strModel;
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

        public bool SaveTestLineRelation(TenderStructureModel model)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_InActiveAllOldTestLineRelation", connection);
                cmd.Parameters.AddWithValue("@prodId", model.ProductIdentityId);
                cmd.Parameters.AddWithValue("@typeId", model.ProdType);
                cmd.Parameters.AddWithValue("@tenId", model.TenderId);
                cmd.Parameters.AddWithValue("@tenRevId", model.TenderRevisionNo);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var result = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                model.LineList = model.LineList.Where(x => x.IsSelected).Select(y => y).ToList();

                if (result > 0)
                {
                    for (int i = 0; i < model.LineList.Count; i++)
                    {
                        cmd = new SqlCommand("sp_SaveTestLineRelationData", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@prodId", model.ProductIdentityId);
                        cmd.Parameters.AddWithValue("@typeId", model.ProdType);
                        cmd.Parameters.AddWithValue("@tenId", model.TenderId);
                        cmd.Parameters.AddWithValue("@tenRevId", model.TenderRevisionNo);
                        cmd.Parameters.AddWithValue("@lineId", model.LineList[i].Id);
                        cmd.Parameters.AddWithValue("@qty", model.LineList[i].TestQuantity == 0 ? 1 : model.LineList[i].TestQuantity);
                        connection.Open();
                        status += Convert.ToInt32(cmd.ExecuteScalar());
                        connection.Close();
                    }
                }

                cmd = new SqlCommand("sp_UpdateChangestoPreSavedTable", connection);
                cmd.Parameters.AddWithValue("@tenderId", model.TenderId);
                cmd.Parameters.AddWithValue("@tenderRevisionId", model.TenderRevisionNo);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var data = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        #endregion

        public TenderDetailsModel GetFinalPrices(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            TenderDetailsModel tenderDetailsModel = new TenderDetailsModel();
            tenderDetailsModel.TndLineValuesList = new List<TenderLineValues>();
            tenderDetailsModel.TenderValues = new List<TenderValues>();

            try
            {
                cmd = new SqlCommand("sp_GetFinalTenderPrices", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(ds);
                connection.Close();

                //for (int j = 0; j < ds.Tables.Count; j++)
                //{
                    DataTable dt = ds.Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            TenderLineValues tndVals = new TenderLineValues();
                            tndVals.LineId = Convert.ToInt32(dt.Rows[i]["LineId"]);
                            tndVals.Description = Convert.ToString(dt.Rows[i]["Description"]);
                            tndVals.Values = Convert.ToDecimal(dt.Rows[i]["Value"]);
                            tndVals.LineName = Convert.ToString(dt.Rows[i]["LineName"]);
                            tenderDetailsModel.TndLineValuesList.Add(tndVals);
                        }
                    }

                dt = ds.Tables[2];
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TenderValues tndVals = new TenderValues();
                        tndVals.Description = Convert.ToString(dt.Rows[i]["Description"]);
                        tndVals.Values = Convert.ToDecimal(dt.Rows[i]["Value"]);
                        tenderDetailsModel.TenderValues.Add(tndVals);
                    }
                }
                return tenderDetailsModel;

            }
            catch (Exception)
            {
                return null;
            }

        }

        public int PublishTender(int tndId, int tndRevId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_PublishTender", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevId", tndRevId);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception)
            {
                return status;
            }
        }

        public int InsertAuditTrial(int userId, int tndId, int tndRevId, string flag)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderStructureModel tndStrModel = new TenderStructureModel();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_InsertAuditTrial", connection);
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.Parameters.Add(new SqlParameter("@tndId", tndId));
                cmd.Parameters.Add(new SqlParameter("@tndRevId", tndRevId));
                cmd.Parameters.Add(new SqlParameter("@Flag", flag));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
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

        public List<TenderAuditTrial> GetAuditTrialDetails(int tndId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            List<TenderAuditTrial> auditList = new List<TenderAuditTrial>();

            try
            {
                cmd = new SqlCommand("sp_GetTenderAuditTrial", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TenderAuditTrial tndVals = new TenderAuditTrial();
                        tndVals.EditedBy = Convert.ToString(dt.Rows[i]["EditedBy"]);
                        tndVals.EditedDate = Convert.ToString(dt.Rows[i]["EditedDate"]);
                        tndVals.Version = Convert.ToString(dt.Rows[i]["Version"]);
                        auditList.Add(tndVals);
                    }
                }
                return auditList;

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

        public List<TextDetails> GetTextDetails(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            List<TextDetails> textList = new List<TextDetails>();

            try
            {
                cmd = new SqlCommand("sp_GetTenderTextDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TextDetails tndText = new TextDetails();
                        tndText.Message = Convert.ToString(dt.Rows[i]["Message"]);
                        tndText.IsActive = Convert.ToInt32(dt.Rows[i]["IsActive"]);
                        textList.Add(tndText);
                    }
                }
                return textList;

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

        public int CreateTenderRevision(int tndId, int tndRevId, int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_CreateTenderRevision", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderId", tndId);
                cmd.Parameters.AddWithValue("@TenderRevId", tndRevId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception)
            {
                return status;
            }
        }

        public bool CancelTenderRev(int tndId, int tndRevId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_CancelTenderRevision", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenderId", tndId);
                cmd.Parameters.AddWithValue("@TenderRevId", tndRevId);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<TenderStructureName> GetTenderStrDetails(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            List<TenderStructureDetails> tndStrDetList = new List<TenderStructureDetails>();
            try
            {
                cmd = new SqlCommand("GetTenderLineStructure", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();
                List<string> strList = new List<string>();
                List<TenderStructureName> TndStrNameList = new List<TenderStructureName>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TenderStructureDetails tndDet = new TenderStructureDetails();
                        tndDet.LineId = Convert.ToInt32(dt.Rows[i]["LineId"]);
                        tndDet.Structure = Convert.ToString(dt.Rows[i]["Structure"]);
                        //tndDet.PackageName = Convert.ToString(dt.Rows[i]["PackageName"]);
                        tndDet.StrType = Convert.ToInt32(dt.Rows[i]["StrType"]);
                        tndStrDetList.Add(tndDet);
                    }
                }
                if (tndStrDetList != null && tndStrDetList.Count > 0)
                {
                    List<int> lineIdList = new List<int>();
                    lineIdList = tndStrDetList.Select(x => x.LineId).Distinct().ToList();
                  

                    foreach(var line in lineIdList)
                    {
                        string structure = string.Empty;
                        foreach (var item in tndStrDetList.OrderByDescending(x=>x.StrType))
                        {
                            if(item.LineId==line)
                            {
                                string str = item.Structure;
                                if (structure.Length >0)
                                {
                                    structure = structure + " > " + str;
                                }
                                else
                                {
                                    structure = str;
                                }
                            }
                        }
                        strList.Add(structure);
                        
                        TenderStructureName strName = new TenderStructureName();
                        strName.LineId = line;
                        strName.Structure = structure;
                        TndStrNameList.Add(strName);
                        
                    }
                }


                return TndStrNameList;

            }
            catch (Exception ex)
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

        public MarkupModel CheckPublish(int tndId, int tndRevId)
        {
            MarkupModel model = new MarkupModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_IsTndPublishPossible", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevId", tndRevId);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    model.Message = dt.Rows[0]["Message"].ToString();
                    model.Flag = Convert.ToBoolean(dt.Rows[0]["Flag"].ToString());
                }

                connection.Close();
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<int> GetRevisionIds(int tenderId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            List<int> revisions = new List<int>();
            try
            {
                cmd = new SqlCommand("sp_GetRevisionIdsList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tenderId", tenderId);
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
                connection.Close();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        revisions.Add(Convert.ToInt32(dt.Rows[i]["TenderRevisionNo"]));
                    }
                }

                return revisions;
            }
            catch (Exception ex)
            {
                return revisions;
            }
        }

        public bool SaveTenderValue(int tndId, int tndRevNo,string key, decimal value)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_SaveTenderValue", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                cmd.Parameters.AddWithValue("@key", key);
                cmd.Parameters.AddWithValue("@value", value);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
