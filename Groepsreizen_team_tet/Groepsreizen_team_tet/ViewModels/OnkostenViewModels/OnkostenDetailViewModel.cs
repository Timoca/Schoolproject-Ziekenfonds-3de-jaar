namespace Groepsreizen_team_tet.ViewModels.OnkostenViewModels
{
    public class OnkostenDetailViewModel
    {
        public int Id { get; set; }
        public int GroepsreisId { get; set; }
        public DateTime Datum { get; set; }
        public string Titel { get; set; }
        public string Omschrijving { get; set; }
        public string Locatie { get; set; }
        public decimal Bedrag { get; set; }
        public byte[]? Betaalbewijs { get; set; } // Voor het weergeven van de foto als base64-string (blob)
    }
}
