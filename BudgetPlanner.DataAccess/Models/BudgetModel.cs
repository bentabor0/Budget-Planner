using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.DataAccess.Models
{
    public class BudgetModel
    {
        // Budget ID (Primary key).
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        // The TotalAmount spent so far in the current budget.
        public decimal TotalSpent { get; set; }

        [Range(.01, 1000000)]
        [DataType(DataType.Currency)]
        // Amount the user hopes to spend this much.
        public decimal BudgetGoal { get; set; }

        [DataType(DataType.Currency)]
        // Amount left in budget until going over goal.
        public decimal AmountLeft { get; set; }

        // List of purchases during the current budget.
        public List<Purchase> Purchases { get; set; }

        // The userID FK Foreign Key
        public int UserID { get; set; }

        [Required]
        // IsArchived(Deleted data).
        public bool IsArchived { get; set; }

        // Represents the navigation Foreign key for user.
        public User User { get; internal set; }
    }
}