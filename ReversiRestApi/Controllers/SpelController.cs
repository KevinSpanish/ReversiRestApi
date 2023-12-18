using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReversiRestApi.DAL;
using ReversiRestApi.Model;

namespace ReversiRestApi.Controllers
{
    [Route("api/Spel")]
    [ApiController]
    public class SpelController : ControllerBase
    {
        private readonly ISpelRepository _iRepository;

        public SpelController(ISpelRepository repository)
        {
            _iRepository = repository;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult JsonResponse(object input)
        {
            Response.Headers.Add("Content-Type", "application/json");
            return Ok(input);
        }

        [HttpGet("all")]
        public ActionResult<IEnumerable<string>> GetSpellen()
        {
            return _iRepository.GetSpellen()
                .Select(x => $"Id: {x.ID} Omschrijving: {x.Omschrijving} Token: {x.Token} Speler1: {x.Speler1Token} Speler2: {x.Speler2Token}")
                .ToList();
        }

        // GET api/spel
        [HttpGet]
        public IActionResult GetSpelOmschrijvingenVanSpellenMetWachtendeSpeler()
        {
            var result = _iRepository.GetSpellen()
                    .Where(s => s.Speler1Token == null || s.Speler2Token == null);
            return JsonResponse(result);
        }

        // GET api/spel/{spelToken}
        [HttpGet("{spelToken}")]
        public ActionResult<SpelInfo> GetSpelBySpelToken(string spelToken)
        {
            Spel spel = _iRepository.GetSpel(spelToken);

            if (spel != null)
            {
                if (spel.Afgerond)
                {
                    spel.Winnaar = spel.OverwegendeKleur().ToString();
                }

                return Ok(new SpelInfo(spel));
            }
            else
            {
                return NotFound(spelToken);
            }
        }

        // GET api/spel/speler1/{speler1Token}
        [HttpGet("speler1/{speler1Token}")]
        public ActionResult<SpelInfo> GetSpelBySpeler1Token(string speler1Token)
        {
            Spel spel = _iRepository.GetSpelBySpeler1Token(speler1Token);

            if (spel != null)
            {
                return Ok(new SpelInfo(spel));
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/spel/speler/{spelerToken}
        [HttpGet("speler/{spelerToken}")]
        public ActionResult<SpelInfo> GetSpelBySpelerToken(string spelerToken)
        {
            Spel spel = _iRepository.GetSpelBySpelerToken(spelerToken);

            if (spel != null)
            {
                return Ok(new SpelInfo(spel));
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/spel/Beurt
        [HttpGet("beurt")]
        public ActionResult<Kleur> GetBeurt(string spelToken)
        {
            Spel spel = _iRepository.GetSpel(spelToken);

            if (spel != null)
            {
                return Ok(spel.AandeBeurt.ToString());
            }
            else
            {
                return NotFound();
            }
        }

        // POST api/spel
        [HttpPost]
        public async Task<ActionResult<Spel>> MaakSpel([FromForm] string speler1Token, [FromForm] string omschrijving)
        {
            Spel spel = new()
            {
                Token = Guid.NewGuid().ToString("N"), // "N" to prevent hypens in the guid, which will not work nicely in REST. Because, reasons.
                Speler1Token = speler1Token,
                Omschrijving = omschrijving
            };

            if (spel == null)
            {
                return BadRequest(spel);
            }

            _iRepository.AddSpel(spel);

            await _iRepository.Save();

            return Ok(new SpelInfo(spel));
        }

        [HttpPost("{spelToken}/join")]
        public async Task<ActionResult<Spel>> JoinSpel(string spelToken, [FromForm] string spelerToken)
        {
            if (spelerToken == null) return Unauthorized();

            Spel spel = _iRepository.GetSpel(spelToken);
            if (spel == null) return NotFound();

            if (spel.Speler2Token != null) return BadRequest("Geen ruimte in dit spel");
            spel.Speler2Token = spelerToken;
            spel.AandeBeurt = Kleur.Zwart;

            await _iRepository.Save();

            return Ok(spel);
        }

        // PUT api/spel/zet
        [HttpPut("/api/Spel/{spelToken}/zet")]
        public async Task<ActionResult<Spel>> DoeZet(string spelToken, [FromForm] string spelerToken, [FromForm] int row, [FromForm] int col)
        {
            if (spelerToken == null) return Unauthorized();

            Spel spel = _iRepository.GetSpel(spelToken);
            if (spel == null) return NotFound();

            if ((spel.AandeBeurt == Kleur.Wit ? spel.Speler1Token : spel.Speler2Token) != spelerToken)
            {
                return Unauthorized("Je speelt voor je beurt vriend");
            }

            if (spel.Afgelopen())
            {
                return Ok("afgelopen");
            }

            if (!spel.DoeZet(row, col))
            {
                return BadRequest("Gaan we niet doen.");
            }

            await _iRepository.Save();

            spel.JsonBord = JsonConvert.SerializeObject(spel.Bord);

            return Ok(spel);
        }

        //[HttpPut("zet")]
        //public ActionResult<string> DoeZetdepr([FromBody] Zet zet )
        //{
        //    if (zet.SpelToken == null)
        //    {
        //        return Unauthorized();
        //    }

        //    Spel spel = _iRepository.GetSpel(zet.SpelToken);

        //    if (spel == null)
        //    {
        //        return NotFound();
        //    }

        //    string status = "";

        //    try
        //    {
        //        if (zet.Passed)
        //        {
        //            spel.Pas();
        //            status = $"{spel.AandeBeurt} heeft gepast";
        //        }
        //        else
        //        {
        //            spel.DoeZet(zet.Row, zet.Col);
        //            status = $"{spel.AandeBeurt} heeft een zet gedaan";
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //        status = e.Message;
        //    }

        //    return status;
        //}

        // PUT api/spel/zet/pas
        
        [HttpPut("zet/pas")]
        public ActionResult<string> Pas([FromHeader] string spelerToken, [FromHeader] string spelToken)
        {
            _iRepository.GetSpel(spelToken).Pas();

            return Content("Gepast");
        }

        // PUT api/spel/opgeven
        [HttpPut("opgeven")]
        public ActionResult<string> GeefOp(string spelToken, string spelerToken)
        {
            _iRepository.GetSpel(spelToken).Afgelopen();

            return Ok("Opgegeven, goed zo.");
        }

        [HttpDelete("/api/spel/{spelToken}/verwijder")]
        public ActionResult<Spel> DeleteSpel(string spelToken)
        {
            Spel spel = _iRepository.GetSpel(spelToken);
            if (spel == null) return NotFound();

            _iRepository.VerwijderSpel(spel);
            _iRepository.Save();

            return Ok(spel);
        }

        [HttpPost("/forcefinish/{spelToken}")]
        public ActionResult<string> GetSpelScores(string spelToken)
        {
            Spel spel = _iRepository.GetSpel(spelToken);
            if (spel == null) return NotFound();

            spel.JsonBord = "11111111:22222211:22111221:21111121:21112211:21212221:22222222:21111110:";
            spel.AandeBeurt = Kleur.Wit;

            _iRepository.Save();

            return Ok(spel);
        }

        [HttpPost("/reset/{spelToken}")]
        public ActionResult<string> ResetSpel(string spelToken)
        {
            Spel spel = _iRepository.GetSpel(spelToken);
            if (spel == null) return NotFound();

            spel.JsonBord = "00000000:00000000:00000000:00012000:00021000:00000000:00000000:00000000:";

            spel.resetbord(spel);
            spel.AandeBeurt = Kleur.Zwart;

            _iRepository.Save();

            return Ok(spel);
        }
    }
}
