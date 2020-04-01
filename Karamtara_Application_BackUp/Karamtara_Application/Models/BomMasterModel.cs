using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class BomMasterModel
    {
        public int ProductId { get; set; }
        public int Type { get; set; }
        public int BomId { get; set; }
        public int RevisionNo { get; set; }
        public string ProductName { get; set; }
    }
}