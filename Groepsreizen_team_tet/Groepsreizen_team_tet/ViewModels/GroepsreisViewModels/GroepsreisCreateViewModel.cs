namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        [DataType(DataType.Date)]
        [ToekomstigeDatum(ErrorMessage = "Begindatum moet vandaag of in de toekomst liggen.")]
        public DateTime Begindatum { get; set; }

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        [DataType(DataType.Date)]
        [EinddatumNaBegindatum(ErrorMessage = "Einddatum moet na of gelijk aan de begindatum liggen.")]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Prijs is verplicht.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "De prijs moet positief zijn.")]
        [DataType(DataType.Currency)]
        public decimal Prijs { get; set; }

        [Required(ErrorMessage = "Selecteer een bestemming.")]
        public int BestemmingId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Deelnemerslimiet moet minstens 1 zijn.")]
        public int Deelnemerslimiet { get; set; }

        public IEnumerable<SelectListItem>? Bestemmingen { get; set; }

        [Required(ErrorMessage = "Selecteer minstens één activiteit.")]
        public List<int> GeselecteerdeActiviteiten { get; set; }

        public IEnumerable<SelectListItem>? Activiteiten { get; set; }
        public int LeeftijdCategorie { get; set; } = 1;

        public int? Code { get; set; }

        public int MinLeeftijd { get; set; }

        public int MaxLeeftijd { get; set; }

        [Display(Name = "Foto uploaden")]
        public IFormFile? FotoBestand { get; set; }
    }

    public class ToekomstigeDatumAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime >= DateTime.Today)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(ErrorMessage ?? "De datum moet vandaag of in de toekomst liggen.");
            }
            return new ValidationResult("Ongeldige datum");
        }


    }
    public class EinddatumNaBegindatumAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime einddatum)
            {
                var instance = validationContext.ObjectInstance as dynamic;
                if (instance != null && instance.Begindatum != null && einddatum > instance.Begindatum)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(ErrorMessage ?? "De einddatum moet na  begindatum liggen.");
            }
            return new ValidationResult("Ongeldige datum");
        }
    }
}