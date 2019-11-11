using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class AddPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Description = "New Password")]
        public string Password { get; set; }

        
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The new and the confirm Password don't match")]
        [Display(Description = "Confirm New Password")]
        public string ConfirmPassword { get; set; }
    }
}
