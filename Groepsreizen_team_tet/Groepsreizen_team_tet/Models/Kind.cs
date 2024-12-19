namespace Groepsreizen_team_tet.Models
{
    public class Kind
    {
        public int Id { get; set; }
        public int PersoonId { get; set; }
        public string Naam { get; set; } = default!;
        public string Voornaam { get; set; } = default!;
        public DateTime Geboortedatum { get; set; }
        public string Allergieën { get; set; } = default!;
        public string Medicatie { get; set; } = default!;


        public CustomUser Ouder { get; set; } = default!;
        public List<Deelnemer> Deelnemers { get; set; } = default!;
        public List<Wachtlijst> Wachtlijst { get; set; } = default!;
    }
}
