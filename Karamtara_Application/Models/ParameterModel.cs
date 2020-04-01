using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class ParameterModel
    {
        public int PrimaryId { get; set; }
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public int SubAssemblyId { get; set; }
        public int AssemblyId { get; set; }
        public int ProductGroupId { get; set; }
        public int BomId { get; set; }
        public int RevId { get; set; }
        public int ParentId { get; set; }
        public int ProdType { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionId { get; set; }
    }
}