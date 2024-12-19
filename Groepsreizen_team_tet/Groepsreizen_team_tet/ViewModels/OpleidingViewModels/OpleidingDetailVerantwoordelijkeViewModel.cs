namespace Groepsreizen_team_tet.ViewModels.OpleidingViewModels
{
    public class OpleidingDetailVerantwoordelijkeViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Locatie { get; set; } = default!;
        public string Beschrijving { get; set; } = default!;
        public DateTime Begindatum { get; set; }
        public DateTime Einddatum { get; set; }
        public int AantalPlaatsen { get; set; }
        public int BeschikbarePlaatsen => AantalPlaatsen - Personen.Count;

        public byte[]? Afbeelding { get; set; }
        public string Vooropleiding { get; set; }

        public List<CustomUser> Personen { get; set; } = new List<CustomUser>();

    }

}
