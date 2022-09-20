using System.Buffers.Text;
using System.Reflection.Metadata;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using BudgetPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Services;
using BudgetPlanner.DataAccess.Data;
using BudgetPlanner.Model;
using Microsoft.Extensions.Logging;

namespace BudgetPlanner.Controllers
{
    [Authorize]
    [Route("")]
    public class AccountController : Controller
    {
        private readonly DataService _dataService;

        private readonly UspsService _uspsService;

        private readonly ILogger<AccountController> _log;

        public AccountController(BudgetDataContext dataContext, UspsService uspsService, ILogger<AccountController> log)
        {
            // Instantiate an instance of the data service.
            _dataService = new DataService(dataContext);
            _uspsService = uspsService;
            _log = log;
        }

        [AllowAnonymous]
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            User existingUser = _dataService.GetUser(registerViewModel.EmailAddress);
            if (existingUser != null)
            {
                // Set email address already in use error message.
                ModelState.AddModelError("Error", "An account already exists with that email address.");

                return View();
            }

            PasswordHasher<string> passwordHasher = new PasswordHasher<string>();

            User user = new User()
            {
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                EmailAddress = registerViewModel.EmailAddress,
                PasswordHash = passwordHasher.HashPassword(null, registerViewModel.Password)
            };

            _dataService.AddUser(user);

            _log.LogInformation($"The {registerViewModel.EmailAddress} user has registered.");

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet("sign-in")]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            User user = _dataService.GetUser(loginViewModel.EmailAddress);

            if (user == null)
            {
                // Set email address not registered error message.
                ModelState.AddModelError("Error", "An account does not exist with that email address.");

                return View();
            }

