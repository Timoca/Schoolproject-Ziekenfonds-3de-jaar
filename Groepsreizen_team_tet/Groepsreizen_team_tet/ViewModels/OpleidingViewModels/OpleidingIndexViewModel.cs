namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingIndexViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Locatie { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public int AantalPlaatsen { get; set; }
        public bool IsIngeschreven { get; set; }
        public string? Vooropleiding { get; set; } = default!;
        public List<CustomUser> Personen { get; set; } = new List<CustomUser>();

        public int BeschikbarePlaatsen => AantalPlaatsen - Personen.Count;

    }
}
