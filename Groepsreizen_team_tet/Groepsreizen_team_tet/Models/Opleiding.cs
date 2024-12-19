using System.Threading;

namespace Groepsreizen_team_tet.Models
{
    public class Opleiding
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public int AantalPlaatsen { get; set; }
        public string Locatie { get; set; } = default!;
        public int? OpleidingVereistId { get; set; }
        public Opleiding? OpleidingVereist { get; set; }
        public string? FotoUrl { get; set; } = default!;
        public byte[]? Afbeelding { get; set; }

        public List<CustomUser> Personen { get; set; } = new List<CustomUser>();
        public int BeschikbarePlaatsen => AantalPlaatsen - Personen.Count;



    }
}
