using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace Karamtara_Application.DAL
{
    public class ReportDAL
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
        SqlConnection connection;
        ExcelRange cell;
        Color lightGray = System.Drawing.ColorTranslator.FromHtml("#e9e9e9");
        Color darkBlue = System.Drawing.ColorTranslator.FromHtml("#105483");
        Border border;

        public DataSet GetTenderTonnage(int tndId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("GetTonnageReportByTender", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                adapter.SelectCommand = cmd;
                cmd.CommandTimeout = 0;
                connection.Open();
                adapter.Fill(ds);

                connection.Close();
            }
            catch (Exception ex)
            {

            }
            return ds;
        }

        public DataSet GetTndRawMaterialPricing(int tndId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("GetRMPricingReportByTnd", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return ds;
        }

        public DataSet GetProductGrpTonnage(int tndId, int tndRevNo)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("GetTonnageReportByPG", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tndId", tndId);
                cmd.Parameters.AddWithValue("@tndRevNo", tndRevNo);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(ds);

                connection.Close();
            }
            catch (Exception ex)
            {

            }
            return ds;
        }

        public DataTable GetBOMComponentQty(int bomId, int revId)
        {
            connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            try
            {
                cmd = new SqlCommand("sp_GetComponentQtyReport", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@bomId", bomId);
                cmd.Parameters.AddWithValue("@revId", revId);
                adapter.SelectCommand = cmd;
                connection.Open();
                adapter.Fill(dt);
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return dt;
        }

        public byte[] GetTenderTonnageReport(int tenderId)
        {
            var tonnageData = GetTenderTonnage(tenderId);
            List<int> revisionList = new List<int>();
            for (int i = 0; i < tonnageData.Tables[0].Rows.Count; i++)
            {
                revisionList.Add(Convert.ToInt32(tonnageData.Tables[0].Rows[i]["TenderRevisionNo"]));
            }

            var reportColumns = new List<ColumnModel>() { new ColumnModel("Raw Material Group", 50, 12, "") };

            foreach (var rev in revisionList)
            {
                reportColumns.Add(new ColumnModel(string.Format("Revision {0}", rev), 60, 12, "") { SubColumns = new List<string>() { "Gross Weigth", "Net Weight" } });
            }

            //reportColumns.Add(new ColumnModel("Total Gross Weight", 30, 12, ""));
            //reportColumns.Add(new ColumnModel("Total Net Weight", 30, 12, ""));

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                var tenderReport = excelPackage.Workbook.Worksheets.Add("Tender Report");
                tenderReport.Name = "Tender Report";

                int row = 1;
                int col = 1;
                foreach (var column in reportColumns)
                {
                    if (column.SubColumns != null && column.SubColumns.Count > 0)
                    {
                        tenderReport.Cells[row, col, row, col + 1].Merge = true;
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = column.Name;

                        cell = tenderReport.Cells[row + 1, col];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = "Gross Weight(Kg)";
                        tenderReport.Column(col).Width = 30;

                        cell = tenderReport.Cells[row + 1, col + 1];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = "Net Weight(Kg)";
                        tenderReport.Column(col + 1).Width = 30;
                        col += 2;
                    }
                    else
                    {
                        tenderReport.Cells[row, col, row + 1, col].Merge = true;
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = column.Name;
                        tenderReport.Column(col).Width = column.Width;
                        col += 1;
                    }
                }

                var dbColNames = new List<KeyValuePair<string, dynamic>>();
                for (int i = 0; i < tonnageData.Tables[1].Columns.Count; i++)
                {
                    if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower() == "rawmaterialname")
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "string"));
                    }
                    else if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower().Contains("_grwt"))
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "decimal"));
                    }
                    else if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower().Contains("_ntwt"))
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "decimal"));
                    }
                }

                row = 3;
                col = 1;
                int lastCol = 1;
                decimal totalGrossWt = 0;
                decimal totalNetWt = 0;
                for (int j = 0; j < tonnageData.Tables[1].Rows.Count; j++, row++)
                {
                    col = 1;
                    foreach (var column in dbColNames)
                    {
                        totalGrossWt = 0;
                        totalNetWt = 0;
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleProperties(cell);

                        if (column.Key.ToLower().Contains("_grwt") && column.Value == "decimal")
                        {
                            var value = Convert.ToDecimal(tonnageData.Tables[1].Rows[j][column.Key]);
                            totalGrossWt = totalGrossWt + value;
                            cell.Value = value;
                        }
                        else if (column.Key.ToLower().Contains("_ntwt") && column.Value == "decimal")
                        {
                            var value = Convert.ToDecimal(tonnageData.Tables[1].Rows[j][column.Key]);
                            totalNetWt = totalNetWt + value;
                            cell.Value = value;
                        }
                        else
                            cell.Value = Convert.ToString(tonnageData.Tables[1].Rows[j][column.Key]);
                        col++;
                        lastCol = lastCol < col ? col : lastCol;
                    }
                    //cell = tenderReport.Cells[row, lastCol];
                    //cell = GiveCellStyleProperties(cell);
                    //cell.Value = totalGrossWt;

                    //cell = tenderReport.Cells[row, lastCol + 1];
                    //cell = GiveCellStyleProperties(cell);
                    //cell.Value = totalNetWt;
                }


                int whileCount = 1;
                while (whileCount <= row)
                {
                    tenderReport.Row(whileCount).Height = 20;
                    whileCount++;
                }


                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] GetTenderTonnageProductWise(int tenderId, int tenderRevId)
        {
            var tonnageData = GetProductGrpTonnage(tenderId, tenderRevId);

            List<string> productList = new List<string>();

            for (int i = 0; i < tonnageData.Tables[0].Rows.Count; i++)
            {
                productList.Add(Convert.ToString(tonnageData.Tables[0].Rows[i]["GroupName"]));
            }

            var reportColumns = new List<ColumnModel>() { new ColumnModel("Raw Material Group", 50, 12, "") };

            foreach (var prod in productList)
            {
                reportColumns.Add(new ColumnModel(prod, 60, 12, "") { SubColumns = new List<string>() { "Gross Weigth", "Net Weight" } });
            }

            reportColumns.Add(new ColumnModel("Total Gross Weight", 30, 12, ""));
            reportColumns.Add(new ColumnModel("Total Net Weight", 30, 12, ""));

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                var tenderReport = excelPackage.Workbook.Worksheets.Add("Tender Products Report");
                tenderReport.Name = "Tender Products Report";

                int row = 1;
                int col = 1;
                foreach (var column in reportColumns)
                {
                    if (column.SubColumns != null && column.SubColumns.Count > 0)
                    {
                        tenderReport.Cells[row, col, row, col + 1].Merge = true;
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = column.Name;

                        cell = tenderReport.Cells[row + 1, col];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = "Gross Weight(Kg)";
                        tenderReport.Column(col).Width = 30;

                        cell = tenderReport.Cells[row + 1, col + 1];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = "Net Weight(Kg)";
                        tenderReport.Column(col + 1).Width = 30;
                        col += 2;
                    }
                    else
                    {
                        tenderReport.Cells[row, col, row + 1, col].Merge = true;
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = column.Name;
                        tenderReport.Column(col).Width = column.Width;
                        col += 1;
                    }
                }

                var dbColNames = new List<KeyValuePair<string, dynamic>>();
                for (int i = 0; i < tonnageData.Tables[1].Columns.Count; i++)
                {
                    if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower() == "rawmaterialname")
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "string"));
                    }
                    else if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower().Contains("_grwt_"))
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "decimal"));
                    }
                    else if (tonnageData.Tables[1].Columns[i].ColumnName.ToLower().Contains("_ntwt_"))
                    {
                        dbColNames.Add(new KeyValuePair<string, dynamic>(Convert.ToString(tonnageData.Tables[1].Columns[i].ColumnName), "decimal"));
                    }
                }

                row = 3;
                col = 1;
                int lastCol = 1;
                decimal totalGrossWt;
                decimal totalNetWt;

                for (int j = 0; j < tonnageData.Tables[1].Rows.Count; j++, row++)
                {
                    totalGrossWt = 0;
                    totalNetWt = 0;
                    col = 1;
                    foreach (var column in dbColNames)
                    {
                        cell = tenderReport.Cells[row, col];
                        cell = GiveCellStyleProperties(cell);

                        if (column.Key.ToLower().Contains("_grwt_") && column.Value == "decimal")
                        {
                            var value = Convert.ToDecimal(tonnageData.Tables[1].Rows[j][column.Key]);
                            totalGrossWt = totalGrossWt + value;
                            cell.Value = value;
                        }
                        else if (column.Key.ToLower().Contains("_ntwt_") && column.Value == "decimal")
                        {
                            var value = Convert.ToDecimal(tonnageData.Tables[1].Rows[j][column.Key]);
                            totalNetWt = totalNetWt + value;
                            cell.Value = value;
                        }
                        else
                            cell.Value = Convert.ToString(tonnageData.Tables[1].Rows[j][column.Key]);
                        col++;
                        lastCol = lastCol < col ? col : lastCol;
                    }
                    cell = tenderReport.Cells[row, lastCol];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = totalGrossWt;

                    cell = tenderReport.Cells[row, lastCol + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = totalNetWt;
                }

                int whileCount = 1;
                while (whileCount <= row)
                {
                    tenderReport.Row(whileCount).Height = 20;
                    whileCount++;
                }


                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] GetBOMComponentQtyReport(int bomId, int revId)
        {
            var data = GetBOMComponentQty(bomId, revId);

            var reportColumns = new List<ColumnModel>() {
                new ColumnModel("Sr. No.", 10, 12, "srno"),
                new ColumnModel("Catalogue No", 20, 12, "CatalogueNo"),
                new ColumnModel("Component Name", 50, 12, "ComponentName"),
                new ColumnModel("Raw Material Name", 50, 12, "MaterialDescription"),
                new ColumnModel("Size", 30, 12, "Size"),
                new ColumnModel("Quantity", 20, 12, "Quantity"),
            };


            //reportColumns.Add(new ColumnModel("Total Gross Weight", 30, 12, ""));
            //reportColumns.Add(new ColumnModel("Total Net Weight", 30, 12, ""));

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                var compReport = excelPackage.Workbook.Worksheets.Add("Component Quantity Report");
                compReport.Name = "Component Quantity Report";

                int row = 1;
                int col = 1;
                foreach (var column in reportColumns)
                {
                    cell = compReport.Cells[row, col];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = column.Name;
                    compReport.Column(col).Width = column.Width;
                    col += 1;
                }

                row = 2;
                col = 1;
                int lastCol = 1;
                for (int j = 0; j < data.Rows.Count; j++, row++)
                {
                    col = 1;
                    foreach (var column in reportColumns)
                    {
                        cell = compReport.Cells[row, col];
                        cell = GiveCellStyleProperties(cell);
                        if (column.PropName == "srno")
                        {
                            cell.Value = j + 1;
                        }
                        else
                            cell.Value = Convert.ToString(data.Rows[j][column.PropName]);
                        col++;
                        lastCol = lastCol < col ? col : lastCol;
                    }
                }


                int whileCount = 1;
                while (whileCount <= row)
                {
                    compReport.Row(whileCount).Height = 20;
                    whileCount++;
                }


                return excelPackage.GetAsByteArray();
            }
        }

        public ExcelRange GiveCellStyleProperties(ExcelRange cellObect)
        {
            cellObect.Style.Font.Color.SetColor(Color.Black);
            cellObect.Style.Font.Size = 11;
            cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cellObect.Style.Fill.BackgroundColor.SetColor(lightGray);
            cell.Style.Indent = 1;

            return cellObect;
        }

        public ExcelRange GiveCellStyleHeaderProperties(ExcelRange cellObect)
        {
            cellObect.Style.Font.Bold = true;
            cellObect.Style.Font.Color.SetColor(Color.White);
            cellObect.Style.Font.Size = 12;
            cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cellObect.Style.Fill.BackgroundColor.SetColor(darkBlue);

            return cellObect;
        }

        public ExcelRange GiveCellSubHeaderProperties(ExcelRange cellObect)
        {
            //cellObect.Style.Font.Bold = true;
            cellObect.Style.Font.Color.SetColor(Color.White);
            cellObect.Style.Font.Size = 12;
            cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cellObect.Style.Fill.BackgroundColor.SetColor(darkBlue);

            return cellObect;
        }

        public void MergeExcelFiles()
        {
            Workbook newbook = new Workbook();
            newbook.Version = ExcelVersion.Version2010;
            newbook.Worksheets.Clear();
            Workbook tempbook = new Workbook();
            string[] excelFiles = new String[] { "sample1.xlsx", "sample2.xlsx", "sample3.xlsx" };
            for (int i = 0; i < excelFiles.Length; i++)
            {
                //tempbook.LoadFromFile(excelFiles[i]);
                //tempbook.LoadFromFile()
                foreach (Worksheet sheet in tempbook.Worksheets)
                {
                    newbook.Worksheets.AddCopy(sheet);
                }
            }
        }
    }
}