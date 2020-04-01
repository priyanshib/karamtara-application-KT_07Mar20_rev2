using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Karamtara_Application.Models
{
    public class EnquiryModel
    {
        public int ProjectId { get; set; }
        [Required]
        [Display(Name = "Tender File Number")]
        public string TenderFileNo { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }
        public string EnquiryType { get; set; }

        public List<DropDown> EnquiryTypeList
        {
            get
            {
                return new List<DropDown>() { new DropDown { Text = "Domestic", Value = 1 }, new DropDown { Text = "International", Value = 2 } };
            }
            set
            {

            }
        }

        public int EnquiryTypeId { get; set; }
        public string EndCustName { get; set; }
        public DateTime EnqDueDate { get; set; }
        public int StatusId { get; set; }
        public string StatusDesc { get; set; }
        public List<CustomerEnquiryModel> CustomerList { get; set; }
        public List<EnquiryModel> EnquiryList { get; set; }
        public string EnquiryAttachmentName { get; set; }
        public HttpPostedFileBase EnquiryAttachment { get; set; }
        public bool IsAttachmentChanged { get; set; }
        public int LineNumber { get; set; }
        public bool IsEdit { get; set; }
        public bool IsPublished { get; set; }
        public List<TechnicalQueryModel> TechQueryList { get; set; }
        public List<CountryModel> CountryList { get; set; }
        public int CountryId { get; set; }
        public bool IsDeletable { get; set; }
        public string Summary { get; set; }
        public DateTime EnqDate { get; set; }
        public FilterData Filter { get; set; }
        public int ColumnId { get; set; }
        public int OrderId { get; set; }
        public string RemovedBoqIds { get; set; }
        public string RemovedPsIds { get; set; }
        public string RemovedOaIds { get; set; }
    }

    public class DropDown
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

}