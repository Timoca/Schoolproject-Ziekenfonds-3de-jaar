namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisDeelnemersViewModel
    {
        public int GroepsreisId { get; set; }
        public string? BestemmingNaam { get; set; } = default!;
        public List<DeelnemerBeheerGroepsreisViewModel> HuidigeDeelnemers { get; set; } = new List<DeelnemerBeheerGroepsreisViewModel>();
        public IEnumerable<SelectListItem>? AlleKinderen { get; set; } = default!;
        public List<int> GeselecteerdeKinderenIds { get; set; } = new List<int>();
        public string? OuderNaamFilter { get; set; }
        public int AantalDeelnemers => HuidigeDeelnemers.Count;
        public int Deelnemerslimiet { get; set; }
        public bool IsGeannuleerd { get; set; }

        public List<GroepsreisDeelnemerViewModel> WachtlijstDeelnemers { get; set; } = new List<GroepsreisDeelnemerViewModel>();
    }

    public class GroepsreisDeelnemerViewModel
    {
        public int Id { get; set; }
        public string KindNaam { get; set; } = default!;
        public string? Opmerkingen { get; set; } = string.Empty;
        public bool IsGeannuleerd { get; set; }
    }
}