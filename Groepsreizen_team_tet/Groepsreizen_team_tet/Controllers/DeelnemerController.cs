namespace Groepsreizen_team_tet.Controllers;

public class DeelnemerController : Controller
{
    private readonly IUnitOfWork _uow;

    public DeelnemerController(IUnitOfWork unitOfWork)
    {
        _uow = unitOfWork;
    }

    // GET: Deelnemer/Index
    public async Task<IActionResult> Index()
    {
        // Haal alle deelnemers op en hun bijhorende kinderen en ouders
        var deelnemers = await _uow.DeelnemerRepository.Search()
            .Include(k => k.Kind)
            .ThenInclude(o => o.Ouder) // Include the parent (CustomUser) which has the Email
            .Include(g => g.Groepsreis)
            .ThenInclude(b => b.Bestemming)
            .ToListAsync();

        // Maan een VeiwMdel aan vooor de deelnemers, inclusief het emailadres van de ouder
        var viewModel = deelnemers.Select(d => new DeelnemerViewModel
        {
            Id = d.Id,
            NaamKind = d.Kind.Naam,
            VoornaamKind = d.Kind.Voornaam,
            EmailOuder = d.Kind.Ouder.Email,
            GroepsreisNaam = d.Groepsreis.Bestemming.Naam,
            Opmerkingen = d.Opmerkingen
        }).ToList();

        return View(viewModel);
    }
}