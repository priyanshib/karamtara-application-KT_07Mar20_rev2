using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class FilterColumn
    {
        public int ColumnId {get;set;}
        public string ColumnName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class Orders
    {
        public int OrderId { get; set; }
        public string Order { get; set; }
        public bool IsSelected { get; set; }
    }

    public class FilterData
    {
        public List<FilterColumn> Columns { get; set; }
        public List<Orders> Orders { get; set; }
    }
}