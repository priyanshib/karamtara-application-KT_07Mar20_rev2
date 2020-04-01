using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class ProjectEnquiryModel
    {
        public int ProjectId { get; set; }
        public int EnquiryId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNo { get; set; }
        public string EndCustomerName { get; set; }
        public DateTime ProjectDueDate { get; set; }
        public DateTime ProjectCreateDate { get; set; }
        public DateTime EnquiryDate { get; set; }
        public DateTime EnquiryDuteDate { get; set; }
        public string EnquiryType { get; set; }
        public string ProjectStatus { get; set; }
        public string EnquiryStatus { get; set; }
        public string EpcCustomerName { get; set; }
        public bool IsPublished { get; set; }
        public int BomId { get; set; }
        public int RevNo { get; set; }
        public string Country { get; set; }
        public bool IsLatestRevision { get; set; }
        public string TNumber { get; set; }
        public string BOMSource { get; set; }

    }
}