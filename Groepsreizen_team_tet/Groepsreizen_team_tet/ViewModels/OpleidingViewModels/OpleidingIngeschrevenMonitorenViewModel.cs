namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingIngeschrevenMonitorenViewModel
    {
        public int OpleidingId { get; set; }
        public string OpleidingNaam { get; set; } = default!;
        public List<MonitorViewModel> IngeschrevenMonitoren { get; set; } = new List<MonitorViewModel>();
    }

   
}
