using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class BOMListModel
    {
        public int EnquiryId { get; set; }
        public int BomId { get; set; }
        public int RevisionNo { get; set; }
        public bool IsPublished { get; set; }
        public int UserId { get; set; }
        public List<BOMListModel> BomList { get; set; }
        public List<ProjectEnquiryModel> DataList{ get; set; }
        public List<ProjectEnquiryModel> InnerDataList { get; set; }
    }
}