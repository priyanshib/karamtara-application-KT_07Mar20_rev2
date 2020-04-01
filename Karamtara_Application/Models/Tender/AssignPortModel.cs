using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Models.Tender
{
    public class AssignPortModel
    {
        public int PortId { get; set; }
        public int PortType { get; set; }
        public string PortTypeName { get; set; }
        public string PortName { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public List<CurrencyModel> CurrencyList { get; set; }
        public int TndId { get; set; }
        public int TndRevNo { get; set; }
        public bool IsEdit { get; set; }
        public bool IsEnabled { get; set; }
        public List<AssignPortModel> DomPortList { get; set; }
        public List<AssignPortModel> IntlPortList { get; set; }
    }
}