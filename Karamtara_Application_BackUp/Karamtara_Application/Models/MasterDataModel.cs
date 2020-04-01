using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class MasterDataModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public List<string> SearchAutoComplete { get; set; }

        public List<MasterListModel> MasterDataList { get; set; }

        public List<DropDownMaster> MasterList
        {
            get
            {
                return new List<DropDownMaster>()
                {
                    new DropDownMaster { Text = "Select Type", Value = 0 },
                    new DropDownMaster { Text = "GroupType", Value = 1 },
                    new DropDownMaster { Text = "LineVoltage", Value = 2 },
                    new DropDownMaster { Text = "ConductorType", Value = 3 },
                    new DropDownMaster { Text = "BundleType", Value = 4 },
                    new DropDownMaster { Text = "BundleSpacing", Value = 5 },
                    new DropDownMaster { Text = "UTSValue", Value = 6 }
                };
            }
            set
            {

            }
        }
    }

    public class DropDownMaster
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class MasterNameModel
    {
        public string Name { get; set; }
    }

    public class MasterListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string TableName { get; set; }

        public List<MasterListModel> MasterDataList { get; set; }
    }
}