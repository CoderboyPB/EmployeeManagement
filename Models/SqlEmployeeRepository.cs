using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class SqlEmployeeRepository : IEmployeeRepository
    {
        private AppDbContext _dbContext;
        public SqlEmployeeRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        Employee IEmployeeRepository.AddEmployee(Employee e)
        {
            _dbContext.Employees.Add(e);
            _dbContext.SaveChanges();
            return e;
        }

        Employee IEmployeeRepository.Delete(int id)
        {
            Employee employee = _dbContext.Employees.Find(id);
            if(employee != null)
            {
                _dbContext.Employees.Remove(employee);
                _dbContext.SaveChanges();
            }
            return employee;
        }

        IEnumerable<Employee> IEmployeeRepository.GetAllEmployees()
        {
            return _dbContext.Employees.ToList();
        }

        Employee IEmployeeRepository.GetEmployee(int id)
        {
            return _dbContext.Employees.Find(id);
        }

        Employee IEmployeeRepository.Update(Employee employeeToUpdate)
        {
            var employee = _dbContext.Employees.Attach(employeeToUpdate);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _dbContext.SaveChanges();
            return employeeToUpdate;
        }
    }
}
