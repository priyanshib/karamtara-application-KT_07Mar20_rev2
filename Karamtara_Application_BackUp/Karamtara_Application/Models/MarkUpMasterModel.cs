using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class MarkUpMasterModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal ReteOfInterest { get; set; }
        public decimal CreditPeriod { get; set; }
        public decimal AdvanceRecieved { get; set; }
        public string MarkUp { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Testing { get; set; }
        public decimal Othertotal { get; set; }
        public decimal Margin { get; set; }
    }
}