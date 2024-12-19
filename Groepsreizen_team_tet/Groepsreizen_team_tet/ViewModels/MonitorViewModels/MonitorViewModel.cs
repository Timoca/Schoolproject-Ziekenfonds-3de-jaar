namespace Groepsreizen_team_tet.ViewModels.MonitorViewModels
{
    public class MonitorViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Voornaam { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public bool IsHoofdMonitor { get; set; }
        public int Leeftijd { get; set; }
        public string Telefoonnummer { get; set; } = default!;
    }
}