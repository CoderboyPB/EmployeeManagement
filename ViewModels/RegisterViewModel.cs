using EmployeeManagement.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Remote("isEmailInUse","Account")]
        [ValidEmailDomain(AllowedDomain = "pragimtech.com", ErrorMessage = "Domain must be pragimtech.com")]
        public string Email { get; set; }
        public string City { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",ErrorMessage = "Password and confirmation password dont match")]
        public string ConfirmPassword { get; set; }
    }
}
