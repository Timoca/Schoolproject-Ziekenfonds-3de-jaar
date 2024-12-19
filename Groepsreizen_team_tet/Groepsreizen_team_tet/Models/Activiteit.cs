namespace Groepsreizen_team_tet.Models
{
    public class Activiteit
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;

        public List<Programma> Programmas { get; set; } = new List<Programma>();
    }
}
