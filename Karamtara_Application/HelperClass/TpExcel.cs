//using Karamtara_Application.DAL;
//using Karamtara_Application.DAL.Tender;
//using Karamtara_Application.Models;
//using Karamtara_Application.Models.Tender;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;

//namespace Karamtara_Application.HelperClass
//{
//    public class TpExcel
//    {
//        #region variables
//        ExcelRange cell;
//        ExcelFill fill;
//        Border border;
//        RawMaterialPricingDAL rmpDAL;
//        MarkupPricingDAL mDAL;
//        FreightChargesDAL fDAL;
//        TenderDetailsDAL tenderDetailsDAL;
//        TestDAL testDAL;
//        int whileCount = 0;
//        Color lightGray = System.Drawing.ColorTranslator.FromHtml("#e9e9e9");
//        Color darkBlue = System.Drawing.ColorTranslator.FromHtml("#105483");
//        Color highlight = System.Drawing.ColorTranslator.FromHtml("#FFC300");
//        Color darkGray = System.Drawing.ColorTranslator.FromHtml("#cbcbcb");
//        int rowCount = 0;
//        int colCount = 1;
//        List<int> _revisions;
//        #endregion

//        public byte[] DownloadInternationalTenderPricingDataI(int bomId, int bomRevId, int tenderId, int tenderRevId)

//        {
//            #region rawmData
//            rmpDAL = new RawMaterialPricingDAL();
//            var rawMatMaster = rmpDAL.GetRawPricingList(tenderId, tenderRevId);

//            var rawMatColumns = new List<ColumnModel>() {
//                new ColumnModel("Sr.No", 8, 12,"SrNo"),
//                new ColumnModel("Raw Material", 40, 12,"MaterialDesc"),
//                new ColumnModel("Group", 20, 12,"MaterialGroup"),
//                new ColumnModel("Price", 10, 12,"Price")
//            };
//            #endregion

//            #region testData
//            testDAL = new TestDAL();
//            var testMaster = testDAL.GetTestPricingList();
//            var testColumns = new List<ColumnModel>() {
//                new ColumnModel("Sr.No", 8, 12,"Id", false),
//                new ColumnModel("Name", 30, 12,"TestName"),
//                new ColumnModel("Description", 50, 12,"TestDescription"),
//                new ColumnModel("Group Type", 30, 12,"Type"),
//                new ColumnModel("Bundle Type", 30, 12,"Bundle"),
//                new ColumnModel("Line Type", 30, 12,"KVLine"),
//                new ColumnModel("UTS", 30, 12,"UTS"),
//                new ColumnModel("Summary", 50, 12,"Summary"),
//                new ColumnModel("Price", 10, 12,"Price")
//            };

//            #endregion

//            #region bgData
//            tenderDetailsDAL = new TenderDetailsDAL();
//            var bgList = tenderDetailsDAL.GetBGData(tenderId, tenderRevId);
//            var bgColumns = new List<ColumnModel>() {

//                new ColumnModel("Bank Guarantee Type", 30, 12,"BGTypeString"),
//                new ColumnModel("Bank Guarantee Month", 30, 12,"BGMonth"),
//                new ColumnModel("Commision (%)", 30, 12,"CommisionPercentage"),
//                new ColumnModel("Bank Guarantee (%)", 30, 12,"BGPercentage"),
//                new ColumnModel("Bank Guarantee Amount", 30, 12,"BGAmount"),
//                new ColumnModel("Bank Guarantee Cost (%)", 30, 12,"BGCostPercentage"),
//            };

//            var contractValue = Truncate(bgList.Any() ? bgList.FirstOrDefault().ContractValue : 0, 3);
//            var deliveryMonth = Truncate(bgList.Any() ? bgList.FirstOrDefault().DeliveryMonth : 0, 3);
//            var performancePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().PerformancePeriod : 0, 3);
//            var gracePeriod = Truncate(bgList.Any() ? bgList.FirstOrDefault().GracePeriod : 0, 3);

//            #endregion

//            #region markupData
//            intDetailsDAL = new IntTenderDetailsDAL();
//            var markupDataset = intDetailsDAL.GetMarkupPricingList(tenderId, tenderRevId);
//            //var markupData
//            //var markupData = new List<MarkupDataModel>();
//            var markupColumns = new List<ColumnModel>()
//            {
//                new ColumnModel("Markup", 50, 0, "Markup"),
//                new ColumnModel("India", 30, 0, "India"),
//                new ColumnModel("Italy", 30, 0, "Italy"),
//                new ColumnModel("BO", 30, 0, "BO"),
//            };

//            #endregion

//            #region freightData

//            var freightMasterData = intDetailsDAL.GetFreightChargesList(tenderId, tenderRevId);

//            var freightRows = new List<RowModel>()
//            {
//                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
//                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
//                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
//                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
//                new RowModel(){ Description = "Containers", IsHeading = true},
//                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
//                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
//                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
//                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
//                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
//                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
//                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
//                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
//                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
//                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
//            };

