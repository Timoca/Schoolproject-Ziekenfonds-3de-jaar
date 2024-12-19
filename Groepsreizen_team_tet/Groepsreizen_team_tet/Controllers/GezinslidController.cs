using Groepsreizen_team_tet.Attributes;
using Groepsreizen_team_tet.ViewModels.GezinsledenViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Groepsreizen_team_tet.Controllers
{
    [Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
    [Breadcrumb("Gezinsleden Beheren", controller: "Gezinslid", action: "Gezinsleden")]
    public class GezinslidController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<CustomUser> _userManager;

        public GezinslidController(IUnitOfWork uow, UserManager<CustomUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public IActionResult GoToDashboard()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Gezinslid/Beheer
        public async Task<IActionResult> Gezinsleden()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            var gezinsleden = await _uow.KindRepository.GetAllAsync();
            gezinsleden = gezinsleden.Where(g => g.PersoonId == user.Id).ToList();

            var viewModel = new GezinsledenManageViewModel
            {
                Gezinsleden = gezinsleden.Select(g => new GezinslidViewModel
                {
                    Id = g.Id,
                    Voornaam = g.Voornaam,
                    Naam = g.Naam,
                    Geboortedatum = g.Geboortedatum,
                    AllergieenList = string.IsNullOrEmpty(g.Allergieën) ? new List<string>() : g.Allergieën.Split(',').Select(a => a.Trim()).ToList(),
                    MedicatieList = string.IsNullOrEmpty(g.Medicatie) ? new List<string>() : g.Medicatie.Split(',').Select(m => m.Trim()).ToList()
                }).ToList(),
                NewGezinslid = new GezinslidViewModel()
            };

            return View(viewModel);
        }

        // POST: Gezinslid/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GezinsledenManageViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Controleer of een gezinslid met dezelfde voornaam en achternaam al bestaat voor deze gebruiker
            var duplicateGezinslid = await _uow.KindRepository.Search()
                .FirstOrDefaultAsync(g =>
                    g.PersoonId == user.Id &&
                    g.Voornaam.ToLower() == model.NewGezinslid.Voornaam.ToLower() &&
                    g.Naam.ToLower() == model.NewGezinslid.Naam.ToLower());

            if (duplicateGezinslid != null)
            {
                ModelState.AddModelError(string.Empty, "Een gezinslid met dezelfde voornaam en achternaam bestaat al.");
            }

            if (ModelState.IsValid)
            {
                // Concatenate Allergieën en Medicatie
                string allergieenString = model.NewGezinslid.AllergieenList != null && model.NewGezinslid.AllergieenList.Any()
                    ? string.Join(", ", model.NewGezinslid.AllergieenList.Where(a => !string.IsNullOrWhiteSpace(a)))
                    : string.Empty;

                string medicatieString = model.NewGezinslid.MedicatieList != null && model.NewGezinslid.MedicatieList.Any()
                    ? string.Join(", ", model.NewGezinslid.MedicatieList.Where(m => !string.IsNullOrWhiteSpace(m)))
                    : string.Empty;

                var gezinslid = new Kind
                {
                    PersoonId = user.Id,
                    Voornaam = model.NewGezinslid.Voornaam,
                    Naam = model.NewGezinslid.Naam,
                    Geboortedatum = model.NewGezinslid.Geboortedatum,
                    Allergieën = allergieenString,
                    Medicatie = medicatieString
                };

                _uow.KindRepository.Create(gezinslid);
                await _uow.SaveAsync();

                return RedirectToAction(nameof(Gezinsleden));
            }

            // Indien model ongeldig is, herlaad de lijst
            var currentUser = await _userManager.GetUserAsync(User);
            var gezinsleden = await _uow.KindRepository.GetAllAsync();
            gezinsleden = gezinsleden.Where(g => g.PersoonId == currentUser.Id).ToList();

            var viewModel = new GezinsledenManageViewModel
            {
                Gezinsleden = gezinsleden.Select(g => new GezinslidViewModel
                {
                    Id = g.Id,
                    Voornaam = g.Voornaam,
                    Naam = g.Naam,
                    Geboortedatum = g.Geboortedatum,
                    AllergieenList = string.IsNullOrEmpty(g.Allergieën) ? new List<string>() : g.Allergieën.Split(',').Select(a => a.Trim()).ToList(),
                    MedicatieList = string.IsNullOrEmpty(g.Medicatie) ? new List<string>() : g.Medicatie.Split(',').Select(m => m.Trim()).ToList()
                }).ToList(),
                NewGezinslid = model.NewGezinslid
            };

            return View("Gezinsleden", viewModel);
        }

        // POST: Gezinslid/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GezinslidViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("Ongeldige Gezinslid ID.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Gebruiker niet gevonden.");
            }

            // Controleer of een ander gezinslid met dezelfde voornaam en achternaam al bestaat voor deze gebruiker
            var duplicateGezinslid = await _uow.KindRepository.Search()
                .FirstOrDefaultAsync(g =>
                    g.PersoonId == user.Id &&
                    g.Voornaam.ToLower() == model.Voornaam.ToLower() &&
                    g.Naam.ToLower() == model.Naam.ToLower() &&
                    g.Id != id); // Uitsluiten van het huidige gezinslid

            if (duplicateGezinslid != null)
            {
                ModelState.AddModelError(string.Empty, "Een gezinslid met dezelfde voornaam en achternaam bestaat al.");
            }

            if (ModelState.IsValid)
            {
                var gezinslid = await _uow.KindRepository.GetByIdAsync(id);
                if (gezinslid == null)
                {
                    return NotFound("Gezinslid niet gevonden.");
                }

                // Concatenate Allergieën en Medicatie
                string allergieenString = model.AllergieenList != null && model.AllergieenList.Any()
                    ? string.Join(", ", model.AllergieenList.Where(a => !string.IsNullOrWhiteSpace(a)))
                    : string.Empty;

                string medicatieString = model.MedicatieList != null && model.MedicatieList.Any()
                    ? string.Join(", ", model.MedicatieList.Where(m => !string.IsNullOrWhiteSpace(m)))
                    : string.Empty;

                gezinslid.Voornaam = model.Voornaam;
                gezinslid.Naam = model.Naam;
                gezinslid.Geboortedatum = model.Geboortedatum;
                gezinslid.Allergieën = allergieenString;
                gezinslid.Medicatie = medicatieString;

                _uow.KindRepository.Update(gezinslid);
                await _uow.SaveAsync();

                return RedirectToAction(nameof(Gezinsleden));
            }

            // Indien model ongeldig is, herlaad de lijst
            var currentUserEdit = await _userManager.GetUserAsync(User);
            var gezinsledenEdit = await _uow.KindRepository.GetAllAsync();
            gezinsledenEdit = gezinsledenEdit.Where(g => g.PersoonId == currentUserEdit.Id).ToList();

            var viewModelEdit = new GezinsledenManageViewModel
            {
                Gezinsleden = gezinsledenEdit.Select(g => new GezinslidViewModel
                {
                    Id = g.Id,
                    Voornaam = g.Voornaam,
                    Naam = g.Naam,
                    Geboortedatum = g.Geboortedatum,
                    AllergieenList = string.IsNullOrEmpty(g.Allergieën) ? new List<string>() : g.Allergieën.Split(',').Select(a => a.Trim()).ToList(),
                    MedicatieList = string.IsNullOrEmpty(g.Medicatie) ? new List<string>() : g.Medicatie.Split(',').Select(m => m.Trim()).ToList()
                }).ToList(),
                NewGezinslid = new GezinslidViewModel()
            };

            return View("Gezinsleden", viewModelEdit);
        }

        // POST: Gezinslid/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var gezinslid = await _uow.KindRepository.GetByIdAsync(id);
            if (gezinslid == null)
            {
                return NotFound("Gezinslid niet gevonden.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || gezinslid.PersoonId != user.Id)
            {
                return Forbid(); // Gebruiker mag dit gezinslid niet verwijderen
            }

            _uow.KindRepository.Delete(gezinslid);
            await _uow.SaveAsync();

            return RedirectToAction(nameof(Gezinsleden));
        }
    }
}