            PasswordHasher<string> passwordHasher = new PasswordHasher<string>();
            PasswordVerificationResult passwordVerificationResult =
                passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginViewModel.Password);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                // Set invalid password error message.
                ModelState.AddModelError("Error", "Invalid password.");

                    _log.LogInformation($"Invalid login for {loginViewModel.EmailAddress} ({user.ID}).");


                return View();
            }

            _log.LogInformation($"User logged in: {loginViewModel.EmailAddress} ({user.ID}).");


            // Add the user's ID (NameIdentifier), first name and role to the claims that will be put in the cookie.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim(ClaimTypes.Email, user.EmailAddress)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties { };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("UserView", "Account");
            }
            else
            {
                return Redirect(returnUrl);
            }
        }

        [HttpGet("sign-out")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("UserView");
        }

        [Route("")]
        public IActionResult UserView(string sort, string filter)
        {
            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);

            BudgetModel budget = null;

            if (user.Budgets != null)
            {
                budget = user.Budgets.FirstOrDefault(x => !x.IsArchived);
            }

            // Check if the user has a budget.
            if (budget != null)
            {
                _dataService.CalculateBudget(budget);

                if (!String.IsNullOrEmpty(filter))
                {
                    budget.Purchases = budget.Purchases
                        .Where(x => x.Description.ToLower().Contains(filter.ToLower()))
                        .ToList();
                }

                switch (sort)
                {
                    case "description_desc":
                        budget.Purchases = budget.Purchases.OrderByDescending(x => x.Description).ToList();
                        break;
                    case "description_asc":
                        budget.Purchases = budget.Purchases.OrderBy(x => x.Description).ToList();
                        break;
                    case "price_asc":
                        budget.Purchases = budget.Purchases.OrderBy(x => x.Amount).ToList();
                        break;
                    case "price_desc":
                        budget.Purchases = budget.Purchases.OrderByDescending(x => x.Amount).ToList();
                        break;
                    case "date_asc":
                        budget.Purchases = budget.Purchases.OrderBy(x => x.Date).ToList();
                        break;
                    case "date_desc":
                        budget.Purchases = budget.Purchases.OrderByDescending(x => x.Date).ToList();
                        break;
                    default:
                        budget.Purchases = budget.Purchases.OrderByDescending(x => x.Date).ToList();
                        break;
                }

                ViewData["DescriptionSortParm"] = sort == "description_asc" ? "description_desc" : "description_asc";
                ViewData["PriceSortParm"] = sort == "price_asc" ? "price_desc" : "price_asc";
                ViewData["DateSortParm"] = sort == "date_asc" ? "date_desc" : "date_asc";

                ViewData["Filter"] = filter;
            }
            // Prompt user to create budget if they dont have one.
            else
            {
                return RedirectToAction(nameof(AddBudget));
            }

            return View(user);
        }

        [HttpGet("addBudget")]
        public IActionResult AddBudget()
        {
            return View();
        }

        [HttpPost("addBudget")]
        public IActionResult AddBudget(BudgetModel budget)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }
            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);
            budget.UserID = user.ID;

            _dataService.AddBudget(budget);

            return RedirectToAction(nameof(UserView));
        }

        [HttpGet("purchases")]
        public IActionResult Purchases(int? pageNumber)
        {
            const int pageSize = 5;

            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);

            List<Purchase> purchases = user.Budgets.FirstOrDefault(x => !x.IsArchived).Purchases;

            return View(PaginatedList<Purchase>.Create(purchases, pageNumber ?? 1, pageSize));
        }

        [HttpGet("OldPurchases")]
        public IActionResult OldPurchases(int? pageNumber)
        {
            const int pageSize = 5;

            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);

            List<Purchase> purchases = _dataService.GetOldPurchases(user.ID);

            return View(PaginatedList<Purchase>.Create(purchases, pageNumber ?? 1, pageSize));
        }

        [HttpGet("purchase/{id:int}")]
        public IActionResult Purchase([FromRoute] int id)
        {
            Purchase model = _dataService.GetPurchase(id);

            return View(model);
        }

        [HttpGet("AddPurchase")]
        public IActionResult AddPurchase()
        {
            return View();
        }

        [HttpPost("addPurchase")]
        public IActionResult AddPurchase(Purchase purchase)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);

            purchase.BudgetID = user.Budgets.FirstOrDefault(x => !x.IsArchived).ID;

            user.Budgets.FirstOrDefault(x => !x.IsArchived).Purchases.Add(purchase);

            _dataService.AddPurchase(purchase);

            return RedirectToAction(nameof(UserView));
        }

        [HttpGet, Route("edit-purchase/{id:int}")]
        public IActionResult EditPurchase(int id)
        {
            Purchase purchase = _dataService.GetPurchase(id);
            return View(purchase);
        }

        [HttpPost, Route("edit-purchase/{id:int}")]
        public IActionResult EditPurchase(Purchase purchase)
        {
            if (purchase.Amount <= 0)
            {
                return View();
            }

            if (purchase.Description is null)
            {
                return View();
            }

            _dataService.UpdatePurchase(purchase);

            return RedirectToAction(nameof(UserView));
        }

        [HttpGet("edit-budget/{id:int}")]
        public IActionResult EditBudget([FromRoute] int id)
        {
            BudgetModel budget = _dataService.GetBudget(id);

            if (budget != null)
            {
                return View(budget);
            }
            else
            {
                return View();
            }
        }

        [HttpPost("edit-budget/{id:int}")]
        public IActionResult EditBudget(BudgetModel budget)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);
            budget.UserID = user.ID;
            _dataService.UpdateBudget(budget);
            return RedirectToAction(nameof(UserView));
        }

        [Route("ViewAllBudgets")]
        public IActionResult ViewAllBudgets()
        {
            User user = _dataService.GetUser(User.FindFirst(ClaimTypes.Email)?.Value);

            List<BudgetModel> archivedBudgets = _dataService.GetDeletedBudgets(user.ID);

            return View(archivedBudgets);
        }

        [HttpGet("ViewPastBudget/{id:int}")]
        public IActionResult ViewPastBudget(int id)
        {
            BudgetModel budget = _dataService.GetBudget(id);

            if (budget.Purchases != null)
            {
                foreach (Purchase p in budget.Purchases)
                {
                    budget.TotalSpent += p.Amount;
                }
            }

            budget.AmountLeft = (budget.BudgetGoal - budget.TotalSpent);

            return View(budget);
        }

        [HttpGet("profile")]
        public IActionResult Profile()
        {
            // Get currently logged in user ID from the auth cookie.
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get user.
            User u = _dataService.GetUser(userId);

            return View(u);
        }

        [HttpGet("edit-profile")]
        public IActionResult EditProfile()
        {
            // Get user id.
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Get user.
            User u = _dataService.GetUser(userId);

            // Populate view model.
            EditProfileViewModel vm = new EditProfileViewModel()
            {
                EmailAddress = u.EmailAddress,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Address = u.Address,
                Address2 = u.Address2,
                City = u.City,
                State = u.State,
                Zip = u.Zip
            };

            return View(vm);
        }

        [HttpPost("edit-profile")]
        public IActionResult EditProfile(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Get current user.
            User current = _dataService.GetUser(Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));

            PasswordHasher<string> hasher = new PasswordHasher<string>();

            // Confirm password.
            if (hasher.VerifyHashedPassword(null, current.PasswordHash, vm.OldPassword) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Your password is incorrect.");

                return View(vm);
            }

            // Set user fields.
            // Set user fields.
            current.FirstName = vm.FirstName;
            current.LastName = vm.LastName;
            current.EmailAddress = vm.EmailAddress;
            current.Address = vm.Address;
            current.Address2 = vm.Address2;
            current.City = vm.City;
            current.State = vm.State;
            current.Zip = vm.Zip;

            // Check if we should be updating the password.
            if (!string.IsNullOrEmpty(vm.NewPassword))
            {
                // Hash password.
                current.PasswordHash = hasher.HashPassword(null, vm.NewPassword);
            }

            // Validate address.
            bool addressVerified = _uspsService.ValidateAddress(vm.Address, vm.Address2, vm.City, vm.State, vm.Zip).Result;
            if (!addressVerified)
            {
                ModelState.AddModelError("Address", "The address you entered is invalid.");
                return View(vm);
            }

            // Update.
            _dataService.UpdateUser(current);

            return RedirectToAction(nameof(Profile));
        }
    }
}