using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public bool LoginStatus { get; set; }   
        public List<UserModel> UserList { get; set; }
        public int UserTypeId { get; set; }
        public string UserType { get; set; }
        public List<UserType> UserTypeList { get; set;}
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string SignUpCode { get; set; }
    }

    public class UserType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsActive { get; set; }
    }
}