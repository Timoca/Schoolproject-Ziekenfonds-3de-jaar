namespace Groepsreizen_team_tet.Models
{
    public class Programma
    {
        public int Id { get; set; }
        public int ActiviteitId { get; set; }
        public int GroepsreisId { get; set; }


        public Activiteit Activiteit { get; set; } = default!;
        public Groepsreis Groepsreis { get; set; } = default!;
    }
}
