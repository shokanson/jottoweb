using Hokanson.JottoRepository;
using Hokanson.JottoRepository.Exceptions;
using Hokanson.JottoRepository.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace JottoOwin.Controllers
{
	using Hubs;
    using System.Linq;

    [RoutePrefix("api/players")]
	public class PlayerController : ApiController
	{
		public const string Player = "Player";
		public const string Players = "Players";
        public const string PlayerAverage = "PlayerAverage";

        public PlayerController(IRepository<JottoPlayer> players, IRepository<JottoGame> games)
		{
			_players = players;
            _games = games;
		}

		private readonly IRepository<JottoPlayer> _players;
        private readonly IRepository<JottoGame> _games;

        [HttpPost]
		[Route("")]
		public async Task<IHttpActionResult> RegisterPlayerAsync([FromBody] string player)
		{
			// validation
			if (string.IsNullOrEmpty(player)) return BadRequest("name must be provided");

			try
			{
				var jottoPlayer = await _players.AddAsync(new JottoPlayer { Name = player });
				await _players.SaveChangesAsync();

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
		public async Task<IHttpActionResult> GetPlayersAsync() => Ok(await _players.GetAllAsync());

		[HttpGet]
		[Route("{playerIdOrName}", Name = Player)]
		public async Task<IHttpActionResult> GetPlayerAsync(string playerIdOrName)
		{
			JottoPlayer player = Guid.TryParse(playerIdOrName, out Guid guid)
				? await _players.GetAsync(playerIdOrName)
				: await _players.GetAsync(p => p.Name == playerIdOrName);

			if (player == null) return NotFound();

			return Ok(player);
		}

        [HttpGet]
        [Route("{playerIdOrName}/average", Name = PlayerAverage)]
        public async Task<IHttpActionResult> GetPlayerAverageAsync(string playerIdOrName)
        {
            JottoPlayer player = Guid.TryParse(playerIdOrName, out Guid guid)
                ? await _players.GetAsync(playerIdOrName)
                : await _players.GetAsync(p => p.Name == playerIdOrName);

            if (player == null) return NotFound();
                        
            return Ok((await _games.GetAllAsync(game => game.Player1Id == player.Id || game.Player2Id == player.Id)).Count());
        }
    }
}
