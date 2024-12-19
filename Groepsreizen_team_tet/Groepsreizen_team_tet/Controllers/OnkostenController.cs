using Groepsreizen_team_tet.Attributes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Groepsreizen_team_tet.Controllers;

[ApiExplorerSettings(IgnoreApi = true)] //deze controller wordt nu niet mee opgenomen in swagger
[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Groepsreizen", controller: "Groepsreis", action: "Detail")]
public class OnkostenController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<CustomUser> _userManager;
    private readonly ILogger<OpleidingController> _logger;


    public OnkostenController(IUnitOfWork uow, UserManager<CustomUser> userManager, ILogger<OpleidingController> logger)
    {
        _uow = uow;
        _userManager = userManager;
        _logger = logger;
    }


    #region Index
    // GET
    [Breadcrumb("Onkosten Overzicht")]
    public async Task<IActionResult> Index(int groepsreisId)
    {
        // Haal de groepsreis op met de bijbehorende bestemming
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Where(g => g.Id == groepsreisId)
            .Include(g => g.Deelnemers) // deelnemers toevoegen zodat budget berekend kan worden
            .Include(g => g.Bestemming) // Zorg ervoor dat Bestemming wordt ingeladen
            .FirstOrDefaultAsync();

        // Controleer of de groepsreis bestaat
        if (groepsreis == null)
        {
            return NotFound(); // Geef een 404 terug als de groepsreis niet bestaat
        }

        // Controleer of de bestemming niet null is
        var groepsreisNaam = groepsreis.Bestemming?.Naam ?? "Onbekende Bestemming";

        // Haal alleen de onkosten op die zijn ingevoerd door de hoofdmonitor
        var onkosten = await _uow.OnkostenRepository.Search()
            .Where(o => o.GroepsreisId == groepsreisId && o.IngegevenDoorHoofdmonitor) // Filter op hoofdmonitor
            .ToListAsync();

        var totaalOnkosten = onkosten.Sum(o => o.Bedrag);
        var budgetHoofdmonitor = groepsreis.Deelnemers.Count * 25;

        // Controleer deze waarden
        _logger.LogInformation("Totaal Onkosten: {TotaalOnkosten}", totaalOnkosten);
        _logger.LogInformation("Budget Hoofdmonitor: {BudgetHoofdmonitor}", budgetHoofdmonitor);

        // Maak een ViewModel aan om de onkosten weer te geven
        var viewModel = new OnkostenIndexViewModel
        {
            GroepsreisId = groepsreisId,
            GroepsreisNaam = groepsreisNaam,
            OnkostenLijst = onkosten.Select(o => new OnkostenDetailViewModel
            {
                Datum = o.Datum,
                Titel = o.Titel,
                Omschrijving = o.Omschrijving,
                Bedrag = o.Bedrag,
                Locatie = o.Locatie,
                Betaalbewijs = o.Betaalbewijs
            }).ToList(),
            TotaalOnkosten = totaalOnkosten,
            BudgetHoofdmonitor = budgetHoofdmonitor
        };

        return View(viewModel);
    }
    #endregion

    #region Beheer
    [Route("Onkosten/Beheer/{groepsreisId}")]
    [Breadcrumb("Onkosten Beheren")]
    [HttpGet]
    public async Task<IActionResult> Beheer(int groepsreisId)
    {
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Where(g => g.Id == groepsreisId)
            .Include(g => g.Bestemming)
            .Include(g => g.Deelnemers)
            .FirstOrDefaultAsync();

        if (groepsreis == null)
        {
            return NotFound();
        }

        var onkosten = await _uow.OnkostenRepository.Search()
            .Where(o => o.GroepsreisId == groepsreisId)
            .ToListAsync();

        var totaalkost = onkosten.Sum(o => o.Bedrag);

        // Berekeningen
        var opbrengst = groepsreis.Deelnemers.Count * groepsreis.Prijs;
        var budgetHoofdmonitor = groepsreis.Deelnemers.Count * 25;
        //var onkostenReis = totaalkost; 

        var onkostenHoofdmonitor = onkosten.Where(o => o.IngegevenDoorHoofdmonitor).Sum(o => o.Bedrag);
        var onkostenVerantwoordelijke = onkosten.Where(o => !o.IngegevenDoorHoofdmonitor).Sum(o => o.Bedrag);


        var viewModel = new OnkostenBeheerViewModel
        {
            GroepsreisId = groepsreisId,
            GroepsreisNaam = groepsreis.Bestemming?.Naam ?? "Onbekende Bestemming",
            Totaalkost = totaalkost,
            Opbrengst = opbrengst,
            BudgetHoofdmonitor = budgetHoofdmonitor,
            //OnkostenReis = onkostenReis,
            IsGeannuleerd = groepsreis.IsGeannuleerd,
            OnkostenHoofdmonitor = onkostenHoofdmonitor,
            OnkostenVerantwoordelijke = onkostenVerantwoordelijke,
            OnkostenHoofdmonitorLijst = onkosten.Where(o => o.IngegevenDoorHoofdmonitor)
            .Select(o => new OnkostenDetailViewModel
            {
                Id = o.Id,
                Datum = o.Datum,
                Titel = o.Titel,
                Omschrijving = o.Omschrijving,
                Bedrag = o.Bedrag,
                Betaalbewijs = o.Betaalbewijs
            }).ToList(),
            OnkostenVerantwoordelijkeLijst = onkosten.Where(o => !o.IngegevenDoorHoofdmonitor)
            .Select(o => new OnkostenDetailViewModel
            {
                Id = o.Id,
                Datum = o.Datum,
                Titel = o.Titel,
                Omschrijving = o.Omschrijving,
                Bedrag = o.Bedrag,
                Betaalbewijs = o.Betaalbewijs
            }).ToList()
        };

        return View(viewModel);
    }


    #endregion

    #region Create
    // GET
    [Breadcrumb("Onkosten Invoeren")]
    [Authorize(Roles = "Hoofdmonitor, Verantwoordelijke")]
    public async Task<IActionResult> Create(int groepsreisId)
    {
        var gebruiker = await _userManager.GetUserAsync(User);

        if (gebruiker == null)
        {
            TempData["ErrorMessage"] = "Je bent niet ingelogd.";
            return RedirectToAction("Index", "Groepsreis");
        }

        // Controleer of de gebruiker een hoofdmonitor of verantwoordelijke is
        var isHoofdmonitor = await _uow.MonitorRepository.Search()
            .AnyAsync(m => m.PersoonId == gebruiker.Id && m.GroepsreisDetailsId == groepsreisId && m.IsHoofdMonitor == 1);

        var isVerantwoordelijke = User.IsInRole("Verantwoordelijke");

        if (!isHoofdmonitor && !isVerantwoordelijke)
        {
            TempData["ErrorMessage"] = "Je hebt geen toegang om onkosten in te voeren voor deze groepsreis.";
            return RedirectToAction("Index", "Groepsreis");
        }

        // Controleer of de groepsreis bestaat
        var groepsreis = await _uow.GroepsreisRepository.GetByIdAsync(groepsreisId);
        if (groepsreis == null)
        {
            TempData["ErrorMessage"] = "De groepsreis bestaat niet.";
            return RedirectToAction("Index", "Groepsreis");
        }

        var viewModel = new OnkostenCreateViewModel
        {
            GroepsreisId = groepsreisId
        };
        _logger.LogWarning("GroepsreisId ontvangen in Create-GET: {GroepsreisId}", groepsreisId);
        return View(viewModel);
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Hoofdmonitor, Verantwoordelijke")]
    public async Task<IActionResult> Create(OnkostenCreateViewModel viewModel)
    {       
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validatiefout bij het invoeren van onkosten: {Fouten}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View(viewModel);
        }

        //Haal huidige gebruiker op + check of hoofdmonitor hoofdmonitor van de groepsreis is
        var gebruikerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var isHoofdmonitor = await _uow.MonitorRepository.Search()
            .AnyAsync(m => m.PersoonId == gebruikerId && m.GroepsreisDetailsId == viewModel.GroepsreisId && m.IsHoofdMonitor == 1);
        var isVerantwoordelijke = User.IsInRole("Verantwoordelijke");

        if (!isHoofdmonitor && !isVerantwoordelijke)
        {
            TempData["ErrorMessage"] = "Je hebt geen toegang om onkosten in te voeren voor deze groepsreis.";
            return RedirectToAction("Index", "Groepsreis");
        }

        // Controleer of de GroepsreisId bestaat
        var groepsreis = await _uow.GroepsreisRepository.GetByIdAsync(viewModel.GroepsreisId);
        if (groepsreis == null)
        {
            _logger.LogError("Groepsreis met ID {GroepsreisId} bestaat niet.", viewModel.GroepsreisId);
            return NotFound("De opgegeven groepsreis bestaat niet.");
        }

        // Verwerk de foto en zet om naar een blob (als er een foto is geüpload)
        byte[]? betaalbewijsBlob = null;
        if (viewModel.Betaalbewijs != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await viewModel.Betaalbewijs.CopyToAsync(memoryStream);
                betaalbewijsBlob = memoryStream.ToArray();
            }
        }        

        // Maak de nieuwe onkost aan 
        var nieuweOnkost = new Onkosten
        {
            GroepsreisId = viewModel.GroepsreisId,
            Titel = viewModel.Titel,
            Omschrijving = viewModel.Omschrijving,
            Bedrag = viewModel.Bedrag,
            Datum = viewModel.Datum,
            Locatie = viewModel.Locatie,
            Betaalbewijs = betaalbewijsBlob, // Sla de foto op als blob
            IngegevenDoorHoofdmonitor = isHoofdmonitor // Stel in wie de onkost heeft ingevoerd
        };

        _uow.OnkostenRepository.Create(nieuweOnkost);
        await _uow.SaveAsync();

        // Redirect afhankelijk van de rol
        if (User.IsInRole("Hoofdmonitor"))
        {
            return RedirectToAction("Index", new { groepsreisId = viewModel.GroepsreisId });
        }
        else if (User.IsInRole("Verantwoordelijke"))
        {
            return RedirectToAction("Beheer", new { id = viewModel.GroepsreisId });
        }

        return RedirectToAction("Detail", "Groepsreis", new { id = viewModel.GroepsreisId }); // Fallback
    }

    #endregion

    #region Detail
    [Breadcrumb("Onkosten Detail")]
    public async Task<IActionResult> Detail(int id)
    {
        var onkost = await _uow.OnkostenRepository.GetByIdAsync(id);

        if (onkost == null)
        {
            return NotFound();
        }

        var viewModel = new OnkostenDetailViewModel
        {
            Id = onkost.Id,
            GroepsreisId = onkost.GroepsreisId,
            Datum = onkost.Datum,
            Titel = onkost.Titel,
            Omschrijving = onkost.Omschrijving,
            Bedrag = onkost.Bedrag,
            Locatie = onkost.Locatie,
            Betaalbewijs = onkost.Betaalbewijs
        };

        return View(viewModel);
    }
    #endregion

    #region Edit
    //GET
    [Breadcrumb("Onkosten Bewerken")]
    public async Task<IActionResult> Edit(int id)
    {
        var onkost = await _uow.OnkostenRepository.GetByIdAsync(id);
        if (onkost == null)
        {
            return NotFound();
        }

        var viewModel = new OnkostenEditViewModel
        {
            Id = onkost.Id,
            GroepsreisId = onkost.GroepsreisId,
            Datum = onkost.Datum,
            Titel = onkost.Titel,
            Omschrijving = onkost.Omschrijving,
            Bedrag = onkost.Bedrag,
            Locatie = onkost.Locatie
        };

        return View(viewModel);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OnkostenEditViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var onkost = await _uow.OnkostenRepository.GetByIdAsync(viewModel.Id);
        if (onkost == null)
        {
            return NotFound();
        }

        onkost.Datum = viewModel.Datum;
        onkost.Titel = viewModel.Titel;
        onkost.Omschrijving = viewModel.Omschrijving;
        onkost.Bedrag = viewModel.Bedrag;
        onkost.Locatie = viewModel.Locatie;

        if (viewModel.Betaalbewijs != null)
        {
            using var memoryStream = new MemoryStream();
            await viewModel.Betaalbewijs.CopyToAsync(memoryStream);
            onkost.Betaalbewijs = memoryStream.ToArray();
        }

        _uow.OnkostenRepository.Update(onkost);
        await _uow.SaveAsync();

        return RedirectToAction("Beheer", new { id = viewModel.GroepsreisId });
    }
    #endregion

    #region Delete
    [Breadcrumb("Onkosten Verwijderen")]
    public async Task<IActionResult> Delete(int id)
    {
        var onkost = await _uow.OnkostenRepository.GetByIdAsync(id);
        if (onkost == null)
        {
            return NotFound();
        }

        var viewModel = new OnkostenDetailViewModel
        {
            Id = onkost.Id,
            GroepsreisId = onkost.GroepsreisId,
            Datum = onkost.Datum,
            Titel = onkost.Titel,
            Omschrijving = onkost.Omschrijving,
            Bedrag = onkost.Bedrag,
            Locatie = onkost.Locatie
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var onkost = await _uow.OnkostenRepository.GetByIdAsync(id);
        if (onkost == null)
        {
            return NotFound();
        }

        _uow.OnkostenRepository.Delete(onkost);
        await _uow.SaveAsync();

        return RedirectToAction("Beheer", new { id = onkost.GroepsreisId });
    }
    #endregion
}