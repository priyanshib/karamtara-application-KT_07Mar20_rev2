using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class SummaryModel
    {
        public List<DataModel> data { get; set; }
    }

    public class DataModel
    {
        public string Name { get; set; }
        public decimal UnitGrWt { get; set; }
        public decimal UnitNetWt { get; set; }
    }
}