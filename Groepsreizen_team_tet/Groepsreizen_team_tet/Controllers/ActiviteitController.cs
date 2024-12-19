using Groepsreizen_team_tet.Attributes;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Activiteitenbeheer", controller: "Activiteit", action: "Beheer")]
public class ActiviteitController : Controller
{
    private readonly IUnitOfWork _uow;

    public ActiviteitController(IUnitOfWork unitOfWork)
    {
        _uow = unitOfWork;
    }

    // GET: Activiteit/Index
    public async Task<IActionResult> Index()
    {
        // Haal alle activiteiten op en hun bijhorende groepsreizen
        var activiteiten = await _uow.ActiviteitRepository.Search()
            .Include(a => a.Programmas)
            .ThenInclude(p => p.Groepsreis)
            .ThenInclude(g => g.Bestemming)
            .ToListAsync();

        // Maak het viewmodel aan
        var viewModel = new ActiviteitViewModel
        {
            Activiteiten = activiteiten.Select(a => new ActiviteitViewModel
            {
                Id = a.Id,
                Naam = a.Naam,
                Beschrijving = a.Beschrijving,
                Groepsreizen = a.Programmas.Select(p => new GroepsreisViewModel
                {
                    Id = p.Groepsreis.Id,
                    Code = p.Groepsreis.Bestemming.Code, // Code van de bestemming
                    BestemmingNaam = p.Groepsreis.Bestemming.Naam, // Naam van de bestemming
                    Begindatum = p.Groepsreis.Begindatum,
                    Einddatum = p.Groepsreis.Einddatum
                }).ToList()
            }).ToList()
        };


        // Geef het viewmodel door aan de view
        return View(viewModel);
    }

    // GET: Activiteit/Beheer
    public async Task<IActionResult> Beheer()
    {
        // Haal de lijst van alle activiteiten op
        var activiteiten = await _uow.ActiviteitRepository.GetAllAsync();

        // Maak een viewmodel voor de beheerpagina
        var viewModel = new ActiviteitBeheerViewModel
        {
            Activiteiten = activiteiten
        };

        return View(viewModel);
    }

    // GET: Activiteit/Create
    [Breadcrumb("Activiteit Toevoegen")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Activiteit/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActiviteitCreateViewModel viewModel)
    {
        Console.WriteLine("Formulier ontvangen");

        // Controleer eerst of het model geldig is
        if (!ModelState.IsValid)
        {
            // Log de validatiefouten
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validatiefout in {foutmelding}: {error.ErrorMessage}");
                }
            }

            // Haal activiteiten opnieuw op bij foutieve invoer
            viewModel.Activiteiten = new SelectList(await _uow.ActiviteitRepository.GetAllAsync(), "Id", "Naam");

            // Return de view met het model en fouten
            return View(viewModel);
        }

        if (ModelState.IsValid)
        {
            Console.WriteLine("ModelState is geldig");

            // Maak een nieuwe activiteit aan
            var nieuweActiviteit = new Activiteit
            {
                Naam = viewModel.Naam,
                Beschrijving = viewModel.Beschrijving,
                Programmas = viewModel.GeselecteerdeActiviteiten.Select(id => new Programma
                {
                    ActiviteitId = id
                }).ToList(),
            };

            // Voeg de nieuwe activiteit toe aan de database
            _uow.ActiviteitRepository.Create(nieuweActiviteit);
            await _uow.SaveAsync();

            // Redirect naar de Beheer-pagina na succesvolle toevoeging
            return RedirectToAction(nameof(Beheer));
        }
        else
        {
            Console.WriteLine("ModelState is ongeldig");
        }
        return View(viewModel);
    }

    // De post-actie voor het aanmaken van een nieuwe activiteit in het modaal venster van de groepsreis
    // POST: Activiteit/CreateGroepsreisModal
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroepsreisModal(ActiviteitCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var nieuweActiviteit = new Activiteit
            {
                Naam = viewModel.Naam,
                Beschrijving = viewModel.Beschrijving
            };

            _uow.ActiviteitRepository.Create(nieuweActiviteit);
            await _uow.SaveAsync();

            return Json(new { success = true, activiteitId = nieuweActiviteit.Id, activiteitNaam = nieuweActiviteit.Naam });
        }

        // Als er validatiefouten zijn, stuur de foutberichten terug in JSON
        return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
    }


    // GET: Activiteit/Edit/5
    [Breadcrumb("Activiteit Bewerken")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return BadRequest("Activiteit ID is vereist.");
        }

        var activiteit = await _uow.ActiviteitRepository.GetByIdAsync(id.Value);
        if (activiteit == null)
        {
            return NotFound("Activiteit niet gevonden.");
        }

        var bestemmingen = await _uow.BestemmingRepository.GetAllAsync();
        var activiteitenList = await _uow.ActiviteitRepository.GetAllAsync();

        var viewModel = new ActiviteitEditViewModel
        {
            Naam = activiteit.Naam,
            Beschrijving = activiteit.Beschrijving,
        };

        return View(viewModel);
    }

    // POST: Activiteit/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActiviteitEditViewModel viewModel)
    {
        if (id <= 0)
        {
            return BadRequest("Ongeldig Activiteit ID.");
        }

        // Controleer of het model geldig is
        if (!ModelState.IsValid)
        {
            // Log de validatiefouten
            foreach (var foutmelding in ModelState.Keys)
            {
                var errors = ModelState[foutmelding]!.Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validatiefout in {foutmelding}: {error.ErrorMessage}");
                }
            }

            // Return de view met het model en fouten
            return View(viewModel);
        }

        // Zoek de bestaande activiteit
        var activiteit = await _uow.ActiviteitRepository.GetByIdAsync(id);
        if (activiteit == null)
        {
            return NotFound("Activiteit niet gevonden.");
        }

        // Update de activiteit met de gegevens uit het viewmodel
        activiteit.Naam = viewModel.Naam;
        activiteit.Beschrijving = viewModel.Beschrijving;

        // Sla de wijzigingen op in de database
        _uow.ActiviteitRepository.Update(activiteit);
        await _uow.SaveAsync();

        TempData["SuccessMessage"] = "Activiteit succesvol bijgewerkt.";
        return RedirectToAction(nameof(Beheer));
    }

    // GET: Activiteit/Delete/5
    [Breadcrumb("Activiteit Verwijderen")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return BadRequest("Activiteit ID is vereist.");
        }

        var activiteit = await _uow.ActiviteitRepository.Search()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activiteit == null)
        {
            return NotFound("Activiteit niet gevonden.");
        }

        var viewModel = new ActiviteitDeleteViewModel
        {
            Id = activiteit.Id,
            Naam = activiteit.Naam,
            Beschrijving = activiteit.Beschrijving
        };

        return View(viewModel);
    }

    // POST: Activiteit/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var activiteit = await _uow.ActiviteitRepository.GetByIdAsync(id);
        if (activiteit == null)
        {
            return NotFound("Activiteit niet gevonden.");
        }

        _uow.ActiviteitRepository.Delete(activiteit);
        await _uow.SaveAsync();

        TempData["SuccessMessage"] = "Activiteit succesvol verwijderd.";
        return RedirectToAction(nameof(Beheer));
    }

    public IActionResult BackToDashboard()
    {
        return RedirectToAction("Index", "Dashboard");
    }

}