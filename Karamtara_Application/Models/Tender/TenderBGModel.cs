using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models.Tender
{
    public class TenderBGModel
    {
        public int TenderNo { get; set; }
        public int TenderRevisionNo { get; set; }
        public decimal DeliveryMonth { get; set; }
        public decimal ContractValue { get; set; }
        public int BGType { get; set; }
        public string BGTypeString { get; set; }
        public decimal BGMonth { get; set; }
        public decimal CommisionPercentage { get; set; }
        public decimal BGPercentage { get; set; }
        public decimal BGCostPercentage { get; set; }
        public decimal BGAmount { get; set; }
        public int UserId { get; set; }
        public TenderBGModel AdvBg { get; set; }
        public TenderBGModel PfmBg { get; set; }
        public TenderBGModel RetBg { get; set; }
        public decimal PerformancePeriod { get; set; }
        public decimal GracePeriod { get; set; }
        public List<TenderBGModel> List { get; set; }
    }

    public class BGType
    {
        public int Id { get; set; }
        public string BGDescription { get; set; }
    }
}