using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class BomAuditModel
    {
        public int Id { get; set; }
        public int BomId { get; set; }
        public int BomRevId { get; set; }
        public string UserName { get; set; }
        public string Version { get; set; }
        public DateTime Timestamp { get; set; }
        public List<BomAuditModel> AuditList { get; set; }
    }
}