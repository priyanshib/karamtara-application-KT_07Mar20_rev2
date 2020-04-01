using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Karamtara_Application.Models
{
    public class EditUserDetails
    {
        public int UserId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password should consist between 8-20 characters")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and confirm password must match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Reset Password")]
        public bool PassCheckbox { get; set; }

        public DateTime? DOB { get; set; }
                
        public string Salutation { get; set; }
    }    
}