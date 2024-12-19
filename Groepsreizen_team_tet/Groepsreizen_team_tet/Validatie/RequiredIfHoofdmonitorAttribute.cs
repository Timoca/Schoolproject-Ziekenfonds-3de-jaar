using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Groepsreizen_team_tet.Models;

namespace Groepsreizen_team_tet.Validatie
{
    public class RequiredIfHoofdmonitorAttribute : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Controleer of de gebruiker een hoofdmonitor is
            var httpContext = (IHttpContextAccessor)validationContext.GetService(typeof(IHttpContextAccessor));
            var user = httpContext?.HttpContext?.User;

            if (user?.IsInRole("Hoofdmonitor") == true)
            {
                // Hoofdmonitor moet de waarde invullen
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? "Dit veld is verplicht voor hoofdmonitor.");
                }
            }

            // Validatie slaagt als de gebruiker geen hoofdmonitor is of als de waarde correct is ingevuld
            return ValidationResult.Success;
        }
    }
}
