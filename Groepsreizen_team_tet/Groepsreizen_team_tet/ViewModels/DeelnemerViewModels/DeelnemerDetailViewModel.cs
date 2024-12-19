namespace Groepsreizen_team_tet.ViewModels.DeelnemerViewModels
{
    public class DeelnemerDetailViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; } = default!;
        public string Email { get; set; } = default!;
        public DateTime Geboortedatum { get; set; }
        public string Groepsreis { get; set; } = default!;
    }
}
