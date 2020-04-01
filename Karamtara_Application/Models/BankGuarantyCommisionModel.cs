using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class BankGuarantyCommisionModel
    {
        public int Id { get; set; }
        public decimal DeliveryMonth { get; set; }
        public decimal ContractValue { get; set; }
        public string Description { get; set;}
        public decimal Month { get; set; }
        public decimal PerOfCommission { get; set; }
        public decimal PerOfBG { get; set; }
        public decimal BGAmt { get; set; }
        public decimal BGCostPer { get; set; }    
    }
}