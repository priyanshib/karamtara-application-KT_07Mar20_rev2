using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.DAL.Tender
{
    public class IntTenderDetailsDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public IntMarkupModel GetMarkupPricingList(int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MarkupDetails> list = new List<MarkupDetails>();
            IntMarkupModel model = new IntMarkupModel();
            model.MarkupDetails = new List<MarkupDetails>();
            model.TenderDetails = new List<IntTenderDetails>();
            try
            {
                cmd = new SqlCommand("sp_GetIntMarkupDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tenderId", tenderId);
                cmd.Parameters.AddWithValue("@tenderRevNo", tenderRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MarkupDetails markup = new MarkupDetails();
                        markup.MarkupId = Convert.ToInt32(ds.Tables[0].Rows[i]["MarkupId"]);
                        markup.Markup = ds.Tables[0].Rows[i]["Markup"].ToString();
                        markup.IndiaVal = ds.Tables[0].Rows[i]["India"].ToString();
                        markup.ItalyVal = ds.Tables[0].Rows[i]["Italy"].ToString();
                        markup.BOVal = ds.Tables[0].Rows[i]["BO"].ToString();
                        model.MarkupDetails.Add(markup);
                    }
                }

                if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    model.PBGValue = Convert.ToDecimal(ds.Tables[1].Rows[0]["BGCost"]);
                    model.AdvBGpercent = Convert.ToDecimal(ds.Tables[1].Rows[0]["AdvBGpercent"]);
                    model.TndType = Convert.ToInt32(ds.Tables[1].Rows[0]["TenderType"]);
                }

                if (ds.Tables.Count > 2 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        IntTenderDetails intTender = new IntTenderDetails();
                        intTender.Value = Convert.ToDecimal(ds.Tables[2].Rows[i]["Value"]);
                        intTender.Description = ds.Tables[2].Rows[i]["Description"].ToString();
                        model.TenderDetails.Add(intTender);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return model;
        }

        public List<IntMarkupModel> GetMarkupPricingListForAllRevisions(int tenderId, List<int> Revisions)
        {
            List<IntMarkupModel> markupList = new List<IntMarkupModel>();
            try
            {
                foreach (var revisionId in Revisions)
                {
                    IntMarkupModel model = new IntMarkupModel();
                    model.MarkupDetails = new List<MarkupDetails>();
                    model.TenderDetails = new List<IntTenderDetails>();
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    cmd = new SqlCommand("sp_GetIntMarkupDetails", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tenderId", tenderId);
                    cmd.Parameters.AddWithValue("@tenderRevNo", revisionId);
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    adapter.Fill(ds);
                    connection.Close();

                    if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            MarkupDetails markup = new MarkupDetails();
                            markup.MarkupId = Convert.ToInt32(ds.Tables[0].Rows[i]["MarkupId"]);
                            markup.Markup = ds.Tables[0].Rows[i]["Markup"].ToString();
                            markup.IndiaVal = ds.Tables[0].Rows[i]["India"].ToString();
                            markup.ItalyVal = ds.Tables[0].Rows[i]["Italy"].ToString();
                            markup.BOVal = ds.Tables[0].Rows[i]["BO"].ToString();
                            model.MarkupDetails.Add(markup);
                        }
                    }

                    if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                    {
                        model.PBGValue = Convert.ToDecimal(ds.Tables[1].Rows[0]["BGCost"]);
                        model.AdvBGpercent = Convert.ToDecimal(ds.Tables[1].Rows[0]["AdvBGpercent"]);
                        model.TndType = Convert.ToInt32(ds.Tables[1].Rows[0]["TenderType"]);
                    }

                    if (ds.Tables.Count > 2 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                        {
                            IntTenderDetails intTender = new IntTenderDetails();
                            intTender.Value = Convert.ToDecimal(ds.Tables[2].Rows[i]["Value"]);
                            intTender.Description = ds.Tables[2].Rows[i]["Description"].ToString();
                            model.TenderDetails.Add(intTender);
                        }
                    }

                    var currencyData = GetCurrencyList(tenderId, revisionId);
                    model.Currency = currencyData;
                    model.TndRevNo = revisionId;
                    markupList.Add(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return markupList;
        }

        public bool SaveMarkupPricing(FormCollection form)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                dt = getMarkupDt(form);
                cmd = new SqlCommand("sp_SaveIntMarkupPricing", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@markup", dt);
                cmd.Parameters.AddWithValue("@intRate", form["IntRate"]);
                cmd.Parameters.AddWithValue("@packingPercentage", form["PackingPercentage"]);
                cmd.Parameters.AddWithValue("@financingDays", form["FinancingDays"]);
                cmd.Parameters.AddWithValue("@finSalesCrDays", form["FinSalesCrDays"]);
                cmd.Parameters.AddWithValue("@interestSaveDays", form["InterestSaveDays"]);
                cmd.Parameters.AddWithValue("@interestSaveMnths", form["InterestSaveMnths"]);
                cmd.Parameters.AddWithValue("@noOfPersons", form["NoOfPersons"]);
                cmd.Parameters.AddWithValue("@noOfDays", form["NoOfDays"]);
                cmd.Parameters.AddWithValue("@fare", form["Fare"]);
                cmd.Parameters.AddWithValue("@lodging", form["Lodging"]);
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
        public int CreateTender(int enqId, int bomId, int revNo, int type, int userId)
        {
            int id = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_CreateIntTender", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EnquiryId", enqId);
                cmd.Parameters.AddWithValue("@BomId", bomId);
                cmd.Parameters.AddWithValue("@RevNo", revNo);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                id = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return id;
        }

        public CurrencyDetailModel GetCurrencyList(int tenderId, int tenderRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            CurrencyDetailModel model = new CurrencyDetailModel();
            model.intTenderDetails = new List<IntTenderDetails>();
            model.List = new List<CurrencyModel>();
            model.CurrencyList = new List<CurrencyMaster>();
            try
            {
                cmd = new SqlCommand("sp_GetTndCurrencyDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tenderId);
                cmd.Parameters.AddWithValue("@tndRevNo", tenderRevNo);

                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    CurrencyModel cmModel = new CurrencyModel();
                    cmModel.Id = Convert.ToInt32(dr["Id"]);
                    cmModel.Name = dr["Name"].ToString();
                    cmModel.Value = Convert.ToDecimal(dr["Value"]);
                    cmModel.DisplayInView = Convert.ToBoolean(dr["DisplayInView"]);
                    model.List.Add(cmModel);
                }

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    IntTenderDetails intTenderDetails = new IntTenderDetails();
                    intTenderDetails.Description = dr["Description"].ToString();
                    intTenderDetails.Value = Convert.ToDecimal(dr["Value"]);
                    model.intTenderDetails.Add(intTenderDetails);
                }

                foreach (DataRow dr in ds.Tables[2].Rows)
                {
                    CurrencyMaster cMaster = new CurrencyMaster();
                    cMaster.Id = Convert.ToInt32(dr["Id"].ToString());
                    cMaster.Name = dr["Name"].ToString();
                    model.CurrencyList.Add(cMaster);
                }
                model.CurrencyId= Convert.ToInt32(ds.Tables[3].Rows[0]["Value"].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return model;
        }

        public bool SaveCurrency(int tenderId, int tenderRevNo, string currencies, decimal conversionRate, int BackCurrId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveTndCurrencyDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@currencies", currencies);
                cmd.Parameters.AddWithValue("@tndId", tenderId);
                cmd.Parameters.AddWithValue("@tndRevNo", tenderRevNo);
                cmd.Parameters.AddWithValue("@conversionRate", conversionRate);
                cmd.Parameters.AddWithValue("@BackCurrId", BackCurrId);
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

        public bool SaveTextDetails(int tenderId, int tenderRevNo, string message)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveTenderTextDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@tndId", tenderId);
                cmd.Parameters.AddWithValue("@tndRevNo", tenderRevNo);
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public IntFreightModel GetFreightChargesList(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            IntFreightModel intFreightModel = new IntFreightModel();
            intFreightModel.TenderPortDetails = new List<TenderPortDetails>();
            intFreightModel.TenderPortNames = new List<TenderPortNames>();
            intFreightModel.IntTndValues = new List<IntTenderDetails>();
            intFreightModel.TndRevNo = tndRevNo;
            try
            {
                cmd = new SqlCommand("sp_GetIntFreightDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TenderPortNames portNames = new TenderPortNames();
                        portNames.CurrencyName = ds.Tables[0].Rows[i]["CurrencyName"].ToString();
                        portNames.PortName = ds.Tables[0].Rows[i]["PortName"].ToString();
                        portNames.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        portNames.IsActive = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsActive"]);
                        portNames.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["PortType"]);
                        portNames.CurrencyValue = Convert.ToDecimal(ds.Tables[0].Rows[i]["CurrencyValue"]);
                        portNames.SeaFreight = Convert.ToDecimal(ds.Tables[0].Rows[i]["SeaFreight"]);
                        portNames.SeaFreightFortyFT = Convert.ToDecimal(ds.Tables[0].Rows[i]["SeaFreightFortyFT"]);
                        portNames.PackingPercentage = Convert.ToDecimal(ds.Tables[0].Rows[i]["PackingPercentage"]);
                        intFreightModel.TenderPortNames.Add(portNames);
                    }
                }
                if (ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        TenderPortDetails portDetails = new TenderPortDetails();
                        portDetails.Remarks = ds.Tables[1].Rows[i]["Remarks"].ToString();
                        portDetails.Description = ds.Tables[1].Rows[i]["Description"].ToString();
                        portDetails.PortId = Convert.ToInt32(ds.Tables[1].Rows[i]["PortId"]);
                        portDetails.Data = Convert.ToDecimal(ds.Tables[1].Rows[i]["Data"]);
                        portDetails.Cost = Convert.ToDecimal(ds.Tables[1].Rows[i]["Cost"]);
                        intFreightModel.TenderPortDetails.Add(portDetails);
                    }
                }

                if (ds.Tables.Count > 2 && ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        IntTenderDetails tenderDetails = new IntTenderDetails();
                        tenderDetails.Description = ds.Tables[2].Rows[i]["Description"].ToString();
                        tenderDetails.Value = Convert.ToDecimal(ds.Tables[2].Rows[i]["Value"]);
                        intFreightModel.IntTndValues.Add(tenderDetails);
                    }
                }
            }
            catch (Exception)
            {

            }
            return intFreightModel;
        }

        public bool SaveFreightCharges(FormCollection form)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            List<int> ports = form["PortId"].Split(',').Select(s => Convert.ToInt32(s)).ToList();
            List<decimal> seaFreight = form["SeaFreight"].Split(',').Select(s => Convert.ToDecimal(s)).ToList();
            List<decimal> seaFreightFortyFT = form["SeaFreightFortyFT"].Split(',').Select(s => Convert.ToDecimal(s)).ToList();
            bool status = true;
            int j = 0;
            try
            {
                connection.Open();
                using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        foreach (var key in form.AllKeys)
                        {
                            if (key != "PortId" && key != "tndId" && key != "tndRevId" && key != "SeaFreight" && key != "SeaFreightFortyFT" && key != "PackingPercentage")
                            {
                                var value = form[key];
                                List<dynamic> dynamic = value.Split(',').Select(s => (dynamic)s).ToList();
                                for (int i = 0; i < dynamic.Count; i++)
                                {
                                    cmd = new SqlCommand("sp_SaveIntFreightDetails", connection);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Transaction = trans;
                                    cmd.Parameters.AddWithValue("@tndId", Convert.ToInt32(form["tndId"]));
                                    cmd.Parameters.AddWithValue("@tndRevId", Convert.ToInt32(form["tndRevId"]));
                                    cmd.Parameters.AddWithValue("@portId", ports[j]);
                                    cmd.Parameters.AddWithValue("@desc", key);
                                    cmd.Parameters.AddWithValue("@data", dynamic[i]);
                                    cmd.Parameters.AddWithValue("@cost", dynamic[i + 1]);
                                    cmd.Parameters.AddWithValue("@remarks", dynamic[i + 2]);
                                    int val = Convert.ToInt32(cmd.ExecuteScalar());

                                    i += 2;
                                    j += 1;
                                }
                            }
                            j = 0;
                        }

                        for (int i = 0; i < seaFreight.Count; i++)
                        {
                            cmd = new SqlCommand("sp_SavePortSeaFreight", connection);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Transaction = trans;
                            cmd.Parameters.AddWithValue("@tndId", Convert.ToInt32(form["tndId"]));
                            cmd.Parameters.AddWithValue("@tndRevId", Convert.ToInt32(form["tndRevId"]));
                            cmd.Parameters.AddWithValue("@portId", ports[i]);
                            cmd.Parameters.AddWithValue("@seaFreight", seaFreight[i]);
                            cmd.Parameters.AddWithValue("@seaFreightFortyFT", seaFreightFortyFT[i]);
                            cmd.Parameters.AddWithValue("@PackingPercentage", form["PackingPercentage"]);
                            int val = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        trans.Commit();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        trans.Rollback();
                        status = false;
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
                status = false;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return status;
        }

        public DataTable getMarkupDt(FormCollection form)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[6] {
                    new DataColumn("TenderId", typeof(int)),
                    new DataColumn("TenderRevNo",typeof(int)),
                    new DataColumn("MarkupId", typeof(int)),
                    new DataColumn("Italy", typeof(string)),
                    new DataColumn("India", typeof(string)),
                    new DataColumn("BO", typeof(string))
            });

            foreach (var key in form.AllKeys)
            {
                var value = form[key];
                List<string> dynamic = value.Split(',').ToList();
                if (dynamic.Count > 3)
                {
                    dt.Rows.Add(Convert.ToInt32(form["tndId"]), Convert.ToInt32(form["tndRevId"]), Convert.ToInt32(dynamic[0]), Convert.ToString(dynamic[1]),
                    Convert.ToString(dynamic[2]), Convert.ToString(dynamic[3]));
                }
            }

            return dt;
        }

        public int SavePortDetails(FormCollection form, int userId)
        {
            var domPortName = form["DomPort"] ?? string.Empty;
            var domCurrency = form["ddlDomCurr"] ?? string.Empty;
            var intPortName = form["IntPort"] ?? string.Empty;
            var intCurrency = form["CurrencyId"] ?? string.Empty;
            var portIds = form["PortId"] ?? string.Empty;
            var enable = form["domEnable"];
            var intenable = form["intEnable"];

            List<string> intPortNameList = new List<string>(intPortName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            List<string> intCurrList = new List<string>(intCurrency.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            List<string> portIdList = new List<string>(portIds.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            List<string> internationalDel = new List<string>(intenable.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            var tndId = form["tndId"];
            var tndRevNo = form["tndRevNo"];
            int status = 0;
            List<AssignPortModel> portList = new List<AssignPortModel>();

            if (!string.IsNullOrEmpty(domPortName))
            {
                AssignPortModel portDom = new AssignPortModel();
                portDom.PortType = 1;
                portDom.PortName = domPortName.ToString();
                portDom.CurrencyId = Convert.ToInt32(domCurrency);
                portDom.PortId = portIdList.ElementAtOrDefault(0) != null ? Convert.ToInt32(portIdList[0]) : 0;
                portDom.IsEnabled = enable == "1" ? true : false;
                portList.Add(portDom);
            }
            portIdList.RemoveAt(0);

            for (int i = 0; i < intPortNameList.Count; i++)
            {
                if (!string.IsNullOrEmpty(intPortNameList[i]))
                {
                    AssignPortModel port = new AssignPortModel();
                    port.PortType = 2;
                    port.PortName = intPortNameList[i].ToString();
                    port.CurrencyId = intCurrList.ElementAtOrDefault(i) != null ? Convert.ToInt32(intCurrList[i]) : 0;
                    port.PortId = portIdList.ElementAtOrDefault(i) != null ? Convert.ToInt32(portIdList[i]) : 0;
                    port.IsEnabled = internationalDel.ElementAtOrDefault(i) != null && internationalDel[i] == "1" ? true : false;
                    portList.Add(port);
                }
            }
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                foreach (var item in portList)
                {
                    cmd = new SqlCommand("sp_SavePortDetails", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@portId", item.PortId);
                    cmd.Parameters.AddWithValue("@tndId", tndId);
                    cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                    cmd.Parameters.AddWithValue("@portName", item.PortName);
                    cmd.Parameters.AddWithValue("@currencyId", item.CurrencyId);
                    cmd.Parameters.AddWithValue("@portType", item.PortType);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@isEnabled", item.IsEnabled);
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception)
            {
                return status;
            }
        }

        public AssignPortModel LoadPortData(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            AssignPortModel assignModel = new AssignPortModel();
            List<AssignPortModel> portList = new List<AssignPortModel>();
            List<CurrencyModel> currencyList = new List<CurrencyModel>();
            try
            {
                cmd = new SqlCommand("sp_GetTndPortDetails", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);

                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

            }
            catch (Exception)
            {

            }
            return assignModel;

        }

        public bool SaveTenderQty(int tndId, int tndRevId, string qtyDetails)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveIntTndQuantity", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@qtyDetails", qtyDetails);
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevId);
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

        public int CreateTenderRevision(int tndId, int tndRevId, int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_CreateIntTenderRevision", connection);
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
                cmd = new SqlCommand("sp_CancelIntTenderRevision", connection);
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

        public MarkupModel CheckPublish(int tndId, int tndRevId)
        {
            MarkupModel model = new MarkupModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_IsIntTndPublishPossible", connection);
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public bool SaveContainerValues(int tndId, int tndRevNo, decimal dividingFactor20Ft, decimal dividingFactor40Ft, int considered20FtCntr, int considered40FtCntr,
            int dollarsPerCnt40Ft, int dollarsPerCnt20Ft)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            try
            {
                cmd = new SqlCommand("sp_SaveDividingFactor", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@dividingFactor20Ft", dividingFactor20Ft);
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                cmd.Parameters.AddWithValue("@dividingFactor40Ft", dividingFactor40Ft);
                cmd.Parameters.AddWithValue("@considered20FtCntr", considered20FtCntr);
                cmd.Parameters.AddWithValue("@considered40FtCntr", considered40FtCntr);
                cmd.Parameters.AddWithValue("@dollarsPerCnt40Ft", dollarsPerCnt40Ft);
                cmd.Parameters.AddWithValue("@dollarsPerCnt20Ft", dollarsPerCnt20Ft);
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

        public bool SaveTenderValue(int tndId, int tndRevNo, string key, decimal value)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd = new SqlCommand("sp_SaveIntTenderValue", connection);
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
