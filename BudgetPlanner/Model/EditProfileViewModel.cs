using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Model
{
    public class EditProfileViewModel
    {
        [Display(Name = "First Name"), Required]
        public string FirstName { get; set; }

        [Display(Name = "Last Name"), Required]
        public string LastName { get; set; }

        [Display(Name = "Email Address"), Required]
        public string EmailAddress { get; set; }

        [Display(Name = "Mailing Address")]
        public string Address { get; set; }

        [Display(Name = "Apt or Suite Number")]
        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        [Required, Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Compare(nameof(ConfirmPassword)), Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm your password.")]
        public string ConfirmPassword { get; set; }
    }
}