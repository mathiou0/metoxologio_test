using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using metoxologio_test.Data;
using metoxologio_test.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace metoxologio_test.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly RoleManager<IdentityUserRole<int>> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<ApplicationIdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(ILogger<AccountController> logger, 
            UserManager<ApplicationIdentityUser> userManager,
            SignInManager<ApplicationIdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel user,string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(user.Username, user.Password,true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
               
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View();
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel user, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var identityUser = new ApplicationIdentityUser
                {
                    UserName = user.Username,
                    Email = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address
                };
                var result = await _userManager.CreateAsync(identityUser, user.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        @"\Account\ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = identityUser.Id, code = code },
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = identityUser.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(identityUser, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View();
        }
        
        public IActionResult Users_Read_Js([DataSourceRequest]DataSourceRequest request)
        {
            var users = _userManager.Users.ToList();
            //DataSourceResult result = users.ToDataSourceResult(request);
            var result = new DataSourceResult()
            {
                Data = users,
                Total = users.Count
            };

            return Json(result.Data);
        }

        public IActionResult Users_Read([DataSourceRequest]DataSourceRequest request)
        {
            var users = _userManager.Users.ToList();
            //DataSourceResult result = users.ToDataSourceResult(request);
            var result = new DataSourceResult()
            {
                Data = users,
                Total = users.Count
            };

            return Json(result);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Users_Update([DataSourceRequest]DataSourceRequest request, ApplicationIdentityUser user)
        {
            if (ModelState.IsValid)
            {
                var finduser = await _userManager.FindByIdAsync(user.Id.ToString());

                finduser.FirstName = user.FirstName;
                finduser.LastName = user.LastName;
                finduser.Address = user.Address;
                finduser.Email = user.Email;
                finduser.AccessFailedCount = finduser.AccessFailedCount;
                finduser.ConcurrencyStamp = finduser.ConcurrencyStamp;
                finduser.EmailConfirmed = finduser.EmailConfirmed;
                finduser.LockoutEnabled = finduser.LockoutEnabled;
                finduser.LockoutEnd = finduser.LockoutEnd;
                finduser.NormalizedEmail = finduser.NormalizedEmail;
                finduser.NormalizedUserName = finduser.NormalizedUserName;
                finduser.PasswordHash = finduser.PasswordHash;
                finduser.PhoneNumber = finduser.PasswordHash;
                finduser.PhoneNumberConfirmed = finduser.PhoneNumberConfirmed;
                finduser.SecurityStamp = finduser.SecurityStamp;
                finduser.TwoFactorEnabled = finduser.TwoFactorEnabled;
                finduser.UserName = user.UserName;

                var user_created = await _userManager.UpdateAsync(finduser);
                return Json(new[] { finduser }.ToDataSourceResult(request, ModelState));
            }
            return null;

        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Users_Create([DataSourceRequest]DataSourceRequest request, ApplicationIdentityUser user)
        {
            if (ModelState.IsValid)
            {
                var user_created = await _userManager.CreateAsync(user);
                if (user_created.Succeeded)
                {
                    return Json(new[] { user }.ToDataSourceResult(request, ModelState));
                }

            }
            return null;
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return LocalRedirect("/");
            }
        }
    }
}