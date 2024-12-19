namespace Groepsreizen_team_tet.ViewModels.MonitorViewModels
{
    public class MonitorDetailsViewModel
    {
        // Basisgegevens
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Voornaam { get; set; } = default!;
        public string Naam { get; set; } = default!;
        public string Straat { get; set; } = default!;
        public string Huisnummer { get; set; } = default!;
        public string Gemeente { get; set; } = default!;
        public string Postcode { get; set; } = default!;
        public DateTime Geboortedatum { get; set; }
        public string Telefoonnummer { get; set; } = default!;
        public string Huisdokter { get; set; } = default!;
        public string? ContractNummer { get; set; }
        public string? RekeningNummer { get; set; }
        public bool IsActief { get; set; }

        // Rolgerelateerde gegevens
        public bool IsHoofdMonitor { get; set; }

        // Gerelateerde entiteiten
        public List<GroepsreisDetails> Groepsreizen { get; set; } = new List<GroepsreisDetails>();
        public List<OpleidingDetails> Opleidingen { get; set; } = new List<OpleidingDetails>();
    }
    public class GroepsreisDetails
    {
        public string Naam { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public bool WasHoofdMonitor { get; set; }
    }

    public class OpleidingDetails
    {
        public string Naam { get; set; } = string.Empty;
        public string Beschrijving { get; set; } = string.Empty;
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public string Locatie { get; set; } = string.Empty;
    }
}
