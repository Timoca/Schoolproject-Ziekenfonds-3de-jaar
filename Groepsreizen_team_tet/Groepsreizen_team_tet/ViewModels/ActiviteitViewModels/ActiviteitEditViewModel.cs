namespace Groepsreizen_team_tet.ViewModels.ActiviteitViewModels
{
    public class ActiviteitEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Naam { get; set; } = default!;

        [Required]
        public string Beschrijving { get; set; } = default!;
    }
}
