using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using static Karamtara_Application.HelperClass.Flags;
using System.Web.Mvc;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Collections.Generic;
using Spire.Xls;
using System.IO;
using Karamtara_Application.Models.Tender;
using Karamtara_Application.DAL.Tender;
using System.Linq;

namespace Karamtara_Application.DAL
{
    public class LoginDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        ExcelRange cell;
        ExcelFill fill;
        Border border;
        RawMaterialPricingDAL rmpDAL;
        MarkupPricingDAL mDAL;
        FreightChargesDAL fDAL;
        TenderDetailsDAL tenderDetailsDAL;
        TestDAL testDAL;
        Color lightGray = System.Drawing.ColorTranslator.FromHtml("#e9e9e9");
        Color darkBlue = System.Drawing.ColorTranslator.FromHtml("#105483");

        public UserModel CheckLogin(LoginModel model)
        {
            int status = 0;
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            UserModel user = new UserModel();
            var passwordHash = new byte[36];
            try
            {
                cmd = new SqlCommand("sp_GetPasswordHash", connection);
                cmd.Parameters.Add(new SqlParameter("@UserName", model.UserName));
                //cmd.Parameters.Add(new SqlParameter("@passwordHash", hashedPass));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();

                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    var passString = Convert.ToString(ds.Tables[0].Rows[0]["passwordhash"]);

                    if (!string.IsNullOrEmpty(passString))
                    {
                        passwordHash = Convert.FromBase64String(passString);
                        PasswordHash pass = new PasswordHash(passwordHash);
                        var result = pass.Verify(model.Password);
                        if (result)
                        {
                            status = 1;
                            user = GetUserbyCred(model.UserName, passString);
                        }
                        else
                            status = 0;
                    }
                }
                else
                    status = 0;

            }
            catch (Exception ex)
            {
                return user;
            }
            return user;
        }

        public UserModel GetUserbyCred(string userName, string passwordHash)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            UserModel user = new UserModel();
            try
            {
                cmd = new SqlCommand("sp_CheckLogin", connection);
                cmd.Parameters.Add(new SqlParameter("@UserName", userName));
                cmd.Parameters.Add(new SqlParameter("@passwordHash", passwordHash));
                cmd.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);
                connection.Close();
                if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
                {
                    user.UserId = Convert.ToInt32(ds.Tables[0].Rows[0]["UserId"]);
                    user.FirstName = Convert.ToString(ds.Tables[0].Rows[0]["FirstName"]);
                    user.LastName = Convert.ToString(ds.Tables[0].Rows[0]["LastName"]);
                    user.UserName = Convert.ToString(ds.Tables[0].Rows[0]["UserName"]);
                    user.Email = Convert.ToString(ds.Tables[0].Rows[0]["EmailId"]);
                    user.IsActive = Convert.ToBoolean(ds.Tables[0].Rows[0]["IsActive"]);
                    user.UserTypeId = Convert.ToInt32(ds.Tables[0].Rows[0]["Type"]);
                    user.Salutation = Convert.ToString(ds.Tables[0].Rows[0]["Salutation"]);
                    user.UserType = Convert.ToString(ds.Tables[0].Rows[0]["TypeName"]);
                }
                return user;
            }
            catch (Exception ex)
            {
                return user;
            }
        }

       

        //public byte[] GetExcelNew ()
        //{
        //    Workbook workbook = new Workbook();
        //    DataTable dt = GetDataTableFromDB();
        //    workbook.Worksheets[0].InsertDataTable(dt, true, 1, 1);
        //    //dt = GetDataTableFromDB("select * from items");
        //    //workbook.Worksheets[1].InsertDataTable(dt, true, 1, 1);
        //    //dt = GetDataTableFromDB("select * from parts");
        //    //workbook.Worksheets[2].InsertDataTable(dt, true, 1, 1);
        //    MemoryStream data = new MemoryStream();
        //    workbook.SaveToStream(data);
        //    return data.ToArray();
        //    //using (MemoryStream ms = new MemoryStream())
        //    //{
        //    //    data.CopyTo(ms);
        //    //    return ms.ToArray();
        //    //}

        //    //workbook.SaveToFile("sample.xlsx", ExcelVersion.Version2013);
        //    //System.Diagnostics.Process.Start("sample.xlsx");
        //}
        //static DataTable GetDataTableFromDB()
        //{
        //    DataTable dt = new DataTable();
        //    var dal = new TestDAL();
        //    dt = dal.GetTestDatatable();
        //    return dt;
        //}
    }
}