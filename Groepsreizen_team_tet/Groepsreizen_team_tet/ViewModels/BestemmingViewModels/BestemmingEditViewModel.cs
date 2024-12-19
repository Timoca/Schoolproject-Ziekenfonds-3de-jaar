namespace Groepsreizen_team_tet.ViewModels.BestemmingViewModels
{
    public class BestemmingEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is verplicht.")]
        public string Code { get; set; } = default!;

        [Required(ErrorMessage = "Naam is verplicht.")]
        public string Naam { get; set; } = default!;

        [Required(ErrorMessage = "Beschrijving is verplicht.")]
        public string Beschrijving { get; set; } = default!;

        [Required(ErrorMessage = "MinLeeftijd is verplicht.")]
        public int MinLeeftijd { get; set; }

        [Required(ErrorMessage = "MaxLeeftijd is verplicht.")]
        public int MaxLeeftijd { get; set; }

        public int LeeftijdCategorie { get; set; } = 1;

        // Foto's die door de gebruiker geüpload worden
        public List<IFormFile>? FotoFiles { get; set; }

        // Foto's die de gebruiker wil verwijderen
        public List<int> FotosToDelete { get; set; } = new();

        // Bestaande foto's van de bestemming
        public List<FotoEditViewModel> BestaandeFotos { get; set; } = new();
    }
}
