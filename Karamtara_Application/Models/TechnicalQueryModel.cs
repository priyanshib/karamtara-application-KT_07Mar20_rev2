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
        public string QueryAttachment { get; set; }
        public string ResponseAttachment { get; set; }
        public string QueryFileName { get; set; }
        public string ResponseFileName { get; set; }
        public List<string> CcMailList { get; set; }
        public List<string> ToMailList { get; set; }
        public string UserMailId { get; set; }
        public string AskedBy { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string ReplyBy { get; set; }
        public DateTime QueryDate { get; set; }
        public DateTime ReplyDate { get; set; }
        public string TenderFileNo { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public string ReplierLastName { get; set; }
        public string ReplierDesignation { get; set; }
        public List<TechnicalAnsModel> Answers { get; set; }
    }

    public class TechnicalAnsModel
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public string Answer { get; set; }
        public string ResponseAttachment { get; set; }
        public string ResponseFileName { get; set; }
        public DateTime ReplyDate { get; set; }
        public string ReplyBy { get; set; }
    }
}