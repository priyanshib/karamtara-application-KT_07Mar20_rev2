using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Karamtara_Application.Models
{
    public class ResetPasswordModel
    {
        public string Password { get; set; }
        public string confirmPassword { get; set; }
        public string ResetPasswordCode { get; set; }
        public string Receiver { get; set; }
        public int UserId { get; set; }
    }
}