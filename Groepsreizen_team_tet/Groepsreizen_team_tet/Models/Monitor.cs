namespace Groepsreizen_team_tet.Models
{
    public class Monitor
    {
        public int Id { get; set; }
        public int PersoonId { get; set; }
        public int? GroepsreisDetailsId { get; set; }
        public int? IsHoofdMonitor { get; set; }
        public CustomUser Persoon { get; set; } = default!;
        public Groepsreis GroepsReis { get; set; } = default!;
        public List<Opleiding> Opleidingen { get; set; } = new List<Opleiding>();
    }
}
