using Groepsreizen_team_tet.Validatie;

namespace Groepsreizen_team_tet.ViewModels.OnkostenViewModels
{
    public class OnkostenEditViewModel
    {
        public int Id { get; set; }
        public int GroepsreisId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; }

        [Required(ErrorMessage = "Geef de onkost een gepaste naam.")]
        public string Titel { get; set; }

        [Required(ErrorMessage = "Geef een korte beschrijving van de onkost.")]
        public string Omschrijving { get; set; }

        [Required(ErrorMessage = "Geef het bedrag in.")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Het bedrag moet groter zijn dan 0.")]
        public decimal Bedrag { get; set; }

        [RequiredIfHoofdmonitor(ErrorMessage = "Geef de locatie in.")] // Alleen verplicht voor hoofdmonitor
        public string? Locatie { get; set; }

        public IFormFile? Betaalbewijs { get; set; } // Voor het opnieuw uploaden van een foto. Hier geen special required omdat enkel de verantwoordelijke een onkost kan wijzigen en die moet geen betaalbewijs uploaden.
    }
}
