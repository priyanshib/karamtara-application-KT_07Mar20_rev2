using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class TestDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<TestModel> GetAllTests(int subAsmId = 0)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<TestModel> testList = new List<TestModel>();
            try
            {
                cmd = new SqlCommand("sp_GetAllTests", connection);
                cmd.Parameters.Add(new SqlParameter("@subAsmId", subAsmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var test = new TestModel();
                        test.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        test.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        test.TestDescription = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        test.IsSelected = Convert.ToBoolean(ds.Tables[0].Rows[i]["IsSelected"]);
                        test.Type = Convert.ToString(ds.Tables[0].Rows[i]["Type"]);
                        test.KVLine = Convert.ToString(ds.Tables[0].Rows[i]["KVLine"]);
                        test.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"]);
                        test.Bundle = Convert.ToString(ds.Tables[0].Rows[i]["Bundle"]);
                        test.Summary = Convert.ToString(ds.Tables[0].Rows[i]["Summary"]);
                        testList.Add(test);
                    }
                }
                return testList;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return testList;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public TestMasterModel GetTestMasterData()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            List<LineVoltageMS> kvLineList = new List<LineVoltageMS>();
            List<ProductGroupTypeMS> typeList = new List<ProductGroupTypeMS>();
            List<UTSMS> utsList = new List<UTSMS>();
            List<BundleTypeMS> bundleList = new List<BundleTypeMS>();
            TestMasterModel model = new TestMasterModel();
            model.TestList = new List<TestModel>();

            try
            {
                cmd = new SqlCommand("sp_GetTestMasterData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        var pgType = new ProductGroupTypeMS();
                        pgType.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        pgType.ProductGroupType = Convert.ToString(ds.Tables[0].Rows[i]["Value"]);
                        typeList.Add(pgType);
                    }
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        var uts = new UTSMS();
                        uts.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        uts.UTSValue = Convert.ToString(ds.Tables[1].Rows[i]["Value"]);
                        utsList.Add(uts);
                    }
                }

                if (ds.Tables[2] != null)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        var lineVoltage = new LineVoltageMS();
                        lineVoltage.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        lineVoltage.LineVoltage = Convert.ToString(ds.Tables[2].Rows[i]["Value"]);
                        kvLineList.Add(lineVoltage);
                    }
                }

                if (ds.Tables[3] != null)
                {
                    for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                    {
                        var bundle = new BundleTypeMS();
                        bundle.Id = Convert.ToInt32(ds.Tables[3].Rows[i]["Id"]);
                        bundle.BundleType = Convert.ToString(ds.Tables[3].Rows[i]["Value"]);
                        bundleList.Add(bundle);
                    }
                }

                if (ds.Tables[4] != null)
                {
                    for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
                    {
                        TestModel mod = new TestModel();
                        mod.Id = Convert.ToInt32(ds.Tables[4].Rows[i]["Id"]);
                        mod.TestName = Convert.ToString(ds.Tables[4].Rows[i]["Name"]);
                        mod.TestDescription = Convert.ToString(ds.Tables[4].Rows[i]["Description"]);
                        mod.Summary = Convert.ToString(ds.Tables[4].Rows[i]["Summary"]);
                        mod.Type = Convert.ToString(ds.Tables[4].Rows[i]["GroupType"]);
                        mod.UTS = Convert.ToString(ds.Tables[4].Rows[i]["UTS"]);
                        mod.Bundle = Convert.ToString(ds.Tables[4].Rows[i]["BundleType"]);
                        mod.KVLine = Convert.ToString(ds.Tables[4].Rows[i]["LineType"]);
                        model.TestList.Add(mod);
                    }
                }
                model.KVLineList = kvLineList;
                model.TypeList = typeList;
                model.BundleList = bundleList;
                model.UtsList = utsList;
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
                        masterList.Add(masterModel);
                    }
                }

                return masterList;
            }
            catch (Exception)
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

        public int SubmitTestData(int ProductId, int Type, int BOMId, string values)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int TestId = Convert.ToInt32(values);
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_getSubAssmTestSubResult", connection);
                cmd.Parameters.Add(new SqlParameter("@ProductId", ProductId));
                cmd.Parameters.Add(new SqlParameter("@Type", Type));
                cmd.Parameters.Add(new SqlParameter("@BOMId", BOMId));
                cmd.Parameters.Add(new SqlParameter("@TestId", TestId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch (Exception)
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

        public TestMasterModel GetTestMasterdata()
        {
            TestMasterModel testMaster = new TestMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_addNewTestData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        //masterModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        //masterModel.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        //masterModel.MasterType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        //masterModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        //masterModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        //masterModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        //masterModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        //masterModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        //masterModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                    }
                }
                return testMaster;
            }
            catch (Exception)
            {
                return testMaster;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int CreateTests(FormCollection form)
        {
            int status = 0;

            var name = form["TestName"] ?? string.Empty;
            var description = form["TestDescription"] ?? string.Empty;
            var summary = form["TestComments"] ?? string.Empty;
            var testType = form["TypeId"] ?? string.Empty;
            var kvLine = form["KvLineId"] ?? string.Empty;
            var uts = form["UtsId"] ?? string.Empty;
            var bundle = form["BundleId"] ?? string.Empty;

            try
            {
                SqlCommand cmd = new SqlCommand();
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand();
                cmd = new SqlCommand("sp_AddNewTest", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@TName", name));
                cmd.Parameters.Add(new SqlParameter("@TDesc", description));
                cmd.Parameters.Add(new SqlParameter("@Type", testType));
                cmd.Parameters.Add(new SqlParameter("@kv", kvLine));
                cmd.Parameters.Add(new SqlParameter("@uts", uts));
                cmd.Parameters.Add(new SqlParameter("@bundle", bundle));
                cmd.Parameters.Add(new SqlParameter("@Summary", summary));

                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception)
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
            return status;
        }

        public TestMasterModel GetTestList()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            TestMasterModel model = new TestMasterModel();
            model.TestList = new List<TestModel>();

            try
            {
                cmd = new SqlCommand("sp_GetTestList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TestModel mod = new TestModel();
                        mod.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        mod.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        mod.TestDescription = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        mod.Summary = Convert.ToString(ds.Tables[0].Rows[i]["Summary"]);
                        mod.Type = Convert.ToString(ds.Tables[0].Rows[i]["GroupType"]);
                        mod.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"]);
                        mod.Bundle = Convert.ToString(ds.Tables[0].Rows[i]["BundleType"]);
                        mod.KVLine = Convert.ToString(ds.Tables[0].Rows[i]["LineType"]);
                        model.TestList.Add(mod);
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

        public TestMasterModel GetTestPricingList(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            TestMasterModel model = new TestMasterModel();
            model.TestList = new List<TestModel>();

            try
            {
                cmd = new SqlCommand("sp_GetTestPricingList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@tndNo", tndId));
                cmd.Parameters.Add(new SqlParameter("@tndRevNo", tndRevNo));
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TestModel mod = new TestModel();
                        mod.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        mod.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        mod.TestDescription = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        mod.Summary = Convert.ToString(ds.Tables[0].Rows[i]["Summary"]);
                        mod.Type = Convert.ToString(ds.Tables[0].Rows[i]["GroupType"]);
                        mod.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"]);
                        mod.Bundle = Convert.ToString(ds.Tables[0].Rows[i]["BundleType"]);
                        mod.KVLine = Convert.ToString(ds.Tables[0].Rows[i]["LineType"]);
                        mod.Price = Convert.ToDecimal(ds.Tables[0].Rows[i]["Price"]);
                        model.TestList.Add(mod);
                    }
                }
                model.DummyList = model.TestList;
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

        public TestMasterModel GetIntTestPricingList(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            TestMasterModel model = new TestMasterModel();
            model.TestList = new List<TestModel>();
            model.IntTndValues = new List<IntTenderDetails>();

            try
            {
                cmd = new SqlCommand("sp_GetIntTestPricingList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@tndNo", tndId));
                cmd.Parameters.Add(new SqlParameter("@tndRevNo", tndRevNo));
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TestModel mod = new TestModel();
                        mod.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        mod.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        mod.TestDescription = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                        mod.Summary = Convert.ToString(ds.Tables[0].Rows[i]["Summary"]);
                        mod.Type = Convert.ToString(ds.Tables[0].Rows[i]["GroupType"]);
                        mod.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"]);
                        mod.Bundle = Convert.ToString(ds.Tables[0].Rows[i]["BundleType"]);
                        mod.KVLine = Convert.ToString(ds.Tables[0].Rows[i]["LineType"]);
                        mod.Inr = Convert.ToDecimal(ds.Tables[0].Rows[i]["INR"]);
                        mod.Euro = Convert.ToDecimal(ds.Tables[0].Rows[i]["EURO"]);
                        mod.Usd = Convert.ToDecimal(ds.Tables[0].Rows[i]["USD"]);
                        mod.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
                        model.TestList.Add(mod);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        IntTenderDetails intTender = new IntTenderDetails();
                        intTender.Value = Convert.ToDecimal(ds.Tables[1].Rows[i]["Value"]);
                        intTender.Description = ds.Tables[1].Rows[i]["Description"].ToString();
                        model.IntTndValues.Add(intTender);
                    }
                }
                model.DummyList = model.TestList;
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

        public DataTable GetTestDatatable()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable dataTable = new DataTable();

            try
            {
                cmd = new SqlCommand("sp_GetTestPricingList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dataTable);
                connection.Close();

                //if (ds.Tables[0] != null)
                //{
                //    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //    {
                //        TestModel mod = new TestModel();
                //        mod.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                //        mod.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                //        mod.TestDescription = Convert.ToString(ds.Tables[0].Rows[i]["Description"]);
                //        mod.Summary = Convert.ToString(ds.Tables[0].Rows[i]["Summary"]);
                //        mod.Type = Convert.ToString(ds.Tables[0].Rows[i]["GroupType"]);
                //        mod.UTS = Convert.ToString(ds.Tables[0].Rows[i]["UTS"]);
                //        mod.Bundle = Convert.ToString(ds.Tables[0].Rows[i]["BundleType"]);
                //        mod.KVLine = Convert.ToString(ds.Tables[0].Rows[i]["LineType"]);
                //        mod.Price = Convert.ToDecimal(ds.Tables[0].Rows[i]["Price"]);
                //        model.TestList.Add(mod);
                //    }
                //}
                //model.DummyList = model.TestList;
                return dataTable;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return dataTable;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public bool SaveTestMasterPricingList(TestMasterModel model)
        {
            var newList = SelectChangedTests(model);
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int result = 0;
            try
            {
                foreach (var data in newList)
                {
                    cmd = new SqlCommand("sp_UpdateTestMasterPricing", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@testId", data.Id));
                    cmd.Parameters.Add(new SqlParameter("@testPrice", data.Price));
                    cmd.Parameters.Add(new SqlParameter("@tndNo", model.TenderId));
                    cmd.Parameters.Add(new SqlParameter("@tndRevNo", model.TenderRevisionId));
                    connection.Open();
                    result += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }

                cmd = new SqlCommand("sp_UpdateChangestoPreSavedTable", connection);
                cmd.Parameters.AddWithValue("@tenderId", model.TenderId);
                cmd.Parameters.AddWithValue("@tenderRevisionId", model.TenderRevisionId);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                if (newList.Count == result)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return false;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public bool SaveIntTestMasterPricingList(TestMasterModel model)
        {
            var newList = SelectChangedTests(model);
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int result = 0;
            try
            {
                foreach (var data in newList)
                {
                    cmd = new SqlCommand("sp_UpdateIntTestMasterPricing", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@testId", data.Id));
                    cmd.Parameters.Add(new SqlParameter("@inrPrice", data.Inr));
                    cmd.Parameters.Add(new SqlParameter("@euroPrice", data.Euro));
                    cmd.Parameters.Add(new SqlParameter("@usdPrice", data.Usd));
                    cmd.Parameters.Add(new SqlParameter("@quantity", data.Quantity));
                    cmd.Parameters.Add(new SqlParameter("@tndNo", model.TenderId));
                    cmd.Parameters.Add(new SqlParameter("@tndRevNo", model.TenderRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@incrementByPercentage", model.IncrementByPercentage));
                    connection.Open();
                    result += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }

                if (newList.Count == 0)
                {
                    cmd = new SqlCommand("sp_UpdateIntTestMasterPricing", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@tndNo", model.TenderId));
                    cmd.Parameters.Add(new SqlParameter("@tndRevNo", model.TenderRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@incrementByPercentage", model.IncrementByPercentage));
                    connection.Open();
                    result += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }

                cmd = new SqlCommand("sp_UpdateChangesMarkup", connection);
                cmd.Parameters.AddWithValue("@tenderId", model.TenderId);
                cmd.Parameters.AddWithValue("@tenderRevisionId", model.TenderRevisionId);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                var status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                //if (result > 0)
                return true;
                //else
                //    return false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return false;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public List<TestModel> SelectChangedTests(TestMasterModel model)
        {
            var oldList = model.DummyList;
            var newList = model.TestList;
            var finalList = new List<TestModel>();

            var compareResult = from old in oldList
                                join newl in newList
                                on old.Id equals newl.Id
                                where old.Price != (newl.Price)
                                || old.Inr != newl.Inr || old.Euro != newl.Euro
                                || old.Usd != newl.Usd || old.Quantity != newl.Quantity
                                select newl;

            finalList = compareResult.ToList();
            return finalList;
        }


        #region Test relation

        public TestMasterModel GetMasterRelationData(int bomId, int revNo, int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            TestMasterModel model = new TestMasterModel();
            model.ProductList = new List<MasterModel>();

            try
            {
                cmd = new SqlCommand("sp_GetBOMHierarchyForTest", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel mod = new MasterModel();
                        mod.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        mod.SrNo = Convert.ToString(ds.Tables[0].Rows[i]["SrNo"]);
                        mod.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        mod.Code = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        mod.TechnicalName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        mod.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        mod.ParentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ParentId"]);
                        mod.Type = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        mod.IsRelated = Convert.ToBoolean(ds.Tables[0].Rows[i]["Related"]);
                        mod.BOMId = Convert.ToInt32(ds.Tables[0].Rows[i]["BomId"]);
                        mod.IsDirectChild = Convert.ToBoolean(ds.Tables[0].Rows[i]["DirectChild"]);
                        mod.TestIds = Convert.ToString(ds.Tables[0].Rows[i]["TestIds"]);
                        mod.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        mod.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["SubAssemblyId"]);
                        mod.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["AssemblyId"]);
                        mod.ProductGroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductGroupId"]);
                        model.ProductList.Add(mod);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                {
                    model.ProjectName = Convert.ToString(ds.Tables[1].Rows[0]["ProjectName"]);
                    model.EndCustomerName = Convert.ToString(ds.Tables[1].Rows[0]["EndCustomerName"]);
                    model.EPCCustomerName = Convert.ToString(ds.Tables[1].Rows[0]["EPCCustomerName"]);
                }

                model.BomId = bomId;
                model.RevNo = revNo;
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

        public TestMasterModel GetTestDetails(ParameterModel param)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            TestMasterModel model = new TestMasterModel();
            model.ProductList = new List<MasterModel>();
            model.TestList = new List<TestModel>();

            try
            {
                cmd = new SqlCommand("sp_GetTestDetailsForProduct", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@bomId", param.BomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", param.RevId));
                cmd.Parameters.Add(new SqlParameter("@prodId", param.Id));
                cmd.Parameters.Add(new SqlParameter("@typeId", param.ProdType));
                cmd.Parameters.Add(new SqlParameter("@compId", param.ComponentId));
                cmd.Parameters.Add(new SqlParameter("@subId", param.SubAssemblyId));
                cmd.Parameters.Add(new SqlParameter("@asmId", param.AssemblyId));
                cmd.Parameters.Add(new SqlParameter("@pgId", param.ProductGroupId));
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    model.ProductName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
                    model.TechnicalName = Convert.ToString(ds.Tables[0].Rows[0]["TechnicalName"]);
                    model.Code = Convert.ToString(ds.Tables[0].Rows[0]["Code"]);
                    model.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[0]["CatalogueNo"]);
                }

                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        TestModel mod = new TestModel();
                        mod.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        mod.TestName = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        mod.TestDescription = Convert.ToString(ds.Tables[1].Rows[i]["Description"]);
                        mod.Summary = Convert.ToString(ds.Tables[1].Rows[i]["Summary"]);
                        mod.Type = Convert.ToString(ds.Tables[1].Rows[i]["GroupType"]);
                        mod.UTS = Convert.ToString(ds.Tables[1].Rows[i]["UTS"]);
                        mod.Bundle = Convert.ToString(ds.Tables[1].Rows[i]["BundleType"]);
                        mod.KVLine = Convert.ToString(ds.Tables[1].Rows[i]["LineType"]);
                        mod.IsSelected = Convert.ToBoolean(ds.Tables[1].Rows[i]["Selected"]);
                        model.TestList.Add(mod);
                    }
                }
                model.BomId = param.BomId;
                model.RevNo = param.RevId;
                model.ProdType = param.ProdType;
                model.SubAssemblyId = param.SubAssemblyId;
                model.AssemblyId = param.AssemblyId;
                model.ComponentId = param.ComponentId;
                model.ProductGroupId = param.ProductGroupId;
                model.ProductId = param.Id;
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

        public int UpdateTestDetails(TestMasterModel model)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int result = 0;
            try
            {
                cmd = new SqlCommand("sp_UpdateBOMTestRelation", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@bomId", model.BomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", model.RevNo));
                cmd.Parameters.Add(new SqlParameter("@prodId", model.ProductId));
                cmd.Parameters.Add(new SqlParameter("@typeId", model.ProdType));
                cmd.Parameters.Add(new SqlParameter("@compId", model.ComponentId));
                cmd.Parameters.Add(new SqlParameter("@subId", model.SubAssemblyId));
                cmd.Parameters.Add(new SqlParameter("@asmId", model.AssemblyId));
                cmd.Parameters.Add(new SqlParameter("@pgId", model.ProductGroupId));
                var data = string.Join(",", model.TestList.Where(x => x.IsSelected).Select(y => y.Id));
                cmd.Parameters.Add(new SqlParameter("@testIds", string.Join(",", model.TestList.Where(x => x.IsSelected).Select(y => y.Id))));
                connection.Open();
                result = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return result;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return result;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        #endregion
    }
}
