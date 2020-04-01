using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class TestMasterModel
    {
        public List<AssemblyMasterModel> SubAssemblies { get; set; }
        public List<TestModel> TestList { get; set; }
    }
}