//            var commonCols = new List<ColumnModel>()
//            {
//                new ColumnModel("Data", 20, 12, ""),
//                new ColumnModel("Total Cost", 20, 12, ""),
//                new ColumnModel("Remarks", 30, 12, ""),
//            };

//            List<TableModel> freightData = new List<TableModel>();

//            foreach (var val in freightMasterData.TenderPortNames)
//            {
//                TableModel mod = new TableModel();
//                mod.Id = val.Id;
//                mod.Title = val.PortName;
//                mod.Rows = new List<RowModel>()
//            {
//                new RowModel(){ Description = "Total weight(MT)", KeyName="TotalWeight" },
//                new RowModel(){ Description = "Supply by container(MT)", KeyName="SupplyByContainer" },
//                new RowModel(){ Description = "Supply by Air (MT)", KeyName="SupplyByAir" },
//                new RowModel(){ Description = "Cost Estimates", KeyName="CostEstimates"},
//                new RowModel(){ Description = "Containers", IsHeading = true},
//                new RowModel(){ Description = "No of 20 Ft Containers: @16 MT / Container", KeyName="NoOfFtContainers" },
//                new RowModel(){ Description = "Sea freight per 20'ft container", KeyName="SeaFrtPerContainer" },
//                new RowModel(){ Description = "Sea freight per Mt", KeyName="SeaFrtPerMT" },
//                new RowModel(){ Description = "Sea freight per Mt (Rounded-UP)", KeyName="SeaFrtPerMTRound" },
//                new RowModel(){ Description = "Total Sea Freight", KeyName="TotalSeaFreight" },
//                new RowModel(){ Description = "Air Freight Cost", IsHeading=true},
//                new RowModel(){ Description = "Freight to airport", KeyName="FreightToAirport" },
//                new RowModel(){ Description = "Freight per Kg", KeyName="FreightPerKg" },
//                new RowModel(){ Description = "Total Cost (Air)", KeyName="TotalCostAir" },
//                new RowModel(){ Description = "Overall Cost", KeyName="OverallTotal" },
//            };

//                mod.Rows.ForEach(x =>
//                {
//                    var temp = freightMasterData.TenderPortDetails.Where(y => y.Description == x.KeyName && y.PortId == mod.Id).Select(y => y).FirstOrDefault();
//                    if (temp != null)
//                    {
//                        x.Value = temp.Cost;
//                        x.Data = temp.Data;
//                        x.Remark = temp.Remarks;
//                    }
//                });

//                mod.Rows.AddRange(new List<RowModel>()
//                {
//                    new RowModel() { Description = "Sea Freight", KeyName = "SeaFreight", ExcludeFromLoop = true, Value = string.Format("{0} {1}", val.SeaFreight, val.CurrencyName), Data=val.SeaFreight, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
//                    new RowModel(){ Description = "Packing Material as Weight Percentage",  ExcludeFromLoop = true, Value = val.PackingPercentage, SubColumns = new List<ColumnModel>(){ new ColumnModel("Value",20,12,"")} },
//                    new RowModel(){ Description = "Estimated Charges 20 Feet Container", KeyName="", ExcludeFromLoop =true, IsHeading= true, SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
//                    new ColumnModel("Rupees", 20,12,"")} },
//                    new RowModel() { Description = "Ocean Freight/MT", KeyName = "", ExcludeFromLoop = true, Value = (val.SeaFreight * val.PackingPercentage), Data=(val.SeaFreight * val.PackingPercentage * val.CurrencyValue), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
//                    new ColumnModel("Rupees", 20,12,"")}},
//                    new RowModel() { Description = "Total In Rupees", KeyName = "", ExcludeFromLoop = true, Value = "", Data=(val.SeaFreight * val.PackingPercentage * val.CurrencyValue), SubColumns = new List<ColumnModel>(){ new ColumnModel(val.CurrencyName, 20,12,""),
//                    new ColumnModel("Rupees", 20,12,"")}},
//                });

//                freightData.Add(mod);
//            }

//            #endregion

//            #region tender pricing view

//            TenderDetailsDAL tndDetailsDAL = new TenderDetailsDAL();
//            var tenderView = tndDetailsDAL.GetBomProdDetails(bomId, bomRevId, tenderId, tenderRevId);
//            var portList = tenderView.TndPortDetails;

//            tenderView.MasterList.ForEach(x =>
//            {
//                x.Quantity = tenderView.IntTndQtyList.Any(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type) ?
//                tenderView.IntTndQtyList.Where(y => y.PrimaryId == x.PrimaryId && y.TypeId == x.Type).Select(z => z.Quantity).FirstOrDefault() : 0;
//            });

