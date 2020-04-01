using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class TechnicalQueryModel
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public string Answer { get; set; }
        public int EnquiryId { get; set; }
    }
}