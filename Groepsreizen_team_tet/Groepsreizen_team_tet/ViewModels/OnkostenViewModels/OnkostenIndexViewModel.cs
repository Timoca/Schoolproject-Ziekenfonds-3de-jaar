namespace Groepsreizen_team_tet.ViewModels.OnkostenViewModels
{
    public class OnkostenIndexViewModel
    {
        public int GroepsreisId { get; set; }
        public string GroepsreisNaam { get; set; }
        public decimal Opbrengst { get; set; }
        public IEnumerable<Onkosten> OnkostenReis { get; set; }
        public IEnumerable<Onkosten> OnkostenHoofdmonitor { get; set; }
        public decimal TotaalOnkosten { get; set; }
        public byte[]? Betaalbewijs { get; set; }

        public decimal BudgetHoofdmonitor { get; set; }
        public decimal ResterendBudget => BudgetHoofdmonitor - TotaalOnkosten;

        public List<OnkostenDetailViewModel> OnkostenLijst { get; set; } = new List<OnkostenDetailViewModel>();

    }
}
