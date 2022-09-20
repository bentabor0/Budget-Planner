using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Emit;
using BudgetPlanner.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace BudgetPlanner.DataAccess.Data
{
    public class BudgetDataContext : DbContext
    {
        public BudgetDataContext(DbContextOptions<BudgetDataContext> options)
            : base(options)
        { }

        public DbSet<BudgetModel> Budget { get; set; }

        public DbSet<Purchase> Purchases {get; set;}

        public DbSet<User> Users {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             //Define the relationship between the Purchase and the Budget.
             modelBuilder.Entity<Purchase>()
             .HasOne(x => x.Budget) 
             .WithMany(x => x.Purchases) 
             .IsRequired();

            // Define the Relationship between the Budget and user.
             modelBuilder.Entity<BudgetModel>()
             .HasOne(x => x.User)
             .WithMany(x => x.Budgets)
             .IsRequired();


             //modelBuilder.Entity<User>()
             //.HasOne(x => x.Budget)
             //.WithOne(x => x.User);
            
            //modelBuilder.Entity<User>()
            //.HasForeignKey(x => x.BudgetModelID);
     
             // Specifiy the seed data.
            //modelBuilder.Entity<BudgetModel>().HasData(
                    //new BudgetModel{ ID = 1, TotalSpent = 0, BudgetGoal = 500, AmountLeft = 500, Purchases = null, UserID = 1},
                    //new BudgetModel{ ID = 2, TotalSpent = 0, BudgetGoal = 1000, AmountLeft = 1000, Purchases = null, UserID = 2},
                    //new BudgetModel{ ID = 3, TotalSpent = 0, BudgetGoal = 2500, AmountLeft = 2500, Purchases = null, UserID = 3},
                    //new BudgetModel{ ID = 4, TotalSpent = 0, BudgetGoal = 3500, AmountLeft = 3500, Purchases = null, UserID = 4}
           // );

              // Specifiy the seed data.
           // modelBuilder.Entity<Purchase>().HasData(
                    //new Purchase{ID = 1, BudgetID = 1, Date = DateTime.Now, Amount = 30, Description = "Gas Station"},
                   // new Purchase{ID = 2, BudgetID = 2, Date = DateTime.Now, Amount = 100, Description = "Grocery Store"},
                    //new Purchase{ID = 3, BudgetID = 3, Date = DateTime.Now, Amount = 80, Description = "Amazon"},
                   // new Purchase{ID = 4, BudgetID = 4, Date = DateTime.Now, Amount = 500, Description = "Rent"}
           // );
        }
    }
}