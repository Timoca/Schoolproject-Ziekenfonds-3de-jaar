using Groepsreizen_team_tet.Validatie;

namespace Groepsreizen_team_tet.ViewModels.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email is verplicht.")]
        [EmailAddress(ErrorMessage = "Vul een geldig emailadres in.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Wachtwoord is verplicht.")]
        [StringLength(100, ErrorMessage = "Het {0} moet minstens {2} tekens lang zijn.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
            ErrorMessage = "Het wachtwoord moet minstens één hoofdletter, één cijfer en één symbool bevatten.")]
        [DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = default!;

        [Required(ErrorMessage = "Bevestig wachtwoord is verplicht.")]
        [DataType(DataType.Password)]
        [Display(Name = "Bevestig wachtwoord")]
        [Compare("Password", ErrorMessage = "Het wachtwoord en de bevestiging komen niet overeen.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required(ErrorMessage = "Naam is verplicht.")]
        [Display(Name = "Naam")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = default!;

        [Required(ErrorMessage = "Straat is verplicht.")]
        [Display(Name = "Straat")]
        public string Straat { get; set; } = default!;

        [Required(ErrorMessage = "Huisnummer is verplicht.")]
        [Display(Name = "Huisnummer")]
        public string Huisnummer { get; set; } = default!;

        [Required(ErrorMessage = "Gemeente is verplicht.")]
        [Display(Name = "Gemeente")]
        public string Gemeente { get; set; } = default!;

        [Required(ErrorMessage = "Postcode is verplicht.")]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; } = default!;


        //Er staat hieronder 18 omdat de maximale combinatie van tekens en spaties voor een telefoonummer 18 charackters is.
        [Required(ErrorMessage = "Telefoonnummer is verplicht.")]
        [TelefoonnummerValidatie(9, 10)]
        [Display(Name = "Telefoonnummer")]
        public string Telefoonnummer { get; set; } = default!;

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        [DataType(DataType.Date)]
        [Display(Name = "Geboortedatum")]
        public DateTime Geboortedatum { get; set; }

        [Required(ErrorMessage = "Huisdokter is verplicht.")]
        [Display(Name = "Huisdokter")]
        public string Huisdokter { get; set; } = default!;

        [Display(Name = "Contractnummer")]
        public string? ContractNummer { get; set; }
    }
}
