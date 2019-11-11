using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>
            {
                new Employee { Id=1, Name="Mary", Department= Dept.HR, Email="mary@pragim.com" },
                new Employee { Id=2, Name="John", Department= Dept.IT, Email="john@pragim.com" },
                new Employee { Id=3, Name="Sam", Department= Dept.IT, Email="sam@pragim.com" }
            };
        }

        public Employee AddEmployee(Employee employee)
        {
            int id = _employeeList.Max(e => e.Id) + 1;
            employee.Id = id;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee deletedEmployee = _employeeList.FirstOrDefault(e => e.Id == id);
            if (deletedEmployee != null)
            {
                _employeeList.Remove(deletedEmployee);
            }
            return deletedEmployee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == id);
        }

        public Employee Update(Employee employeeToUpdate)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeeToUpdate.Id);
            if(employee != null)
            { 
                employee.Name = employeeToUpdate.Name;
                employee.Email = employeeToUpdate.Email;
                employee.Department = employeeToUpdate.Department;
            }
            return employee;
        }
    }
}
