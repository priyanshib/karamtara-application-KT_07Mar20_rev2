using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models.Tender
{

    public class RawMaterialPricingModel
    {
        public string MaterialDesc { get; set; }
        public int SrNo { get; set; }
        public int Id { get; set; }
        public string MaterialGroup { get; set; }
        public double Price { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
    }

    public class RawMaterialPricingDetail
    {
        public List<RawMaterialPricingModel> RawMaterialList { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
    }

    public class RawMaterialReportModel
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; }
        public string GroupName { get; set; }
        public List<RMRevPricingModel> Pricing { get; set; }
        public List<int> Revisions { get; set; }
    }

    public class RMRevPricingModel
    {
        public int TenderId { get; set; }
        public int TenderRevId { get; set; }
        public int RawMaterialId { get; set; }
        public decimal Price { get; set; }
    }
}