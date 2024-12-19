using Groepsreizen_team_tet.Services;
using Microsoft.AspNetCore.Authorization;
using Groepsreizen_team_tet.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using System.Security.Claims;
using System.Transactions;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Groepsreizen", controller: "Groepsreis", action: "Beheer")]
public class GroepsreisController : Controller
{
    // Haal de repository en UserManager op via dependency injection.
    // UnitOfWork wordt gebruikt om de database te benaderen, de userManager wordt gebruikt om de huidige gebruiker op te halen.
    private readonly IUnitOfWork _uow;
    private readonly UserManager<CustomUser> _userManager;
    private readonly ILogger<GroepsreisController> _logger;
    private readonly EmailService _emailService;

    // Constructor voor de GroepsreisController met dependency injection voor de UnitOfWork en UserManager
    public GroepsreisController(IUnitOfWork unitOfWork, UserManager<CustomUser> userManager, EmailService emailService, ILogger<GroepsreisController> logger)
    {
        _uow = unitOfWork;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
    }

    #region Index
    // Indexpagina van groepsreizen
    public async Task<IActionResult> Index()
    {
        // Haal filters op uit TempData
        var startDatum = TempData["StartDatum"] != null ? DateTime.Parse(TempData["StartDatum"].ToString()) : (DateTime?)null;
        var eindDatum = TempData["EindDatum"] != null ? DateTime.Parse(TempData["EindDatum"].ToString()) : (DateTime?)null;
        var minLeeftijd = TempData["MinLeeftijd"] != null ? int.Parse(TempData["MinLeeftijd"].ToString()) : (int?)null;
        var maxLeeftijd = TempData["MaxLeeftijd"] != null ? int.Parse(TempData["MaxLeeftijd"].ToString()) : (int?)null;

        TempData.Keep();

        // Haal groepsreizen op
        var groepsreizenQuery = _uow.GroepsreisRepository.Search()
            .Where(g => g.Begindatum > DateTime.Now)
            .Include(g => g.Bestemming)
                .ThenInclude(b => b.Fotos)
            .OrderBy(g => g.Begindatum)
            .AsQueryable();

        // Pas filters toe
        if (startDatum.HasValue)
        {
            groepsreizenQuery = groepsreizenQuery.Where(g => g.Begindatum >= startDatum.Value);
        }
        if (eindDatum.HasValue)
        {
            groepsreizenQuery = groepsreizenQuery.Where(g => g.Einddatum <= eindDatum.Value);
        }
        if (minLeeftijd.HasValue && maxLeeftijd.HasValue)
        {
            groepsreizenQuery = groepsreizenQuery.Where(g =>
                g.Bestemming.MinLeeftijd <= maxLeeftijd &&
                g.Bestemming.MaxLeeftijd >= minLeeftijd
            );
        }

        var groepsreizen = await groepsreizenQuery.ToListAsync();

        var viewModel = new GroepsreisViewModel
        {
            StartDatum = startDatum,
            EindDatum = eindDatum,
            MinLeeftijd = minLeeftijd,
            MaxLeeftijd = maxLeeftijd,
            Groepsreizen = groepsreizen
        };

        return View(viewModel);
    }
    #endregion

    #region FilterReizen
    // Reizen filteren
    // POST: Groepsreis/FilterReizen
    [HttpPost]
    public async Task<IActionResult> FilterReizen(GroepsreisViewModel viewModel)
    {
        // Log de ontvangen filterwaarden
        Console.WriteLine($"StartDatum: {viewModel.StartDatum}, EindDatum: {viewModel.EindDatum}, MinLeeftijd: {viewModel.MinLeeftijd}, MaxLeeftijd: {viewModel.MaxLeeftijd}");

        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }
        if (ModelState.IsValid)
        {
            // Haal groepsreizen op met datumfilters
            var groepsreizenQuery = _uow.GroepsreisRepository.Search()
                .Where(g => g.Begindatum > DateTime.Now)
                .Include(g => g.Bestemming)
                .ThenInclude(b => b.Fotos)
                .OrderBy(g => g.Begindatum)
                .AsQueryable();

            // Filter op start- en einddatum in de database-query
            if (viewModel.StartDatum.HasValue)
            {
                groepsreizenQuery = groepsreizenQuery.Where(groepsreis => groepsreis.Begindatum >= viewModel.StartDatum.Value);
            }
            if (viewModel.EindDatum.HasValue)
            {
                groepsreizenQuery = groepsreizenQuery.Where(groepsreis => groepsreis.Einddatum <= viewModel.EindDatum.Value);
            }

            // Haal de gefilterde groepsreizen op van de database
            var groepsreizen = await groepsreizenQuery.ToListAsync();

            // Pas de leeftijdsfilter toe in het geheugen
            if (viewModel.MinLeeftijd.HasValue && viewModel.MaxLeeftijd.HasValue)
            {
                int minLeeftijd = viewModel.MinLeeftijd.Value;
                int maxLeeftijd = viewModel.MaxLeeftijd.Value;

                // Filter reizen die gedeeltelijk of volledig overlappen met de geselecteerde leeftijdscategorie
                groepsreizen = groepsreizen.Where(reis =>
                    reis.Bestemming.MinLeeftijd <= maxLeeftijd &&
                    reis.Bestemming.MaxLeeftijd >= minLeeftijd
                ).ToList();
            }

            Console.WriteLine($"Filter Leeftijd: Min={viewModel.MinLeeftijd}, Max={viewModel.MaxLeeftijd}");

            foreach (var reis in groepsreizen)
            {
                Console.WriteLine($"Reis ID: {reis.Id}, Leeftijdsbereik: {reis.Bestemming.MinLeeftijd} - {reis.Bestemming.MaxLeeftijd}");
            }

            // Zet de gefilterde resultaten in het viewmodel
            viewModel.Groepsreizen = groepsreizen;

            // Sla de filterwaarden op in TempData
            TempData["StartDatum"] = viewModel.StartDatum;
            TempData["EindDatum"] = viewModel.EindDatum;
            TempData["MinLeeftijd"] = viewModel.MinLeeftijd;
            TempData["MaxLeeftijd"] = viewModel.MaxLeeftijd;

            // Zorg ervoor dat de TempData beschikbaar blijft
            TempData.Keep();
        }

