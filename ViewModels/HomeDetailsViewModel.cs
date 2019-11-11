using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModels
{
    public class HomeDetailsViewModel
    {
        public string Title { get; set; }
        public Employee employee { get; set; }
    }
}
