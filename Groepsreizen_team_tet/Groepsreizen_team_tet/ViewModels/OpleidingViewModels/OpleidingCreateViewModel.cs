namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingCreateViewModel
    {
        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; } = default!;

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        [DataType(DataType.Date)]
        [ToekomstigeDatum(ErrorMessage = "Begindatum moet vandaag of in de toekomst liggen.")]
        public DateTime Begindatum { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        [DataType(DataType.Date)]
        [EinddatumNaBegindatum(ErrorMessage = "Einddatum moet na of gelijk aan de begindatum liggen.")]
        [ToekomstigeDatum(ErrorMessage = "Einddatum moet in de toekomst liggen.")]
        public DateTime Einddatum { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Aantal plaatsen is verplicht.")]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal plaatsen moet minimaal 1 zijn.")]
        public int AantalPlaatsen { get; set; } = 1;

        [Required(ErrorMessage = "Locatie is verplicht.")]
        public string Locatie { get; set; } = default!;

        public IFormFile? Afbeelding { get; set; }


        public int? OpleidingVereistId { get; set; }
        public List<Opleiding> OpleidingVereisten { get; set; } = new List<Opleiding>();

        
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
                if (instance != null && instance.Begindatum != null && einddatum >= instance.Begindatum)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult(ErrorMessage ?? "De einddatum moet na of gelijk aan de begindatum liggen.");
            }
            return new ValidationResult("Ongeldige datum");
        }
    }

}
