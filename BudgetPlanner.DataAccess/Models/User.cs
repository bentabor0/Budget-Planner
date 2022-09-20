using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetPlanner.DataAccess.Models
{
    public class User
    {
        // ID of User, primary key.
        public int ID {get; set;}

        // Budget ID, forign key. Note: you guys can change this if you want to.
        //public int BudgetID {get; set;}

        [Required]
        // The User's Name.
        public string FirstName {get; set;}

        [Required]
        // The User's Last Name.
        public string LastName {get; set;}

        [Required]
        [DataType(DataType.EmailAddress)]
        // The users Email address.
        public string EmailAddress {get; set;}

        [Display(Name = "Mailing Address")]
        public string Address { get; set; }

        [Display(Name = "Apt or Suite Number")]
        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        // Navigation.
        public List<BudgetModel> Budgets {get; set;}

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        // The user is an admin.
        public bool IsAdmin {get; set;}

        [NotMapped, Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                // Note to Team: both of them will do the same thing.
                // string fullName = FirstName + " " + LastName;
                return $"{FirstName} {LastName}";
            }
        }
    }
}