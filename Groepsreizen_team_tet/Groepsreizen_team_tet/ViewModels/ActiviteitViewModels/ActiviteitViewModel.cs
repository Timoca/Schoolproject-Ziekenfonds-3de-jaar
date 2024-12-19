namespace Groepsreizen_team_tet.ViewModels.ActiviteitViewModels
{
    public class ActiviteitViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;
        public List<ActiviteitViewModel> Activiteiten { get; set; } = default!;
        public List<GroepsreisViewModel> Groepsreizen { get; set; } = default!;
    }
}