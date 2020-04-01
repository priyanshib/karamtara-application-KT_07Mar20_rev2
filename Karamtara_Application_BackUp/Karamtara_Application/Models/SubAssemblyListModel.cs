namespace Karamtara_Application.Models
{
    public class SubAssemblyListModel
    {
        public int SubAssemblyId { get; set; }
        public int CategoryId { get; set; }
        public string SubAssemblyName { get; set; }
        public string CatalogueNo { get; set; }
        public int Quantity { get; set; }
        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
        public string RawMaterial { get; set; }
        public string Size { get; set; }
        public int AssemblyId { get; set; }
        public string DrawingNo { get; set; }
    }
}