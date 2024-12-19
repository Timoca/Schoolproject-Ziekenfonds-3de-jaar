namespace Groepsreizen_team_tet.ViewModels.DeelnemerViewModels
{
    public class DeelnemerBeheerGroepsreisViewModel
    {
        public int Id { get; set; }
        public string KindNaam { get; set; } = default!;
        public string? Opmerkingen { get; set; }
        public int Leeftijd { get; set; }
        public string OuderTelefoonnummer { get; set; } = default!;
        public string? Medicatie { get; set; }
        public string? Allergieën { get; set; }
    }
}
