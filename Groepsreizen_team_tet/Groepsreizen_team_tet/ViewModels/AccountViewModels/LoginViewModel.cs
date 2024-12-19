using System.ComponentModel.DataAnnotations;

namespace Groepsreizen_team_tet.ViewModels.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is verplicht.")]
        [EmailAddress(ErrorMessage = "Vul een geldig emailadres in.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Wachtwoord is verplicht.")]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = default!;

        [Display(Name = "Onthoud mij?")]

        public string? ReturnUrl { get; set; }
    }
}
