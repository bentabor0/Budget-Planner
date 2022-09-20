using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.DataAccess.Models
{
    public class Purchase
    {
        // ID of purchse, primary key.
        public int ID { get; set; }

        // Budget ID, forign key.
        public int BudgetID { get; set; }

        // The day the user purchased something.
        public DateTime Date { get; set; }

        [Required]
        [Range(.01, 1000000)]
        [DataType(DataType.Currency)]
        // Amount spent on purchase.
        public decimal Amount { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "The description must be longer than 2 characters.")]
        [DataType(DataType.MultilineText)]
        // Location of the purchase.
        public string Description { get; set; }

        // Naviagtion Perperty. Have a references to the budget.
        public BudgetModel Budget { get; set; }

        [Required]
        // IsArchived(Deleted data).
        public bool IsArchived { get; set; }
    }
}