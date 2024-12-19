using Groepsreizen_team_tet.Attributes;

namespace Groepsreizen_team_tet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _uow;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _uow = unitOfWork;
        }

        // Toon de homepagina
        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            // Haal een lijst van de 6 groepsreizen op met de dichtstbijzijnde startdatum ten opzichte van de huidige datum + enkel niet-geannuleerde reizen
            var groepsreizen = await _uow.GroepsreisRepository.Search()
                .Where(g => g.Begindatum > DateTime.Now && !g.IsGeannuleerd)
                .OrderBy(g => g.Begindatum)
                .Take(6)
                .Include(g => g.Bestemming)
                    .ThenInclude(b => b.Fotos)
                    .Where(g => !g.IsKopie)
                .ToListAsync();


            // Maak het viewmodel en vul het met de opgehaalde reizen
            var viewModel = new HomeViewModel
            {
                Groepsreizen = groepsreizen
            };

            // Geef het viewmodel door aan de view
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Detailpagina van een groepsreis
        // GET: Home/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            // Haal de groepsreis op inclusief gerelateerde data (Bestemming, Activiteiten, Foto's)
            var groepsreis = await _uow.GroepsreisRepository.Search()
                .Where(g => g.Id == id)
                .Include(g => g.Bestemming)
                    .ThenInclude(b => b.Fotos)
                .Include(g => g.Programmas)
                    .ThenInclude(p => p.Activiteit)
                .FirstOrDefaultAsync();

            if (groepsreis == null)
            {
                return NotFound();
            }

            // Maak een ViewModel aan en vul het met de gegevens van de groepsreis
            var viewModel = new HomeDetailViewModel
            {
                Id = groepsreis.Id,
                Begindatum = groepsreis.Begindatum,
                Einddatum = groepsreis.Einddatum,
                Prijs = groepsreis.Prijs,
                BestemmingNaam = groepsreis.Bestemming.Naam,
                BestemmingBeschrijving = groepsreis.Bestemming.Beschrijving,
                MinLeeftijd = groepsreis.Bestemming.MinLeeftijd,
                MaxLeeftijd = groepsreis.Bestemming.MaxLeeftijd,
                FotoBase64Strings = groepsreis.Bestemming.Fotos.Select(f => f.Afbeelding != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(f.Afbeelding)}" : "https://via.placeholder.com/150").ToList(),
                Activiteiten = groepsreis.Programmas.Select(p => new ActiviteitDetailViewModel
                {
                    Naam = p.Activiteit.Naam,
                    Beschrijving = p.Activiteit.Beschrijving
                }).ToList(),
            };

            // Geef het viewmodel door aan de view
            return View(viewModel);
        }
    }
}