//            var mainColumns = new List<ColumnModel>()
//                {
//                new ColumnModel("Sr.No", 8, 12, "srno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
//                new ColumnModel("Description", 60, 12, "desc"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
//                new ColumnModel("Unit", 10, 12, "unit"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
//                new ColumnModel("Drawing No.", 16, 12, "drawno"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
//                new ColumnModel("Quantity", 12, 12, "qty"){ Rows = new List<dynamic>(), SubColumns = new List<string>() { ""}},
//                new ColumnModel("Unit Weight", 15, 12, "unitwt") { SubColumns = new List<string>() { "KG"}, Rows = new List<dynamic>() },
//                new ColumnModel("India Cost", 20, 12, "indiacost") { SubColumns = new List<string>() { "INR" }, Rows = new List<dynamic>() },
//                new ColumnModel("Total India Cost", 25, 12, "totalindiacost") { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
//                new ColumnModel("BO", 15, 12, "bo", false) { SubColumns = new List<string>() { "EURO" }, Rows = new List<dynamic>() },
//                new ColumnModel("Cost of Sales", 25, 12, "costofsales", false) { SubColumns = new List<string>() { "INR"},Rows = new List<dynamic>() },
//                new ColumnModel("Selling Price(Exworks)", 25, 12, "spinr", false) { SubColumns = new List<string>() { "INR"}, Rows = new List<dynamic>() },
//                new ColumnModel("Selling Price(Exworks)", 25, 12, "speuro", false) { SubColumns = new List<string>() { "EURO"}, Rows = new List<dynamic>() },
//                };

//            var index = 5;
//            portList.ForEach(x =>
//            {
//                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
//                {
//                    SubColumns = new List<string>() { "USD" },
//                    UniqueId = x.Id,
//                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
//                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
//                    ExtraKey = x.CurrencyName,
//                    ExtraValue = x.CurrencyValue,
//                    Rows = new List<dynamic>()
//                }); index++;

//                mainColumns.Insert(index, new ColumnModel(string.Format("Unit Price {0}", x.PortName), 30, 12, "ports", false, true)
//                {
//                    SubColumns = new List<string>() { "EURO" },
//                    UniqueId = x.Id,
//                    Value = tenderView.TndPortFrtDetails.Any(y => y.Description == "OverallTotal" && y.PortId == x.Id) ?
//                    tenderView.TndPortFrtDetails.Where(y => y.Description == "OverallTotal" && y.PortId == x.Id).Select(y => y.Cost).FirstOrDefault() : 0,
//                    ExtraKey = x.CurrencyName,
//                    ExtraValue = x.CurrencyValue,
//                    Rows = new List<dynamic>()
//                }); index++;
//            });

//            decimal costOfSales = 0, indiaCost = 0, indiaTotal = 0, italyTotalCost = 0, boCost = 0, euroToInrCost = 0, boTotal = 0,
//                    spInr = 0, indiaMargin = 0, italyMargin = 0, negoItaly = 0, usdToInr = 0, cifPort = 0, cifPortUsd = 0;

//            decimal euroToUsd = 0, spEuro = 0, mt = 0, noOfCon = 0, rsPerCon = 0, totalRsForCon = 0, distFact = 0, loadingFactor = 0, containerCharges = 0, currencyConvRate = 0;

//            decimal indiaCostSummation = 0, boSummation = 0, costOfSalesSummation = 0, exWorksSumInr = 0, exWorksEuro = 0;

//            indiaTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
//            italyTotalCost = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
//            indiaMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.IndiaVal).FirstOrDefault() ?? "0");
//            italyMargin = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 31).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
//            negoItaly = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 32).Select(x => x.ItalyVal).FirstOrDefault() ?? "0");
//            euroToInrCost = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "euro").Select(x => x.Value).FirstOrDefault();
//            boTotal = Convert.ToDecimal(tenderView.TndMarkupDetails.Where(x => x.MarkupId == 27).Select(x => x.BOVal).FirstOrDefault() ?? "0");
//            usdToInr = tenderView.CurrencyList.Where(x => x.Name.ToLower() == "usd").Select(x => x.Value).FirstOrDefault();
//            loadingFactor = portList.Count > 0 ? portList.FirstOrDefault().PackingPercentage : 0;
//            currencyConvRate = tenderView.IntTndValues.Where(x => x.Description == "ConversionRate").Select(y => y.Value).FirstOrDefault();
//            euroToUsd = usdToInr == 0 ? 0 : ((euroToInrCost / usdToInr) + (currencyConvRate / 100));
//            containerCharges = tenderView.IntTndValues.Where(x => x.Description == "RsPerContainer").Select(y => y.Value).FirstOrDefault();

//            foreach (var col in mainColumns)
//            {
//                switch (col.PropName)
//                {
//                    case "srno":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.SrNo) ? "" : x.SrNo));
//                            break;
//                        }
//                    case "desc":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Name) ? "" : x.Name));
//                            col.Rows.Add("Total");
//                            break;
//                        }
//                    case "unit":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.Unit) ? "" : x.Unit));
//                            break;
//                        }
//                    case "drawno":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => string.IsNullOrEmpty(x.DrawingNo) ? "" : x.DrawingNo));
//                            break;
//                        }
//                    case "qty":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.Quantity : (dynamic)(string.Empty))));
//                            break;
//                        }
//                    case "unitwt":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)x.TotalUnitNetWt : (dynamic)(string.Empty))));
//                            mt = Truncate((GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.Quantity * x.UnitNetWt, 3) : (dynamic)(0))).ToList()) * loadingFactor) / 1000, 3);
//                            break;
//                        }
//                    case "indiacost":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(string.Empty))));
//                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate(x.UnitCost, 3) : (dynamic)(0))).ToList()));
//                            break;
//                        }
//                    case "totalindiacost":
//                        {
//                            col.Rows = new List<dynamic>(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(string.Empty))));
//                            col.Rows.Add(GetSum(tenderView.MasterList.Select(x => (x.Type != 1 ? (dynamic)Truncate((x.UnitCost * x.Quantity), 3) : (dynamic)(0))).ToList()));
//                            break;
//                        }
//                }
//            }

