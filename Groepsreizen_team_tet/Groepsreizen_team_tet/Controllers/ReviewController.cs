using Groepsreizen_team_tet.ViewModels.ReviewViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Groepsreizen_team_tet.Controllers;

[Authorize(Roles = "Deelnemer")]
public class ReviewController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<CustomUser> _userManager;
    private readonly GroepsreizenContext _context;

    public ReviewController(IUnitOfWork uow, UserManager<CustomUser> userManager, GroepsreizenContext context)
    {
        _uow = uow;
        _userManager = userManager;
        _context = context;
    }


    // GET: Review/Create/{groepsreisId}
    public async Task<IActionResult> Create(int groepsreisId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Controleer of de gebruiker een deelnemer is van de opgegeven groepsreis
        var deelnemer = await _context.Deelnemers
            .Include(d => d.Groepsreis)
            .FirstOrDefaultAsync(d => d.GroepsreisDetailsId == groepsreisId && d.Kind.PersoonId == user.Id);

        if (deelnemer == null)
        {
            return Unauthorized("Je bent geen deelnemer van deze groepsreis.");
        }

        if (deelnemer.ReviewScore.HasValue)
        {
            return RedirectToAction("Index", "Dashboard", new { message = "Je hebt al een review gegeven voor deze groepsreis." });
        }

        // Controleer of de groepsreis afgelopen is en binnen de laatste maand
        var now = DateTime.Now;
        if (deelnemer.Groepsreis.Einddatum > now || deelnemer.Groepsreis.Einddatum < now.AddMonths(-1))
        {
            return RedirectToAction("Index", "Dashboard", new { message = "Je kunt alleen binnen een maand na de groepsreis een review geven." });
        }

        var viewModel = new ReviewViewModel
        {
            GroepsreisId = groepsreisId
        };

        return View(viewModel);
    }

    // POST: Review/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Zoek de deelnemer
        var deelnemer = await _context.Deelnemers
            .Include(d => d.Groepsreis)
            .FirstOrDefaultAsync(d => d.GroepsreisDetailsId == model.GroepsreisId && d.Kind.PersoonId == user.Id);

        if (deelnemer == null)
        {
            return Unauthorized("Je bent geen deelnemer van deze groepsreis.");
        }

        if (deelnemer.ReviewScore.HasValue)
        {
            return RedirectToAction("Index", "Dashboard", new { message = "Je hebt al een review gegeven voor deze groepsreis." });
        }

        // Controleer of de groepsreis afgelopen is en binnen de laatste maand
        var now = DateTime.Now;
        if (deelnemer.Groepsreis.Einddatum > now || deelnemer.Groepsreis.Einddatum < now.AddMonths(-1))
        {
            return RedirectToAction("Index", "Dashboard", new { message = "Je kunt alleen binnen een maand na de groepsreis een review geven." });
        }

        // Sla de review op
        deelnemer.ReviewScore = model.Score;
        deelnemer.Review = model.Opmerking;

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Dashboard", new { message = "Bedankt voor je review!" });
    }
}