using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class TenderListModel
    {
        public int Id { get; set; }
        public int BomId { get; set; }
        public int BomRevisionId { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionId { get; set; }
        public int EnquiryId { get; set; }
        public List<ProjectEnquiryModel> ProjectList { get; set; }
        public List<TenderEnquiryModel> CustomerList { get; set; }
        public List<TndCompareModel> TndCompareList { get; set; }
    }


    public class TenderEnquiryModel
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public int ProjectId { get; set; }
        public string CustomerName { get; set; }
        public DateTime EnquiryDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public int TenderId { get; set; }
        public int TenderRevisionId { get; set; }
        public int BomId { get; set; }
        public int BomRevisionId { get; set; }
        public int MaxTndRevNo { get; set; }
        public int TenderType { get; set; }
        public string TenderTypeName { get; set; }
    }
    public class TndCompareModel
    {
        public int TenderId { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public string TenderName { get; set; }
        public int BomId { get; set; }
        public int BomRevisionNo { get; set; }
        public int TenderRevNo { get; set; }
        public int TenderType { get; set; }
    }
}