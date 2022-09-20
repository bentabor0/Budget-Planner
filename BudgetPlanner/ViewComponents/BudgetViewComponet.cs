using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using BudgetPlanner.Services;
using BudgetPlanner.DataAccess.Data;
using BudgetPlanner.DataAccess.Models;

namespace BudgetPlanner.ViewComponents
{
    public class BudgetViewComponent : ViewComponent
    {
        private readonly DataService _dataService;

        public BudgetViewComponent(BudgetDataContext dataContext)
        {
            _dataService = new DataService(dataContext);
        }

        public IViewComponentResult Invoke()
        {
            if (User.Identity.IsAuthenticated)
            {
                User user = _dataService.GetUser(Request.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value);

                if (user.Budgets.FirstOrDefault(x => !x.IsArchived).Purchases != null)
                {
                    foreach (Purchase p in user.Budgets.FirstOrDefault(x => !x.IsArchived).Purchases)
                    {
                        user.Budgets.FirstOrDefault(x => !x.IsArchived).TotalSpent += p.Amount;
                    }
                }

                user.Budgets.FirstOrDefault(x => !x.IsArchived).AmountLeft = (user.Budgets.FirstOrDefault(x => !x.IsArchived).BudgetGoal - user.Budgets.FirstOrDefault(x => !x.IsArchived).TotalSpent);

                return View(user.Budgets.FirstOrDefault(x => !x.IsArchived));
            }

            return View();
        }
    }
}