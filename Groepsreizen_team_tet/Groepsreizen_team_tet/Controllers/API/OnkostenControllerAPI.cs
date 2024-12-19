using Groepsreizen_team_tet.Models;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace Groepsreizen_team_tet.Controllers.API
{
    [Route("api/onkosten")]
    [ApiController]
    public class OnkostenControllerAPI : ControllerBase
    {
        private readonly IUnitOfWork _context;
        private readonly ILogger<OnkostenControllerAPI> _logger;

        public OnkostenControllerAPI(IUnitOfWork context, ILogger<OnkostenControllerAPI> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Haal alle onkosten op", Description = "Geeft een lijst van alle onkosten terug")]
        public IActionResult GetOnkosten()
        {
            return Ok();
        }


        //GET /api/onkosten/{groepsreisId} - alle onkosten van een groepsreis ophalen
        [HttpGet("{groepsreisId}")]
        public async Task<ActionResult<IEnumerable<Onkosten>>> GetOnkosten(int groepsreisId)
        {
            // Controleer of de groepsreis bestaat
            var groepsreis = await _context.GroepsreisRepository.GetByIdAsync(groepsreisId);
            if (groepsreis == null)
            {
                _logger.LogWarning("Groepsreis met ID {GroepsreisId} bestaat niet.", groepsreisId);
                return NotFound($"Groepsreis met ID {groepsreisId} bestaat niet.");
            }

            // Controleer of er een repository beschikbaar is
            if (_context.OnkostenRepository == null)
            {
                _logger.LogError("OnkostenRepository is null.");
                return NotFound("OnkostenRepository is niet beschikbaar.");
            }

            // Haal de onkosten op voor de specifieke groepsreis, enkel degene ingegeven door hoofdmonitor
            var onkosten = await _context.OnkostenRepository.Search()
                .Where(o => o.GroepsreisId == groepsreisId && o.IngegevenDoorHoofdmonitor)
                .ToListAsync();

            if (!onkosten.Any())
            {
                _logger.LogInformation("Geen onkosten gevonden voor groepsreisId {GroepsreisId}.", groepsreisId);
                return NotFound($"Geen onkosten gevonden voor groepsreisId: {groepsreisId}");
            }

            _logger.LogInformation("Onkosten gevonden voor groepsreisId {GroepsreisId}: {AantalOnkosten}", groepsreisId, onkosten.Count);
            return Ok(onkosten);
        }


        //POST /api/onkosten/groepsreisId/create - onkost toevoegen aan bepaalde groepsreis
        [HttpPost("{groepsreisId}/create")]
        public async Task<ActionResult<Onkosten>> CreateOnkosten(int groepsreisId, [FromBody] Onkosten onkosten)
        {
            _logger.LogInformation("Ontvangen onkosten: {@Onkosten}", onkosten);

            // Valideer groepsreisId
            if (groepsreisId <= 0)
            {
                _logger.LogError("Ongeldige groepsreisId: {GroepsreisId}", groepsreisId);
                return BadRequest("GroepsreisId is ongeldig.");
            }

            //Valideer request body
            if (onkosten == null)
            {
                _logger.LogError("Request body is leeg.");
                return BadRequest("Request body mag niet leeg zijn.");
            }

            //Verifieer ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogError("ModelState is ongeldig: {ModelStateErrors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return ValidationProblem(ModelState);
            }

            // Controleer of de groepsreis bestaat
            var groepsreis = await _context.GroepsreisRepository.GetByIdAsync(groepsreisId);
            if (groepsreis == null)
            {
                _logger.LogError("Groepsreis met ID {GroepsreisId} bestaat niet.", groepsreisId);
                return NotFound($"Groepsreis met ID {groepsreisId} bestaat niet.");
            }

            // Koppel de onkost aan de groepsreis
            onkosten.GroepsreisId = groepsreisId;

            //Voeg de onkost toe
            await _context.OnkostenRepository.AddAsync(onkosten);
            await _context.SaveAsync();

            // Log succesvol toevoegen
            _logger.LogInformation("Nieuwe onkosten toegevoegd: {@Onkosten}", onkosten);

     
            //return CreatedAtAction(nameof(GetOnkosten), new { groepsreisId }, onkosten);
            return CreatedAtAction(nameof(GetOnkosten), new { groepsreisId = groepsreisId }, onkosten);

        }
    }
    }

