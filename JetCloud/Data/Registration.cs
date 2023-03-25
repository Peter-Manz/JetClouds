using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace JetCloud.Data
{
    public class Registration
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, StringLength(50, ErrorMessage = "The {0} must be at least {2} and at MAX {1} characters long.", MinimumLength = 3), Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at MAX {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }
    }
}
