namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Begindatum is verplicht.")]
        public DateTime Begindatum { get; set; }

        [Required(ErrorMessage = "Einddatum is verplicht.")]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Prijs is verplicht.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "De prijs moet positief zijn.")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Prijs { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Deelnemerslimiet moet minstens 1 zijn.")]
        public int Deelnemerslimiet { get; set; }

        [Required(ErrorMessage = "Bestemming is verplicht.")]
        public int BestemmingId { get; set; }

        public IEnumerable<SelectListItem>? Bestemmingen { get; set; }

        [Required(ErrorMessage = "Selecteer minstens één activiteit.")]
        public List<int> GeselecteerdeActiviteiten { get; set; }

        public IEnumerable<SelectListItem>? Activiteiten { get; set; }
    }
}