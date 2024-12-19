namespace Groepsreizen_team_tet.Models
{
    public class CustomUser : IdentityUser<int>
    {
        //Id is onderdeel van IdentityUser.
        public string Naam { get; set; } = default!;

        public string Voornaam { get; set; } = default!;

        public string Straat { get; set; } = default!;

        public string Huisnummer { get; set; } = default!;

        public string Gemeente { get; set; } = default!;

        public string Postcode { get; set; } = default!;

        public DateTime Geboortedatum { get; set; }

        public string Huisdokter { get; set; } = default!;

        public string? ContractNummer { get; set; } = default!;

        //Email is onderdeel van IdentityUser.

        //Telefoonnummer is onderdeel van IdentityUser.

        public string? RekeningNummer { get; set; } = default!;

        public bool IsActief { get; set; }

        public Monitor? Monitor { get; set; }
        public List<Kind> Kinderen { get; set; } = new List<Kind>();
        public List<Opleiding> Opleidingen { get; set; } = new List<Opleiding>();
    }
}
