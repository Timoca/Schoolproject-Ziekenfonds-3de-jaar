namespace Groepsreizen_team_tet.ViewModels.GebruikerViewModels
{
    public class DeleteGebruikerViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = default!;

        [Display(Name = "Naam")]
        public string Naam { get; set; } = default!;

        [Display(Name = "Rol")]
        public string Rol { get; set; } = default!;
    }
}
