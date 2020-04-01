using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class ProductGroupListModel
    {
        public int ProductGroupId { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string Summary { get; set; }
    }
}