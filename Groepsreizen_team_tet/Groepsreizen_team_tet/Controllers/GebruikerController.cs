using Groepsreizen_team_tet.Attributes;
using Groepsreizen_team_tet.ViewModels.GebruikerViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Groepsreizen_team_tet.Controllers;

[Authorize(Roles = "Beheerder")]
[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Gebruikerbeheer", controller: "Gebruiker", action: "Beheer")]
public class GebruikerController : Controller
{
    private readonly GroepsreizenContext _context;
    private readonly UserManager<CustomUser> _userManager;
    private readonly RoleManager<CustomRole> _roleManager;

    public GebruikerController(GroepsreizenContext context, UserManager<CustomUser> userManager, RoleManager<CustomRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Gebruiker
    public async Task<IActionResult> Beheer(string searchString)
    {
        var usersQuery = _userManager.Users.Include(u => u.Monitor).AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            usersQuery = usersQuery.Where(u => u.Voornaam.Contains(searchString) || u.Naam.Contains(searchString));
        }

        var userList = await usersQuery.ToListAsync();

        var model = userList.Select(u => new GebruikerViewModel
        {
            Id = u.Id,
            Voornaam = u.Voornaam,
            Naam = u.Naam,
            Leeftijd = CalculateAge(u.Geboortedatum),
            IsActief = u.IsActief,
            Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()!,
        }).ToList();

        return View(model);
    }

    // GET: Gebruiker/Edit/{id}
    [Breadcrumb("Gebruiker Bewerken")]
    public async Task<IActionResult> Edit(int id) // Id is int
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

        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditGebruikerViewModel
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

    // POST: Gebruiker/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditGebruikerViewModel model)
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

        return RedirectToAction(nameof(Beheer));
    }

    // GET: Gebruiker/Delete/{id}
    [Breadcrumb("Gebruiker Verwijderen")]
    public async Task<IActionResult> Delete(int id)
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

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Geen rol";

        var model = new DeleteGebruikerViewModel
        {
            Id = user.Id,
            Voornaam = user.Voornaam,
            Naam = user.Naam,
            Rol = role
        };

        return View(model);
    }

    // POST: Gebruiker/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            // Voeg fouten toe aan ModelState en toon de Delete view opnieuw
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Geen rol";

            var model = new DeleteGebruikerViewModel
            {
                Id = user.Id,
                Voornaam = user.Voornaam,
                Naam = user.Naam,
                Rol = role
            };

            return View(model);
        }

        return RedirectToAction(nameof(Beheer));
    }


    public IActionResult Vorige()
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