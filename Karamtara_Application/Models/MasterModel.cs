using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class MasterModel
    {
        public int PrimaryId { get; set; }
        public string SrNo { get; set; }
        public int Id { get; set; }
        public int Type { get; set; }
        public int ComponentId { get; set; }
        public int SubAssemblyId { get; set; }
        public int AssemblyId { get; set; }
        public int ProductGroupId { get; set; }
        public int BOMId { get; set; }
        public int MasterType { get; set; }
        public int Quantity { get; set; }
        public int ParentId { get; set; }
        public int ParentType { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string CatalogueNo { get; set; }
        public string TechnicalName { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public string Grade { get; set; }
        public string DrawingNo { get; set; }

        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
        public decimal TotalUnitGrWt { get; set; }
        public decimal TotalUnitNetWt { get; set; }

        public bool IsRelated { get; set; }
        public bool IsDirectChild { get; set; }

        public decimal UnitCost { get; set; }
        public string TestIds { get; set; }
        public decimal LineQuantity { get; set; }
        public decimal SalesCost { get; set; }
        public decimal ExWorks { get; set; }
        public decimal Freight { get; set; }
        public List<MasterLineModel> LineDetails { get; set; }

        public string Unit { get; set; }

        public decimal WastagePercentage { get; set; }
        public decimal CalculatedUnitGrWt { get; set; }
        public decimal TotalCalcUnitGrWt { get; set; }
        public decimal GalCost { get; set; }
        public decimal BlackCost { get; set; }
        public decimal CostPerPiece { get; set; }

    }

    public class MasterLineModel
    {
        public int LineId { get; set; }
        public string LineName { get; set; }
        public int Quantity { get; set; }
        public decimal SalesCost { get; set; }
        public decimal ExWorks { get; set; }
        public decimal Freight { get; set; }
    }
}