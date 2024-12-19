namespace Groepsreizen_team_tet.ViewModels.OnkostenViewModels
{
    public class OnkostenBeheerViewModel
    {
        public int GroepsreisId { get; set; }
        public string GroepsreisNaam { get; set; }

        public List<OnkostenDetailViewModel> OnkostenLijst { get; set; } = new List<OnkostenDetailViewModel>();
        public decimal Totaalkost { get; set; } 
        public decimal Opbrengst { get; set; } // Aantal deelnemers * kostprijs groepsreis
        public decimal OnkostenReis { get; set; } // Totaal van reisgerelateerde kosten
        public decimal BudgetHoofdmonitor { get; set; } // Aantal deelnemers * 25
        public decimal OnkostenHoofdmonitor { get; set; } // Som van hoofdmonitor onkosten
        public decimal OnkostenVerantwoordelijke { get; set; } // Som van verantwoordelijke onkosten

        public decimal Balans => Opbrengst - Totaalkost;
        public decimal BalansHM => BudgetHoofdmonitor - OnkostenHoofdmonitor;

        public bool IsGeannuleerd { get; set; }


        public List<OnkostenDetailViewModel> OnkostenHoofdmonitorLijst { get; set; } = new List<OnkostenDetailViewModel>();
        public List<OnkostenDetailViewModel> OnkostenVerantwoordelijkeLijst { get; set; } = new List<OnkostenDetailViewModel>();
    }
}
