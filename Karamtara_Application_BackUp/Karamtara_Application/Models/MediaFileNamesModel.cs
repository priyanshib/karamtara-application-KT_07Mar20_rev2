using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class MediaFileNamesModel
    {
        public List<string> BoqFileName { get; set; }

        public List<string> ProjectSpecFileNames { get; set; }

        public List<string> OtherFileNames { get; set; }

        public string ProjectAttachmentName { get; set; }
    }
}