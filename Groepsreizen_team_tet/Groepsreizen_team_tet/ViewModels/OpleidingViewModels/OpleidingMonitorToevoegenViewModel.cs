namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingMonitorToevoegenViewModel
    {
        public int OpleidingId { get; set; }
        public string OpleidingNaam { get; set; } = default!;
        public List<MonitorViewModel> BeschikbareMonitoren { get; set; } = new List<MonitorViewModel>();

        public int GeselecteerdeMonitorId { get; set; }

    }
}
