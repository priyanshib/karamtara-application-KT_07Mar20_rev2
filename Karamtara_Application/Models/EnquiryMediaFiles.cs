using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class EnquiryMediaFiles
    {
        public List<HttpPostedFileBase> BOQFiles { get; set; }
        public List<HttpPostedFileBase> ProjectSpecificationFiles { get; set; }
        public List<HttpPostedFileBase> OtherAttachmentFiles { get; set; }
        public HttpPostedFileBase ProjectAttachment { get; set; }
    }
}