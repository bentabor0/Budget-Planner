using System.ComponentModel;
using System.Reflection.Metadata;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using BudgetPlanner.DataAccess.Data;
using BudgetPlanner.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Services
{
    public class DataService
    {
        private readonly BudgetDataContext _BudgetDataContext;

        public DataService(BudgetDataContext budgetDataContext)
        {
            _BudgetDataContext = budgetDataContext;
            _BudgetDataContext.Purchases = budgetDataContext.Purchases;
        }

        public Purchase GetPurchase(int ID)
        {
            return _BudgetDataContext.Purchases
            .AsNoTracking()
            .Where(x => x.ID == ID)
            .FirstOrDefault();
        }

        public List<Purchase> GetOldPurchases(int id)
        {
            List<BudgetModel> budgets = GetBudgets(id);

            List<Purchase> purchases = new List<Purchase>();

            foreach (BudgetModel b in budgets)
            {
                if (b.IsArchived)
                {
                    foreach (Purchase p in b.Purchases)
                    {
                        purchases.Add(p);
                    }
                }
            }

            return purchases;
        }

        public List<BudgetModel> GetBudgets(int userID)
        {
            List<BudgetModel> budgets = _BudgetDataContext.Budget
                .AsNoTracking()
                .Where(x => x.UserID == userID)
                .Include(x => x.Purchases)
                .ToList();

            return budgets;
        }

        public BudgetModel GetBudget(int budgetID)
        {
            BudgetModel budget = _BudgetDataContext.Budget
            .AsNoTracking()
            .Where(x => x.ID == budgetID)
            .Include(x => x.Purchases)
            .FirstOrDefault();

            return budget;
        }

        public User GetUser(string emailAddress)
        {
            User user = _BudgetDataContext.Users
            .AsNoTracking()
            .Where(x => x.EmailAddress.ToLower() == emailAddress.ToLower())
            .Include(x => x.Budgets).ThenInclude(x => x.Purchases)
            .FirstOrDefault();

            return user;
        }

        public BudgetModel CalculateBudget(BudgetModel budget)
        {
            foreach (Purchase p in budget.Purchases)
            {
                budget.TotalSpent += p.Amount;
            }

            budget.AmountLeft = (budget.BudgetGoal - budget.TotalSpent);

            return budget;
        }

        public List<BudgetModel> GetDeletedBudgets(int userID)
        {
            return _BudgetDataContext.Budget
            .AsNoTracking()
            .Where(b => b.UserID == userID && b.IsArchived == true)
            .ToList();
        }

        public BudgetModel AddBudget(BudgetModel newBudget)
        {
            BudgetModel oldBudget = _BudgetDataContext.Budget
                .AsNoTracking()
                .Where(b => b.UserID == newBudget.UserID && b.IsArchived == false)
                .Include(x => x.Purchases)
                .FirstOrDefault();

            if (oldBudget != null)
            {
                oldBudget.IsArchived = true;

                foreach (Purchase p in oldBudget.Purchases)
                {
                    p.IsArchived = true;
                }

                UpdateBudget(oldBudget);
            }

            _BudgetDataContext.Budget.Add(newBudget);
            _BudgetDataContext.SaveChanges();
            return newBudget;
        }

        public Purchase AddPurchase(Purchase purchase)
        {
            _BudgetDataContext.Purchases.Add(purchase);
            _BudgetDataContext.SaveChanges();
            return purchase;
        }

        public User AddUser(User user)
        {
            _BudgetDataContext.Users.Add(user);
            _BudgetDataContext.SaveChanges();
            return user;
        }


        public void UpdatePurchase(Purchase purchase)
        {
            _BudgetDataContext.Purchases.Update(purchase);
            _BudgetDataContext.SaveChanges();
        }

        public void UpdateBudget(BudgetModel budget)
        {
            _BudgetDataContext.Budget.Update(budget);
            _BudgetDataContext.SaveChanges();
        }

        // public void UpdateUser(User user)
        // {
        //     user.Budgets.ForEach(x => x.Purchases = null);
        //     _BudgetDataContext.users.Update(user);
        //     _BudgetDataContext.SaveChanges();
        // }

        // This method is never used, Do you think this will be needed in the future?
        //public void RestoreBudget(int budgetId)
        //{
        //    BudgetModel budget = _BudgetDataContext.Budget
        //     .Where(x => x.ID == budgetId)
        //     .FirstOrDefault();
        //
        //    budget.IsArchived = false;
        //
        //    _BudgetDataContext.SaveChanges();
        //}

        // This method is never used, Do you think this will be needed in the future?
        //public void DeleteBudget(int budgetId)
        //{
        //    BudgetModel budget = _BudgetDataContext.Budget
        //     .Where(x => x.ID == budgetId)
        //     .FirstOrDefault();
        //
        //    budget.IsArchived = true;
        //
        //    _BudgetDataContext.SaveChanges();
        //}

        public User GetUser(int id)
        {
            return _BudgetDataContext.Users
                .AsNoTracking()
                .FirstOrDefault(x => x.ID == id);
        }

        public void UpdateUser(User u)
        {
            _BudgetDataContext.Users.Update(u);
            _BudgetDataContext.SaveChanges();
        }
    }
}