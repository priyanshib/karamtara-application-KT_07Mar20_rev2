using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class ColumnModel
    {
        public ColumnModel(string name, int width, int font, string prop, bool useValue = true, bool exclude = false)
        {
            Name = name;
            Width = width;
            FontSize = font;
            PropName = prop;
            UseValue = useValue;
            Exclude = exclude;
        }
        public string Name { get; }
        public int Width { get; }
        public int FontSize { get; }
        public string PropName { get; }
        public bool UseValue { get; set; }
        public List<string> SubColumns { get; set; }
        public bool Exclude { get; }
        public int CellMergeCount { get; set; }
        public int UniqueId { get; set; }
        public List<dynamic> Rows { get; set; }
        public dynamic Value { get; set; }
        public dynamic ExtraKey { get; set; }
        public dynamic ExtraValue { get; set; }
        public decimal Summation { get; set; }
    }

    public class MarkupDataModel
    {
        public List<DynamicColumns> Columns { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Testing { get; set; }
        public decimal OverideTestCharges { get; set; }
        public decimal TravelLodgingBoarding { get; set; }
        public decimal Development { get; set; }
        public decimal OtherTotal { get; set; }
        public decimal PercentageToUnitCost { get; set; }
        public decimal LineUnitCost { get; set; }
        public decimal FinalSubtotal { get; set; }
        public decimal Margin { get; set; }
        public int LineId { get; set; }
        public string LineName { get; set; }
    }

    public class Location
    {
        public string LocationName { get; set; }
        public decimal Charge { get; set; }
    }

    public class DynamicColumns
    {
        public string ColumnName { get; set; }
        public dynamic Value { get; set; }
    }

    public class FinalPriceModel
    {
        public int LineId { get; set; }
        public string LineName { get; set; }
        public decimal ExWorks { get; set; }
        public decimal Freight { get; set; }
        public decimal Gst { get; set; }
        public decimal Total { get; set; }
    }

    public class TableModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<ColumnModel> Columns { get; set; }
        public List<RowModel> Rows { get; set; }
    }

    public class RowModel
    {
        public string Description { get; set; }
        public dynamic Value { get; set; }
        public dynamic Data { get; set; }
        public string Remark { get; set; }
        public string KeyName { get; set; }
        public bool IsHeading { get; set; }
        public bool ExcludeFromLoop { get; set; }
        public bool OnlyStore { get; set; }
        public dynamic value20FtContr { get; set; }
        public dynamic value40FtContr { get; set; }
        public List<ColumnModel> SubColumns { get; set; }
        
    }

    public class TotalRowModel
    {
        public int LineId { get; set; }
        public decimal Value { get; set; }
        public string Key { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Freight { get; set; }
        public decimal SalesCost { get; set; }
        public decimal ExWorks { get; set; }

    }
}