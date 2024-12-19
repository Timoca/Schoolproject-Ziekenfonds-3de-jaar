namespace Groepsreizen_team_tet.ViewModels.MonitorViewModels
{
    public class MonitorCreateViewModel
    {
        [Display(Name = "Zoek gebruikers")]
        public string? SearchString { get; set; }

        public List<MonitorUserViewModel> Gebruikers { get; set; } = new List<MonitorUserViewModel>();
    }

    public class MonitorUserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = string.Empty;

        [Display(Name = "Naam")]
        public string Naam { get; set; } = string.Empty;

        [Display(Name = "Leeftijd")]
        public int Leeftijd { get; set; }

        [Display(Name = "Huidige Rol")]
        public string Role { get; set; } = string.Empty;
    }
}
