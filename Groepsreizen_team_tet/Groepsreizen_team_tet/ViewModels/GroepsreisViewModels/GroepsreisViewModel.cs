namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisViewModel
    {
        public List<Groepsreis> Groepsreizen { get; set; } = new List<Groepsreis>();

        public int Id { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime? Einddatum { get; set; }
        public decimal Prijs { get; set; }
        public string? Code { get; set; }
        public string? BestemmingNaam { get; set; }
        

        // Filter criteria
        public DateTime? StartDatum { get; set; }
        public DateTime? EindDatum { get; set; }
        public int? MinLeeftijd { get; set; }
        public int? MaxLeeftijd { get; set; }
        public int LeeftijdCategorie { get; set; }
    }
}
