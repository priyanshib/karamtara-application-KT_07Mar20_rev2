using System;
using System.Collections.Generic;
using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.Models
{

    public class MasterDataModel
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public int Id { get; set; }
        public Boolean Delete { get; set; }
        public List<MasterDataModel> MasterList { get; set; }
        public List<string> SearchAutoComplete { get; set; }
        public string Message { get; set; }
        private List<DropDown> MTypeList = new List<DropDown>() {
                    new DropDown { Text = "Group Type", Value = 1,IsTender=false},
                    new DropDown { Text = "Line Voltage", Value = 2 ,IsTender=false},
                    new DropDown { Text = "Conductor Type", Value = 3 ,IsTender=false},
                    new DropDown { Text = "Bundle Type", Value = 4,IsTender=false},
                    new DropDown { Text = "Bundle Spacing", Value = 5 ,IsTender=false},
                    new DropDown { Text = "UTS Value", Value = 6 ,IsTender=false},
                    new DropDown { Text = "Conductor Name", Value = 7 ,IsTender=false},
                    new DropDown { Text = "Truck Metric", Value = 8 ,IsTender=true}
                };

        public List<DropDown> MasterTypeList
        {
            get
            {
                return MTypeList;
            }
            set
            {
                this.MTypeList = value;
            }
        }

        public class DropDown
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public bool IsTender { get; set; }
        }
    }
}