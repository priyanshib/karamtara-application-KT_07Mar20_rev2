using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models.Tender
{
    public class MarkupModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public decimal Value { get; set; }
        public string Message { get; set; }
        public bool Flag { get; set; }
    }
}