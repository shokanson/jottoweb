using Hokanson.JottoRepository;
using Hokanson.JottoRepository.Exceptions;
using Hokanson.JottoRepository.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace JottoOwin.Controllers
{
    using Hubs;

    [RoutePrefix("api/players")]
    public class PlayerController : ApiController
    {
        public const string Player = "Player";
        public const string Players = "Players";

        public PlayerController(IJottoRepository repo)
        {
            _repo = repo;
        }

        private readonly IJottoRepository _repo;
        
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> RegisterPlayerAsync([FromBody] string player)
        {
            // validation
            if (string.IsNullOrEmpty(player)) return BadRequest("name must be provided");

            try
            {
                var jottoPlayer = await _repo.AddPlayerAsync(player);

                GameHub.NotifyClientsOfPlayerAdded(jottoPlayer);

                return CreatedAtRoute(Player, new { playerIdOrName = jottoPlayer.Id }, jottoPlayer);
            }
            catch (PkException pe)
            {
                return BadRequest(pe.Message);
            }
        }

        [HttpGet]
        [Route("", Name = Players)]
        public async Task<IHttpActionResult> GetPlayersAsync()
        {
            return Ok(await _repo.GetPlayersAsync());
        }

        [HttpGet]
        [Route("{playerIdOrName}", Name = Player)]
        public async Task<IHttpActionResult> GetPlayerAsync(string playerIdOrName)
        {
            JottoPlayer player;
            Guid guid;
            if (Guid.TryParse(playerIdOrName, out guid))
            {
                player = await _repo.GetPlayerByIdAsync(playerIdOrName);
            }
            else
            {
                player = await _repo.GetPlayerByNameAsync(playerIdOrName);
            }
            
            if (player == null) return NotFound();
            
            return Ok(player);
        }
    }
}
