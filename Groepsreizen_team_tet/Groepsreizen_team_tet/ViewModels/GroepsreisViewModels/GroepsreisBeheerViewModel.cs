namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisBeheerViewModel
    {
        public List<GroepsreisBeheerItemViewModel> Groepsreizen { get; set; } = default!;
        public bool ToonGearchiveerd { get; set; }
    }

    public class GroepsreisBeheerItemViewModel
    {
        public int Id { get; set; }
        public string BestemmingNaam { get; set; } = default!;
        public string BestemmingCode { get; set; } = default!;
        public decimal Prijs { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public int MinLeeftijd { get; set; }
        public int MaxLeeftijd { get; set; }
        public int Deelnemerslimiet { get; set; }
        public int AantalDeelnemers { get; set; }
        public int AantalWachtlijst { get; set; }
        public bool OnkostenIngegeven { get; set; }
        public bool IsKopie { get; set; }
        public bool IsGeannuleerd { get; set; }
    }
}
