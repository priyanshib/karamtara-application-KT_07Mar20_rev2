using System.Collections.Generic;

namespace Karamtara_Application.Models
{
    public class AssignToModel
    {
        public int ProjectId { get; set; }
        public int EnquiryId { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public string TenderFileNo { get; set; }
        public string EndCustomerName { get; set; }
        public List<UserAssignModel> Users { get; set; }
    }

    public class UserAssignModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public bool IsSelected { get; set; }
    }
}