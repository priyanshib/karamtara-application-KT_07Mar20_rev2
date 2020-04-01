using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Karamtara_Application.DAL
{
    public class RMMasterDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public RMMasterModel GetRMDetails()
        {
            RMMasterModel rmMasterModel = new RMMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<RawMaterialModel> rmList = new List<RawMaterialModel>();
            try
            {
                cmd = new SqlCommand("sp_GetRawMaterialDetails", connection);
              //  cmd.Parameters.Add(new SqlParameter("@CategoryId", catId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        RawMaterialModel rmModel = new RawMaterialModel();
                        rmModel.MaterialId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        rmModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        rmModel.MaterialDesc = Convert.ToString(ds.Tables[0].Rows[i]["MaterialDescription"]);
                        rmModel.MaterialType = Convert.ToString(ds.Tables[0].Rows[i]["MType"]);
                        rmList.Add(rmModel);
                    }
                }
                rmMasterModel.SearchAutoComplete = rmList.Select(x => x.MaterialDesc).ToList();
                //rmMasterModel.CategoryList = catList.ToList();
                rmMasterModel.RawMaterialList = rmList;
                return rmMasterModel;
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

        public List<RMGroupTypeModel> GetRMTypes(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<RMGroupTypeModel> grouplist = new List<RMGroupTypeModel>();
            try
            {
                cmd = new SqlCommand("sp_GetRMGroupTypes", connection);
                cmd.Parameters.Add(new SqlParameter("@prefix", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        RMGroupTypeModel model = new RMGroupTypeModel();
                        model.GroupId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.GroupName = Convert.ToString(ds.Tables[0].Rows[i]["GroupName"]);
                        grouplist.Add(model);
                    }
                }
                return grouplist;
            }
            catch (Exception ex)
            {
                return new List<RMGroupTypeModel>();
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public int SaveRawMaterial(RMMasterModel model)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            //string matCode = string.Empty;
            int status = 0;

            //if(!string.IsNullOrEmpty(model.MaterialCode))
            //{
            //    matCode = model.MaterialCode;
            //}
            //else if(!string.IsNullOrEmpty(model.MaterialCategoryTxt))
            //{
            //    matCode = model.MaterialCategoryTxt;
            //}
            try
            {
                cmd = new SqlCommand("sp_SaveRawMaterial", connection);
                //cmd.Parameters.Add(new SqlParameter("@Material", matCode));
                cmd.Parameters.Add(new SqlParameter("@MaterialDesc", model.MaterialName));
                cmd.Parameters.Add(new SqlParameter("@groupId", model.GroupId));
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

        public List<string> AutoCompleteList(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            List<string> rawMatList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetRMAutoCompList ", connection);
                cmd.Parameters.Add(new SqlParameter("@Text", prefix));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                connection.Open();
                da.Fill(ds);
                connection.Close();
                if (ds.Tables[0].Rows.Count != 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string data = Convert.ToString(ds.Tables[0].Rows[i]["data"]);
                        rawMatList.Add(data);
                    }
                }
                return rawMatList;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return rawMatList;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public RMMasterModel GetRMSearchData(string searchText)
        {
            RMMasterModel rmMasterModel = new RMMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<RawMaterialModel> rmList = new List<RawMaterialModel>();
            try
            {
                cmd = new SqlCommand("sp_GetRMAutoCompList", connection);
                cmd.Parameters.Add(new SqlParameter("@Text", searchText));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        RawMaterialModel rmModel = new RawMaterialModel();
                        rmModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        rmModel.MaterialDesc = Convert.ToString(ds.Tables[0].Rows[i]["data"]);
                        rmModel.MaterialType = Convert.ToString(ds.Tables[0].Rows[i]["MType"]);
                        rmList.Add(rmModel);
                    }
                }
                rmMasterModel.SearchAutoComplete = rmList.Select(x => x.MaterialDesc).ToList();
                rmMasterModel.RawMaterialList = rmList;
                return rmMasterModel;
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

    }
}