//            foreach (var mod in tenderView.MasterList)
//            {
//                indiaCost = mod.UnitCost;
//                indiaCostSummation += Truncate(indiaCost, 3);

//                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
//                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;
//                boSummation += Truncate(mod.Quantity * boCost, 3);

//                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);
//                costOfSalesSummation += Truncate(mod.Quantity * costOfSales, 3);
//            }

//            noOfCon = Math.Ceiling(mt / 23);
//            rsPerCon = containerCharges * usdToInr;
//            totalRsForCon = noOfCon * rsPerCon;
//            distFact = costOfSalesSummation == 0 ? 0 : Truncate(Math.Ceiling((totalRsForCon / costOfSalesSummation) * 100), 1);

//            foreach (var mod in tenderView.MasterList)
//            {
//                indiaCost = mod.UnitCost;

//                boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
//                    tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

//                costOfSales = (indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal);

//                spInr = (costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100));
//                exWorksSumInr += Truncate(mod.Quantity * spInr, 3);

//                spEuro = euroToInrCost == 0 ? 0 : (spInr / euroToInrCost);
//                exWorksEuro += Truncate(mod.Quantity * spEuro, 3);
//            }

//            var last = tenderView.MasterList.Last();
//            foreach (var mod in tenderView.MasterList)
//            {
//                if (mod.Type != 1)
//                {
//                    indiaCost = mod.UnitCost;

//                    boCost = tenderView.IntTndQtyList.Any(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId) ?
//                        tenderView.IntTndQtyList.Where(y => y.TypeId == mod.Type && y.PrimaryId == mod.PrimaryId).Select(y => y.BO).FirstOrDefault() : 0;

//                    costOfSales = Truncate((indiaCost * indiaTotal * italyTotalCost) + (boCost * euroToInrCost * boTotal), 3);

//                    spInr = Truncate((costOfSales * (1 + distFact / 100) * (1 + (italyMargin / 100)) * (1 + (indiaMargin / 100))) / (1 - (negoItaly / 100)), 3);

//                    spEuro = euroToInrCost == 0 ? 0 : Truncate(spInr / euroToInrCost, 3);

//                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSales, 3));
//                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boCost, 3));
//                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spInr, 3));
//                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(spEuro, 3));
//                    //spUsdSummation += (spInr * mod.Quantity);
//                }
//                else
//                {
//                    mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add("");
//                    mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add("");
//                    mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add("");
//                    mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add("");
//                }
//                var lastPortId = 0;
//                foreach (var col in mainColumns.Where(x => x.Exclude))
//                {
//                    if (mod.Type != 1)
//                    {
//                        var seaFreight = Truncate(Math.Ceiling((col.Value / exWorksSumInr) * 100), 1);
//                        cifPort = exWorksSumInr == 0 ? 0 : Truncate(spEuro * (1 + (seaFreight / 100)), 3);

//                        if (col.SubColumns.Contains("USD"))
//                        {
//                            cifPortUsd = Truncate(cifPort * (euroToUsd), 1);
//                            col.Rows.Add(Truncate(cifPortUsd, 3));
//                            col.Summation = col.Summation + Truncate(cifPortUsd, 1);
//                        }
//                        else
//                        {
//                            col.Rows.Add(cifPort);
//                            col.Summation = col.Summation + Truncate(cifPort, 1);
//                            lastPortId = col.UniqueId;
//                        }

//                    }
//                    else
//                        col.Rows.Add("");

//                    if (mod.Equals(last))
//                    {
//                        col.Rows.Add(col.Summation);
//                    }

//                }
//            };

//            mainColumns.Where(x => x.PropName == "costofsales").Select(x => x).FirstOrDefault().Rows.Add(Truncate(costOfSalesSummation, 3));
//            mainColumns.Where(x => x.PropName == "bo").Select(x => x).FirstOrDefault().Rows.Add(Truncate(boSummation, 3));
//            mainColumns.Where(x => x.PropName == "spinr").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksSumInr, 3));
//            mainColumns.Where(x => x.PropName == "speuro").Select(x => x).FirstOrDefault().Rows.Add(Truncate(exWorksEuro, 3));
//            #endregion

//            #region currency

//            var currencyData = intDetailsDAL.GetCurrencyList(tenderId, tenderRevId);

//            #endregion

//            using (var excelPackage = new ExcelPackage())
//            {
//                excelPackage.Workbook.Properties.Author = "Karamtara";
//                excelPackage.Workbook.Properties.Title = "Karamtara";

//                #region Raw material pricing section

//                var rawMatPricing = excelPackage.Workbook.Worksheets.Add("Raw Material Pricing");
//                rawMatPricing.Name = "RawMaterialPricing";

