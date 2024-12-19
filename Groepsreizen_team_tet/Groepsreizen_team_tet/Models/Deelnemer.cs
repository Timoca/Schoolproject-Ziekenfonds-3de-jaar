namespace Groepsreizen_team_tet.Models
{
    public class Deelnemer
    {
        public int Id { get; set; }
        public int KindId { get; set; }
        public int GroepsreisDetailsId { get; set; }
        public string? Opmerkingen { get; set; } = default!;
        public int? ReviewScore { get; set; }
        public string? Review { get; set; } = default!;

        [DataType(DataType.Date)]
        public DateTime InschrijvingDatum { get; set; }
        public Kind Kind { get; set; } = default!;
        public Groepsreis Groepsreis { get; set; } = default!;
    }
}
