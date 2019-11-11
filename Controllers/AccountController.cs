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
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = vm.Email, 
                    Email = vm.Email, 
                    City = vm.City 
                };

                var result = await userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationLink);

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    ViewBag.ErrorTitle = "Registration successful.";
                    ViewBag.ErrorMessage = "Before you Login, please confirm your email " +
                        "by clicking the link, we send to you";

                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var model = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl)
        {
            vm.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(vm.Email);

                if(user != null && !user.EmailConfirmed && 
                    (await userManager.CheckPasswordAsync(user, vm.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(vm);
                }
               
                var result = await signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Login attempt failed");
            }
            return View(vm);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> isEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUrl = returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult>ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }

            var username = info.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? info.Principal.FindFirstValue(ClaimTypes.Name);
            ApplicationUser user = null;
            user = await userManager.FindByNameAsync(username);

            if(info.LoginProvider == "Twitter")
            {
                // Let them pass
                if(user != null)
                {
                    // User exists, so log him in
                    if ((await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true)).Succeeded)
                    {
                        // return the user to his before visited url
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        ViewBag.ErrorTitle = $"Could not login with: {info.LoginProvider}";
                        ViewBag.ErrorMessage = "Please contact support on Pragim@PragimTech.com";
                        return View("Error");
                    }
                }
                else
                {
                    user = new ApplicationUser
                    {
                        UserName = username,
                        Email = ""
                    };

                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user, info);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    _ = await userManager.ConfirmEmailAsync(user, token);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }
            }
            else
            {
                // Facebook, Google, etc ... 
                if(user != null)
                {
                    if(await userManager.IsEmailConfirmedAsync(user))
                    {
                        if ((await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true)).Succeeded)
                        {
                            // return the user to his before visited url
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            ViewBag.ErrorTitle = $"Could not login with: {info.LoginProvider}";
                            ViewBag.ErrorMessage = "Please contact support on Pragim@PragimTech.com";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,"Email not confirmed yet.");
                        return View("Login", loginViewModel);
                    }
                }
                else
                {
                    user = new ApplicationUser
                    {
                        UserName = username,
                        Email = username
                    };

                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user, info);
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmationLink);

                    ViewBag.ErrorTitle = "Registration successful.";
                    ViewBag.ErrorMessage = "Before you Login, please confirm your email " +
                        "by clicking the link, we send to you";

                    return View("Error");
                }
            } 
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {userId} is invalid.";
                return View("NotFound");
            }

            var result  = await userManager.ConfirmEmailAsync(user, token);

            if (result != null)
                return View();

            ViewBag.ErrorTitle = "Email cannot be confirmed.";
            return View("Error");
        }

        [HttpGet][AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost][AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(vm.Email);

                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var url = Url.Action("ResetPassword", "Account", new { email = vm.Email, token = token }, Request.Scheme);
                    logger.LogWarning(url);
                    return View("ForgotPasswordConfirmation");
                }
                return View("ForgotPasswordConfirmation"); 
            }
            return View(vm);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if(email == null || token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid password reset token");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(vm.Email);
                if(user != null)
                {
                    var result = await userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
                    if (result.Succeeded)
                        return View("ResetPasswordConfirmation");
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(vm);
                    }
                }
                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            var userHasPassword = await userManager.HasPasswordAsync(user);

            if(!userHasPassword)
            {
                return RedirectToAction("AddPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            var user = await userManager.GetUserAsync(User);

            if(user != null)
            {
                return RedirectToAction("Login");
            }
            
            var result = await userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View();
            }
            else
            {
                await signInManager.RefreshSignInAsync(user);
                return View("PasswordChanged");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);

            var userHasPassword = await userManager.HasPasswordAsync(user);
            if(userHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel vm)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                var result = await userManager.AddPasswordAsync(user, vm.Password);

                if(!result.Succeeded)
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("string.empty", error.Description);
                    }
                    return View();
                }

                await signInManager.RefreshSignInAsync(user);

                return View("AddPasswordConfirmation");
            }

            return View();
        }
    }
}