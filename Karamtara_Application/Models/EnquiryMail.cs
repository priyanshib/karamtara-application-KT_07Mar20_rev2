using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class EnquiryMail
    {
        public int EnquiryId { get; set; }
        public string ProjectName { get; set; }
        public string EndCustName { get; set; }
        public List<string> CustomerNames { get; set; }
        public List<UserEmail> UserEmails { get; set; }
        public string PublisherName { get; set; }
        public string PublisherDesignation { get; set; }
    }

    public class UserEmail
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string Designation { get; set; }
    }
}