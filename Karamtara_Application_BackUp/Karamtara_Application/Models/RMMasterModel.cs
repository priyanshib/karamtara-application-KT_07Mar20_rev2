using System.Collections.Generic;

namespace Karamtara_Application.Models
{
    public class RMMasterModel
    {
        public string MaterialName { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialCategoryTxt { get; set; }
        public List<RawMaterialModel> RawMaterialList { get; set; }
        public int MaterialId { get; set; }
        public string MaterialType { get; set; }
        public int SaveStatus { get; set; }
        public List<string> SearchAutoComplete { get; set; }
        public string MatCategory { get;set; }
        public List<string> CategoryList { get; set; }
        public string MaterialGroup { get; set; }
        public int GroupId { get; set; }
    }

    public class RawMaterialModel
    {
        public int MaterialId { get; set; }
        public string Material { get; set; }
        public string MaterialDesc { get; set; }
        public string MaterialType { get; set; }
        public string Category { get; set; }
    }

    public class RMGroupTypeModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}