using Groepsreizen_team_tet.Validatie;

namespace Groepsreizen_team_tet.ViewModels.AccountViewModels
{
    public class AccountViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Naam is verplicht.")]
        [StringLength(100, ErrorMessage = "Naam mag maximaal 100 tekens lang zijn.")]
        [Display(Name = "Naam")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        [StringLength(100, ErrorMessage = "Voornaam mag maximaal 100 tekens lang zijn.")]
        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = default!;

        [Required(ErrorMessage = "Straat is verplicht.")]
        [StringLength(200, ErrorMessage = "Straat mag maximaal 200 tekens lang zijn.")]
        [Display(Name = "Straat")]
        public string Straat { get; set; } = default!;

        [Required(ErrorMessage = "Huisnummer is verplicht.")]
        [RegularExpression(@"^\d+[a-zA-Z]?$", ErrorMessage = "Voer een geldig huisnummer in.")]
        [Display(Name = "Huisnummer")]
        public string Huisnummer { get; set; } = default!;

        [Required(ErrorMessage = "Gemeente is verplicht.")]
        [StringLength(100, ErrorMessage = "Gemeente mag maximaal 100 tekens lang zijn.")]
        [Display(Name = "Gemeente")]
        public string Gemeente { get; set; } = default!;

        [Required(ErrorMessage = "Postcode is verplicht.")]
        [RegularExpression(@"^\d{4}\s?$", ErrorMessage = "Voer een geldige Belgische postcode in (bijv. 1234).")]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; } = default!;

        [Required(ErrorMessage = "Telefoonnummer is verplicht.")]
        [TelefoonnummerValidatie(9, 10)]
        [Display(Name = "Telefoonnummer")]
        public string Telefoonnummer { get; set; } = default!;

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        [DataType(DataType.Date)]
        [Display(Name = "Geboortedatum")]
        public DateTime Geboortedatum { get; set; }

        [Required(ErrorMessage = "Huisdokter is verplicht.")]
        [StringLength(100, ErrorMessage = "Huisdokter mag maximaal 100 tekens lang zijn.")]
        [Display(Name = "Huisdokter")]
        public string Huisdokter { get; set; } = default!;

        [Display(Name = "Contractnummer")]
        [StringLength(50, ErrorMessage = "Contractnummer mag maximaal 50 tekens lang zijn.")]
        public string? ContractNummer { get; set; }

        [Display(Name = "Rekeningnummer")]
        [RegularExpression(@"^[Bb][Ee]\d{14}$", ErrorMessage = "Voer een geldig Belgisch IBAN in (bijv. BE68539007547034).")]
        public string? RekeningNummer { get; set; }
    }
}
