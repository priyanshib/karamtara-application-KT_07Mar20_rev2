using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class TestModel
    {
        public int Id { get; set; }
        public string TestName { get; set;}
        public string TestDescription { get; set; }
        public bool IsSelected { get; set; }
        public string Type { get; set; }
        public string KVLine { get; set; }
        public string UTS { get; set; }
        public string Bundle { get; set; }
        public string Summary { get; set; }
    }
}