//                for (int i = 1; i <= rawMatColumns.Count; i++)
//                {
//                    rawMatPricing.Column(i).Width = rawMatColumns[i - 1].Width;
//                    rawMatPricing.Row(1).Height = 25;
//                    cell = rawMatPricing.Cells[1, i];
//                    cell = GiveCellStyleHeaderProperties(cell);
//                    cell.Value = rawMatColumns[i - 1].Name;
//                    cell.Style.Font.Size = rawMatColumns[i - 1].FontSize;
//                }

//                for (int i = 1; i <= rawMatMaster.Count; i++)
//                {
//                    for (int j = 1; j <= rawMatColumns.Count; j++)
//                    {
//                        cell = rawMatPricing.Cells[i + 1, j];
//                        cell = GiveCellStyleProperties(cell);
//                        rawMatPricing.Row(i + 1).Height = 20;
//                        if (rawMatColumns[j - 1].UseValue)
//                        {
//                            cell.Value = GetPropValue(rawMatMaster[i - 1], rawMatColumns[j - 1].PropName);
//                        }
//                        else
//                        {
//                            cell.Value = string.Empty;
//                        }
//                    }
//                }
//                #endregion

//                #region test master pricing

//                var testPricing = excelPackage.Workbook.Worksheets.Add("Test Master Pricing");
//                testPricing.Name = "Test Master Pricing";

//                for (int i = 1; i <= testColumns.Count; i++)
//                {
//                    testPricing.Column(i).Width = testColumns[i - 1].Width;
//                    testPricing.Row(1).Height = 25;
//                    cell = testPricing.Cells[1, i];
//                    cell.Value = testColumns[i - 1].Name;
//                    cell = GiveCellStyleHeaderProperties(cell);
//                }

//                for (int i = 1; i <= testMaster.TestList.Count; i++)
//                {
//                    for (int j = 1; j <= testColumns.Count; j++)
//                    {
//                        cell = testPricing.Cells[i + 1, j];
//                        cell = GiveCellStyleProperties(cell);
//                        if (testColumns[j - 1].UseValue)
//                        {
//                            cell.Value = GetPropValue(testMaster.TestList[i - 1], testColumns[j - 1].PropName);
//                        }
//                        else
//                        {
//                            cell.Value = string.Empty;
//                        }
//                    }
//                }
//                #endregion

//                #region bank guarantee

//                var bankGuaranteeSheet = excelPackage.Workbook.Worksheets.Add("Bank Guarantee");
//                bankGuaranteeSheet.Name = "Bank Guarantee";

//                int bgRowCount = 1;
//                int bgColCount = 1;

//                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
//                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
//                cell.Value = "Contract Value : " + contractValue;
//                cell = GiveCellStyleHeaderProperties(cell);
//                bgColCount += 3;

//                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
//                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
//                cell.Value = "Delivery Month : " + deliveryMonth;
//                cell = GiveCellStyleHeaderProperties(cell);
//                bgRowCount++;
//                bgColCount = 1;

//                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
//                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount];
//                cell.Value = "Grace Period : " + gracePeriod;
//                cell = GiveCellStyleHeaderProperties(cell);
//                bgColCount += 3;

//                bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2].Merge = true;
//                cell = bankGuaranteeSheet.Cells[bgRowCount, bgColCount, bgRowCount, bgColCount + 2];
//                cell.Value = "Performance Period : " + performancePeriod;
//                cell = GiveCellStyleHeaderProperties(cell);

//                bankGuaranteeSheet.Row(1).Height = 25;
//                bankGuaranteeSheet.Row(2).Height = 25;
//                bankGuaranteeSheet.Column(1).Width = 90;
//                bankGuaranteeSheet.Column(3).Width = 90;

//                for (int i = 1; i <= bgColumns.Count; i++)
//                {
//                    bankGuaranteeSheet.Column(i).Width = bgColumns[i - 1].Width;
//                    bankGuaranteeSheet.Row(3).Height = 25;
//                    bankGuaranteeSheet.Row(3).Height = 25;
//                    cell = bankGuaranteeSheet.Cells[3, i];
//                    cell = GiveCellStyleHeaderProperties(cell);
//                    cell.Value = bgColumns[i - 1].Name;
//                }

//                for (int i = 1; i <= bgList.Count; i++)
//                {
//                    for (int j = 1; j <= bgColumns.Count; j++)
//                    {
//                        cell = bankGuaranteeSheet.Cells[i + 3, j];
//                        cell = GiveCellStyleProperties(cell);

//                        if (bgColumns[j - 1].UseValue)
//                        {
//                            cell.Value = GetPropValue(bgList[i - 1], bgColumns[j - 1].PropName);
//                        }
//                        else
//                        {
//                            cell.Value = "";
//                        }
//                    }
//                }
//                bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count].Value = Truncate(bgList.Sum(x => x.BGCostPercentage), 3);
//                cell = bankGuaranteeSheet.Cells[bgList.Count + 1 + 3, bgColumns.Count];
//                cell = GiveCellStyleProperties(cell);

