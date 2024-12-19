using Groepsreizen_team_tet.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Monitor", controller: "Monitor", action: "Index")]
public class MonitorController : Controller
{
    private readonly GroepsreizenContext _context;
    private readonly UserManager<CustomUser> _userManager;
    private readonly RoleManager<CustomRole> _roleManager;

    public MonitorController( GroepsreizenContext context, UserManager<CustomUser> userManager, RoleManager<CustomRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Monitor
    public async Task<IActionResult> Index()
    {
        // Haal alle monitoren op inclusief de gerelateerde Persoon
        var monitoren = await _context.Monitoren
            .Include(m => m.Persoon).Where(m => m.Persoon.IsActief)
            .ToListAsync();

        // Haal alle rollen voor de betrokken gebruikers op
        var userIds = monitoren.Select(m => m.PersoonId).ToList();
        var userRoles = await _userManager.GetUsersInRoleAsync("Hoofdmonitor");

        var hoofdmonitorIds = userRoles.Select(u => u.Id).ToHashSet();

        // Map de monitoren naar de ViewModel
        var monitorListViewModel = monitoren.Select(m => new MonitorViewModel
        {
            Id = m.PersoonId,
            Email = m.Persoon.Email ?? "",
            Voornaam = m.Persoon.Voornaam,
            Naam = m.Persoon.Naam,
            IsHoofdMonitor = hoofdmonitorIds.Contains(m.PersoonId)
        }).ToList();

        return View(monitorListViewModel);
    }

    // GET: Monitor/Create
    [Breadcrumb("Monitor Toevoegen")]
    public async Task<IActionResult> CreateAsync(string? searchString)
    {
        var model = new MonitorCreateViewModel
        {
            SearchString = searchString
        };

        if (!string.IsNullOrEmpty(searchString))
        {
            var excludedRoles = new[] { "Beheerder", "Verantwoordelijke", "Monitor", "Hoofdmonitor" };

            var excludedUserIds = await _context.UserRoles
                .Where(ur => excludedRoles.Contains(_context.Roles
                    .Where(r => r.Id == ur.RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefault()))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            var usersQuery = _userManager.Users
                .Where(u => u.IsActief &&
                            !excludedUserIds.Contains(u.Id))
                .Where(u => u.Voornaam.Contains(searchString) || u.Naam.Contains(searchString));

            // Filter op voornaam of naam
            usersQuery = usersQuery.Where(u => u.Voornaam.Contains(searchString) || u.Naam.Contains(searchString) && u.IsActief == true);

            var userList = await usersQuery.ToListAsync();

            model.Gebruikers = userList.Select(u => new MonitorUserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Voornaam = u.Voornaam,
                Naam = u.Naam,
                Leeftijd = CalculateAge(u.Geboortedatum),
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()!
            }).ToList();
        }

        return View(model);
    }

    // POST: Monitor/Add
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int userId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        var existingMonitor = await _context.Monitoren.FirstOrDefaultAsync(m => m.PersoonId == userId);
        if (existingMonitor != null)
        {
            return RedirectToAction(nameof(Create));
        }

        // Creëer de monitor
        var monitor = new Models.Monitor
        {
            PersoonId = userId,
            IsHoofdMonitor = 0
        };

        _context.Monitoren.Add(monitor);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Create));
    }

    // GET: Gebruiker/Edit/{id}
    [Breadcrumb("Monitor Bewerken")]
    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        // Definieer de rollen die beschikbaar moeten zijn
        var allowedRoles = new[] { "Monitor", "Hoofdmonitor" };

        // Haal alleen de toegestane rollen op
        var roles = await _roleManager.Roles
            .Where(r => allowedRoles.Contains(r.Name))
            .Select(r => r.Name)
            .ToListAsync();

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditMonitorViewModel
        {
            Id = user.Id,
            Voornaam = user.Voornaam,
            Naam = user.Naam,
            Straat = user.Straat,
            Huisnummer = user.Huisnummer,
            Gemeente = user.Gemeente,
            Postcode = user.Postcode,
            Geboortedatum = user.Geboortedatum,
            Telefoonnummer = user.PhoneNumber!,
            Huisdokter = user.Huisdokter,
            ContractNummer = user.ContractNummer,
            RekeningNummer = user.RekeningNummer,
            IsActief = user.IsActief,
            SelectedRole = userRoles.FirstOrDefault() ?? string.Empty,
            AvailableRoles = roles!
        };

        return View(model);
    }

    // POST: Monitor/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMonitorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Herlaad de beschikbare rollen in geval van fouten
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
        if (user == null)
        {
            return NotFound();
        }

        // Update de gebruikersinformatie
        user.Voornaam = model.Voornaam;
        user.Naam = model.Naam;
        user.Straat = model.Straat;
        user.Huisnummer = model.Huisnummer;
        user.Gemeente = model.Gemeente;
        user.Postcode = model.Postcode;
        user.Geboortedatum = model.Geboortedatum;
        user.PhoneNumber = model.Telefoonnummer;
        user.Huisdokter = model.Huisdokter;
        user.ContractNummer = model.ContractNummer;
        user.RekeningNummer = model.RekeningNummer;
        user.IsActief = model.IsActief;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // Beheer de rol van de gebruiker
        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRole = model.SelectedRole;

        if (currentRoles.FirstOrDefault() != selectedRole)
        {
            // Verwijder huidige rollen
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            // Voeg de geselecteerde rol toe
            if (!string.IsNullOrEmpty(selectedRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, selectedRole);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Monitor/Detail/{id}
    [Breadcrumb("Monitor Details")]
    public async Task<IActionResult> Detail(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        // Haal de monitor op inclusief gerelateerde Persoon en Opleidingen via Persoon
        var monitor = await _context.Monitoren
            .Include(m => m.Persoon)
                .ThenInclude(p => p.Opleidingen)
            .FirstOrDefaultAsync(m => m.PersoonId == id && m.Persoon.IsActief);

        if (monitor == null)
        {
            return NotFound();
        }

        // Haal de rollen van de monitor op
        var roles = await _userManager.GetRolesAsync(monitor.Persoon);

        // Controleer of de monitor de rol "Hoofdmonitor" heeft
        bool isHoofdMonitor = roles.Contains("Hoofdmonitor");

        // Haal de groepsreizen op waaraan de monitor heeft deelgenomen
        var groepsreizen = await _context.Groepsreizen
            .Where(g => g.Monitoren.Any(m => m.PersoonId == id))
            .Include(g => g.Bestemming)
            .ToListAsync();

        // Creëer een lijst van groepsreis details
        var groepsreisDetails = groepsreizen.Select(g => new GroepsreisDetails
        {
            Naam = g.Bestemming?.Naam ?? "Onbekend", // Veilige toegang
            Datum = g.Begindatum,
            WasHoofdMonitor = isHoofdMonitor // Aanpassen indien nodig per groepsreis
        }).ToList();

        // Creëer een lijst van opleiding details
        var opleidingen = monitor.Persoon.Opleidingen.Select(o => new OpleidingDetails
        {
            Naam = o.Naam,
            Beschrijving = o.Beschrijving ?? "Geen Beschrijving",
            Begindatum = o.Begindatum,
            Einddatum = o.Einddatum,
            Locatie = o.Locatie ?? "Onbekend",
        }).ToList();

        // Creëer de ViewModel
        var detailsViewModel = new MonitorDetailsViewModel
        {
            Id = monitor.PersoonId,
            Email = monitor.Persoon.Email ?? "",
            Voornaam = monitor.Persoon.Voornaam,
            Naam = monitor.Persoon.Naam,
            Straat = monitor.Persoon.Straat,
            Huisnummer = monitor.Persoon.Huisnummer,
            Gemeente = monitor.Persoon.Gemeente,
            Postcode = monitor.Persoon.Postcode,
            Geboortedatum = monitor.Persoon.Geboortedatum,
            Telefoonnummer = monitor.Persoon.PhoneNumber ?? "",
            Huisdokter = monitor.Persoon.Huisdokter,
            ContractNummer = monitor.Persoon.ContractNummer,
            RekeningNummer = monitor.Persoon.RekeningNummer,
            IsActief = monitor.Persoon.IsActief,
            IsHoofdMonitor = isHoofdMonitor,
            Opleidingen = opleidingen,
            Groepsreizen = groepsreisDetails
        };

        return View(detailsViewModel);
    }


    // POST: Monitor/SetHoofdMonitor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetHoofdMonitor(int id)
    {
        if (id <= 0)
        {
            TempData["Error"] = "Ongeldig monitor ID.";
            return NotFound();
        }

        // Haal de monitor op
        var monitor = await _context.Monitoren
            .Include(m => m.Persoon)
            .Include(m => m.Opleidingen)
            .FirstOrDefaultAsync(m => m.PersoonId == id && m.Persoon.IsActief);

        if (monitor == null)
        {
            TempData["Error"] = "Monitor niet gevonden.";
            return NotFound();
        }

        // Wijs de rol "Hoofdmonitor" toe als deze nog niet is toegewezen
        var user = monitor.Persoon;
        var currentRoles = await _userManager.GetRolesAsync(user);

        if (!currentRoles.Contains("Hoofdmonitor"))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Hoofdmonitor");
            if (!addRoleResult.Succeeded)
            {
                TempData["Error"] = "Er is een fout opgetreden bij het toewijzen van de Hoofdmonitor rol.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (currentRoles.Contains("Monitor"))
            {
                var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "Monitor");
                if (!removeRoleResult.Succeeded)
                {
                    TempData["Error"] = "Er is een fout opgetreden bij het verwijderen van de Monitor rol.";
                    return RedirectToAction(nameof(Detail), new { id });
                }
            }
        }

        TempData["Success"] = "Monitor succesvol ingesteld als Hoofdmonitor.";
        return RedirectToAction(nameof(Detail), new { id });
    }


    public IActionResult Vorige()
    {
        return RedirectToAction("Index", "Monitor");
    }

    public IActionResult ToDashboard()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    private int CalculateAge(DateTime geboortedatum)
    {
        var today = DateTime.Today;
        var age = today.Year - geboortedatum.Year;
        if (geboortedatum.Date > today.AddYears(-age)) age--;
        return age;
    }
}