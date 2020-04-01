using System.Collections.Generic;

namespace Karamtara_Application.Models.Tender
{
    public class CurrencyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public decimal Value { get; set; }
        public bool DisplayInView { get; set; }
        public string Message { get; set; }
    }

    public class CurrencyMaster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class CurrencyDetailModel
    {
        public List<CurrencyModel> List { get; set; }
        //public List<CurrencyConversionModel> ConversionList { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
        public int TenderType { get; set; }
        public List<IntTenderDetails> intTenderDetails { get; set; }
        public List<CurrencyMaster> CurrencyList { get; set; }
        public int CurrencyId { get; set; }
    }
}