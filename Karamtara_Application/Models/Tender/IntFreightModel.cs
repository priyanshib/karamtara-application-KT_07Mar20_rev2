using System.Collections.Generic;

namespace Karamtara_Application.Models.Tender
{
    public class IntFreightModel
    {
        public List<TenderPortNames> TenderPortNames { get; set; }
        public List<TenderPortDetails> TenderPortDetails { get; set; }
        public List<IntTenderDetails> IntTndValues { get; set; }
        public string Message { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
    }

    public class TenderPortDetails
    {
        public int PortId { get; set; }
        public string Description { get; set; }
        public string PortTypeName { get; set; }
        public string Remarks { get; set; }
        public decimal Data { get; set; }
        public decimal Cost { get; set; }
    }

    public class TenderPortNames
    {
        public int Id { get; set; }
        public string PortName { get; set; }
        public string CurrencyName { get; set; }
        public bool IsActive { get; set; }
        public int Type { get; set; }
        public decimal CurrencyValue { get; set; }
        public decimal SeaFreight { get; set; }
        public decimal SeaFreightFortyFT { get; set; }
        public decimal PackingPercentage { get; set; }
    }
}