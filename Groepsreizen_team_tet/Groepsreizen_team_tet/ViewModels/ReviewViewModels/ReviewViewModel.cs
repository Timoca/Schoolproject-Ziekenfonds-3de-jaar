namespace Groepsreizen_team_tet.ViewModels.ReviewViewModels
{
    public class ReviewViewModel
    {
        public int GroepsreisId { get; set; }

        [Range(1, 5, ErrorMessage = "Geef een score tussen 1 en 5.")]
        public int Score { get; set; }

        public string? Opmerking { get; set; }
    }
}
