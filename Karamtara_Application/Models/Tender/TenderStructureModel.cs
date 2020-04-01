using System.Collections.Generic;

namespace Karamtara_Application.Models.Tender
{
    public class TenderStructureModel
    {
        public int StrId { get; set; }
        public string Structure { get; set; }
        public int EnquiryId { get; set; }
        public int BomId { get; set; }
        public int RevisionNo { get; set; }
        public List<StructureDetails> LineList { get; set; }
        public List<StructureDetails> LotList { get; set; }
        public List<StructureDetails> PackageList { get; set; }

        public int LineId { get; set; }
        public int LotId { get; set; }
        public int PackageId { get; set; }
        public List<TenderStructureModel> DetailsList { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionNo { get; set; }

        //extra data for test linking
        public int ProductIdentityId { get; set; }
        public int TestQuantity { get; set; }
        public List<string> TestNames { get; set; }
        public string ProductName { get; set; }
        public int ProdType { get; set; }
    }

    public class StructureDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StrType { get; set; }
        public int TestQuantity { get; set; }
        public bool IsSelected { get; set; }
        public decimal Price { get; set; }
    }

}