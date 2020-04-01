using System.Collections.Generic;

namespace Karamtara_Application.Models
{
    public class SubAssemblyMasterModel
    {
        public string SubAssemblyCode { get; set; }
        public string SubAssemblyName { get; set; }
        public string SubAssmTechName { get; set; }
        public int SubAssemblyId { get; set; }
        public string CatalogueNo { get; set; }
        public int Qty { get; set; }
        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
        public string Size { get; set; }
        public string DrawingNo { get; set; }

        public List<SubAssemblyListModel> SubAssemblyList { get; set; }
        public List<ComponentModel> ComponenetList { get; set; }
        public List<RawMaterialModel> RawMaterialList { get; set; }
        public int CategoryId { get; set; }
        public bool RenderPartialView { get; set; }
        public int Status { get; set; }
        public List<string> ProductAutoComplete { get; set; }
        public int AssemblyId { get; set; }
        public List<SubAssemblyListModel> AutoCompleteList { get; set; }

    }
}