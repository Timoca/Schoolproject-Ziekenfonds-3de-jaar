namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisMonitorenViewModel
    {
        public int GroepsreisId { get; set; }
        public string? BestemmingNaam { get; set; } = default!;
        public List<Models.Monitor> HuidigeMonitoren { get; set; } = new List<Models.Monitor>();
        public IEnumerable<SelectListItem>? BeschikbareMonitoren { get; set; } = default!;
        public IEnumerable<SelectListItem>? BeschikbareHoofdmonitoren { get; set; } = default!;
        public List<int> GeselecteerdeMonitorenIds { get; set; } = new List<int>();
        public int? GeselecteerdeHoofdmonitorId { get; set; }
        public bool IsGeannuleerd { get; set; }
        public List<MonitorViewModel> IngeschrevenMonitoren { get; set; } = new List<MonitorViewModel>();
    }
}