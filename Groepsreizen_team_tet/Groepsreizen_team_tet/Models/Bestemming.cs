namespace Groepsreizen_team_tet.Models
{
    public class Bestemming
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Naam { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;
        public int MinLeeftijd { get; set; }
        public int MaxLeeftijd { get; set; }
        public List<Groepsreis> Groepsreizen { get; set; } = new List<Groepsreis>();
        public List<Foto> Fotos { get; set; } = new List<Foto>();
    }
}