using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository _employeeRepository;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector protector;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly DataProtectionPurposeStrings _dataProtectionPurposeStrings;

        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment,
            IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            _hostingEnvironment = hostingEnvironment;
            _dataProtectionProvider = dataProtectionProvider;
            _dataProtectionPurposeStrings = dataProtectionPurposeStrings;
            protector = _dataProtectionProvider.CreateProtector(_dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees().Select(e =>
            {
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }

        public ViewResult Details(string id)
        {
            string decryptedId = protector.Unprotect(id);
            int employeeId = Convert.ToInt32(decryptedId);

            Employee model = _employeeRepository.GetEmployee(employeeId);

            if (model == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }

            var viewModel = new HomeDetailsViewModel { employee = model, Title = "Details View" };
            return View(viewModel);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if(ModelState.IsValid)
            {
                string uniqueFilename = null;
                if(model.Photo != null)
                {
                    string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    uniqueFilename = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFilename);
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }
                Employee employee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFilename
                };
                _employeeRepository.AddEmployee(employee);
                return RedirectToAction("Details", new { id = employee.Id });
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            var vm = new EmployeeEditViewModel
            {
                Id = id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotopath = employee.PhotoPath
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel vm)
        {
            if(ModelState.IsValid)
            {
                var employee = new Employee
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    Email = vm.Email,
                    Department = vm.Department,
                    PhotoPath = vm.ExistingPhotopath
                };

                if(vm.Photo != null)
                {
                    string webRoothPath = _hostingEnvironment.WebRootPath;
                    string imagePath = Path.Combine(webRoothPath, "images");
                    string imageFileName = Guid.NewGuid().ToString() + "_" + vm.Photo.FileName;
                    string fullImagePath = Path.Combine(imagePath, imageFileName);
                    using(var imageStream = new FileStream(fullImagePath, FileMode.Create))
                    {
                        vm.Photo.CopyTo(imageStream);
                    }
                    employee.PhotoPath = imageFileName;
                }

                _employeeRepository.Update(employee);

                return new RedirectToActionResult("Details", "Home", new { vm.Id });
            }
            return View(vm);
        }
    }
}
