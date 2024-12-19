using Groepsreizen_team_tet.Attributes;

namespace Groepsreizen_team_tet.Controllers
{
    [Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
    [Breadcrumb("Groepsreizen", controller: "Groepsreis", action: "Beheer")]
    public class BestemmingController : Controller
    {
        private readonly IUnitOfWork _uow;

        public BestemmingController(IUnitOfWork unitOfWork)
        {
            _uow = unitOfWork;
        }

        // De post-actie voor het aanmaken van een nieuwe bestemming in het modaal venster van de groepsreis
        // POST: Bestemming/CreateGroepsreisModal
        [HttpPost]
        public async Task<IActionResult> CreateGroepsreisModal(BestemmingCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Maak de nieuwe bestemming aan inclusief de Code
                var newBestemming = new Bestemming
                {
                    Naam = model.Naam,
                    Beschrijving = model.Beschrijving,
                    Code = model.Code,
                    MinLeeftijd = model.MinLeeftijd,
                    MaxLeeftijd = model.MaxLeeftijd
                };

                // Verwerk elk geüpload bestand
                if (model.FotoFiles != null && model.FotoFiles.Any())
                {
                    foreach (var fotoFile in model.FotoFiles)
                    {
                        if (fotoFile.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await fotoFile.CopyToAsync(memoryStream);
                                var newFoto = new Foto
                                {
                                    Naam = fotoFile.FileName,
                                    Afbeelding = memoryStream.ToArray(),
                                    Bestemming = newBestemming
                                };
                                _uow.FotoRepository.Create(newFoto);
                            }
                        }
                    }
                }

                _uow.BestemmingRepository.Create(newBestemming);
                await _uow.SaveAsync();

                return Json(new { success = true, bestemmingId = newBestemming.Id, bestemmingNaam = newBestemming.Naam });
            }

            // Return validation errors in JSON format
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        [Breadcrumb("Bestemmingen Beheren")]
        public async Task<IActionResult> Beheer()
        {
            var bestemmingen = await _uow.BestemmingRepository.GetAllAsync();
            var viewModel = new BestemmingBeheerViewModel
            {
                Bestemmingen = bestemmingen.ToList()
            };
            return View(viewModel);
        }

        #region Edit GET
        // GET: Bestemming/Edit/{id}
        [Breadcrumb("Bestemming Beheren", controller: "Bestemming", action: "Beheer")]
        [Breadcrumb("Bestemming Bewerken")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Haal de bestemming op aan de hand van het ID inclusief foto's
            var bestemming = await _uow.BestemmingRepository.Search()
                .Include(b => b.Fotos)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bestemming == null)
            {
                return NotFound();
            }

            var viewModel = new BestemmingEditViewModel
            {
                Id = bestemming.Id,
                Naam = bestemming.Naam,
                Beschrijving = bestemming.Beschrijving,
                Code = bestemming.Code,
                MinLeeftijd = bestemming.MinLeeftijd,
                MaxLeeftijd = bestemming.MaxLeeftijd,
                BestaandeFotos = bestemming.Fotos.Select(f => new FotoEditViewModel
                {
                    Id = f.Id,
                    Naam = f.Naam,
                    AfbeeldingBase64 = $"data:image/png;base64,{Convert.ToBase64String(f.Afbeelding)}"
                }).ToList()
            };

            return View(viewModel);
        }
        #endregion

        #region Edit POST
        // POST: Bestemming/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BestemmingEditViewModel model)
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
                var bestemming = await _uow.BestemmingRepository.Search()
                    .Include(b => b.Fotos) // Include de foto's zodat deze beschikbaar zijn
                    .FirstOrDefaultAsync(b => b.Id == model.Id);

                if (bestemming == null)
                {
                    return NotFound();
                }

                bestemming.Naam = model.Naam;
                bestemming.Beschrijving = model.Beschrijving;
                bestemming.Code = model.Code;
                bestemming.MinLeeftijd = model.MinLeeftijd;
                bestemming.MaxLeeftijd = model.MaxLeeftijd;

                // Verwijder geselecteerde foto's
                if (model.FotosToDelete != null && model.FotosToDelete.Any())
                {
                    foreach (var fotoId in model.FotosToDelete)
                    {
                        var foto = bestemming.Fotos.FirstOrDefault(f => f.Id == fotoId);
                        if (foto != null)
                        {
                            _uow.FotoRepository.Delete(foto);
                        }
                    }
                }

                // Voeg nieuwe foto's toe
                if (model.FotoFiles != null && model.FotoFiles.Any())
                {
                    foreach (var fotoFile in model.FotoFiles)
                    {
                        if (fotoFile.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await fotoFile.CopyToAsync(memoryStream);
                                var newFoto = new Foto
                                {
                                    Naam = fotoFile.FileName,
                                    Afbeelding = memoryStream.ToArray(),
                                    Bestemming = bestemming
                                };
                                _uow.FotoRepository.Create(newFoto);
                            }
                        }
                    }
                }

                _uow.BestemmingRepository.Update(bestemming);
                await _uow.SaveAsync();

                return RedirectToAction("Beheer", "Groepsreis");
            }

            // Als de ModelState niet geldig is, herlaad de gegevens en toon de view opnieuw
            model.BestaandeFotos = (await _uow.BestemmingRepository.Search()
                .Include(b => b.Fotos)
                .FirstOrDefaultAsync(b => b.Id == model.Id))
                ?.Fotos.Select(f => new FotoEditViewModel
                {
                    Id = f.Id,
                    Naam = f.Naam,
                    AfbeeldingBase64 = $"data:image/png;base64,{Convert.ToBase64String(f.Afbeelding)}"
                }).ToList() ?? new List<FotoEditViewModel>();

            return View(model);
        }
        #endregion
    }
}