        // Redirect naar de Index-actie om de gefilterde resultaten te tonen
        return RedirectToAction("Index");
    }
    #endregion

    #region ResetFilters
    public IActionResult ResetFilters()
    {
        // Wis de opgeslagen filterwaarden in TempData
        TempData.Remove("StartDatum");
        TempData.Remove("EindDatum");
        TempData.Remove("MinLeeftijd");
        TempData.Remove("MaxLeeftijd");

        // Redirect naar de Index-pagina zonder filters
        return RedirectToAction("Index");
    }
    #endregion

    #region Detail
    [Breadcrumb("Details Groepsreis")]
    public async Task<IActionResult> Detail(int id)
    {
        // Haal de groepsreis op met het opgegeven Id, inclusief gerelateerde data zoals Bestemming en Fotos
        var groepsreis = await _uow.GroepsreisRepository.Search()
        .Where(g => g.Id == id)
        .Include(g => g.Bestemming)
            .ThenInclude(b => b.Fotos)
        .Include(g => g.Programmas)
            .ThenInclude(p => p.Activiteit)
        .Include(g => g.Monitoren)
            .ThenInclude(m => m.Persoon)
        .Include(g => g.Deelnemers)
        .FirstOrDefaultAsync();

        // Controleer of de groepsreis bestaat
        if (groepsreis == null)
        {
            Console.WriteLine($"Groepsreis met Id {id} niet gevonden");
            return NotFound();
        }

        // Controleer de referer header
        var referer = Request.Headers["Referer"].ToString();
        ViewData["Referer"] = referer;

        // Controleer of de gebruiker een verantwoordelijke is
        var isVerantwoordelijke = User.IsInRole("Verantwoordelijke");
        var isHoofdmonitor = User.IsInRole("Hoofdmonitor");

        // Haal de huidige gebruiker op
        var gebruikerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var gebruiker = await _userManager.FindByIdAsync(gebruikerId.ToString());

        // Controleer of de gebruiker de hoofdmonitor is en gekoppeld aan deze groepsreis
        bool isHoofdmonitorIngeschreven = false;
        foreach (var monitor in groepsreis.Monitoren)
        {
            if (monitor.PersoonId == gebruikerId && await _userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor"))
            {
                isHoofdmonitorIngeschreven = true;
                break;
            }
        }

        // Controleer of de gebruiker de monitor is en gekoppeld aan deze groepsreis
        bool isMonitorIngeschreven = false;
        foreach (var monitor in groepsreis.Monitoren)
        {
            if (monitor.PersoonId == gebruikerId && await _userManager.IsInRoleAsync(gebruiker, "Monitor"))
            {
                isMonitorIngeschreven = true;
                break;
            }
        }

        // Stel de IsVolzet eigenschap in
        bool isVolzet = groepsreis.Deelnemers.Count >= groepsreis.Deelnemerslimiet;

        // Maak een ViewModel aan en vul het met de gegevens van de groepsreis
        var viewModel = new GroepsreisDetailViewModel
        {
            Id = groepsreis.Id,
            Begindatum = groepsreis.Begindatum,
            Einddatum = groepsreis.Einddatum,
            Prijs = groepsreis.Prijs,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            BestemmingBeschrijving = groepsreis.Bestemming.Beschrijving,
            MinLeeftijd = groepsreis.Bestemming.MinLeeftijd,
            MaxLeeftijd = groepsreis.Bestemming.MaxLeeftijd,
            IsGeannuleerd = groepsreis.IsGeannuleerd, // Voeg annulatiestatus toe
            // Zet de foto's om naar base64-strings om deze correct weer te geven in de view
            FotoBase64Strings = groepsreis.Bestemming.Fotos
                .Where(f => f.Afbeelding != null)
                .Select(f => $"data:image/png;base64,{Convert.ToBase64String(f.Afbeelding)}")
                .ToList(),
            // Zet de lijst van activiteiten om naar een lijst van ActiviteitDetailViewModel
            Activiteiten = groepsreis.Programmas.Select(p => new ActiviteitDetailViewModel
            {
                Naam = p.Activiteit.Naam,
                Beschrijving = p.Activiteit.Beschrijving
            }).ToList(),

            IsVerantwoordelijke = isVerantwoordelijke,
            IsHoofdmonitor = isHoofdmonitor,
            IsMonitor = isMonitorIngeschreven,
            IsEenHoofdmonitor = isHoofdmonitorIngeschreven,
            IsVolzet = isVolzet
        };

        // Sla filters op in TempData (als deze beschikbaar zijn).
        TempData["StartDatum"] = TempData["StartDatum"];
        TempData["EindDatum"] = TempData["EindDatum"];
        TempData["MinLeeftijd"] = TempData["MinLeeftijd"];
        TempData["MaxLeeftijd"] = TempData["MaxLeeftijd"];

        // Geef het viewmodel door aan de view
        return View(viewModel);
    }
    #endregion

    #region Beheer
    // GET: Groepsreizen/Beheer
    public async Task<IActionResult> Beheer(string filter = "actueel")
    {
        IQueryable<Groepsreis> groepsreizen = _uow.GroepsreisRepository.Search()
        .Include(g => g.Bestemming)
        .Include(g => g.Deelnemers)
        .Include(g => g.Wachtlijst)
        .AsQueryable();

        if (filter == "archiveren")
        {
            groepsreizen = groepsreizen.Where(g => g.IsGeannuleerd || g.Einddatum < DateTime.Today && g.OnkostenIngegeven);
        }
        else if (filter == "onkosten")
        {
            groepsreizen = groepsreizen.Where(g => g.Einddatum < DateTime.Today && !g.OnkostenIngegeven && !g.IsGeannuleerd);
        }
        else // filter == "actueel"
        {
            groepsreizen = groepsreizen.Where(g => g.Einddatum >= DateTime.Today && !g.IsGeannuleerd);
        }

        var groepsreizenList = await groepsreizen.ToListAsync();

        ViewBag.Filter = filter; // Voor gebruik in de view

        var viewModel = new GroepsreisBeheerViewModel
        {
            Groepsreizen = groepsreizenList.Select(g => new GroepsreisBeheerItemViewModel
            {
                Id = g.Id,
                BestemmingNaam = g.Bestemming.Naam,
                BestemmingCode = g.Bestemming.Code,
                Prijs = g.Prijs,
                Begindatum = g.Begindatum,
                Einddatum = g.Einddatum,
                MinLeeftijd = g.Bestemming.MinLeeftijd,
                MaxLeeftijd = g.Bestemming.MaxLeeftijd,
                Deelnemerslimiet = g.Deelnemerslimiet,
                AantalDeelnemers = g.Deelnemers.Count,
                AantalWachtlijst = g.Wachtlijst.Count,
                OnkostenIngegeven = g.OnkostenIngegeven,
                IsKopie = g.IsKopie
            }).ToList(),
            ToonGearchiveerd = filter.ToLower() == "archiveren"
        };

        return View(viewModel);
    }
    #endregion

    #region ValideerOnkosten
    // Actie om de onkosten te valideren
    [HttpPost]
    public async Task<IActionResult> ValideerOnkosten(int id)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"ValideerOnkosten POST foutmelding(en):");
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        // Als het model geldig is, zet de OnkostenIngegeven op true
        if (ModelState.IsValid)
        {
            // Haal de groepsreis op aan de hand van het ID
            var groepsreis = await _uow.GroepsreisRepository.GetByIdAsync(id);

            if (groepsreis == null)
            {
                return NotFound();
            }

            // Zet OnkostenIngegeven op true
            groepsreis.OnkostenIngegeven = true;

            // Update de groepsreis en sla de wijzigingen op
            _uow.GroepsreisRepository.Update(groepsreis);
            await _uow.SaveAsync();

        }
        // Redirect naar de Beheer pagina met de filter op onkosten
        return RedirectToAction("Beheer", new { filter = "onkosten" });
    }
    #endregion

    #region Create

    // GET: Groepsreis/Create
    [Breadcrumb("Groepsreis Aanmaken")]
    public async Task<IActionResult> Create()
    {
        var bestemmingen = await _uow.BestemmingRepository.GetAllAsync();
        var activiteiten = await _uow.ActiviteitRepository.GetAllAsync();

        var viewModel = new GroepsreisCreateViewModel
        {
            Bestemmingen = new SelectList(bestemmingen, "Id", "Naam"),
            Activiteiten = new SelectList(activiteiten, "Id", "Naam"),
        };

        return View(viewModel);
    }

    // POST: Groepsreis/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GroepsreisCreateViewModel viewModel)
    {
        Console.WriteLine("Formulier ontvangen");

        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        // Als het model geldig is, maak een nieuwe groepsreis aan
        if (ModelState.IsValid)
        {
            Console.WriteLine("ModelState is geldig");
            // Haal de bestemming op uit de database
            var bestemming = await _uow.BestemmingRepository.GetByIdAsync(viewModel.BestemmingId);
            if (bestemming == null)
            {
                Console.WriteLine($"Groepsreis met Id {viewModel.Id} niet gevonden");
                return NotFound();
            }

            // Zet MinLeeftijd en MaxLeeftijd voor de nieuwe groepsreis
            viewModel.MinLeeftijd = bestemming.MinLeeftijd;
            viewModel.MaxLeeftijd = bestemming.MaxLeeftijd;

            // Maak een nieuwe groepsreis aan
            var nieuweGroepsreis = new Groepsreis
            {
                Begindatum = viewModel.Begindatum,
                Einddatum = viewModel.Einddatum,
                Prijs = viewModel.Prijs,
                BestemmingId = viewModel.BestemmingId,
                Deelnemerslimiet = viewModel.Deelnemerslimiet,
                Programmas = viewModel.GeselecteerdeActiviteiten.Select(id => new Programma
                {
                    ActiviteitId = id
                }).ToList(),
                Wachtlijst = new List<Wachtlijst>()
            };

            // Voeg een foto toe aan de bestemming als er een foto is geüpload
            if (viewModel.FotoBestand != null && viewModel.FotoBestand.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await viewModel.FotoBestand.CopyToAsync(memoryStream);
                    var nieuweFoto = new Foto
                    {
                        Naam = viewModel.FotoBestand.FileName,
                        Afbeelding = memoryStream.ToArray(),
                        BestemmingId = viewModel.BestemmingId
                    };

                    nieuweGroepsreis.Bestemming.Fotos.Add(nieuweFoto);
                }
            }

            // Voeg de groepsreis toe aan de database
            _uow.GroepsreisRepository.Create(nieuweGroepsreis);
            await _uow.SaveAsync();

            // Redirect naar de beheerpagina na het aanmaken van de groepsreis
            return RedirectToAction(nameof(Beheer));
        }

        // Als het model niet geldig is, herlaad de bestemmingen en activiteiten
        return View(viewModel);
    }
    #endregion

    #region Edit : GET
    // GET: Groepsreis/Edit/5
    [Breadcrumb("Groepsreis Bewerken")]
    public async Task<IActionResult> Edit(int id)
    {
        // Haal de groepsreis op aan de hand van het id
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Programmas)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            Console.WriteLine($"Groepsreis met ID {id} niet gevonden");
            return NotFound();
        }

        // Haal bestemmingen en activiteiten op
        var bestemmingen = await _uow.BestemmingRepository.GetAllAsync();
        var activiteiten = await _uow.ActiviteitRepository.GetAllAsync();

        // Maak het ViewModel aan met de gegevens van de groepsreis
        var viewModel = new GroepsreisEditViewModel
        {
            Id = groepsreis.Id,
            Begindatum = groepsreis.Begindatum,
            Einddatum = groepsreis.Einddatum,
            Prijs = groepsreis.Prijs,
            BestemmingId = groepsreis.BestemmingId,
            Deelnemerslimiet = groepsreis.Deelnemerslimiet,
            GeselecteerdeActiviteiten = groepsreis.Programmas.Select(p => p.ActiviteitId).ToList(),
            Bestemmingen = new SelectList(bestemmingen, "Id", "Naam", groepsreis.BestemmingId),
            Activiteiten = new SelectList(activiteiten, "Id", "Naam")
        };

        // Geef het ViewModel door aan de View
        return View(viewModel);
    }
    #endregion

    #region Edit : POST
    // POST: Groepsreis/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GroepsreisEditViewModel viewModel)
    {
        Console.WriteLine("Formulier ontvangen");

        // Controleer of het ID overeenkomt met de groepsreis die we willen bewerken
        if (id != viewModel.Id)
        {
            Console.WriteLine($"Fout: Id {id} komt niet overeen met het model Id {viewModel.Id}");
            return NotFound();
        }

        // Controleer de validatie van het model
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validatiefout in {foutmelding}: {error.ErrorMessage}");
                }
            }

            // Herlaad bestemmingen en activiteiten als het model niet geldig is
            viewModel.Bestemmingen = new SelectList(await _uow.BestemmingRepository.GetAllAsync(), "Id", "Naam", viewModel.BestemmingId);
            viewModel.Activiteiten = new SelectList(await _uow.ActiviteitRepository.GetAllAsync(), "Id", "Naam", viewModel.GeselecteerdeActiviteiten);
            return View(viewModel);
        }

        // Als het model geldig is, bewerk de groepsreis
        if (ModelState.IsValid)
        {
            // Haal de bestemming op uit de database
            var bestemming = await _uow.BestemmingRepository.GetByIdAsync(viewModel.BestemmingId);
            if (bestemming == null)
            {
                return NotFound();
            }

            // Zoek de groepsreis en de bijbehorende bestemming
            var groepsreis = await _uow.GroepsreisRepository
                .Search()
                .Include(g => g.Bestemming)
                .Include(g => g.Programmas)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (groepsreis == null)
            {
                Console.WriteLine($"Groepsreis met ID {id} niet gevonden");
                return NotFound();
            }

            // Werk de groepsreis bij met de nieuwe of bestaande gegevens uit het ViewModel
            groepsreis.Begindatum = viewModel.Begindatum;
            groepsreis.Einddatum = viewModel.Einddatum;
            groepsreis.Prijs = viewModel.Prijs;
            groepsreis.Deelnemerslimiet = viewModel.Deelnemerslimiet;

            // Controleer of de bestemming is gewijzigd en update indien nodig
            if (groepsreis.BestemmingId != viewModel.BestemmingId)
            {
                groepsreis.BestemmingId = viewModel.BestemmingId;
                Console.WriteLine("Bestemming bijgewerkt");
            }

            // Werk de activiteiten bij (wis de oude en voeg de nieuwe toe)
            groepsreis.Programmas.Clear();
            groepsreis.Programmas = viewModel.GeselecteerdeActiviteiten.Select(activiteitId => new Programma
            {
                ActiviteitId = activiteitId,
                GroepsreisId = groepsreis.Id
            }).ToList();

            try
            {
                // Wijzigingen opslaan
                _uow.GroepsreisRepository.Update(groepsreis);
                await _uow.SaveAsync();
                Console.WriteLine("Wijzigingen succesvol opgeslagen.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency-exceptie: {ex.Message}");
                if (!await GroepsreisExists(viewModel.Id))
                {
                    Console.WriteLine($"Groepsreis met ID {viewModel.Id} bestaat niet meer.");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // Ga terug naar de beheerpagina na het bewerken van de groepsreis
        return RedirectToAction(nameof(Beheer));
    }
    #endregion

    #region Helpermethode: Groepsreis bestaat
    // Helpermethode om te controleren of de groepsreis bestaat
    private async Task<bool> GroepsreisExists(int id)
    {
        return await _uow.GroepsreisRepository.GetByIdAsync(id) != null;
    }
    #endregion

    #region Delete
    // GET: Groepsreis/Delete/5
    [Breadcrumb("Groepsreis Verwijderen")]
    public async Task<IActionResult> Delete(int id)
    {
        // Haal de groepsreis op die verwijderd moet worden
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Bestemming)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Maak een ViewModel voor de bevestigingspagina
        var viewModel = new GroepsreisDeleteViewModel
        {
            Id = groepsreis.Id,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            Prijs = groepsreis.Prijs,
            Begindatum = groepsreis.Begindatum,
            Einddatum = groepsreis.Einddatum
        };

        // Geef de ViewModel door aan de view
        return View(viewModel);
    }

    // POST: Groepsreis/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        // Als het model geldig is, verwijder de groepsreis
        if (ModelState.IsValid)
        {
            var groepsreis = await _uow.GroepsreisRepository.GetByIdAsync(id);
            if (groepsreis == null)
            {
                return NotFound();
            }

            // Verwijder de groepsreis uit de repository
            _uow.GroepsreisRepository.Delete(groepsreis);
            await _uow.SaveAsync();
        }

        // Ga terug naar de beheerpagina na verwijderen
        return RedirectToAction(nameof(Beheer));
    }
    #endregion

    #region BeheerDeelnemers : GET
    // GET: Groepsreis/BeheerDeelnemers/5
    [Breadcrumb("Deelnemers Beheren")]
    public async Task<IActionResult> BeheerDeelnemers(int id, string? ouderNaamFilter = null)
    {
        // Haal de groepsreis op inclusief bestemming en huidige deelnemers
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Bestemming)
            .Include(g => g.Deelnemers)
                .ThenInclude(d => d.Kind)
            .Include(g => g.Wachtlijst)
                .ThenInclude(w => w.Kind)
                    .ThenInclude(k => k.Ouder)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Haal alle kinderen op die nog niet ingeschreven zijn
        var huidigeKindIds = groepsreis.Deelnemers.Select(d => d.KindId).ToList();
        var wachtlijstKindIds = groepsreis.Wachtlijst.Select(w => w.KindId).ToList();
        var beschikbareKinderen = new List<Kind>();

        // Controleer of er kinderen beschikbaar zijn
        if (!string.IsNullOrEmpty(ouderNaamFilter))
        {
            var beschikbareKinderenQuery = _uow.KindRepository.Search()
                .Include(k => k.Ouder)
                .Where(k => !huidigeKindIds.Contains(k.Id) && !wachtlijstKindIds.Contains(k.Id));

            ouderNaamFilter = ouderNaamFilter.ToLower();
            beschikbareKinderenQuery = beschikbareKinderenQuery.Where(k =>
                k.Ouder.Voornaam.ToLower().Contains(ouderNaamFilter) ||
                k.Ouder.Naam.ToLower().Contains(ouderNaamFilter));

            beschikbareKinderen = await beschikbareKinderenQuery.ToListAsync();
        }

        // Zorg ervoor dat AlleKinderen een lege lijst is als er geen kinderen zijn
        var beschikbareKinderenSelectList = (beschikbareKinderen ?? new List<Kind>())
            .Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = $"{k.Voornaam} {k.Naam}, Leeftijd: {DateTime.Now.Year - k.Geboortedatum.Year - (DateTime.Now < k.Geboortedatum.AddYears(DateTime.Now.Year - k.Geboortedatum.Year) ? 1 : 0)} jaar (Ouder/voogd: {k.Ouder.Voornaam} {k.Ouder.Naam})"
            }).ToList();

        // Haal de wachtlijst deelnemers op
        var wachtlijstDeelnemers = groepsreis.Wachtlijst
            .Select(w => new GroepsreisDeelnemerViewModel
            {
                Id = w.Id,
                KindNaam = $"{w.Kind.Voornaam} {w.Kind.Naam}",
                Opmerkingen = w.Opmerkingen
            }).ToList();

        var huidigeDeelnemers = groepsreis.Deelnemers.Select(d => new DeelnemerBeheerGroepsreisViewModel
        {
            Id = d.Id,
            KindNaam = $"{d.Kind.Voornaam} {d.Kind.Naam}",
            Opmerkingen = d.Opmerkingen
        }).ToList();

        var viewModel = new GroepsreisDeelnemersViewModel
        {
            GroepsreisId = groepsreis.Id,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            HuidigeDeelnemers = huidigeDeelnemers,
            WachtlijstDeelnemers = wachtlijstDeelnemers,
            Deelnemerslimiet = groepsreis.Deelnemerslimiet,
            AlleKinderen = beschikbareKinderenSelectList,
            OuderNaamFilter = ouderNaamFilter
        };

        return View(viewModel);
    }
    #endregion

    #region BeheerDeelnemers : POST
    // POST: Groepsreis/BeheerDeelnemers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BeheerDeelnemers(GroepsreisDeelnemersViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // Log de foutmeldingen
            Console.WriteLine("ModelState is ongeldig.");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Fout: {foutmelding}: {error.ErrorMessage}");
                }
            }
            return View(viewModel);  // Toon de view opnieuw bij een fout
        }

        // Haal de groepsreis op inclusief de huidige deelnemers
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Deelnemers)
            .ThenInclude(d => d.Kind)
            .FirstOrDefaultAsync(g => g.Id == viewModel.GroepsreisId);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Haal de geselecteerde kinderen op uit het formulier
        var geselecteerdeKinderenIds = viewModel.GeselecteerdeKinderenIds ?? new List<int>();

        // Voeg geselecteerde kinderen toe als nieuwe deelnemers
        foreach (var kindId in geselecteerdeKinderenIds)
        {
            // Voeg alleen toe als het kind nog niet ingeschreven is
            if (!groepsreis.Deelnemers.Any(d => d.KindId == kindId))
            {
                var nieuweDeelnemer = new Deelnemer
                {
                    KindId = kindId,
                    GroepsreisDetailsId = groepsreis.Id
                };
                _uow.DeelnemerRepository.Create(nieuweDeelnemer);
            }
        }

        // Sla de wijzigingen op
        await _uow.SaveAsync();

        // Voeg een succesbericht toe
        TempData["SuccessMessage"] = "De geselecteerde deelnemers zijn toegevoegd.";

        // Herlaad de groepsreis inclusief de huidige deelnemers
        return RedirectToAction(nameof(BeheerDeelnemers), new { id = viewModel.GroepsreisId, ouderNaamFilter = viewModel.OuderNaamFilter });
    }
    #endregion

    #region Beschikbare monitoren ophalen : GET

    private async Task<List<Models.Monitor>> GetBeschikbareMonitoren(DateTime startDatum, DateTime eindDatum)
    {
        // Haal alle gebruikers met de rol "Monitor" op
        var gebruikersInRol = await _userManager.GetUsersInRoleAsync("monitor".ToLower());

        // Selecteer de monitoren van deze gebruikers
        var alleMonitoren = await _uow.MonitorRepository.Search()
            .Include(m => m.Persoon)
            .Where(m => gebruikersInRol.Select(u => u.Id).Contains(m.PersoonId))
            .ToListAsync();

        // Filter monitoren die geen groepsreis hebben binnen de periode
        var beschikbareMonitoren = alleMonitoren
            .Where(m => m.GroepsReis == null ||
                        m.GroepsReis.Begindatum > eindDatum ||
                        m.GroepsReis.Einddatum < startDatum)
            .ToList();

        return beschikbareMonitoren;
    }
    #endregion

    #region Monitoren beheren ophalen : GET
    // GET: Groepsreis/BeheerMonitoren/5
    [Breadcrumb("Monitoren Beheren")]
    public async Task<IActionResult> BeheerMonitoren(int id)
    {
        // Haal de groepsreis en huidige monitoren op
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Monitoren)
                .ThenInclude(m => m.Persoon)
            .Include(g => g.Bestemming)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Haal alle gebruikers op
        var alleGebruikers = await _userManager.Users.ToListAsync();

        // Filter beschikbare monitoren
        var beschikbareMonitoren = alleGebruikers
            .Where(gebruiker =>
            {
                // Controleer of de gebruiker de rol "Monitor" of "Hoofdmonitor" heeft
                if (!_userManager.IsInRoleAsync(gebruiker, "Monitor").Result && !_userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor").Result)
                {
                    return false;
                }

                // Controleer of de monitor zich kandidaat gesteld heeft voor deze groepsreis
                var isReedsKandidaat = _uow.MonitorRepository.Search()
                    .Any(m => m.PersoonId == gebruiker.Id && m.GroepsreisDetailsId == id);
                if (isReedsKandidaat)
                {
                    return false;
                }

                // Controleer de leeftijd van de monitor
                var leeftijd = DateTime.Now.Year - gebruiker.Geboortedatum.Year;
                if (DateTime.Now < gebruiker.Geboortedatum.AddYears(leeftijd))
                {
                    leeftijd--;
                }

                return leeftijd >= groepsreis.Bestemming.MinLeeftijd;
            })
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Voornaam} {m.Naam} (Leeftijd: {DateTime.Now.Year - m.Geboortedatum.Year})"
            })
            .ToList();

        // Filter beschikbare hoofdmonitoren
        var beschikbareHoofdmonitoren = alleGebruikers
            .Where(gebruiker =>
            {
                // Controleer of de gebruiker de rol "Hoofdmonitor" heeft
                if (!_userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor").Result)
                {
                    return false;
                }

                // Controleer de leeftijd van de hoofdmonitoren
                var leeftijd = DateTime.Now.Year - gebruiker.Geboortedatum.Year;
                if (DateTime.Now < gebruiker.Geboortedatum.AddYears(leeftijd))
                {
                    leeftijd--;
                }

                return leeftijd >= groepsreis.Bestemming.MinLeeftijd;
            })
            .Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = $"{h.Voornaam} {h.Naam} (Leeftijd: {DateTime.Now.Year - h.Geboortedatum.Year})"
            })
            .ToList();

        var viewModel = new GroepsreisMonitorenViewModel
        {
            GroepsreisId = groepsreis.Id,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            HuidigeMonitoren = groepsreis.Monitoren.ToList(),
            BeschikbareMonitoren = beschikbareMonitoren,
            BeschikbareHoofdmonitoren = beschikbareHoofdmonitoren,
            GeselecteerdeMonitorenIds = groepsreis.Monitoren.Select(m => m.Id).ToList(),
            GeselecteerdeHoofdmonitorId = null
        };

        return View(viewModel);
    }
    #endregion

    #region Monitoren beheheren uitvoeren : POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BeheerMonitoren(GroepsreisMonitorenViewModel viewModel)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        if (ModelState.IsValid)
        {
            // Haal de groepsreis op inclusief de huidige monitoren
            var groepsreis = await _uow.GroepsreisRepository.Search()
                .Include(g => g.Monitoren)
                    .ThenInclude(m => m.Persoon)
                .FirstOrDefaultAsync(g => g.Id == viewModel.GroepsreisId);

            if (groepsreis == null)
            {
                TempData["ErrorMessage"] = "Groepsreis niet gevonden.";
                return RedirectToAction(nameof(Beheer));
            }

            // Verkrijg geselecteerde monitoren-IDs
            var geselecteerdeMonitorenIds = viewModel.GeselecteerdeMonitorenIds ?? new List<int>();

            // Voeg monitoren toe die nog niet in de groepsreis staan
            var geselecteerdeMonitoren = await _uow.MonitorRepository.Search()
                .Include(m => m.Persoon)
                .Where(m => geselecteerdeMonitorenIds.Contains(m.PersoonId))
                .ToListAsync();

            foreach (var monitor in geselecteerdeMonitoren)
            {
                if (!groepsreis.Monitoren.Any(m => m.PersoonId == monitor.PersoonId))
                {
                    Console.WriteLine($"Toevoegen Monitor ID: {monitor.PersoonId}");
                    monitor.GroepsreisDetailsId = groepsreis.Id;
                    groepsreis.Monitoren.Add(monitor);
                }
            }

            // Stel hoofdmonitor in
            Console.WriteLine($"Ontvangen Hoofdmonitor ID: {viewModel.GeselecteerdeHoofdmonitorId}");
            if (viewModel.GeselecteerdeHoofdmonitorId.HasValue)
            {
                // Haal de geselecteerde hoofdmonitor op uit de database
                var hoofdmonitorUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == viewModel.GeselecteerdeHoofdmonitorId.Value);

                if (hoofdmonitorUser == null)
                {
                    TempData["ErrorMessage"] = "De geselecteerde hoofdmonitor bestaat niet.";
                    return RedirectToAction(nameof(BeheerMonitoren), new { id = viewModel.GroepsreisId });
                }

                // Controleer of de hoofdmonitor al in de groepsreis zit
                var nieuweHoofdmonitor = groepsreis.Monitoren.FirstOrDefault(m => m.PersoonId == hoofdmonitorUser.Id);

                // Maak een nieuwe hoofdmonitor als deze nog niet bestaat
                nieuweHoofdmonitor = new Models.Monitor
                {
                    PersoonId = hoofdmonitorUser.Id,
                    GroepsreisDetailsId = groepsreis.Id,
                    IsHoofdMonitor = 1
                };
                groepsreis.Monitoren.Add(nieuweHoofdmonitor);
                Console.WriteLine($"Hoofdmonitor toegevoegd: {hoofdmonitorUser.Voornaam} {hoofdmonitorUser.Naam}");
            }

            // Sla wijzigingen op in de database
            await _uow.SaveAsync();
        }

        // Redirect terug naar de beheerpagina
        return RedirectToAction(nameof(BeheerMonitoren), new { id = viewModel.GroepsreisId });
    }
    #endregion

    #region VerwijderMonitor : POST
    // POST: Groepsreis/VerwijderMonitor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerwijderMonitor(int monitorId)
    {
        try
        {
            // Haal de monitor op
            var monitor = await _uow.MonitorRepository.Search()
                .FirstOrDefaultAsync(m => m.Id == monitorId);

            // Controleer of de monitor bestaat en of deze is gekoppeld aan een groepsreis
            if (monitor == null || monitor.GroepsreisDetailsId == null)
            {
                return RedirectToAction(nameof(Beheer));
            }

            // Verwijder de de monitor uit de groepsreis
            var groepsreisId = monitor.GroepsreisDetailsId.Value;
            monitor.GroepsreisDetailsId = null;
            monitor.IsHoofdMonitor = 0;

            await _uow.SaveAsync();

            return RedirectToAction(nameof(BeheerMonitoren), new { id = groepsreisId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij het verwijderen van de (hoofd)monitor: {ex.Message}");
            return RedirectToAction(nameof(Beheer));
        }
    }
    #endregion

    #region Inschrijving aanmaken ophalen : GET
    // GET: Inschrijving/Create
    [HttpGet]
    public async Task<IActionResult> Inschrijven(int groepsreisId)
    {
        // Haal de originele groepsreis op inclusief bestemming en wachtlijst
        var origineleGroepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Bestemming)
            .Include(g => g.Deelnemers)
                .ThenInclude(d => d.Kind)
            .Include(g => g.Wachtlijst)
                .ThenInclude(w => w.Kind)
            .FirstOrDefaultAsync(g => g.Id == groepsreisId);

        if (origineleGroepsreis == null)
        {
            return NotFound();
        }

        // Haal alle kopieën van de originele groepsreis op
        var kopieën = await _uow.GroepsreisRepository.Search()
            .Where(g => g.OrigineleGroepsreisId == groepsreisId)
            .Include(g => g.Deelnemers)
                .ThenInclude(d => d.Kind)
            .ToListAsync();

        // Combineer de originele groepsreis met de kopieën
        var alleGroepsreizen = new List<Groepsreis> { origineleGroepsreis };
        alleGroepsreizen.AddRange(kopieën);

        // Haal de leeftijdscategorie van de originele groepsreis op
        var minLeeftijd = origineleGroepsreis.Bestemming.MinLeeftijd;
        var maxLeeftijd = origineleGroepsreis.Bestemming.MaxLeeftijd;

        // Haal de huidige gebruiker op
        var gebruikerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var gebruiker = await _userManager.FindByIdAsync(gebruikerId.ToString());

        if (gebruiker == null)
        {
            return NotFound();
        }

        // Haal alle kinderen van de gebruiker op die voldoen aan de leeftijdscategorie
        var beschikbareKinderen = await _uow.KindRepository.Search()
            .Where(k => k.PersoonId == gebruikerId &&
                        DateTime.Now.Year - k.Geboortedatum.Year >= minLeeftijd &&
                        DateTime.Now.Year - k.Geboortedatum.Year <= maxLeeftijd)
            .ToListAsync();

        // Haal de kinderen die al zijn ingeschreven voor de groepsreis en kopieën
        var ingeschrevenKinderenIds = alleGroepsreizen
            .SelectMany(g => g.Deelnemers)
            .Select(d => d.KindId)
            .Distinct()
            .ToList();

        // Haal de kinderen op die op de wachtlijst staan voor de originele groepsreis
        var wachtlijstKinderenIds = origineleGroepsreis.Wachtlijst.Select(w => w.KindId).ToList();

        // Filter de beschikbare kinderen zodat reeds ingeschreven of op wachtlijst staande kinderen worden uitgesloten
        var gefilterdeBeschikbareKinderen = beschikbareKinderen
            .Where(k => !ingeschrevenKinderenIds.Contains(k.Id) && !wachtlijstKinderenIds.Contains(k.Id))
            .ToList();

        // Haal de kinderen op die op de wachtlijst staan
        var wachtlijstKinderen = origineleGroepsreis.Wachtlijst
            .Select(w => new KindViewModel
            {
                Id = w.Kind.Id,
                Voornaam = w.Kind.Voornaam,
                Naam = w.Kind.Naam,
                Leeftijd = DateTime.Now.Year - w.Kind.Geboortedatum.Year -
                          (DateTime.Now < w.Kind.Geboortedatum.AddYears(DateTime.Now.Year - w.Kind.Geboortedatum.Year) ? 1 : 0)
            })
            .ToList();

        // Maak het viewmodel met de beschikbare, ingeschreven en wachtlijst kinderen
        var viewModel = new GroepsreisInschrijvenViewModel
        {
            GroepsreisId = groepsreisId,
            GroepsreisNaam = origineleGroepsreis.Bestemming.Naam,
            BeschikbareKinderen = gefilterdeBeschikbareKinderen
                .Select(k => new KindViewModel
                {
                    Id = k.Id,
                    Voornaam = k.Voornaam,
                    Naam = k.Naam,
                    Leeftijd = DateTime.Today.Year - k.Geboortedatum.Year -
                              (DateTime.Today < k.Geboortedatum.AddYears(DateTime.Today.Year - k.Geboortedatum.Year) ? 1 : 0)
                }).ToList(),
            IngeschrevenKinderen = alleGroepsreizen
                .SelectMany(g => g.Deelnemers)
                .Select(d => d.Kind)
                .Distinct()
                .ToList()
                .Select(k => new KindViewModel
                {
                    Id = k.Id,
                    Voornaam = k.Voornaam,
                    Naam = k.Naam,
                    Leeftijd = DateTime.Today.Year - k.Geboortedatum.Year -
                              (DateTime.Today < k.Geboortedatum.AddYears(DateTime.Today.Year - k.Geboortedatum.Year) ? 1 : 0)
                })
                .ToList(),
            WachtlijstKinderen = wachtlijstKinderen
        };

        return View(viewModel);
    }
    #endregion

    #region Inschrijving aanmaken uitvoeren : POST
    // POST: Inschrijving/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inschrijven(GroepsreisInschrijvenViewModel viewModel)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // Controleer de rol en verander deze naar "Deelnemer" indien het geen deelnemer is
        if (!await _userManager.IsInRoleAsync(user, "Deelnemer"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Gebruiker");
            await _userManager.AddToRoleAsync(user, "Deelnemer");
        }

        // Haal de groepsreis op inclusief deelnemers en limiet
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Deelnemers)
            .Include(g => g.Wachtlijst)
            .FirstOrDefaultAsync(g => g.Id == viewModel.GroepsreisId);

        if (groepsreis == null)
        {
            TempData["ErrorMessage"] = "Groepsreis niet gevonden.";
            return RedirectToAction("Index", "Groepsreis");
        }

        // Controleer of de groepsreis al is begonnen
        if (groepsreis.Begindatum <= DateTime.Now)
        {
            TempData["ErrorMessage"] = "De groepsreis is al begonnen.";
            return RedirectToAction("Index", "Groepsreis");
        }
        // Haal de geselecteerde kinderen op uit het formulier
        var geselecteerdeKinderenIds = viewModel.GeselecteerdeKinderenIds ?? new List<int>();

        // Initializeer een lokale variabele voor het huidige aantal deelnemers
        int huidigeDeelnemers = groepsreis.Deelnemers.Count;
        Console.WriteLine($"Huidige deelnemers voor groepsreis ID {viewModel.GroepsreisId}: {huidigeDeelnemers} / Limiet: {groepsreis.Deelnemerslimiet}");

        // Voeg geselecteerde kinderen toe als nieuwe deelnemers
        foreach (var kindId in geselecteerdeKinderenIds)
        {
            // Controleer of het kind al is ingeschreven of op de wachtlijst staat
            var bestaandeInschrijving = await _uow.DeelnemerRepository.Search()
                .AnyAsync(d => d.KindId == kindId && d.GroepsreisDetailsId == viewModel.GroepsreisId);

            var staatOpWachtlijst = await _uow.WachtlijstRepository.Search()
                .AnyAsync(w => w.KindId == kindId && w.GroepsreisId == viewModel.GroepsreisId);

            if (bestaandeInschrijving || staatOpWachtlijst)
            {
                Console.WriteLine($"KindId {kindId} is al ingeschreven of op wachtlijst.");
                continue; // Overslaan als het kind al is ingeschreven of op de wachtlijst staat
            }

            if (huidigeDeelnemers < groepsreis.Deelnemerslimiet)
            {
                // Voeg toe als deelnemer
                var deelnemer = new Deelnemer
                {
                    KindId = kindId,
                    GroepsreisDetailsId = viewModel.GroepsreisId,
                    InschrijvingDatum = DateTime.Now,
                    Opmerkingen = viewModel.Opmerkingen
                };
                _uow.DeelnemerRepository.Create(deelnemer);
                groepsreis.Deelnemers.Add(deelnemer);
                huidigeDeelnemers++; // Verhoog het aantal deelnemers
                Console.WriteLine($"Toegevoegd als deelnemer: KindId {kindId}, Huidige deelnemers: {huidigeDeelnemers}");
            }
            else
            {
                // Voeg toe aan wachtlijst
                var wachtlijstEntry = new Wachtlijst
                {
                    GroepsreisId = viewModel.GroepsreisId,
                    KindId = kindId,
                    InschrijvingDatum = DateTime.Now,
                    Opmerkingen = viewModel.Opmerkingen
                };
                _uow.WachtlijstRepository.Create(wachtlijstEntry);
                Console.WriteLine($"Toegevoegd aan wachtlijst: KindId {kindId}");
            }
        }

        await _uow.SaveAsync();

        TempData["SuccessMessage"] = "De inschrijving is succesvol afgerond!";
        return RedirectToAction("Index", "Groepsreis");
    }
    #endregion

    #region Kindidaat stellen als monitor uitvoeren : POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KandidaatStellenAlsMonitor(int groepsreisId)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        // Als het model geldig is
        if (ModelState.IsValid)
        {
            // Haal de huidige gebruiker op
            var gebruikerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var gebruiker = await _userManager.FindByIdAsync(gebruikerId.ToString());

            if (gebruiker == null)
            {
                TempData["ErrorMessage"] = "Gebruiker niet gevonden.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of de gebruiker de juiste rol heeft
            if (!await _userManager.IsInRoleAsync(gebruiker, "Monitor") && !await _userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor"))
            {
                TempData["ErrorMessage"] = "Je hebt niet de juiste bevoegdheid om je kandidaat te stellen als monitor.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Haal de groepsreis op
            var groepsreis = await _uow.GroepsreisRepository.Search()
                .Include(g => g.Bestemming)
                .Include(g => g.Monitoren)
                .FirstOrDefaultAsync(g => g.Id == groepsreisId);

            if (groepsreis == null)
            {
                TempData["ErrorMessage"] = "Groepsreis niet gevonden.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of de gebruiker al is ingeschreven voor deze groepsreis
            if (groepsreis.Monitoren.Any(m => m.PersoonId == gebruikerId))
            {
                TempData["ErrorMessage"] = "Je bent al ingeschreven als monitor voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer op overlappende groepsreizen
            var bestaandeGroepsreizen = await _uow.GroepsreisRepository.Search()
                .Where(g => g.Monitoren.Any(m => m.PersoonId == gebruikerId) && g.Id != groepsreisId)
                .ToListAsync();

            if (bestaandeGroepsreizen.Any(g =>
                groepsreis.Begindatum < g.Einddatum && groepsreis.Einddatum > g.Begindatum))
            {
                TempData["ErrorMessage"] = "Je bent al gekoppeld aan een andere groepsreis waarvan de periode overlapt met " +
                    $"de groepsreis naar {groepsreis.Bestemming.Naam} die door gaat van {groepsreis.Begindatum.ToString()} tot en met {groepsreis.Einddatum.ToString()}";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Bereken leeftijd van de gebruiker
            var geboortedatum = gebruiker.Geboortedatum;
            var leeftijd = DateTime.Now.Year - geboortedatum.Year;
            if (DateTime.Now < geboortedatum.AddYears(leeftijd))
            {
                leeftijd--;
            }

            // Controleer of de gebruiker oud genoeg is
            if (leeftijd < groepsreis.Bestemming.MinLeeftijd)
            {
                TempData["ErrorMessage"] = $"De minimumleeftijd voor deze groepsreis bedraagt {groepsreis.Bestemming.MinLeeftijd} jaar. " +
                                           $"\n Je bent helaas niet oud genoeg om je als monitor kandidaat te stellen voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Voeg de gebruiker toe aan de groepsreis als monitor
            var monitor = new Models.Monitor
            {
                PersoonId = gebruikerId,
                GroepsreisDetailsId = groepsreisId,
                IsHoofdMonitor = 0
            };

            try
            {
                _uow.MonitorRepository.Create(monitor);
                await _uow.SaveAsync();
                TempData["SuccessMessage"] = "Je kandidatuur als monitor is goed ontvangen.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Er is een fout opgetreden: {ex.InnerException?.Message ?? ex.Message}");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het opslaan. Probeer het later opnieuw.";
            }
        }

        return RedirectToAction("Detail", new { id = groepsreisId });

    }
    #endregion

    #region Kindidaat stellen als hoofdmonitor uitvoeren : POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KandidaatStellenAlsHoofdmonitor(int groepsreisId)
    {
        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is ongeldig. Validatie fouten:");
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in {foutmelding}: {error.ErrorMessage}");
                }
            }
        }

        if (ModelState.IsValid)
        {
            // Haal de huidige gebruiker op
            var gebruikerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var gebruiker = await _userManager.FindByIdAsync(gebruikerId.ToString());

            if (gebruiker == null)
            {
                TempData["ErrorMessage"] = "Gebruiker niet gevonden.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Haal de groepsreis op
            var groepsreis = await _uow.GroepsreisRepository.Search()
                .Include(g => g.Bestemming)
                .Include(g => g.Monitoren)
                .FirstOrDefaultAsync(g => g.Id == groepsreisId);

            if (groepsreis == null)
            {
                TempData["ErrorMessage"] = "Groepsreis niet gevonden.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of de gebruiker de juiste rol heeft
            if (!await _userManager.IsInRoleAsync(gebruiker, "Hoofdmonitor"))
            {
                TempData["ErrorMessage"] = "Je hebt niet de juiste bevoegdheid om je kandidaat te stellen als hoofdmonitor.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of de gebriuker al is ingeschreven als hoofdmonitor voor deze groepsreis
            if (groepsreis.Monitoren.Any(m => m.PersoonId == gebruikerId))
            {
                TempData["ErrorMessage"] = "Je bent al ingeschreven als hoofdmonitor voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of de gebruiker al is ingeschreven als monitor
            if (groepsreis.Monitoren.Any(m => m.PersoonId == gebruikerId))
            {
                TempData["ErrorMessage"] = "Je bent al ingeschreven als monitor voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer of er al een andere hoofdmonitor is ingeschreven
            if (groepsreis.Monitoren.Any(m => m.IsHoofdMonitor == 1))
            {
                TempData["ErrorMessage"] = "Er is reeds een kandidaat hoofdmonitor voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Controleer op overlappende groepsreizen
            var bestaandeGroepsreizen = await _uow.GroepsreisRepository.Search()
                .Where(g => g.Monitoren.Any(m => m.PersoonId == gebruikerId) && g.Id != groepsreisId)
                .ToListAsync();

            if (bestaandeGroepsreizen.Any(g =>
                groepsreis.Begindatum < g.Einddatum && groepsreis.Einddatum > g.Begindatum))
            {
                TempData["ErrorMessage"] = "Je bent al gekoppeld aan een andere groepsreis waarvan de periode overlapt met " +
                    $"de groepsreis naar {groepsreis.Bestemming.Naam} die door gaat van {groepsreis.Begindatum.ToString()} tot en met {groepsreis.Einddatum.ToString()}";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Bereken leeftijd van de gebruiker
            var geboortedatum = gebruiker.Geboortedatum;
            var leeftijd = DateTime.Now.Year - geboortedatum.Year;
            if (DateTime.Now < geboortedatum.AddYears(leeftijd))
            {
                leeftijd--;
            }

            // Controleer of de gebruiker oud genoeg is
            if (leeftijd < groepsreis.Bestemming.MinLeeftijd)
            {
                TempData["ErrorMessage"] = $"De minimumleeftijd voor deze groepsreis bedraagt {groepsreis.Bestemming.MinLeeftijd} jaar. " +
                                           $"\n Je bent helaas niet oud genoeg om je als hoofdmonitor kandidaat te stellen voor deze groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }

            // Voeg de gebruiker toe aan de groepsreis als hoofdmonitor
            var hoofdmonitor = new Models.Monitor
            {
                PersoonId = gebruikerId,
                GroepsreisDetailsId = groepsreisId,
                IsHoofdMonitor = 1
            };

            try
            {
                _uow.MonitorRepository.Create(hoofdmonitor);
                await _uow.SaveAsync();
                TempData["SuccessMessage"] = "Je kandidatuur als hoofdmonitor is goed ontvangen.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Er is een fout opgetreden: {ex.InnerException?.Message ?? ex.Message}");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het opslaan. Probeer het later opnieuw.";
            }

        }
        return RedirectToAction("Detail", new { id = groepsreisId });
    }
    #endregion

    #region Deelnemerslijst ophalen : GET
    // GET: Groepsreis/LijstDeelnemers/5
    public async Task<IActionResult> LijstDeelnemers(int id)
    {
        // Haal de groepsreis op met de deelnemers en hun details
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Deelnemers)
                .ThenInclude(d => d.Kind)
                    .ThenInclude(k => k.Ouder)
            .Include(g => g.Bestemming)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Maak een ViewModel met de huidige deelnemers
        var viewModel = new GroepsreisDeelnemersViewModel
        {
            GroepsreisId = groepsreis.Id,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            HuidigeDeelnemers = groepsreis.Deelnemers.Select(d => new DeelnemerBeheerGroepsreisViewModel
            {
                Id = d.Id,
                KindNaam = $"{d.Kind.Voornaam} {d.Kind.Naam}",
                Leeftijd = DateTime.Now.Year - d.Kind.Geboortedatum.Year,
                OuderTelefoonnummer = d.Kind.Ouder.PhoneNumber,
                Medicatie = d.Kind.Medicatie,
                Allergieën = d.Kind.Allergieën,
                Opmerkingen = d.Opmerkingen
            }).ToList()
        };

        return View(viewModel);
    }
    #endregion

    #region Monitorenlijst ophalen : GET
    // GET: Groepsreis/LijstMonitoren/5
    public async Task<IActionResult> LijstMonitoren(int id)
    {
        // Haal de groepsreis op met monitoren en bestemming
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Monitoren)
                .ThenInclude(m => m.Persoon)
            .Include(g => g.Bestemming)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (groepsreis == null)
        {
            return NotFound();
        }

        // Maak de lijst van monitoren aan
        // Maak de lijst van monitoren aan
        var monitoren = groepsreis.Monitoren.Select(m => new MonitorViewModel
        {
            Naam = $"{m.Persoon.Voornaam} {m.Persoon.Naam}",
            Leeftijd = DateTime.Now.Year - m.Persoon.Geboortedatum.Year -
                       (DateTime.Now < m.Persoon.Geboortedatum.AddYears(DateTime.Now.Year - m.Persoon.Geboortedatum.Year) ? 1 : 0),
            Email = m.Persoon.Email,
            Telefoonnummer = m.Persoon.PhoneNumber
        }).ToList();

        // Maak een ViewModel met de monitorenlijst
        var viewModel = new GroepsreisMonitorenViewModel
        {
            GroepsreisId = groepsreis.Id,
            BestemmingNaam = groepsreis.Bestemming.Naam,
            IngeschrevenMonitoren = monitoren
        };

        return View(viewModel);
    }
    #endregion


    #region Kopie maken van groepsreis voor wachtlijst.
    public async Task<Groepsreis> KopieerGroepsreisAsync(int origineleGroepsreisId)
    {
        // Haal de originele groepsreis op
        var origineleGroepsreis = await _uow.GroepsreisRepository.Search()
            .Include(g => g.Bestemming)
            .Include(g => g.Programmas)
            .FirstOrDefaultAsync(g => g.Id == origineleGroepsreisId);

        if (origineleGroepsreis == null)
        {
            throw new ArgumentException("Originele groepsreis niet gevonden.");
        }

        // Maak een kopie van de groepsreis
        var nieuweGroepsreis = new Groepsreis
        {
            BestemmingId = origineleGroepsreis.BestemmingId,
            Begindatum = origineleGroepsreis.Begindatum,
            Einddatum = origineleGroepsreis.Einddatum,
            Prijs = origineleGroepsreis.Prijs,
            Deelnemerslimiet = origineleGroepsreis.Deelnemerslimiet,
            IsKopie = true,
            OrigineleGroepsreisId = origineleGroepsreis.Id
        };

        // Kopieer de programma's indien nodig
        nieuweGroepsreis.Programmas = origineleGroepsreis.Programmas.Select(p => new Programma
        {
            ActiviteitId = p.ActiviteitId,
            // Stel andere relevante velden in
        }).ToList();

        // Voeg de nieuwe groepsreis toe aan de repository
        _uow.GroepsreisRepository.Create(nieuweGroepsreis);
        await _uow.SaveAsync();

        return nieuweGroepsreis;
    }
    #endregion

    #region Kopie Groepsreis opvullen.
    // POST: Groepsreis/AddExtraGroepsreis
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExtraGroepsreis(int groepsreisId)
    {
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // Haal de originele groepsreis op
                var origineleGroepsreis = await _uow.GroepsreisRepository.Search()
                    .Include(g => g.Wachtlijst)
                    .FirstOrDefaultAsync(g => g.Id == groepsreisId);

                if (origineleGroepsreis == null)
                {
                    TempData["ErrorMessage"] = "Groepsreis niet gevonden.";
                    return RedirectToAction("Detail", new { id = groepsreisId });
                }

                // Controleer of er genoeg mensen op de wachtlijst staan
                if (origineleGroepsreis.Wachtlijst.Count < origineleGroepsreis.Deelnemerslimiet)
                {
                    TempData["ErrorMessage"] = "Er staan niet genoeg mensen op de wachtlijst om een extra reis aan te maken.";
                    return RedirectToAction("Detail", new { id = groepsreisId });
                }

                // Kopieer de groepsreis
                var nieuweGroepsreis = await KopieerGroepsreisAsync(groepsreisId);

                // Verplaats deelnemers van de wachtlijst naar de nieuwe reis
                var deelnemersVanWachtlijst = origineleGroepsreis.Wachtlijst
                    .OrderBy(w => w.InschrijvingDatum)
                    .Take(origineleGroepsreis.Deelnemerslimiet)
                    .ToList();

                foreach (var wachtlijstItem in deelnemersVanWachtlijst)
                {
                    // Verwijder van de wachtlijst
                    _uow.WachtlijstRepository.Delete(wachtlijstItem);

                    // Voeg toe als deelnemer aan de nieuwe groepsreis
                    var deelnemer = new Deelnemer
                    {
                        KindId = wachtlijstItem.KindId,
                        GroepsreisDetailsId = nieuweGroepsreis.Id,
                        InschrijvingDatum = wachtlijstItem.InschrijvingDatum,
                        Opmerkingen = wachtlijstItem.Opmerkingen

                    };
                    _uow.DeelnemerRepository.Create(deelnemer);
                }

                await _uow.SaveAsync();
                transaction.Complete();

                TempData["SuccessMessage"] = "Een nieuwe groepsreis is aangemaakt en deelnemers zijn verplaatst.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }
            catch (Exception ex)
            {
                transaction.Dispose();
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de extra groepsreis.";
                return RedirectToAction("Detail", new { id = groepsreisId });
            }
        }
    }

    #region Reis annuleren GET
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    public async Task<IActionResult> AnnuleerGroepsreis(int groepsreisId)
    {
        _logger.LogInformation($"Start annuleren van groepsreis met ID: {groepsreisId}");

        //groepsreis ophalen met deelnemers en monitoren en bestemming
        var groepsreis = await _uow.GroepsreisRepository.GetByIdWithIncludeAsync(
            groepsreisId,
            r => r.Bestemming,
            r => r.Deelnemers,
            r => r.Monitoren);

        //checken of groepsreis bestaat of al geannuleerd is
        if (groepsreis == null || groepsreis.IsGeannuleerd)
        {
            _logger.LogError($"Groepsreis met ID {groepsreisId} niet gevonden of is al geannuleerd.");
            TempData["Foutmelding"] = "De groepsreis bestaat niet of is al geannuleerd.";
            return RedirectToAction("Beheer");
        }

        var viewModel = new GroepsreisAnnuleerViewModel
        {
            Id = groepsreis.Id,
            Naam = groepsreis.Bestemming.Naam ?? "Onbekend", //null-reference voorkomen
            Begindatum = groepsreis.Begindatum,
            Einddatum = groepsreis.Einddatum,
        };

        return View(viewModel);
    }
    #endregion

    #region Reis annuleren POST
    [Authorize(Roles = "Verantwoordelijke, Beheerder")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnnuleerGroepsreis(int groepsreisId, GroepsreisAnnuleerViewModel viewModel)
    {
        //groepsreis ophalen met deelnemers en monitoren
        var groepsreis = await _uow.GroepsreisRepository.Search()
            .Include(r => r.Bestemming)
            .Include(r => r.Deelnemers)
                .ThenInclude(d => d.Kind)
                    .ThenInclude(k => k.Ouder)
            .Include(r => r.Monitoren)
            .FirstOrDefaultAsync(r => r.Id == groepsreisId);

        //checken of groepsreis bestaat
        if (groepsreis == null)
        {
            TempData["Foutmelding"] = "De groepsreis bestaat niet.";
            return RedirectToAction("Beheer");
        }

        if (ModelState.IsValid)
        {
            //groepsreis als geannuleerd zetten en reden opslaan
            groepsreis.IsGeannuleerd = true;
            groepsreis.RedenAnnulatie = viewModel.RedenAnnulatie;

            _uow.GroepsreisRepository.Update(groepsreis);
            await _uow.SaveAsync();

            //alle deelnemers groepren op basis van de ouder
            var oudersMetKinderen = groepsreis.Deelnemers
                  .Where(d => d.Kind.Ouder != null && !string.IsNullOrWhiteSpace(d.Kind.Ouder.Email))
                  .GroupBy(d => d.Kind.Ouder) //alle deelnemers worden gegroepeerd op ouder. elke groep bevat alle deelnemers (kinderen) waarvan Kind.Ouder hetzelfde is.
                  .ToList();

            //een e-mail per ouder sturen
            foreach (var ouderGroep in oudersMetKinderen)
            {
                var ouder = ouderGroep.Key; //toont de unieke ouder van de groep

                if (ouder == null || string.IsNullOrWhiteSpace(ouder.Email))
                {
                    _logger.LogWarning($"De deelnemer '{ouder.Voornaam} {ouder.Naam}' heeft geen geldig e-mailadres. E-mail wordt niet verstuurd.");
                    continue; // Skip deze deelnemer
                }

                try
                {
                    // Email inhoud voorbereiden
                    var subject = $"Annulering van groepsreis {groepsreis.Bestemming.Naam}";
                    //html opmaak
                    var body = $"""
<p>Beste {ouder.Voornaam} {ouder.Naam},</p>

<p>Het spijt ons u te moeten meedelen dat groepsreis <strong>{groepsreis.Bestemming.Naam} ({groepsreis.Begindatum:dd/MM/yyyy} - {groepsreis.Einddatum:dd/MM/yyyy})</strong> geannuleerd is.</p>

<p>Reden: {groepsreis.RedenAnnulatie}</p>

<p>Als u vragen hebt, mag u ons natuurlijk altijd contacteren.</p>

<p>Met vriendelijke groeten,<br/>Het groepsreisteam van De Reizende Ziekenkas</p>
""";



                    // Verstuur e-mail met SendGrid
                    await _emailService.SendEmailAsync(ouder.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fout bij het versturen van e-mail naar {ouder.Voornaam} {ouder.Naam} ({ouder.Email}).");
                }
            }


            TempData["Succesmelding"] = "De groepsreis is succesvol geannuleerd en de deelnemers zijn geïnformeerd.";
            return RedirectToAction("Beheer");
        }


        return View(viewModel);
    }
    #endregion

    public IActionResult IngeschrevenReizen()
    {
        return View();
    }
    #endregion
}