using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<AdministrationController> Logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            logger = Logger;
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {userId} doesn't exist";
                return View("NotFound");
            }
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel()
            {
                UserId = userId
            };

            foreach(var claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                userClaim.IsSelected = existingUserClaims.Any(ec => ec.Type == claim.Type);

                model.Claims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel vm)
        {
            var user = await userManager.FindByIdAsync(vm.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {vm.UserId} cannot be found";
                return View("NotFound");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);

            var selectedClaims = vm.Claims.Select(c=> new Claim(c.ClaimType, c.IsSelected ? "true" : "false"));
            result = await userManager.AddClaimsAsync(user, selectedClaims);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(vm);
            }

            return RedirectToAction("EditUser", new { Id = vm.UserId });
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            
            var user = await userManager.FindByIdAsync(userId);
            var model = new List<UserRolesViewModel>();

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {userId} doesn't exist";
                return View("NotFound");
            }
            else
            {
                var roles = roleManager.Roles;
                
                foreach (var role in roles)
                {
                    model.Add(new UserRolesViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = await userManager.IsInRoleAsync(user, role.Name)
                    });
                }
            }
           
            return View(model);
        }

        [HttpPost]
        [Authorize (Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user,
                model.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {id} doesn't exist";
                return View("NotFound");
            }
            else
            {
                try
                {
                    var result = await roleManager.DeleteAsync(role);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles", "Administration");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        return View("ListRoles");
                    }
                }
                catch (DbUpdateException ex)
                {
                    logger.LogError(ex.Message);

                    ViewBag.Title = $"{role.Name} role is in use";
                    ViewBag.ErrorMessage = $"{role.Name} cannot be deleted as there are users " +
                        $"If you want to delete this role, please remove the users from " +
                        $"the role and then try to delete.";

                    return View("Error");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {id} doesn't exist";
                return View("NotFound");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);

                if(result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Administration");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("",error.Description);
                    }

                    return View("ListUsers");
                }
            }
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole() { Name = vm.RoleName };
                var result = await roleManager.CreateAsync(new IdentityRole { Name = vm.RoleName });

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if(role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {id} doesn't exist";
                return View("NotFound");
            }

            List<string> users = new List<string>();

            var model = new EditRoleViewModel()
            {
                Id = id,
                RoleName = role.Name,
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.Email);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {id} doesn't exist";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var claims = await userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel()
            {
                Id = id,
                Name = user.UserName,
                Email = user.Email,
                City = user.City,
                Roles = roles.ToList(),
                Claims = claims.Select(c=> c.Type + " : " + c.Value).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel vm)
        {
            var role = await roleManager.FindByIdAsync(vm.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {vm.Id} doesn't exist";
                return View("NotFound");
            }
            else
            {
                role.Name = vm.RoleName;
                var result = await roleManager.UpdateAsync(role);

                if(result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(vm);
            }   
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel vm)
        {
            var user = await userManager.FindByIdAsync(vm.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id {vm.Id} doesn't exist";
                return View("NotFound");
            }
            else
            {
                user.UserName = vm.Name;
                user.Email = vm.Email;
                user.City = vm.City;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {roleId} doesn't exist";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach(var user in userManager.Users)
            {
                var vm = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    vm.isSelected = true;
                }
                else
                {
                    vm.isSelected = false;
                }

                model.Add(vm);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> vm, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id {role.Id} doesn't exist";
                return View("NotFound");
            }

            for(int i=0; i < vm.Count; i++)
            {
                var user = await userManager.FindByIdAsync(vm[i].UserId);
                IdentityResult result = null;

                if(vm[i].isSelected && !(await userManager.IsInRoleAsync(user,role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if(!vm[i].isSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
;
                if (result.Succeeded)
                {
                    if (i < vm.Count - 1)
                        continue;
                    else
                        return RedirectToAction("EditRole", "Administration", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", "Administration", new { Id = roleId });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}