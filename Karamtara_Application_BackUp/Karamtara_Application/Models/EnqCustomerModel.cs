using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Karamtara_Application.Models
{
    public class CustomerEnquiryModel
    {
        public int EnquiryId { get; set; }
        public int ProjectId { get; set; }
        public string EpCCustomerName { get; set; }
        [DataType(DataType.Date)]
        public DateTime EnquiryDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        public string EnquiryStatus { get; set; }
        public string BoqFileName { get; set; }
        public HttpPostedFileBase BoqFile { get; set; }
        public bool IsBoqFileChanged { get; set; }
        public bool IsBoqFileRemoved { get; set; }
        public string ProjectSpecFileName { get; set; }
        public HttpPostedFileBase ProjectSpecFile { get; set; }
        public bool IsProSpecChanged { get; set; }
        public bool IsProSpecRemoved { get; set; }
        public string OtherFileName { get; set; }
        public HttpPostedFileBase OtherFile { get; set; }
        public bool IsOtherFileChanged { get; set; }
        public bool IsOtherFileRemoved { get; set; }
        //public string TechnicalQuery { get; set; }
        public List<TechnicalQueryModel> TechnicalQuery { get; set; }
        public string Country { get; set; }
        public int CountryId { get; set; }
        public bool CanCreateBOM { get; set; }
        public bool IsPublished { get; set; }
    }
}