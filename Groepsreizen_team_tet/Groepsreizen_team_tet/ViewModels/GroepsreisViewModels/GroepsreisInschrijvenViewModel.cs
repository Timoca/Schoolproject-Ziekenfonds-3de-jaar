namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisInschrijvenViewModel
    {
        public int GroepsreisId { get; set; }
        public string? GroepsreisNaam { get; set; } = default!;
        public string? Opmerkingen { get; set; } = default!;
        public List<KindViewModel> BeschikbareKinderen { get; set; } = new List<KindViewModel>();
        public List<KindViewModel> IngeschrevenKinderen { get; set; } = new List<KindViewModel>();
        public List<int> GeselecteerdeKinderenIds { get; set; } = new List<int>();
        public List<KindViewModel> WachtlijstKinderen { get; set; } = new List<KindViewModel>();
    }
}
