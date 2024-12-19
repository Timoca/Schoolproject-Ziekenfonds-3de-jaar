namespace Groepsreizen_team_tet.Models
{
    public class Groepsreis
    {
        public int Id { get; set; }
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public decimal Prijs { get; set; }
        public bool OnkostenIngegeven { get; set; }
        public int Deelnemerslimiet { get; set; }

        public bool IsGeannuleerd { get; set; }
        public string? RedenAnnulatie { get; set; } 

        // Verwijzing naar één bestemming
        public int BestemmingId { get; set; }
        public Bestemming Bestemming { get; set; } = default!;

        // Deze hebben we nodig om te zien of we met een kopie of met de originele groepsreis te maken hebben.
        public bool IsKopie { get; set; } = false;
        public int? OrigineleGroepsreisId { get; set; }
        public Groepsreis OrigineleGroepsreis { get; set; } = default!;


        public List<Programma> Programmas { get; set; } = new List<Programma>();
        public List<Monitor> Monitoren { get; set; } = new List<Monitor>();
        public List<Deelnemer> Deelnemers { get; set; } = new List<Deelnemer>();
        public List<Onkosten> OnkostenLijst { get; set; } = new List<Onkosten>();
        public List<Wachtlijst> Wachtlijst { get; set; } = new List<Wachtlijst>();

    }
}
