using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class TestDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<TestModel> GetAllTests(int subAsmId=0)
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
                        test.TestName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]); ;
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

        public int SubmitTestData(int ProductId,int Type,int BOMId,string values)
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

        
        public TestMasterModel GetTestMasterdata()
        {
            TestMasterModel testMaster = new TestMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            int status = 0;
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
            catch (Exception ex)
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
            List<int> statuses = new List<int>();
            int status = 0;
            string pattern = "~!,";

            //var TestId = form["TestId"];
            var Name = form["Name"];
            var Disciption = form["Disciption"];
            var Type = form["Type"];
            var KVLine = form["KVLine"];
            var UTS = form["UTS"];
            var Bundle = form["Bundle"];

            try
            {
                //var testIds = TestId.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                //testIds = testIds.Select(x => x = x.Replace("~!", "")).ToList();
                var names = Name.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                names = names.Select(x => x = x.Replace("~!", "")).ToList();
                var disciptions = Disciption.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                disciptions = disciptions.Select(x => x = x.Replace("~!", "")).ToList();
                var types = Type.Split(',').ToList();
                types = types.Select(x => x = x.Replace("~!", "")).ToList();
                var kVLines = KVLine.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                kVLines = kVLines.Select(x => x = x.Replace("~!", "")).ToList();
                var uTSs = UTS.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                uTSs = uTSs.Select(x => x = x.Replace("~!", "")).ToList();
                var bundles = Bundle.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                bundles = bundles.Select(x => x = x.Replace("~!", "")).ToList();
                

                SqlCommand cmd = new SqlCommand();

                for (int i = 0; i < names.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(names[i]))
                    {
                        connection = new SqlConnection(connectionString);
                        cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_addTests", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.Add(new SqlParameter("@TestId", string.IsNullOrEmpty(testIds[i]) ? string.Empty : testIds[i]));
                        cmd.Parameters.Add(new SqlParameter("@Name", names[i]));
                        cmd.Parameters.Add(new SqlParameter("@Disciption", string.IsNullOrEmpty(disciptions[i]) ? string.Empty : Convert.ToString(disciptions[i])));
                        cmd.Parameters.Add(new SqlParameter("@Type", string.IsNullOrEmpty(types[i]) ? string.Empty : Convert.ToString(types[i])));
                        cmd.Parameters.Add(new SqlParameter("@KVLine", string.IsNullOrEmpty(kVLines[i]) ? string.Empty : Convert.ToString(kVLines[i])));
                        cmd.Parameters.Add(new SqlParameter("@UTS", string.IsNullOrEmpty(uTSs[i]) ? string.Empty : Convert.ToString(uTSs[i])));
                        cmd.Parameters.Add(new SqlParameter("@Bundle", string.IsNullOrEmpty(bundles[i]) ? string.Empty : (bundles[i])));

                        connection.Open();
                        status = Convert.ToInt32(cmd.ExecuteScalar());
                        statuses.Add(status);
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
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
    }
}