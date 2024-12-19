namespace Groepsreizen_team_tet.ViewModels.DeelnemerViewModels
{
    public class DeelnemerViewModel
    {
        public int Id { get; set; }
        public string NaamKind { get; set; }
        public string VoornaamKind { get; set; }
        public string EmailOuder { get; set; } // Parent's email address
        public string GroepsreisNaam { get; set; }
        public string? Opmerkingen { get; set; }
        public List<Deelnemer> Deelnemers { get; set; } = default!;
    }
}
