using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models.Tender
{ 
    public class TenderDetailsModel
    {
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int EnquiryId { get; set; }
        public int BomId { get; set; }
        public int RevisionNo { get; set; }
        public string EPCCustName { get; set; }
        public string EndCustName { get; set; }
        public List<ProductGroupModel> ProductGroupList { get; set; }
        public List<AssemblyMasterModel> AssemblyList { get; set; }
        public List<SubAssemblyListModel> SubAssemblyList { get; set; }
        public List<ComponentModel> ComponentList { get; set; }
        public List<TenderBomModel> BomList { get; set; }
        public List<MasterModel> MasterList { get; set; }
        public List<LineStructure> LineList { get; set; }
        public List<dynamic> LineQtyList { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionNo { get; set; }
        public List<TenderLineValues> TndLineValuesList { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsEdit { get; set; }
        public List<TenderAuditTrial> AuditTrialList { get; set; }
        public List<TextDetails> TextList { get; set; }
        public List<CurrencyModel> CurrencyList { get; set; }
        public List<IntTndQuantity> IntTndQtyList { get; set; }
        public List<TenderPortNames> TndPortDetails { get; set; }
        public List<TenderPortDetails> TndPortFrtDetails { get; set; }
        public List<MarkupDetails> TndMarkupDetails { get; set; }
        public List<IntTenderDetails> IntTndValues { get; set; }
        public List<TenderRevisions> RevList { get; set; }
        public List<TenderStructureName> TenderStrName { get; set; }
        public List<TenderValues> TenderValues { get; set; }
        public CurrencyDetailModel CurrencyData { get; set; }
        public int TenderType { get; set; }
        public string TenderFileNo { get; set; }

    }

    public class TenderRevisions
    {
        public int RevisionNo { get; set; }
        public int TenderType { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string CreatedByName { get; set; }
    }

    public class TenderBomModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int ParentId { get; set; }
        public bool IsRelated { get; set; }
    }

    public class LineStructure
    {
        public int LineId { get; set; }
        public string LineName { get; set; }
    }

    public class TenderLineValues
    {
        public int LineId { get; set; }
        public string Description { get; set; }
        public decimal Values { get; set; }
        public string LineName { get; set; }
    }

    public class TenderValues
    {
        public string Description { get; set; }
        public decimal Values { get; set; }
    }

    public class IntTndQuantity
    {
        public int PrimaryId { get; set; }
        public int TypeId { get; set; }
        public int Quantity { get; set; }
        public decimal BO { get; set; }

    }

    public class TenderAuditTrial
    {
        public string EditedBy { get; set; }
        public string EditedDate { get; set; }
        public string Version { get; set; }
    }

    public class TextDetails
    {
        public string Message { get; set; }
        public int IsActive { get; set; }
    }

    public class TenderStructureDetails
    {
        public int LineId { get; set; }
        public string LotName { get; set; }
        public string PackageName { get; set; }
        public int StrType { get; set; }
        public string Structure { get; set; }

    }

    public class TenderStructureName
    {
        public int LineId { get; set; }
        public string Structure { get; set; }
    }



}