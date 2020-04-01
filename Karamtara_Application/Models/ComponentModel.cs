using System.Collections.Generic;

namespace Karamtara_Application.Models
{
    public class ComponentModel
    {
        public int ComponentId { get; set; }
        public string CatalogueNo { get; set; }
        public string ComponentName { get; set; }
        public string RawMaterialId { get; set; }
        public string RawMaterial { get; set; }
        public int Qty { get; set; }
        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
        public int SubAssemblyId { get; set; }
        public string Size { get; set; }
        public string DrawingNo { get; set; }
        public string MaterialGrade { get; set; }
        public int AssemblyId { get; set; }
        public int ProductGroupId { get; set; }
        // Galvanized Required
        public bool GalvanizedRequired { get; set; }
        public int GalvanizedRequiredInt { get; set; }
        public string GalvanizedMaterial { get; set; }
        //unit master
        public string Unit { get; set; }
        public int UnitId { get; set; }

        public List<DropDown> GalvanizedRequiredList
        {
            get
            {
                return new List<DropDown>() { new DropDown { Text = "Yes", Value = 1 }, new DropDown { Text = "No", Value = 0 } };
            }
            set
            {

            }
        }
        public List<GalvanizedMaterials> GalvanizedMaterialList { get; set; }
        public List<ComponentModel> ComponentList { get; set; }
        public List<UnitMaster> UnitList { get; set; }
    }

    public class GalvanizedMaterials
    {
        public int Id { get; set; }
        public string Element { get; set; }
    }

    public class UnitMaster
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public string UnitName { get; set; }
    }
}