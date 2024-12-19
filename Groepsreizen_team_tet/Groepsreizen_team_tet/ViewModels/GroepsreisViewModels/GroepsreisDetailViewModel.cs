namespace Groepsreizen_team_tet.ViewModels.GroepsreisViewModels
{
    public class GroepsreisDetailViewModel
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

        public bool IsGeannuleerd { get; set; }
        public string? RedenAnnulatie { get; set; }

        // Foto's
        public List<string> FotoBase64Strings { get; set; } = new List<string>();

        // Activiteiten
        public List<ActiviteitDetailViewModel> Activiteiten { get; set; }

        // Is de reis volzet?
        public bool IsVolzet { get; set; }

        // Nieuwe eigenschap voor de rollen
        public bool IsVerantwoordelijke { get; set; }
        public bool IsHoofdmonitor { get; set; }
        public bool IsMonitor { get; set; }
        public bool IsEenHoofdmonitor { get; set; } //is hoofdmonitor van de groepsreis
        public bool IsHoofdmonitorIngeschreven { get; set; }

    }
}