//                whileCount = 1;
//                while (whileCount <= bankGuaranteeSheet.Dimension.End.Row)
//                {
//                    bankGuaranteeSheet.Row(whileCount).Height = 20;
//                    whileCount++;
//                }

//                #endregion

//                #region markup

//                var markupSheet = excelPackage.Workbook.Worksheets.Add("Markup");
//                markupSheet.Name = "Markup";

//                int markupRowCount = 1;
//                int markupColCount = 1;

//                foreach (var col in markupColumns)
//                {
//                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                    cell = GiveCellStyleHeaderProperties(cell);
//                    cell.Value = col.Name;
//                    markupSheet.Column(markupColCount).Width = col.Width;
//                    markupColCount++;
//                }

//                markupRowCount = 2;
//                markupColCount = 1;
//                foreach (var mark in markupDataset.MarkupDetails)
//                {
//                    bool isPBG = false;
//                    isPBG = mark.MarkupId == 16 ? true : false;

//                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = mark.Markup;
//                    markupColCount++;

//                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = isPBG ? (markupDataset.TndType == 1 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal))
//                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.IndiaVal) ? "0" : mark.IndiaVal);
//                    markupColCount++;

//                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = isPBG ? (markupDataset.TndType == 2 ? markupDataset.PBGValue : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal))
//                        : Convert.ToDecimal(string.IsNullOrEmpty(mark.ItalyVal) ? "0" : mark.ItalyVal);
//                    markupColCount++;

//                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = Convert.ToDecimal(string.IsNullOrEmpty(mark.BOVal) ? "0" : mark.BOVal);
//                    markupColCount++;

//                    switch (mark.MarkupId)
//                    {
//                        case 10: //financing
//                            {
//                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "financingdays") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "financingdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
//                                if (!string.IsNullOrEmpty(value))
//                                {
//                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                                    cell.Value = value;
//                                    cell = GiveCellStyleProperties(cell, true);
//                                    markupColCount++;
//                                }
//                                break;
//                            }
//                        case 17: //financing sales cr
//                            {
//                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "finsalescrdays") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "finsalescrdays").Select(y => string.Format("{0} days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
//                                if (!string.IsNullOrEmpty(value))
//                                {
//                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                                    cell.Value = value;
//                                    cell = GiveCellStyleProperties(cell, true);
//                                    markupColCount++;
//                                }
//                                break;
//                            }
//                        case 16: //pbg
//                            {
//                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "intrate") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "intrate").Select(y => string.Format("{0} ROI", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
//                                if (!string.IsNullOrEmpty(value))
//                                {
//                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                                    cell.Value = value;
//                                    cell = GiveCellStyleProperties(cell, true);
//                                    markupColCount++;
//                                }
//                                break;
//                            }
//                        case 18: //interest savings on advance
//                            {
//                                string value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavedays") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavedays").Select(y => string.Format("{0} Days", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
//                                if (!string.IsNullOrEmpty(value))
//                                {
//                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                                    cell.Value = value;
//                                    cell = GiveCellStyleProperties(cell, true);
//                                    markupColCount++;
//                                }

//                                string value2 = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "interestsavemnths") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "interestsavemnths").Select(y => string.Format("{0} Months", Convert.ToInt32(y.Value))).FirstOrDefault() : string.Empty;
//                                if (!string.IsNullOrEmpty(value))
//                                {
//                                    cell = markupSheet.Cells[markupRowCount, markupColCount];
//                                    cell.Value = value2;
//                                    cell = GiveCellStyleProperties(cell, true);
//                                    markupColCount++;
//                                }

//                                break;
//                            }
//                    }

//                    markupColCount = 1;
//                    markupRowCount++;
//                }
//                markupSheet.Column(5).Width = 20;
//                markupSheet.Column(6).Width = 20;

//                int markupWhile = 1;
//                while (markupWhile < markupRowCount)
//                {
//                    markupSheet.Row(markupWhile).Height = 20;
//                    markupWhile++;
//                }

//                markupWhile++;

//                //Create table for travel, lodging and boarding
//                markupSheet.Cells[2, 8, 2, 9].Merge = true;
//                cell = markupSheet.Cells[2, 8, 2, 9];
//                cell = GiveCellStyleProperties(cell, true, true);
//                cell.Value = "Travel, Lodging and Boarding";

//                cell = markupSheet.Cells[3, 8];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = "No. Of Persons";

//                cell = markupSheet.Cells[3, 9];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofpersons") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofpersons").Select(y => y.Value).FirstOrDefault() : 0;

//                cell = markupSheet.Cells[4, 8];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = "No. Of Days";

//                cell = markupSheet.Cells[4, 9];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "noofdays") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "noofdays").Select(y => y.Value).FirstOrDefault() : 0;

//                cell = markupSheet.Cells[5, 8];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = "Fare";

//                cell = markupSheet.Cells[5, 9];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "fare") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "fare").Select(y => y.Value).FirstOrDefault() : 0;

//                cell = markupSheet.Cells[6, 8];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = "Lodging";

