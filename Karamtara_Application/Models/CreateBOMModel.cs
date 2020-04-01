using System.Collections.Generic;

namespace Karamtara_Application.Models
{
    public class CreateBOMModel
    {
        public List<string> SearchList { get; set; }
        public string SearchText { get; set; }
        public List<ProductGroupModel> ProductGroupList { get; set; }
        public List<AssemblyMasterModel> AssemblyList { get; set; }
        public List<SubAssemblyListModel> SubAssemblyList { get; set; }
        public List<ComponentModel> ComponentList { get; set; }

        public string DisplayText { get; set; }
        public int AssemblyId { get; set; }
        public int ProjectId { get; set; }
        public int EnquiryId { get; set; }
        public int ProductType { get; set; }
        public int BomId { get; set; }
        public string CustomerName { get; set; }
        public string ProjectName { get; set; }
        public List<BomMasterModel> BomMasterList { get; set; }
        public bool IsNewBom { get; set; }
        public int ProductGroupId { get; set; }
        public int RevisionNo { get; set; }
        public bool BomHasProducts { get; set; }
        public List<MasterModel> MasterList { get; set; }
        public int OldRevisionNo { get; set; }
        public string CatalogueNo { get; set; }
        public int SubAssmId { get; set; }
        public bool IsBomRevTemp { get; set; }
        public bool IsTemp { get; set; }
        public string BomType { get; set; }
        public bool IsPublished { get; set; }
        public string TNumber { get; set; }

        public bool CreateBOMHasRows { get; set; }
        public int ComponentId { get; set; }
        public string Bom { get; set; }
        public string BomSource { get; set; }
        public bool IsEdit { get; set; }
        public SummaryModel Summary { get; set; }

        public string AssignedToNames { get; set; }
        public string PublishedBy { get; set; }

        public bool IsTenderUser { get; set; }
    }

}