using Groepsreizen_team_tet.Validatie;

namespace Groepsreizen_team_tet.ViewModels.OnkostenViewModels
{
    public class OnkostenCreateViewModel
    {
        public int GroepsreisId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Datum { get; set; } = DateTime.Now; //standaard altijd huidige datum

        [Required(ErrorMessage ="Geef de onkost een gepaste naam.")]
        public string Titel { get; set; }

        [Required(ErrorMessage = "Geef een korte beschrijving van de onkost.")]
        public string Omschrijving { get; set; }

        [RequiredIfHoofdmonitor(ErrorMessage = "Geef de locatie in.")] // Alleen verplicht voor hoofdmonitor
        public string? Locatie { get; set; }

        [Required(ErrorMessage = "Geef het bedrag in.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Het bedrag moet groter zijn dan 0.")]
        [DataType(DataType.Currency)]
        public decimal Bedrag { get; set; }

        [RequiredIfHoofdmonitor(ErrorMessage = "Upload een betaalbewijs.")]
        public IFormFile? Betaalbewijs { get; set; } // Voor het uploaden van een foto
    }
}
