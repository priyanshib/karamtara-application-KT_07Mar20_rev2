using Karamtara_Application.Models;
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
    public class TenderStructureDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        public TenderStructureModel tenderStrModel;

        public int CreateStructure(FormCollection form,int userId)
        {
            int type = 0;
            string lineName = string.Empty;
            string lotName = string.Empty;
            string pckgName = string.Empty;

            int enqId = Convert.ToInt32(form["EnquiryId"]);
            int bomId = Convert.ToInt32(form["BomId"]);
            int revNo = Convert.ToInt32(form["RevisionNo"]);
            int tndId = Convert.ToInt32(form["TenderId"]);
            int tndRevNo = Convert.ToInt32(form["TndRevNo"]);
            type = Convert.ToInt32(form["hidStrType"]);
            List<string> strList = new List<string>();
            int status = 0;

            if (type > 0 && type == 1)
            {
                lineName = form["lineName"];
                strList = (lineName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            else if (type > 0 && type == 2)
            {
                lotName = form["lotName"];
                strList = (lotName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            else if (type > 0 && type == 3)
            {
                pckgName = form["pkgName"];
                strList = (pckgName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            try
            {

                for (int i = 0; i < strList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    cmd = new SqlCommand("sp_CreateTenderStructure", connection);
                    cmd.Parameters.Add(new SqlParameter("@EnquiryId", enqId));
                    cmd.Parameters.Add(new SqlParameter("@BomId", bomId));
                    cmd.Parameters.Add(new SqlParameter("@RevNo", revNo));
                    cmd.Parameters.Add(new SqlParameter("@Type", type));
                    cmd.Parameters.Add(new SqlParameter("@Name", strList[i]));
                    cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                    cmd.Parameters.Add(new SqlParameter("@tndId", tndId));
                    cmd.Parameters.Add(new SqlParameter("@tndRevNo", tndRevNo));
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand = cmd;
                    connection.Open();
                    status += Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                }
                return status;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public TenderStructureModel GetStructureDetails(int enqId,int tndId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<StructureDetails> strList = new List<StructureDetails>();
            try
            {
                cmd = new SqlCommand("sp_GetTenderStrDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@tndId", tndId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables.Count > 0)
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        {
                            StructureDetails strModel = new StructureDetails();
                            strModel.Id = Convert.ToInt32(ds.Tables[0].Rows[i]["Id"]);
                            strModel.Name = Convert.ToString(ds.Tables[0].Rows[i]["StrName"]);
                            strModel.StrType = Convert.ToInt32(ds.Tables[0].Rows[i]["StrType"]);
                            strList.Add(strModel);
                        }
                    }
                tenderStrModel = new TenderStructureModel();
                tenderStrModel.LineList = strList.Where(x => x.StrType == 1).ToList();
                tenderStrModel.LotList = strList.Where(x => x.StrType == 2).ToList();
                tenderStrModel.PackageList = strList.Where(x => x.StrType == 3).ToList();

                return tenderStrModel;
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

        public int SaveTenderDetails(FormCollection formData,int userId)
        {
            string line = formData["LineId"];
            string lot = formData["LotId"];
            string package = formData["PackageId"];
            int status = 0;
            try
            {
                List<string> lineList = new List<string>(line.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> lotList = new List<String>(lot.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                List<String> pckgList = new List<String>(package.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                for (int i = 0; i < lineList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    cmd = new SqlCommand("sp_SaveTenderStructure", connection);
                    cmd.Parameters.Add(new SqlParameter("@StrId", lineList[i]));
                    cmd.Parameters.Add(new SqlParameter("@LineId", lineList[i]));
                    cmd.Parameters.Add(new SqlParameter("@LotId", lotList[i]));
                    cmd.Parameters.Add(new SqlParameter("@PackageId", pckgList[i]));
                    cmd.Parameters.Add(new SqlParameter("@UserId", userId));
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
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public TenderStructureModel GetEditTenderStrDetails(int enqId)
        {
            TenderStructureModel strModel = new TenderStructureModel();
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            List<StructureDetails> strList = new List<StructureDetails>();
            List<TenderStructureModel> strDetailsList = new List<TenderStructureModel>();
            try
            {
                cmd = new SqlCommand("sp_GetTenderEditDetails", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        StructureDetails strDetails = new StructureDetails();
                        strDetails.StrType = Convert.ToInt32(ds.Tables[0].Rows[i]["StrType"]);
                        strDetails.Name = Convert.ToString(ds.Tables[0].Rows[i]["StrName"]);
                        strList.Add(strDetails);
                    }
                }
                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {
                        TenderStructureModel tndModel = new TenderStructureModel();
                        tndModel.StrId = Convert.ToInt32(ds.Tables[1].Rows[i]["StrId"]);
                        tndModel.LineId = Convert.ToInt32(ds.Tables[1].Rows[i]["LineId"]);
                        tndModel.LotId = Convert.ToInt32(ds.Tables[1].Rows[i]["LotId"]);
                        tndModel.PackageId = Convert.ToInt32(ds.Tables[1].Rows[i]["PackageId"]);
                        strDetailsList.Add(tndModel);
                    }
                }
                strModel.LineList = strList.Where(x => x.StrType == 1).ToList();
                strModel.LotList = strList.Where(x => x.StrType == 2).ToList();
                strModel.PackageList = strList.Where(x => x.StrType == 3).ToList();
                strModel.DetailsList = strDetailsList;
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

        public int UpdateTenderStr(FormCollection formData)
        {
            int type = 0;
            string lineName = string.Empty;
            string lotName = string.Empty;
            string pckgName = string.Empty;

            int enqId = Convert.ToInt32(formData["EnquiryId"]);
            type = Convert.ToInt32(formData["hidStrType"]);
            int tndId = Convert.ToInt32(formData["TenderId"]);
            int tndRevNo = Convert.ToInt32(formData["TndRevNo"]);
            List<string> strList = new List<string>();
            int status = 0;

            connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            SqlDataAdapter ad = new SqlDataAdapter();

            command = new SqlCommand("sp_DeactiveExistingStr", connection);
            command.Parameters.Add(new SqlParameter("@EnqId", enqId));
            command.Parameters.Add(new SqlParameter("@TndId", enqId));
            command.Parameters.Add(new SqlParameter("@StrType", type));
            command.CommandType = CommandType.StoredProcedure;
            ad.SelectCommand = command;
            connection.Open();
            int inactiveStatus = 0;
            inactiveStatus = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();

            if (type > 0 && type == 1)
            {
                lineName = formData["lineName"];
                strList = (lineName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            else if (type > 0 && type == 2)
            {
                lotName = formData["lotName"];
                strList = (lotName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            else if (type > 0 && type == 3)
            {
                pckgName = formData["pkgName"];
                strList = (pckgName.Split(',')).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
            try
            {

                for (int i = 0; i < strList.Count; i++)
                {
                    connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand();
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    cmd = new SqlCommand("sp_UpdateTenderStr", connection);
                    cmd.Parameters.Add(new SqlParameter("@EnquiryId", enqId));
                    cmd.Parameters.Add(new SqlParameter("@TndId",tndId));
                    cmd.Parameters.Add(new SqlParameter("@TndRevNo",tndRevNo));
                    cmd.Parameters.Add(new SqlParameter("@Type", type));
                    cmd.Parameters.Add(new SqlParameter("@Name", strList[i]));
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
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

        }

        public TenderStructureModel CreateTenderId(int enqId,int bomId,int revNo,int userId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderStructureModel tndStrModel = new TenderStructureModel();
            try
            {
                cmd = new SqlCommand("sp_CreateTenderId", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.Parameters.Add(new SqlParameter("@userId", userId));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                if (dt !=null && dt.Rows.Count>0)
                {
                    tndStrModel.EnquiryId = Convert.ToInt32(dt.Rows[0]["EnquiryId"].ToString());
                    tndStrModel.BomId = Convert.ToInt32(dt.Rows[0]["BomId"].ToString());
                    tndStrModel.RevisionNo = Convert.ToInt32(dt.Rows[0]["RevisionNo"].ToString());
                    tndStrModel.TenderId = Convert.ToInt32(dt.Rows[0]["TenderId"].ToString());
                    tndStrModel.TenderRevisionNo = Convert.ToInt32(dt.Rows[0]["TenderRevNo"].ToString());
                }
                return tndStrModel;
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

        public bool CheckIfStrExists(int enqId, int bomId,int revNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            TenderStructureModel tndStrModel = new TenderStructureModel();
            try
            {
                cmd = new SqlCommand("sp_CheckTenderStrExists", connection);
                cmd.Parameters.Add(new SqlParameter("@enqId", enqId));
                cmd.Parameters.Add(new SqlParameter("@bomId", bomId));
                cmd.Parameters.Add(new SqlParameter("@revNo", revNo));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
                bool strExists = false;
                if (dt != null && dt.Rows.Count > 0)
                {
                    strExists = Convert.ToBoolean(dt.Rows[0]["StrExists"]);
                }
                return strExists;
            }
            catch (Exception ex)
            {
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

      

    }
}