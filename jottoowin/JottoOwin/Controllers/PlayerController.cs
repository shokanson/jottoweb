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

		public PlayerController(IRepository<JottoPlayer> players)
		{
			_players = players;
		}

		private readonly IRepository<JottoPlayer> _players;

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
	}
}
