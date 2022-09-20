using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BudgetPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BudgetPlanner.DataAccess.Data;
using BudgetPlanner.Services;
using BudgetPlanner.DataAccess.Models;
using System.Security.Claims;

namespace BudgetPlanner.Controllers
{
    [Route("")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppConfig _appConfig;

        private readonly DataService _dataService;

        public HomeController(ILogger<HomeController> logger, IOptions<AppConfig> appConfig, BudgetDataContext budgetDataContext)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
            _dataService = new DataService(budgetDataContext);
        }

        [Route("Index")]
        [AllowAnonymous]
        public IActionResult Index(User user,string sort, string filter)
        {
            if (user.Budgets != null){
            return View();
            }
            else{
               return View("Login");
            }
        }

        [Route("about")]
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }
    }
}