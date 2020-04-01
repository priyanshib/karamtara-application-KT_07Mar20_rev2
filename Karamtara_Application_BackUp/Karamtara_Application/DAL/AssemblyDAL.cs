using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class AssemblyDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;

        public List<AssemblyMasterModel> GetAssemblyList()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
            try
            {
                cmd = new SqlCommand("sp_GetAssemblyList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        assmList.Add(assmModel);
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

        public List<string> GetAutoCompleteList(string prefix)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<string> assmList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetAssemblyMasterAutoComplete", connection);
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
                        string assmName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
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

        public List<AssemblyMasterModel> SearchSelectList(string searchText)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<AssemblyMasterModel> assmList = new List<AssemblyMasterModel>();
            try
            {
                cmd = new SqlCommand("sp_AssmSearchSelect", connection);
                cmd.Parameters.Add(new SqlParameter("@SearchText", searchText));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        AssemblyMasterModel assmModel = new AssemblyMasterModel();
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[i]["Code"]);
                        assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[i]["TechnicalName"]);
                        assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        assmList.Add(assmModel);
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

        public int CreateAssembly(FormCollection form, List<HttpPostedFileBase> drawingFiles)
        {
            AssemblyMasterModel assmModel = new AssemblyMasterModel();
            string assmCode = form["txtAssmCode"];
            string name = form["txtAssmName"];
            string techName = form["txtTechName"];
            string drawingNo = form["txtDrawingNo"];
            string utsValue = form["UtsValueId"];
            int assemblyId = 0;
            bool cancel = false;

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                connection.Open();
                using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        DataSet ds = new DataSet();
                        cmd = new SqlCommand("sp_AddNewAssembly", connection);
                        cmd.Parameters.Add(new SqlParameter("@Code", assmCode));
                        cmd.Parameters.Add(new SqlParameter("@Name", name));
                        cmd.Parameters.Add(new SqlParameter("@TechName", techName));
                        cmd.Parameters.Add(new SqlParameter("@DrawingNo", drawingNo));
                        cmd.Parameters.Add(new SqlParameter("@utsValue", Convert.ToInt32(utsValue)));
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Transaction = trans;
                        assemblyId = Convert.ToInt32(cmd.ExecuteScalar());

                        if (assemblyId < 0)
                            return 0;

                        DocumentDAL docDal = new DocumentDAL();
                        if(drawingFiles!= null && drawingFiles.Count > 0)
                        {
                            var drawingFile = drawingFiles.FirstOrDefault();
                            var names = docDal.SaveDrawingFileAttachements(drawingFile, assemblyId);
                            if(drawingFile != null && drawingFile.ContentLength > 0 && !string.IsNullOrEmpty(names))
                            {
                                cmd = new SqlCommand("sp_AttachDrawingFiles", connection);
                                cmd.Parameters.Add(new SqlParameter("@assemblyId", assemblyId));
                                cmd.Parameters.Add(new SqlParameter("@DrawingFile", names));
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Transaction = trans;
                                int statuss = Convert.ToInt32(cmd.ExecuteScalar());
                                if (statuss <= 0)
                                    cancel = true;
                            }
                        }
                        if(!cancel)
                        {
                            trans.Commit();
                            connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                    }
                }
                return assemblyId;
            }
            catch (Exception ex)
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

        public AssemblyMasterModel GetAssemblyProducts(int assmId = 0)
        {
            AssemblyMasterModel assmModel = new AssemblyMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MasterModel> masterList = new List<MasterModel>();
            //List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
            List<UTSMS> utsList = new List<UTSMS>();
            try
            {
                cmd = new SqlCommand("sp_GetAllAssmDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@AssmId", assmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    if(assmId == 0)
                        assmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"]);
                    else
                        assmModel.AssemblyId = assmId;
                    assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[0]["Code"]) ?? string.Empty;
                    assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]) ?? string.Empty;
                    assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[0]["TechnicalName"]) ?? string.Empty;
                    assmModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[0]["DrawingNo"]) ?? string.Empty;
                    assmModel.UTS = Convert.ToString(ds.Tables[0].Rows[0]["UTSValue"]) ?? string.Empty;
                    assmModel.DrawingFileName = Convert.ToString(ds.Tables[0].Rows[0]["DrawingFileName"]) ?? string.Empty;
                    assmModel.TotalGrWt = Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalGrWt"]);
                    assmModel.TotalNetWt = Convert.ToDecimal(ds.Tables[0].Rows[0]["TotalNetWt"]);
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        MasterModel model = new MasterModel();
                        model.SrNo = Convert.ToString(ds.Tables[1].Rows[i]["Srno"]);
                        model.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        model.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"]);
                        model.Name = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        model.Quantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        model.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        model.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        model.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        model.ParentId= Convert.ToInt32(ds.Tables[1].Rows[i]["ParentId"]);
                        model.IsRelated = Convert.ToBoolean(ds.Tables[1].Rows[i]["Related"]);
                        model.MasterType = Convert.ToInt32(ds.Tables[1].Rows[i]["Type"]);
                        model.Material = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                        model.Code = Convert.ToString(ds.Tables[1].Rows[i]["Code"]);
                        model.TechnicalName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"]);
                        model.Grade = Convert.ToString(ds.Tables[1].Rows[i]["Grade"]);
                        model.DrawingNo = Convert.ToString(ds.Tables[1].Rows[i]["DrawingNo"]);
                        model.TotalUnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalGrWt"]);
                        model.TotalUnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["TotalNetWt"]);
                        masterList.Add(model);
                    }
                }
                if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                    {
                        UTSMS utsModel = new UTSMS();
                        utsModel.Id = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                        utsModel.UTSValue = Convert.ToString(ds.Tables[2].Rows[i]["UTSValue"]) ?? string.Empty;
                        utsList.Add(utsModel);
                    }
                }
                assmModel.MasterList = masterList;
                assmModel.UtsValueList = utsList;
                //assmModel.ComponentList = compModelList;
                return assmModel;
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

        //public List<MasterModel> numbering(List<MasterModel> masterList)
        //{
        //    int level1 = 1;
        //    int level2 = 1;
        //    int level3 = 1;
        //    int level4 = 1;

        //    foreach (var pg in masterList.Where(x=> x.ParentType == 0 && x.Type == 1 && x.Id > 0 && x.IsRelated == true))
        //    {
        //       foreach(var asm in masterList.Where(x=>x.Id > 0 && x.Type == 2 && x.ParentType == 1))
        //    }

        //    return mainList;
        //}

        public AssemblyMasterModel GetAssemblyProductsAutocomplete(int assmId)
        {
            AssemblyMasterModel assmModel = new AssemblyMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<MasterModel> masterList = new List<MasterModel>();
            List<SubAssemblyListModel> autoCompleteList = new List<SubAssemblyListModel>();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetAllAssemblyDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@AssmId", assmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    assmModel.AssemblyId = assmId;
                    assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[0]["Code"]) ?? string.Empty;
                    assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]) ?? string.Empty;
                    assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[0]["TechnicalName"]) ?? string.Empty;
                    assmModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[0]["DrawingNo"]) ?? string.Empty;
                    assmModel.DrawingFileName = Convert.ToString(ds.Tables[0].Rows[0]["DrawingFileName"]) ?? string.Empty;
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        MasterModel masterModel = new MasterModel();
                        masterModel.Id = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        masterModel.Name = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        masterModel.CatalogueNo = Convert.ToString(ds.Tables[1].Rows[i]["CatalogueNo"]);
                        masterModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        masterModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        masterModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        masterModel.Quantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]);
                        masterModel.TechnicalName = Convert.ToString(ds.Tables[1].Rows[i]["TechnicalName"]);
                        masterModel.Code = Convert.ToString(ds.Tables[1].Rows[i]["Code"]);
                        masterModel.MasterType = Convert.ToInt32(ds.Tables[1].Rows[i]["Type"]);
                        masterModel.Material = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                        masterModel.Grade = Convert.ToString(ds.Tables[1].Rows[i]["MaterialGrade"]);
                        masterModel.DrawingNo = Convert.ToString(ds.Tables[1].Rows[i]["DrawingNo"]);
                        masterList.Add(masterModel);
                    }
                }
               
                assmModel.MasterList = masterList;
                return assmModel;
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

        public AssemblyMasterModel GetSubAssembliesAndComponentsBySearch(string searchString)
        {
            AssemblyMasterModel assmModel = new AssemblyMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            //List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
            List<MasterModel> modelList = new List<MasterModel>();
            //List<SubAssemblyListModel> autoCompleteList = new List<SubAssemblyListModel>();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetSubAssmAndCompBySearch", connection);
                cmd.Parameters.Add(new SqlParameter("@searchText", searchString));
                cmd.Parameters.Add(new SqlParameter("@Type", 2));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                //if (ds.Tables[0] != null)
                //{
                //    assmModel.AssemblyId = assmId;
                //    assmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[0]["Id"]) ?? string.Empty;
                //    assmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]) ?? string.Empty;
                //    assmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[0]["TechnicalName"]) ?? string.Empty;
                //}
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        MasterModel masterModel = new MasterModel();
                        masterModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        masterModel.MasterType = Convert.ToInt32(ds.Tables[0].Rows[i]["Type"]);
                        masterModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        masterModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        masterModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        masterModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        masterModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        masterModel.Material = Convert.ToString(ds.Tables[0].Rows[i]["Material"]);
                        modelList.Add(masterModel);
                    }
                }
                
                assmModel.MasterList = modelList;
                //assmModel.AutoCompleteList = autoCompleteList;
                return assmModel;
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

        public int EditAssembly(FormCollection form)
        {
            int assemblyId = Convert.ToInt32(form["AssmId"]);
            if (assemblyId <= 0)
                return 0;

            int status = 0;
            string pattern = ",";
            //var SubAssmNamesString = form["SubAssmName"];
            var subAssmQty = form["SubAssmQty"];
            var subAssmIds = form["SubAssmIds"];
            var types = form["ItemType"];

            try
            {
                //var SubAssmNames = SubAssmNamesString.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                //SubAssmNames = SubAssmNames.Select(x => x = x.Replace("~!", "")).ToList();
                var subAssmQties = subAssmQty.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                subAssmQties = subAssmQties.Select(x => x = x.Replace("~!", "")).ToList();
                var subAssmIdsList = subAssmIds.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                subAssmIdsList = subAssmIdsList.Select(x => x = x.Replace("~!", "")).ToList();
                var typeList = types.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                typeList = typeList.Select(x => x = x.Replace("~!", "")).ToList();
                List<int> sAList = new List<int>();
                List<int> cList = new List<int>();
                connection = new SqlConnection(connectionString);
                connection.Open();
                using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand();
                        for (int i = 0; i < subAssmIdsList.Count; i++)
                        {
                            if(!string.IsNullOrEmpty(subAssmIdsList[i]))
                            {
                                cmd = new SqlCommand("sp_EditAssemblyDetails", connection);
                                cmd.Parameters.Add(new SqlParameter("@AssmId", assemblyId));
                                cmd.Parameters.Add(new SqlParameter("@Id", Convert.ToInt32(subAssmIdsList[i])));
                                cmd.Parameters.Add(new SqlParameter("@Qty", subAssmQties.ElementAtOrDefault(i) != null ? (string.IsNullOrEmpty(subAssmQties[i]) ? 1 : (Convert.ToInt32(subAssmQties[i]) > 1 ? Convert.ToInt32(subAssmQties[i]) : 1)) : 1));
                                cmd.Parameters.Add(new SqlParameter("@Type", Convert.ToInt32(typeList[i])));
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Transaction = trans;
                                status += Convert.ToInt32(cmd.ExecuteScalar());
                                if (Convert.ToInt32(typeList[i]) == 3)
                                    sAList.Add(Convert.ToInt32(subAssmIdsList[i]));
                                else if (Convert.ToInt32(typeList[i]) == 4)
                                    cList.Add(Convert.ToInt32(subAssmIdsList[i]));
                            }
                        }

                        cmd = new SqlCommand("sp_DeleteOldAssemblyDetails", connection);
                        cmd.Parameters.Add(new SqlParameter("@AssmId", assemblyId));
                        cmd.Parameters.Add(new SqlParameter("@SubAssmIds", string.Join(",", sAList)));
                        cmd.Parameters.Add(new SqlParameter("@CompIds", string.Join(",", cList)));
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Transaction = trans;
                        status += Convert.ToInt32(cmd.ExecuteScalar());

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
                return 0;
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

        public List<string> AutoCompleteCodeList(string searchText)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            List<string> assCodeList = new List<string>();
            try
            {
                cmd = new SqlCommand("sp_GetassCodeAutoCompList ", connection);
                cmd.Parameters.Add(new SqlParameter("@Text", searchText));
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                connection.Open();
                da.Fill(ds);
                connection.Close();
                if (ds.Tables[0].Rows.Count != 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string data = Convert.ToString(ds.Tables[0].Rows[i]["code"]);
                        assCodeList.Add(data);
                    }
                }
                return assCodeList;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return assCodeList;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public AssemblyMasterModel uploadFile(FormCollection form, List<HttpPostedFileBase> drawingFiles)
        {
            AssemblyMasterModel assmModel = new AssemblyMasterModel();
            string assmId = form["AssmId"];
            bool cancel = false;

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;

            try
            {
                connection.Open();
                        DocumentDAL docDal = new DocumentDAL();
                        if (drawingFiles != null && drawingFiles.Count > 0)
                        {
                            var drawingFile = drawingFiles.FirstOrDefault();
                            var names = docDal.SaveDrawingFileAttachements(drawingFile, Convert.ToInt32(assmId));
                            if (drawingFile != null && drawingFile.ContentLength > 0 && !string.IsNullOrEmpty(names))
                            {
                                cmd = new SqlCommand("sp_AttachDrawingFiles", connection);
                                cmd.Parameters.Add(new SqlParameter("@assemblyId", Convert.ToInt32(assmId)));
                                cmd.Parameters.Add(new SqlParameter("@DrawingFile", names));
                                cmd.CommandType = CommandType.StoredProcedure;
                               // cmd.Transaction = trans;
                                status = Convert.ToInt32(cmd.ExecuteScalar());
                                if (status <= 0)
                                    cancel = true;
                            }
                        }

                assmModel.AssemblyId = Convert.ToInt32(assmId);
                 connection.Close();
                return assmModel;
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

        public int deleteFile(string AssmId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            int status = 0;
            try
            {
                cmd = new SqlCommand("sp_deleteDrawFileName", connection);
                cmd.Parameters.Add(new SqlParameter("@assmId", AssmId));
                cmd.CommandType = CommandType.StoredProcedure;
                
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                
                return status;
            }
            catch (Exception ex)
            {
                //ex.ToString();
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
        //public List<string> GetAutoComp_SubAssmCode(string prefix)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<string> subAssmCodeList = new List<string>();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetSubAssmAutoComplete", connection);
        //        cmd.Parameters.Add(new SqlParameter("@SearchText", prefix));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        if (ds.Tables[1] != null)
        //        {
        //            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
        //            {
        //                string subAssmCode = Convert.ToString(ds.Tables[1].Rows[i]["SubAssmCode"]);
        //                subAssmCodeList.Add(subAssmCode);
        //            }
        //        }
        //        return subAssmCodeList;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.ToString());
        //        return null;
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}

        //public AssemblyMasterModel GetSubAssmRowDetails(string code)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetSubAssmRows", connection);
        //        cmd.Parameters.Add(new SqlParameter("@Text", code));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        AssemblyMasterModel subasmModel = new AssemblyMasterModel();
        //        if (ds.Tables[0] != null)
        //        {
        //            subasmModel.AssemblyId = Convert.ToInt32(ds.Tables[0].Rows[0]["AssmId"]);
        //            subasmModel.AssemblyCode = Convert.ToString(ds.Tables[0].Rows[0]["SubAssmCode"]);
        //            subasmModel.AssemblyName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
        //            subasmModel.AssmTechName = Convert.ToString(ds.Tables[0].Rows[0]["TechnicalName"]);
        //        }

        //        return subasmModel;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.ToString());
        //        return null;
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }

        //}

        //public List<string> GetAutoComp_TName(string prefix)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<string> subAssmCodeList = new List<string>();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetSubAssmAutoComplete", connection);
        //        cmd.Parameters.Add(new SqlParameter("@SearchText", prefix));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        if (ds.Tables[2] != null)
        //        {
        //            for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
        //            {
        //                string subAssmCode = Convert.ToString(ds.Tables[2].Rows[i]["TechnicalName"]);
        //                subAssmCodeList.Add(subAssmCode);
        //            }
        //        }
        //        return subAssmCodeList;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.ToString());
        //        return null;
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}

        //public int DeleteAssembly(int subAssmId, int userId)
        //{
        //    int status = 0;
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();

        //    try
        //    {
        //        cmd = new SqlCommand("sp_DeleteSubAssembly", connection);
        //        cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
        //        cmd.Parameters.Add(new SqlParameter("@userId", userId));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        connection.Open();
        //        status = Convert.ToInt32(cmd.ExecuteScalar());
        //        connection.Close();
        //        return status;
        //    }
        //    catch (Exception ex)
        //    {
        //        return status;
        //    }
        //}

        public byte[] DocumentDownload(int assmId, string actualFileName, out string fileName)
        {
            fileName = string.Empty;
            try
            {

                string path = HostingEnvironment.MapPath(string.Format("~/Documents/Assembly/{0}/{1}", assmId, actualFileName));
                var extension = Path.GetExtension(path);
                fileName = string.Format("Assembly{0}", extension);
                byte[] fileBytes = File.ReadAllBytes(path);
                return fileBytes;
                //return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            //fileName = string.Format("{0}", docType.ToString());
            //return fileBytes;
        }
    }
}