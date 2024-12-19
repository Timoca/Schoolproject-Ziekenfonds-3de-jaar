using Groepsreizen_team_tet.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<CustomUser> _userManager;
    private readonly GroepsreizenContext _context;

    public DashboardController(IUnitOfWork uow, UserManager<CustomUser> userManager, GroepsreizenContext context)
    {
        _uow = uow;
        _userManager = userManager;
        _context = context;
    }

    #region Index
    public async Task<IActionResult> Index(string? message)
    {
        // Haal de huidige ingelogde gebruiker op
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Haal alle opleidingen op inclusief personen
        var opleidingen = await _uow.OpleidingRepository.GetAllWithIncludeAsync(o => o.Personen);

        // Filter alleen de opleidingen waarvoor de gebruiker is ingeschreven en die nog niet zijn gestart
        var ingeschrevenOpleidingen = opleidingen
            .Where(o => o.Personen.Any(p => p.Id == user.Id) && o.Begindatum > DateTime.Now)
            .Select(o => new OpleidingIndexViewModel
            {
                Id = o.Id,
                Naam = o.Naam,
                Beschrijving = o.Beschrijving,
                Locatie = o.Locatie,
                Begindatum = o.Begindatum,
                Einddatum = o.Einddatum,                
                IsIngeschreven = true // Omdat we alleen de opleidingen ophalen waarvoor de gebruiker is ingeschreven
            })
            .ToList();

        // Haal meldingen op uit de sessie
        var meldingen = HttpContext.Session.GetObjectFromJson<List<string>>("MeldingenDoorVerantwoordelijke") ?? new List<string>();

        // Haal de groepsreizen op waarvoor de gebruiker een review kan geven
        var now = DateTime.Now;
        var oneMonthAgo = now.AddMonths(-1);

        // Haal alle Deelnemers voor deze gebruiker op die groepsreizen hebben gevolgd binnen de laatste maand en nog geen review hebben gegeven
        var deelnemerGroepsreizen = await _context.Deelnemers
            .Include(d => d.Groepsreis)
            .Where(d => d.Kind.PersoonId == user.Id
                && d.Groepsreis.Einddatum <= now
                && d.Groepsreis.Einddatum >= oneMonthAgo
                && !d.ReviewScore.HasValue)
            .Select(d => new GroepsreisForReviewViewModel
            {
                GroepsreisId = d.Groepsreis.Id,
                Naam = d.Groepsreis.Bestemming.Naam,
                Einddatum = d.Groepsreis.Einddatum
            })
            .Distinct()
            .ToListAsync();


        // Maak een viewmodel aan voor de dashboard view
        var viewModel = new DashboardViewModel
        {
            IngeschrevenOpleidingen = ingeschrevenOpleidingen,
            MeldingenDoorVerantwoordelijke = meldingen,
            ReviewGroepsreizen = deelnemerGroepsreizen
        };

        return View(viewModel);
    }
    #endregion
}