namespace Groepsreizen_team_tet.ViewModels.BestemmingViewModels
{
    public class BestemmingCreateViewModel
    {
        [Required(ErrorMessage = "Code is verplicht.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; } = default!;

        [Required(ErrorMessage = "MinLeeftijd is verplicht.")]
        public int MinLeeftijd { get; set; }

        [Required(ErrorMessage = "MaxLeeftijd is verplicht.")]
        public int MaxLeeftijd { get; set; }
        public List<IFormFile>? FotoFiles { get; set; }
    }
}
