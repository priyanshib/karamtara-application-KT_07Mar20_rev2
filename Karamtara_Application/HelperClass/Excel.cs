using Karamtara_Application.DAL;
using Karamtara_Application.DAL.Tender;
using Karamtara_Application.Models;
using Karamtara_Application.Models.Tender;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace Karamtara_Application.HelperClass
{

    public class Excel
    {
        #region variables

        ExcelRange cell;
        ExcelFill fill;
        Border border;
        RawMaterialPricingDAL rmpDAL;
        MarkupPricingDAL mDAL;
        FreightChargesDAL fDAL;
        TenderDetailsDAL tenderDetailsDAL;
        TestDAL testDAL;
        int whileCount = 0;
        Color lightGray = System.Drawing.ColorTranslator.FromHtml("#e9e9e9");
        Color darkBlue = System.Drawing.ColorTranslator.FromHtml("#105483");
        Color highlight = System.Drawing.ColorTranslator.FromHtml("#FFC300");
        Color darkGray = System.Drawing.ColorTranslator.FromHtml("#cbcbcb");
        int rowCount = 0;
        int colCount = 1;
        List<int> _revisions;
        #endregion

        public IntTenderDetailsDAL intDetailsDAL = new IntTenderDetailsDAL();

        public dynamic GetPropValue(object src, string propName)
        {
            try
            {
                return (dynamic)src.GetType().GetProperty(propName).GetValue(src, null);
            }
            catch
            {
                return string.Empty;
            }
        }

        #region domestic reports

        public byte[] DownloadTenderPricingData(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            decimal mt = 0, loadingFactor = 0;

            #region rawmData
            rmpDAL = new RawMaterialPricingDAL();
            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"SrNo"),
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
                new ColumnModel("Price", 10, 12,"Price")
            };
            #endregion

            #region testData
            testDAL = new TestDAL();
            var testMaster = testDAL.GetTestPricingList(tenderId, tenderRevId);
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 10, 12,"Price")
            };

            //var summation = testColumns.Where(x => x.Name != "Flag").ToList().Sum(x => x.Width);

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
            var bgColumns = new List<ColumnModel>() {

                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
            };

            var contractValue = Truncate(bgList.FirstOrDefault().ContractValue, 3);
            var deliveryMonth = Truncate(bgList.FirstOrDefault().DeliveryMonth, 3);
            var performancePeriod = Truncate(bgList.FirstOrDefault().PerformancePeriod, 3);
            var gracePeriod = Truncate(bgList.FirstOrDefault().GracePeriod, 3);

            #endregion

            #region freightData

            fDAL = new FreightChargesDAL();
            var freightMasterData = fDAL.GetFreightChargesList(bomId, bomRevId, tenderId, tenderRevId);
            var lineNames = new List<ColumnModel>();
            try
            {
                if (freightMasterData.Tables != null && freightMasterData.Tables.Count > 0)
                {
                    for (int i = 0; i < freightMasterData.Tables[0].Rows.Count; i++)
                    {
                        lineNames.Add(new ColumnModel(Convert.ToString(freightMasterData.Tables[0].Rows[i][0]), 60, 14, "") { SubColumns = new List<string>() { "Description", "Value" } });
                    }
                }
            }
            catch (Exception)
            {

            }

            #endregion

            #region markupData
            mDAL = new MarkupPricingDAL();
            var markupDataset = mDAL.GetMarkupPricingList(bomId, bomRevId, tenderId, tenderRevId);
            //var markupData
            var markupData = new List<MarkupDataModel>();
            var markupColumnList = new List<ColumnModel>();

            try
            {
                for (int i = 0; i < markupDataset.Tables[0].Columns.Count; i++)
                {
                    string colName = string.Empty;

                    if (markupDataset.Tables[0].Columns[i].ColumnName.ToLower().Contains("markupid"))
                        continue;
                    if (markupDataset.Tables[0].Columns[i].ColumnName.Contains("_"))
                    {
                        colName = (markupDataset.Tables[0].Columns[i].ColumnName.Substring(0, (markupDataset.Tables[0].Columns[i].ColumnName.LastIndexOf("_"))));
                        var id = Convert.ToInt32(markupDataset.Tables[0].Columns[i].ColumnName[markupDataset.Tables[0].Columns[i].ColumnName.Length - 1]);
                        markupColumnList.Add(new ColumnModel(colName, 20, 12, markupDataset.Tables[0].Columns[i].ColumnName) { });
                    }
                    else
                    {
                        colName = markupDataset.Tables[0].Columns[i].ColumnName;
                        markupColumnList.Add(new ColumnModel(colName, 40, 12, markupDataset.Tables[0].Columns[i].ColumnName, true, true) { });
                    }
                }
            }
            catch (Exception)
            {

            }


            #endregion

            #region final prices
            decimal gstPerc = 0;
            tenderDetailsDAL = new TenderDetailsDAL();
            var finalPriceData = tenderDetailsDAL.GetFinalPrices(tenderId, tenderRevId);
            List<FinalPriceModel> fpDataList = new List<FinalPriceModel>();
            var finalPriceColumns = new List<ColumnModel>()
            {
                new ColumnModel("Description", 30, 12,""),
                new ColumnModel("EXW", 30, 12,""),
                new ColumnModel("Freight", 30, 12,""),
                new ColumnModel("Gst", 30, 12,""),
                new ColumnModel("Total", 30, 12,""),
                new ColumnModel("Value in CR", 30, 12,""),
            };

            try
            {
                if (finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").Any())
                {
                    gstPerc = finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").FirstOrDefault().Values;
                }
                else
                {
                    gstPerc = 18;
                }

                foreach (var data in finalPriceData.TndLineValuesList)
                {
                    var currentModel = fpDataList.Where(x => x.LineId == data.LineId).Select(y => y).FirstOrDefault();
                    if (currentModel == null)
                    {
                        var fpData = new FinalPriceModel();
                        fpData.LineId = data.LineId;
                        fpData.LineName = data.LineName;
                        if (data.Description.ToLower() == "exworks")
                        {
                            fpData.ExWorks = data.Values;
                        }
                        else if (data.Description.ToLower() == "freight")
                        {
                            fpData.Freight = data.Values;
                        }
                        fpDataList.Add(fpData);
                    }
                    else
                    {
                        if (data.Description.ToLower() == "exworks")
                        {
                            currentModel.ExWorks = data.Values;
                        }
                        else if (data.Description.ToLower() == "freight")
                        {
                            currentModel.Freight = data.Values;
                        }
                    }
                }

                foreach (var mod in fpDataList)
                {
                    var total = mod.ExWorks + mod.Freight;
                    var gstPrice = total * gstPerc / 100;
                    mod.Gst = gstPrice;
                    mod.Total = total + gstPrice;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            #endregion

            #region tender pricing view

            tenderDetailsDAL = new TenderDetailsDAL();
            var mainViewData = tenderDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var lineListData = mainViewData.LineList ?? new List<LineStructure>();
            var lineCols = new List<string>();
            var mainColumns = new List<ColumnModel>();


            try
            {
                lineCols = lineListData.Select(y => y.LineName).ToList();
                lineCols.Add("Total");

                mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 20, 12, ""),
                new ColumnModel("Description", 60, 12, ""),
                new ColumnModel("Unit", 10, 12, ""),
                new ColumnModel("Quantity", 100, 12, "") { SubColumns = lineCols, CellMergeCount = lineCols.Count },
                new ColumnModel("Unit Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                new ColumnModel("Unit Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                new ColumnModel("Total Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                new ColumnModel("Total Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG" } },
                new ColumnModel("Unit Cost", 20, 12, "") { SubColumns = new List<string>() { "INR"} },
                };

                foreach (var data in lineListData)
                {
                    ColumnModel model = new ColumnModel(data.LineName, 90, 12, "") { SubColumns = new List<string>() { "Unit Cost", "Sales Cost", "Ex-Works", "Freight" } };
                    mainColumns.Add(model);
                }

                var pendingCols = new List<ColumnModel>()
            {
                new ColumnModel("Sales Cost", 20, 12, ""),
                new ColumnModel("Ex-Works", 20, 12, ""),
            };

                mainColumns.AddRange(pendingCols);

            }
            catch (Exception)
            {

            }

            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                for (int i = 1; i <= rawMatColumns.Count; i++)
                {
                    rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
                    rawMatPricing.Row(1).Height = 25;
                    cell = rawMatPricing.Cells[1, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = rawMatColumns[i - 1].Name;
                    cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
                }

                for (int i = 1; i <= rawMatMaster.Count; i++)
                {
                    for (int j = 1; j <= rawMatColumns.Count; j++)
                    {
                        cell = rawMatPricing.Cells[i + 1, j];
                        cell = GiveCellStyleProperties(cell);
                        rawMatPricing.Row(i + 1).Height = 20;
                        if (rawMatColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = string.Empty;
                        }
                    }
                }
                #endregion

                #region test master pricing

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";

                for (int i = 1; i <= testColumns.Count; i++)
                {
                    testPricing.Column(i).Width = testColumns[i - 1].Width;
                    testPricing.Row(1).Height = 25;
                    cell = testPricing.Cells[1, i];
                    cell.Value = testColumns[i - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);
                }

                for (int i = 1; i <= testMaster.TestList.Count; i++)
                {
                    for (int j = 1; j <= testColumns.Count; j++)
                    {
                        cell = testPricing.Cells[i + 1, j];
                        cell = GiveCellStyleProperties(cell);
                        if (testColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = string.Empty;
                        }
                    }
                }
                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                int bgRowCount = 1;
                int bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Contract Value : " + contractValue;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Delivery Month : " + deliveryMonth;
                cell = GiveCellStyleHeaderProperties(cell);
                bgRowCount++;
                bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
                cell.Value = "Grace Period : " + gracePeriod;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Performance Period : " + performancePeriod;
                cell = GiveCellStyleHeaderProperties(cell);

                bankGuaranteeSheet.Row(1).Height = 25;
                bankGuaranteeSheet.Row(2).Height = 25;
                bankGuaranteeSheet.Column(1).Width = 90;
                bankGuaranteeSheet.Column(3).Width = 90;

                for (int i = 1; i <= bgColumns.Count; i++)
                {
                    bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    cell = bankGuaranteeSheet.Cells[3, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = bgColumns[i - 1].Name;
                }

                for (int i = 1; i <= bgList.Count; i++)
                {
                    for (int j = 1; j <= bgColumns.Count; j++)
                    {
                        cell = bankGuaranteeSheet.Cells[i + 3, j];
                        cell = GiveCellStyleProperties(cell);

                        if (bgColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }
                bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
                cell = bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count];
                cell = GiveCellStyleProperties(cell);

                whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region freight

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";

                var colPos = 1;
                for (int l = 1; l <= lineNames.Count; l++, colPos += 3)
                {
                    freightSheet.Cells[1, colPos, 1, colPos + 1].Merge = true;
                    freightSheet.Column(colPos).Width = 40;
                    freightSheet.Column(colPos + 1).Width = 20;
                    freightSheet.Row(1).Height = 25;
                    cell = freightSheet.Cells[1, colPos];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = lineNames[l - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);

                    var innerColPos = colPos;
                    for (int k = 0; k < lineNames[l - 1].SubColumns.Count; k++, innerColPos++)
                    {
                        cell = freightSheet.Cells[2, innerColPos];
                        cell.Value = lineNames[l - 1].SubColumns[k];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.Size = 12;
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                    }
                    freightSheet.Row(2).Height = 20;
                }

                colPos = 1;
                int rowStart = 3;
                int maxRow = 0;
                for (int j = 1; j <= lineNames.Count; j++, colPos += 3)
                {
                    decimal totalWtofMaterial = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"]);
                    loadingFactor = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"]);
                    loadingFactor = loadingFactor == 0 ? 1.15m : loadingFactor;
                    decimal totalWtMt = (totalWtofMaterial * loadingFactor) / 1000;
                    string truckName = Convert.ToString(freightMasterData.Tables[0].Rows[j - 1]["TruckName"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TruckName"]);

                    List<Location> locations = new List<Location>();

                    for (int loc = 0; loc < freightMasterData.Tables[j].Rows.Count; loc++)
                    {
                        Location locModel = new Location();
                        locModel.LocationName = Convert.ToString(freightMasterData.Tables[j].Rows[loc]["Destinations"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Destinations"]);
                        locModel.Charge = Convert.ToDecimal(freightMasterData.Tables[j].Rows[loc]["Charges"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Charges"]);
                        locations.Add(locModel);
                    }

                    decimal avgfreight = GetAverage(locations.Select(x => (dynamic)x.Charge).ToList());
                    decimal maxfreight = locations.Any() ? locations.Max(x => x.Charge) : 0;
                    decimal freightConsidered = (avgfreight + maxfreight) / 2;
                    decimal ratePerTruck = Math.Round(freightConsidered);
                    decimal noOfTruck = Math.Ceiling(totalWtMt / 24);
                    //if (noOfTruck < 1 && noOfTruck > 0)
                    //    noOfTruck = 1;
                    decimal subtotalFreight = ratePerTruck * noOfTruck;
                    decimal contingencyFreight = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["Contingency"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["Contingency"]);
                    decimal totalFreight = subtotalFreight + ((subtotalFreight * contingencyFreight) / 100);

                    rowStart = 3;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Total Weight(MT)";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);

                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = totalWtMt.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Total Weight of Material(KG)";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = totalWtofMaterial.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Loading Factor";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = loadingFactor.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Truck Name";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = truckName;
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    if (locations.Count > 0)
                    {
                        //rowStart++;
                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Locations";
                        cell.Style.Indent = 1;
                        cell.Style.Font.Bold = true;
                        cell = GiveCellStyleHeaderProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        rowStart++;



                        foreach (var loc in locations)
                        {
                            cell = freightSheet.Cells[rowStart, colPos];
                            cell.Value = loc.LocationName;
                            cell.Style.Indent = 1;
                            cell = GiveCellStyleProperties(cell);
                            cell = freightSheet.Cells[rowStart, colPos + 1];
                            cell.Value = loc.Charge.ToString("N4");
                            cell.Style.Indent = 1;
                            cell = GiveCellStyleProperties(cell);
                            rowStart++;
                        }

                        //rowStart++;
                    }

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Average Freight";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = avgfreight.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Max Freight";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = maxfreight.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;


                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Freight Considered";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = freightConsidered.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Rate Per Truck";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = ratePerTruck.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "No. of Truck";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = noOfTruck.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Subtotal Freight";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = subtotalFreight.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Contingency on Freight";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = contingencyFreight.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    cell = freightSheet.Cells[rowStart, colPos];
                    cell.Value = "Total Freight";
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    cell = freightSheet.Cells[rowStart, colPos + 1];
                    cell.Value = totalFreight.ToString("N4");
                    cell.Style.Indent = 1;
                    cell = GiveCellStyleProperties(cell);
                    rowStart++;

                    maxRow = rowStart > maxRow ? rowStart : maxRow;

                    //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Fill.BackgroundColor.SetColor(lightGray);
                    //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                }

                int rowCounter = 2;
                while (rowCounter <= maxRow)
                {
                    freightSheet.Row(rowCounter).Height = 20;
                    rowCounter++;
                }

                #endregion

                #region markup
                if (markupDataset.Tables.Count > 0 && markupDataset.Tables[0] != null)
                {
                    var rowCount = markupDataset.Tables[0].Rows.Count;
                    foreach (var column in markupColumnList.Where(x => x.Exclude == false))
                    {
                        MarkupDataModel markup = new MarkupDataModel();
                        markup.Columns = new List<DynamicColumns>();
                        for (int r = 0; r < rowCount; r++)
                        {
                            markup.Columns.Add(new DynamicColumns
                            {
                                ColumnName = Convert.ToString(markupDataset.Tables[0].Rows[r]["Description"]),
                                Value = Convert.ToDecimal(markupDataset.Tables[0].Rows[r][column.PropName])
                            });
                        }
                        markup.SubTotal = Truncate(GetSum(markup.Columns.Select(x => x.Value).ToList()), 3);
                        markup.LineId = Convert.ToInt32(column.PropName.Substring(column.PropName.LastIndexOf("_")).Replace("_", ""));
                        markup.LineName = column.Name;
                        markupData.Add(markup);
                    }
                }

                if (markupDataset.Tables.Count > 2 && markupDataset.Tables[2] != null)
                {
                    for (int i = 0; i < markupDataset.Tables[2].Rows.Count; i++)
                    {
                        var lineId = Convert.ToInt32(markupDataset.Tables[2].Rows[i]["LineId"]);
                        var currentModel = markupData.Where(x => x.LineId == lineId).Select(y => y).FirstOrDefault();
                        currentModel.Testing = markupDataset.Tables[2].Columns.Contains("Testing") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Testing"]) : 0;
                        currentModel.OverideTestCharges = markupDataset.Tables[2].Columns.Contains("TestingOverride") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TestingOverride"]) : 0;
                        currentModel.TravelLodgingBoarding = markupDataset.Tables[2].Columns.Contains("TravelLB") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TravelLB"]) : 0;
                        currentModel.Development = markupDataset.Tables[2].Columns.Contains("Development") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Development"]) : 0;
                        currentModel.OtherTotal = currentModel.OverideTestCharges > 0 ? (currentModel.OverideTestCharges + currentModel.TravelLodgingBoarding + currentModel.Development) :
                            (currentModel.Testing + currentModel.TravelLodgingBoarding + currentModel.Development);
                        currentModel.PercentageToUnitCost = markupDataset.Tables[2].Columns.Contains("PercentUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["PercentUnitCost"]) : 0;
                        currentModel.LineUnitCost = markupDataset.Tables[2].Columns.Contains("LineUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["LineUnitCost"]) : 0;
                        currentModel.Margin = markupDataset.Tables[2].Columns.Contains("Margin") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Margin"]) : 0;
                        currentModel.FinalSubtotal = Truncate((1 + ((currentModel.SubTotal + currentModel.PercentageToUnitCost) / 100)), 4);
                    }

                    var markupExtraRows = new List<string>() {"Subtotal", "Testing", "Testing Override Charges", "Travel, Loding and Boarding", "Development", "Other Total",
                "Unit Cost", "Percentage to Unit Cost", "Subtotal", "Margin"};

                    var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                    markupSheet.Name = "Markup";
                    markupSheet.Column(1).Width = 40;

                    int markupRowCount = 1;
                    cell = markupSheet.Cells[markupRowCount, 1];
                    cell.Value = "Description";
                    cell = GiveCellStyleHeaderProperties(cell);
                    markupRowCount++;

                    foreach (var col in markupData.FirstOrDefault().Columns)
                    {
                        cell = markupSheet.Cells[markupRowCount, 1];
                        cell.Value = col.ColumnName;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;
                    }

                    foreach (var col in markupExtraRows)
                    {
                        cell = markupSheet.Cells[markupRowCount, 1];
                        cell.Value = col;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;
                    }

                    int markupWhileCount = 1;
                    while (markupWhileCount < markupRowCount)
                    {
                        markupSheet.Row(markupWhileCount).Height = 20;
                        markupWhileCount++;
                    }

                    int markupColCount = 2;
                    foreach (var markup in markupData)
                    {
                        markupRowCount = 1;
                        markupSheet.Column(markupColCount).Width = 20;
                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.LineName;
                        cell = GiveCellStyleHeaderProperties(cell);
                        markupRowCount++;

                        foreach (var dynCols in markup.Columns)
                        {
                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = dynCols.Value;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;
                        }

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.SubTotal;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.Testing;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.OverideTestCharges;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.TravelLodgingBoarding;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.Development;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.OtherTotal;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.LineUnitCost;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.PercentageToUnitCost;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.FinalSubtotal;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount];
                        cell.Value = markup.Margin;
                        cell = GiveCellStyleProperties(cell);
                        markupRowCount++;

                        markupColCount++;
                    }
                }

                #endregion

                #region finalPrices
                var finalPriceSheet = excelPackage.Workbook.Worksheets.Add("Final Price Sheet");
                finalPriceSheet.Name = "Final Price";

                int finalPriceRowCount = 1;
                int finalPriceColCount = 1;
                foreach (var col in finalPriceColumns)
                {
                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    if (col.Name.ToLower() == "gst")
                    {
                        cell.Value = col.Name + " @" + gstPerc + "%";
                    }
                    else
                        cell.Value = col.Name;
                    cell = GiveCellStyleHeaderProperties(cell);
                    finalPriceRowCount++;

                    finalPriceSheet.Row(finalPriceColCount).Height = 20;
                }
                finalPriceSheet.Column(finalPriceColCount).Width = 30;

                foreach (var model in fpDataList)
                {
                    finalPriceRowCount = 1;
                    finalPriceColCount += 1;

                    finalPriceSheet.Column(finalPriceColCount).Width = 30;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = model.LineName;
                    cell = GiveCellStyleHeaderProperties(cell);
                    finalPriceRowCount++;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = model.ExWorks.ToString("N4");
                    cell = GiveCellStyleProperties(cell);
                    finalPriceRowCount++;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = model.Freight.ToString("N4");
                    cell = GiveCellStyleProperties(cell);
                    finalPriceRowCount++;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = model.Gst.ToString("N4");
                    cell = GiveCellStyleProperties(cell);
                    finalPriceRowCount++;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = model.Total.ToString("N4");
                    cell = GiveCellStyleProperties(cell);
                    finalPriceRowCount++;

                    cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                    cell.Value = GetValueInCrores(model.Total).ToString("N4");
                    cell = GiveCellStyleProperties(cell);
                    finalPriceRowCount++;
                }

                finalPriceRowCount = 1;
                finalPriceColCount += 1;
                finalPriceSheet.Column(finalPriceColCount).Width = 30;
                foreach (var col in finalPriceColumns)
                {
                    if (col.Name.ToLower() == "description")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = "Total";
                        cell = GiveCellStyleHeaderProperties(cell);
                        finalPriceRowCount++;
                    }
                    else if (col.Name.ToLower() == "exw")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.ExWorks).ToList()).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }
                    else if (col.Name.ToLower() == "freight")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Freight).ToList()).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }
                    else if (col.Name.ToLower() == "gst")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Gst).ToList()).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }
                    else if (col.Name.ToLower() == "total")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList()).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }
                    else if (col.Name.ToLower() == "value in cr")
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetValueInCrores(GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList())).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }
                }

                #endregion

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                FillCommonFields(ref range, mainViewData);

                int tenderPricingRowCount = 9;
                int tenderPricingColCount = 1;
                var innerColCount = 1;
                foreach (var data in mainColumns)
                {
                    tenderPricingColCount = innerColCount;
                    if (data.SubColumns != null && data.SubColumns.Count > 0)
                    {
                        tenderPricing.Cells[tenderPricingRowCount, innerColCount, tenderPricingRowCount, innerColCount + data.SubColumns.Count - 1].Merge = true;

                        foreach (var sub in data.SubColumns)
                        {
                            cell = tenderPricing.Cells[tenderPricingRowCount + 1, innerColCount];
                            tenderPricing.Column(innerColCount).Width = Convert.ToInt32(data.Width / data.SubColumns.Count);
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = sub;
                            innerColCount++;
                        }
                        innerColCount--;
                    }

                    cell = tenderPricing.Cells[tenderPricingRowCount, tenderPricingColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = data.Name;
                    innerColCount++;
                    if (data.SubColumns == null)
                    {
                        tenderPricing.Column(tenderPricingColCount).Width = data.Width;
                        cell = tenderPricing.Cells[tenderPricingRowCount + 1, tenderPricingColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                    }

                }
                tenderPricing.Row(tenderPricingRowCount).Height = 20;

                List<TotalRowModel> totalList = new List<TotalRowModel>() {
                        new  TotalRowModel(){LineId = 0, Key ="UnitCost",Value=0}
                    };

                var masterData = mainViewData.MasterList;
                if (masterData != null && masterData.Count > 0)
                {
                    var lineList = lineListData;
                    var lineQtyList = mainViewData.LineQtyList;
                    var tenderLineValues = mainViewData.TndLineValuesList;

                    foreach (var line in lineList)
                    {
                        totalList.Add(new TotalRowModel() { LineId = line.LineId, UnitCost = 0, Freight = 0, SalesCost = 0, ExWorks = 0, Key = "" });
                        foreach (var master in masterData)
                        {
                            if (master.LineDetails != null)
                            {
                                master.LineDetails.Add(new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName,
                                });
                            }
                            else
                            {
                                master.LineDetails = new List<MasterLineModel>()
                            {
                                new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName
                                }};
                            }
                        }
                    }
                    totalList.Add(new TotalRowModel() { LineId = 0, Key = "SalesCost", Value = 0 });
                    totalList.Add(new TotalRowModel() { LineId = 0, Key = "ExWorks", Value = 0 });

                    int id, productId, type, pgId, assmId, subAssmId, compId, mainLineId, qty;
                    id = productId = type = pgId = assmId = subAssmId = compId = mainLineId = qty = 0;

                    foreach (var master in lineQtyList)
                    {
                        if (CheckIfPropertyExistsInDynamicObject(master, "LineId"))
                            mainLineId = GetPropertyValueFromDynamicObject(master, "LineId");

                        //if (CheckIfPropertyExistsInDynamicObject(master, "Id"))
                        //    id = GetPropertyValueFromDynamicObject(master, "Id");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ProductId"))
                            productId = GetPropertyValueFromDynamicObject(master, "ProductId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "Type"))
                            type = GetPropertyValueFromDynamicObject(master, "Type");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ProductGroupId"))
                            pgId = GetPropertyValueFromDynamicObject(master, "ProductGroupId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "AssemblyId"))
                            assmId = GetPropertyValueFromDynamicObject(master, "AssemblyId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "SubAssemblyId"))
                            subAssmId = GetPropertyValueFromDynamicObject(master, "SubAssemblyId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ComponentId"))
                            compId = GetPropertyValueFromDynamicObject(master, "ComponentId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "Quantity"))
                            qty = GetPropertyValueFromDynamicObject(master, "Quantity");

                        var currentModel = masterData.Where(x => x.Id == productId && x.Type == type && x.ProductGroupId == pgId
                        && x.SubAssemblyId == subAssmId && x.AssemblyId == assmId && x.ComponentId == compId).Select(y => y).FirstOrDefault();

                        if (currentModel != null)
                        {
                            var finalTotal = tenderLineValues.Where(x => x.Description == "FinalTotal" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                            //var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                            var unitCost = currentModel.UnitCost;
                            var totalUnitCost = currentModel.UnitCost * qty;
                            var salesCost = totalUnitCost * finalTotal;
                            var margin = tenderLineValues.Where(x => x.Description == "Margin" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                            var exWorks = salesCost / (1 - (margin / 100));
                            var freight = 1;
                            currentModel.LineDetails.Where(x => x.LineId == mainLineId).ToList().ForEach(x => { x.Quantity = qty; x.SalesCost = salesCost; x.ExWorks = exWorks; x.Freight = freight; });
                        }
                    }

                    int rC = 11;
                    int cC = 1;
                    int unitCostColNo = 0;

                    foreach (var mod in masterData)
                    {
                        cC = 1;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.SrNo;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.Name;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.Unit;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        foreach (var line in mod.LineDetails)
                        {
                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = line.Quantity;
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.LineDetails.Sum(x => x.Quantity);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitGrWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitNetWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.TotalCalcUnitGrWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.TotalUnitNetWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitCost;
                        cell = GiveCellStyleProperties(cell);
                        unitCostColNo = cC;
                        cC++;

                        if (mod.Type != 1)
                        {
                            totalList.Where(y => y.Key == "UnitCost").FirstOrDefault().Value += mod.UnitCost;
                        }

                        foreach (var line in mod.LineDetails)
                        {
                            var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Math.Round(line.Quantity * mod.UnitCost);
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                            totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost += Math.Round(line.Quantity * mod.UnitCost);

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(line.SalesCost, 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                            totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight += Truncate(line.SalesCost, 3);

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(line.ExWorks, 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                            totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost += Truncate(line.ExWorks, 3);

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                            totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks += Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);

                        rC++;
                    }
                    cC = unitCostColNo;

                    cell = tenderPricing.Cells[rC, cC];
                    cell.Value = totalList.Where(x => x.Key == "UnitCost").FirstOrDefault().Value;
                    cell = GiveCellStyleProperties(cell);
                    cC++;
                    foreach (var line in lineList)
                    {
                        var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks;
                        cell = GiveCellStyleProperties(cell);
                        cC++;
                    }

                    cell = tenderPricing.Cells[rC, cC];
                    cell.Value = totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value;
                    cell = GiveCellStyleProperties(cell);
                    cC++;

                    cell = tenderPricing.Cells[rC, cC];
                    cell.Value = totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value;
                    cell = GiveCellStyleProperties(cell);
                    cC++;

                    tenderPricing.Cells[rC, unitCostColNo - 1].Merge = true;
                    cell = tenderPricing.Cells[rC, unitCostColNo - 1];
                    cell.Value = "Total";
                }

                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                cell = GiveLastRowHighlightProperties(cell);

                for (int i = 1; i <= tenderPricing.Dimension.End.Column; i++)
                {
                    tenderPricing.Column(i).AutoFit(20);
                }

                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] DownloadTenderPricingCustomerDataG(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            //int tenderRevId = 0;

            #region tender pricing view

            tenderDetailsDAL = new TenderDetailsDAL();
            var mainViewData = tenderDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var lineListData = mainViewData.LineList ?? new List<LineStructure>();
            var lineCols = new List<string>();
            var mainColumns = new List<ColumnModel>();

            try
            {
                lineCols = lineListData.Select(y => y.LineName).ToList();
                lineCols.Add("Total");

                mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, ""),
                new ColumnModel("Description", 60, 12, ""),
                new ColumnModel("Unit", 10, 12, ""),
                new ColumnModel("Quantity", 100, 12, "") { SubColumns = lineCols, CellMergeCount = lineCols.Count },
                new ColumnModel("Unit Gr.Wt", 15, 12, "",true,true) { SubColumns = new List<string>() { "KG"}, },
                new ColumnModel("Unit Net.Wt", 15, 12, "",true,true) { SubColumns = new List<string>() { "KG"} },
                new ColumnModel("Total Gr.Wt", 15, 12, "",true,true) { SubColumns = new List<string>() { "KG"} },
                new ColumnModel("Total Net.Wt", 15, 12, "",true,true) { SubColumns = new List<string>() { "KG" } },
                new ColumnModel("Unit Cost", 20, 12, "") { SubColumns = new List<string>() { "INR"} },
                };

                foreach (var data in lineListData)
                {
                    ColumnModel model = new ColumnModel(data.LineName, 90, 12, "", true, true) { SubColumns = new List<string>() { "Unit Cost", "Sales Cost", "Ex-Works", "Freight" } };
                    mainColumns.Add(model);
                }

                var pendingCols = new List<ColumnModel>()
            {
                new ColumnModel("Sales Cost", 20, 12, ""),
                new ColumnModel("Ex-Works", 20, 12, ""),
                new ColumnModel("Freight", 20, 12, "")
            };

                mainColumns.AddRange(pendingCols);

            }
            catch (Exception)
            {

            }

            //int index = mainColumns.IndexOf(mainColumns.Where(x => x.Name.ToLower() == "unit cost").Select(y => y).FirstOrDefault());
            //foreach (var data in mainViewDat.LineList)
            //{
            //    ColumnModel model = new ColumnModel(data.LineName, 60, 12, "");
            //    mainColumns.Insert(index + 1, model);
            //    index++;
            //}

            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                int tenderPricingRowCount = 1;
                int tenderPricingColCount = 1;
                var innerColCount = 1;
                foreach (var data in mainColumns)
                {
                    tenderPricingColCount = innerColCount;
                    if (data.SubColumns != null && data.SubColumns.Count > 0)
                    {
                        tenderPricing.Cells[tenderPricingRowCount, innerColCount, tenderPricingRowCount, innerColCount + data.SubColumns.Count - 1].Merge = true;

                        foreach (var sub in data.SubColumns)
                        {
                            cell = tenderPricing.Cells[tenderPricingRowCount + 1, innerColCount];
                            tenderPricing.Column(innerColCount).Width = Convert.ToInt32(data.Width / data.SubColumns.Count);
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = sub;
                            innerColCount++;
                        }
                        innerColCount--;
                    }

                    cell = tenderPricing.Cells[tenderPricingRowCount, tenderPricingColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = data.Name;
                    innerColCount++;
                    if (data.SubColumns == null)
                    {
                        tenderPricing.Column(tenderPricingColCount).Width = data.Width;
                        cell = tenderPricing.Cells[tenderPricingRowCount + 1, tenderPricingColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                    }

                }
                tenderPricing.Row(tenderPricingRowCount).Height = 20;

                var masterData = mainViewData.MasterList;
                if (masterData != null && masterData.Count > 0)
                {
                    var lineList = lineListData;
                    var lineQtyList = mainViewData.LineQtyList;
                    var tenderLineValues = mainViewData.TndLineValuesList;

                    foreach (var line in lineList)
                    {
                        foreach (var master in masterData)
                        {
                            if (master.LineDetails != null)
                            {
                                master.LineDetails.Add(new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName,
                                });
                            }
                            else
                            {
                                master.LineDetails = new List<MasterLineModel>()
                            {
                                new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName
                                }};
                            }
                        }
                    }

                    int id, productId, type, pgId, assmId, subAssmId, compId, mainLineId, qty;
                    id = productId = type = pgId = assmId = subAssmId = compId = mainLineId = qty = 0;

                    foreach (var master in lineQtyList)
                    {
                        if (CheckIfPropertyExistsInDynamicObject(master, "LineId"))
                            mainLineId = GetPropertyValueFromDynamicObject(master, "LineId");

                        //if (CheckIfPropertyExistsInDynamicObject(master, "Id"))
                        //    id = GetPropertyValueFromDynamicObject(master, "Id");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ProductId"))
                            productId = GetPropertyValueFromDynamicObject(master, "ProductId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "Type"))
                            type = GetPropertyValueFromDynamicObject(master, "Type");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ProductGroupId"))
                            pgId = GetPropertyValueFromDynamicObject(master, "ProductGroupId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "AssemblyId"))
                            assmId = GetPropertyValueFromDynamicObject(master, "AssemblyId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "SubAssemblyId"))
                            subAssmId = GetPropertyValueFromDynamicObject(master, "SubAssemblyId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "ComponentId"))
                            compId = GetPropertyValueFromDynamicObject(master, "ComponentId");

                        if (CheckIfPropertyExistsInDynamicObject(master, "Quantity"))
                            qty = GetPropertyValueFromDynamicObject(master, "Quantity");

                        var currentModel = masterData.Where(x => x.Id == productId && x.Type == type && x.ProductGroupId == pgId
                        && x.SubAssemblyId == subAssmId && x.AssemblyId == assmId && x.ComponentId == compId).Select(y => y).FirstOrDefault();

                        if (currentModel != null)
                        {
                            var finalTotal = tenderLineValues.Where(x => x.Description == "FinalTotal" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                            var unitCost = currentModel.UnitCost;
                            var totalUnitCost = currentModel.UnitCost * qty;
                            var salesCost = totalUnitCost * finalTotal;
                            var margin = tenderLineValues.Where(x => x.Description == "Margin" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                            var exWorks = salesCost / (1 - (margin / 100));
                            var freight = 1;
                            currentModel.LineDetails.Where(x => x.LineId == mainLineId).ToList().ForEach(x => { x.Quantity = qty; x.SalesCost = salesCost; x.ExWorks = exWorks; x.Freight = freight; });
                        }
                    }

                    int rC = 3;
                    int cC = 1;
                    foreach (var mod in masterData)
                    {
                        cC = 1;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.SrNo;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.Name;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.Unit;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        foreach (var line in mod.LineDetails)
                        {
                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = line.Quantity;
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.LineDetails.Sum(x => x.Quantity);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitGrWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitNetWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.TotalCalcUnitGrWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.TotalUnitNetWt;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.UnitCost;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        foreach (var line in mod.LineDetails)
                        {
                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Math.Round(line.Quantity * mod.UnitCost);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = line.SalesCost;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = line.ExWorks;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = line.Freight;
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = mod.LineDetails.Max(x => x.ExWorks);
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        //cell = tenderPricing.Cells[rC, cC];
                        //cell.Value = "Freight Pending";
                        //cell = GiveCellStyleProperties(cell);
                        //cC++;

                        rC++;
                    }
                    tenderPricing.DeleteColumn(5 + lineList.Count, 4);
                    tenderPricing.DeleteColumn(6 + lineList.Count, lineList.Count * 4);
                    tenderPricing.DeleteColumn(tenderPricing.Dimension.End.Column);
                    tenderPricing.DeleteColumn(tenderPricing.Dimension.End.Column);
                    //tenderPricing.DeleteColumn(3);
                }
                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] DomTenderCompareRevision(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            decimal loadingFactor = 0;
            int maxRow = 0, rowStart = 1;

            #region revisions
            _revisions = tndDetailsDAL.GetRevisionIds(tenderId);
            #endregion

            #region rawm Data

            rmpDAL = new RawMaterialPricingDAL();
            var revisions = new List<int>();
            var rawMatMaster = rmpDAL.GetRawMaterialPricingListForTender(tenderId, out revisions);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
            };

            revisions.ForEach(x => { rawMatColumns.Add(new ColumnModel(string.Format("Revision {0} Price (Rs)", x), 25, 12, x.ToString())); });

            #endregion

            #region testData
            //    //testDAL = new TestDAL();
            //    //var testMaster = testDAL.GetTestPricingList();
            //    //var testColumns = new List<ColumnModel>() {
            //    //    new ColumnModel("Sr.No", 8, 12,"Id", false),
            //    //    new ColumnModel("Name", 30, 12,"TestName"),
            //    //    new ColumnModel("Description", 50, 12,"TestDescription"),
            //    //    new ColumnModel("Group Type", 30, 12,"Type"),
            //    //    new ColumnModel("Bundle Type", 30, 12,"Bundle"),
            //    //    new ColumnModel("Line Type", 30, 12,"KVLine"),
            //    //    new ColumnModel("UTS", 30, 12,"UTS"),
            //    //    new ColumnModel("Summary", 50, 12,"Summary"),
            //    //    new ColumnModel("Price", 10, 12,"Price")
            //    //};

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGDataForAllRevisions(tenderId);

            #endregion

            #region testData
            testDAL = new TestDAL();
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 10, 12,"Price")
            };

            #endregion

            using (var excelPackage = new ExcelPackage())
            {

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                rowCount = 1;
                colCount = 1;
                foreach (var col in rawMatColumns)
                {
                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = col.Name;
                    rawMatPricing.Column(colCount).Width = col.Width;
                    colCount++;
                }

                rowCount++;
                foreach (var rawMat in rawMatMaster)
                {
                    colCount = 1;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.RawMaterialName;
                    colCount++;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.GroupName;
                    colCount++;

                    foreach (var rev in rawMat.Pricing)
                    {
                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rev.Price;
                        colCount++;
                    }
                    rowCount++;
                }

                whileCount = 1;
                while (whileCount <= rawMatPricing.Dimension.End.Row)
                {
                    rawMatPricing.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region test master pricing

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";
                int rowNum = 1;
                foreach (var revNo in revisions)
                {
                    var testMaster = testDAL.GetTestPricingList(tenderId, revNo);

                    for (int i = 1; i <= testColumns.Count; i++)
                    {
                        testPricing.Column(i).Width = testColumns[i - 1].Width;
                        testPricing.Row(rowNum).Height = 25;
                        cell = testPricing.Cells[rowNum, i];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);
                    }
                    for (int i = 1; i <= testMaster.TestList.Count; i++)
                    {
                        for (int j = 1; j <= testColumns.Count; j++)
                        {
                            cell = testPricing.Cells[rowNum + 1, j];
                            cell = GiveCellStyleProperties(cell);
                            if (testColumns[j - 1].UseValue)
                            {
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                            }
                            else
                            {
                                cell.Value = string.Empty;
                            }
                        }
                        rowNum++;
                    }

                    rowNum = testMaster.TestList.Count + 5;
                }
                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                whileCount = 1;
                while (whileCount <= 5)
                {
                    bankGuaranteeSheet.Column(whileCount).Width = 40;
                    whileCount++;
                }

                rowCount = 1;
                foreach (var bgModel in bgList)
                {
                    if (bgModel.Count > 0)
                    {
                        colCount = 1;

                        var bgVerticalCols = new List<ColumnModel>() {
                    new ColumnModel(string.Format("Revision {0}", bgModel.FirstOrDefault().TenderRevisionNo), 40, 12,""),
                    new ColumnModel("Bank Guarantee Type", 30, 12,""),
                    new ColumnModel("Bank Guarantee Month", 30, 12,""),
                    new ColumnModel("Commision (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee Amount", 30, 12,""),
                    new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"")
                    };

                        var bghorizontalCols = new List<ColumnModel>()
                    {
                    new ColumnModel("Advance BG", 30, 12,""),
                    new ColumnModel("Performance BG", 30, 12,""),
                    new ColumnModel("Retension BG", 30, 12,""),
                    new ColumnModel("Total", 30, 12,"")
                    };

                        var contractValue = Truncate(bgModel.FirstOrDefault().ContractValue, 3);
                        var deliveryMonth = Truncate(bgModel.FirstOrDefault().DeliveryMonth, 3);
                        var performancePeriod = Truncate(bgModel.FirstOrDefault().PerformancePeriod, 3);
                        var gracePeriod = Truncate(bgModel.FirstOrDefault().GracePeriod, 3);

                        var bgHorizontalTopColumns = new List<ColumnModel>()
                    {
                    new ColumnModel("Contract Value", 30, 12,""){ Value = contractValue},
                    new ColumnModel("Performance Period", 30, 12,""){ Value = performancePeriod},
                    new ColumnModel("Grace Period", 30, 12,""){ Value = gracePeriod},
                    new ColumnModel("Delivery Month", 30, 12,""){ Value = deliveryMonth}
                    };

                        int innerCount = 0;
                        foreach (var col in bgVerticalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount + innerCount, colCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        colCount++;

                        innerCount = 0;
                        foreach (var col in bgHorizontalTopColumns)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = string.Format("{0} : {1}", col.Name, col.Value);
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var col in bghorizontalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgMonth in bgModel.Select(x => x.BGMonth))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgMonth;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGMonth);
                        rowCount++;

                        innerCount = 0;
                        foreach (var commisionPer in bgModel.Select(x => x.CommisionPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = commisionPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.CommisionPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgpercent in bgModel.Select(x => x.BGPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgpercent;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgAmount in bgModel.Select(x => x.BGAmount))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgAmount;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGAmount);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgCostPer in bgModel.Select(x => x.BGCostPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgCostPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGCostPercentage);
                        rowCount += 2;
                    }
                }

                whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region freightData

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";

                foreach (int revNo in revisions)
                {

                    fDAL = new FreightChargesDAL();
                    var freightMasterData = fDAL.GetFreightChargesList(bomId, bomRevId, tenderId, revNo);
                    var lineNames = new List<ColumnModel>();
                    try
                    {
                        if (freightMasterData.Tables != null && freightMasterData.Tables.Count > 0)
                        {
                            for (int i = 0; i < freightMasterData.Tables[0].Rows.Count; i++)
                            {
                                lineNames.Add(new ColumnModel(Convert.ToString(freightMasterData.Tables[0].Rows[i][0]), 60, 14, "") { SubColumns = new List<string>() { "Description", "Value" } });
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }


                    rowStart += 2;
                    var colPos = 1;
                    for (int l = 1; l <= lineNames.Count; l++, colPos += 3)
                    {
                        freightSheet.Cells[rowStart, colPos, rowStart, colPos + 1].Merge = true;
                        freightSheet.Column(colPos).Width = 40;
                        freightSheet.Column(colPos + 1).Width = 20;
                        freightSheet.Row(rowStart).Height = 25;
                        cell = freightSheet.Cells[rowStart, colPos];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = lineNames[l - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);
                        // rowStart++;

                        var innerColPos = colPos;
                        for (int k = 0; k < lineNames[l - 1].SubColumns.Count; k++, innerColPos++)
                        {
                            cell = freightSheet.Cells[rowStart + 1, innerColPos];
                            cell.Value = lineNames[l - 1].SubColumns[k];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Size = 12;
                            cell.Style.Indent = 1;
                            cell = GiveCellSubHeaderProperties(cell);
                        }
                        freightSheet.Row(rowStart).Height = 20;
                    }
                    freightSheet.Cells[rowStart - 2, 1, rowStart - 2, (lineNames.Count * 2) + (lineNames.Count - 1)].Merge = true;
                    cell = freightSheet.Cells[rowStart - 2, 1, rowStart - 2, (lineNames.Count * 2) + (lineNames.Count - 1)];
                    cell.Value = "Revision " + revNo;
                    cell = GiveCellStyleHeaderProperties(cell);
                    rowStart++;

                    colPos = 1;
                    int prevRowCount = rowStart;

                    maxRow = 0;
                    for (int j = 1; j <= lineNames.Count; j++, colPos += 3)
                    {
                        decimal totalWtofMaterial = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"]);
                        loadingFactor = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"]);
                        loadingFactor = loadingFactor == 0 ? 1.15m : loadingFactor;
                        decimal totalWtMt = (totalWtofMaterial * loadingFactor) / 1000;
                        string truckName = Convert.ToString(freightMasterData.Tables[0].Rows[j - 1]["TruckName"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TruckName"]);

                        List<Location> locations = new List<Location>();

                        for (int loc = 0; loc < freightMasterData.Tables[j].Rows.Count; loc++)
                        {
                            Location locModel = new Location();
                            locModel.LocationName = Convert.ToString(freightMasterData.Tables[j].Rows[loc]["Destinations"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Destinations"]);
                            locModel.Charge = Convert.ToDecimal(freightMasterData.Tables[j].Rows[loc]["Charges"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Charges"]);
                            locations.Add(locModel);
                        }

                        decimal avgfreight = GetAverage(locations.Select(x => (dynamic)x.Charge).ToList());
                        decimal maxfreight = locations.Any() ? locations.Max(x => x.Charge) : 0;
                        decimal freightConsidered = (avgfreight + maxfreight) / 2;
                        decimal ratePerTruck = Math.Round(freightConsidered);
                        decimal noOfTruck = Math.Ceiling(totalWtMt / 24);
                        //if (noOfTruck < 1 && noOfTruck > 0)
                        //    noOfTruck = 1;
                        decimal subtotalFreight = ratePerTruck * noOfTruck;
                        decimal contingencyFreight = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["Contingency"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["Contingency"]);
                        decimal totalFreight = subtotalFreight + ((subtotalFreight * contingencyFreight) / 100);

                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Weight(MT)";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalWtMt.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Weight of Material(KG)";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalWtofMaterial.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Loading Factor";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = loadingFactor.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Truck Name";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = truckName;
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        if (locations.Count > 0)
                        {
                            cell = freightSheet.Cells[rowStart, colPos];
                            cell.Value = "Locations";
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            cell = GiveCellStyleHeaderProperties(cell);

                            cell = freightSheet.Cells[rowStart, colPos + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            rowStart++;

                            foreach (var loc in locations)
                            {
                                cell = freightSheet.Cells[rowStart, colPos];
                                cell.Value = loc.LocationName;
                                cell.Style.Indent = 1;
                                cell = GiveCellStyleProperties(cell);

                                cell = freightSheet.Cells[rowStart, colPos + 1];
                                cell.Value = loc.Charge.ToString("N4");
                                cell.Style.Indent = 1;
                                cell = GiveCellStyleProperties(cell);
                                rowStart++;
                            }

                            rowStart++;
                        }
                        rowStart--;
                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Average Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = avgfreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Max Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = maxfreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;


                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Freight Considered";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = freightConsidered.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Rate Per Truck";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = ratePerTruck.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "No. of Truck";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = noOfTruck.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Subtotal Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = subtotalFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Contingency on Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = contingencyFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        maxRow = rowStart > maxRow ? rowStart : maxRow;
                        rowStart = prevRowCount;
                        //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Fill.BackgroundColor.SetColor(lightGray);
                        //freightSheet.Cells[2, colPos, rowStart, colPos + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    }

                    //rowStart++;

                    rowStart = freightSheet.Dimension.End.Row + 2;

                }
                int rowCounter = 2;
                while (rowCounter <= maxRow)
                {
                    freightSheet.Row(rowCounter).Height = 20;
                    rowCounter++;
                }

                #endregion

                #region markup
                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                markupSheet.Name = "Markup";
                markupSheet.Column(1).Width = 40;

                int markupRowCount = 1;
                mDAL = new MarkupPricingDAL();
                foreach (var revNo in revisions)
                {
                    var markupDataset = mDAL.GetMarkupPricingList(bomId, bomRevId, tenderId, revNo);
                    //var markupData
                    var markupData = new List<MarkupDataModel>();
                    var markupColumnList = new List<ColumnModel>();

                    try
                    {
                        for (int i = 0; i < markupDataset.Tables[0].Columns.Count; i++)
                        {
                            string colName = string.Empty;

                            if (markupDataset.Tables[0].Columns[i].ColumnName.ToLower().Contains("markupid"))
                                continue;
                            if (markupDataset.Tables[0].Columns[i].ColumnName.Contains("_"))
                            {
                                colName = (markupDataset.Tables[0].Columns[i].ColumnName.Substring(0, (markupDataset.Tables[0].Columns[i].ColumnName.LastIndexOf("_"))));
                                var id = Convert.ToInt32(markupDataset.Tables[0].Columns[i].ColumnName[markupDataset.Tables[0].Columns[i].ColumnName.Length - 1]);
                                markupColumnList.Add(new ColumnModel(colName, 20, 12, markupDataset.Tables[0].Columns[i].ColumnName) { });
                            }
                            else
                            {
                                colName = markupDataset.Tables[0].Columns[i].ColumnName;
                                markupColumnList.Add(new ColumnModel(colName, 40, 12, markupDataset.Tables[0].Columns[i].ColumnName, true, true) { });
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    if (markupDataset.Tables.Count > 0 && markupDataset.Tables[0] != null)
                    {
                        var rowCount = markupDataset.Tables[0].Rows.Count;
                        foreach (var column in markupColumnList.Where(x => x.Exclude == false))
                        {
                            MarkupDataModel markup = new MarkupDataModel();
                            markup.Columns = new List<DynamicColumns>();
                            for (int r = 0; r < rowCount; r++)
                            {
                                markup.Columns.Add(new DynamicColumns
                                {
                                    ColumnName = Convert.ToString(markupDataset.Tables[0].Rows[r]["Description"]),
                                    Value = Convert.ToDecimal(markupDataset.Tables[0].Rows[r][column.PropName])
                                });
                            }
                            markup.SubTotal = Truncate(GetSum(markup.Columns.Select(x => x.Value).ToList()), 3);
                            markup.LineId = Convert.ToInt32(column.PropName.Substring(column.PropName.LastIndexOf("_")).Replace("_", ""));
                            markup.LineName = column.Name;
                            markupData.Add(markup);
                        }
                    }

                    if (markupDataset.Tables.Count > 2 && markupDataset.Tables[2] != null)
                    {
                        for (int i = 0; i < markupDataset.Tables[2].Rows.Count; i++)
                        {
                            var lineId = Convert.ToInt32(markupDataset.Tables[2].Rows[i]["LineId"]);
                            var currentModel = markupData.Where(x => x.LineId == lineId).Select(y => y).FirstOrDefault();
                            currentModel.Testing = markupDataset.Tables[2].Columns.Contains("Testing") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Testing"]) : 0;
                            currentModel.OverideTestCharges = markupDataset.Tables[2].Columns.Contains("TestingOverride") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TestingOverride"]) : 0;
                            currentModel.TravelLodgingBoarding = markupDataset.Tables[2].Columns.Contains("TravelLB") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TravelLB"]) : 0;
                            currentModel.Development = markupDataset.Tables[2].Columns.Contains("Development") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Development"]) : 0;
                            currentModel.OtherTotal = currentModel.OverideTestCharges > 0 ? (currentModel.OverideTestCharges + currentModel.TravelLodgingBoarding + currentModel.Development) :
                                (currentModel.Testing + currentModel.TravelLodgingBoarding + currentModel.Development);
                            currentModel.PercentageToUnitCost = markupDataset.Tables[2].Columns.Contains("PercentUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["PercentUnitCost"]) : 0;
                            currentModel.LineUnitCost = markupDataset.Tables[2].Columns.Contains("LineUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["LineUnitCost"]) : 0;
                            currentModel.Margin = markupDataset.Tables[2].Columns.Contains("Margin") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Margin"]) : 0;
                            currentModel.FinalSubtotal = Truncate((1 + ((currentModel.SubTotal + currentModel.PercentageToUnitCost) / 100)), 4);
                        }

                        var markupExtraRows = new List<string>() {"Subtotal", "Testing", "Testing Override Charges", "Travel, Loding and Boarding", "Development", "Other Total",
                "Unit Cost", "Percentage to Unit Cost", "Subtotal", "Margin"};

                        markupSheet.Cells[markupRowCount, 1, markupRowCount, 1 + markupData.Count].Merge = true;
                        cell = markupSheet.Cells[markupRowCount, 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Revision " + revNo;
                        markupRowCount++;

                        int prevRowCount = markupRowCount;
                        cell = markupSheet.Cells[markupRowCount, 1];
                        cell.Value = "Description";
                        cell = GiveCellStyleHeaderProperties(cell);
                        markupRowCount++;

                        foreach (var col in markupData.FirstOrDefault().Columns)
                        {
                            cell = markupSheet.Cells[markupRowCount, 1];
                            cell.Value = col.ColumnName;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;
                        }

                        foreach (var col in markupExtraRows)
                        {
                            cell = markupSheet.Cells[markupRowCount, 1];
                            cell.Value = col;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;
                        }

                        int markupWhileCount = 1;
                        while (markupWhileCount < markupRowCount)
                        {
                            markupSheet.Row(markupWhileCount).Height = 20;
                            markupWhileCount++;
                        }

                        int markupColCount = 2;
                        foreach (var markup in markupData)
                        {
                            markupRowCount = prevRowCount;
                            markupSheet.Column(markupColCount).Width = 20;
                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.LineName;
                            cell = GiveCellStyleHeaderProperties(cell);
                            markupRowCount++;

                            foreach (var dynCols in markup.Columns)
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = dynCols.Value;
                                cell = GiveCellStyleProperties(cell);
                                markupRowCount++;
                            }

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.SubTotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Testing;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.OverideTestCharges;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.TravelLodgingBoarding;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Development;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.OtherTotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.LineUnitCost;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.PercentageToUnitCost;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.FinalSubtotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Margin;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            markupColCount++;
                        }
                    }

                    markupRowCount++;
                }
                #endregion

                #region final prices
                var finalPriceSheet = excelPackage.Workbook.Worksheets.Add("Final Price Sheet");
                finalPriceSheet.Name = "Final Price";
                decimal gstPerc = 0;
                int finalPriceRowCount = 1;
                int finalPriceColCount = 1;
                int prevFinalPriceRowCount = 1;
                tenderDetailsDAL = new TenderDetailsDAL();

                foreach (var revNo in revisions)
                {
                    var finalPriceData = tenderDetailsDAL.GetFinalPrices(tenderId, revNo);
                    List<FinalPriceModel> fpDataList = new List<FinalPriceModel>();
                    var finalPriceColumns = new List<ColumnModel>()
                    {
                        new ColumnModel("Description", 30, 12,""),
                        new ColumnModel("EXW", 30, 12,""),
                        new ColumnModel("Freight", 30, 12,""),
                        new ColumnModel("Gst", 30, 12,""),
                        new ColumnModel("Total", 30, 12,""),
                        new ColumnModel("Value in CR", 30, 12,""),
                    };

                    try
                    {
                        if (finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").Any())
                        {
                            gstPerc = finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").FirstOrDefault().Values;
                        }
                        else
                        {
                            gstPerc = 18;
                        }

                        foreach (var data in finalPriceData.TndLineValuesList)
                        {
                            var currentModel = fpDataList.Where(x => x.LineId == data.LineId).Select(y => y).FirstOrDefault();
                            if (currentModel == null)
                            {
                                var fpData = new FinalPriceModel();
                                fpData.LineId = data.LineId;
                                fpData.LineName = data.LineName;
                                if (data.Description.ToLower() == "exworks")
                                {
                                    fpData.ExWorks = data.Values;
                                }
                                else if (data.Description.ToLower() == "freight")
                                {
                                    fpData.Freight = data.Values;
                                }
                                fpDataList.Add(fpData);
                            }
                            else
                            {
                                if (data.Description.ToLower() == "exworks")
                                {
                                    currentModel.ExWorks = data.Values;
                                }
                                else if (data.Description.ToLower() == "freight")
                                {
                                    currentModel.Freight = data.Values;
                                }
                            }
                        }

                        foreach (var mod in fpDataList)
                        {
                            var total = mod.ExWorks + mod.Freight;
                            var gstPrice = total * gstPerc / 100;
                            mod.Gst = gstPrice;
                            mod.Total = total + gstPrice;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    finalPriceSheet.Cells[finalPriceRowCount, 1, finalPriceRowCount, 2 + (fpDataList.Count)].Merge = true;
                    cell = finalPriceSheet.Cells[finalPriceRowCount, 1, finalPriceRowCount, 2 + (fpDataList.Count)];
                    cell.Value = "Revision " + revNo;
                    cell = GiveCellStyleHeaderProperties(cell);
                    finalPriceRowCount++;

                    finalPriceColCount = 1;
                    prevFinalPriceRowCount = finalPriceRowCount;

                    foreach (var col in finalPriceColumns)
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        if (col.Name.ToLower() == "gst")
                        {
                            cell.Value = col.Name + " @" + gstPerc + "%";
                        }
                        else
                            cell.Value = col.Name;
                        cell = GiveCellStyleHeaderProperties(cell);
                        finalPriceRowCount++;

                        finalPriceSheet.Row(finalPriceColCount).Height = 20;
                    }
                    finalPriceSheet.Column(finalPriceColCount).Width = 30;

                    foreach (var model in fpDataList)
                    {
                        finalPriceRowCount = prevFinalPriceRowCount;
                        finalPriceColCount += 1;

                        finalPriceSheet.Column(finalPriceColCount).Width = 30;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.LineName;
                        cell = GiveCellStyleHeaderProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.ExWorks.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Freight.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Gst.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Total.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetValueInCrores(model.Total).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }

                    //finalPriceRowCount += 1;
                    finalPriceRowCount = prevFinalPriceRowCount;
                    finalPriceColCount += 1;
                    finalPriceSheet.Column(finalPriceColCount).Width = 30;
                    foreach (var col in finalPriceColumns)
                    {
                        if (col.Name.ToLower() == "description")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = "Total";
                            cell = GiveCellStyleHeaderProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "exw")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.ExWorks).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "freight")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Freight).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "gst")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Gst).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "total")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "value in cr")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetValueInCrores(GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList())).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                    }

                    finalPriceRowCount += 1;

                }
                #endregion

                #region tender pricing view
                int tenderPricingRowCount = 1;
                int tenderPricingPrevRowCount = 1;
                int tenderPricingColCount = 1;
                tenderDetailsDAL = new TenderDetailsDAL();
                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                foreach (var revNo in revisions)
                {
                    var mainViewData = tenderDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, revNo);
                    var lineListData = mainViewData.LineList ?? new List<LineStructure>();
                    var lineCols = new List<string>();
                    var mainColumns = new List<ColumnModel>();


                    try
                    {
                        lineCols = lineListData.Select(y => y.LineName).ToList();
                        lineCols.Add("Total");

                        mainColumns = new List<ColumnModel>()
                    {
                        new ColumnModel("Sr.No", 20, 12, ""),
                        new ColumnModel("Description", 60, 12, ""),
                        new ColumnModel("Unit", 10, 12, ""),
                        new ColumnModel("Quantity", 100, 12, "") { SubColumns = lineCols, CellMergeCount = lineCols.Count },
                        new ColumnModel("Unit Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                        new ColumnModel("Unit Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                        new ColumnModel("Total Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                        new ColumnModel("Total Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG" } },
                        new ColumnModel("Unit Cost", 20, 12, "") { SubColumns = new List<string>() { "INR"} },
                    };

                        foreach (var data in lineListData)
                        {
                            ColumnModel model = new ColumnModel(data.LineName, 90, 12, "") { SubColumns = new List<string>() { "Unit Cost", "Sales Cost", "Ex-Works", "Freight" } };
                            mainColumns.Add(model);
                        }

                        var pendingCols = new List<ColumnModel>()
                    {
                        new ColumnModel("Sales Cost", 20, 12, ""),
                        new ColumnModel("Ex-Works", 20, 12, ""),
                    };

                        mainColumns.AddRange(pendingCols);

                    }
                    catch (Exception)
                    {

                    }

                    tenderPricing.Cells[tenderPricingRowCount, 1, tenderPricingRowCount, 11 + (5 * lineListData.Count)].Merge = true;
                    cell = tenderPricing.Cells[tenderPricingRowCount, 1, tenderPricingRowCount, 11 + (5 * lineListData.Count)];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Revision " + revNo;
                    tenderPricingRowCount++;

                    var innerColCount = 1;
                    foreach (var data in mainColumns)
                    {
                        tenderPricingColCount = innerColCount;
                        if (data.SubColumns != null && data.SubColumns.Count > 0)
                        {
                            tenderPricing.Cells[tenderPricingRowCount, innerColCount, tenderPricingRowCount, innerColCount + data.SubColumns.Count - 1].Merge = true;

                            foreach (var sub in data.SubColumns)
                            {
                                cell = tenderPricing.Cells[tenderPricingRowCount + 1, innerColCount];
                                tenderPricing.Column(innerColCount).Width = Convert.ToInt32(data.Width / data.SubColumns.Count);
                                cell = GiveCellSubHeaderProperties(cell);
                                cell.Value = sub;
                                innerColCount++;
                            }
                            innerColCount--;
                        }

                        cell = tenderPricing.Cells[tenderPricingRowCount, tenderPricingColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = data.Name;
                        innerColCount++;
                        if (data.SubColumns == null)
                        {
                            tenderPricing.Column(tenderPricingColCount).Width = data.Width;
                            cell = tenderPricing.Cells[tenderPricingRowCount + 1, tenderPricingColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                        }

                    }
                    tenderPricing.Row(tenderPricingRowCount).Height = 20;

                    List<TotalRowModel> totalList = new List<TotalRowModel>() {
                        new  TotalRowModel(){LineId = 0, Key ="UnitCost",Value=0}
                    };

                    var masterData = mainViewData.MasterList;
                    if (masterData != null && masterData.Count > 0)
                    {
                        var lineList = lineListData;
                        var lineQtyList = mainViewData.LineQtyList;
                        var tenderLineValues = mainViewData.TndLineValuesList;

                        foreach (var line in lineList)
                        {
                            totalList.Add(new TotalRowModel() { LineId = line.LineId, UnitCost = 0, Freight = 0, SalesCost = 0, ExWorks = 0, Key = "" });
                            foreach (var master in masterData)
                            {
                                if (master.LineDetails != null)
                                {
                                    master.LineDetails.Add(new MasterLineModel()
                                    {
                                        LineId = line.LineId,
                                        LineName = line.LineName,
                                    });
                                }
                                else
                                {
                                    master.LineDetails = new List<MasterLineModel>()
                            {
                                new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName
                                }};
                                }
                            }
                        }
                        totalList.Add(new TotalRowModel() { LineId = 0, Key = "SalesCost", Value = 0 });
                        totalList.Add(new TotalRowModel() { LineId = 0, Key = "ExWorks", Value = 0 });

                        int id, productId, type, pgId, assmId, subAssmId, compId, mainLineId, qty;
                        id = productId = type = pgId = assmId = subAssmId = compId = mainLineId = qty = 0;

                        foreach (var master in lineQtyList)
                        {
                            if (CheckIfPropertyExistsInDynamicObject(master, "LineId"))
                                mainLineId = GetPropertyValueFromDynamicObject(master, "LineId");

                            //if (CheckIfPropertyExistsInDynamicObject(master, "Id"))
                            //    id = GetPropertyValueFromDynamicObject(master, "Id");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ProductId"))
                                productId = GetPropertyValueFromDynamicObject(master, "ProductId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "Type"))
                                type = GetPropertyValueFromDynamicObject(master, "Type");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ProductGroupId"))
                                pgId = GetPropertyValueFromDynamicObject(master, "ProductGroupId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "AssemblyId"))
                                assmId = GetPropertyValueFromDynamicObject(master, "AssemblyId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "SubAssemblyId"))
                                subAssmId = GetPropertyValueFromDynamicObject(master, "SubAssemblyId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ComponentId"))
                                compId = GetPropertyValueFromDynamicObject(master, "ComponentId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "Quantity"))
                                qty = GetPropertyValueFromDynamicObject(master, "Quantity");

                            var currentModel = masterData.Where(x => x.Id == productId && x.Type == type && x.ProductGroupId == pgId
                            && x.SubAssemblyId == subAssmId && x.AssemblyId == assmId && x.ComponentId == compId).Select(y => y).FirstOrDefault();

                            if (currentModel != null)
                            {
                                var finalTotal = tenderLineValues.Where(x => x.Description == "FinalTotal" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                //var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                var unitCost = currentModel.UnitCost;
                                var totalUnitCost = currentModel.UnitCost * qty;
                                var salesCost = totalUnitCost * finalTotal;
                                var margin = tenderLineValues.Where(x => x.Description == "Margin" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                var exWorks = salesCost / (1 - (margin / 100));
                                var freight = 1;
                                currentModel.LineDetails.Where(x => x.LineId == mainLineId).ToList().ForEach(x => { x.Quantity = qty; x.SalesCost = salesCost; x.ExWorks = exWorks; x.Freight = freight; });
                            }
                        }

                        int rC = tenderPricingRowCount + 2;
                        int cC = 1;
                        int unitCostColNo = 0;

                        foreach (var mod in masterData)
                        {
                            cC = 1;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.SrNo;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.Name;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.Unit;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            foreach (var line in mod.LineDetails)
                            {
                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = line.Quantity;
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                            }

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.LineDetails.Sum(x => x.Quantity);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitGrWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitNetWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.TotalCalcUnitGrWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.TotalUnitNetWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitCost;
                            cell = GiveCellStyleProperties(cell);
                            unitCostColNo = cC;
                            cC++;

                            if (mod.Type != 1)
                            {
                                totalList.Where(y => y.Key == "UnitCost").FirstOrDefault().Value += mod.UnitCost;
                            }

                            foreach (var line in mod.LineDetails)
                            {
                                var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Math.Round(line.Quantity * mod.UnitCost);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost += Math.Round(line.Quantity * mod.UnitCost);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(line.SalesCost, 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight += Truncate(line.SalesCost, 3);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(line.ExWorks, 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost += Truncate(line.ExWorks, 3);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks += Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                            }

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);

                            rC++;
                        }
                        cC = unitCostColNo;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "UnitCost").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;
                        foreach (var line in lineList)
                        {
                            var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks;
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        tenderPricing.Cells[rC, unitCostColNo - 1].Merge = true;
                        cell = tenderPricing.Cells[rC, unitCostColNo - 1];
                        cell.Value = "Total";
                        tenderPricingRowCount = rC + 1;
                    }

                    cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                    cell = GiveLastRowHighlightProperties(cell);

                    tenderPricingRowCount++;
                }

                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] DomDiffTenderComparison(int firstTndId, int firstTndRevNo, int otherTndId, int otherTndRevNo)
        {
            int sheetCount = 1;
            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            TenderListDAL listDAL = new TenderListDAL();

            List<TndCompareModel> tndList = new List<TndCompareModel>();
            tndList.Add(new TndCompareModel { TenderId = firstTndId, TenderRevNo = firstTndRevNo });
            tndList.Add(new TndCompareModel { TenderId = otherTndId, TenderRevNo = otherTndRevNo });

            List<TenderDetailsModel> tenderDetailsList = new List<TenderDetailsModel>();

            TenderListModel listModel = new TenderListModel();
            listModel.TndCompareList = listDAL.GetTndCompareList(1);
            foreach (var tender in tndList)
            {
                var bomId = listModel.TndCompareList.Where(x => x.TenderId == tender.TenderId && x.TenderRevNo == tender.TenderRevNo).FirstOrDefault().BomId;
                var bomRevId = listModel.TndCompareList.Where(x => x.TenderId == tender.TenderId && x.TenderRevNo == tender.TenderRevNo).FirstOrDefault().BomRevisionNo;
                var detailsModel = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tender.TenderId, tender.TenderRevNo);
                tenderDetailsList.Add(detailsModel);
            }

            //tenderDetailsDAL = new TenderDetailsDAL();
            //var mainViewData = tenderDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region rawmData
                sheetCount = 1;
                foreach (var tender in tndList)
                {
                    List<int> list = new List<int>();
                    list.Add(tender.TenderRevNo);

                    rmpDAL = new RawMaterialPricingDAL();
                    var revisions = new List<int>();
                    var rawMatMaster = rmpDAL.GetRawMaterialPricingListForTender(tender.TenderId, out list);

                    var rawMatColumns = new List<ColumnModel>() {
                    new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                    new ColumnModel("Group", 20, 12,"MaterialGroup"),
                    new ColumnModel("Price (Rs)", 20, 12,"Price"),
                    };

                    //list.ForEach(x => { rawMatColumns.Add(new ColumnModel("Price (Rs)", 25, 12, x.ToString())); });

                    var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material " + sheetCount);
                    rawMatPricing.Name = "RawMaterialPricing " + sheetCount;

                    ExcelRange range = rawMatPricing.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, tenderDetailsList[sheetCount - 1]);

                    rowCount = 9;
                    colCount = 1;
                    foreach (var col in rawMatColumns)
                    {
                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = col.Name;
                        rawMatPricing.Column(colCount).Width = col.Width;
                        colCount++;
                    }

                    rowCount++;
                    foreach (var rawMat in rawMatMaster)
                    {
                        colCount = 1;

                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rawMat.RawMaterialName;
                        colCount++;

                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rawMat.GroupName;
                        colCount++;

                        foreach (var rev in rawMat.Pricing.Where(x => x.TenderRevId == tender.TenderRevNo))
                        {
                            cell = rawMatPricing.Cells[rowCount, colCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = rev.Price;
                            colCount++;
                        }
                        rowCount++;
                    }

                    whileCount = 1;
                    while (whileCount <= rawMatPricing.Dimension.End.Row)
                    {
                        rawMatPricing.Row(whileCount).Height = 20;
                        whileCount++;
                    }

                    sheetCount++;
                }
                #endregion

                #region testData
                sheetCount = 1;
                foreach (var tender in tndList)
                {

                    testDAL = new TestDAL();
                    var testMaster = testDAL.GetTestPricingList(tender.TenderId, tender.TenderRevNo);
                    var testColumns = new List<ColumnModel>() {
                    new ColumnModel("Sr.No", 40, 12,"Id", false),
                    new ColumnModel("Name", 30, 12,"TestName"),
                    new ColumnModel("Description", 50, 12,"TestDescription"),
                    new ColumnModel("Group Type", 30, 12,"Type"),
                    new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                    new ColumnModel("Line Type", 30, 12,"KVLine"),
                    new ColumnModel("UTS", 30, 12,"UTS"),
                    new ColumnModel("Summary", 50, 12,"Summary"),
                    new ColumnModel("Price", 10, 12,"Price")
                    };

                    var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing " + sheetCount);
                    testPricing.Name = "Test Master Pricing " + sheetCount;
                    ExcelRange range = testPricing.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, tenderDetailsList[sheetCount - 1]);
                    int rowNum = 9;
                    for (int i = 1; i <= testColumns.Count; i++)
                    {
                        testPricing.Column(i).Width = testColumns[i - 1].Width;
                        testPricing.Row(rowNum).Height = 25;
                        cell = testPricing.Cells[rowNum, i];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);
                    }
                    for (int i = 1; i <= testMaster.TestList.Count; i++)
                    {
                        for (int j = 1; j <= testColumns.Count; j++)
                        {
                            cell = testPricing.Cells[rowNum + 1, j];
                            cell = GiveCellStyleProperties(cell);
                            if (testColumns[j - 1].UseValue)
                            {
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                            }
                            else
                            {
                                cell.Value = i;
                            }
                        }
                        rowNum++;
                    }

                    rowNum = testMaster.TestList.Count + 5;
                    sheetCount++;
                }
                #endregion

                #region bgData
                sheetCount = 1;
                foreach (var tender in tndList)
                {
                    tenderDetailsDAL = new TenderDetailsDAL();
                    var bgList = tenderDetailsDAL.GetBGData(tender.TenderId, tender.TenderRevNo);
                    var bgColumns = new List<ColumnModel>() {
                        new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                        new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                        new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                        new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                        new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                        new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
                    };

                    var contractValue = Truncate(bgList.FirstOrDefault().ContractValue, 3);
                    var deliveryMonth = Truncate(bgList.FirstOrDefault().DeliveryMonth, 3);
                    var performancePeriod = Truncate(bgList.FirstOrDefault().PerformancePeriod, 3);
                    var gracePeriod = Truncate(bgList.FirstOrDefault().GracePeriod, 3);

                    var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee " + sheetCount);
                    bankGuaranteeSheet.Name = "Bank Guarantee " + sheetCount;

                    ExcelRange range = bankGuaranteeSheet.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, tenderDetailsList[sheetCount - 1]);

                    int bgRowCount = 9;
                    int bgColCount = 1;

                    bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                    cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                    cell.Value = "Contract Value : " + contractValue;
                    cell = GiveCellStyleHeaderProperties(cell);
                    bgColCount += 3;

                    bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                    cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                    cell.Value = "Delivery Month : " + deliveryMonth;
                    cell = GiveCellStyleHeaderProperties(cell);
                    bgRowCount++;
                    bgColCount = 1;

                    bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                    cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
                    cell.Value = "Grace Period : " + gracePeriod;
                    cell = GiveCellStyleHeaderProperties(cell);
                    bgColCount += 3;

                    bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                    cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                    cell.Value = "Performance Period : " + performancePeriod;
                    cell = GiveCellStyleHeaderProperties(cell);

                    bankGuaranteeSheet.Row(1).Height = 25;
                    bankGuaranteeSheet.Row(2).Height = 25;
                    bankGuaranteeSheet.Column(1).Width = 90;
                    bankGuaranteeSheet.Column(3).Width = 90;

                    for (int i = 1; i <= bgColumns.Count; i++)
                    {
                        bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                        bankGuaranteeSheet.Row(3).Height = 25;
                        bankGuaranteeSheet.Row(3).Height = 25;
                        cell = bankGuaranteeSheet.Cells[11, i];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = bgColumns[i - 1].Name;
                    }

                    for (int i = 1; i <= bgList.Count; i++)
                    {
                        for (int j = 1; j <= bgColumns.Count; j++)
                        {
                            cell = bankGuaranteeSheet.Cells[i + 11, j];
                            cell = GiveCellStyleProperties(cell);

                            if (bgColumns[j - 1].UseValue)
                            {
                                cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                            }
                            else
                            {
                                cell.Value = "";
                            }
                        }
                    }
                    bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
                    cell = bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count];
                    cell = GiveCellStyleProperties(cell);

                    whileCount = 1;
                    while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                    {
                        bankGuaranteeSheet.Row(whileCount).Height = 20;
                        whileCount++;
                    }

                    sheetCount++;
                }

                #endregion

                #region freightData
                sheetCount = 1;
                foreach (var details in tenderDetailsList)
                {
                    fDAL = new FreightChargesDAL();
                    var freightMasterData = fDAL.GetFreightChargesList(details.BomId, details.RevisionNo, details.TenderId, details.TenderRevisionNo);
                    var lineNames = new List<ColumnModel>();
                    try
                    {
                        if (freightMasterData.Tables != null && freightMasterData.Tables.Count > 0)
                        {
                            for (int i = 0; i < freightMasterData.Tables[0].Rows.Count; i++)
                            {
                                lineNames.Add(new ColumnModel(Convert.ToString(freightMasterData.Tables[0].Rows[i][0]), 60, 14, "") { SubColumns = new List<string>() { "Description", "Value" } });
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight " + sheetCount);
                    freightSheet.Name = "Freight " + sheetCount;

                    ExcelRange range = freightSheet.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, details);
                    int rowStart = 9;
                    var colPos = 1;
                    for (int l = 1; l <= lineNames.Count; l++, colPos += 3)
                    {
                        freightSheet.Cells[rowStart, colPos, rowStart, colPos + 1].Merge = true;
                        freightSheet.Column(colPos).Width = 40;
                        freightSheet.Column(colPos + 1).Width = 20;
                        freightSheet.Row(1).Height = 25;
                        cell = freightSheet.Cells[rowStart, colPos];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = lineNames[l - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);

                        var innerColPos = colPos;
                        for (int k = 0; k < lineNames[l - 1].SubColumns.Count; k++, innerColPos++)
                        {
                            cell = freightSheet.Cells[rowStart + 1, innerColPos];
                            cell.Value = lineNames[l - 1].SubColumns[k];
                            cell.Style.Font.Bold = true;
                            cell.Style.Font.Size = 12;
                            cell.Style.Indent = 1;
                            cell = GiveCellStyleProperties(cell);
                        }
                        freightSheet.Row(2).Height = 20;
                    }

                    colPos = 1;
                    rowStart = 12;
                    int maxRow = 0;
                    decimal loadingFactor;
                    for (int j = 1; j <= lineNames.Count; j++, colPos += 3)
                    {
                        decimal totalWtofMaterial = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TotalNtWt"]);
                        loadingFactor = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["LoadingFactor"]);
                        loadingFactor = loadingFactor == 0 ? 1.15m : loadingFactor;
                        decimal totalWtMt = (totalWtofMaterial * loadingFactor) / 1000;
                        string truckName = Convert.ToString(freightMasterData.Tables[0].Rows[j - 1]["TruckName"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["TruckName"]);

                        List<Location> locations = new List<Location>();

                        for (int loc = 0; loc < freightMasterData.Tables[j].Rows.Count; loc++)
                        {
                            Location locModel = new Location();
                            locModel.LocationName = Convert.ToString(freightMasterData.Tables[j].Rows[loc]["Destinations"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Destinations"]);
                            locModel.Charge = Convert.ToDecimal(freightMasterData.Tables[j].Rows[loc]["Charges"] == DBNull.Value ? 0 : freightMasterData.Tables[j].Rows[loc]["Charges"]);
                            locations.Add(locModel);
                        }

                        decimal avgfreight = GetAverage(locations.Select(x => (dynamic)x.Charge).ToList());
                        decimal maxfreight = locations.Any() ? locations.Max(x => x.Charge) : 0;
                        decimal freightConsidered = (avgfreight + maxfreight) / 2;
                        decimal ratePerTruck = Math.Round(freightConsidered);
                        decimal noOfTruck = Math.Ceiling(totalWtMt / 24);
                        //if (noOfTruck < 1 && noOfTruck > 0)
                        //    noOfTruck = 1;
                        decimal subtotalFreight = ratePerTruck * noOfTruck;
                        decimal contingencyFreight = Convert.ToDecimal(freightMasterData.Tables[0].Rows[j - 1]["Contingency"] == DBNull.Value ? 0 : freightMasterData.Tables[0].Rows[j - 1]["Contingency"]);
                        decimal totalFreight = subtotalFreight + ((subtotalFreight * contingencyFreight) / 100);

                        rowStart = 11;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Weight(MT)";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);

                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalWtMt.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Weight of Material(KG)";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalWtofMaterial.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Loading Factor";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = loadingFactor.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Truck Name";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = truckName;
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        if (locations.Count > 0)
                        {
                            //rowStart++;
                            cell = freightSheet.Cells[rowStart, colPos];
                            cell.Value = "Locations";
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            cell = GiveCellStyleHeaderProperties(cell);

                            cell = freightSheet.Cells[rowStart, colPos + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            rowStart++;



                            foreach (var loc in locations)
                            {
                                cell = freightSheet.Cells[rowStart, colPos];
                                cell.Value = loc.LocationName;
                                cell.Style.Indent = 1;
                                cell = GiveCellStyleProperties(cell);
                                cell = freightSheet.Cells[rowStart, colPos + 1];
                                cell.Value = loc.Charge.ToString("N4");
                                cell.Style.Indent = 1;
                                cell = GiveCellStyleProperties(cell);
                                rowStart++;
                            }

                            //rowStart++;
                        }

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Average Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = avgfreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Max Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = maxfreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;


                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Freight Considered";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = freightConsidered.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Rate Per Truck";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = ratePerTruck.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "No. of Truck";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = noOfTruck.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Subtotal Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = subtotalFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Contingency on Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = contingencyFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        cell = freightSheet.Cells[rowStart, colPos];
                        cell.Value = "Total Freight";
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        cell = freightSheet.Cells[rowStart, colPos + 1];
                        cell.Value = totalFreight.ToString("N4");
                        cell.Style.Indent = 1;
                        cell = GiveCellStyleProperties(cell);
                        rowStart++;

                        maxRow = rowStart > maxRow ? rowStart : maxRow;
                    }

                    int rowCounter = 2;
                    while (rowCounter <= maxRow)
                    {
                        freightSheet.Row(rowCounter).Height = 20;
                        rowCounter++;
                    }

                    sheetCount++;
                }

                #endregion

                #region markupData
                sheetCount = 1;
                foreach (var details in tenderDetailsList)
                {
                    mDAL = new MarkupPricingDAL();
                    var markupDataset = mDAL.GetMarkupPricingList(details.BomId, details.RevisionNo, details.TenderId, details.TenderRevisionNo);
                    //var markupData
                    var markupData = new List<MarkupDataModel>();
                    var markupColumnList = new List<ColumnModel>();

                    try
                    {
                        for (int i = 0; i < markupDataset.Tables[0].Columns.Count; i++)
                        {
                            string colName = string.Empty;

                            if (markupDataset.Tables[0].Columns[i].ColumnName.ToLower().Contains("markupid"))
                                continue;
                            if (markupDataset.Tables[0].Columns[i].ColumnName.Contains("_"))
                            {
                                colName = (markupDataset.Tables[0].Columns[i].ColumnName.Substring(0, (markupDataset.Tables[0].Columns[i].ColumnName.LastIndexOf("_"))));
                                var id = Convert.ToInt32(markupDataset.Tables[0].Columns[i].ColumnName[markupDataset.Tables[0].Columns[i].ColumnName.Length - 1]);
                                markupColumnList.Add(new ColumnModel(colName, 20, 12, markupDataset.Tables[0].Columns[i].ColumnName) { });
                            }
                            else
                            {
                                colName = markupDataset.Tables[0].Columns[i].ColumnName;
                                markupColumnList.Add(new ColumnModel(colName, 40, 12, markupDataset.Tables[0].Columns[i].ColumnName, true, true) { });
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    if (markupDataset.Tables.Count > 0 && markupDataset.Tables[0] != null)
                    {
                        var rowCount = markupDataset.Tables[0].Rows.Count;
                        foreach (var column in markupColumnList.Where(x => x.Exclude == false))
                        {
                            MarkupDataModel markup = new MarkupDataModel();
                            markup.Columns = new List<DynamicColumns>();
                            for (int r = 0; r < rowCount; r++)
                            {
                                markup.Columns.Add(new DynamicColumns
                                {
                                    ColumnName = Convert.ToString(markupDataset.Tables[0].Rows[r]["Description"]),
                                    Value = Convert.ToDecimal(markupDataset.Tables[0].Rows[r][column.PropName])
                                });
                            }
                            markup.SubTotal = Truncate(GetSum(markup.Columns.Select(x => x.Value).ToList()), 3);
                            markup.LineId = Convert.ToInt32(column.PropName.Substring(column.PropName.LastIndexOf("_")).Replace("_", ""));
                            markup.LineName = column.Name;
                            markupData.Add(markup);
                        }
                    }

                    if (markupDataset.Tables.Count > 2 && markupDataset.Tables[2] != null)
                    {
                        for (int i = 0; i < markupDataset.Tables[2].Rows.Count; i++)
                        {
                            var lineId = Convert.ToInt32(markupDataset.Tables[2].Rows[i]["LineId"]);
                            var currentModel = markupData.Where(x => x.LineId == lineId).Select(y => y).FirstOrDefault();
                            currentModel.Testing = markupDataset.Tables[2].Columns.Contains("Testing") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Testing"]) : 0;
                            currentModel.OverideTestCharges = markupDataset.Tables[2].Columns.Contains("TestingOverride") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TestingOverride"]) : 0;
                            currentModel.TravelLodgingBoarding = markupDataset.Tables[2].Columns.Contains("TravelLB") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["TravelLB"]) : 0;
                            currentModel.Development = markupDataset.Tables[2].Columns.Contains("Development") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Development"]) : 0;
                            currentModel.OtherTotal = currentModel.OverideTestCharges > 0 ? (currentModel.OverideTestCharges + currentModel.TravelLodgingBoarding + currentModel.Development) :
                                (currentModel.Testing + currentModel.TravelLodgingBoarding + currentModel.Development);
                            currentModel.PercentageToUnitCost = markupDataset.Tables[2].Columns.Contains("PercentUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["PercentUnitCost"]) : 0;
                            currentModel.LineUnitCost = markupDataset.Tables[2].Columns.Contains("LineUnitCost") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["LineUnitCost"]) : 0;
                            currentModel.Margin = markupDataset.Tables[2].Columns.Contains("Margin") ? Convert.ToDecimal(markupDataset.Tables[2].Rows[i]["Margin"]) : 0;
                            currentModel.FinalSubtotal = Truncate((1 + ((currentModel.SubTotal + currentModel.PercentageToUnitCost) / 100)), 4);
                        }

                        var markupExtraRows = new List<string>() {"Subtotal", "Testing", "Testing Override Charges", "Travel, Loding and Boarding", "Development", "Other Total",
                "Unit Cost", "Percentage to Unit Cost", "Subtotal", "Margin"};

                        var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup " + sheetCount);
                        markupSheet.Name = "Markup " + sheetCount;
                        ExcelRange range = markupSheet.Cells[1, 1, 7, 2];
                        FillCommonFields(ref range, details);
                        markupSheet.Column(1).Width = 40;

                        int markupRowCount = 9;
                        cell = markupSheet.Cells[markupRowCount, 1];
                        cell.Value = "Description";
                        cell = GiveCellStyleHeaderProperties(cell);
                        markupRowCount++;

                        foreach (var col in markupData.FirstOrDefault().Columns)
                        {
                            cell = markupSheet.Cells[markupRowCount, 1];
                            cell.Value = col.ColumnName;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;
                        }

                        foreach (var col in markupExtraRows)
                        {
                            cell = markupSheet.Cells[markupRowCount, 1];
                            cell.Value = col;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;
                        }

                        int markupWhileCount = 1;
                        while (markupWhileCount < markupRowCount)
                        {
                            markupSheet.Row(markupWhileCount).Height = 20;
                            markupWhileCount++;
                        }

                        int markupColCount = 2;
                        foreach (var markup in markupData)
                        {
                            markupRowCount = 9;
                            markupSheet.Column(markupColCount).Width = 20;
                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.LineName;
                            cell = GiveCellStyleHeaderProperties(cell);
                            markupRowCount++;

                            foreach (var dynCols in markup.Columns)
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = dynCols.Value;
                                cell = GiveCellStyleProperties(cell);
                                markupRowCount++;
                            }

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.SubTotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Testing;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.OverideTestCharges;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.TravelLodgingBoarding;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Development;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.OtherTotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.LineUnitCost;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.PercentageToUnitCost;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.FinalSubtotal;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            cell = markupSheet.Cells[markupRowCount, markupColCount];
                            cell.Value = markup.Margin;
                            cell = GiveCellStyleProperties(cell);
                            markupRowCount++;

                            markupColCount++;
                        }
                    }

                    sheetCount++;
                }
                #endregion

                #region final prices
                sheetCount = 1;
                foreach (var tender in tndList)
                {
                    decimal gstPerc = 0;
                    tenderDetailsDAL = new TenderDetailsDAL();
                    var finalPriceData = tenderDetailsDAL.GetFinalPrices(tender.TenderId, tender.TenderRevNo);
                    List<FinalPriceModel> fpDataList = new List<FinalPriceModel>();
                    var finalPriceColumns = new List<ColumnModel>()
            {
                new ColumnModel("Description", 30, 12,""),
                new ColumnModel("EXW", 30, 12,""),
                new ColumnModel("Freight", 30, 12,""),
                new ColumnModel("Gst", 30, 12,""),
                new ColumnModel("Total", 30, 12,""),
                new ColumnModel("Value in CR", 30, 12,""),
            };

                    try
                    {
                        if (finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").Any())
                        {
                            gstPerc = finalPriceData.TenderValues.Where(x => x.Description == "GSTPercentage").FirstOrDefault().Values;
                        }
                        else
                        {
                            gstPerc = 18;
                        }

                        foreach (var data in finalPriceData.TndLineValuesList)
                        {
                            var currentModel = fpDataList.Where(x => x.LineId == data.LineId).Select(y => y).FirstOrDefault();
                            if (currentModel == null)
                            {
                                var fpData = new FinalPriceModel();
                                fpData.LineId = data.LineId;
                                fpData.LineName = data.LineName;
                                if (data.Description.ToLower() == "exworks")
                                {
                                    fpData.ExWorks = data.Values;
                                }
                                else if (data.Description.ToLower() == "freight")
                                {
                                    fpData.Freight = data.Values;
                                }
                                fpDataList.Add(fpData);
                            }
                            else
                            {
                                if (data.Description.ToLower() == "exworks")
                                {
                                    currentModel.ExWorks = data.Values;
                                }
                                else if (data.Description.ToLower() == "freight")
                                {
                                    currentModel.Freight = data.Values;
                                }
                            }
                        }

                        foreach (var mod in fpDataList)
                        {
                            var total = mod.ExWorks + mod.Freight;
                            var gstPrice = total * gstPerc / 100;
                            mod.Gst = gstPrice;
                            mod.Total = total + gstPrice;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    var finalPriceSheet = excelPackage.Workbook.Worksheets.Add("Final Price Sheet " + sheetCount);
                    finalPriceSheet.Name = "Final Price " + sheetCount;

                    ExcelRange range = finalPriceSheet.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, tenderDetailsList[sheetCount - 1]);

                    int finalPriceRowCount = 9;
                    int finalPriceColCount = 1;
                    foreach (var col in finalPriceColumns)
                    {
                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        if (col.Name.ToLower() == "gst")
                        {
                            cell.Value = col.Name + " @" + gstPerc + "%";
                        }
                        else
                            cell.Value = col.Name;
                        cell = GiveCellStyleHeaderProperties(cell);
                        finalPriceRowCount++;

                        finalPriceSheet.Row(finalPriceColCount).Height = 20;
                    }
                    finalPriceSheet.Column(finalPriceColCount).Width = 30;

                    foreach (var model in fpDataList)
                    {
                        finalPriceRowCount = 9;
                        finalPriceColCount += 1;

                        finalPriceSheet.Column(finalPriceColCount).Width = 30;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.LineName;
                        cell = GiveCellStyleHeaderProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.ExWorks.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Freight.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Gst.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = model.Total.ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;

                        cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                        cell.Value = GetValueInCrores(model.Total).ToString("N4");
                        cell = GiveCellStyleProperties(cell);
                        finalPriceRowCount++;
                    }

                    finalPriceRowCount = 9;
                    finalPriceColCount += 1;
                    finalPriceSheet.Column(finalPriceColCount).Width = 30;
                    foreach (var col in finalPriceColumns)
                    {
                        if (col.Name.ToLower() == "description")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = "Total";
                            cell = GiveCellStyleHeaderProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "exw")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.ExWorks).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "freight")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Freight).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "gst")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)x.Gst).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "total")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList()).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                        else if (col.Name.ToLower() == "value in cr")
                        {
                            cell = finalPriceSheet.Cells[finalPriceRowCount, finalPriceColCount];
                            cell.Value = GetValueInCrores(GetSum(fpDataList.Select(x => (dynamic)(x.Total)).ToList())).ToString("N4");
                            cell = GiveCellStyleProperties(cell);
                            finalPriceRowCount++;
                        }
                    }
                    sheetCount++;
                }
                #endregion

                #region tender pricing view
                sheetCount = 1;
                foreach (var details in tenderDetailsList)
                {
                    var mainViewData = details;
                    var lineListData = mainViewData.LineList ?? new List<LineStructure>();
                    var lineCols = new List<string>();
                    var mainColumns = new List<ColumnModel>();


                    try
                    {
                        lineCols = lineListData.Select(y => y.LineName).ToList();
                        lineCols.Add("Total");

                        mainColumns = new List<ColumnModel>()
                    {
                    new ColumnModel("Sr.No", 20, 12, ""),
                    new ColumnModel("Description", 60, 12, ""),
                    new ColumnModel("Unit", 10, 12, ""),
                    new ColumnModel("Quantity", 100, 12, "") { SubColumns = lineCols, CellMergeCount = lineCols.Count },
                    new ColumnModel("Unit Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                    new ColumnModel("Unit Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                    new ColumnModel("Total Gr.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG"} },
                    new ColumnModel("Total Net.Wt", 15, 12, "") { SubColumns = new List<string>() { "KG" } },
                    new ColumnModel("Unit Cost", 20, 12, "") { SubColumns = new List<string>() { "INR"} },
                    };

                        foreach (var data in lineListData)
                        {
                            ColumnModel model = new ColumnModel(data.LineName, 90, 12, "") { SubColumns = new List<string>() { "Unit Cost", "Sales Cost", "Ex-Works", "Freight" } };
                            mainColumns.Add(model);
                        }

                        var pendingCols = new List<ColumnModel>()
            {
                new ColumnModel("Sales Cost", 20, 12, ""),
                new ColumnModel("Ex-Works", 20, 12, ""),
            };

                        mainColumns.AddRange(pendingCols);

                    }
                    catch (Exception)
                    {

                    }

                    var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing " + sheetCount);
                    tenderPricing.Name = "Tender Pricing " + sheetCount;

                    ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                    FillCommonFields(ref range, mainViewData);

                    int tenderPricingRowCount = 9;
                    int tenderPricingColCount = 1;
                    var innerColCount = 1;
                    foreach (var data in mainColumns)
                    {
                        tenderPricingColCount = innerColCount;
                        if (data.SubColumns != null && data.SubColumns.Count > 0)
                        {
                            tenderPricing.Cells[tenderPricingRowCount, innerColCount, tenderPricingRowCount, innerColCount + data.SubColumns.Count - 1].Merge = true;

                            foreach (var sub in data.SubColumns)
                            {
                                cell = tenderPricing.Cells[tenderPricingRowCount + 1, innerColCount];
                                tenderPricing.Column(innerColCount).Width = Convert.ToInt32(data.Width / data.SubColumns.Count);
                                cell = GiveCellSubHeaderProperties(cell);
                                cell.Value = sub;
                                innerColCount++;
                            }
                            innerColCount--;
                        }

                        cell = tenderPricing.Cells[tenderPricingRowCount, tenderPricingColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = data.Name;
                        innerColCount++;
                        if (data.SubColumns == null)
                        {
                            tenderPricing.Column(tenderPricingColCount).Width = data.Width;
                            cell = tenderPricing.Cells[tenderPricingRowCount + 1, tenderPricingColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                        }

                    }
                    tenderPricing.Row(tenderPricingRowCount).Height = 20;

                    List<TotalRowModel> totalList = new List<TotalRowModel>() {
                        new  TotalRowModel(){LineId = 0, Key ="UnitCost",Value=0}
                    };

                    var masterData = mainViewData.MasterList;
                    if (masterData != null && masterData.Count > 0)
                    {
                        var lineList = lineListData;
                        var lineQtyList = mainViewData.LineQtyList;
                        var tenderLineValues = mainViewData.TndLineValuesList;

                        foreach (var line in lineList)
                        {
                            totalList.Add(new TotalRowModel() { LineId = line.LineId, UnitCost = 0, Freight = 0, SalesCost = 0, ExWorks = 0, Key = "" });
                            foreach (var master in masterData)
                            {
                                if (master.LineDetails != null)
                                {
                                    master.LineDetails.Add(new MasterLineModel()
                                    {
                                        LineId = line.LineId,
                                        LineName = line.LineName,
                                    });
                                }
                                else
                                {
                                    master.LineDetails = new List<MasterLineModel>()
                            {
                                new MasterLineModel()
                                {
                                    LineId = line.LineId,
                                    LineName = line.LineName
                                }};
                                }
                            }
                        }
                        totalList.Add(new TotalRowModel() { LineId = 0, Key = "SalesCost", Value = 0 });
                        totalList.Add(new TotalRowModel() { LineId = 0, Key = "ExWorks", Value = 0 });

                        int id, productId, type, pgId, assmId, subAssmId, compId, mainLineId, qty;
                        id = productId = type = pgId = assmId = subAssmId = compId = mainLineId = qty = 0;

                        foreach (var master in lineQtyList)
                        {
                            if (CheckIfPropertyExistsInDynamicObject(master, "LineId"))
                                mainLineId = GetPropertyValueFromDynamicObject(master, "LineId");

                            //if (CheckIfPropertyExistsInDynamicObject(master, "Id"))
                            //    id = GetPropertyValueFromDynamicObject(master, "Id");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ProductId"))
                                productId = GetPropertyValueFromDynamicObject(master, "ProductId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "Type"))
                                type = GetPropertyValueFromDynamicObject(master, "Type");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ProductGroupId"))
                                pgId = GetPropertyValueFromDynamicObject(master, "ProductGroupId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "AssemblyId"))
                                assmId = GetPropertyValueFromDynamicObject(master, "AssemblyId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "SubAssemblyId"))
                                subAssmId = GetPropertyValueFromDynamicObject(master, "SubAssemblyId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "ComponentId"))
                                compId = GetPropertyValueFromDynamicObject(master, "ComponentId");

                            if (CheckIfPropertyExistsInDynamicObject(master, "Quantity"))
                                qty = GetPropertyValueFromDynamicObject(master, "Quantity");

                            var currentModel = masterData.Where(x => x.Id == productId && x.Type == type && x.ProductGroupId == pgId
                            && x.SubAssemblyId == subAssmId && x.AssemblyId == assmId && x.ComponentId == compId).Select(y => y).FirstOrDefault();

                            if (currentModel != null)
                            {
                                var finalTotal = tenderLineValues.Where(x => x.Description == "FinalTotal" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                //var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                var unitCost = currentModel.UnitCost;
                                var totalUnitCost = currentModel.UnitCost * qty;
                                var salesCost = totalUnitCost * finalTotal;
                                var margin = tenderLineValues.Where(x => x.Description == "Margin" && x.LineId == mainLineId).Select(y => y.Values).FirstOrDefault();
                                var exWorks = salesCost / (1 - (margin / 100));
                                var freight = 1;
                                currentModel.LineDetails.Where(x => x.LineId == mainLineId).ToList().ForEach(x => { x.Quantity = qty; x.SalesCost = salesCost; x.ExWorks = exWorks; x.Freight = freight; });
                            }
                        }

                        int rC = 11;
                        int cC = 1;
                        int unitCostColNo = 0;

                        foreach (var mod in masterData)
                        {
                            cC = 1;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.SrNo;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.Name;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.Unit;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            foreach (var line in mod.LineDetails)
                            {
                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = line.Quantity;
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                            }

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.LineDetails.Sum(x => x.Quantity);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitGrWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitNetWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.TotalCalcUnitGrWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.TotalUnitNetWt;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = mod.UnitCost;
                            cell = GiveCellStyleProperties(cell);
                            unitCostColNo = cC;
                            cC++;

                            if (mod.Type != 1)
                            {
                                totalList.Where(y => y.Key == "UnitCost").FirstOrDefault().Value += mod.UnitCost;
                            }

                            foreach (var line in mod.LineDetails)
                            {
                                var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Math.Round(line.Quantity * mod.UnitCost);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost += Math.Round(line.Quantity * mod.UnitCost);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(line.SalesCost, 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight += Truncate(line.SalesCost, 3);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(line.ExWorks, 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost += Truncate(line.ExWorks, 3);

                                cell = tenderPricing.Cells[rC, cC];
                                cell.Value = Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                                cell = GiveCellStyleProperties(cell);
                                cC++;
                                totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks += Truncate(unitFreight * ((mod.TotalUnitNetWt * mod.LineDetails.Sum(x => x.Quantity)) / 1000), 3);
                            }

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.SalesCost), 3);

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value += Truncate(mod.LineDetails.Max(x => x.ExWorks), 3);

                            rC++;
                        }
                        cC = unitCostColNo;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "UnitCost").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;
                        foreach (var line in lineList)
                        {
                            var unitFreight = tenderLineValues.Where(x => x.Description == "UnitFreight" && x.LineId == line.LineId).Select(y => y.Values).FirstOrDefault();

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().UnitCost;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().Freight;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().SalesCost;
                            cell = GiveCellStyleProperties(cell);
                            cC++;

                            cell = tenderPricing.Cells[rC, cC];
                            cell.Value = totalList.Where(x => x.LineId == line.LineId).FirstOrDefault().ExWorks;
                            cell = GiveCellStyleProperties(cell);
                            cC++;
                        }

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "SalesCost").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        cell = tenderPricing.Cells[rC, cC];
                        cell.Value = totalList.Where(x => x.Key == "ExWorks").FirstOrDefault().Value;
                        cell = GiveCellStyleProperties(cell);
                        cC++;

                        tenderPricing.Cells[rC, unitCostColNo - 1].Merge = true;
                        cell = tenderPricing.Cells[rC, unitCostColNo - 1];
                        cell.Value = "Total";
                    }

                    cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                    cell = GiveLastRowHighlightProperties(cell);
                    sheetCount++;
                }

                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        #endregion

        #region karamtara reports

        public byte[] DownloadInternationalTenderPricingDataK(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            #region rawmData
            rmpDAL = new RawMaterialPricingDAL();
            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"SrNo"),
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
                new ColumnModel("Price", 10, 12,"Price")
            };
            #endregion

            #region testData
            testDAL = new TestDAL();
            var testMaster = testDAL.GetIntTestPricingList(tenderId, tenderRevId);
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                new ColumnModel("Quantity", 20, 12,"Quantity"),
                new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
            };

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
            var bgColumns = new List<ColumnModel>() {

                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
            };

            var contractValue = Truncate(bgList.FirstOrDefault().ContractValue, 3);
            var deliveryMonth = Truncate(bgList.FirstOrDefault().DeliveryMonth, 3);
            var performancePeriod = Truncate(bgList.FirstOrDefault().PerformancePeriod, 3);
            var gracePeriod = Truncate(bgList.FirstOrDefault().GracePeriod, 3);

            #endregion

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            //var markupData
            //var markupData = new List<MarkupDataModel>();
            var markupColumns = new List<ColumnModel>()
            {
                new ColumnModel("Markup", 50, 0, "Markup"),
                new ColumnModel("Italy", 30, 0, "Italy"),
                new ColumnModel("India", 30, 0, "India"),
                new ColumnModel("BO", 30, 0, "BO"),
            };

            #endregion

            #region freightData

            var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, tenderRevId);

            var freightRows = new List<RowModel>()
            {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "Override No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainersOverridden" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
            };

            var commonCols = new List<ColumnModel>()
            {
                new ColumnModel("Data", 20, 12, ""),
                new ColumnModel("Total Cost", 20, 12, ""),
                new ColumnModel("Remarks", 30, 12, ""),
            };

            List<TableModel> freightData = new List<TableModel>();
            int total20FtContainers = 0;
            int total40FtContainers = 0;
            decimal finalOverallTotal = 0;

            foreach (var val in freightMasterData.TenderPortNames)
            {
                TableModel mod = new TableModel();
                List<RowModel> rowList = new List<RowModel>();
                mod.Rows = new List<RowModel>();
                mod.Id = val.Id;
                mod.Title = val.PortName;
                rowList = new List<RowModel>()
                {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "Override No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainersOverridden" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                 };

                foreach (var data in rowList)
                {
                    var temp = freightMasterData.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                    if (temp != null)
                    {
                        //if (data.KeyName == "NoOfFortyFtContainers")
                        //    total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                        if (data.KeyName == "OverallTotal")
                            finalOverallTotal += temp.Cost;
                        data.Value = temp.Cost;
                        data.Data = temp.Data;
                        data.Remark = temp.Remarks;
                    }
                }

                var No20Ft = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                var No40Ft = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFortyFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                var No20FtOver = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                var No40FtOver= freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFortyFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                if ((Convert.ToInt32(Decimal.Ceiling(No20FtOver.Data))) == 0 && (Convert.ToInt32(Decimal.Ceiling(No40FtOver.Data))) == 0)
                {
                    total20FtContainers += Convert.ToInt32(Decimal.Ceiling(No20Ft.Data));
                    total40FtContainers += Convert.ToInt32(Decimal.Ceiling(No40Ft.Data));
                }
                else
                {
                    total20FtContainers += Convert.ToInt32(Decimal.Ceiling(No20FtOver.Data));
                    total40FtContainers += Convert.ToInt32(Decimal.Ceiling(No40FtOver.Data));
                }

                //no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                //tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                //if (tempData.Data > 0)
                //    no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                //total20FtContainers += no20FtContainers;

                decimal pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                mod.Rows.AddRange(new List<RowModel>()
                {
                    new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                });

                mod.Rows.AddRange(rowList);

                freightData.Add(mod);
            }

            var freightTotal = new List<RowModel>()
            {
                new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
            };
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            var otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(Exworks)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { x.CurrencyName },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, spUsd = 0, usdToInr = 0, euroToInr = 0, cifPort = 0, spUsdSummation = 0, spEuro = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksSumUsd = 0, exWorksSumEuro = 0, loadingFactor = 0, mt = 0; ;

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
            euroToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            col.Rows.Add(string.Format("{0} MT", mt));
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)(Truncate(x.UnitCost, 3) * x.Quantity) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;
                    indiaCostSummation += Truncate(indiaCost, 3) * mod.Quantity;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                    boSummation += Truncate(mod.Quantity * boCost, 3);

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);
                    costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);

                    spInr = (costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
                    exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                    spUsd = usdToInr == 0 ? 0 : (spInr / usdToInr);
                    spEuro = usdToInr == 0 ? 0 : (spInr / euroToInr);
                    exWorksSumUsd += Truncate(mod.Quantity * spUsd, 3);
                    exWorksSumEuro += Truncate(mod.Quantity * spEuro, 3);
                }
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spUsd = usdToInr == 0 ? 0 : Truncate((spInr / usdToInr), 3);

                    spEuro = euroToInr == 0 ? 0 : Truncate((spInr / euroToInr), 3);

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    spUsdSummation += (spInr * mod.Quantity);

                    foreach (var currency in otherCurrencies)
                    {
                        var value = spInr / currency.Value;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = value * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), Truncate(qtyValue, 3)));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }

                var last = tenderView.MasterList.Last();
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        decimal seaFreight = 0, sellingPrice = 0;

                        if (exWorksSumInr != 0)
                            seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        else
                            seaFreight = 0;

                        if (col.ExtraValue != 0)
                            sellingPrice = Truncate(Math.Ceiling((Truncate(spInr, 3) / col.ExtraValue)), 1);
                        else
                            sellingPrice = 0;

                        //if (col.ExtraKey.ToLower() == "euro")
                        //    cifPort = Truncate(((usdToInr / euroToInrCost) * spUsd) * (1 + seaFreight / 100), 1);
                        //else
                        //    cifPort = Truncate(spUsd * (1 + (seaFreight / 100)), 1);

                        cifPort = Truncate((sellingPrice * (1 + (seaFreight / 100))), 1);

                        col.Rows.Add(Truncate(cifPort, 3));
                        col.Summation += Truncate(cifPort, 3) * mod.Quantity;
                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumUsd, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumEuro, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }


            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                for (int i = 1; i <= rawMatColumns.Count; i++)
                {
                    rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
                    rawMatPricing.Row(1).Height = 25;
                    cell = rawMatPricing.Cells[1, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = rawMatColumns[i - 1].Name;
                    cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
                }

                for (int i = 1; i <= rawMatMaster.Count; i++)
                {
                    for (int j = 1; j <= rawMatColumns.Count; j++)
                    {
                        cell = rawMatPricing.Cells[i + 1, j];
                        cell = GiveCellStyleProperties(cell);
                        rawMatPricing.Row(i + 1).Height = 20;
                        if (rawMatColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = string.Empty;
                        }
                    }
                }
                #endregion

                #region test master pricing

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";
                int testCol = 1;
                int testRow = 1;
                for (int i = 1; i <= testColumns.Count; i++)
                {
                    if (testColumns[i - 1].SubColumns == null)
                    {

                        testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                        testPricing.Row(testRow).Height = 25;

                        testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                        cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);

                        if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                        {
                            decimal value = 0;

                            if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                            cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                        }
                    }
                    else
                    {
                        testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                        testPricing.Row(testRow).Height = 25;
                        testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                        cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);

                        colCount = i;

                        foreach (var columns in testColumns[i - 1].SubColumns)
                        {
                            testPricing.Column(testCol).Width = 20;
                            testPricing.Row(testRow).Height = 25;
                            cell = testPricing.Cells[testRow + 1, testCol];
                            cell.Value = columns.ToUpper();
                            cell = GiveCellStyleHeaderProperties(cell);
                            testCol++;
                        }

                        testCol--;

                    }

                    testCol++;

                }
                testRow += 2;
                testCol = 1;
                for (int i = 1; i <= testMaster.TestList.Count; i++)
                {
                    testCol = 1;
                    for (int j = 1; j <= testColumns.Count; j++)
                    {
                        if (testColumns[j - 1].SubColumns == null)
                        {
                            cell = testPricing.Cells[testRow, testCol];
                            cell = GiveCellStyleProperties(cell);
                            if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                            {
                                if (testColumns[j - 1].UseValue)
                                {
                                    cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                                }
                                else
                                {
                                    cell.Value = i;
                                }
                            }
                            else
                            {
                                decimal value = 0;

                                if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                    value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                            }
                        }
                        else
                        {
                            foreach (var column in testColumns[j - 1].SubColumns)
                            {
                                cell = testPricing.Cells[testRow, testCol];
                                cell = GiveCellStyleProperties(cell);
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                                testCol++;
                            }

                            testCol--;
                        }
                        testCol++;
                    }
                    testRow++;
                }
                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                int bgRowCount = 1;
                int bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Contract Value : " + contractValue;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Delivery Month : " + deliveryMonth;
                cell = GiveCellStyleHeaderProperties(cell);
                bgRowCount++;
                bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
                cell.Value = "Grace Period : " + gracePeriod;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Performance Period : " + performancePeriod;
                cell = GiveCellStyleHeaderProperties(cell);

                bankGuaranteeSheet.Row(1).Height = 25;
                bankGuaranteeSheet.Row(2).Height = 25;
                bankGuaranteeSheet.Column(1).Width = 90;
                bankGuaranteeSheet.Column(3).Width = 90;

                for (int i = 1; i <= bgColumns.Count; i++)
                {
                    bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    cell = bankGuaranteeSheet.Cells[3, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = bgColumns[i - 1].Name;
                }

                for (int i = 1; i <= bgList.Count; i++)
                {
                    for (int j = 1; j <= bgColumns.Count; j++)
                    {
                        cell = bankGuaranteeSheet.Cells[i + 3, j];
                        cell = GiveCellStyleProperties(cell);

                        if (bgColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }
                bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
                cell = bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count];
                cell = GiveCellStyleProperties(cell);

                whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region markup

                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                markupSheet.Name = "Markup";

                int markupRowCount = 1;
                int markupColCount = 1;

                foreach (var col in markupColumns)
                {
                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = col.Name;
                    markupSheet.Column(markupColCount).Width = col.Width;
                    markupColCount++;
                }

                markupRowCount = 2;
                markupColCount = 1;
                foreach (var mark in markupDataset.MarkupDetails)
                {
                    bool isPBG = false;
                    isPBG = mark.MarkupId == 16 ? true : false;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = mark.Markup;
                    markupColCount++;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = isPBG ? (markupDataset.TndType == 2 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = isPBG ? (markupDataset.TndType == 1 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;


                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;


                    switch (mark.MarkupId)
                    {
                        case 10: //financing
                            {

                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 17: //financing sales cr
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 16: //pbg
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 18: //interest savings on advance
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }

                                string value2 = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value2;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }

                                break;
                            }
                    }

                    markupColCount = 1;
                    markupRowCount++;
                }
                markupSheet.Column(5).Width = 20;
                markupSheet.Column(6).Width = 20;

                int markupWhile = 1;
                while (markupWhile < markupRowCount)
                {
                    markupSheet.Row(markupWhile).Height = 20;
                    markupWhile++;
                }

                markupWhile++;

                //Create table for travel, lodging and boarding
                markupSheet.Cells[2, 8, 2, 9].Merge = true;
                cell = markupSheet.Cells[2, 8, 2, 9];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Travel, Lodging and Boarding";

                cell = markupSheet.Cells[3, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "No. Of Persons";

                cell = markupSheet.Cells[3, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[4, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "No. Of Days";

                cell = markupSheet.Cells[4, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[5, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "Fare";

                cell = markupSheet.Cells[5, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[6, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "Lodging";

                cell = markupSheet.Cells[6, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;

                //PackingPercentage
                decimal pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                int tempRowCount = 8, tempColCount = 8;

                pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
                cell = markupSheet.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Packing Material Weight as Percentage";

                cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = pckPercentage;

                cell = markupSheet.Cells[10, 8];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Cross Currency Margin";

                cell = markupSheet.Cells[10, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                //Create table for currency master
                markupSheet.Cells[12, 8, 12, 11].Merge = true;
                cell = markupSheet.Cells[12, 8, 12, 11];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Currency";

                cell = markupSheet.Cells[13, 8];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "International Currency";

                cell = markupSheet.Cells[13, 9];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Convert Into INR";

                cell = markupSheet.Cells[13, 10];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Actual Value";

                cell = markupSheet.Cells[13, 11];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Considered Value";

                tempRowCount = 14;
                tempColCount = 8;

                decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                int count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                        cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                        cell = GiveCellStyleProperties(cell, true, true);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = markupSheet.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = currency.Name;

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }


                markupSheet.Column(8).Width = 40;
                markupSheet.Column(9).Width = 20;
                markupSheet.Column(10).Width = 20;
                markupSheet.Column(11).Width = 20;

                //var markupLastCol = markupSheet.Dimension.End.Column + 1;
                //cell = markupSheet.Cells[1, markupLastCol];

                #endregion

                #region freight

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";
                freightSheet.View.FreezePanes(1, 2);

                int freightRowCount = 1;
                int freightColCount = 1;

                cell = freightSheet.Cells[freightRowCount, freightColCount]; 
                freightSheet.Column(freightColCount).Width = 50;
                cell.Value = "Freight";
                cell = GiveCellStyleHeaderProperties(cell);
                freightRowCount += 1;

                var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
                var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

                foreach (var col in secondayCols)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell.Value = col;
                    if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                        cell = GiveCellStyleHeaderProperties(cell);
                    else
                        cell = GiveCellStyleProperties(cell);
                    freightRowCount++;
                }

                cell = freightSheet.Cells[freightRowCount, 1];
                cell = GiveCellSubHeaderProperties(cell);
                freightColCount = 1;
                freightRowCount++;
                foreach (var col in actualCols)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell.Value = col;
                    if (col == "Containers" || col == "Air Freight Cost")
                        cell = GiveCellStyleHeaderProperties(cell);
                    else
                        cell = GiveCellStyleProperties(cell);
                    freightRowCount++;
                }

                freightColCount = 2;

                foreach (var mod in freightData)
                {
                    freightRowCount = 1;

                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = mod.Title;
                    freightRowCount++;
                    freightColCount += 2;
                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                    {
                        freightColCount -= 2;

                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Value;

                        freightColCount += 2;
                        freightRowCount++;
                    }

                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                    {
                        freightColCount -= 2;

                        if (rowCol.IsHeading)
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = rowCol.SubColumns[0].Name;
                            freightColCount += 2;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = rowCol.SubColumns[1].Name;
                        }
                        else
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = rowCol.Value;
                            freightColCount += 2;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = rowCol.Data;
                        }
                        freightRowCount++;

                    }



                    //freightColCount -= 2;
                    cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Data";
                    freightSheet.Column(freightColCount - 2).Width = 15;

                    cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Total Cost";
                    freightSheet.Column(freightColCount - 1).Width = 15;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Remarks";
                    freightSheet.Column(freightColCount).Width = 30;

                    freightRowCount++;
                    foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                    {
                        freightColCount -= 2;
                        if (row.IsHeading)
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                            cell = GiveCellStyleHeaderProperties(cell);
                            freightColCount += 2;
                        }
                        else
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Data;
                            freightColCount++;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Value;
                            freightColCount++;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Remark;

                        }
                        freightRowCount++;
                    }


                    freightColCount += 2;
                }

                freightRowCount = 1;
                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Freight Total";
                freightRowCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Sr. No.";
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Description";
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Value";
                freightRowCount++;
                freightColCount -= 2;
                count = 1;
                foreach (var row in freightTotal)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = count;
                    freightColCount++;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Description;
                    freightSheet.Column(freightColCount).Width = 30;
                    freightColCount++;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Value;
                    freightSheet.Column(freightColCount).Width = 20;
                    freightColCount -= 2;
                    freightRowCount++;
                    count++;
                }

                int freightWhileCount = 1;
                while (freightWhileCount < freightRowCount)
                {
                    freightSheet.Row(freightWhileCount).Height = 20;
                    freightWhileCount++;
                }

                #endregion

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                FillCommonFields(ref range, tenderView);

                int mainRowCount = 9;
                int mainColCount = 1;
                foreach (var cols in mainColumns)
                {
                    tenderPricing.Column(mainColCount).Width = cols.Width;

                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = cols.Name;
                    mainRowCount++;

                    if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = cols.SubColumns[0];
                    }

                    mainRowCount++;

                    foreach (var row in cols.Rows)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row;
                        mainRowCount++;
                    }
                    mainColCount++;
                    mainRowCount = 9;
                }

                tempRowCount = mainRowCount;
                tempColCount = tenderPricing.Dimension.End.Column + 2;

                whileCount = 3;
                while (whileCount < tenderPricing.Dimension.End.Row)
                {
                    tenderPricing.Row(whileCount).Height = 18;
                    whileCount++;
                }

                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                cell = GiveLastRowHighlightProperties(cell);

                //Create table for currency master
                tempRowCount = 1;
                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Packing Material Weight as Percentage";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = pckPercentage;
                tempRowCount += 2;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Cross Currency Margin";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempRowCount += 2;
                //Create table for currency master
                tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Currency";
                tempRowCount++;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "International Currency";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Convert Into INR";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Actual Value";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Considered Value";
                tempRowCount++;

                tempEuroValue = 0;
                tempUsdValue = 0; tempCurrencyValue = 0;
                conversionRate = 0;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                        cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = currency.Name;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }

                tenderPricing.Column(1).Width = 30;
                tenderPricing.Column(tempColCount).Width = 45;
                tenderPricing.Column(tempColCount + 1).Width = 20;
                tenderPricing.Column(tempColCount + 2).Width = 20;
                tenderPricing.Column(tempColCount + 3).Width = 20;

                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] IntTenderCompareRevisionK(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            decimal pckPercentage;
            int count, tempRowCount = 1, tempColCount;

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            #region revisions
            _revisions = tndDetailsDAL.GetRevisionIds(tenderId);
            #endregion

            #region rawm Data

            rmpDAL = new RawMaterialPricingDAL();
            var revisions = new List<int>();
            var rawMatMaster = rmpDAL.GetRawMaterialPricingListForTender(tenderId, out revisions);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
            };

            revisions.ForEach(x => { rawMatColumns.Add(new ColumnModel(string.Format("Revision {0} Price (Rs)", x), 25, 12, x.ToString())); });

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGDataForAllRevisions(tenderId);

            #endregion

            #region markupData

            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingListForAllRevisions(tenderId, _revisions);

            #endregion

            #region freightData

            var freightMasterList = new List<IntFreightModel>();
            foreach (var rev in _revisions)
            {
                var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, rev);
                freightMasterList.Add(freightMasterData);
            }

            #endregion

            #region tender pricing view

            tndDetailsDAL = new TenderDetailsDAL();
            var mainViewList = new List<TenderDetailsModel>();
            foreach (var revision in _revisions)
            {
                var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, revision);
                mainViewList.Add(tenderView);
            }

            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                rowCount = 1;
                colCount = 1;
                foreach (var col in rawMatColumns)
                {
                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = col.Name;
                    rawMatPricing.Column(colCount).Width = col.Width;
                    colCount++;
                }

                rowCount++;
                foreach (var rawMat in rawMatMaster)
                {
                    colCount = 1;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.RawMaterialName;
                    colCount++;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.GroupName;
                    colCount++;

                    foreach (var rev in rawMat.Pricing)
                    {
                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rev.Price;
                        colCount++;
                    }
                    rowCount++;
                }

                whileCount = 1;
                while (whileCount <= rawMatPricing.Dimension.End.Row)
                {
                    rawMatPricing.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region testData

                var testColumns = new List<ColumnModel>() {
                    new ColumnModel("Sr.No", 8, 12,"Id", false),
                    new ColumnModel("Name", 30, 12,"TestName"),
                    new ColumnModel("Description", 50, 12,"TestDescription"),
                    new ColumnModel("Group Type", 30, 12,"Type"),
                    new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                    new ColumnModel("Line Type", 30, 12,"KVLine"),
                    new ColumnModel("UTS", 30, 12,"UTS"),
                    new ColumnModel("Summary", 50, 12,"Summary"),
                    new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                    new ColumnModel("Quantity", 20, 12,"Quantity"),
                    new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
                };

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";
                int testCol = 1;
                int testRow = 1;
                foreach (var rev in revisions)
                {
                    testDAL = new TestDAL();
                    var testMaster = testDAL.GetIntTestPricingList(tenderId, rev);

                    var tenderView = mainViewList.Where(x => x.TenderRevisionNo == rev).FirstOrDefault();

                    testPricing.Cells[testRow, 1, testRow, testColumns.Count + 2].Merge = true;
                    cell = testPricing.Cells[testRow, 1, testRow, testColumns.Count + 2];
                    cell.Value = "Revision " + rev;
                    cell = GiveCellStyleHeaderProperties(cell);
                    testRow++;

                    testCol = 1;
                    for (int i = 1; i <= testColumns.Count; i++)
                    {
                        if (testColumns[i - 1].SubColumns == null)
                        {

                            testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                            testPricing.Row(testRow).Height = 25;

                            testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                            cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                            cell.Value = testColumns[i - 1].Name;
                            cell = GiveCellStyleHeaderProperties(cell);

                            if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                            {
                                decimal value = 0;

                                if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                    value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                            }

                        }
                        else
                        {
                            testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                            testPricing.Row(testRow).Height = 25;
                            testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                            cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                            cell.Value = testColumns[i - 1].Name;
                            cell = GiveCellStyleHeaderProperties(cell);

                            colCount = i;

                            foreach (var columns in testColumns[i - 1].SubColumns)
                            {
                                testPricing.Column(testCol).Width = 20;
                                testPricing.Row(testRow).Height = 25;
                                cell = testPricing.Cells[testRow + 1, testCol];
                                cell.Value = columns.ToUpper();
                                cell = GiveCellStyleHeaderProperties(cell);
                                testCol++;
                            }

                            testCol--;

                        }

                        testCol++;

                    }
                    testRow += 2;
                    testCol = 1;
                    for (int i = 1; i <= testMaster.TestList.Count; i++)
                    {
                        testCol = 1;
                        for (int j = 1; j <= testColumns.Count; j++)
                        {
                            if (testColumns[j - 1].SubColumns == null)
                            {
                                cell = testPricing.Cells[testRow, testCol];
                                cell = GiveCellStyleProperties(cell);
                                if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                                {
                                    if (testColumns[j - 1].UseValue)
                                    {
                                        cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                                    }
                                    else
                                    {
                                        cell.Value = i;
                                    }
                                }
                                else
                                {
                                    decimal value = 0;

                                    if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                        value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                    cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                                }
                            }
                            else
                            {
                                foreach (var column in testColumns[j - 1].SubColumns)
                                {
                                    cell = testPricing.Cells[testRow, testCol];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                                    testCol++;
                                }

                                testCol--;
                            }
                            testCol++;
                        }
                        testRow++;
                    }
                    testRow++;
                }

                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                whileCount = 1;
                while (whileCount <= 5)
                {
                    bankGuaranteeSheet.Column(whileCount).Width = 40;
                    whileCount++;
                }

                rowCount = 1;
                foreach (var bgModel in bgList)
                {
                    if (bgModel.Count > 0)
                    {
                        colCount = 1;

                        var bgVerticalCols = new List<ColumnModel>() {
                    new ColumnModel(string.Format("Revision {0}", bgModel.FirstOrDefault().TenderRevisionNo), 40, 12,""),
                    new ColumnModel("Bank Guarantee Type", 30, 12,""),
                    new ColumnModel("Bank Guarantee Month", 30, 12,""),
                    new ColumnModel("Commision (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee Amount", 30, 12,""),
                    new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"")
                    };

                        var bghorizontalCols = new List<ColumnModel>()
                    {
                    new ColumnModel("Advance BG", 30, 12,""),
                    new ColumnModel("Performance BG", 30, 12,""),
                    new ColumnModel("Retension BG", 30, 12,""),
                    new ColumnModel("Total", 30, 12,"")
                    };

                        var contractValue = Truncate(bgModel.FirstOrDefault().ContractValue, 3);
                        var deliveryMonth = Truncate(bgModel.FirstOrDefault().DeliveryMonth, 3);
                        var performancePeriod = Truncate(bgModel.FirstOrDefault().PerformancePeriod, 3);
                        var gracePeriod = Truncate(bgModel.FirstOrDefault().GracePeriod, 3);

                        var bgHorizontalTopColumns = new List<ColumnModel>()
                    {
                    new ColumnModel("Contract Value", 30, 12,""){ Value = contractValue},
                    new ColumnModel("Performance Period", 30, 12,""){ Value = performancePeriod},
                    new ColumnModel("Grace Period", 30, 12,""){ Value = gracePeriod},
                    new ColumnModel("Delivery Month", 30, 12,""){ Value = deliveryMonth}
                    };

                        int innerCount = 0;
                        foreach (var col in bgVerticalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount + innerCount, colCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        colCount++;

                        innerCount = 0;
                        foreach (var col in bgHorizontalTopColumns)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = string.Format("{0} : {1}", col.Name, col.Value);
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var col in bghorizontalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgMonth in bgModel.Select(x => x.BGMonth))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgMonth;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGMonth);
                        rowCount++;

                        innerCount = 0;
                        foreach (var commisionPer in bgModel.Select(x => x.CommisionPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = commisionPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.CommisionPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgpercent in bgModel.Select(x => x.BGPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgpercent;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgAmount in bgModel.Select(x => x.BGAmount))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgAmount;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGAmount);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgCostPer in bgModel.Select(x => x.BGCostPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgCostPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGCostPercentage);
                        rowCount += 2;
                    }
                }

                whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region markup

                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                markupSheet.Name = "Markup";

                int markupRowCount = 1;
                int markupColCount = 1;

                int incr = 0;

                foreach (var markup in markupDataset)
                {
                    var currencyData = markup.Currency;
                    incr = 0;
                    markupColCount = 1;

                    int startRowCount = markupRowCount;
                    int startColcount = markupColCount;

                    markupSheet.Cells[markupRowCount, markupColCount, markupRowCount, markupColCount + 3].Merge = true;
                    cell = markupSheet.Cells[markupRowCount, markupColCount, markupRowCount, markupColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = string.Format("Revision {0}", markup.TndRevNo);
                    markupRowCount++;

                    var markupColumns = new List<ColumnModel>()
                    {
                        new ColumnModel("Markup", 50, 0, "Markup"),
                        new ColumnModel("India", 30, 0, "India"),
                        new ColumnModel("Italy", 30, 0, "Italy"),
                        new ColumnModel("BO", 30, 0, "BO"),
                        //new ColumnModel("", 30, 0, ""),
                        //new ColumnModel("", 30, 0, "")
                    };

                    foreach (var col in markupColumns)
                    {
                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = col.Name;
                        markupSheet.Column(markupColCount).Width = col.Width;
                        incr++;
                    }

                    markupRowCount++;
                    incr = 0;
                    List<int> keys = new List<int>() { 10, 16, 17, 18 };
                    foreach (var mark in markup.MarkupDetails)
                    {
                        bool isPBG = false;
                        isPBG = mark.MarkupId == 16 ? true : false;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = mark.Markup;
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = isPBG ? (markup.TndType == 1 ? markup.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                            : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = isPBG ? (markup.TndType == 2 ? markup.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                            : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        switch (mark.MarkupId)
                        {
                            case 10: //financing
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 17: //financing sales cr
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 16: //pbg
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 18: //interest savings on advance
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }

                                    string value2 = markup.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value2;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }

                                    break;
                                }
                        }

                        incr = 0;
                        markupRowCount++;
                    }

                    markupSheet.Column(5).Width = 20;
                    markupSheet.Column(6).Width = 20;

                    int markupWhile = 1;
                    while (markupWhile < markupRowCount)
                    {
                        markupSheet.Row(markupWhile).Height = 20;
                        markupWhile++;
                    }

                    markupWhile = 2;
                    while (markupWhile < 7)
                    {
                        markupSheet.Column(markupWhile).Width = 30;
                        markupWhile++;
                    }

                    var newRow = markupSheet.Dimension.End.Row - 23;
                    incr = 0;

                    //Create table for travel, lodging and boarding
                    markupSheet.Cells[newRow, 8, newRow, 9].Merge = true;
                    cell = markupSheet.Cells[newRow, 8, newRow, 9];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Travel, Lodging and Boarding";
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "No. Of Persons";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "No. Of Days";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "Fare";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "Lodging";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;
                    incr += 2;

                    //PackingPercentage
                    pckPercentage = markup.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                    tempRowCount = newRow + incr;
                    tempColCount = 8;

                    pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
                    cell = markupSheet.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Packing Material Weight as Percentage";

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = pckPercentage;
                    incr += 2;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Cross Currency Margin";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;
                    incr += 2;

                    //Create table for currency master
                    markupSheet.Cells[newRow + incr, 8, newRow + incr, 11].Merge = true;
                    cell = markupSheet.Cells[newRow + incr, 8, newRow + incr, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Currency";
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "International Currency";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Convert Into INR";

                    cell = markupSheet.Cells[newRow + incr, 10];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Actual Value";

                    cell = markupSheet.Cells[newRow + incr, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Considered Value";
                    incr++;

                    tempRowCount = newRow + incr;
                    tempColCount = 8;

                    decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

                    conversionRate = markup.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                    tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                    count = 0;
                    foreach (var currency in currencyData.List)
                    {
                        if (count == 2)
                        {
                            markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                            cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                            cell = GiveCellStyleProperties(cell, true, true);
                            cell.Value = "Euro To Other Currencies";
                            tempRowCount++;
                        }

                        cell = markupSheet.Cells[tempRowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = currency.Name;

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(currency.Value, 3);

                        if (currency.Name.ToLower() == "euro")
                        {
                            tempCurrencyValue = (tempEuroValue / tempUsdValue);

                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else if (currency.Name.ToLower() == "usd")
                        {
                            tempCurrencyValue = (tempUsdValue / tempEuroValue);

                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else
                        {
                            tempCurrencyValue = (tempEuroValue / currency.Value);
                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }

                        tempCurrencyValue += conversionRate / 100;

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);

                        tempRowCount++;
                        count++;
                    }


                    markupSheet.Column(8).Width = 40;
                    markupSheet.Column(9).Width = 20;
                    markupSheet.Column(10).Width = 20;
                    markupSheet.Column(11).Width = 20;
                    startRowCount += 2;

                    for (int i = startRowCount; i < markupRowCount; i++)
                    {
                        //cell = markupSheet.Cells[i, 5];

                        //if (string.IsNullOrEmpty(Convert.ToString(cell.Value)))
                        //    cell = GiveCellStyleProperties(cell);

                        //cell = markupSheet.Cells[i, 6];

                        //if (string.IsNullOrEmpty(Convert.ToString(cell.Value)))
                        //    cell = GiveCellStyleProperties(cell);
                    }
                    markupRowCount++;
                }


                #endregion

                #region freight
                rowCount = 1;
                colCount = 1;
                incr = 0;
                int incrCol = 0;

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";
                freightSheet.View.FreezePanes(1, 2);
                int freightRowCount = 1;
                int freightColCount = 1;

                foreach (var freight in freightMasterList)
                {
                    freightColCount = 1;
                    if (freight.TenderPortNames.Count > 0)
                    {
                        colCount = 1;
                        incrCol = 0;

                        var commonCols = new List<ColumnModel>()
                    {
                        new ColumnModel("Data", 20, 12, ""),
                        new ColumnModel("Total Cost", 20, 12, ""),
                        new ColumnModel("Remarks", 30, 12, ""),
                    };

                        List<TableModel> freightData = new List<TableModel>();

                        int total20FtContainers = 0;
                        int total40FtContainers = 0;
                        decimal finalOverallTotal = 0;

                        foreach (var val in freight.TenderPortNames)
                        {
                            TableModel mod = new TableModel();
                            List<RowModel> rowList = new List<RowModel>();
                            mod.Rows = new List<RowModel>();
                            mod.Id = val.Id;
                            mod.Title = val.PortName;
                            rowList = new List<RowModel>()
                            {
                                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                                new RowModel(){ Description = "Containers", IsHeading = true},
                                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                            };

                            foreach (var data in rowList)
                            {
                                var temp = freight.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                                if (temp != null)
                                {
                                    if (data.KeyName == "NoOfFortyFtContainers")
                                        total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                                    if (data.KeyName == "OverallTotal")
                                        finalOverallTotal += temp.Cost;
                                    data.Value = temp.Cost;
                                    data.Data = temp.Data;
                                    data.Remark = temp.Remarks;
                                }
                            }
                            var no20FtContainers = 0;
                            var tempData = freight.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                            no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                            tempData = freight.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                            if (tempData.Data > 0)
                                no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                            total20FtContainers += no20FtContainers;

                            pckPercentage = markupDataset.Where(x => x.TndRevNo == freight.TndRevNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                                markupDataset.Where(x => x.TndRevNo == freight.TndRevNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                            mod.Rows.AddRange(new List<RowModel>()
                            {
                                new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")} },
                                new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")} },
                                new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                            });

                            mod.Rows.AddRange(rowList);

                            freightData.Add(mod);
                        }

                        var freightTotal = new List<RowModel>()
                        {
                            new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                            new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                            new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
                        };

                        freightSheet.Cells[freightRowCount, 1, freightRowCount, freightColCount + (freight.TenderPortNames.Count * 3)].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, 1, freightRowCount, freightColCount + (freight.TenderPortNames.Count * 3)];
                        cell.Value = "Revision " + freight.TndRevNo;
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightRowCount += 1;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        freightSheet.Column(freightColCount).Width = 50;
                        cell.Value = "Freight";
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightRowCount += 1;

                        var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
                        var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

                        foreach (var col in secondayCols)
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell.Value = col;
                            if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                                cell = GiveCellStyleHeaderProperties(cell);
                            else
                                cell = GiveCellStyleProperties(cell);
                            freightRowCount++;
                        }

                        cell = freightSheet.Cells[freightRowCount, 1];
                        cell = GiveCellSubHeaderProperties(cell);
                        freightColCount = 1;
                        freightRowCount++;
                        foreach (var col in actualCols)
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell.Value = col;
                            if (col == "Containers" || col == "Air Freight Cost")
                                cell = GiveCellStyleHeaderProperties(cell);
                            else
                                cell = GiveCellStyleProperties(cell);
                            freightRowCount++;
                        }

                        freightColCount = 2;

                        foreach (var mod in freightData)
                        {
                            freightRowCount -= 29;

                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = mod.Title;
                            freightRowCount++;
                            freightColCount += 2;
                            foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                            {
                                freightColCount -= 2;

                                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                                cell = GiveCellStyleProperties(cell);
                                cell.Value = rowCol.Value;

                                freightColCount += 2;
                                freightRowCount++;
                            }

                            foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                            {
                                freightColCount -= 2;

                                if (rowCol.IsHeading)
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    cell.Value = rowCol.SubColumns[0].Name;
                                    freightColCount += 2;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    cell.Value = rowCol.SubColumns[1].Name;
                                }
                                else
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = rowCol.Value;
                                    freightColCount += 2;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = rowCol.Data;
                                }
                                freightRowCount++;

                            }

                            //freightColCount -= 2;
                            cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Data";
                            freightSheet.Column(freightColCount - 2).Width = 15;

                            cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Total Cost";
                            freightSheet.Column(freightColCount - 1).Width = 15;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Remarks";
                            freightSheet.Column(freightColCount).Width = 30;

                            freightRowCount++;
                            foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                            {
                                freightColCount -= 2;
                                if (row.IsHeading)
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    freightColCount += 2;
                                }
                                else
                                {
                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Data;
                                    freightColCount++;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Value;
                                    freightColCount++;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Remark;

                                }
                                freightRowCount++;
                            }


                            freightColCount++;
                        }

                        int freightTempRowCount = freightRowCount - 29;
                        freightColCount++;
                        freightSheet.Cells[freightTempRowCount, freightColCount, freightTempRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightTempRowCount, freightColCount, freightTempRowCount, freightColCount + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Freight Total";
                        freightTempRowCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Sr. No.";
                        freightColCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Description";
                        freightColCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Value";
                        freightTempRowCount++;
                        freightColCount -= 2;
                        count = 1;
                        foreach (var row in freightTotal)
                        {
                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = count;
                            freightColCount++;

                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Description;
                            freightSheet.Column(freightColCount).Width = 30;
                            freightColCount++;

                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Value;
                            freightSheet.Column(freightColCount).Width = 20;
                            freightColCount -= 2;
                            freightTempRowCount++;
                            count++;
                        }

                        int freightWhileCount = 1;
                        while (freightWhileCount < freightRowCount)
                        {
                            freightSheet.Row(freightWhileCount).Height = 20;
                            freightWhileCount++;
                        }

                        freightRowCount++;
                    }
                }

                #endregion

                #region mainView
                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                rowCount = 1;
                colCount = 1;
                incrCol = 1;
                int mainRowCount = 1;
                int mainColCount = 1;
                int currencyCol = 12 + mainViewList.Max(x => x.TndPortDetails.Count) + 1;
                tempRowCount = 1;
                foreach (var tenderView in mainViewList)
                {
                    var otherCurrencies = tenderView.CurrencyList.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

                    var portList = tenderView.TndPortDetails;

                    tenderView.MasterList.ForEach(x =>
                    {
                        x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                        tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
                    });

                    var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                };

                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Add(new ColumnModel("Selling Price(Exworks)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
                    }

                    var index = 5;
                    portList.ForEach(x =>
                    {
                        mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                        {
                            SubColumns = new List<string>() { x.CurrencyName },
                            UniqueId = x.Id,
                            Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                            tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                            ExtraKey = x.CurrencyName,
                            ExtraValue = x.CurrencyValue,
                            Rows = new List<dynamic>()
                        }); index++;
                    });

                    decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                            spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, spUsd = 0, usdToInr = 0, euroToInr = 0, cifPort = 0, spUsdSummation = 0, spEuro = 0;

                    decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksSumUsd = 0, exWorksSumEuro = 0, loadingFactor = 0, mt = 0; ;

                    indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
                    boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
                    usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
                    euroToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
                    loadingFactor = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                            markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;
                    pckPercentage = loadingFactor;
                    foreach (var col in mainColumns)
                    {
                        switch (col.PropName)
                        {
                            case "srno":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                                    break;
                                }
                            case "desc":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                                    col.Rows.Add("Total");
                                    break;
                                }
                            case "unit":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                                    break;
                                }
                            case "drawno":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                                    break;
                                }
                            case "qty":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                                    break;
                                }
                            case "unitwt":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                                    mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                                    col.Rows.Add(string.Format("{0} MT", mt));
                                    break;
                                }
                            case "indiacost":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)(Truncate(x.UnitCost, 3) * x.Quantity) : (dynamic)(0))).ToList()));
                                    break;
                                }
                            case "totalindiacost":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                                    break;
                                }
                        }
                    }

                    foreach (var mod in tenderView.MasterList)
                    {
                        if (mod.Type != 1)
                        {
                            indiaCost = mod.UnitCost;
                            indiaCostSummation += Truncate(indiaCost, 3) * mod.Quantity;

                            boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                                tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                            boSummation += Truncate(mod.Quantity * boCost, 3);

                            costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);
                            costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);

                            spInr = (costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
                            exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                            spUsd = usdToInr == 0 ? 0 : (spInr / usdToInr);
                            spEuro = usdToInr == 0 ? 0 : (spInr / euroToInr);
                            exWorksSumUsd += Truncate(mod.Quantity * spUsd, 3);
                            exWorksSumEuro += Truncate(mod.Quantity * spEuro, 3);
                        }
                    }
                    List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
                    foreach (var mod in tenderView.MasterList)
                    {
                        if (mod.Type != 1)
                        {
                            indiaCost = mod.UnitCost;

                            boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                                tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                            costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                            spInr = Truncate((costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                            spUsd = usdToInr == 0 ? 0 : Truncate((spInr / usdToInr), 3);

                            spEuro = euroToInr == 0 ? 0 : Truncate((spInr / euroToInr), 3);

                            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));
                            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                            spUsdSummation += (spInr * mod.Quantity);

                            foreach (var currency in otherCurrencies)
                            {
                                var value = spInr / currency.Value;
                                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                                var qtyValue = value * mod.Quantity;
                                otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), Truncate(qtyValue, 3)));
                            }
                        }
                        else
                        {
                            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                            foreach (var currency in otherCurrencies)
                            {
                                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                            }
                        }

                        var last = tenderView.MasterList.Last();
                        foreach (var col in mainColumns.Where(x => x.Exclude))
                        {
                            if (mod.Type != 1)
                            {
                                decimal seaFreight = 0, sellingPrice = 0;

                                if (exWorksSumInr != 0)
                                    seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                                else
                                    seaFreight = 0;

                                if (col.ExtraValue != 0)
                                    sellingPrice = Truncate(Math.Ceiling((Truncate(spInr, 3) / col.ExtraValue)), 1);
                                else
                                    sellingPrice = 0;

                                //if (col.ExtraKey.ToLower() == "euro")
                                //    cifPort = Truncate(((usdToInr / euroToInrCost) * spUsd) * (1 + seaFreight / 100), 1);
                                //else
                                //    cifPort = Truncate(spUsd * (1 + (seaFreight / 100)), 1);

                                cifPort = Truncate((sellingPrice * (1 + (seaFreight / 100))), 1);

                                col.Rows.Add(Truncate(cifPort, 3));
                                col.Summation += Truncate(cifPort, 3) * mod.Quantity;
                            }
                            else
                                col.Rows.Add("");

                            if (mod.Equals(last))
                            {
                                col.Rows.Add(col.Summation);
                            }
                        }
                    };

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumUsd, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumEuro, 3));
                    foreach (var currency in otherCurrencies)
                    {
                        var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
                    }

                    tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count].Merge = true;
                    cell = tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Revision " + tenderView.TenderRevisionNo;
                    tempRowCount++;

                    mainColCount = 1;
                    mainRowCount = tempRowCount;
                    int prevRowCount = mainRowCount;

                    foreach (var cols in mainColumns)
                    {
                        tenderPricing.Column(mainColCount).Width = cols.Width;

                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = cols.Name;
                        mainRowCount++;

                        if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = cols.SubColumns[0];
                        }

                        mainRowCount++;

                        foreach (var row in cols.Rows)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row;
                            mainRowCount++;
                        }
                        mainColCount++;
                        mainRowCount = prevRowCount;
                    }
                    tempRowCount += tenderView.MasterList.Count + 2;
                    tempColCount = mainColumns.Count + 2;

                    whileCount = 3;
                    while (whileCount < tenderPricing.Dimension.End.Row)
                    {
                        tenderPricing.Row(whileCount).Height = 18;
                        whileCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count];
                    cell = GiveLastRowHighlightProperties(cell);

                    //Create table for currency master
                    //tempRowCount = 1;
                    rowCount = prevRowCount - 1;
                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Packing Material Weight as Percentage";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = pckPercentage;
                    rowCount += 2;

                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Cross Currency Margin";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    rowCount += 2;
                    //Create table for currency master
                    tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3].Merge = true;
                    cell = tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Currency";
                    rowCount++;

                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "International Currency";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Convert Into INR";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Actual Value";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Considered Value";
                    rowCount++;

                    decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate = 0;

                    conversionRate = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    tempEuroValue = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                    tempUsdValue = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                    count = 0;
                    foreach (var currency in tenderView.CurrencyList)
                    {
                        if (count == 2)
                        {
                            tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3].Merge = true;
                            cell = tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = "Euro To Other Currencies";
                            rowCount++;
                        }

                        cell = tenderPricing.Cells[rowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = currency.Name;

                        cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(currency.Value, 3);

                        if (currency.Name.ToLower() == "euro")
                        {
                            tempCurrencyValue = (tempEuroValue / tempUsdValue);

                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else if (currency.Name.ToLower() == "usd")
                        {
                            tempCurrencyValue = (tempUsdValue / tempEuroValue);

                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else
                        {
                            tempCurrencyValue = (tempEuroValue / currency.Value);
                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }

                        tempCurrencyValue += conversionRate / 100;

                        cell = tenderPricing.Cells[rowCount, tempColCount + 3];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);

                        rowCount++;
                        count++;
                    }

                    if (rowCount > tempRowCount)
                    {
                        tempRowCount = rowCount + 1;
                    }
                    else
                        tempRowCount += 1;

                    tenderPricing.Column(1).Width = 30;
                    tenderPricing.Column(tempColCount).Width = 45;
                    tenderPricing.Column(tempColCount + 1).Width = 20;
                    tenderPricing.Column(tempColCount + 2).Width = 20;
                    tenderPricing.Column(tempColCount + 3).Width = 20;

                    //colCount = 1;
                    //incrRow = 0;
                    //incrCol = 0;
                    //portCount = tenderView.TndPortDetails.Count;

                    //if (portCount > 0)
                    //{
                    //    var portList = tenderView.TndPortDetails;
                    //    tenderView.MasterList.ForEach(x =>
                    //    {
                    //        x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                    //        tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
                    //    });

                    //    var mainColumns = new List<ColumnModel>()
                    //{
                    //new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                    //new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                    //new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                    //new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                    //new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                    //new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                    //new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                    //new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                    //new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                    //new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                    //new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                    //new ColumnModel("Selling Price(Exworks)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                    //};

                    //    var index = 5;
                    //    portList.ForEach(x =>
                    //    {
                    //        mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                    //        {
                    //            SubColumns = new List<string>() { x.CurrencyName },
                    //            UniqueId = x.Id,
                    //            Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    //            tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    //            ExtraKey = x.CurrencyName,
                    //            ExtraValue = x.CurrencyValue,
                    //            Rows = new List<dynamic>()
                    //        }); index++;
                    //    });

                    //    decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    //    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, spUsd = 0, usdToInr = 0, cifPort = 0, spUsdSummation = 0;

                    //    decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksSumUsd = 0, loadingFactor = 0, mt = 0;

                    //    indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    //    italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    //    indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    //    italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    //    negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    //    euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
                    //    boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
                    //    usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
                    //    loadingFactor = portList.FirstOrDefault().PackingPercentage;

                    //    foreach (var col in mainColumns)
                    //    {
                    //        switch (col.PropName)
                    //        {
                    //            case "srno":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                    //                    break;
                    //                }
                    //            case "desc":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                    //                    col.Rows.Add("Total");
                    //                    break;
                    //                }
                    //            case "unit":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                    //                    break;
                    //                }
                    //            case "drawno":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                    //                    break;
                    //                }
                    //            case "qty":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                    //                    break;
                    //                }
                    //            case "unitwt":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                    //                    mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                    //                    col.Rows.Add(string.Format("{0} MT", mt));
                    //                    break;
                    //                }
                    //            case "indiacost":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                    //                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
                    //                    break;
                    //                }
                    //            case "totalindiacost":
                    //                {
                    //                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                    //                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                    //                    break;
                    //                }
                    //        }
                    //    }

                    //    foreach (var mod in tenderView.MasterList)
                    //    {
                    //        if (mod.Type != 1)
                    //        {
                    //            indiaCost = mod.UnitCost;
                    //            indiaCostSummation += Truncate(indiaCost, 3);

                    //            boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    //                tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                    //            boSummation += Truncate(mod.Quantity * boCost, 3);

                    //            costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);
                    //            costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);

                    //            spInr = (costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
                    //            exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                    //            spUsd = (spInr / usdToInr);
                    //            exWorksSumUsd += Truncate(mod.Quantity * spUsd, 3);
                    //        }
                    //    }


                    //    foreach (var mod in tenderView.MasterList)
                    //    {
                    //        if (mod.Type != 1)
                    //        {
                    //            indiaCost = mod.UnitCost;

                    //            boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    //                tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    //            costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    //            spInr = Truncate((costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    //            spUsd = Truncate(Math.Ceiling((spInr / usdToInr)), 1);

                    //            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    //            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    //            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    //            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));
                    //            spUsdSummation += (spInr * mod.Quantity);
                    //        }
                    //        else
                    //        {
                    //            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    //            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    //            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    //            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");
                    //        }

                    //        var last = tenderView.MasterList.Last();
                    //        foreach (var col in mainColumns.Where(x => x.Exclude))
                    //        {
                    //            if (mod.Type != 1)
                    //            {
                    //                var seaFreight = exWorksSumInr == 0 ? 0 : Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);

                    //                if (col.ExtraKey.ToLower() == "euro")
                    //                    cifPort = Truncate(((usdToInr / euroToInrCost) * spUsd) * (1 + seaFreight / 100), 1);
                    //                else
                    //                    cifPort = Truncate(spUsd * (1 + (seaFreight / 100)), 1);

                    //                col.Rows.Add(Truncate(cifPort, 3));
                    //                col.Summation = col.Summation + Truncate(cifPort, 3);
                    //            }
                    //            else
                    //                col.Rows.Add("");

                    //            if (mod.Equals(last))
                    //            {
                    //                col.Rows.Add(col.Summation);
                    //            }
                    //        }
                    //    };

                    //    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
                    //    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
                    //    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
                    //    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumUsd, 3));

                    //    tenderPricing.Cells[rowCount, 1, rowCount, 12 + portCount].Merge = true;
                    //    cell = tenderPricing.Cells[rowCount, 1, rowCount, 12 + portCount];
                    //    cell = GiveCellStyleHeaderProperties(cell);
                    //    cell.Value = string.Format("Revision {0}", tenderView.TenderRevisionNo);
                    //    rowCount++;

                    //    foreach (var cols in mainColumns)
                    //    {
                    //        tenderPricing.Column(colCount + incrCol).Width = cols.Width;

                    //        cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol];
                    //        cell = GiveCellStyleHeaderProperties(cell);
                    //        cell.Value = cols.Name;
                    //        incrRow++;

                    //        if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                    //        {
                    //            cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol];
                    //            cell = GiveCellSubHeaderProperties(cell);
                    //            cell.Value = cols.SubColumns[0];
                    //        }

                    //        incrRow++;

                    //        foreach (var row in cols.Rows)
                    //        {
                    //            cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol];
                    //            cell = GiveCellStyleProperties(cell);
                    //            cell.Value = row;
                    //            incrRow++;
                    //        }
                    //        incrCol++;
                    //        incrRow = 0;
                    //    }

                    //    ////temporary
                    //    //mainRowCount++; mainColCount++;
                    //    //cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    //    //cell.Value = spUsdSummation;
                    //    //cell.Style.Font.Bold = true;

                    //    incrCol = currencyCol;

                    //    //Create table for currency master
                    //    tenderPricing.Cells[rowCount + incrRow, colCount + incrCol, rowCount + incrRow, colCount + incrCol + 1].Merge = true;
                    //    cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol, rowCount + incrRow, colCount + incrCol + 1];
                    //    cell = GiveCellStyleHeaderProperties(cell);
                    //    cell.Value = "Currency";
                    //    incrRow++;

                    //    foreach (var currency in tenderView.CurrencyData.List)
                    //    {
                    //        cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol];
                    //        cell = GiveCellStyleProperties(cell);
                    //        cell.Value = currency.Name;

                    //        cell = tenderPricing.Cells[rowCount + incrRow, colCount + incrCol + 1];
                    //        cell = GiveCellStyleProperties(cell);
                    //        cell.Value = Truncate(currency.Value, 3);
                    //        incrRow++;
                    //    }

                    //    tenderPricing.Column(colCount + incrCol).Width = (tenderPricing.Column(colCount + incrCol).Width < 15) ? 15 : tenderPricing.Column(colCount + incrCol).Width;
                    //    tenderPricing.Column(colCount + incrCol + 1).Width = (tenderPricing.Column(colCount + incrCol + 1).Width < 15) ? 15 : tenderPricing.Column(colCount + incrCol + 1).Width;

                    //    whileCount = 3;
                    //    while (whileCount < tenderPricing.Dimension.End.Row)
                    //    {
                    //        tenderPricing.Row(whileCount).Height = 18;
                    //        whileCount++;
                    //    }

                    //    cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, 12 + portCount];
                    //    cell = GiveLastRowHighlightProperties(cell);

                    //    rowCount = tenderPricing.Dimension.End.Row + 2;
                    //}
                }
                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] DownloadInternationalTenderPricingCustomerDataG(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            int tempColCount = 0, tempRowCount = 0, count = 0;
            decimal pckPercentage = 0, conversionRate = 0;

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            var otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(Exworks)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { x.CurrencyName },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, spUsd = 0, usdToInr = 0, euroToInr = 0, cifPort = 0, spUsdSummation = 0, spEuro = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksSumUsd = 0, exWorksSumEuro = 0, loadingFactor = 0, mt = 0; ;

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
            euroToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            col.Rows.Add(string.Format("{0} MT", mt));
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)(Truncate(x.UnitCost, 3) * x.Quantity) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;
                    indiaCostSummation += Truncate(indiaCost, 3) * mod.Quantity;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                    boSummation += Truncate(mod.Quantity * boCost, 3);

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);
                    costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);

                    spInr = (costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
                    exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                    spUsd = usdToInr == 0 ? 0 : (spInr / usdToInr);
                    spEuro = usdToInr == 0 ? 0 : (spInr / euroToInr);
                    exWorksSumUsd += Truncate(mod.Quantity * spUsd, 3);
                    exWorksSumEuro += Truncate(mod.Quantity * spEuro, 3);
                }
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spUsd = usdToInr == 0 ? 0 : Truncate((spInr / usdToInr), 3);

                    spEuro = euroToInr == 0 ? 0 : Truncate((spInr / euroToInr), 3);

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    spUsdSummation += (spInr * mod.Quantity);

                    foreach (var currency in otherCurrencies)
                    {
                        var value = spInr / currency.Value;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = value * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), Truncate(qtyValue, 3)));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }

                var last = tenderView.MasterList.Last();
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        decimal seaFreight = 0, sellingPrice = 0;

                        if (exWorksSumInr != 0)
                            seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        else
                            seaFreight = 0;

                        if (col.ExtraValue != 0)
                            sellingPrice = Truncate(Math.Ceiling((Truncate(spInr, 3) / col.ExtraValue)), 1);
                        else
                            sellingPrice = 0;

                        //if (col.ExtraKey.ToLower() == "euro")
                        //    cifPort = Truncate(((usdToInr / euroToInrCost) * spUsd) * (1 + seaFreight / 100), 1);
                        //else
                        //    cifPort = Truncate(spUsd * (1 + (seaFreight / 100)), 1);

                        cifPort = Truncate((sellingPrice * (1 + (seaFreight / 100))), 1);

                        col.Rows.Add(Truncate(cifPort, 3));
                        col.Summation += Truncate(cifPort, 3) * mod.Quantity;
                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumUsd, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumEuro, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }


            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                //ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                //FillCommonFields(ref range, tenderView);

                int mainRowCount = 1;
                int mainColCount = 1;
                foreach (var cols in mainColumns)
                {
                    tenderPricing.Column(mainColCount).Width = cols.Width;

                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = cols.Name;
                    mainRowCount++;

                    if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = cols.SubColumns[0];
                    }

                    mainRowCount++;

                    foreach (var row in cols.Rows)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row;
                        mainRowCount++;
                    }
                    mainColCount++;
                    mainRowCount = 1;
                }

                tempRowCount = mainRowCount;
                tempColCount = tenderPricing.Dimension.End.Column + 2;

                whileCount = 3;
                while (whileCount < tenderPricing.Dimension.End.Row)
                {
                    tenderPricing.Row(whileCount).Height = 18;
                    whileCount++;
                }

                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                cell = GiveLastRowHighlightProperties(cell);

                //Create table for currency master
                tempRowCount = 1;
                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Packing Material Weight as Percentage";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = pckPercentage;
                tempRowCount += 2;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Cross Currency Margin";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempRowCount += 2;
                //Create table for currency master
                tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Currency";
                tempRowCount++;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "International Currency";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Convert Into INR";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Actual Value";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Considered Value";
                tempRowCount++;

                decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0;
                conversionRate = 0;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                        cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = currency.Name;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }

                tenderPricing.Column(1).Width = 30;
                tenderPricing.Column(tempColCount).Width = 45;
                tenderPricing.Column(tempColCount + 1).Width = 20;
                tenderPricing.Column(tempColCount + 2).Width = 20;
                tenderPricing.Column(tempColCount + 3).Width = 20;

                tenderPricing.DeleteColumn(7 + portList.Count, tenderPricing.Dimension.Columns);
                tenderPricing.DeleteColumn(tenderPricing.Dimension.Columns);
                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        #endregion

        #region iselfa reports

        public byte[] DownloadInternationalTenderPricingDataI(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            int count = 0;
            decimal otherCurrencyValue = 0, mt = 0, loadingFactor = 0, dividingFactor40Ft, dividingFactor20Ft, considered40FtCntr, considered20FtCntr, dollarsPer40Ft, dollarsPer20Ft,
                rsPer40FtContainer, rsPer20FtContainer, rsPer40FtContainers, rsPer20FtContainers, totalRsPer40FtContainers, totalRsPer20FtContainers, usdToInr = 0, sizeWise20FtContr,
                sizeWise40FtContr, totalRsForCon;

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, cifPort = 0, cifPortUsd = 0, euroToOtherCurrency, cifPortOtherCurrency;

            decimal euroToUsd = 0, spEuro = 0, spUsd = 0, noOfCon = 0, rsPerCon = 0, distFact = 0, containerCharges = 0, currencyConvRate = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksEuro = 0, exWorksUsd = 0;

            List<CurrencyModel> otherCurrencies = null;

            #region rawmData
            rmpDAL = new RawMaterialPricingDAL();
            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"SrNo"),
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
                new ColumnModel("Price", 10, 12,"Price")
            };
            #endregion

            #region testData
            testDAL = new TestDAL();
            var testMaster = testDAL.GetIntTestPricingList(tenderId, tenderRevId);
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                new ColumnModel("Quantity", 20, 12,"Quantity"),
                new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
            };

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
            var bgColumns = new List<ColumnModel>() {

                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
            };

            var contractValue = Truncate(bgList.Any() ? bgList.FirstOrDefault().ContractValue : 0, 3);
            var deliveryMonth = Truncate(bgList.Any() ? bgList.FirstOrDefault().DeliveryMonth : 0, 3);
            var performancePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().PerformancePeriod : 0, 3);
            var gracePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().GracePeriod : 0, 3);

            #endregion

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            //var markupData
            //var markupData = new List<MarkupDataModel>();
            var markupColumns = new List<ColumnModel>()
            {
                new ColumnModel("Markup", 50, 0, "Markup"),
                new ColumnModel("India", 30, 0, "India"),
                new ColumnModel("Italy", 30, 0, "Italy"),
                new ColumnModel("BO", 30, 0, "BO"),
            };

            #endregion

            #region freightData

            var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, tenderRevId);

            var freightRows = new List<RowModel>()
            {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                 new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
            };

            var commonCols = new List<ColumnModel>()
            {
                new ColumnModel("Data", 20, 12, ""),
                new ColumnModel("Total Cost", 20, 12, ""),
                new ColumnModel("Remarks", 30, 12, ""),
            };

            List<TableModel> freightData = new List<TableModel>();
            int total20FtContainers = 0;
            int total40FtContainers = 0;
            decimal finalOverallTotal = 0;

            foreach (var val in freightMasterData.TenderPortNames)
            {
                TableModel mod = new TableModel();
                List<RowModel> rowList = new List<RowModel>();
                mod.Rows = new List<RowModel>();
                mod.Id = val.Id;
                mod.Title = val.PortName;
                rowList = new List<RowModel>()
                {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                 };

                foreach (var data in rowList)
                {
                    var temp = freightMasterData.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                    if (temp != null)
                    {
                        if (data.KeyName == "NoOfFortyFtContainers")
                            total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                        if (data.KeyName == "OverallTotal")
                            finalOverallTotal += temp.Cost;
                        data.Value = temp.Cost;
                        data.Data = temp.Data;
                        data.Remark = temp.Remarks;
                    }
                }
                var no20FtContainers = 0;
                var tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                if (tempData.Data > 0)
                    no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                total20FtContainers += no20FtContainers;

                decimal pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                mod.Rows.AddRange(new List<RowModel>()
                {
                    new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                });

                mod.Rows.AddRange(rowList);

                freightData.Add(mod);
            }

            var freightTotal = new List<RowModel>()
            {
                new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
            };
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(FOB)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                foreach (var currency in otherCurrencies)
                {
                    mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                    {
                        SubColumns = new List<string>() { currency.Name.ToUpper() },
                        UniqueId = x.Id,
                        Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                        ExtraKey = x.CurrencyName,
                        ExtraValue = x.CurrencyValue,
                        Rows = new List<dynamic>()
                    }); index++;
                }

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "USD" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "EURO" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            currencyConvRate = tenderView.IntTndValues.Where(x => x.Description == "ConversionRate").Select(y => y.Value).FirstOrDefault();
            euroToUsd = usdToInr == 0 ? 0 : ((euroToInrCost / usdToInr) + (currencyConvRate / 100));
            containerCharges = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").Select(y => y.Value).FirstOrDefault();

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;
                indiaCostSummation += Truncate(indiaCost, 3);

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                boSummation += Truncate(mod.Quantity * boCost, 3);

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);
                costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);
            }


            #region Internal Calculation

            dividingFactor40Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor40Ft").FirstOrDefault().Value;
            dividingFactor20Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor20Ft").FirstOrDefault().Value;
            considered40FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered40FtCntr").FirstOrDefault().Value;
            considered20FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered20FtCntr").FirstOrDefault().Value;
            dollarsPer40Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").FirstOrDefault().Value;
            dollarsPer20Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPer20FtContainer").FirstOrDefault().Value;

            if (dividingFactor20Ft == 0)
                sizeWise20FtContr = 0;
            else
                sizeWise20FtContr = Truncate(mt / dividingFactor20Ft, 1);

            if (dividingFactor40Ft == 0)
                sizeWise40FtContr = 0;
            else
                sizeWise40FtContr = Truncate((mt / dividingFactor40Ft), 1);

            if (sizeWise20FtContr > 0 && sizeWise20FtContr < 1)
                sizeWise20FtContr = 1;

            if (sizeWise40FtContr >= 0 && sizeWise40FtContr < 1)
                sizeWise40FtContr = 1;


            rsPer40FtContainer = dollarsPer40Ft * usdToInr;
            rsPer20FtContainer = dollarsPer20Ft * usdToInr;

            considered20FtCntr = Math.Max(0, Math.Ceiling((mt - (considered40FtCntr * dividingFactor40Ft)) / dividingFactor20Ft));

            totalRsPer20FtContainers = considered20FtCntr * rsPer20FtContainer;
            totalRsPer40FtContainers = considered40FtCntr * rsPer40FtContainer;

            totalRsForCon = totalRsPer20FtContainers + totalRsPer40FtContainers;

            if (costOfSalesSummation == 0)
            {
                distFact = 0;
            }
            else
            {
                distFact = Truncate(Math.Ceiling((totalRsForCon / costOfSalesSummation) * 100), 1);
            }

            var rows = new List<RowModel>()
            {
                new RowModel(){ Description = "TOTAL UNIT WEIGHT (MT)",Value=mt},
                new RowModel(){ Description = "WEIGHT PER CONTAINERS",value20FtContr=dividingFactor20Ft,value40FtContr=dividingFactor40Ft },
                new RowModel(){ Description = "NO OF CONTAINERS SIZE WISE" ,value20FtContr=sizeWise20FtContr,value40FtContr=sizeWise40FtContr},
                new RowModel(){ Description = "CONTAINERS TOBE CONSIDER",value40FtContr=considered40FtCntr,value20FtContr=considered20FtCntr},
                new RowModel(){ Description = "DOLLARS PER CONTAINER",value40FtContr=dollarsPer40Ft,value20FtContr=dollarsPer20Ft},
                new RowModel(){ Description = "₹ PER CONTAINER" ,value40FtContr=rsPer40FtContainer,value20FtContr=rsPer20FtContainer},
                new RowModel(){ Description = "TOTAL ₹ PER CONTAINERS" ,value40FtContr=totalRsPer40FtContainers,value20FtContr=totalRsPer20FtContainers},
                new RowModel(){ Description = "FACTOR FOR DISTRIBUTING FREIGHT" ,Value=distFact},
            };

            var columns = new List<ColumnModel>()
            {
                new ColumnModel("Containers", 40, 12, "") { Value = mt, Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } },
                new ColumnModel("40 Ft", 40, 12, "") {Rows = new List<dynamic>(){ }, SubColumns = new List<string>() { "" } },
                new ColumnModel("20 Ft", 40, 12, "") {Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } }

            };


            #endregion

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);

                spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);
                exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                spEuro = euroToInrCost == 0 ? 0 : (spInr / euroToInrCost);
                spEuro = Truncate(spEuro, 3);
                spUsd = spEuro * euroToUsd;
                spUsd = Truncate(spUsd, 3);
                exWorksEuro += Truncate(mod.Quantity * spEuro, 3);
                exWorksUsd += Truncate(mod.Quantity * spUsd, 3);
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            var last = tenderView.MasterList.Last();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spEuro = euroToInrCost == 0 ? 0 : Truncate(spInr / euroToInrCost, 3);

                    spUsd = spEuro * euroToUsd;

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));

                    foreach (var currency in otherCurrencies)
                    {
                        euroToOtherCurrency = (euroToInrCost / currency.Value) + (currencyConvRate / 100);
                        var value = spEuro * euroToOtherCurrency;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = Truncate(value, 3) * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), qtyValue));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");

                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }
                var lastPortId = 0;
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        var seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        cifPort = exWorksSumInr == 0 ? 0 : Truncate(spEuro * (1 + (seaFreight / 100)), 1);

                        if (col.SubColumns.Contains("USD"))
                        {
                            cifPortUsd = cifPort * (euroToUsd);
                            col.Rows.Add(Truncate(cifPortUsd, 1));
                            col.Summation = col.Summation + Truncate(cifPortUsd, 1) * mod.Quantity;
                        }
                        else if (col.SubColumns.Contains("EURO"))
                        {
                            col.Rows.Add(cifPort);
                            col.Summation = col.Summation + Truncate(cifPort, 1) * mod.Quantity;
                            lastPortId = col.UniqueId;
                        }
                        else
                        {
                            otherCurrencyValue = otherCurrencies.Where(x => x.Name.ToUpper() == col.SubColumns.First()).FirstOrDefault().Value;
                            // otherCurrencyValue += currencyConvRate / 100;
                            euroToOtherCurrency = (euroToInrCost / otherCurrencyValue) + (currencyConvRate / 100);
                            cifPortOtherCurrency = Truncate(cifPort * (euroToOtherCurrency), 1);
                            col.Rows.Add(Truncate(cifPortOtherCurrency, 3));
                            col.Summation = col.Summation + Truncate(cifPortOtherCurrency, 1) * mod.Quantity;
                        }

                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksEuro, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksUsd, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }
            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                for (int i = 1; i <= rawMatColumns.Count; i++)
                {
                    rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
                    rawMatPricing.Row(1).Height = 25;
                    cell = rawMatPricing.Cells[1, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = rawMatColumns[i - 1].Name;
                    cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
                }

                for (int i = 1; i <= rawMatMaster.Count; i++)
                {
                    for (int j = 1; j <= rawMatColumns.Count; j++)
                    {
                        cell = rawMatPricing.Cells[i + 1, j];
                        cell = GiveCellStyleProperties(cell);
                        rawMatPricing.Row(i + 1).Height = 20;
                        if (rawMatColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = string.Empty;
                        }
                    }
                }
                #endregion

                #region test master pricing

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";
                int testCol = 1;
                int testRow = 1;
                for (int i = 1; i <= testColumns.Count; i++)
                {
                    if (testColumns[i - 1].SubColumns == null)
                    {

                        testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                        testPricing.Row(testRow).Height = 25;

                        testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                        cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);

                        if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                        {
                            decimal value = 0;

                            if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                            cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                        }
                    }
                    else
                    {
                        testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                        testPricing.Row(testRow).Height = 25;
                        testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                        cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                        cell.Value = testColumns[i - 1].Name;
                        cell = GiveCellStyleHeaderProperties(cell);

                        colCount = i;

                        foreach (var col in testColumns[i - 1].SubColumns)
                        {
                            testPricing.Column(testCol).Width = 20;
                            testPricing.Row(testRow).Height = 25;
                            cell = testPricing.Cells[testRow + 1, testCol];
                            cell.Value = col.ToUpper();
                            cell = GiveCellStyleHeaderProperties(cell);
                            testCol++;
                        }

                        testCol--;

                    }

                    testCol++;

                }
                testRow += 2;
                testCol = 1;
                for (int i = 1; i <= testMaster.TestList.Count; i++)
                {
                    testCol = 1;
                    for (int j = 1; j <= testColumns.Count; j++)
                    {
                        if (testColumns[j - 1].SubColumns == null)
                        {
                            cell = testPricing.Cells[testRow, testCol];
                            cell = GiveCellStyleProperties(cell);
                            if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                            {
                                if (testColumns[j - 1].UseValue)
                                {
                                    cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                                }
                                else
                                {
                                    cell.Value = i;
                                }
                            }
                            else
                            {
                                decimal value = 0;

                                if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                    value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                            }
                        }
                        else
                        {
                            foreach (var column in testColumns[j - 1].SubColumns)
                            {
                                cell = testPricing.Cells[testRow, testCol];
                                cell = GiveCellStyleProperties(cell);
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                                testCol++;
                            }

                            testCol--;
                        }
                        testCol++;
                    }
                    testRow++;
                }
                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                int bgRowCount = 1;
                int bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Contract Value : " + contractValue;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Delivery Month : " + deliveryMonth;
                cell = GiveCellStyleHeaderProperties(cell);
                bgRowCount++;
                bgColCount = 1;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
                cell.Value = "Grace Period : " + gracePeriod;
                cell = GiveCellStyleHeaderProperties(cell);
                bgColCount += 3;

                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
                cell.Value = "Performance Period : " + performancePeriod;
                cell = GiveCellStyleHeaderProperties(cell);

                bankGuaranteeSheet.Row(1).Height = 25;
                bankGuaranteeSheet.Row(2).Height = 25;
                bankGuaranteeSheet.Column(1).Width = 90;
                bankGuaranteeSheet.Column(3).Width = 90;

                for (int i = 1; i <= bgColumns.Count; i++)
                {
                    bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    bankGuaranteeSheet.Row(3).Height = 25;
                    cell = bankGuaranteeSheet.Cells[3, i];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = bgColumns[i - 1].Name;
                }

                for (int i = 1; i <= bgList.Count; i++)
                {
                    for (int j = 1; j <= bgColumns.Count; j++)
                    {
                        cell = bankGuaranteeSheet.Cells[i + 3, j];
                        cell = GiveCellStyleProperties(cell);

                        if (bgColumns[j - 1].UseValue)
                        {
                            cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }
                bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
                cell = bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count];
                cell = GiveCellStyleProperties(cell);

                int whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region markup

                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                markupSheet.Name = "Markup";

                int markupRowCount = 1;
                int markupColCount = 1;

                foreach (var col in markupColumns)
                {
                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = col.Name;
                    markupSheet.Column(markupColCount).Width = col.Width;
                    markupColCount++;
                }

                markupRowCount = 2;
                markupColCount = 1;
                foreach (var mark in markupDataset.MarkupDetails)
                {
                    bool isPBG = false;
                    isPBG = mark.MarkupId == 16 ? true : false;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = mark.Markup;
                    markupColCount++;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = isPBG ? (markupDataset.TndType == 2 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;

                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = isPBG ? (markupDataset.TndType == 1 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;


                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                    cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                    markupColCount++;


                    switch (mark.MarkupId)
                    {
                        case 10: //financing
                            {

                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 17: //financing sales cr
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 16: //pbg
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }
                                break;
                            }
                        case 18: //interest savings on advance
                            {
                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }

                                string value2 = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                if (!string.IsNullOrEmpty(value))
                                {
                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
                                    cell.Value = value2;
                                    cell = GiveCellStyleProperties(cell, true);
                                    markupColCount++;
                                }

                                break;
                            }
                    }

                    markupColCount = 1;
                    markupRowCount++;
                }
                markupSheet.Column(5).Width = 20;
                markupSheet.Column(6).Width = 20;

                int markupWhile = 1;
                while (markupWhile < markupRowCount)
                {
                    markupSheet.Row(markupWhile).Height = 20;
                    markupWhile++;
                }

                markupWhile++;

                //Create table for travel, lodging and boarding
                markupSheet.Cells[2, 8, 2, 9].Merge = true;
                cell = markupSheet.Cells[2, 8, 2, 9];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Travel, Lodging and Boarding";

                cell = markupSheet.Cells[3, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "No. Of Persons";

                cell = markupSheet.Cells[3, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[4, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "No. Of Days";

                cell = markupSheet.Cells[4, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[5, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "Fare";

                cell = markupSheet.Cells[5, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;

                cell = markupSheet.Cells[6, 8];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = "Lodging";

                cell = markupSheet.Cells[6, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;

                //PackingPercentage
                decimal pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                int tempRowCount = 8, tempColCount = 8;

                pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
                cell = markupSheet.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Packing Material Weight as Percentage";

                cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = pckPercentage;

                cell = markupSheet.Cells[10, 8];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Cross Currency Margin";

                cell = markupSheet.Cells[10, 9];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                //Create table for currency master
                markupSheet.Cells[12, 8, 12, 11].Merge = true;
                cell = markupSheet.Cells[12, 8, 12, 11];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Currency";

                cell = markupSheet.Cells[13, 8];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "International Currency";

                cell = markupSheet.Cells[13, 9];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Convert Into INR";

                cell = markupSheet.Cells[13, 10];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Actual Value";

                cell = markupSheet.Cells[13, 11];
                cell = GiveCellStyleProperties(cell, true, true);
                cell.Value = "Considered Value";

                tempRowCount = 14;
                tempColCount = 8;

                decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                        cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                        cell = GiveCellStyleProperties(cell, true, true);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = markupSheet.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = currency.Name;

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }


                markupSheet.Column(8).Width = 40;
                markupSheet.Column(9).Width = 20;
                markupSheet.Column(10).Width = 20;
                markupSheet.Column(11).Width = 20;

                //var markupLastCol = markupSheet.Dimension.End.Column + 1;
                //cell = markupSheet.Cells[1, markupLastCol];

                #endregion

                #region freight

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";
                freightSheet.View.FreezePanes(1, 2);

                int freightRowCount = 1;
                int freightColCount = 1;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                freightSheet.Column(freightColCount).Width = 50;
                cell.Value = "Freight";
                cell = GiveCellStyleHeaderProperties(cell);
                freightRowCount += 1;

                var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
                var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

                foreach (var col in secondayCols)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell.Value = col;
                    if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                        cell = GiveCellStyleHeaderProperties(cell);
                    else
                        cell = GiveCellStyleProperties(cell);
                    freightRowCount++;
                }

                cell = freightSheet.Cells[freightRowCount, 1];
                cell = GiveCellSubHeaderProperties(cell);
                freightColCount = 1;
                freightRowCount++;
                foreach (var col in actualCols)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell.Value = col;
                    if (col == "Containers" || col == "Air Freight Cost")
                        cell = GiveCellStyleHeaderProperties(cell);
                    else
                        cell = GiveCellStyleProperties(cell);
                    freightRowCount++;
                }

                freightColCount = 2;

                foreach (var mod in freightData)
                {
                    freightRowCount = 1;

                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = mod.Title;
                    freightRowCount++;
                    freightColCount += 2;
                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                    {
                        freightColCount -= 2;

                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Value;

                        freightColCount += 2;
                        freightRowCount++;
                    }

                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                    {
                        freightColCount -= 2;

                        if (rowCol.IsHeading)
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = rowCol.SubColumns[0].Name;
                            freightColCount += 2;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = rowCol.SubColumns[1].Name;
                        }
                        else
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = rowCol.Value;
                            freightColCount += 2;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = rowCol.Data;
                        }
                        freightRowCount++;

                    }



                    //freightColCount -= 2;
                    cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Data";
                    freightSheet.Column(freightColCount - 2).Width = 15;

                    cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Total Cost";
                    freightSheet.Column(freightColCount - 1).Width = 15;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = "Remarks";
                    freightSheet.Column(freightColCount).Width = 30;

                    freightRowCount++;
                    foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                    {
                        freightColCount -= 2;
                        if (row.IsHeading)
                        {
                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                            cell = GiveCellStyleHeaderProperties(cell);
                            freightColCount += 2;
                        }
                        else
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Data;
                            freightColCount++;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Value;
                            freightColCount++;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Remark;

                        }
                        freightRowCount++;
                    }


                    freightColCount += 2;
                }

                freightRowCount = 1;
                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Freight Total";
                freightRowCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Sr. No.";
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Description";
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Value";
                freightRowCount++;
                freightColCount -= 2;
                count = 1;
                foreach (var row in freightTotal)
                {
                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = count;
                    freightColCount++;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Description;
                    freightSheet.Column(freightColCount).Width = 30;
                    freightColCount++;

                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Value;
                    freightSheet.Column(freightColCount).Width = 20;
                    freightColCount -= 2;
                    freightRowCount++;
                    count++;
                }

                int freightWhileCount = 1;
                while (freightWhileCount < freightRowCount)
                {
                    freightSheet.Row(freightWhileCount).Height = 20;
                    freightWhileCount++;
                }

                #endregion

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                FillCommonFields(ref range, tenderView);

                int mainRowCount = 9;
                int mainColCount = 1;
                foreach (var cols in mainColumns)
                {
                    tenderPricing.Column(mainColCount).Width = cols.Width;

                    if (cols.Exclude == true)
                    {
                        bool isMerged = tenderPricing.Cells[mainRowCount, mainColCount].Merge;
                        if (isMerged)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = cols.Name;
                            mainRowCount++;
                        }
                        else
                        {
                            tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1 + otherCurrencies.Count].Merge = true;
                            cell = tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = cols.Name;
                            mainRowCount++;
                        }
                    }
                    else
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = cols.Name;
                        mainRowCount++;
                    }

                    if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = cols.SubColumns[0];
                    }

                    mainRowCount++;

                    foreach (var row in cols.Rows)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row;
                        mainRowCount++;
                    }

                    //ExcelRange cell1 = tenderPricing.Cells[mainRowCount, mainColCount]; ;

                    mainColCount++;
                    mainRowCount = 9;
                }

                tempRowCount = mainRowCount;
                tempColCount = tenderPricing.Dimension.End.Column + 2;

                whileCount = 3;
                while (whileCount < tenderPricing.Dimension.End.Row)
                {
                    tenderPricing.Row(whileCount).Height = 18;
                    whileCount++;
                }

                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                cell = GiveLastRowHighlightProperties(cell);

                //Create table for currency master
                tempRowCount = 1;
                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Packing Material Weight as Percentage";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = pckPercentage;
                tempRowCount += 2;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Cross Currency Margin";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempRowCount += 2;
                //Create table for currency master
                tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Currency";
                tempRowCount++;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "International Currency";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Convert Into INR";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Actual Value";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Considered Value";
                tempRowCount++;

                tempEuroValue = 0;
                tempUsdValue = 0; tempCurrencyValue = 0;
                conversionRate = 0;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                        cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = currency.Name;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }

                tenderPricing.Column(1).Width = 30;
                tenderPricing.Column(tempColCount).Width = 45;
                tenderPricing.Column(tempColCount + 1).Width = 20;
                tenderPricing.Column(tempColCount + 2).Width = 20;
                tenderPricing.Column(tempColCount + 3).Width = 20;
                #endregion

                #region Internal Calculations

                var calculations = excelPackage.Workbook.Worksheets.Add("Internal Calculations");
                calculations.Name = "Container Details";

                cell = calculations.Cells[1, 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Containers";

                cell = calculations.Cells[1, 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "40Ft";

                cell = calculations.Cells[1, 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "20Ft";

                tempRowCount = 2;
                tempColCount = 1;
                foreach (var row in rows)
                {
                    tempColCount = 1;

                    cell = calculations.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Description;
                    tempColCount++;
                    if (row.Description.ToUpper() != "FACTOR FOR DISTRIBUTING FREIGHT" && row.Description.ToUpper() != "TOTAL UNIT WEIGHT (MT)")
                    {
                        cell = calculations.Cells[tempRowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.value40FtContr;
                        tempColCount++;

                        cell = calculations.Cells[tempRowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.value20FtContr;
                    }
                    else
                    {
                        calculations.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1].Merge = true;
                        cell = calculations.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Value;
                    }
                    tempRowCount++;
                }
                calculations.Column(1).Width = 35;
                calculations.Column(2).Width = 20;
                calculations.Column(3).Width = 20;

                whileCount = 1;
                while (whileCount < tempRowCount)
                {
                    calculations.Row(whileCount).Height = 20;
                    whileCount++;
                }
                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] DownloadInternationalTenderPricingCustomerDataIG(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            int count = 0;
            decimal otherCurrencyValue = 0, mt = 0, loadingFactor = 0, dividingFactor40Ft, dividingFactor20Ft, considered40FtCntr, considered20FtCntr, dollarsPer40Ft, dollarsPer20Ft,
                rsPer40FtContainer, rsPer20FtContainer, rsPer40FtContainers, rsPer20FtContainers, totalRsPer40FtContainers, totalRsPer20FtContainers, usdToInr = 0, sizeWise20FtContr, pckPercentage,
                sizeWise40FtContr, totalRsForCon;

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, cifPort = 0, cifPortUsd = 0, euroToOtherCurrency, cifPortOtherCurrency;

            decimal euroToUsd = 0, spEuro = 0, spUsd = 0, noOfCon = 0, rsPerCon = 0, distFact = 0, containerCharges = 0, currencyConvRate = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksEuro = 0, exWorksUsd = 0;

            List<CurrencyModel> otherCurrencies = null;

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(FOB)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                foreach (var currency in otherCurrencies)
                {
                    mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                    {
                        SubColumns = new List<string>() { currency.Name.ToUpper() },
                        UniqueId = x.Id,
                        Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                        ExtraKey = x.CurrencyName,
                        ExtraValue = x.CurrencyValue,
                        Rows = new List<dynamic>()
                    }); index++;
                }

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "USD" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "EURO" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            currencyConvRate = tenderView.IntTndValues.Where(x => x.Description == "ConversionRate").Select(y => y.Value).FirstOrDefault();
            euroToUsd = usdToInr == 0 ? 0 : ((euroToInrCost / usdToInr) + (currencyConvRate / 100));
            containerCharges = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").Select(y => y.Value).FirstOrDefault();

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;
                indiaCostSummation += Truncate(indiaCost, 3);

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                boSummation += Truncate(mod.Quantity * boCost, 3);

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);
                costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);
            }


            #region Internal Calculation

            dividingFactor40Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor40Ft").FirstOrDefault().Value;
            dividingFactor20Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor20Ft").FirstOrDefault().Value;
            considered40FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered40FtCntr").FirstOrDefault().Value;
            considered20FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered20FtCntr").FirstOrDefault().Value;
            dollarsPer40Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").FirstOrDefault().Value;
            dollarsPer20Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPer20FtContainer").FirstOrDefault().Value;

            if (dividingFactor20Ft == 0)
                sizeWise20FtContr = 0;
            else
                sizeWise20FtContr = Truncate(mt / dividingFactor20Ft, 1);

            if (dividingFactor40Ft == 0)
                sizeWise40FtContr = 0;
            else
                sizeWise40FtContr = Truncate((mt / dividingFactor40Ft), 1);

            if (sizeWise20FtContr > 0 && sizeWise20FtContr < 1)
                sizeWise20FtContr = 1;

            if (sizeWise40FtContr >= 0 && sizeWise40FtContr < 1)
                sizeWise40FtContr = 1;


            rsPer40FtContainer = dollarsPer40Ft * usdToInr;
            rsPer20FtContainer = dollarsPer20Ft * usdToInr;

            considered20FtCntr = Math.Max(0, Math.Ceiling((mt - (considered40FtCntr * dividingFactor40Ft)) / dividingFactor20Ft));

            totalRsPer20FtContainers = considered20FtCntr * rsPer20FtContainer;
            totalRsPer40FtContainers = considered40FtCntr * rsPer40FtContainer;

            totalRsForCon = totalRsPer20FtContainers + totalRsPer40FtContainers;

            if (costOfSalesSummation == 0)
            {
                distFact = 0;
            }
            else
            {
                distFact = Truncate(Math.Ceiling((totalRsForCon / costOfSalesSummation) * 100), 1);
            }

            var rows = new List<RowModel>()
            {
                new RowModel(){ Description = "TOTAL UNIT WEIGHT (MT)",Value=mt},
                new RowModel(){ Description = "WEIGHT PER CONTAINERS",value20FtContr=dividingFactor20Ft,value40FtContr=dividingFactor40Ft },
                new RowModel(){ Description = "NO OF CONTAINERS SIZE WISE" ,value20FtContr=sizeWise20FtContr,value40FtContr=sizeWise40FtContr},
                new RowModel(){ Description = "CONTAINERS TOBE CONSIDER",value40FtContr=considered40FtCntr,value20FtContr=considered20FtCntr},
                new RowModel(){ Description = "DOLLARS PER CONTAINER",value40FtContr=dollarsPer40Ft,value20FtContr=dollarsPer20Ft},
                new RowModel(){ Description = "₹ PER CONTAINER" ,value40FtContr=rsPer40FtContainer,value20FtContr=rsPer20FtContainer},
                new RowModel(){ Description = "TOTAL ₹ PER CONTAINERS" ,value40FtContr=totalRsPer40FtContainers,value20FtContr=totalRsPer20FtContainers},
                new RowModel(){ Description = "FACTOR FOR DISTRIBUTING FREIGHT" ,Value=distFact},
            };

            var columns = new List<ColumnModel>()
            {
                new ColumnModel("Containers", 40, 12, "") { Value = mt, Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } },
                new ColumnModel("40 Ft", 40, 12, "") {Rows = new List<dynamic>(){ }, SubColumns = new List<string>() { "" } },
                new ColumnModel("20 Ft", 40, 12, "") {Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } }

            };


            #endregion

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);

                spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);
                exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                spEuro = euroToInrCost == 0 ? 0 : (spInr / euroToInrCost);
                spEuro = Truncate(spEuro, 3);
                spUsd = spEuro * euroToUsd;
                spUsd = Truncate(spUsd, 3);
                exWorksEuro += Truncate(mod.Quantity * spEuro, 3);
                exWorksUsd += Truncate(mod.Quantity * spUsd, 3);
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            var last = tenderView.MasterList.Last();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spEuro = euroToInrCost == 0 ? 0 : Truncate(spInr / euroToInrCost, 3);

                    spUsd = spEuro * euroToUsd;

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));

                    foreach (var currency in otherCurrencies)
                    {
                        euroToOtherCurrency = (euroToInrCost / currency.Value) + (currencyConvRate / 100);
                        var value = spEuro * euroToOtherCurrency;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = Truncate(value, 3) * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), qtyValue));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");

                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }
                var lastPortId = 0;
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        var seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        cifPort = exWorksSumInr == 0 ? 0 : Truncate(spEuro * (1 + (seaFreight / 100)), 1);

                        if (col.SubColumns.Contains("USD"))
                        {
                            cifPortUsd = cifPort * (euroToUsd);
                            col.Rows.Add(Truncate(cifPortUsd, 1));
                            col.Summation = col.Summation + Truncate(cifPortUsd, 1) * mod.Quantity;
                        }
                        else if (col.SubColumns.Contains("EURO"))
                        {
                            col.Rows.Add(cifPort);
                            col.Summation = col.Summation + Truncate(cifPort, 1) * mod.Quantity;
                            lastPortId = col.UniqueId;
                        }
                        else
                        {
                            otherCurrencyValue = otherCurrencies.Where(x => x.Name.ToUpper() == col.SubColumns.First()).FirstOrDefault().Value;
                            // otherCurrencyValue += currencyConvRate / 100;
                            euroToOtherCurrency = (euroToInrCost / otherCurrencyValue) + (currencyConvRate / 100);
                            cifPortOtherCurrency = Truncate(cifPort * (euroToOtherCurrency), 1);
                            col.Rows.Add(Truncate(cifPortOtherCurrency, 3));
                            col.Summation = col.Summation + Truncate(cifPortOtherCurrency, 1) * mod.Quantity;
                        }

                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksEuro, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksUsd, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }
            #endregion

            int tempRowCount = 1, tempColCount = 1;
            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                //ExcelRange range = tenderPricing.Cells[1, 1, 7, 2];
                //FillCommonFields(ref range, tenderView);

                int mainRowCount = 1;
                int mainColCount = 1;
                foreach (var cols in mainColumns)
                {
                    tenderPricing.Column(mainColCount).Width = cols.Width;

                    if (cols.Exclude == true)
                    {
                        bool isMerged = tenderPricing.Cells[mainRowCount, mainColCount].Merge;
                        if (isMerged)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = cols.Name;
                            mainRowCount++;
                        }
                        else
                        {
                            tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1 + otherCurrencies.Count].Merge = true;
                            cell = tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = cols.Name;
                            mainRowCount++;
                        }
                    }
                    else
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = cols.Name;
                        mainRowCount++;
                    }

                    if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellSubHeaderProperties(cell);
                        cell.Value = cols.SubColumns[0];
                    }

                    mainRowCount++;

                    foreach (var row in cols.Rows)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row;
                        mainRowCount++;
                    }

                    //ExcelRange cell1 = tenderPricing.Cells[mainRowCount, mainColCount]; ;

                    mainColCount++;
                    mainRowCount = 1;
                }

                tempRowCount = mainRowCount;
                tempColCount = tenderPricing.Dimension.End.Column + 2;

                whileCount = 3;
                while (whileCount < tenderPricing.Dimension.End.Row)
                {
                    tenderPricing.Row(whileCount).Height = 18;
                    whileCount++;
                }

                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
                cell = GiveLastRowHighlightProperties(cell);

                //Create table for currency master
                tempRowCount = 1;
                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Packing Material Weight as Percentage";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = pckPercentage;
                tempRowCount += 2;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Cross Currency Margin";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempRowCount += 2;
                //Create table for currency master
                tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Currency";
                tempRowCount++;

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "International Currency";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Convert Into INR";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Actual Value";

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = "Considered Value";
                tempRowCount++;

                decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate = 0;

                conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                count = 0;
                foreach (var currency in currencyData.List)
                {
                    if (count == 2)
                    {
                        tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                        cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Euro To Other Currencies";
                        tempRowCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = currency.Name;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(currency.Value, 3);

                    if (currency.Name.ToLower() == "euro")
                    {
                        tempCurrencyValue = (tempEuroValue / tempUsdValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else if (currency.Name.ToLower() == "usd")
                    {
                        tempCurrencyValue = (tempUsdValue / tempEuroValue);

                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }
                    else
                    {
                        tempCurrencyValue = (tempEuroValue / currency.Value);
                        cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);
                    }

                    tempCurrencyValue += conversionRate / 100;

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);

                    tempRowCount++;
                    count++;
                }

                tenderPricing.Column(1).Width = 30;
                tenderPricing.Column(tempColCount).Width = 45;
                tenderPricing.Column(tempColCount + 1).Width = 20;
                tenderPricing.Column(tempColCount + 2).Width = 20;
                tenderPricing.Column(tempColCount + 3).Width = 20;
                #endregion
                tenderPricing.DeleteColumn(7 + (portList.Count * (2 + otherCurrencies.Count)), tenderPricing.Dimension.Columns - 1);
                tenderPricing.DeleteColumn(tenderPricing.Dimension.Columns);
                return excelPackage.GetAsByteArray();
            }
        }

        public byte[] IntTenderCompareRevisionI(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            decimal pckPercentage;
            int count, tempRowCount = 1, tempColCount;
            decimal otherCurrencyValue = 0, mt = 0, loadingFactor = 0, dividingFactor40Ft, dividingFactor20Ft, considered40FtCntr, considered20FtCntr, dollarsPer40Ft, dollarsPer20Ft,
                rsPer40FtContainer, rsPer20FtContainer, rsPer40FtContainers, rsPer20FtContainers, totalRsPer40FtContainers, totalRsPer20FtContainers, usdToInr = 0, sizeWise20FtContr,
                sizeWise40FtContr, totalRsForCon;

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, cifPort = 0, cifPortUsd = 0, euroToOtherCurrency, cifPortOtherCurrency;

            decimal euroToUsd = 0, spEuro = 0, spUsd = 0, noOfCon = 0, rsPerCon = 0, distFact = 0, containerCharges = 0, currencyConvRate = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksEuro = 0, exWorksUsd = 0;

            List<CurrencyModel> otherCurrencies = null;

            #region revisions
            _revisions = tndDetailsDAL.GetRevisionIds(tenderId);
            #endregion

            #region rawm Data

            rmpDAL = new RawMaterialPricingDAL();
            var revisions = new List<int>();
            var rawMatMaster = rmpDAL.GetRawMaterialPricingListForTender(tenderId, out revisions);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
            };

            revisions.ForEach(x => { rawMatColumns.Add(new ColumnModel(string.Format("Revision {0} Price (Rs)", x), 25, 12, x.ToString())); });

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGDataForAllRevisions(tenderId);

            #endregion

            #region markupData

            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingListForAllRevisions(tenderId, _revisions);

            #endregion

            #region freightData

            var freightMasterList = new List<IntFreightModel>();
            foreach (var rev in _revisions)
            {
                var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, rev);
                freightMasterList.Add(freightMasterData);
            }

            #endregion

            #region tender pricing view

            tndDetailsDAL = new TenderDetailsDAL();
            var mainViewList = new List<TenderDetailsModel>();
            foreach (var revision in _revisions)
            {
                var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, revision);
                mainViewList.Add(tenderView);
            }

            #endregion

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";

                #region Raw material pricing section

                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
                rawMatPricing.Name = "RawMaterialPricing";

                rowCount = 1;
                colCount = 1;
                foreach (var col in rawMatColumns)
                {
                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = col.Name;
                    rawMatPricing.Column(colCount).Width = col.Width;
                    colCount++;
                }

                rowCount++;
                foreach (var rawMat in rawMatMaster)
                {
                    colCount = 1;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.RawMaterialName;
                    colCount++;

                    cell = rawMatPricing.Cells[rowCount, colCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rawMat.GroupName;
                    colCount++;

                    foreach (var rev in rawMat.Pricing)
                    {
                        cell = rawMatPricing.Cells[rowCount, colCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rev.Price;
                        colCount++;
                    }
                    rowCount++;
                }

                whileCount = 1;
                while (whileCount <= rawMatPricing.Dimension.End.Row)
                {
                    rawMatPricing.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region testData

                var testColumns = new List<ColumnModel>() {
                    new ColumnModel("Sr.No", 8, 12,"Id", false),
                    new ColumnModel("Name", 30, 12,"TestName"),
                    new ColumnModel("Description", 50, 12,"TestDescription"),
                    new ColumnModel("Group Type", 30, 12,"Type"),
                    new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                    new ColumnModel("Line Type", 30, 12,"KVLine"),
                    new ColumnModel("UTS", 30, 12,"UTS"),
                    new ColumnModel("Summary", 50, 12,"Summary"),
                    new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                    new ColumnModel("Quantity", 20, 12,"Quantity"),
                    new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
                };

                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
                testPricing.Name = "Test Master Pricing";
                int testCol = 1;
                int testRow = 1;
                foreach (var rev in revisions)
                {
                    testDAL = new TestDAL();
                    var testMaster = testDAL.GetIntTestPricingList(tenderId, rev);

                    var tenderView = mainViewList.Where(x => x.TenderRevisionNo == rev).FirstOrDefault();

                    testPricing.Cells[testRow, 1, testRow, testColumns.Count + 2].Merge = true;
                    cell = testPricing.Cells[testRow, 1, testRow, testColumns.Count + 2];
                    cell.Value = "Revision " + rev;
                    cell = GiveCellStyleHeaderProperties(cell);
                    testRow++;

                    testCol = 1;
                    for (int i = 1; i <= testColumns.Count; i++)
                    {
                        if (testColumns[i - 1].SubColumns == null)
                        {

                            testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                            testPricing.Row(testRow).Height = 25;

                            testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                            cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                            cell.Value = testColumns[i - 1].Name;
                            cell = GiveCellStyleHeaderProperties(cell);

                            if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                            {
                                decimal value = 0;

                                if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                    value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                            }

                        }
                        else
                        {
                            testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                            testPricing.Row(testRow).Height = 25;
                            testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                            cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                            cell.Value = testColumns[i - 1].Name;
                            cell = GiveCellStyleHeaderProperties(cell);

                            colCount = i;

                            foreach (var columns in testColumns[i - 1].SubColumns)
                            {
                                testPricing.Column(testCol).Width = 20;
                                testPricing.Row(testRow).Height = 25;
                                cell = testPricing.Cells[testRow + 1, testCol];
                                cell.Value = columns.ToUpper();
                                cell = GiveCellStyleHeaderProperties(cell);
                                testCol++;
                            }

                            testCol--;

                        }

                        testCol++;

                    }
                    testRow += 2;
                    testCol = 1;
                    for (int i = 1; i <= testMaster.TestList.Count; i++)
                    {
                        testCol = 1;
                        for (int j = 1; j <= testColumns.Count; j++)
                        {
                            if (testColumns[j - 1].SubColumns == null)
                            {
                                cell = testPricing.Cells[testRow, testCol];
                                cell = GiveCellStyleProperties(cell);
                                if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                                {
                                    if (testColumns[j - 1].UseValue)
                                    {
                                        cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                                    }
                                    else
                                    {
                                        cell.Value = j;
                                    }
                                }
                                else
                                {
                                    decimal value = 0;

                                    if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                        value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                                    cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                                }
                            }
                            else
                            {
                                foreach (var column in testColumns[j - 1].SubColumns)
                                {
                                    cell = testPricing.Cells[testRow, testCol];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                                    testCol++;
                                }

                                testCol--;
                            }
                            testCol++;
                        }
                        testRow++;
                    }
                    testRow++;
                }

                #endregion

                #region bank guarantee

                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
                bankGuaranteeSheet.Name = "Bank Guarantee";

                whileCount = 1;
                while (whileCount <= 5)
                {
                    bankGuaranteeSheet.Column(whileCount).Width = 40;
                    whileCount++;
                }

                rowCount = 1;
                foreach (var bgModel in bgList)
                {
                    if (bgModel.Count > 0)
                    {
                        colCount = 1;

                        var bgVerticalCols = new List<ColumnModel>() {
                    new ColumnModel(string.Format("Revision {0}", bgModel.FirstOrDefault().TenderRevisionNo), 40, 12,""),
                    new ColumnModel("Bank Guarantee Type", 30, 12,""),
                    new ColumnModel("Bank Guarantee Month", 30, 12,""),
                    new ColumnModel("Commision (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee (%)", 30, 12,""),
                    new ColumnModel("Bank Guarantee Amount", 30, 12,""),
                    new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"")
                    };

                        var bghorizontalCols = new List<ColumnModel>()
                    {
                    new ColumnModel("Advance BG", 30, 12,""),
                    new ColumnModel("Performance BG", 30, 12,""),
                    new ColumnModel("Retension BG", 30, 12,""),
                    new ColumnModel("Total", 30, 12,"")
                    };

                        var contractValue = Truncate(bgModel.FirstOrDefault().ContractValue, 3);
                        var deliveryMonth = Truncate(bgModel.FirstOrDefault().DeliveryMonth, 3);
                        var performancePeriod = Truncate(bgModel.FirstOrDefault().PerformancePeriod, 3);
                        var gracePeriod = Truncate(bgModel.FirstOrDefault().GracePeriod, 3);

                        var bgHorizontalTopColumns = new List<ColumnModel>()
                    {
                    new ColumnModel("Contract Value", 30, 12,""){ Value = contractValue},
                    new ColumnModel("Performance Period", 30, 12,""){ Value = performancePeriod},
                    new ColumnModel("Grace Period", 30, 12,""){ Value = gracePeriod},
                    new ColumnModel("Delivery Month", 30, 12,""){ Value = deliveryMonth}
                    };

                        int innerCount = 0;
                        foreach (var col in bgVerticalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount + innerCount, colCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        colCount++;

                        innerCount = 0;
                        foreach (var col in bgHorizontalTopColumns)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = string.Format("{0} : {1}", col.Name, col.Value);
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var col in bghorizontalCols)
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = col.Name;
                            innerCount++;
                        }
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgMonth in bgModel.Select(x => x.BGMonth))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgMonth;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGMonth);
                        rowCount++;

                        innerCount = 0;
                        foreach (var commisionPer in bgModel.Select(x => x.CommisionPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = commisionPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.CommisionPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgpercent in bgModel.Select(x => x.BGPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgpercent;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGPercentage);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgAmount in bgModel.Select(x => x.BGAmount))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgAmount;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGAmount);
                        rowCount++;

                        innerCount = 0;
                        foreach (var bgCostPer in bgModel.Select(x => x.BGCostPercentage))
                        {
                            cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = bgCostPer;
                            innerCount++;
                        }

                        cell = bankGuaranteeSheet.Cells[rowCount, colCount + innerCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = bgModel.Sum(x => x.BGCostPercentage);
                        rowCount += 2;
                    }
                }

                whileCount = 1;
                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
                {
                    bankGuaranteeSheet.Row(whileCount).Height = 20;
                    whileCount++;
                }

                #endregion

                #region markup

                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
                markupSheet.Name = "Markup";

                int markupRowCount = 1;
                int markupColCount = 1;

                int incr = 0;

                foreach (var markup in markupDataset)
                {
                    var currencyData = markup.Currency;
                    incr = 0;
                    markupColCount = 1;

                    int startRowCount = markupRowCount;
                    int startColcount = markupColCount;

                    markupSheet.Cells[markupRowCount, markupColCount, markupRowCount, markupColCount + 3].Merge = true;
                    cell = markupSheet.Cells[markupRowCount, markupColCount, markupRowCount, markupColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = string.Format("Revision {0}", markup.TndRevNo);
                    markupRowCount++;

                    var markupColumns = new List<ColumnModel>()
                    {
                        new ColumnModel("Markup", 50, 0, "Markup"),
                        new ColumnModel("India", 30, 0, "India"),
                        new ColumnModel("Italy", 30, 0, "Italy"),
                        new ColumnModel("BO", 30, 0, "BO"),
                        //new ColumnModel("", 30, 0, ""),
                        //new ColumnModel("", 30, 0, "")
                    };

                    foreach (var col in markupColumns)
                    {
                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = col.Name;
                        markupSheet.Column(markupColCount).Width = col.Width;
                        incr++;
                    }

                    markupRowCount++;
                    incr = 0;
                    List<int> keys = new List<int>() { 10, 16, 17, 18 };
                    foreach (var mark in markup.MarkupDetails)
                    {
                        bool isPBG = false;
                        isPBG = mark.MarkupId == 16 ? true : false;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = mark.Markup;
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = isPBG ? (markup.TndType == 1 ? markup.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                            : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = isPBG ? (markup.TndType == 2 ? markup.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                            : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                        cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                        incr++;

                        switch (mark.MarkupId)
                        {
                            case 10: //financing
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 17: //financing sales cr
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 16: //pbg
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }
                                    break;
                                }
                            case 18: //interest savings on advance
                                {
                                    string value = markup.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }

                                    string value2 = markup.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        cell = markupSheet.Cells[markupRowCount, markupColCount + incr];
                                        cell.Value = value2;
                                        cell = GiveCellStyleProperties(cell, true);
                                        incr++;
                                    }

                                    break;
                                }
                        }

                        incr = 0;
                        markupRowCount++;
                    }

                    markupSheet.Column(5).Width = 20;
                    markupSheet.Column(6).Width = 20;

                    int markupWhile = 1;
                    while (markupWhile < markupRowCount)
                    {
                        markupSheet.Row(markupWhile).Height = 20;
                        markupWhile++;
                    }

                    markupWhile = 2;
                    while (markupWhile < 7)
                    {
                        markupSheet.Column(markupWhile).Width = 30;
                        markupWhile++;
                    }

                    var newRow = markupSheet.Dimension.End.Row - 23;
                    incr = 0;

                    //Create table for travel, lodging and boarding
                    markupSheet.Cells[newRow, 8, newRow, 9].Merge = true;
                    cell = markupSheet.Cells[newRow, 8, newRow, 9];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Travel, Lodging and Boarding";
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "No. Of Persons";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "No. Of Days";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "Fare";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = "Lodging";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;
                    incr += 2;

                    //PackingPercentage
                    pckPercentage = markup.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                    tempRowCount = newRow + incr;
                    tempColCount = 8;

                    pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
                    cell = markupSheet.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Packing Material Weight as Percentage";

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = pckPercentage;
                    incr += 2;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Cross Currency Margin";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = markup.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;
                    incr += 2;

                    //Create table for currency master
                    markupSheet.Cells[newRow + incr, 8, newRow + incr, 11].Merge = true;
                    cell = markupSheet.Cells[newRow + incr, 8, newRow + incr, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Currency";
                    incr++;

                    cell = markupSheet.Cells[newRow + incr, 8];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "International Currency";

                    cell = markupSheet.Cells[newRow + incr, 9];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Convert Into INR";

                    cell = markupSheet.Cells[newRow + incr, 10];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Actual Value";

                    cell = markupSheet.Cells[newRow + incr, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Considered Value";
                    incr++;

                    tempRowCount = newRow + incr;
                    tempColCount = 8;

                    decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

                    conversionRate = markup.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markup.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                    tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                    count = 0;
                    foreach (var currency in currencyData.List)
                    {
                        if (count == 2)
                        {
                            markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                            cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                            cell = GiveCellStyleProperties(cell, true, true);
                            cell.Value = "Euro To Other Currencies";
                            tempRowCount++;
                        }

                        cell = markupSheet.Cells[tempRowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = currency.Name;

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(currency.Value, 3);

                        if (currency.Name.ToLower() == "euro")
                        {
                            tempCurrencyValue = (tempEuroValue / tempUsdValue);

                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else if (currency.Name.ToLower() == "usd")
                        {
                            tempCurrencyValue = (tempUsdValue / tempEuroValue);

                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else
                        {
                            tempCurrencyValue = (tempEuroValue / currency.Value);
                            cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell, true);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }

                        tempCurrencyValue += conversionRate / 100;

                        cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                        cell = GiveCellStyleProperties(cell, true);
                        cell.Value = Truncate(tempCurrencyValue, 3);

                        tempRowCount++;
                        count++;
                    }


                    markupSheet.Column(8).Width = 40;
                    markupSheet.Column(9).Width = 20;
                    markupSheet.Column(10).Width = 20;
                    markupSheet.Column(11).Width = 20;
                    startRowCount += 2;

                    for (int i = startRowCount; i < markupRowCount; i++)
                    {
                        //cell = markupSheet.Cells[i, 5];

                        //if (string.IsNullOrEmpty(Convert.ToString(cell.Value)))
                        //    cell = GiveCellStyleProperties(cell);

                        //cell = markupSheet.Cells[i, 6];

                        //if (string.IsNullOrEmpty(Convert.ToString(cell.Value)))
                        //    cell = GiveCellStyleProperties(cell);
                    }
                    markupRowCount++;
                }


                #endregion

                #region freight
                rowCount = 1;
                colCount = 1;
                incr = 0;
                int incrCol = 0;

                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
                freightSheet.Name = "Freight";
                freightSheet.View.FreezePanes(1, 2);
                int freightRowCount = 1;
                int freightColCount = 1;

                foreach (var freight in freightMasterList)
                {
                    freightColCount = 1;
                    if (freight.TenderPortNames.Count > 0)
                    {
                        colCount = 1;
                        incrCol = 0;

                        var commonCols = new List<ColumnModel>()
                    {
                        new ColumnModel("Data", 20, 12, ""),
                        new ColumnModel("Total Cost", 20, 12, ""),
                        new ColumnModel("Remarks", 30, 12, ""),
                    };

                        List<TableModel> freightData = new List<TableModel>();

                        int total20FtContainers = 0;
                        int total40FtContainers = 0;
                        decimal finalOverallTotal = 0;

                        foreach (var val in freight.TenderPortNames)
                        {
                            TableModel mod = new TableModel();
                            List<RowModel> rowList = new List<RowModel>();
                            mod.Rows = new List<RowModel>();
                            mod.Id = val.Id;
                            mod.Title = val.PortName;
                            rowList = new List<RowModel>()
                            {
                                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                                new RowModel(){ Description = "Containers", IsHeading = true},
                                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                            };

                            foreach (var data in rowList)
                            {
                                var temp = freight.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                                if (temp != null)
                                {
                                    if (data.KeyName == "NoOfFortyFtContainers")
                                        total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                                    if (data.KeyName == "OverallTotal")
                                        finalOverallTotal += temp.Cost;
                                    data.Value = temp.Cost;
                                    data.Data = temp.Data;
                                    data.Remark = temp.Remarks;
                                }
                            }
                            var no20FtContainers = 0;
                            var tempData = freight.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                            no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                            tempData = freight.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                            if (tempData.Data > 0)
                                no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                            total20FtContainers += no20FtContainers;

                            pckPercentage = markupDataset.Where(x => x.TndRevNo == freight.TndRevNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                                markupDataset.Where(x => x.TndRevNo == freight.TndRevNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                            mod.Rows.AddRange(new List<RowModel>()
                            {
                                new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                                new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")} },
                                new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")} },
                                new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                                new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                                new ColumnModel("Rupees", 20,12,"")}},
                            });

                            mod.Rows.AddRange(rowList);

                            freightData.Add(mod);
                        }

                        var freightTotal = new List<RowModel>()
                        {
                            new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                            new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                            new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
                        };

                        freightSheet.Cells[freightRowCount, 1, freightRowCount, freightColCount + (freight.TenderPortNames.Count * 3)].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, 1, freightRowCount, freightColCount + (freight.TenderPortNames.Count * 3)];
                        cell.Value = "Revision " + freight.TndRevNo;
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightRowCount += 1;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        freightSheet.Column(freightColCount).Width = 50;
                        cell.Value = "Freight";
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightRowCount += 1;

                        var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
                        var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

                        foreach (var col in secondayCols)
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell.Value = col;
                            if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                                cell = GiveCellStyleHeaderProperties(cell);
                            else
                                cell = GiveCellStyleProperties(cell);
                            freightRowCount++;
                        }

                        cell = freightSheet.Cells[freightRowCount, 1];
                        cell = GiveCellSubHeaderProperties(cell);
                        freightColCount = 1;
                        freightRowCount++;
                        foreach (var col in actualCols)
                        {
                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell.Value = col;
                            if (col == "Containers" || col == "Air Freight Cost")
                                cell = GiveCellStyleHeaderProperties(cell);
                            else
                                cell = GiveCellStyleProperties(cell);
                            freightRowCount++;
                        }

                        freightColCount = 2;

                        foreach (var mod in freightData)
                        {
                            freightRowCount -= 29;

                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = mod.Title;
                            freightRowCount++;
                            freightColCount += 2;
                            foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                            {
                                freightColCount -= 2;

                                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                                cell = GiveCellStyleProperties(cell);
                                cell.Value = rowCol.Value;

                                freightColCount += 2;
                                freightRowCount++;
                            }

                            foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                            {
                                freightColCount -= 2;

                                if (rowCol.IsHeading)
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    cell.Value = rowCol.SubColumns[0].Name;
                                    freightColCount += 2;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    cell.Value = rowCol.SubColumns[1].Name;
                                }
                                else
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = rowCol.Value;
                                    freightColCount += 2;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = rowCol.Data;
                                }
                                freightRowCount++;

                            }

                            //freightColCount -= 2;
                            cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Data";
                            freightSheet.Column(freightColCount - 2).Width = 15;

                            cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Total Cost";
                            freightSheet.Column(freightColCount - 1).Width = 15;

                            cell = freightSheet.Cells[freightRowCount, freightColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = "Remarks";
                            freightSheet.Column(freightColCount).Width = 30;

                            freightRowCount++;
                            foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                            {
                                freightColCount -= 2;
                                if (row.IsHeading)
                                {
                                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                                    cell = GiveCellStyleHeaderProperties(cell);
                                    freightColCount += 2;
                                }
                                else
                                {
                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Data;
                                    freightColCount++;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Value;
                                    freightColCount++;

                                    cell = freightSheet.Cells[freightRowCount, freightColCount];
                                    cell = GiveCellStyleProperties(cell);
                                    cell.Value = row.Remark;

                                }
                                freightRowCount++;
                            }


                            freightColCount++;
                        }

                        int freightTempRowCount = freightRowCount - 29;
                        freightColCount++;
                        freightSheet.Cells[freightTempRowCount, freightColCount, freightTempRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightTempRowCount, freightColCount, freightTempRowCount, freightColCount + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Freight Total";
                        freightTempRowCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Sr. No.";
                        freightColCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Description";
                        freightColCount++;

                        cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = "Value";
                        freightTempRowCount++;
                        freightColCount -= 2;
                        count = 1;
                        foreach (var row in freightTotal)
                        {
                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = count;
                            freightColCount++;

                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Description;
                            freightSheet.Column(freightColCount).Width = 30;
                            freightColCount++;

                            cell = freightSheet.Cells[freightTempRowCount, freightColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Value;
                            freightSheet.Column(freightColCount).Width = 20;
                            freightColCount -= 2;
                            freightTempRowCount++;
                            count++;
                        }

                        int freightWhileCount = 1;
                        while (freightWhileCount < freightRowCount)
                        {
                            freightSheet.Row(freightWhileCount).Height = 20;
                            freightWhileCount++;
                        }

                        freightRowCount++;
                    }
                }

                #endregion

                #region mainView

                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
                tenderPricing.Name = "Tender Pricing";

                var calculations = excelPackage.Workbook.Worksheets.Add("Internal Calculations");
                calculations.Name = "Container Details";

                rowCount = 1;
                colCount = 1;
                int incrRow = 1;
                incrCol = 1;

                int portCount = 0;
                int mainRowCount = 1;
                int mainColCount = 1;
                int currencyCol = 12 + mainViewList.Max(x => x.TndPortDetails.Count) + 1;
                tempRowCount = 1;
                int calcRowCount = 1;
                foreach (var tenderView in mainViewList)
                {
                    otherCurrencies = tenderView.CurrencyList.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();
                    colCount = 1;
                    incrRow = 0;
                    incrCol = 0;
                    portCount = tenderView.TndPortDetails.Count;
                    var portList = tenderView.TndPortDetails;
                    loadingFactor = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;
                    pckPercentage = loadingFactor;
                    usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();

                    indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
                    italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
                    euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
                    boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");

                    tenderView.MasterList.ForEach(x =>
                    {
                        x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                        tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
                    });

                    var mainColumns = new List<ColumnModel>()
                    {
                        new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                        new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                        new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                        new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                        new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                        new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                        new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                        new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                        new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                        new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                        new ColumnModel("Selling Price(FOB)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                        new ColumnModel("Selling Price(FOB)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                        new ColumnModel("Selling Price(FOB)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                    };

                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Add(new ColumnModel("Selling Price(FOB)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
                    }

                    var index = 5;
                    portList.ForEach(x =>
                    {
                        foreach (var currency in otherCurrencies)
                        {
                            mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                            {
                                SubColumns = new List<string>() { currency.Name.ToUpper() },
                                UniqueId = x.Id,
                                Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                            tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                                ExtraKey = x.CurrencyName,
                                ExtraValue = x.CurrencyValue,
                                Rows = new List<dynamic>()
                            }); index++;
                        }

                        mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                        {
                            SubColumns = new List<string>() { "USD" },
                            UniqueId = x.Id,
                            Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                            tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                            ExtraKey = x.CurrencyName,
                            ExtraValue = x.CurrencyValue,
                            Rows = new List<dynamic>()
                        }); index++;

                        mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                        {
                            SubColumns = new List<string>() { "EURO" },
                            UniqueId = x.Id,
                            Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                            tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                            ExtraKey = x.CurrencyName,
                            ExtraValue = x.CurrencyValue,
                            Rows = new List<dynamic>()
                        }); index++;
                    });

                    currencyConvRate = tenderView.IntTndValues.Where(x => x.Description == "ConversionRate").Select(y => y.Value).FirstOrDefault();
                    euroToUsd = usdToInr == 0 ? 0 : ((euroToInrCost / usdToInr) + (currencyConvRate / 100));
                    containerCharges = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").Select(y => y.Value).FirstOrDefault();

                    foreach (var col in mainColumns)
                    {
                        switch (col.PropName)
                        {
                            case "srno":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                                    break;
                                }
                            case "desc":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                                    col.Rows.Add("Total");
                                    break;
                                }
                            case "unit":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                                    break;
                                }
                            case "drawno":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                                    break;
                                }
                            case "qty":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                                    break;
                                }
                            case "unitwt":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                                    mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                                    break;
                                }
                            case "indiacost":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
                                    break;
                                }
                            case "totalindiacost":
                                {
                                    col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                                    col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                                    break;
                                }
                        }
                    }

                    foreach (var mod in tenderView.MasterList)
                    {
                        indiaCost = mod.UnitCost;
                        indiaCostSummation += Truncate(indiaCost, 3);

                        boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                            tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                        boSummation += Truncate(mod.Quantity * boCost, 3);

                        costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);
                        costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);
                    }


                    #region Internal Calculation

                    dividingFactor40Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor40Ft").FirstOrDefault().Value;
                    dividingFactor20Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor20Ft").FirstOrDefault().Value;
                    considered40FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered40FtCntr").FirstOrDefault().Value;
                    considered20FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered20FtCntr").FirstOrDefault().Value;
                    dollarsPer40Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").FirstOrDefault().Value;
                    dollarsPer20Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPer20FtContainer").FirstOrDefault().Value;

                    if (dividingFactor20Ft == 0)
                        sizeWise20FtContr = 0;
                    else
                        sizeWise20FtContr = Truncate(mt / dividingFactor20Ft, 1);

                    if (dividingFactor40Ft == 0)
                        sizeWise40FtContr = 0;
                    else
                        sizeWise40FtContr = Truncate((mt / dividingFactor40Ft), 1);

                    if (sizeWise20FtContr > 0 && sizeWise20FtContr < 1)
                        sizeWise20FtContr = 1;

                    if (sizeWise40FtContr >= 0 && sizeWise40FtContr < 1)
                        sizeWise40FtContr = 1;


                    rsPer40FtContainer = dollarsPer40Ft * usdToInr;
                    rsPer20FtContainer = dollarsPer20Ft * usdToInr;

                    considered20FtCntr = Math.Max(0, Math.Ceiling((mt - (considered40FtCntr * dividingFactor40Ft)) / dividingFactor20Ft));

                    totalRsPer20FtContainers = considered20FtCntr * rsPer20FtContainer;
                    totalRsPer40FtContainers = considered40FtCntr * rsPer40FtContainer;

                    totalRsForCon = totalRsPer20FtContainers + totalRsPer40FtContainers;

                    if (costOfSalesSummation == 0)
                    {
                        distFact = 0;
                    }
                    else
                    {
                        distFact = Truncate(Math.Ceiling((totalRsForCon / costOfSalesSummation) * 100), 1);
                    }

                    var rows = new List<RowModel>()
                    {
                        new RowModel(){ Description = "TOTAL UNIT WEIGHT (MT)",Value=mt},
                        new RowModel(){ Description = "WEIGHT PER CONTAINERS",value20FtContr=dividingFactor20Ft,value40FtContr=dividingFactor40Ft },
                        new RowModel(){ Description = "NO OF CONTAINERS SIZE WISE" ,value20FtContr=sizeWise20FtContr,value40FtContr=sizeWise40FtContr},
                        new RowModel(){ Description = "CONTAINERS TOBE CONSIDER",value40FtContr=considered40FtCntr,value20FtContr=considered20FtCntr},
                        new RowModel(){ Description = "DOLLARS PER CONTAINER",value40FtContr=dollarsPer40Ft,value20FtContr=dollarsPer20Ft},
                        new RowModel(){ Description = "₹ PER CONTAINER" ,value40FtContr=rsPer40FtContainer,value20FtContr=rsPer20FtContainer},
                        new RowModel(){ Description = "TOTAL ₹ PER CONTAINERS" ,value40FtContr=totalRsPer40FtContainers,value20FtContr=totalRsPer20FtContainers},
                        new RowModel(){ Description = "FACTOR FOR DISTRIBUTING FREIGHT" ,Value=distFact},
                    };

                    var columns = new List<ColumnModel>()
                    {
                        new ColumnModel("Containers", 40, 12, "") { Value = mt, Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } },
                        new ColumnModel("40 Ft", 40, 12, "") {Rows = new List<dynamic>(){ }, SubColumns = new List<string>() { "" } },
                        new ColumnModel("20 Ft", 40, 12, "") {Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } }

                    };


                    #endregion

                    foreach (var mod in tenderView.MasterList)
                    {
                        indiaCost = mod.UnitCost;

                        boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                            tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                        costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);

                        spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);
                        exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                        spEuro = euroToInrCost == 0 ? 0 : (spInr / euroToInrCost);
                        spEuro = Truncate(spEuro, 3);
                        spUsd = spEuro * euroToUsd;
                        spUsd = Truncate(spUsd, 3);
                        exWorksEuro += Truncate(mod.Quantity * spEuro, 3);
                        exWorksUsd += Truncate(mod.Quantity * spUsd, 3);
                    }
                    List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
                    var last = tenderView.MasterList.Last();
                    foreach (var mod in tenderView.MasterList)
                    {
                        if (mod.Type != 1)
                        {
                            indiaCost = mod.UnitCost;

                            boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                                tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                            costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                            spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                            spEuro = euroToInrCost == 0 ? 0 : Truncate(spInr / euroToInrCost, 3);

                            spUsd = spEuro * euroToUsd;

                            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));

                            foreach (var currency in otherCurrencies)
                            {
                                euroToOtherCurrency = (euroToInrCost / currency.Value) + (currencyConvRate / 100);
                                var value = spEuro * euroToOtherCurrency;
                                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                                var qtyValue = Truncate(value, 3) * mod.Quantity;
                                otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), qtyValue));
                            }
                        }
                        else
                        {
                            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");

                            foreach (var currency in otherCurrencies)
                            {
                                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                            }
                        }
                        var lastPortId = 0;
                        foreach (var col in mainColumns.Where(x => x.Exclude))
                        {
                            if (mod.Type != 1)
                            {
                                var seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                                cifPort = exWorksSumInr == 0 ? 0 : Truncate(spEuro * (1 + (seaFreight / 100)), 1);

                                if (col.SubColumns.Contains("USD"))
                                {
                                    cifPortUsd = cifPort * (euroToUsd);
                                    col.Rows.Add(Truncate(cifPortUsd, 1));
                                    col.Summation = col.Summation + Truncate(cifPortUsd, 1) * mod.Quantity;
                                }
                                else if (col.SubColumns.Contains("EURO"))
                                {
                                    col.Rows.Add(cifPort);
                                    col.Summation = col.Summation + Truncate(cifPort, 1) * mod.Quantity;
                                    lastPortId = col.UniqueId;
                                }
                                else
                                {
                                    otherCurrencyValue = otherCurrencies.Where(x => x.Name.ToUpper() == col.SubColumns.First()).FirstOrDefault().Value;
                                    // otherCurrencyValue += currencyConvRate / 100;
                                    euroToOtherCurrency = (euroToInrCost / otherCurrencyValue) + (currencyConvRate / 100);
                                    cifPortOtherCurrency = Truncate(cifPort * (euroToOtherCurrency), 1);
                                    col.Rows.Add(Truncate(cifPortOtherCurrency, 3));
                                    col.Summation = col.Summation + Truncate(cifPortOtherCurrency, 1) * mod.Quantity;
                                }

                            }
                            else
                                col.Rows.Add("");

                            if (mod.Equals(last))
                            {
                                col.Rows.Add(col.Summation);
                            }
                        }
                    };

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksEuro, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksUsd, 3));
                    foreach (var currency in otherCurrencies)
                    {
                        var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
                    }

                    tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count].Merge = true;
                    cell = tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Revision " + tenderView.TenderRevisionNo;
                    tempRowCount++;

                    mainColCount = 1;
                    mainRowCount = tempRowCount;
                    int prevRowCount = mainRowCount;
                    foreach (var cols in mainColumns)
                    {
                        tenderPricing.Column(mainColCount).Width = cols.Width;

                        if (cols.Exclude == true)
                        {
                            bool isMerged = tenderPricing.Cells[mainRowCount, mainColCount].Merge;
                            if (isMerged)
                            {
                                cell = tenderPricing.Cells[mainRowCount, mainColCount];
                                cell = GiveCellStyleHeaderProperties(cell);
                                cell.Value = cols.Name;
                                mainRowCount++;
                            }
                            else
                            {
                                tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1 + otherCurrencies.Count].Merge = true;
                                cell = tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1];
                                cell = GiveCellStyleHeaderProperties(cell);
                                cell.Value = cols.Name;
                                mainRowCount++;
                            }
                        }
                        else
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = cols.Name;
                            mainRowCount++;
                        }

                        if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellSubHeaderProperties(cell);
                            cell.Value = cols.SubColumns[0];
                        }

                        mainRowCount++;

                        foreach (var row in cols.Rows)
                        {
                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row;
                            mainRowCount++;
                        }

                        //ExcelRange cell1 = tenderPricing.Cells[mainRowCount, mainColCount]; ;

                        mainColCount++;
                        mainRowCount = prevRowCount;
                    }
                    tempRowCount += tenderView.MasterList.Count + 2;
                    tempColCount = mainColumns.Count + 2;

                    whileCount = 3;
                    while (whileCount < tenderPricing.Dimension.End.Row)
                    {
                        tenderPricing.Row(whileCount).Height = 18;
                        whileCount++;
                    }

                    cell = tenderPricing.Cells[tempRowCount, 1, tempRowCount, mainColumns.Count];
                    cell = GiveLastRowHighlightProperties(cell);

                    //Create table for currency master
                    //tempRowCount = 1;
                    rowCount = prevRowCount - 1;
                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Packing Material Weight as Percentage";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = pckPercentage;
                    rowCount += 2;

                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Cross Currency Margin";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    rowCount += 2;
                    //Create table for currency master
                    tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3].Merge = true;
                    cell = tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Currency";
                    rowCount++;

                    cell = tenderPricing.Cells[rowCount, tempColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "International Currency";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Convert Into INR";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Actual Value";

                    cell = tenderPricing.Cells[rowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Considered Value";
                    rowCount++;

                    decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate = 0;

                    conversionRate = markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                        markupDataset.Where(x => x.TndRevNo == tenderView.TenderRevisionNo).FirstOrDefault().TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

                    tempEuroValue = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
                    tempUsdValue = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

                    count = 0;
                    foreach (var currency in tenderView.CurrencyList)
                    {
                        if (count == 2)
                        {
                            tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3].Merge = true;
                            cell = tenderPricing.Cells[rowCount, tempColCount, rowCount, tempColCount + 3];
                            cell = GiveCellStyleHeaderProperties(cell);
                            cell.Value = "Euro To Other Currencies";
                            rowCount++;
                        }

                        cell = tenderPricing.Cells[rowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = currency.Name;

                        cell = tenderPricing.Cells[rowCount, tempColCount + 1];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(currency.Value, 3);

                        if (currency.Name.ToLower() == "euro")
                        {
                            tempCurrencyValue = (tempEuroValue / tempUsdValue);

                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else if (currency.Name.ToLower() == "usd")
                        {
                            tempCurrencyValue = (tempUsdValue / tempEuroValue);

                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }
                        else
                        {
                            tempCurrencyValue = (tempEuroValue / currency.Value);
                            cell = tenderPricing.Cells[rowCount, tempColCount + 2];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = Truncate(tempCurrencyValue, 3);
                        }

                        tempCurrencyValue += conversionRate / 100;

                        cell = tenderPricing.Cells[rowCount, tempColCount + 3];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = Truncate(tempCurrencyValue, 3);

                        rowCount++;
                        count++;
                    }

                    if (rowCount > tempRowCount)
                    {
                        tempRowCount = rowCount + 1;
                    }
                    else
                        tempRowCount += 1;

                    tenderPricing.Column(1).Width = 30;
                    tenderPricing.Column(tempColCount).Width = 45;
                    tenderPricing.Column(tempColCount + 1).Width = 20;
                    tenderPricing.Column(tempColCount + 2).Width = 20;
                    tenderPricing.Column(tempColCount + 3).Width = 20;

                    #region Internal Calculations
                    calculations.Cells[calcRowCount, 1, calcRowCount, 3].Merge = true;
                    cell = calculations.Cells[calcRowCount, 1, calcRowCount, 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Revision " + tenderView.TenderRevisionNo;
                    calcRowCount++;

                    cell = calculations.Cells[calcRowCount, 1];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Containers";

                    cell = calculations.Cells[calcRowCount, 2];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "40Ft";

                    cell = calculations.Cells[calcRowCount, 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "20Ft";

                    calcRowCount++;
                    tempColCount = 1;
                    foreach (var row in rows)
                    {
                        tempColCount = 1;

                        cell = calculations.Cells[calcRowCount, tempColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Description;
                        tempColCount++;
                        if (row.Description.ToUpper() != "FACTOR FOR DISTRIBUTING FREIGHT" && row.Description.ToUpper() != "TOTAL UNIT WEIGHT (MT)")
                        {
                            cell = calculations.Cells[calcRowCount, tempColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.value40FtContr;
                            tempColCount++;

                            cell = calculations.Cells[calcRowCount, tempColCount];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.value20FtContr;
                        }
                        else
                        {
                            calculations.Cells[calcRowCount, tempColCount, calcRowCount, tempColCount + 1].Merge = true;
                            cell = calculations.Cells[calcRowCount, tempColCount, calcRowCount, tempColCount + 1];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = row.Value;
                        }
                        calcRowCount++;
                    }
                    calculations.Column(1).Width = 35;
                    calculations.Column(2).Width = 20;
                    calculations.Column(3).Width = 20;

                    calcRowCount++;

                    whileCount = 1;
                    while (whileCount < tempRowCount)
                    {
                        calculations.Row(whileCount).Height = 20;
                        whileCount++;
                    }
                    #endregion
                }


                #endregion

                return excelPackage.GetAsByteArray();
            }
        }

        #endregion

        #region IntDiffTenderCompare

        public byte[] IntDiffTenderComparison(int firstTndId, int firstTndRevNo, int otherTndId, int otherTndRevNo)
        {
            int sheetCount = 1;
            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            TenderListDAL listDAL = new TenderListDAL();

            List<TndCompareModel> tndList = new List<TndCompareModel>();
            tndList.Add(new TndCompareModel { TenderId = firstTndId, TenderRevNo = firstTndRevNo });
            tndList.Add(new TndCompareModel { TenderId = otherTndId, TenderRevNo = otherTndRevNo });

            List<TenderDetailsModel> tenderDetailsList = new List<TenderDetailsModel>();

            TenderListModel listModel = new TenderListModel();
            listModel.TndCompareList = listDAL.GetTndCompareList(2);
            foreach (var tender in tndList)
            {
                var bomId = listModel.TndCompareList.Where(x => x.TenderId == tender.TenderId && x.TenderRevNo == tender.TenderRevNo).FirstOrDefault().BomId;
                var bomRevId = listModel.TndCompareList.Where(x => x.TenderId == tender.TenderId && x.TenderRevNo == tender.TenderRevNo).FirstOrDefault().BomRevisionNo;
                var detailsModel = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tender.TenderId, tender.TenderRevNo);
                tenderDetailsList.Add(detailsModel);
            }

            using (var excelPackage = new ExcelPackage())
            {
                excelPackage.Workbook.Properties.Author = "Karamtara";
                excelPackage.Workbook.Properties.Title = "Karamtara";
                ExcelPackage excel;
                ExcelWorksheet sheet;
                List<ExcelPackage> excelPackages = new List<ExcelPackage>();
                foreach (var details in tenderDetailsList)
                {
                    if (details.TenderType == 1)
                    {
                        excel = IntTndPricingK(details.BomId, details.RevisionNo, details.TenderId, details.TenderRevisionNo);
                    }
                    else
                    {
                        excel = IntTndPricingI(details.BomId, details.RevisionNo, details.TenderId, details.TenderRevisionNo);
                    }
                    excelPackages.Add(excel);
                }

                #region RM

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "RawMaterialPricing").FirstOrDefault());
                    Worksheet.Name = "RawMaterialPricing " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region testData

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Test Pricing " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Test Master Pricing").FirstOrDefault());
                    Worksheet.Name = "Test Pricing " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region bgData

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Bank Guarantee").FirstOrDefault());
                    Worksheet.Name = "Bank Guarantee " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region markupData

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Markup " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Markup").FirstOrDefault());
                    Worksheet.Name = "Markup " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region freightData

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Freight " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Freight").FirstOrDefault());
                    Worksheet.Name = "Freight " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region tender pricing view

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    var Worksheet = excelPackage.Workbook.Worksheets.Add("Tender Pricing " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Tender Pricing").FirstOrDefault());
                    Worksheet.Name = "Tender Pricing " + sheetCount;

                    sheetCount++;
                }

                #endregion

                #region Internal Calculations

                sheetCount = 1;
                foreach (var excelPck in excelPackages)
                {
                    if (excelPck.Workbook.Worksheets.Where(x => x.Name == "Container Details").Any())
                    {
                        var Worksheet = excelPackage.Workbook.Worksheets.Add("Container Details " + sheetCount, excelPck.Workbook.Worksheets.Where(x => x.Name == "Container Details").FirstOrDefault());
                        Worksheet.Name = "Container Details " + sheetCount;
                    }
                    sheetCount++;
                }

                #endregion


                return excelPackage.GetAsByteArray();
            }
        }

        public ExcelPackage IntTndPricingK(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            decimal pckPercentage = 0;
            #region rawmData
            rmpDAL = new RawMaterialPricingDAL();
            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 8, 12,"SrNo"),
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
                new ColumnModel("Price", 10, 12,"Price")
            };
            #endregion

            #region testData
            testDAL = new TestDAL();
            var testMaster = testDAL.GetIntTestPricingList(tenderId, tenderRevId);
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 25, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                new ColumnModel("Quantity", 20, 12,"Quantity"),
                new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
            };

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
            var bgColumns = new List<ColumnModel>() {

                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
            };

            var contractValue = Truncate(bgList.FirstOrDefault().ContractValue, 3);
            var deliveryMonth = Truncate(bgList.FirstOrDefault().DeliveryMonth, 3);
            var performancePeriod = Truncate(bgList.FirstOrDefault().PerformancePeriod, 3);
            var gracePeriod = Truncate(bgList.FirstOrDefault().GracePeriod, 3);

            #endregion

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            //var markupData
            //var markupData = new List<MarkupDataModel>();
            var markupColumns = new List<ColumnModel>()
            {
                new ColumnModel("Markup", 50, 0, "Markup"),
                new ColumnModel("Italy", 30, 0, "Italy"),
                new ColumnModel("India", 30, 0, "India"),
                new ColumnModel("BO", 30, 0, "BO"),
            };

            #endregion

            #region freightData

            var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, tenderRevId);

            var freightRows = new List<RowModel>()
            {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                 new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
            };

            var commonCols = new List<ColumnModel>()
            {
                new ColumnModel("Data", 20, 12, ""),
                new ColumnModel("Total Cost", 20, 12, ""),
                new ColumnModel("Remarks", 30, 12, ""),
            };

            List<TableModel> freightData = new List<TableModel>();
            int total20FtContainers = 0;
            int total40FtContainers = 0;
            decimal finalOverallTotal = 0;

            foreach (var val in freightMasterData.TenderPortNames)
            {
                TableModel mod = new TableModel();
                List<RowModel> rowList = new List<RowModel>();
                mod.Rows = new List<RowModel>();
                mod.Id = val.Id;
                mod.Title = val.PortName;
                rowList = new List<RowModel>()
                {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                 };

                foreach (var data in rowList)
                {
                    var temp = freightMasterData.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                    if (temp != null)
                    {
                        if (data.KeyName == "NoOfFortyFtContainers")
                            total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                        if (data.KeyName == "OverallTotal")
                            finalOverallTotal += temp.Cost;
                        data.Value = temp.Cost;
                        data.Data = temp.Data;
                        data.Remark = temp.Remarks;
                    }
                }
                var no20FtContainers = 0;
                var tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                if (tempData != null && tempData.Data > 0)
                    no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                total20FtContainers += no20FtContainers;

                pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                mod.Rows.AddRange(new List<RowModel>()
                {
                    new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                });

                mod.Rows.AddRange(rowList);

                freightData.Add(mod);
            }

            var freightTotal = new List<RowModel>()
            {
                new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
            };
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            var otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(Exworks)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(Exworks)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { x.CurrencyName },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, spUsd = 0, usdToInr = 0, euroToInr = 0, cifPort = 0, spUsdSummation = 0, spEuro = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksSumUsd = 0, exWorksSumEuro = 0, loadingFactor = 0, mt = 0; ;

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
            euroToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            col.Rows.Add(string.Format("{0} MT", mt));
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)(Truncate(x.UnitCost, 3) * x.Quantity) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;
                    indiaCostSummation += Truncate(indiaCost, 3) * mod.Quantity;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                    boSummation += Truncate(mod.Quantity * boCost, 3);

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);
                    costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);

                    spInr = (costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
                    exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                    spUsd = usdToInr == 0 ? 0 : (spInr / usdToInr);
                    spEuro = usdToInr == 0 ? 0 : (spInr / euroToInr);
                    exWorksSumUsd += Truncate(mod.Quantity * spUsd, 3);
                    exWorksSumEuro += Truncate(mod.Quantity * spEuro, 3);
                }
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spUsd = usdToInr == 0 ? 0 : Truncate((spInr / usdToInr), 3);

                    spEuro = euroToInr == 0 ? 0 : Truncate((spInr / euroToInr), 3);

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    spUsdSummation += (spInr * mod.Quantity);

                    foreach (var currency in otherCurrencies)
                    {
                        var value = spInr / currency.Value;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = value * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), Truncate(qtyValue, 3)));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }

                var last = tenderView.MasterList.Last();
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        decimal seaFreight = 0, sellingPrice = 0;

                        if (exWorksSumInr != 0)
                            seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        else
                            seaFreight = 0;

                        if (col.ExtraValue != 0)
                            sellingPrice = Truncate(Math.Ceiling((Truncate(spInr, 3) / col.ExtraValue)), 1);
                        else
                            sellingPrice = 0;

                        //if (col.ExtraKey.ToLower() == "euro")
                        //    cifPort = Truncate(((usdToInr / euroToInrCost) * spUsd) * (1 + seaFreight / 100), 1);
                        //else
                        //    cifPort = Truncate(spUsd * (1 + (seaFreight / 100)), 1);

                        cifPort = Truncate((sellingPrice * (1 + (seaFreight / 100))), 1);

                        col.Rows.Add(Truncate(cifPort, 3));
                        col.Summation += Truncate(cifPort, 3) * mod.Quantity;
                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumUsd, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumEuro, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }


            #endregion

            var excelPackage = new ExcelPackage();
            excelPackage.Workbook.Properties.Author = "Karamtara";
            excelPackage.Workbook.Properties.Title = "Karamtara";

            #region Raw material pricing section

            var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
            rawMatPricing.Name = "RawMaterialPricing";

            ExcelRange range = rawMatPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            for (int i = 1; i <= rawMatColumns.Count; i++)
            {
                rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
                rawMatPricing.Row(1).Height = 25;
                cell = rawMatPricing.Cells[9, i];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = rawMatColumns[i - 1].Name;
                cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
            }

            for (int i = 1; i <= rawMatMaster.Count; i++)
            {
                for (int j = 1; j <= rawMatColumns.Count; j++)
                {
                    cell = rawMatPricing.Cells[i + 9, j];
                    cell = GiveCellStyleProperties(cell);
                    rawMatPricing.Row(i + 1).Height = 20;
                    if (rawMatColumns[j - 1].UseValue)
                    {
                        cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
                    }
                    else
                    {
                        cell.Value = string.Empty;
                    }
                }
            }
            #endregion

            #region test master pricing

            var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
            range = testPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);
            int testCol = 1;
            int testRow = 9;
            for (int i = 1; i <= testColumns.Count; i++)
            {
                if (testColumns[i - 1].SubColumns == null)
                {

                    testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                    testPricing.Row(testRow).Height = 25;

                    testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                    cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                    cell.Value = testColumns[i - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);

                    if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                    {
                        decimal value = 0;

                        if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                            value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                        cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                    }
                }
                else
                {
                    testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                    testPricing.Row(testRow).Height = 25;
                    testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                    cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                    cell.Value = testColumns[i - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);

                    colCount = i;

                    foreach (var col in testColumns[i - 1].SubColumns)
                    {
                        testPricing.Column(testCol).Width = 20;
                        testPricing.Row(testRow).Height = 25;
                        cell = testPricing.Cells[testRow + 1, testCol];
                        cell.Value = col.ToUpper();
                        cell = GiveCellStyleHeaderProperties(cell);
                        testCol++;
                    }

                    testCol--;

                }

                testCol++;

            }
            testRow += 2;
            testCol = 1;
            for (int i = 1; i <= testMaster.TestList.Count; i++)
            {
                testCol = 1;
                for (int j = 1; j <= testColumns.Count; j++)
                {
                    if (testColumns[j - 1].SubColumns == null)
                    {
                        cell = testPricing.Cells[testRow, testCol];
                        cell = GiveCellStyleProperties(cell);
                        if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                        {
                            if (testColumns[j - 1].UseValue)
                            {
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                            }
                            else
                            {
                                cell.Value = i;
                            }
                        }
                        else
                        {
                            decimal value = 0;

                            if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                            cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                        }
                    }
                    else
                    {
                        foreach (var column in testColumns[j - 1].SubColumns)
                        {
                            cell = testPricing.Cells[testRow, testCol];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                            testCol++;
                        }

                        testCol--;
                    }
                    testCol++;
                }
                testRow++;
            }
            #endregion

            #region bank guarantee

            var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
            bankGuaranteeSheet.Name = "Bank Guarantee";

            range = bankGuaranteeSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int bgRowCount = 9;
            int bgColCount = 1;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Contract Value : " + contractValue;
            cell = GiveCellStyleHeaderProperties(cell);
            bgColCount += 3;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Delivery Month : " + deliveryMonth;
            cell = GiveCellStyleHeaderProperties(cell);
            bgRowCount++;
            bgColCount = 1;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
            cell.Value = "Grace Period : " + gracePeriod;
            cell = GiveCellStyleHeaderProperties(cell);
            bgColCount += 3;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Performance Period : " + performancePeriod;
            cell = GiveCellStyleHeaderProperties(cell);

            bankGuaranteeSheet.Row(1).Height = 25;
            bankGuaranteeSheet.Row(2).Height = 25;
            bankGuaranteeSheet.Column(1).Width = 90;
            bankGuaranteeSheet.Column(3).Width = 90;

            for (int i = 1; i <= bgColumns.Count; i++)
            {
                bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                bankGuaranteeSheet.Row(3).Height = 25;
                bankGuaranteeSheet.Row(3).Height = 25;
                cell = bankGuaranteeSheet.Cells[11, i];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = bgColumns[i - 1].Name;
            }

            for (int i = 1; i <= bgList.Count; i++)
            {
                for (int j = 1; j <= bgColumns.Count; j++)
                {
                    cell = bankGuaranteeSheet.Cells[i + 11, j];
                    cell = GiveCellStyleProperties(cell);

                    if (bgColumns[j - 1].UseValue)
                    {
                        cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                    }
                    else
                    {
                        cell.Value = "";
                    }
                }
            }
            bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
            cell = bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count];
            cell = GiveCellStyleProperties(cell);

            whileCount = 1;
            while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
            {
                bankGuaranteeSheet.Row(whileCount).Height = 20;
                whileCount++;
            }

            #endregion

            #region markup

            var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
            markupSheet.Name = "Markup";

            range = markupSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int markupRowCount = 9;
            int markupColCount = 1;

            foreach (var col in markupColumns)
            {
                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = col.Name;
                markupSheet.Column(markupColCount).Width = col.Width;
                markupColCount++;
            }

            markupRowCount = 10;
            markupColCount = 1;
            foreach (var mark in markupDataset.MarkupDetails)
            {
                bool isPBG = false;
                isPBG = mark.MarkupId == 16 ? true : false;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = mark.Markup;
                markupColCount++;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = isPBG ? (markupDataset.TndType == 2 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                    : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = isPBG ? (markupDataset.TndType == 1 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                    : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;


                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;


                switch (mark.MarkupId)
                {
                    case 10: //financing
                        {

                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 17: //financing sales cr
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 16: //pbg
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 18: //interest savings on advance
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }

                            string value2 = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value2;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }

                            break;
                        }
                }

                markupColCount = 1;
                markupRowCount++;
            }
            markupSheet.Column(5).Width = 20;
            markupSheet.Column(6).Width = 20;

            int markupWhile = 1;
            while (markupWhile < markupRowCount)
            {
                markupSheet.Row(markupWhile).Height = 20;
                markupWhile++;
            }

            markupWhile++;

            markupRowCount = 10;
            //Create table for travel, lodging and boarding
            markupSheet.Cells[markupRowCount, 8, markupRowCount, 9].Merge = true;
            cell = markupSheet.Cells[markupRowCount, 8, markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Travel, Lodging and Boarding";
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "No. Of Persons";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "No. Of Days";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "Fare";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "Lodging";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            //PackingPercentage
            pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            int tempRowCount = markupRowCount + 2, tempColCount = 8;

            pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
            cell = markupSheet.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Packing Material Weight as Percentage";

            cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = pckPercentage;

            cell = markupSheet.Cells[10, 8];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Cross Currency Margin";

            cell = markupSheet.Cells[10, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            //Create table for currency master
            markupSheet.Cells[12, 8, 12, 11].Merge = true;
            cell = markupSheet.Cells[12, 8, 12, 11];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Currency";

            cell = markupSheet.Cells[13, 8];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "International Currency";

            cell = markupSheet.Cells[13, 9];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Convert Into INR";

            cell = markupSheet.Cells[13, 10];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Actual Value";

            cell = markupSheet.Cells[13, 11];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Considered Value";

            tempRowCount = 14;
            tempColCount = 8;

            decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

            conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
            tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

            int count = 0;
            foreach (var currency in currencyData.List)
            {
                if (count == 2)
                {
                    markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                    cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Euro To Other Currencies";
                    tempRowCount++;
                }

                cell = markupSheet.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = currency.Name;

                cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = Truncate(currency.Value, 3);

                if (currency.Name.ToLower() == "euro")
                {
                    tempCurrencyValue = (tempEuroValue / tempUsdValue);

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else if (currency.Name.ToLower() == "usd")
                {
                    tempCurrencyValue = (tempUsdValue / tempEuroValue);

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else
                {
                    tempCurrencyValue = (tempEuroValue / currency.Value);
                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }

                tempCurrencyValue += conversionRate / 100;

                cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = Truncate(tempCurrencyValue, 3);

                tempRowCount++;
                count++;
            }


            markupSheet.Column(8).Width = 40;
            markupSheet.Column(9).Width = 20;
            markupSheet.Column(10).Width = 20;
            markupSheet.Column(11).Width = 20;

            //var markupLastCol = markupSheet.Dimension.End.Column + 1;
            //cell = markupSheet.Cells[1, markupLastCol];

            #endregion

            #region freight

            var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
            freightSheet.Name = "Freight";
            freightSheet.View.FreezePanes(1, 2);

            range = freightSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int freightRowCount = 9;
            int freightColCount = 1;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            freightSheet.Column(freightColCount).Width = 50;
            cell.Value = "Freight";
            cell = GiveCellStyleHeaderProperties(cell);
            freightRowCount += 1;

            var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
            var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

            foreach (var col in secondayCols)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell.Value = col;
                if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                    cell = GiveCellStyleHeaderProperties(cell);
                else
                    cell = GiveCellStyleProperties(cell);
                freightRowCount++;
            }

            cell = freightSheet.Cells[freightRowCount, 1];
            cell = GiveCellSubHeaderProperties(cell);
            freightColCount = 1;
            freightRowCount++;
            foreach (var col in actualCols)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell.Value = col;
                if (col == "Containers" || col == "Air Freight Cost")
                    cell = GiveCellStyleHeaderProperties(cell);
                else
                    cell = GiveCellStyleProperties(cell);
                freightRowCount++;
            }

            freightColCount = 2;

            foreach (var mod in freightData)
            {
                freightRowCount = 9;

                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = mod.Title;
                freightRowCount++;
                freightColCount += 2;
                foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                {
                    freightColCount -= 2;

                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rowCol.Value;

                    freightColCount += 2;
                    freightRowCount++;
                }

                foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                {
                    freightColCount -= 2;

                    if (rowCol.IsHeading)
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = rowCol.SubColumns[0].Name;
                        freightColCount += 2;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = rowCol.SubColumns[1].Name;
                    }
                    else
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Value;
                        freightColCount += 2;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Data;
                    }
                    freightRowCount++;

                }



                //freightColCount -= 2;
                cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Data";
                freightSheet.Column(freightColCount - 2).Width = 15;

                cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Total Cost";
                freightSheet.Column(freightColCount - 1).Width = 15;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Remarks";
                freightSheet.Column(freightColCount).Width = 30;

                freightRowCount++;
                foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                {
                    freightColCount -= 2;
                    if (row.IsHeading)
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightColCount += 2;
                    }
                    else
                    {
                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Data;
                        freightColCount++;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Value;
                        freightColCount++;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Remark;

                    }
                    freightRowCount++;
                }


                freightColCount += 2;
            }

            freightRowCount = 9;
            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Freight Total";
            freightRowCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Sr. No.";
            freightColCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Description";
            freightColCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Value";
            freightRowCount++;
            freightColCount -= 2;
            count = 1;
            foreach (var row in freightTotal)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = count;
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = row.Description;
                freightSheet.Column(freightColCount).Width = 30;
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = row.Value;
                freightSheet.Column(freightColCount).Width = 20;
                freightColCount -= 2;
                freightRowCount++;
                count++;
            }

            int freightWhileCount = 1;
            while (freightWhileCount < freightRowCount)
            {
                freightSheet.Row(freightWhileCount).Height = 20;
                freightWhileCount++;
            }

            #endregion

            #region mainView

            var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
            tenderPricing.Name = "Tender Pricing";

            range = tenderPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int mainRowCount = 9;
            int mainColCount = 1;
            foreach (var cols in mainColumns)
            {
                tenderPricing.Column(mainColCount).Width = cols.Width;

                cell = tenderPricing.Cells[mainRowCount, mainColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = cols.Name;
                mainRowCount++;

                if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                {
                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = cols.SubColumns[0];
                }

                mainRowCount++;

                foreach (var row in cols.Rows)
                {
                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row;
                    mainRowCount++;
                }
                mainColCount++;
                mainRowCount = 9;
            }

            tempRowCount = mainRowCount;
            tempColCount = tenderPricing.Dimension.End.Column + 2;

            whileCount = 3;
            while (whileCount < tenderPricing.Dimension.End.Row)
            {
                tenderPricing.Row(whileCount).Height = 18;
                whileCount++;
            }

            cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
            cell = GiveLastRowHighlightProperties(cell);

            //Create table for currency master
            tempRowCount = 1;
            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Packing Material Weight as Percentage";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell);
            cell.Value = pckPercentage;
            tempRowCount += 2;

            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Cross Currency Margin";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempRowCount += 2;
            //Create table for currency master
            tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
            cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Currency";
            tempRowCount++;

            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "International Currency";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Convert Into INR";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Actual Value";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Considered Value";
            tempRowCount++;

            tempEuroValue = 0;
            tempUsdValue = 0; tempCurrencyValue = 0;
            conversionRate = 0;

            conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
            tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

            count = 0;
            foreach (var currency in currencyData.List)
            {
                if (count == 2)
                {
                    tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                    cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Euro To Other Currencies";
                    tempRowCount++;
                }

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = currency.Name;

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Truncate(currency.Value, 3);

                if (currency.Name.ToLower() == "euro")
                {
                    tempCurrencyValue = (tempEuroValue / tempUsdValue);

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else if (currency.Name.ToLower() == "usd")
                {
                    tempCurrencyValue = (tempUsdValue / tempEuroValue);

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else
                {
                    tempCurrencyValue = (tempEuroValue / currency.Value);
                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }

                tempCurrencyValue += conversionRate / 100;

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Truncate(tempCurrencyValue, 3);

                tempRowCount++;
                count++;
            }

            tenderPricing.Column(1).Width = 30;
            tenderPricing.Column(tempColCount).Width = 45;
            tenderPricing.Column(tempColCount + 1).Width = 20;
            tenderPricing.Column(tempColCount + 2).Width = 20;
            tenderPricing.Column(tempColCount + 3).Width = 20;

            #endregion

            return excelPackage;
        }

        public ExcelPackage IntTndPricingI(int bomId, int bomRevId, int tenderId, int tenderRevId)
        {
            int count = 0;
            decimal otherCurrencyValue = 0, mt = 0, loadingFactor = 0, dividingFactor40Ft, dividingFactor20Ft, considered40FtCntr, considered20FtCntr, dollarsPer40Ft, dollarsPer20Ft,
                rsPer40FtContainer, rsPer20FtContainer, rsPer40FtContainers, rsPer20FtContainers, totalRsPer40FtContainers, totalRsPer20FtContainers, usdToInr = 0, sizeWise20FtContr,
                sizeWise40FtContr, totalRsForCon, pckPercentage;

            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, cifPort = 0, cifPortUsd = 0, euroToOtherCurrency, cifPortOtherCurrency;

            decimal euroToUsd = 0, spEuro = 0, spUsd = 0, noOfCon = 0, rsPerCon = 0, distFact = 0, containerCharges = 0, currencyConvRate = 0;

            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksEuro = 0, exWorksUsd = 0;

            List<CurrencyModel> otherCurrencies = null;

            #region rawmData
            rmpDAL = new RawMaterialPricingDAL();
            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

            var rawMatColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 25, 12,"SrNo"),
                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
                new ColumnModel("Group", 20, 12,"MaterialGroup"),
                new ColumnModel("Price", 10, 12,"Price")
            };
            #endregion

            #region testData
            testDAL = new TestDAL();
            var testMaster = testDAL.GetIntTestPricingList(tenderId, tenderRevId);
            var testColumns = new List<ColumnModel>() {
                new ColumnModel("Sr.No", 25, 12,"Id", false),
                new ColumnModel("Name", 30, 12,"TestName"),
                new ColumnModel("Description", 50, 12,"TestDescription"),
                new ColumnModel("Group Type", 30, 12,"Type"),
                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
                new ColumnModel("Line Type", 30, 12,"KVLine"),
                new ColumnModel("UTS", 30, 12,"UTS"),
                new ColumnModel("Summary", 50, 12,"Summary"),
                new ColumnModel("Price", 50, 12,"Price") { SubColumns = new List<string>() { "Inr","Euro","Usd"}, Rows = new List<dynamic>() },
                new ColumnModel("Quantity", 20, 12,"Quantity"),
                new ColumnModel("Incremented Price", 30, 12,"IncrementedPrice")
            };

            #endregion

            #region bgData
            tenderDetailsDAL = new TenderDetailsDAL();
            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
            var bgColumns = new List<ColumnModel>() {

                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
            };

            var contractValue = Truncate(bgList.Any() ? bgList.FirstOrDefault().ContractValue : 0, 3);
            var deliveryMonth = Truncate(bgList.Any() ? bgList.FirstOrDefault().DeliveryMonth : 0, 3);
            var performancePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().PerformancePeriod : 0, 3);
            var gracePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().GracePeriod : 0, 3);

            #endregion

            #region markupData
            intDetailsDAL = new IntTenderDetailsDAL();
            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
            //var markupData
            //var markupData = new List<MarkupDataModel>();
            var markupColumns = new List<ColumnModel>()
            {
                new ColumnModel("Markup", 50, 0, "Markup"),
                new ColumnModel("India", 30, 0, "India"),
                new ColumnModel("Italy", 30, 0, "Italy"),
                new ColumnModel("BO", 30, 0, "BO"),
            };

            #endregion

            #region freightData

            var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, tenderRevId);

            var freightRows = new List<RowModel>()
            {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                 new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
            };

            var commonCols = new List<ColumnModel>()
            {
                new ColumnModel("Data", 20, 12, ""),
                new ColumnModel("Total Cost", 20, 12, ""),
                new ColumnModel("Remarks", 30, 12, ""),
            };

            List<TableModel> freightData = new List<TableModel>();
            int total20FtContainers = 0;
            int total40FtContainers = 0;
            decimal finalOverallTotal = 0;

            foreach (var val in freightMasterData.TenderPortNames)
            {
                TableModel mod = new TableModel();
                List<RowModel> rowList = new List<RowModel>();
                mod.Rows = new List<RowModel>();
                mod.Id = val.Id;
                mod.Title = val.PortName;
                rowList = new List<RowModel>()
                {
                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
                new RowModel(){ Description = "Containers", IsHeading = true},
                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
                new RowModel(){ Description = "Override No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainersOverridden" },
                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
                new RowModel(){ Description = "No of 40 Ft Containers: @23 MT / Container", KeyName="NoOfFortyFtContainers" },
                new RowModel(){ Description = "sea freight per 40'ft container", KeyName="SeaFrtPerFortyFtContainer" },
                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
                 };

                foreach (var data in rowList)
                {
                    var temp = freightMasterData.TenderPortDetails.Where(y => y.Description == data.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
                    if (temp != null)
                    {
                        if (data.KeyName == "NoOfFortyFtContainers")
                            total40FtContainers += Convert.ToInt32(decimal.Ceiling(temp.Data));
                        if (data.KeyName == "OverallTotal")
                            finalOverallTotal += temp.Cost;
                        data.Value = temp.Cost;
                        data.Data = temp.Data;
                        data.Remark = temp.Remarks;
                    }
                }
                var no20FtContainers = 0;
                var tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainers" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                if (tempData != null)
                    no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));
                else
                    no20FtContainers = 0;

                tempData = freightMasterData.TenderPortDetails.Where(y => y.Description == "NoOfFtContainersOverridden" && y.PortId == mod.Id).Select(y => y).FirstOrDefault();

                if (tempData  != null && tempData.Data > 0)
                    no20FtContainers = Convert.ToInt32(Decimal.Ceiling(tempData.Data));

                total20FtContainers += no20FtContainers;

                pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

                mod.Rows.AddRange(new List<RowModel>()
                {
                    new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel() { Description = "Sea Freight 40FT Container", KeyName = "SeaFreightFortyFT", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreightFortyFT, val.CurrencyName), Data=val.SeaFreightFortyFT, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
                    new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * pckPercentage), Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel(){ Description = "Estimated Charges 40 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")} },
                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreightFortyFT * pckPercentage), Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreightFortyFT * val.CurrencyValue * pckPercentage), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
                    new ColumnModel("Rupees", 20,12,"")}},
                });

                mod.Rows.AddRange(rowList);

                freightData.Add(mod);
            }

            var freightTotal = new List<RowModel>()
            {
                new RowModel(){Description="NO OF 20FT CONTAINERS",Value=total20FtContainers},
                new RowModel(){Description="NO OF 40FT CONTAINERS",Value=total40FtContainers},
                new RowModel(){Description="OVERALL TOTAL",Value=Truncate(finalOverallTotal,3)}
            };
            #endregion

            #region currency

            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);
            otherCurrencies = currencyData.List.Where(x => x.Name.ToLower() != "euro" && x.Name.ToLower() != "usd" && x.DisplayInView == true).ToList();

            #endregion

            #region tender pricing view

            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
            var portList = tenderView.TndPortDetails;
            loadingFactor = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();

            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");

            tenderView.MasterList.ForEach(x =>
            {
                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
            });

            var mainColumns = new List<ColumnModel>()
                {
                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
                new ColumnModel("Selling Price(FOB)", 25, 12, "spusd", false) { SubColumns = new List<string>() { "USD"}, Rows = new List<dynamic>() },
                };

            foreach (var currency in otherCurrencies)
            {
                mainColumns.Add(new ColumnModel("Selling Price(FOB)", 25, 12, "sp" + currency.Name.ToLower(), false) { SubColumns = new List<string>() { currency.Name.ToUpper() }, Rows = new List<dynamic>() });
            }

            var index = 5;
            portList.ForEach(x =>
            {
                foreach (var currency in otherCurrencies)
                {
                    mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                    {
                        SubColumns = new List<string>() { currency.Name.ToUpper() },
                        UniqueId = x.Id,
                        Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                        ExtraKey = x.CurrencyName,
                        ExtraValue = x.CurrencyValue,
                        Rows = new List<dynamic>()
                    }); index++;
                }

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "USD" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;

                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
                {
                    SubColumns = new List<string>() { "EURO" },
                    UniqueId = x.Id,
                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
                    ExtraKey = x.CurrencyName,
                    ExtraValue = x.CurrencyValue,
                    Rows = new List<dynamic>()
                }); index++;
            });

            currencyConvRate = tenderView.IntTndValues.Where(x => x.Description == "ConversionRate").Select(y => y.Value).FirstOrDefault();
            euroToUsd = usdToInr == 0 ? 0 : ((euroToInrCost / usdToInr) + (currencyConvRate / 100));
            containerCharges = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").Select(y => y.Value).FirstOrDefault();

            foreach (var col in mainColumns)
            {
                switch (col.PropName)
                {
                    case "srno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
                            break;
                        }
                    case "desc":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
                            col.Rows.Add("Total");
                            break;
                        }
                    case "unit":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
                            break;
                        }
                    case "drawno":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
                            break;
                        }
                    case "qty":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
                            break;
                        }
                    case "unitwt":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
                            break;
                        }
                    case "indiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                    case "totalindiacost":
                        {
                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
                            break;
                        }
                }
            }

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;
                indiaCostSummation += Truncate(indiaCost, 3);

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
                boSummation += Truncate(mod.Quantity * boCost, 3);

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);
                costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);
            }


            #region Internal Calculation

            dividingFactor40Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor40Ft").FirstOrDefault().Value;
            dividingFactor20Ft = tenderView.IntTndValues.Where(x => x.Description == "DividingFactor20Ft").FirstOrDefault().Value;
            considered40FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered40FtCntr").FirstOrDefault().Value;
            considered20FtCntr = tenderView.IntTndValues.Where(x => x.Description == "Considered20FtCntr").FirstOrDefault().Value;
            dollarsPer40Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").FirstOrDefault().Value;
            dollarsPer20Ft = tenderView.IntTndValues.Where(x => x.Description == "RsPer20FtContainer").FirstOrDefault().Value;

            if (dividingFactor20Ft == 0)
                sizeWise20FtContr = 0;
            else
                sizeWise20FtContr = Truncate(mt / dividingFactor20Ft, 1);

            if (dividingFactor40Ft == 0)
                sizeWise40FtContr = 0;
            else
                sizeWise40FtContr = Truncate((mt / dividingFactor40Ft), 1);

            if (sizeWise20FtContr > 0 && sizeWise20FtContr < 1)
                sizeWise20FtContr = 1;

            if (sizeWise40FtContr >= 0 && sizeWise40FtContr < 1)
                sizeWise40FtContr = 1;


            rsPer40FtContainer = dollarsPer40Ft * usdToInr;
            rsPer20FtContainer = dollarsPer20Ft * usdToInr;

            considered20FtCntr = Math.Max(0, Math.Ceiling((mt - (considered40FtCntr * dividingFactor40Ft)) / dividingFactor20Ft));

            totalRsPer20FtContainers = considered20FtCntr * rsPer20FtContainer;
            totalRsPer40FtContainers = considered40FtCntr * rsPer40FtContainer;

            totalRsForCon = totalRsPer20FtContainers + totalRsPer40FtContainers;

            if (costOfSalesSummation == 0)
            {
                distFact = 0;
            }
            else
            {
                distFact = Truncate(Math.Ceiling((totalRsForCon / costOfSalesSummation) * 100), 1);
            }

            var rows = new List<RowModel>()
            {
                new RowModel(){ Description = "TOTAL UNIT WEIGHT (MT)",Value=mt},
                new RowModel(){ Description = "WEIGHT PER CONTAINERS",value20FtContr=dividingFactor20Ft,value40FtContr=dividingFactor40Ft },
                new RowModel(){ Description = "NO OF CONTAINERS SIZE WISE" ,value20FtContr=sizeWise20FtContr,value40FtContr=sizeWise40FtContr},
                new RowModel(){ Description = "CONTAINERS TOBE CONSIDER",value40FtContr=considered40FtCntr,value20FtContr=considered20FtCntr},
                new RowModel(){ Description = "DOLLARS PER CONTAINER",value40FtContr=dollarsPer40Ft,value20FtContr=dollarsPer20Ft},
                new RowModel(){ Description = "₹ PER CONTAINER" ,value40FtContr=rsPer40FtContainer,value20FtContr=rsPer20FtContainer},
                new RowModel(){ Description = "TOTAL ₹ PER CONTAINERS" ,value40FtContr=totalRsPer40FtContainers,value20FtContr=totalRsPer20FtContainers},
                new RowModel(){ Description = "FACTOR FOR DISTRIBUTING FREIGHT" ,Value=distFact},
            };

            var columns = new List<ColumnModel>()
            {
                new ColumnModel("Containers", 40, 12, "") { Value = mt, Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } },
                new ColumnModel("40 Ft", 40, 12, "") {Rows = new List<dynamic>(){ }, SubColumns = new List<string>() { "" } },
                new ColumnModel("20 Ft", 40, 12, "") {Rows = new List<dynamic>(), SubColumns = new List<string>() { "" } }

            };


            #endregion

            foreach (var mod in tenderView.MasterList)
            {
                indiaCost = mod.UnitCost;

                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);

                spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);
                exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

                spEuro = euroToInrCost == 0 ? 0 : (spInr / euroToInrCost);
                spEuro = Truncate(spEuro, 3);
                spUsd = spEuro * euroToUsd;
                spUsd = Truncate(spUsd, 3);
                exWorksEuro += Truncate(mod.Quantity * spEuro, 3);
                exWorksUsd += Truncate(mod.Quantity * spUsd, 3);
            }
            List<KeyValuePair<string, decimal>> otherCurrencyTotal = new List<KeyValuePair<string, decimal>>();
            var last = tenderView.MasterList.Last();
            foreach (var mod in tenderView.MasterList)
            {
                if (mod.Type != 1)
                {
                    indiaCost = mod.UnitCost;

                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

                    spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

                    spEuro = euroToInrCost == 0 ? 0 : Truncate(spInr / euroToInrCost, 3);

                    spUsd = spEuro * euroToUsd;

                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spUsd, 3));

                    foreach (var currency in otherCurrencies)
                    {
                        euroToOtherCurrency = (euroToInrCost / currency.Value) + (currencyConvRate / 100);
                        var value = spEuro * euroToOtherCurrency;
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(Truncate(value, 3));
                        var qtyValue = Truncate(value, 3) * mod.Quantity;
                        otherCurrencyTotal.Add(new KeyValuePair<string, decimal>(currency.Name.ToLower(), qtyValue));
                    }
                }
                else
                {
                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
                    mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add("");

                    foreach (var currency in otherCurrencies)
                    {
                        mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add("");
                    }
                }
                var lastPortId = 0;
                foreach (var col in mainColumns.Where(x => x.Exclude))
                {
                    if (mod.Type != 1)
                    {
                        var seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
                        cifPort = exWorksSumInr == 0 ? 0 : Truncate(spEuro * (1 + (seaFreight / 100)), 1);

                        if (col.SubColumns.Contains("USD"))
                        {
                            cifPortUsd = cifPort * (euroToUsd);
                            col.Rows.Add(Truncate(cifPortUsd, 1));
                            col.Summation = col.Summation + Truncate(cifPortUsd, 1) * mod.Quantity;
                        }
                        else if (col.SubColumns.Contains("EURO"))
                        {
                            col.Rows.Add(cifPort);
                            col.Summation = col.Summation + Truncate(cifPort, 1) * mod.Quantity;
                            lastPortId = col.UniqueId;
                        }
                        else
                        {
                            otherCurrencyValue = otherCurrencies.Where(x => x.Name.ToUpper() == col.SubColumns.First()).FirstOrDefault().Value;
                            // otherCurrencyValue += currencyConvRate / 100;
                            euroToOtherCurrency = (euroToInrCost / otherCurrencyValue) + (currencyConvRate / 100);
                            cifPortOtherCurrency = Truncate(cifPort * (euroToOtherCurrency), 1);
                            col.Rows.Add(Truncate(cifPortOtherCurrency, 3));
                            col.Summation = col.Summation + Truncate(cifPortOtherCurrency, 1) * mod.Quantity;
                        }

                    }
                    else
                        col.Rows.Add("");

                    if (mod.Equals(last))
                    {
                        col.Rows.Add(col.Summation);
                    }
                }
            };

            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksEuro, 3));
            mainColumns.Where(x => x.PropName == "spusd").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksUsd, 3));
            foreach (var currency in otherCurrencies)
            {
                var tempVal = otherCurrencyTotal.Sum(x => x.Key == currency.Name.ToLower() ? x.Value : 0);
                mainColumns.Where(x => x.PropName == "sp" + currency.Name.ToLower()).Select(x => x).FirstOrDefault().Rows.Add(tempVal);
            }
            #endregion

            var excelPackage = new ExcelPackage();
            excelPackage.Workbook.Properties.Author = "Karamtara";
            excelPackage.Workbook.Properties.Title = "Karamtara";

            #region Raw material pricing section

            var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
            rawMatPricing.Name = "RawMaterialPricing";

            ExcelRange range = rawMatPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            for (int i = 1; i <= rawMatColumns.Count; i++)
            {
                rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
                rawMatPricing.Row(1).Height = 25;
                cell = rawMatPricing.Cells[9, i];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = rawMatColumns[i - 1].Name;
                cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
            }

            for (int i = 1; i <= rawMatMaster.Count; i++)
            {
                for (int j = 1; j <= rawMatColumns.Count; j++)
                {
                    cell = rawMatPricing.Cells[i + 9, j];
                    cell = GiveCellStyleProperties(cell);
                    rawMatPricing.Row(i + 1).Height = 20;
                    if (rawMatColumns[j - 1].UseValue)
                    {
                        cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
                    }
                    else
                    {
                        cell.Value = string.Empty;
                    }
                }
            }
            #endregion

            #region test master pricing

            var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
            testPricing.Name = "Test Master Pricing";
            range = testPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);
            int testCol = 1;
            int testRow = 9;
            for (int i = 1; i <= testColumns.Count; i++)
            {
                if (testColumns[i - 1].SubColumns == null)
                {

                    testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                    testPricing.Row(testRow).Height = 25;

                    testPricing.Cells[testRow, testCol, testRow + 1, testCol].Merge = true;
                    cell = testPricing.Cells[testRow, testCol, testRow + 1, testCol];
                    cell.Value = testColumns[i - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);

                    if (testColumns[i - 1].PropName.ToLower() == "incrementedprice")
                    {
                        decimal value = 0;

                        if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                            value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                        cell.Value = testColumns[i - 1].Name + " by " + Truncate(value, 3) + "%";
                    }
                }
                else
                {
                    testPricing.Column(testCol).Width = testColumns[i - 1].Width;
                    testPricing.Row(testRow).Height = 25;
                    testPricing.Cells[testRow, testCol, testRow, testCol + 2].Merge = true;
                    cell = testPricing.Cells[testRow, testCol, testRow, testCol + 2];
                    cell.Value = testColumns[i - 1].Name;
                    cell = GiveCellStyleHeaderProperties(cell);

                    colCount = i;

                    foreach (var col in testColumns[i - 1].SubColumns)
                    {
                        testPricing.Column(testCol).Width = 20;
                        testPricing.Row(testRow).Height = 25;
                        cell = testPricing.Cells[testRow + 1, testCol];
                        cell.Value = col.ToUpper();
                        cell = GiveCellStyleHeaderProperties(cell);
                        testCol++;
                    }

                    testCol--;

                }

                testCol++;

            }
            testRow += 2;
            testCol = 1;
            for (int i = 1; i <= testMaster.TestList.Count; i++)
            {
                testCol = 1;
                for (int j = 1; j <= testColumns.Count; j++)
                {
                    if (testColumns[j - 1].SubColumns == null)
                    {
                        cell = testPricing.Cells[testRow, testCol];
                        cell = GiveCellStyleProperties(cell);
                        if (testColumns[j - 1].PropName.ToLower() != "incrementedprice")
                        {
                            if (testColumns[j - 1].UseValue)
                            {
                                cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
                            }
                            else
                            {
                                cell.Value = i;
                            }
                        }
                        else
                        {
                            decimal value = 0;

                            if (tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").Any())
                                value = tenderView.IntTndValues.Where(x => x.Description == "IncrementByPercentage").FirstOrDefault().Value;

                            cell.Value = Truncate(testMaster.TestList[i - 1].Inr + (testMaster.TestList[i - 1].Inr * value / 100), 3);
                        }
                    }
                    else
                    {
                        foreach (var column in testColumns[j - 1].SubColumns)
                        {
                            cell = testPricing.Cells[testRow, testCol];
                            cell = GiveCellStyleProperties(cell);
                            cell.Value = GetPropValue(testMaster.TestList[i - 1], column);

                            testCol++;
                        }

                        testCol--;
                    }
                    testCol++;
                }
                testRow++;
            }
            #endregion

            #region bank guarantee

            var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
            bankGuaranteeSheet.Name = "Bank Guarantee";

            range = bankGuaranteeSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int bgRowCount = 9;
            int bgColCount = 1;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Contract Value : " + contractValue;
            cell = GiveCellStyleHeaderProperties(cell);
            bgColCount += 3;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Delivery Month : " + deliveryMonth;
            cell = GiveCellStyleHeaderProperties(cell);
            bgRowCount++;
            bgColCount = 1;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
            cell.Value = "Grace Period : " + gracePeriod;
            cell = GiveCellStyleHeaderProperties(cell);
            bgColCount += 3;

            bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
            cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
            cell.Value = "Performance Period : " + performancePeriod;
            cell = GiveCellStyleHeaderProperties(cell);

            bankGuaranteeSheet.Row(1).Height = 25;
            bankGuaranteeSheet.Row(2).Height = 25;
            bankGuaranteeSheet.Column(1).Width = 90;
            bankGuaranteeSheet.Column(3).Width = 90;

            for (int i = 1; i <= bgColumns.Count; i++)
            {
                bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
                bankGuaranteeSheet.Row(3).Height = 25;
                bankGuaranteeSheet.Row(3).Height = 25;
                cell = bankGuaranteeSheet.Cells[11, i];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = bgColumns[i - 1].Name;
            }

            for (int i = 1; i <= bgList.Count; i++)
            {
                for (int j = 1; j <= bgColumns.Count; j++)
                {
                    cell = bankGuaranteeSheet.Cells[i + 11, j];
                    cell = GiveCellStyleProperties(cell);

                    if (bgColumns[j - 1].UseValue)
                    {
                        cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
                    }
                    else
                    {
                        cell.Value = "";
                    }
                }
            }
            bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
            cell = bankGuaranteeSheet.Cells[bgList.Count + 12, bgColumns.Count];
            cell = GiveCellStyleProperties(cell);

            int whileCount = 1;
            while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
            {
                bankGuaranteeSheet.Row(whileCount).Height = 20;
                whileCount++;
            }

            #endregion

            #region markup

            var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
            markupSheet.Name = "Markup";

            range = markupSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int markupRowCount = 9;
            int markupColCount = 1;

            foreach (var col in markupColumns)
            {
                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = col.Name;
                markupSheet.Column(markupColCount).Width = col.Width;
                markupColCount++;
            }

            markupRowCount = 10;
            markupColCount = 1;
            foreach (var mark in markupDataset.MarkupDetails)
            {
                bool isPBG = false;
                isPBG = mark.MarkupId == 16 ? true : false;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = mark.Markup;
                markupColCount++;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = isPBG ? (markupDataset.TndType == 2 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
                    : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;

                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = isPBG ? (markupDataset.TndType == 1 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
                    : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;


                cell = markupSheet.Cells[markupRowCount, markupColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
                cell.Value = Convert.ToDecimal(cell.Value) == 0 ? "" : Convert.ToString(cell.Value);
                markupColCount++;


                switch (mark.MarkupId)
                {
                    case 10: //financing
                        {

                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 17: //financing sales cr
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 16: //pbg
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }
                            break;
                        }
                    case 18: //interest savings on advance
                        {
                            string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }

                            string value2 = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
                            if (!string.IsNullOrEmpty(value))
                            {
                                cell = markupSheet.Cells[markupRowCount, markupColCount];
                                cell.Value = value2;
                                cell = GiveCellStyleProperties(cell, true);
                                markupColCount++;
                            }

                            break;
                        }
                }

                markupColCount = 1;
                markupRowCount++;
            }
            markupSheet.Column(5).Width = 20;
            markupSheet.Column(6).Width = 20;

            int markupWhile = 1;
            while (markupWhile < markupRowCount)
            {
                markupSheet.Row(markupWhile).Height = 20;
                markupWhile++;
            }

            markupWhile++;

            markupRowCount = 10;
            //Create table for travel, lodging and boarding
            markupSheet.Cells[markupRowCount, 8, markupRowCount, 9].Merge = true;
            cell = markupSheet.Cells[markupRowCount, 8, markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Travel, Lodging and Boarding";
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "No. Of Persons";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "No. Of Days";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "Fare";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            cell = markupSheet.Cells[markupRowCount, 8];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = "Lodging";

            cell = markupSheet.Cells[markupRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;
            markupRowCount++;

            //PackingPercentage
            pckPercentage = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "packingpercentage") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "packingpercentage").Select(y => y.Value).FirstOrDefault() : 0;

            int tempRowCount = markupRowCount + 2, tempColCount = 8;

            pckPercentage = pckPercentage > 0 ? (pckPercentage - 1) * 100 : 0;
            cell = markupSheet.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Packing Material Weight as Percentage";

            cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = pckPercentage;
            tempRowCount += 2;

            cell = markupSheet.Cells[tempRowCount, 8];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Cross Currency Margin";

            cell = markupSheet.Cells[tempRowCount, 9];
            cell = GiveCellStyleProperties(cell, true);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempRowCount += 2;
            //Create table for currency master
            markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
            cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Currency";
            tempRowCount++;

            cell = markupSheet.Cells[tempRowCount, 8];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "International Currency";

            cell = markupSheet.Cells[tempRowCount, 9];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Convert Into INR";

            cell = markupSheet.Cells[tempRowCount, 10];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Actual Value";

            cell = markupSheet.Cells[tempRowCount, 11];
            cell = GiveCellStyleProperties(cell, true, true);
            cell.Value = "Considered Value";
            tempRowCount++;

            tempColCount = 8;

            decimal tempEuroValue = 0, tempUsdValue = 0, tempCurrencyValue = 0, conversionRate;

            conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
            tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

            count = 0;
            foreach (var currency in currencyData.List)
            {
                if (count == 2)
                {
                    markupSheet.Cells[tempRowCount, 8, tempRowCount, 11].Merge = true;
                    cell = markupSheet.Cells[tempRowCount, 8, tempRowCount, 11];
                    cell = GiveCellStyleProperties(cell, true, true);
                    cell.Value = "Euro To Other Currencies";
                    tempRowCount++;
                }

                cell = markupSheet.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = currency.Name;

                cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = Truncate(currency.Value, 3);

                if (currency.Name.ToLower() == "euro")
                {
                    tempCurrencyValue = (tempEuroValue / tempUsdValue);

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else if (currency.Name.ToLower() == "usd")
                {
                    tempCurrencyValue = (tempUsdValue / tempEuroValue);

                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else
                {
                    tempCurrencyValue = (tempEuroValue / currency.Value);
                    cell = markupSheet.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell, true);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }

                tempCurrencyValue += conversionRate / 100;

                cell = markupSheet.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleProperties(cell, true);
                cell.Value = Truncate(tempCurrencyValue, 3);

                tempRowCount++;
                count++;
            }


            markupSheet.Column(8).Width = 40;
            markupSheet.Column(9).Width = 20;
            markupSheet.Column(10).Width = 20;
            markupSheet.Column(11).Width = 20;

            //var markupLastCol = markupSheet.Dimension.End.Column + 1;
            //cell = markupSheet.Cells[1, markupLastCol];

            #endregion

            #region freight

            var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
            freightSheet.Name = "Freight";
            freightSheet.View.FreezePanes(1, 2);

            range = freightSheet.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int freightRowCount = 9;
            int freightColCount = 1;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            freightSheet.Column(freightColCount).Width = 50;
            cell.Value = "Freight";
            cell = GiveCellStyleHeaderProperties(cell);
            freightRowCount += 1;

            var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
            var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

            foreach (var col in secondayCols)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell.Value = col;
                if (col == "Estimated Charges 20 Feet Container" || col == "Estimated Charges 40 Feet Container")
                    cell = GiveCellStyleHeaderProperties(cell);
                else
                    cell = GiveCellStyleProperties(cell);
                freightRowCount++;
            }

            cell = freightSheet.Cells[freightRowCount, 1];
            cell = GiveCellSubHeaderProperties(cell);
            freightColCount = 1;
            freightRowCount++;
            foreach (var col in actualCols)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell.Value = col;
                if (col == "Containers" || col == "Air Freight Cost")
                    cell = GiveCellStyleHeaderProperties(cell);
                else
                    cell = GiveCellStyleProperties(cell);
                freightRowCount++;
            }

            freightColCount = 2;

            foreach (var mod in freightData)
            {
                freightRowCount = 9;

                freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                cell = GiveCellStyleHeaderProperties(cell);
                cell.Value = mod.Title;
                freightRowCount++;
                freightColCount += 2;
                foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
                {
                    freightColCount -= 2;

                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = rowCol.Value;

                    freightColCount += 2;
                    freightRowCount++;
                }

                foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
                {
                    freightColCount -= 2;

                    if (rowCol.IsHeading)
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = rowCol.SubColumns[0].Name;
                        freightColCount += 2;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = rowCol.SubColumns[1].Name;
                    }
                    else
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Value;
                        freightColCount += 2;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = rowCol.Data;
                    }
                    freightRowCount++;

                }



                //freightColCount -= 2;
                cell = freightSheet.Cells[freightRowCount, freightColCount - 2];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Data";
                freightSheet.Column(freightColCount - 2).Width = 15;

                cell = freightSheet.Cells[freightRowCount, freightColCount - 1];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Total Cost";
                freightSheet.Column(freightColCount - 1).Width = 15;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellSubHeaderProperties(cell);
                cell.Value = "Remarks";
                freightSheet.Column(freightColCount).Width = 30;

                freightRowCount++;
                foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
                {
                    freightColCount -= 2;
                    if (row.IsHeading)
                    {
                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
                        cell = GiveCellStyleHeaderProperties(cell);
                        freightColCount += 2;
                    }
                    else
                    {
                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Data;
                        freightColCount++;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Value;
                        freightColCount++;

                        cell = freightSheet.Cells[freightRowCount, freightColCount];
                        cell = GiveCellStyleProperties(cell);
                        cell.Value = row.Remark;

                    }
                    freightRowCount++;
                }


                freightColCount += 2;
            }

            freightRowCount = 9;
            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Freight Total";
            freightRowCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Sr. No.";
            freightColCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Description";
            freightColCount++;

            cell = freightSheet.Cells[freightRowCount, freightColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Value";
            freightRowCount++;
            freightColCount -= 2;
            count = 1;
            foreach (var row in freightTotal)
            {
                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = count;
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = row.Description;
                freightSheet.Column(freightColCount).Width = 30;
                freightColCount++;

                cell = freightSheet.Cells[freightRowCount, freightColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = row.Value;
                freightSheet.Column(freightColCount).Width = 20;
                freightColCount -= 2;
                freightRowCount++;
                count++;
            }

            int freightWhileCount = 1;
            while (freightWhileCount < freightRowCount)
            {
                freightSheet.Row(freightWhileCount).Height = 20;
                freightWhileCount++;
            }

            #endregion

            #region mainView

            var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
            tenderPricing.Name = "Tender Pricing";

            range = tenderPricing.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            int mainRowCount = 9;
            int mainColCount = 1;
            foreach (var cols in mainColumns)
            {
                tenderPricing.Column(mainColCount).Width = cols.Width;

                if (cols.Exclude == true)
                {
                    bool isMerged = tenderPricing.Cells[mainRowCount, mainColCount].Merge;
                    if (isMerged)
                    {
                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = cols.Name;
                        mainRowCount++;
                    }
                    else
                    {
                        tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1 + otherCurrencies.Count].Merge = true;
                        cell = tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1];
                        cell = GiveCellStyleHeaderProperties(cell);
                        cell.Value = cols.Name;
                        mainRowCount++;
                    }
                }
                else
                {
                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = cols.Name;
                    mainRowCount++;
                }

                if (cols.SubColumns != null && cols.SubColumns.Count > 0)
                {
                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellSubHeaderProperties(cell);
                    cell.Value = cols.SubColumns[0];
                }

                mainRowCount++;

                foreach (var row in cols.Rows)
                {
                    cell = tenderPricing.Cells[mainRowCount, mainColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row;
                    mainRowCount++;
                }

                //ExcelRange cell1 = tenderPricing.Cells[mainRowCount, mainColCount]; ;

                mainColCount++;
                mainRowCount = 9;
            }

            tempRowCount = mainRowCount;
            tempColCount = tenderPricing.Dimension.End.Column + 2;

            whileCount = 3;
            while (whileCount < tenderPricing.Dimension.End.Row)
            {
                tenderPricing.Row(whileCount).Height = 18;
                whileCount++;
            }

            cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column];
            cell = GiveLastRowHighlightProperties(cell);

            //Create table for currency master
            tempRowCount = 1;
            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Packing Material Weight as Percentage";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell);
            cell.Value = pckPercentage;
            tempRowCount += 2;

            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Cross Currency Margin";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleProperties(cell);
            cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempRowCount += 2;
            //Create table for currency master
            tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
            cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Currency";
            tempRowCount++;

            cell = tenderPricing.Cells[tempRowCount, tempColCount];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "International Currency";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Convert Into INR";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Actual Value";

            cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Considered Value";
            tempRowCount++;

            tempEuroValue = 0;
            tempUsdValue = 0; tempCurrencyValue = 0;
            conversionRate = 0;

            conversionRate = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "conversionrate") ?
                                markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "conversionrate").Select(y => y.Value).FirstOrDefault() : 0;

            tempEuroValue = currencyData.List.Where(x => x.Name.ToLower() == "euro").FirstOrDefault().Value;
            tempUsdValue = currencyData.List.Where(x => x.Name.ToLower() == "usd").FirstOrDefault().Value;

            count = 0;
            foreach (var currency in currencyData.List)
            {
                if (count == 2)
                {
                    tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3].Merge = true;
                    cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 3];
                    cell = GiveCellStyleHeaderProperties(cell);
                    cell.Value = "Euro To Other Currencies";
                    tempRowCount++;
                }

                cell = tenderPricing.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = currency.Name;

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Truncate(currency.Value, 3);

                if (currency.Name.ToLower() == "euro")
                {
                    tempCurrencyValue = (tempEuroValue / tempUsdValue);

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else if (currency.Name.ToLower() == "usd")
                {
                    tempCurrencyValue = (tempUsdValue / tempEuroValue);

                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }
                else
                {
                    tempCurrencyValue = (tempEuroValue / currency.Value);
                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 2];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = Truncate(tempCurrencyValue, 3);
                }

                tempCurrencyValue += conversionRate / 100;

                cell = tenderPricing.Cells[tempRowCount, tempColCount + 3];
                cell = GiveCellStyleProperties(cell);
                cell.Value = Truncate(tempCurrencyValue, 3);

                tempRowCount++;
                count++;
            }

            tenderPricing.Column(1).Width = 30;
            tenderPricing.Column(tempColCount).Width = 45;
            tenderPricing.Column(tempColCount + 1).Width = 20;
            tenderPricing.Column(tempColCount + 2).Width = 20;
            tenderPricing.Column(tempColCount + 3).Width = 20;
            #endregion

            #region Internal Calculations

            var calculations = excelPackage.Workbook.Worksheets.Add("Internal Calculations");
            calculations.Name = "Container Details";

            range = calculations.Cells[1, 1, 7, 2];
            FillCommonFields(ref range, tenderView);

            cell = calculations.Cells[9, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Containers";

            cell = calculations.Cells[9, 2];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "40Ft";

            cell = calculations.Cells[9, 3];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "20Ft";

            tempRowCount = 10;
            tempColCount = 1;
            foreach (var row in rows)
            {
                tempColCount = 1;

                cell = calculations.Cells[tempRowCount, tempColCount];
                cell = GiveCellStyleProperties(cell);
                cell.Value = row.Description;
                tempColCount++;
                if (row.Description.ToUpper() != "FACTOR FOR DISTRIBUTING FREIGHT" && row.Description.ToUpper() != "TOTAL UNIT WEIGHT (MT)")
                {
                    cell = calculations.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.value40FtContr;
                    tempColCount++;

                    cell = calculations.Cells[tempRowCount, tempColCount];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.value20FtContr;
                }
                else
                {
                    calculations.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1].Merge = true;
                    cell = calculations.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1];
                    cell = GiveCellStyleProperties(cell);
                    cell.Value = row.Value;
                }
                tempRowCount++;
            }
            calculations.Column(1).Width = 35;
            calculations.Column(2).Width = 20;
            calculations.Column(3).Width = 20;

            whileCount = 1;
            while (whileCount < tempRowCount)
            {
                calculations.Row(whileCount).Height = 20;
                whileCount++;
            }
            #endregion

            return excelPackage;

        }

        #endregion

        public bool CheckIfPropertyExistsInDynamicObject(dynamic obj, string property)
        {
            var data = ((IDictionary<string, dynamic>)obj);
            if (data.ContainsKey(property))
                return true;
            else
                return false;
        }

        public dynamic GetPropertyValueFromDynamicObject(dynamic obj, string property)
        {
            var data = ((IDictionary<string, dynamic>)obj);
            dynamic outData = null;
            data.TryGetValue(property, out outData);
            return Convert.ToInt32(outData);
        }

        public decimal GetAverage(List<dynamic> data)
        {
            decimal result = 0;
            if (data.Count > 0)
            {
                foreach (var temp in data)
                {
                    result += temp;
                }

                return result / data.Count;
            }
            else
                return 0;
        }

        public decimal GetSum(List<dynamic> data)
        {
            decimal result = 0;
            if (data.Count > 0)
            {
                foreach (var temp in data)
                {
                    result += temp;
                }
                return result;
            }
            else
                return 0;
        }

        public ExcelRange GiveCellStyleProperties(ExcelRange cellObect, bool shouldHighlight = false, bool header = false)
        {
            cellObect.Style.Font.Color.SetColor(Color.Black);
            cellObect.Style.Font.Size = 11;

            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            if (shouldHighlight)
            {
                if (header)
                {
                    cellObect.Style.Font.Bold = true;
                    cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else
                {
                    cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                cellObect.Style.Fill.BackgroundColor.SetColor(highlight);
            }
            else
            {
                cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                cellObect.Style.Fill.BackgroundColor.SetColor(lightGray);
            }

            cell.Style.WrapText = true;
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

        public ExcelRange GiveLastRowHighlightProperties(ExcelRange cellObect)
        {
            //cellObect.Style.Font.Bold = true;
            cellObect.Style.Font.Color.SetColor(Color.White);
            cellObect.Style.Font.Size = 12;
            cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cellObect.Style.Fill.BackgroundColor.SetColor(darkBlue);

            return cellObect;
        }

        public ExcelRange DarkGrayHighlight(ExcelRange cellObect)
        {
            cellObect.Style.Font.Size = 12;
            cellObect.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cellObect.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            border = cellObect.Style.Border;
            border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
            cellObect.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cellObect.Style.Fill.BackgroundColor.SetColor(darkGray);

            return cellObect;
        }

        public decimal GetValueInCrores(decimal value)
        {
            return value / 10000000;
        }

        decimal Truncate(decimal d, byte decimals)
        {
            decimal r = Math.Round(d, decimals, MidpointRounding.AwayFromZero);
            //if (d > 0 && r > d)
            //{
            //    return r - new decimal(1, 0, 0, false, decimals);
            //}
            //else if (d < 0 && r < d)
            //{
            //    return r + new decimal(1, 0, 0, false, decimals);
            //}
            return r;
        }

        public void FillCommonFields(ref ExcelRange excelRange, TenderDetailsModel mainViewData)
        {
            dynamic tndDetails = mainViewData.RevList.Where(x => x.RevisionNo == mainViewData.TenderRevisionNo).FirstOrDefault();
            cell = excelRange[1, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Project Name";

            cell = excelRange[1, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = mainViewData.ProjectName;

            cell = excelRange[2, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Tender File Number";

            cell = excelRange[2, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = mainViewData.TenderFileNo;

            cell = excelRange[3, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "NTPC Name";

            cell = excelRange[3, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = mainViewData.EndCustName;

            cell = excelRange[4, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "EPC Customer Name";

            cell = excelRange[4, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = mainViewData.EPCCustName;

            cell = excelRange[5, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Revision no";

            cell = excelRange[5, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = mainViewData.TenderRevisionNo;

            cell = excelRange[6, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Published Date";

            cell = excelRange[6, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = tndDetails.PublishedDate;
            cell.Style.Numberformat.Format = "dd/mm/yyyy h:mm";

            cell = excelRange[7, 1];
            cell = GiveCellStyleHeaderProperties(cell);
            cell.Value = "Created by";

            cell = excelRange[7, 2];
            cell = GiveCellStyleProperties(cell);
            cell.Value = tndDetails.CreatedByName;
        }

        //public int GetLastEmptyRow(ExcelWorksheet sheet)
        //{

        //}

    }
}