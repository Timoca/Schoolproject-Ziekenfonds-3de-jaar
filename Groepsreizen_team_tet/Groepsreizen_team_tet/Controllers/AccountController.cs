using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Groepsreizen_team_tet.Data;
using System.Threading.Tasks;
using Groepsreizen_team_tet.ViewModels.AccountViewModels;
using System;
using Groepsreizen_team_tet.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace Groepsreizen_team_tet.Controllers;

[Breadcrumb("Dashboard", controller: "Dashboard", action: "Index")]
[Breadcrumb("Account", controller: "Account", action: "Index")]
public class AccountController : Controller
{
    private readonly UserManager<CustomUser> _userManager;
    private readonly SignInManager<CustomUser> _signInManager;
    private readonly RoleManager<CustomRole> _roleManager;

    public AccountController(UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager, RoleManager<CustomRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Gebruiker niet gevonden.");
        }

        var model = new AccountViewModel
        {
            Email = user.Email!,
            Naam = user.Naam,
            Voornaam = user.Voornaam,
            Straat = user.Straat,
            Huisnummer = user.Huisnummer,
            Gemeente = user.Gemeente,
            Postcode = user.Postcode,
            Telefoonnummer = user.PhoneNumber!,
            Geboortedatum = user.Geboortedatum,
            Huisdokter = user.Huisdokter,
            ContractNummer = user.ContractNummer,
            RekeningNummer = user.RekeningNummer
        };

        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    [Breadcrumb("Account Aanmaken")]
    public IActionResult Register()
    {
        return View();
    }
    
    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Controleer of de gebruiker met hetzelfde e-mailadres al bestaat
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Deze gebruiker lijkt al geregistreerd te zijn. Controleer uw gegevens of probeer in te loggen.");
                return View(model);
            }

            // Controleer of de geboortedatum in het verleden ligt
            if (model.Geboortedatum >= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "De geboortedatum moet in het verleden liggen.");
                return View(model);
            }

            // Maak de gebruiker aan
            var user = new CustomUser
            {
                UserName = model.Email,
                Email = model.Email,
                Naam = model.Naam,
                Voornaam = model.Voornaam,
                Straat = model.Straat,
                Huisnummer = model.Huisnummer,
                Gemeente = model.Gemeente,
                Postcode = model.Postcode,
                Geboortedatum = model.Geboortedatum,
                PhoneNumber = model.Telefoonnummer,
                Huisdokter = model.Huisdokter,
                ContractNummer = model.ContractNummer,
                RekeningNummer = null,
                IsActief = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Voeg de gebruiker toe aan de "Gebruiker" rol
                await _userManager.AddToRoleAsync(user, "Gebruiker");

                // Log de gebruiker automatisch in
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }


    // GET: /Account/Login
    [AllowAnonymous]
    [Breadcrumb("Inloggen")]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        model.ReturnUrl = returnUrl;

        if (ModelState.IsValid)
        {
            // Zoek de gebruiker op basis van het emailadres
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // Controleer of de gebruiker actief is
                if (!user.IsActief)
                {
                    // Voeg een foutmelding toe aan ModelState
                    ModelState.AddModelError(string.Empty, "Uw account is inactief. Neem contact op met de beheerder.");
                    return View(model);
                }
            }

            // Probeer de gebruiker in te loggen
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Stuur de gebruiker naar de gewenste pagina of naar de homepagina
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Dit account is vergrendeld. Probeer het later opnieuw.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ongeldige inlogpoging. Controleer uw inloggegevens.");
            }
        }

        // Toon het formulier opnieuw met eventuele fouten
        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: Manage/Delete
    [Breadcrumb("Account Verwijderen")]
    public async Task<IActionResult> Delete()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Gebruiker niet gevonden.");
        }

        return View();
    }

    // GET: Manage/Edit
    [Breadcrumb("Account Bewerken")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Gebruiker niet gevonden.");
        }

        var model = new AccountViewModel
        {
            Email = user.Email!,
            Naam = user.Naam,
            Voornaam = user.Voornaam,
            Straat = user.Straat,
            Huisnummer = user.Huisnummer,
            Gemeente = user.Gemeente,
            Postcode = user.Postcode,
            Telefoonnummer = user.PhoneNumber!,
            Geboortedatum = user.Geboortedatum,
            Huisdokter = user.Huisdokter,
            ContractNummer = user.ContractNummer,
            RekeningNummer = user.RekeningNummer
        };

        return View(model);
    }

    // POST: Manage/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>Edit(AccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Gebruiker niet gevonden.");
        }

        user.Naam = model.Naam;
        user.Voornaam = model.Voornaam;
        user.Straat = model.Straat;
        user.Huisnummer = model.Huisnummer;
        user.Gemeente = model.Gemeente;
        user.Postcode = model.Postcode;
        user.PhoneNumber = model.Telefoonnummer;
        user.Geboortedatum = model.Geboortedatum;
        user.Huisdokter = model.Huisdokter;
        user.ContractNummer = model.ContractNummer;
        user.RekeningNummer = model.RekeningNummer?.ToUpper();

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // Herlaad de sign-in om wijzigingen direct zichtbaar te maken
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Je account is succesvol bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // POST: Manage/DeleteConfirmed
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Gebruiker niet gevonden.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Delete");
    }
}