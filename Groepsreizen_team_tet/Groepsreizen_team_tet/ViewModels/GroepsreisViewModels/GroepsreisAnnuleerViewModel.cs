namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisAnnuleerViewModel
    {
        public int Id { get; set; }
        
        public string Naam { get; set; } = default!;

        [DataType(DataType.Date)]
        public DateTime Begindatum { get; set; }

        [DataType(DataType.Date)]
        public DateTime Einddatum { get; set; }

        [Required(ErrorMessage = "Geef een reden voor de annulatie op.")]
        public string RedenAnnulatie { get; set; } = default!;


    }
}
