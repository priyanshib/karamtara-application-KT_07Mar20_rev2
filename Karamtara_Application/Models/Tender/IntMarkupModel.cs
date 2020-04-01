using System.Collections.Generic;

namespace Karamtara_Application.Models.Tender
{
    public class IntMarkupModel
    {
        public List<IntTenderDetails> TenderDetails { get; set; }
        public List<MarkupDetails> MarkupDetails { get; set; }
        public CurrencyDetailModel Currency { get; set; }
        public decimal AdvBGpercent { get; set; }
        public decimal PBGValue { get; set; }
        public int TndType { get; set; }
        public string Message { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
    }

    public class MarkupDetails
    {
        public int MarkupId { get; set; }
        public string Markup { get; set; }
        public string IndiaVal { get; set; }
        public string ItalyVal { get; set; }
        public string BOVal { get; set; }
    }

    public class IntTenderDetails
    {
        public decimal Value { get; set; }
        public string Description { get; set; }
    }

}