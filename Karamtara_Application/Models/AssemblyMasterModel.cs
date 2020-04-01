using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class AssemblyMasterModel
    {
        public string AssemblyCode { get; set; }
        public string AssemblyName { get; set; }
        public string AssmTechName { get; set; }
        public int AssemblyId { get; set; }
        public int SubAssemblyId { get; set; }
        public int ProductGroupId { get; set; }
        public string CatalogueNo { get; set; }
        public int Quantity { get; set; }
        public List<AssemblyMasterModel> AssemblyList {get;set;}
        public List<SubAssemblyListModel> SubAssemblyList { get; set; }
        public List<ComponentModel> ComponentList { get; set; }
        public List<SubAssemblyListModel> AutoCompleteList { get; set; }
        public List<MasterModel> MasterList { get; set; }
        public List<UnitMaster> UnitList { get; set; }
        public int Status { get; set; }
        public List<string> ProductAutoComplete { get; set; }
        public string DrawingNo { get; set; }
        public string DrawingFileName { get; set; }
        public bool IsRemove { get; set; } 
        public List<UTSMS> UtsValueList { get; set; }
        public string UtsValueId { get; set; }
        public string UTS { get; set; }
        public decimal TotalGrWt { get; set; }
        public decimal TotalNetWt { get; set; }
        public int UnitId { get; set; }
        public string Unit { get; set; }
    }
}