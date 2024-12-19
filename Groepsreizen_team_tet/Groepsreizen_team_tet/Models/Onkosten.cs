using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Groepsreizen_team_tet.Models
{
    public class Onkosten
    {
        public int Id { get; set; }

        [Required] 
        public int GroepsreisId { get; set; }
       
        [Required]
        public string Titel { get; set; } = default!;
       
        [Required] 
        public string Omschrijving { get; set; } = default!;
      
        public string? Locatie { get; set; } = default!;
     
        [Required] 
        public decimal Bedrag { get; set; }
     
        [Required] 
        public DateTime Datum { get; set; }

        [DefaultValue("VGhpcyBpcyBhIHRlc3Qgc3RyaW5nIGNvbnZlcnRlZCB0byBiYXNlNjQ=")]
        public byte[]? Betaalbewijs { get; set; } = default!;


        public bool IngegevenDoorHoofdmonitor { get; set; } //om te bepalen wie de onkost heeft ingegeven


        [JsonIgnore] // Uitsluiten van serialisatie en validatie
        [ValidateNever] // Voorkomt validatie door de modelbinder
        public Groepsreis Groepsreis { get; set; } 
    }
}
