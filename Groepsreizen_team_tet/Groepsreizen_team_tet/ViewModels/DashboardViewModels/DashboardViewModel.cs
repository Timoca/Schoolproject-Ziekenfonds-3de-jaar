namespace Groepsreizen_team_tet.ViewModels.DashboardViewModels
{
    public class DashboardViewModel
    {
        public List<OpleidingIndexViewModel> IngeschrevenOpleidingen { get; set; } = new List<OpleidingIndexViewModel>();

        public List<string> MeldingenDoorVerantwoordelijke { get; set; } = new List<string>(); //lijst van alle opleidingen waarvoor verantwoordelijke een monitor inschrijft

        public List<GroepsreisForReviewViewModel> ReviewGroepsreizen { get; set; } = new List<GroepsreisForReviewViewModel>();
    }

    public class GroepsreisForReviewViewModel
    {
        public int GroepsreisId { get; set; }
        public string Naam { get; set; } = default!;
        public DateTime Einddatum { get; set; }
    }
}
