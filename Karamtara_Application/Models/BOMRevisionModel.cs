using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class BOMRevisionModel
    {
        public List<MasterModel> MasterList { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CatalogueNo { get; set; }
        public int BomId { get; set; }
        public int RevisionNo { get; set; }
        public int ProductType { get; set; }
        public Boolean IsRelated { get; set; }
        public int ParentId { get; set; }
        public int Type { get; set; }
        public int ProductGroupId { get; set; }
        public int AssemblyId { get; set; }
        public int SubAssemblyId { get; set; }
        public int ComponentId { get; set; }
        public int EnquiryId { get; set; }

    }
}