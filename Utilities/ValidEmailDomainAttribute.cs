using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Utilities
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        public string AllowedDomain { get; set; }
       
        public override bool IsValid(object value)
        {
            string email = (string)value;
            var parts = email.Split('@');
            if(parts[1].ToLower() == AllowedDomain.ToLower())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
