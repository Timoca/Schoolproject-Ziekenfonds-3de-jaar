namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; } = default!;

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        [DataType(DataType.Date)]
        [ToekomstigeDatum(ErrorMessage = "Begindatum moet in de toekomst liggen.")]
        public DateTime Begindatum { get; set; }

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        [DataType(DataType.Date)]
        [EinddatumNaBegindatum(ErrorMessage = "Einddatum moet na of gelijk aan de begindatum liggen.")]
        [ToekomstigeDatum(ErrorMessage = "Einddatum moet in de toekomst liggen.")]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Aantal plaatsen is verplicht.")]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal plaatsen moet minimaal 1 zijn.")]
        public int AantalPlaatsen { get; set; }

        [Required(ErrorMessage = "Locatie is verplicht.")]
        public string Locatie { get; set; } = default!;

        public IFormFile? Afbeelding { get; set; }

        public int? OpleidingVereistId { get; set; }
        public List<Opleiding> OpleidingVereisten { get; set; } = new List<Opleiding>();
    }
}
