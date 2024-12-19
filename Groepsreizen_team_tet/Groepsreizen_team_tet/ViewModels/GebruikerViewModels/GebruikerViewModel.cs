namespace Groepsreizen_team_tet.ViewModels.GebruikerViewModels
{
    public class GebruikerViewModel
    {
        public int Id { get; set; }
        public string Voornaam { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public int Leeftijd { get; set; }
        public bool IsActief { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
