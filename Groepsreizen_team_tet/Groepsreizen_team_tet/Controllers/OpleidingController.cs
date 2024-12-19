using Groepsreizen_team_tet.Attributes;
using Groepsreizen_team_tet.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Opleidingbeheer", controller: "Opleiding", action: "Beheer")]
public class OpleidingController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<CustomUser> _userManager;
    private readonly ILogger<OpleidingController> _logger;



    public OpleidingController(IUnitOfWork uow, UserManager<CustomUser> userManager, ILogger<OpleidingController> logger)
    {
        _uow = uow;
        _userManager = userManager;
        _logger = logger;
    }

    #region Index
    public async Task<IActionResult> Index()
    {
        // Haal de huidige ingelogde gebruiker op
        var userIdString = _userManager.GetUserId(User);
        int? userId = null;
        if (int.TryParse(userIdString, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        // Haal alle opleidingen op inclusief personen
        var opleidingen = await _uow.OpleidingRepository.GetAllWithIncludeAsync(o => o.Personen);

        // Filter de opleidingen op basis van de begindatum
        var aankomendeOpleidingen = opleidingen
            .Where(o => o.Begindatum > DateTime.Now)
            .Select(o => new OpleidingIndexViewModel
            {
                Id = o.Id,
                Naam = o.Naam,
                Beschrijving = o.Beschrijving,
                Locatie = o.Locatie,
                Begindatum = o.Begindatum,
                Einddatum = o.Einddatum,
                AantalPlaatsen = o.AantalPlaatsen,
                Vooropleiding = o.OpleidingVereist != null ? o.OpleidingVereist.Naam : "Geen",
                Personen = o.Personen.ToList(), // Nodig voor de berekening van BeschikbarePlaatsen
                IsIngeschreven = userId.HasValue && o.Personen.Any(p => p.Id == userId.Value)
            })
            .ToList();

        // Extra log om de beschikbare plaatsen en inschrijvingsstatus te controleren
        foreach (var opleiding in aankomendeOpleidingen)
        {
            _logger.LogInformation($"Opleiding ID: {opleiding.Id}, Aantal plaatsen: {opleiding.AantalPlaatsen}, Beschikbare plaatsen: {opleiding.BeschikbarePlaatsen}, Is ingeschreven: {opleiding.IsIngeschreven}");
        }

        return View(aankomendeOpleidingen);
    }

    #endregion

    #region Beheer
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> Beheer()
    {
        // Haal alle opleidingen op inclusief de ingeschreven personen
        var opleidingen = await _uow.OpleidingRepository.GetAllWithIncludeAsync(o => o.Personen);
        var viewModel = new OpleidingBeheerViewModel
        {
            Opleidingen = opleidingen.ToList()
        };

        return View(viewModel);
    }
    #endregion

    #region Detail
    //Detail verantwoordelijke
    [Breadcrumb("Details Opleiding")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> DetailVerantwoordelijke(int id)
    {
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(id, o => o.Personen, o => o.OpleidingVereist);
        if (opleiding == null)
        {
            return NotFound();
        }

        var viewModel = new OpleidingDetailVerantwoordelijkeViewModel
        {
            Id = opleiding.Id,
            Naam = opleiding.Naam,
            Locatie = opleiding.Locatie,
            Beschrijving = opleiding.Beschrijving,
            Begindatum = opleiding.Begindatum,
            Einddatum = opleiding.Einddatum,
            AantalPlaatsen = opleiding.AantalPlaatsen,
            Afbeelding = opleiding.Afbeelding,
            Vooropleiding = opleiding.OpleidingVereist != null ? opleiding.OpleidingVereist.Naam : "Geen",
            Personen = opleiding.Personen.ToList() // Nodig voor de berekening van BeschikbarePlaatsen

        };

        return View(viewModel);
    }

    //Detail monitor
    public async Task<IActionResult> DetailMonitor(int id)
    {
        //Haal de opleidingen op, inclusief ingeschreven personen, inclusief vereiste vooropleiding
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(id, o => o.Personen, m => m.OpleidingVereist);

        if (opleiding == null)
        {
            return NotFound();
        }

        // Controleer of de huidige gebruiker is ingeschreven
        var userIdString = _userManager.GetUserId(User);
        bool isIngeschreven = false;
        if (int.TryParse(userIdString, out int userId))
        {
            isIngeschreven = opleiding.Personen.Any(p => p.Id == userId);
        }


        var viewModel = new OpleidingDetailMonitorViewModel
        {
            Id = opleiding.Id,
            Naam = opleiding.Naam,
            Beschrijving = opleiding.Beschrijving,
            Locatie = opleiding.Locatie,
            Afbeelding = opleiding.Afbeelding,
            Begindatum = opleiding.Begindatum,
            Einddatum = opleiding.Einddatum,
            AantalPlaatsen = opleiding.AantalPlaatsen,
            IsIngeschreven = isIngeschreven,
            Vooropleiding = opleiding.OpleidingVereist != null ? opleiding.OpleidingVereist.Naam : "Geen",
            Personen = opleiding.Personen.ToList() // Nodig voor de berekening van BeschikbarePlaatsen
        };

        return View(viewModel);
    }


    #endregion

    #region Create
    // GET: Create
    [Breadcrumb("Opleiding Aanmaken")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new OpleidingCreateViewModel();

        // Verkrijg de vereiste opleidingen
        var opleidingVereisten = await _uow.OpleidingRepository.GetAllAsync();
        viewModel.OpleidingVereisten = opleidingVereisten.ToList();

        return View(viewModel);
    }


    // POST: Create
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OpleidingCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            byte[]? afbeeldingBlob = null;

            if (viewModel.Afbeelding != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await viewModel.Afbeelding.CopyToAsync(memoryStream);
                    afbeeldingBlob = memoryStream.ToArray();
                }
            }
            
            var opleiding = new Opleiding
            {
                Naam = viewModel.Naam,
                Beschrijving = viewModel.Beschrijving,
                Begindatum = viewModel.Begindatum,
                Einddatum = viewModel.Einddatum,
                AantalPlaatsen = viewModel.AantalPlaatsen,
                Locatie = viewModel.Locatie,
                Afbeelding = afbeeldingBlob,
                OpleidingVereistId = viewModel.OpleidingVereistId
            };

            _uow.OpleidingRepository.Create(opleiding);
            await _uow.SaveAsync();

            return RedirectToAction("Beheer");
        }

        // Reload the view with available prerequisites if model is invalid
        viewModel.OpleidingVereisten = (await _uow.OpleidingRepository.GetAllAsync()).ToList();
        return View(viewModel);
    }

    #endregion

    #region Edit
    //GET: Edit
    [Breadcrumb("Opleiding Bewerken")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> Edit(int id)
    {
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(id);
        if (opleiding == null)
        {
            return NotFound();
        }

        var viewModel = new OpleidingEditViewModel
        {
            Id = opleiding.Id,
            Naam = opleiding.Naam,
            Beschrijving = opleiding.Beschrijving,
            Begindatum = opleiding.Begindatum,
            Einddatum = opleiding.Einddatum,
            AantalPlaatsen = opleiding.AantalPlaatsen,
            Locatie = opleiding.Locatie,
            OpleidingVereistId = opleiding.OpleidingVereistId,
            OpleidingVereisten = (await _uow.OpleidingRepository.GetAllAsync()).ToList()
        };

        return View(viewModel);
    }

    //POST: Edit
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OpleidingEditViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var opleiding = await _uow.OpleidingRepository.GetByIdAsync(viewModel.Id);
            if (opleiding == null)
            {
                return NotFound();
            }

            opleiding.Naam = viewModel.Naam;
            opleiding.Beschrijving = viewModel.Beschrijving;
            opleiding.Begindatum = viewModel.Begindatum;
            opleiding.Einddatum = viewModel.Einddatum;
            opleiding.AantalPlaatsen = viewModel.AantalPlaatsen;
            opleiding.Locatie = viewModel.Locatie;
            opleiding.OpleidingVereistId = viewModel.OpleidingVereistId;

            if (viewModel.Afbeelding != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await viewModel.Afbeelding.CopyToAsync(memoryStream);
                    opleiding.Afbeelding = memoryStream.ToArray();
                }
            }

            _uow.OpleidingRepository.Update(opleiding);
            await _uow.SaveAsync();

            return RedirectToAction("Beheer");
        }

        viewModel.OpleidingVereisten = (await _uow.OpleidingRepository.GetAllAsync()).ToList();
        return View(viewModel);
    }
    #endregion

    #region Delete
    // GET: Delete
    [Breadcrumb("Opleiding Verwijderen")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> Delete(int id)
    {
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(id);
        if (opleiding == null)
        {
            return NotFound();
        }

        return View(opleiding);
    }

    // POST: Delete
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Haal de opleiding op die je wilt verwijderen
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(id);
        if (opleiding != null)
        {
            // Zoek naar opleidingen die deze opleiding als vereiste hebben
            var opleidingenMetVereiste = await _uow.OpleidingRepository.GetAllAsync();
            foreach (var vereisteOpleiding in opleidingenMetVereiste)
            {
                if (vereisteOpleiding.OpleidingVereistId == id)
                {
                    // Zet de OpleidingVereistId op NULL
                    vereisteOpleiding.OpleidingVereistId = null;
                    _uow.OpleidingRepository.Update(vereisteOpleiding);
                }
            }

            // Verwijder de opleiding
            _uow.OpleidingRepository.Delete(opleiding);
            await _uow.SaveAsync();
        }

        return RedirectToAction("Beheer");
    }


    #endregion

    #region Monitor Inschrijven
    // GET: Opleiding/Inschrijven/5 - Toont de bevestigingspagina
    [Authorize(Roles = "Monitor")]
    public async Task<IActionResult> Inschrijven(int id)
    {
        // Opleiding ophalen, inclusief vereiste opleiding
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(id, o => o.OpleidingVereist);
        if (opleiding == null || opleiding.AantalPlaatsen <= opleiding.Personen.Count)
        {
            // Redirect naar de index als de opleiding niet bestaat of volzet is
            return RedirectToAction("Index");
        }

        // Controleer of de gebruiker al is ingeschreven
        var userIdString = _userManager.GetUserId(User);
        if (int.TryParse(userIdString, out int userId))
        {
            if (opleiding.Personen.Any(p => p.Id == userId))
            {
                // Gebruiker is al ingeschreven
                return RedirectToAction("Index");
            }
        }

        return View(opleiding);
    }

    // GET: Opleiding/InschrijvenBevestigen/5 - Toont de bevestigingspagina na een succesvolle inschrijving
    [Authorize(Roles = "Monitor"), ActionName("InschrijvenBevestigen")]
    public async Task<IActionResult> InschrijvenBevestigen_Get(int id)
    {
        // Verkrijg de opleiding op basis van het ID
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(id);
        if (opleiding == null)
        {
            return RedirectToAction("Index");
        }

        return View(opleiding);
    }


    // POST: Opleiding/InschrijvenBevestigen/5 - Verwerkt de daadwerkelijke inschrijving
    [Authorize(Roles = "Monitor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InschrijvenBevestigen(int id)
    {
        // Verkrijg de opleiding op basis van het ID en checken op vereiste vooropleiding
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(id, o => o.Personen, o => o.OpleidingVereist);
        if (opleiding == null || opleiding.AantalPlaatsen <= opleiding.Personen.Count)
        {
            //Foutmelding en redirect naar de index als de opleiding niet bestaat of volzet is
            TempData["Foutmelding"] = "De opleiding bestaat niet of is volzet.";
            return RedirectToAction("Index");
        }

        // Verkrijg de huidige ingelogde gebruiker
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // Als de gebruiker niet bestaat of niet ingelogd is, redirect naar index
            TempData["Foutmelding"] = "U bent niet ingelogd of de gebruiker bestaat niet.";
            return RedirectToAction("Index");
        }

        // Controleer of de gebruiker al is ingeschreven voor deze opleiding
        var isAlIngeschreven = opleiding.Personen.Any(p => p.Id == user.Id);

        if (isAlIngeschreven)
        {
            // De gebruiker is al ingeschreven, redirect naar index
            TempData["Foutmelding"] = "Je bent al ingeschreven voor deze opleiding.";
            _logger.LogInformation($"Gebruiker met ID {user.Id} is al ingeschreven voor opleiding met ID {opleiding.Id}. Aantal ingeschreven personen: {opleiding.Personen.Count}");
            return RedirectToAction("Index");
        }

        // Controleer of de vereiste opleiding is afgerond
        if (opleiding.OpleidingVereistId.HasValue)
        {
            var vereisteOpleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(opleiding.OpleidingVereistId.Value, o => o.Personen);
            var heeftVereisteAfgerond = vereisteOpleiding?.Personen.Any(p => p.Id == user.Id) ?? false;

            if (!heeftVereisteAfgerond)
            {
                TempData["Foutmelding"] = "Je kunt je niet inschrijven voor deze gevorderde opleiding zonder de vereiste basisopleiding te hebben afgerond.";
                return RedirectToAction("Index");
            }
        }

        try
        {
            // Voeg de gebruiker toe aan de inschrijving van de opleiding
            opleiding.Personen.Add(user);

            // Update de database
            _uow.OpleidingRepository.Update(opleiding);
            await _uow.SaveAsync();

            // Zet een succesmelding
            // Extra log om te controleren of de inschrijving succesvol is
            _logger.LogInformation($"Gebruiker met ID {user.Id} succesvol ingeschreven voor opleiding met ID {opleiding.Id}. Aantal ingeschreven personen: {opleiding.Personen.Count}");
            TempData["Succesmelding"] = "Je bent succesvol ingeschreven voor de opleiding.";
        }
        catch (Exception ex)
        {
            // Zet een foutmelding indien er iets fout is gegaan
            TempData["Foutmelding"] = "Er is een fout opgetreden tijdens de inschrijving. Probeer het later opnieuw.";
            _logger.LogInformation($"Fout bij het inschrijven voor opleiding: {ex.Message}");

        }

        // Redirect naar de index na de inschrijving
        return RedirectToAction("Index");

        //// Redirect naar de inschrijfbevestiging
        //return RedirectToAction("InschrijvenBevestigen", new { id = opleiding.Id });
    }

    #endregion

    #region Lijst Ingeschreven Monitoren
    [Breadcrumb("Ingeschreven Monitoren")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> IngeschrevenMonitoren(int id)
    {
        // Haal de opleiding op inclusief ingeschreven personen
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(id, o => o.Personen);
        if (opleiding == null)
        {
            return NotFound();
        }

        // Verkrijg alle gebruikers die ingeschreven zijn voor de opleiding
        var ingeschrevenPersonen = opleiding.Personen.ToList();

        // Verkrijg alle gebruikers met de rol 'Monitor'
        var monitoren = await _userManager.GetUsersInRoleAsync("Monitor");

        // Filter de ingeschreven personen om alleen monitoren te tonen
        var ingeschrevenMonitoren = ingeschrevenPersonen
            .Where(p => monitoren.Any(m => m.Id == p.Id))
            .Select(p => new MonitorViewModel
            {
                Id = p.Id,
                Naam = $"{p.Voornaam} {p.Naam}", // Assuming `Voornaam` en `Naam` worden gebruikt voor de volledige naam
                Email = p.Email
            })
            .ToList();

        var viewModel = new OpleidingIngeschrevenMonitorenViewModel
        {
            OpleidingId = opleiding.Id,
            OpleidingNaam = opleiding.Naam,
            IngeschrevenMonitoren = ingeschrevenMonitoren
        };

        return View(viewModel);
    }
    #endregion

    #region Monitor Toevoegen door Verantwoordelijke
    [Breadcrumb("Monitor Toevoegen")]
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> MonitorToevoegen(int id)
    {
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(id);
        if (opleiding == null)
        {
            return NotFound();
        }

        // Verkrijg alle gebruikers die monitoren zijn, maar nog niet zijn ingeschreven voor deze opleiding
        var alleMonitoren = await _userManager.GetUsersInRoleAsync("Monitor");
        var nietIngeschrevenMonitoren = alleMonitoren.Where(m => !opleiding.Personen.Any(p => p.Id == m.Id)).ToList();

        var viewModel = new OpleidingMonitorToevoegenViewModel
        {
            OpleidingId = opleiding.Id,
            OpleidingNaam = opleiding.Naam,
            BeschikbareMonitoren = nietIngeschrevenMonitoren.Select(m => new MonitorViewModel
            {
                Id = m.Id,
                Naam = $"{m.Voornaam} {m.Naam}",
                Email = m.Email
            }).ToList()
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MonitorToevoegenBevestigen(int opleidingId, int geselecteerdeMonitorId)
    {
        // Verkrijg de opleiding op basis van het ID
        var opleiding = await _uow.OpleidingRepository.GetByIdAsync(opleidingId);
        if (opleiding == null)
        {
            return NotFound();
        }

        // Verkrijg de monitor op basis van het geselecteerde ID
        var monitor = await _userManager.FindByIdAsync(geselecteerdeMonitorId.ToString());
        if (monitor == null)
        {
            return NotFound();
        }

        // Controleer of de monitor al is ingeschreven
        if (opleiding.Personen.Any(p => p.Id == geselecteerdeMonitorId))
        {
            // Monitor is al ingeschreven, geef een boodschap mee via TempData
            TempData["Melding"] = "Deze monitor is al ingeschreven voor deze opleiding.";
            // Monitor is al ingeschreven, redirect naar de lijst van ingeschreven monitoren
            return RedirectToAction("IngeschrevenMonitoren", new { id = opleidingId });
        }

        try
        {
            // Voeg de monitor toe aan de opleiding
            opleiding.Personen.Add(monitor);
            _uow.OpleidingRepository.Update(opleiding);
            await _uow.SaveAsync();

            // Voeg een melding toe voor deze opleiding aan de sessie
            var meldingen = HttpContext.Session.GetObjectFromJson<List<string>>("MeldingenDoorVerantwoordelijke") ?? new List<string>();
            meldingen.Add($"Je bent ingeschreven voor de opleiding: {opleiding.Naam} door een verantwoordelijke.");
            HttpContext.Session.SetObjectAsJson("MeldingenDoorVerantwoordelijke", meldingen);

        }
        catch (DbUpdateException ex)
        {
            // Log het probleem (optioneel) en geef een foutmelding weer aan de gebruiker
            TempData["Foutmelding"] = "Er is een fout opgetreden tijdens het toevoegen van de monitor. Probeer het later opnieuw.";
            ModelState.AddModelError("", "Er is een fout opgetreden tijdens het toevoegen van de monitor. Mogelijk is de monitor al ingeschreven.");
            return RedirectToAction("MonitorToevoegen", new { id = opleidingId });
        }

        // Redirect terug naar de lijst van ingeschreven monitoren
        return RedirectToAction("IngeschrevenMonitoren", new { id = opleidingId });
    }


    #endregion

    #region Monitor Verwijderen door Verantwoordelijke
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> VerwijderMonitor(int opleidingId, int monitorId)
    {
        // Haal de opleiding op basis van het ID
        var opleiding = await _uow.OpleidingRepository.GetByIdWithIncludeAsync(opleidingId, o => o.Personen);
        if (opleiding == null)
        {
            return NotFound();
        }

        // Zoek de monitor in de lijst van personen die aan de opleiding verbonden zijn
        var monitor = opleiding.Personen.FirstOrDefault(p => p.Id == monitorId);
        if (monitor == null)
        {
            TempData["ErrorMessage"] = "Monitor is niet ingeschreven voor deze opleiding.";
            return RedirectToAction("IngeschrevenMonitoren", new { id = opleidingId });
        }

        // Verwijder de monitor uit de lijst
        opleiding.Personen.Remove(monitor);

        // Update de database
        _uow.OpleidingRepository.Update(opleiding);
        await _uow.SaveAsync();

        TempData["SuccessMessage"] = "Monitor succesvol verwijderd uit de opleiding.";
        return RedirectToAction("IngeschrevenMonitoren", new { id = opleidingId });
    }

    #endregion

}


