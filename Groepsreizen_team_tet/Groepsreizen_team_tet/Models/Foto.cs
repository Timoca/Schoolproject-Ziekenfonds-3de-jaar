namespace Groepsreizen_team_tet.Models
{
    public class Foto
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public byte[]? Afbeelding { get; set; }
        public int BestemmingId { get; set; }

        public Bestemming Bestemming { get; set; } = default!;
    }
}