//                cell = markupSheet.Cells[6, 9];
//                cell = GiveCellStyleProperties(cell, true);
//                cell.Value = markupDataset.TenderDetails.Any(x => x.Description.ToLower() == "lodging") ?
//                                    markupDataset.TenderDetails.Where(x => x.Description.ToLower() == "lodging").Select(y => y.Value).FirstOrDefault() : 0;

//                //Create table for currency master
//                markupSheet.Cells[8, 8, 8, 9].Merge = true;
//                cell = markupSheet.Cells[8, 8, 8, 9];
//                cell = GiveCellStyleProperties(cell, true, true);
//                cell.Value = "Currency";

//                int tempRowCount = 9;
//                int tempColCount = 8;

//                foreach (var currency in currencyData.List)
//                {
//                    cell = markupSheet.Cells[tempRowCount, tempColCount];
//                    cell = GiveCellStyleProperties(cell, true);
//                    cell.Value = currency.Name;

//                    cell = markupSheet.Cells[tempRowCount, tempColCount + 1];
//                    cell = GiveCellStyleProperties(cell, true);
//                    cell.Value = Truncate(currency.Value, 3);
//                    tempRowCount++;
//                }

//                markupSheet.Column(8).Width = 20;
//                markupSheet.Column(9).Width = 20;

//                //var markupLastCol = markupSheet.Dimension.End.Column + 1;
//                //cell = markupSheet.Cells[1, markupLastCol];

//                #endregion

//                #region freight

//                var freightSheet = excelPackage.Workbook.Worksheets.Add("Freight");
//                freightSheet.Name = "Freight";
//                freightSheet.View.FreezePanes(1, 2);

//                int freightRowCount = 1;
//                int freightColCount = 1;

//                cell = freightSheet.Cells[freightRowCount, freightColCount];
//                freightSheet.Column(freightColCount).Width = 50;
//                cell.Value = "Freight";
//                cell = GiveCellStyleHeaderProperties(cell);
//                freightRowCount += 2;

//                var actualCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => !x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();
//                var secondayCols = freightData.Count > 0 ? freightData[0].Rows.Where(x => x.ExcludeFromLoop).Select(y => y.Description).ToList() : new List<string>();

//                foreach (var col in actualCols)
//                {
//                    cell = freightSheet.Cells[freightRowCount, freightColCount];
//                    cell.Value = col;
//                    if (col == "Containers" || col == "Air Freight Cost")
//                        cell = GiveCellStyleHeaderProperties(cell);
//                    else
//                        cell = GiveCellStyleProperties(cell);
//                    freightRowCount++;
//                }

//                foreach (var col in secondayCols)
//                {
//                    cell = freightSheet.Cells[freightRowCount, freightColCount];
//                    cell.Value = col;
//                    if (col == "Estimated Charges 20 Feet Container")
//                        cell = GiveCellStyleHeaderProperties(cell);
//                    else
//                        cell = GiveCellStyleProperties(cell);
//                    freightRowCount++;
//                }

//                cell = freightSheet.Cells[2, 1];
//                cell = GiveCellSubHeaderProperties(cell);

//                freightColCount = 2;
//                foreach (var mod in freightData)
//                {
//                    freightRowCount = 1;

//                    freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
//                    cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
//                    cell = GiveCellStyleHeaderProperties(cell);
//                    cell.Value = mod.Title;
//                    freightRowCount++;

//                    cell = freightSheet.Cells[freightRowCount, freightColCount];
//                    cell = GiveCellSubHeaderProperties(cell);
//                    cell.Value = "Data";
//                    freightSheet.Column(freightColCount).Width = 15;
//                    freightColCount++;

//                    cell = freightSheet.Cells[freightRowCount, freightColCount];
//                    cell = GiveCellSubHeaderProperties(cell);
//                    cell.Value = "Total Cost";
//                    freightSheet.Column(freightColCount).Width = 15;
//                    freightColCount++;

//                    cell = freightSheet.Cells[freightRowCount, freightColCount];
//                    cell = GiveCellSubHeaderProperties(cell);
//                    cell.Value = "Remarks";
//                    freightSheet.Column(freightColCount).Width = 30;

//                    freightRowCount++;
//                    foreach (var row in mod.Rows.Where(x => !x.ExcludeFromLoop).Select(y => y))
//                    {
//                        freightColCount -= 2;
//                        if (row.IsHeading)
//                        {
//                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
//                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
//                            cell = GiveCellStyleHeaderProperties(cell);
//                            freightColCount += 2;
//                        }
//                        else
//                        {
//                            cell = freightSheet.Cells[freightRowCount, freightColCount];
//                            cell = GiveCellStyleProperties(cell);
//                            cell.Value = row.Data;
//                            freightColCount++;

//                            cell = freightSheet.Cells[freightRowCount, freightColCount];
//                            cell = GiveCellStyleProperties(cell);
//                            cell.Value = row.Value;
//                            freightColCount++;

//                            cell = freightSheet.Cells[freightRowCount, freightColCount];
//                            cell = GiveCellStyleProperties(cell);
//                            cell.Value = row.Remark;

//                        }
//                        freightRowCount++;
//                    }

//                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 1).Select(y => y))
//                    {
//                        freightColCount -= 2;

