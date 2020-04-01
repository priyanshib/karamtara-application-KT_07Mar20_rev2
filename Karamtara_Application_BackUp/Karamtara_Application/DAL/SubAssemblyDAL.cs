using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Karamtara_Application.DAL
{
    public class SubAssemblyDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        
        public SubAssemblyMasterModel GetSumAssemblyMasterData()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            List<SubAssemblyListModel> subAssemblyList = new List<SubAssemblyListModel>();
            List<ComponentModel> componentList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetSubAssemblyAllData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        SubAssemblyListModel subModel = new SubAssemblyListModel();
                        subModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        subModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        subModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        subModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        subAssemblyList.Add(subModel);
                    }
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[1].Rows[i]["Qty"]);
                        compModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        compModel.RawMaterial = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        compModel.DrawingNo = Convert.ToString(ds.Tables[1].Rows[i]["DrawingNo"]);
                        compModel.MaterialGrade = Convert.ToString(ds.Tables[1].Rows[i]["MaterialGrade"]);
                        componentList.Add(compModel);
                    }
                }
                subAssmModel.ComponenetList = componentList;
                subAssmModel.SubAssemblyList = subAssemblyList;
                return subAssmModel;
            }
            catch (Exception ex)
            {
                //  Console.Write(ex.ToString());
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

        public SubAssemblyMasterModel GetComponentDetails(int subAssId)
        {
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            List<SubAssemblyListModel> subList = new List<SubAssemblyListModel>();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetComponentDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@SubAssmId", subAssId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    subAssmModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"]);
                    subAssmModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[0]["Name"]);
                    subAssmModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[0]["CatalogueNo"]);
                    subAssmModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[0]["DrawingNo"]);
                }
                if (ds.Tables[1] != null)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[1].Rows[i]["Id"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[1].Rows[i]["Name"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[1].Rows[i]["Qty"]);
                        compModel.Size = Convert.ToString(ds.Tables[1].Rows[i]["Size"]);
                        compModel.RawMaterial = Convert.ToString(ds.Tables[1].Rows[i]["Material"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[1].Rows[i]["UnitNetWt"]);
                        compModel.DrawingNo = Convert.ToString(ds.Tables[1].Rows[i]["DrawingNo"]);
                        compModel.MaterialGrade = Convert.ToString(ds.Tables[1].Rows[i]["MaterialGrade"]);
                        compList.Add(compModel);
                    }
                }
                //if (ds.Tables[2] != null && refreshList)
                //{
                //    for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                //    {
                //        SubAssemblyListModel subModel = new SubAssemblyListModel();
                //        subModel.SubAssemblyId = Convert.ToInt32(ds.Tables[2].Rows[i]["Id"]);
                //        subModel.SubAssemblyName = Convert.ToString(ds.Tables[2].Rows[i]["Name"]);
                //        subModel.CatalogueNo = Convert.ToString(ds.Tables[2].Rows[i]["CatalogueNo"]);
                //        subModel.DrawingNo = Convert.ToString(ds.Tables[2].Rows[i]["DrawingNo"]);
                //        subList.Add(subModel);
                //    }
                //}
                subAssmModel.ComponenetList = compList;
                subAssmModel.SubAssemblyList = subList;
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
            return subAssmModel;

        }

        public SubAssemblyMasterModel GetSubAssemblyList(string prefix = "")
        {
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            List<SubAssemblyListModel> subList = new List<SubAssemblyListModel>();

            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();

            try
            {
                cmd = new SqlCommand("sp_GetSubAssembliesList", connection);
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
                        SubAssemblyListModel subModel = new SubAssemblyListModel();
                        subModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        subModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        subModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        subModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        subList.Add(subModel);
                    }
                }
                subAssmModel.SubAssemblyList = subList;
            }
            catch (Exception ex)
            {} 
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return subAssmModel;
        }

        public SubAssemblyMasterModel GetComponents(int subAssmId = 0)
        {
            SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_GetComponents", connection);
                cmd.Parameters.Add(new SqlParameter("@SubAssmId", subAssmId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[0].Rows[i]["Name"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        compModel.RawMaterialId = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterialId"]);
                        compModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        compModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterial"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        compModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        compModel.MaterialGrade = Convert.ToString(ds.Tables[0].Rows[i]["MaterialGrade"]);
                        compList.Add(compModel);
                    }
                }
                subAssmModel.ComponenetList = compList;
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
            return subAssmModel;

        }

        public int EditSubAssembly(FormCollection form)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            int status = 0;
            int disableStatus = 0;
            int editCount = 0;
            try
            {
                int subAssmId = Convert.ToInt32(form["SubAssemblyId"]);

                string componentId = form["item.ComponentId"];
                string Qty = form["item.Qty"];

                List<int> compIdList = new List<string>(componentId.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).Select(Int32.Parse).ToList();
                List<double> qtyList = new List<string>(Qty.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).Select(double.Parse).ToList();

                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("sp_DisabledOldCompnentDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@SubAssmId", Convert.ToInt32(subAssmId)));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                disableStatus = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

                for (int i = 0; i < compIdList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    cmd = new SqlCommand("sp_EditSubProducts", connection);
                    cmd.Parameters.Add(new SqlParameter("@ProductId", Convert.ToInt32(subAssmId)));
                    cmd.Parameters.Add(new SqlParameter("@SubProductId", compIdList.ElementAtOrDefault(i)!=0 ? compIdList[i] :0));
                    cmd.Parameters.Add(new SqlParameter("@Qty", qtyList.ElementAtOrDefault(i) > 1 ? qtyList[i] : 1));
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    status = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                    editCount += status;
                }

                return editCount;
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
            return editCount;
        }

        public int CreateSubAssembly(FormCollection form)
        {
            int status = 0;
            int subAssmId = 0;
            string pattern = "~!,";
            string catNum = form["SumAssmCatNo"];
            string names = form["SumAssmName"];
            string componentName = form["CompName"];
            string compId = form["ComponentId"];
            string qtys = form["Qty"];
            string drawingNum = form["DrawingNoMaster"] ?? string.Empty;

            try
            {
                var subAssmCatNo = catNum.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                subAssmCatNo = subAssmCatNo.Select(x => x = x.Replace("~!", "")).ToList();
                var subAssmNames = names.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                subAssmNames = subAssmNames.Select(x => x = x.Replace("~!", "")).ToList();
                var compNames = componentName.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                compNames = compNames.Select(x => x = x.Replace("~!", "")).ToList();
                var compIds = compId.Split(',').ToList();
                compIds = compIds.Select(x => x = x.Replace("~!", "")).ToList();
                var quantities = qtys.Split(',').ToList();
                quantities = quantities.Select(x => x = x.Replace("~!", "")).ToList();
                var drawingNo = drawingNum.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                drawingNo = drawingNo.Select(x => x = x.Replace("~!", "")).ToList();

                connection = new SqlConnection(connectionString);
                connection.Open();
                using (var trans = connection.BeginTransaction(IsolationLevel.Snapshot))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_AddSubAssembly", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CatNo", subAssmCatNo.Where(x => !string.IsNullOrEmpty(x)).Select(y => y).FirstOrDefault() ?? string.Empty));
                        cmd.Parameters.Add(new SqlParameter("@SubAssemblyName", subAssmNames.Where(x => !string.IsNullOrEmpty(x)).Select(y => y).FirstOrDefault() ?? string.Empty));
                        cmd.Parameters.Add(new SqlParameter("@DrawingNum", drawingNo.Where(x => !string.IsNullOrEmpty(x)).Select(y => y).FirstOrDefault() ?? string.Empty));
                        cmd.Transaction = trans;
                        subAssmId = Convert.ToInt32(cmd.ExecuteScalar());

                        if (subAssmId < 1)
                        {
                            trans.Rollback();
                            return subAssmId;
                        }

                        for (int i = 0; i < compIds.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(compIds[i]))
                            {
                                cmd = new SqlCommand();
                                cmd = new SqlCommand("sp_AddSubAssemblyRelation", connection);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@SubAssmId", subAssmId));
                                cmd.Parameters.Add(new SqlParameter("@ComponentId", Convert.ToInt32(compIds[i])));
                                cmd.Parameters.Add(new SqlParameter("@Quantity", quantities.ElementAtOrDefault(i) != null ? (string.IsNullOrEmpty(quantities[i]) ? 1 : Convert.ToInt32(quantities[i])) : 1));
                                cmd.Transaction = trans;
                                status += Convert.ToInt32(cmd.ExecuteScalar());
                            }
                        }

                        if (status > 0)
                        {
                            trans.Commit();
                            connection.Close();
                            return subAssmId;
                        }
                        else
                        {
                            trans.Rollback();
                            return 0;
                        }
                    }
                    catch(Exception ex)
                    {
                        return 0;
                    }
                }
            }
            catch(Exception ex)
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

        public int CreateComponents(FormCollection form)
        {
            List<int> statuses = new List<int>();
            int status = 0;
            string pattern = "~!,";

            var catNum = form["CatNum"] ?? string.Empty;
            var compName = form["CompName"] ?? string.Empty;
            var rawMat = form["RawMat"] ?? string.Empty;
            var quantity = form["Quantity"] ?? string.Empty;
            var drawingNo = form["drawingNo"] ?? string.Empty;
            var uGW = form["UGW"] ?? string.Empty;
            var uNW = form["UNW"] ?? string.Empty;
            var mGrade = form["MaterialGrade"] ?? string.Empty;
            var size = form["Size"] ?? string.Empty;
            var checkbox = form["checkbox"] ?? string.Empty;
            var radio = form["radio"] ?? string.Empty;
            var temp1 = form.AllKeys.Where(x => x.Contains("radio")).ToList();
           
            try
            {
                var catNums = catNum.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                catNums = catNums.Select(x => x = x.Replace("~!", "")).ToList();
                var compNames = compName.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                compNames = compNames.Select(x => x = x.Replace("~!", "")).ToList();
                var rawMats = rawMat.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                rawMats = rawMats.Select(x => x = x.Replace("~!", "")).ToList();
                var drawingNos = drawingNo.Split(',').ToList();
                drawingNos = drawingNos.Select(x => x = x.Replace("~!", "")).ToList();
                var uGWs = uGW.Split(',').ToList();
                uGWs = uGWs.Select(x => x = x.Replace("~!", "")).ToList();
                var uNWs = uNW.Split(',').ToList();
                uNWs = uNWs.Select(x => x = x.Replace("~!", "")).ToList();
                var sizes = size.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                sizes = sizes.Select(x => x = x.Replace("~!", "")).ToList();
                var chbox = checkbox.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                chbox = chbox.Select(x => x = x.Replace("~!", "")).ToList();
                var matGrades = mGrade.Split(new string[] { pattern }, StringSplitOptions.None).ToList();
                matGrades = matGrades.Select(x => x = x.Replace("~!", "")).ToList();

                List<string> GalvanizedRequired = new List<string>();
                for (int i = 0; i < compNames.Count();i++)
                {
                    var temp = 0;
                    for (int j = 0; j < chbox.Count(); j++)
                    {
                        if(!string.IsNullOrEmpty(chbox[j]))
                        {
                            if (i == Convert.ToInt32(chbox[j]))
                            {
                                GalvanizedRequired.Add("1");
                                temp = 1;
                            }
                            
                        }
                       
                    }
                    if (temp == 0)
                    {
                        GalvanizedRequired.Add("0");
                    }

                }

                List<string> GalvanizedMaterial = new List<string>();
                if (temp1 != null)
                {
                    //List<string> GalvanizedMaterial = new List<string>();
                    for (int i = 0; i < compNames.Count(); i++)
                    {
                        var temp = 0;
                        foreach (var abc in temp1)
                        {
                            string[] abcd = abc.Split('_');
                            var ab = abcd[1];

                            if (i == Convert.ToInt32(ab))
                            {
                                for (int j = i; j < compNames.Count(); j++)
                                {
                                    if (j == Convert.ToInt32(ab))
                                    {
                                        var tempNum = form[abc];
                                        if (tempNum == "Zinc")
                                        { GalvanizedMaterial.Add("1"); }
                                        else if (tempNum == "2") { GalvanizedMaterial.Add("2"); }

                                        temp = 1;
                                    }
                                }
                            }
                        }
                        if (temp == 0)
                        {
                            GalvanizedMaterial.Add("0");
                        }
                    }
                }
                    
                    //var tempNum = string.IsNullOrEmpty(form[abc]) ? 0 : 1;

                

                SqlCommand cmd = new SqlCommand();

                for (int i = 0; i < compNames.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(compNames[i]))
                    {
                        connection = new SqlConnection(connectionString);
                        cmd = new SqlCommand();
                        cmd = new SqlCommand("sp_AddComponents", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CatNum", string.IsNullOrEmpty(catNums[i]) ? string.Empty : catNums[i]));
                        cmd.Parameters.Add(new SqlParameter("@CompName", compNames[i]));
                        cmd.Parameters.Add(new SqlParameter("@RawMaterial", rawMats[i]));
                        cmd.Parameters.Add(new SqlParameter("@MatGrade", string.IsNullOrEmpty(matGrades[i]) ? "" : matGrades[i]));
                        cmd.Parameters.Add(new SqlParameter("@DrawingNo", string.IsNullOrEmpty(drawingNos[i]) ? string.Empty : Convert.ToString(drawingNos[i])));
                        cmd.Parameters.Add(new SqlParameter("@Ugw", string.IsNullOrEmpty(uGWs[i]) ? 0 : Convert.ToDecimal(uGWs[i])));
                        cmd.Parameters.Add(new SqlParameter("@Unw", string.IsNullOrEmpty(uNWs[i]) ? 0 : Convert.ToDecimal(uNWs[i])));
                        cmd.Parameters.Add(new SqlParameter("@Size", string.IsNullOrEmpty(sizes[i]) ? string.Empty : (sizes[i])));
                        //cmd.Parameters.Add(new SqlParameter("@GalvanizedRequired", string.IsNullOrEmpty(GalvanizedRequired[i]) ? 0 : (Convert.ToInt32(GalvanizedRequired[i]) == i) ? 1 : 0));
                        cmd.Parameters.Add(new SqlParameter("@GalvanizedRequired", string.IsNullOrEmpty(GalvanizedRequired[i]) ? 0 : (Convert.ToInt32(GalvanizedRequired[i]))));
                        cmd.Parameters.Add(new SqlParameter("@GalvanizedMaterial", string.IsNullOrEmpty(GalvanizedMaterial[i]) ? 0 : (Convert.ToInt32(GalvanizedMaterial[i]))));

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

        public List<ComponentModel> getComponenetMaterData()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                cmd = new SqlCommand("sp_getComponentMasterData", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[0].Rows[i]["ComponentName"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        compModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterial"]);
                        //compModel.RawMaterialId = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterialId"]);
                        compModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        compModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        compModel.MaterialGrade = Convert.ToString(ds.Tables[0].Rows[i]["MaterialGrade"]);
                        compModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        compModel.GalvanizedRequired = Convert.ToBoolean(ds.Tables[0].Rows[i]["GalvanizedRequired"]);
                        compModel.GalvanizedMaterial = Convert.ToString(ds.Tables[0].Rows[i]["GalvanizedMaterial"]);
                        compList.Add(compModel);
                    }
                }
                return compList;
            }
            catch (Exception ex)
            {
                return compList;
            }
        }

        public int SubmitComponentMaster(ComponentModel model)
            //int ComponentId string ComponentName,string RawMaterialId,string Size, int Qty, string UnitGrWt, string UnitNetWt,int GalvanizedRequired,int GalvanizedMaterial, string DrawingNo, string MaterialGrade;
        {
            int status = 0;
        connection = new SqlConnection(connectionString);
        SqlCommand cmd = new SqlCommand();

        DataSet ds = new DataSet();
        SqlDataAdapter adapter = new SqlDataAdapter();
        List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand();
                cmd = new SqlCommand("sp_UpdateComponentMaster", connection);
                
                cmd.Parameters.Add(new SqlParameter("@ComponentId", model.ComponentId));
                cmd.Parameters.Add(new SqlParameter("@DrawingNo", string.IsNullOrEmpty(model.DrawingNo) ? string.Empty : Convert.ToString(model.DrawingNo)));
                cmd.Parameters.Add(new SqlParameter("@UnitGrWt", string.IsNullOrEmpty(Convert.ToString(model.UnitGrWt)) ? 0 : Convert.ToDecimal(model.UnitGrWt)));
                cmd.Parameters.Add(new SqlParameter("@UnitNetWt", string.IsNullOrEmpty(Convert.ToString(model.UnitNetWt)) ? 0 : Convert.ToDecimal(model.UnitNetWt)));
                cmd.Parameters.Add(new SqlParameter("@Size", string.IsNullOrEmpty(model.Size) ? string.Empty : Convert.ToString(model.Size)));
                cmd.Parameters.Add(new SqlParameter("@MaterialGrade", string.IsNullOrEmpty(model.MaterialGrade) ? string.Empty : Convert.ToString(model.MaterialGrade)));
                cmd.Parameters.Add(new SqlParameter("@RawMaterial", string.IsNullOrEmpty(model.RawMaterial) ? string.Empty : Convert.ToString(model.RawMaterial)));
                cmd.Parameters.Add(new SqlParameter("@GalvanizedRequired", model.GalvanizedRequiredInt));
                cmd.Parameters.Add(new SqlParameter("@GalvanizedMaterial", model.GalvanizedRequiredInt == 1 ? Convert.ToInt32(model.GalvanizedMaterial) : 0));
                //cmd.Parameters.Add(new SqlParameter("@GalvanizedMaterial", model.GalvanizedRequiredInt > 1 ? 0 : model.GalvanizeequiredInt));
                cmd.CommandType = CommandType.StoredProcedure;

                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();

            }
            catch (Exception ex)
            {

            }
            return status;
        }

        public ComponentModel GetComponentMasterdata(int ComponentId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ComponentModel compModel = new ComponentModel();
            //List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand();
                cmd = new SqlCommand("sp_getComponentMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@ComponentId", ComponentId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
               
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        //ComponentModel compModel = new ComponentModel();
                        compModel.ComponentId = Convert.ToInt32(ds.Tables[0].Rows[i]["ComponentId"]);
                        compModel.ComponentName = Convert.ToString(ds.Tables[0].Rows[i]["ComponentName"]);
                        compModel.Qty = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);
                        //compModel.RawMaterialId = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterialId"]);
                        compModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterial"]);
                        compModel.Size = Convert.ToString(ds.Tables[0].Rows[i]["Size"]);
                        //compModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterial"]);
                        compModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
                        compModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
                        compModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
                        compModel.MaterialGrade = Convert.ToString(ds.Tables[0].Rows[i]["MaterialGrade"]);
                        compModel.DrawingNo = Convert.ToString(ds.Tables[0].Rows[i]["DrawingNo"]);
                        compModel.GalvanizedRequired = Convert.ToBoolean(ds.Tables[0].Rows[i]["GalvanizedRequired"]);
                        compModel.GalvanizedRequiredInt = Convert.ToInt32(ds.Tables[0].Rows[i]["GalvanizedRequired"]);
                        compModel.GalvanizedMaterial = Convert.ToString(ds.Tables[0].Rows[i]["GalvanizedMaterial"]);
                        //compList.Add(compModel);
                    }
                }
                return compModel;
            }
            catch (Exception ex)
            {
                return compModel;
            }
            //return compModel;
        }

        public int DeleteComponenetMaster(int ComponentId,int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            ComponentModel compModel = new ComponentModel();
            //List<ComponentModel> compList = new List<ComponentModel>();
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand();
                cmd = new SqlCommand("sp_deleteComponentMaster", connection);
                cmd.Parameters.Add(new SqlParameter("@ComponentId", ComponentId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
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
            //return compModel;
        }

        public int DeleteSubAssembly(int subAssmId, int userId)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd = new SqlCommand("sp_DeleteSubAssembly", connection);
                cmd.Parameters.Add(new SqlParameter("@subAssmId", subAssmId));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
                return status;
            }
            catch(Exception ex)
            {
                return status;
            }
        }

        public List<GalvanizedMaterials> GetGalvanizedMaterialList()
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();

            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<GalvanizedMaterials> materialList = new List<GalvanizedMaterials>();
            try
            {
                cmd = new SqlCommand("sp_GetGalvanizedMaterialList", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        GalvanizedMaterials model = new GalvanizedMaterials();
                        model.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                        model.Element = Convert.ToString(ds.Tables[0].Rows[i]["Element"]);
                        materialList.Add(model);
                    }
                }
                return materialList;
            }
            catch (Exception ex)
            {
                return materialList;
            }
        }

        //public List<SubAssemblyListModel> GetProducts()
        //{
        //    SubAssemblyMasterModel prodMsModel = new SubAssemblyMasterModel();
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();

        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<SubAssemblyListModel> prodList = new List<SubAssemblyListModel>();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetAllProducts", connection);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        if (ds.Tables[0] != null)
        //        {
        //            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //            {
        //                SubAssemblyListModel prodModel = new SubAssemblyListModel();
        //                prodModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
        //                prodModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["ProductName"]);
        //                prodModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
        //                prodModel.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"]);
        //                prodModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
        //                prodModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
        //                prodList.Add(prodModel);
        //            }
        //        }
        //        return prodList;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //    return prodList;

        //}

        //public int AddProdCategory(string category)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    int status = 0;
        //    try
        //    {
        //        cmd = new SqlCommand("sp_AddProdCategory", connection);
        //        cmd.Parameters.Add(new SqlParameter("@Category", category));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        connection.Open();
        //        status = Convert.ToInt32(cmd.ExecuteScalar());
        //        connection.Close();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //    return status;

        //}

        //public List<SubAssemblyListModel> GetProductListFromCat(int catId)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();

        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<SubAssemblyListModel> prodList = new List<SubAssemblyListModel>();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetProductList_Cat", connection);
        //        cmd.Parameters.Add(new SqlParameter("@CategoryId", catId));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        if (ds.Tables[0] != null)
        //        {
        //            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //            {
        //                SubAssemblyListModel sumAssmListModel = new SubAssemblyListModel();

        //                sumAssmListModel.SubAssemblyId = Convert.ToInt32(ds.Tables[0].Rows[i]["ProductId"]);
        //                sumAssmListModel.SubAssemblyName = Convert.ToString(ds.Tables[0].Rows[i]["ProductName"]);
        //                sumAssmListModel.CategoryId = Convert.ToInt32(ds.Tables[0].Rows[i]["CategoryId"]);
        //                sumAssmListModel.CatalogueNo = Convert.ToString(ds.Tables[0].Rows[i]["CatalogueNo"]);
        //                prodList.Add(sumAssmListModel);
        //            }
        //        }

        //        return prodList;
        //    }
        //    catch (Exception)
        //    {
        //        //  Console.Write(ex.ToString());
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

        //public int AddComponents(FormCollection form, int CategoryId)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();

        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<SubAssemblyListModel> subAssmList = new List<SubAssemblyListModel>();
        //    int subProdCount = 0;
        //    try
        //    {
        //        string catalogueNo = form["SumAssmCatNo"];
        //        string sumAssmName = form["SumAssmName"];

        //        int addSubAssmId = 0;
        //        int addComponents = 0;

        //        addSubAssmId = AddSubAssmbly(catalogueNo, sumAssmName, CategoryId);
        //        if (addSubAssmId > 0)
        //        {
        //            string CompName = form["CompName"];
        //            string RM = form["RM"];
        //            string Qty = form["Qty"];
        //            string GrossWt = form["GrossWt"];
        //            string NetWt = form["NetWt"];

        //            List<String> componentName = new List<String>(CompName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        //            List<String> rmList = new List<String>(RM.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        //            List<String> qtyList = new List<String>(Qty.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        //            List<String> grWtList = new List<String>(GrossWt.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        //            List<String> netWtList = new List<String>(NetWt.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        //            for (int i = 0; i < componentName.Count; i++)
        //            {
        //                connection = new SqlConnection(connectionString);
        //                SqlCommand cmd1 = new SqlCommand();

        //                DataSet ds = new DataSet();
        //                cmd = new SqlCommand("sp_AddSubProducts", connection);
        //                cmd.Parameters.Add(new SqlParameter("@ProductId", addSubAssmId));
        //                cmd.Parameters.Add(new SqlParameter("@ComponentName", componentName[i]));
        //                cmd.Parameters.Add(new SqlParameter("@RMId", rmList[i]));
        //                cmd.Parameters.Add(new SqlParameter("@Qty", qtyList[i]));
        //                cmd.Parameters.Add(new SqlParameter("@UnitGrWt", grWtList[i]));
        //                cmd.Parameters.Add(new SqlParameter("@UnitNetWt", netWtList[i]));
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                connection.Open();
        //                addComponents = Convert.ToInt32(cmd.ExecuteScalar());
        //                connection.Close();
        //                subProdCount += addComponents;
        //            }
        //        }
        //        return subProdCount;
        //    }
        //    catch (Exception)
        //    {
        //        //  Console.Write(ex.ToString());
        //        return subProdCount;
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }

        //}

        //private int AddSubAssmbly(string catalogueNo, string subAssmName, int CategoryId)
        //{
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();
        //    int status = 0;
        //    try
        //    {
        //        cmd = new SqlCommand("sp_AddProduct", connection);
        //        cmd.Parameters.Add(new SqlParameter("@CatalogueNo", catalogueNo));
        //        cmd.Parameters.Add(new SqlParameter("@ProductName", subAssmName));
        //        cmd.Parameters.Add(new SqlParameter("@CategoryId", CategoryId));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        connection.Open();
        //        status = Convert.ToInt32(cmd.ExecuteScalar());
        //        connection.Close();
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //    return status;

        //}

        //public List<int> GetComponentIds(int subAssmId)
        //{
        //    SubAssemblyMasterModel subAssmModel = new SubAssemblyMasterModel();
        //    connection = new SqlConnection(connectionString);
        //    SqlCommand cmd = new SqlCommand();

        //    DataSet ds = new DataSet();
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    List<int> compIds = new List<int>();
        //    try
        //    {
        //        cmd = new SqlCommand("sp_GetSubProduct", connection);
        //        cmd.Parameters.Add(new SqlParameter("@prodId", subAssmId));
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        adapter.SelectCommand = cmd;
        //        connection.Open();
        //        adapter.Fill(ds);
        //        connection.Close();
        //        if (ds.Tables[0] != null)
        //        {
        //            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //            {
        //                compIds.Add(Convert.ToInt32(ds.Tables[0].Rows[i]["SubProductId"]));
        //                //SubProductModel subProdModel = new SubProductModel();
        //                //subProdModel.SubProductId = 
        //                //subProdModel.ComponentName = Convert.ToString(ds.Tables[0].Rows[i]["ComponentName"]);
        //                //subProdModel.Qty = Convert.ToDecimal(ds.Tables[0].Rows[i]["Qty"]);
        //                //subProdModel.RawMaterialId = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterialId"]);
        //                //subProdModel.RawMaterial = Convert.ToString(ds.Tables[0].Rows[i]["RawMaterial"]);
        //                //subProdModel.UnitGrWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitGrWt"]);
        //                //subProdModel.UnitNetWt = Convert.ToDecimal(ds.Tables[0].Rows[i]["UnitNetWt"]);
        //                //subProdList.Add(subProdModel);
        //            }
        //        }
        //        //prodMsModel.SubProductList = subProdList;
        //        return compIds;
        //    }
        //    catch (Exception ex)
        //    {
        //        return compIds;
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //        }
        //    }
        //}
    }
}