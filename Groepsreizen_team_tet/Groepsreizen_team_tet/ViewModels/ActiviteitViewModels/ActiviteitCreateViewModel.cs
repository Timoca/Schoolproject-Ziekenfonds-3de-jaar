namespace Groepsreizen_team_tet.ViewModels.ActiviteitViewModels
{
    public class ActiviteitCreateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage ="Een naam is verplicht")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage ="Een beschrijving is verplicht")]
        public string Beschrijving { get; set; } = default!;

        // Voeg de lijst van geselecteerde activiteiten toe
        public List<int> GeselecteerdeActiviteiten { get; set; } = new List<int>();

        // Voeg de SelectList eigenschappen toe voor dropdowns
        public SelectList? Activiteiten { get; set; } = default!;
        public SelectList? Bestemmingen { get; set; } = default!;
    }
}