//                        freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2].Merge = true;
//                        cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 2];
//                        cell = GiveCellStyleProperties(cell);
//                        cell.Value = rowCol.Value;

//                        freightColCount += 2;
//                        freightRowCount++;
//                    }

//                    foreach (var rowCol in mod.Rows.Where(x => x.ExcludeFromLoop && x.SubColumns.Count == 2).Select(y => y))
//                    {
//                        freightColCount -= 2;

//                        if (rowCol.IsHeading)
//                        {
//                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
//                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
//                            cell = GiveCellStyleHeaderProperties(cell);
//                            cell.Value = rowCol.SubColumns[0].Name;
//                            freightColCount += 2;

//                            cell = freightSheet.Cells[freightRowCount, freightColCount];
//                            cell = GiveCellStyleHeaderProperties(cell);
//                            cell.Value = rowCol.SubColumns[1].Name;
//                        }
//                        else
//                        {
//                            freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1].Merge = true;
//                            cell = freightSheet.Cells[freightRowCount, freightColCount, freightRowCount, freightColCount + 1];
//                            cell = GiveCellStyleProperties(cell);
//                            cell.Value = rowCol.Value;
//                            freightColCount += 2;

//                            cell = freightSheet.Cells[freightRowCount, freightColCount];
//                            cell = GiveCellStyleProperties(cell);
//                            cell.Value = rowCol.Data;
//                        }
//                        freightRowCount++;

//                    }
//                    freightColCount += 2;
//                }

//                int freightWhileCount = 1;
//                while (freightWhileCount < freightRowCount)
//                {
//                    freightSheet.Row(freightWhileCount).Height = 20;
//                    freightWhileCount++;
//                }

//                #endregion

//                #region mainView

//                var tenderPricing = excelPackage.Workbook.Worksheets.Add("Tender Pricing");
//                tenderPricing.Name = "Tender Pricing";

//                int mainRowCount = 1;
//                int mainColCount = 1;
//                foreach (var cols in mainColumns)
//                {
//                    tenderPricing.Column(mainColCount).Width = cols.Width;

//                    if (cols.Exclude == true)
//                    {
//                        bool isMerged = tenderPricing.Cells[mainRowCount, mainColCount].Merge;
//                        if (isMerged)
//                        {
//                            cell = tenderPricing.Cells[mainRowCount, mainColCount];
//                            cell = GiveCellStyleHeaderProperties(cell);
//                            cell.Value = cols.Name;
//                            mainRowCount++;
//                        }
//                        else
//                        {
//                            tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1].Merge = true;
//                            cell = tenderPricing.Cells[mainRowCount, mainColCount, mainRowCount, mainColCount + 1];
//                            cell = GiveCellStyleHeaderProperties(cell);
//                            cell.Value = cols.Name;
//                            mainRowCount++;
//                        }
//                    }
//                    else
//                    {
//                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
//                        cell = GiveCellStyleHeaderProperties(cell);
//                        cell.Value = cols.Name;
//                        mainRowCount++;
//                    }

//                    if (cols.SubColumns != null && cols.SubColumns.Count > 0)
//                    {
//                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
//                        cell = GiveCellSubHeaderProperties(cell);
//                        cell.Value = cols.SubColumns[0];
//                    }

//                    mainRowCount++;

//                    foreach (var row in cols.Rows)
//                    {
//                        cell = tenderPricing.Cells[mainRowCount, mainColCount];
//                        cell = GiveCellStyleProperties(cell);
//                        cell.Value = row;
//                        mainRowCount++;
//                    }
//                    mainColCount++;
//                    mainRowCount = 1;
//                }

//                tempRowCount = 1;
//                tempColCount = tenderPricing.Dimension.End.Column + 2;

//                //Create table for currency master
//                tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1].Merge = true;
//                cell = tenderPricing.Cells[tempRowCount, tempColCount, tempRowCount, tempColCount + 1];
//                cell = GiveCellStyleHeaderProperties(cell);
//                cell.Value = "Currency";
//                tempRowCount++;

//                foreach (var currency in currencyData.List)
//                {
//                    cell = tenderPricing.Cells[tempRowCount, tempColCount];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = currency.Name;

//                    cell = tenderPricing.Cells[tempRowCount, tempColCount + 1];
//                    cell = GiveCellStyleProperties(cell);
//                    cell.Value = Truncate(currency.Value, 3);
//                    tempRowCount++;
//                }

//                tenderPricing.Column(tempColCount).Width = 15;
//                tenderPricing.Column(tempColCount + 1).Width = 15;

//                whileCount = 3;
//                while (whileCount < tenderPricing.Dimension.End.Row)
//                {
//                    tenderPricing.Row(whileCount).Height = 18;
//                    whileCount++;
//                }

//                cell = tenderPricing.Cells[tenderPricing.Dimension.End.Row, 1, tenderPricing.Dimension.End.Row, tenderPricing.Dimension.End.Column - 3];
//                cell = GiveLastRowHighlightProperties(cell);
//                #endregion

//                return excelPackage.GetAsByteArray();
//            }
//        }
//    }
//}