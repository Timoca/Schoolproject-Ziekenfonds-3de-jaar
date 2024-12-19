namespace Groepsreizen_team_tet.ViewModels.GezinsledenViewModels
{
    public class GezinsledenManageViewModel
    {
        // Lijst van bestaande gezinsleden
        public List<GezinslidViewModel> Gezinsleden { get; set; } = new List<GezinslidViewModel>();

        // Gegevens voor een nieuw gezinslid
        public GezinslidViewModel NewGezinslid { get; set; } = new GezinslidViewModel();
    }

    public class GezinslidViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        [StringLength(100)]
        public string Voornaam { get; set; } = default!;

        [Required(ErrorMessage = "Naam is verplicht.")]
        [StringLength(100)]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        [DataType(DataType.Date)]
        public DateTime Geboortedatum { get; set; }

        // Lijsten voor Allergieën en Medicatie
        public List<string> AllergieenList { get; set; } = new List<string>();

        public List<string> MedicatieList { get; set; } = new List<string>();
    }
}
