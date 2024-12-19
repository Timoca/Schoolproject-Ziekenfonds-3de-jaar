namespace Groepsreizen_team_tet.ViewModels.HomeViewModels
{
    public class HomeDetailViewModel
    {
        public int Id { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public decimal Prijs { get; set; }

        // Bestemming informatie
        public string BestemmingNaam { get; set; }
        public string BestemmingBeschrijving { get; set; }
        public int MinLeeftijd { get; set; }
        public int MaxLeeftijd { get; set; }

        // Foto's
        public List<string> FotoBase64Strings { get; set; } = new List<string>();

        // Activiteiten
        public List<ActiviteitDetailViewModel> Activiteiten { get; set; }
    }
}
