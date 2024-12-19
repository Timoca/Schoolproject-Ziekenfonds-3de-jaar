using System.ComponentModel.DataAnnotations.Schema;

namespace Groepsreizen_team_tet.Models
{
    public class Wachtlijst
    {
        public int Id { get; set; }
        public int GroepsreisId { get; set; }

        public int KindId { get; set; }

        public DateTime InschrijvingDatum { get; set; }
        public string? Opmerkingen { get; set; }

        public Groepsreis Groepsreis { get; set; } = default!;

        public Kind Kind { get; set; } = default!;
    }
}
