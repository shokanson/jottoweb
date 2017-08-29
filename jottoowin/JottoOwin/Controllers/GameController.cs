using Hokanson.JottoHelper;
using Hokanson.JottoRepository;
using Hokanson.JottoRepository.Exceptions;
using Hokanson.JottoRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace JottoOwin.Controllers
{
	using Hubs;
	using Models;
	using Services;

	[RoutePrefix("api/games")]
	public class GameController : ApiController
	{
		public const string StartGame = "StartGame";
		public const string Games = "Games";
		public const string Game = "Game";
		public const string LatestGame = "LatestGame";
		public const string MakeGuess = "MakeGuess";
		public const string Guesses = "Guesses";
		public const string Helps = "Helps";

		public GameController(IWordList words, IRepository<JottoPlayer> players, IRepository<JottoGame> games, IRepository<PlayerGuess> guesses)
		{
            _words = words;
            _players = players;
            _games = games;
            _guesses = guesses;
        }

        private readonly IWordList _words;
        private readonly IRepository<JottoPlayer> _players;
        private readonly IRepository<JottoGame> _games;
        private readonly IRepository<PlayerGuess> _guesses;
        private static readonly Dictionary<string, Dictionary<string, JottoGameHelper>> Helpers = new Dictionary<string, Dictionary<string, JottoGameHelper>>();

        [HttpPost]
        [Route(Name = StartGame)]
        public async Task<IHttpActionResult> StartGameAsync([FromBody] JottoGameModel game)
        {
            //  invariants:
            //      player1 must always be human

            // validation
            JottoPlayer player1 = await _players.GetAsync(game.Player1Id);
            JottoPlayer player2 = await _players.GetAsync(game.Player2Id);

            if (player1 == null) return BadRequest("player1 refers to non-existent player");
            if (player2 == null) return BadRequest("player2 refers to non-existent player");

            if (player1.IsComputer) return BadRequest("player 1 must be human");

            if (!player2.IsComputer && string.IsNullOrEmpty(game.Word1))
                return BadRequest("player 1 must provide a word if not playing against the computer");
            // note, it's OK for player 2 not to have provided a word yet

            // logic

            // get computer word
            if (player2.IsComputer) game.Word2 = _words.GetRandomWord();

            JottoGame jottoGame = await _games.AddAsync(new JottoGame
            {
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                Word1 = game.Word1,
                Word2 = game.Word2
            });
            await _games.SaveChangesAsync();

            SetupHelpers(player2, jottoGame);

			GameHub.NotifyClientsOfGameStarted(jottoGame);

			return CreatedAtRoute(Game, new { gameId = jottoGame.Id }, jottoGame);
		}

		[HttpPatch]
		[Route("{gameId}")]
		public async Task<IHttpActionResult> UpdateGameAsync(string gameId, [FromBody] string word2)
		{
			var game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

            game.Word2 = word2;
			game = await _games.UpdateAsync(gameId, game);
            await _games.SaveChangesAsync();

			GameHub.NotifyClientsOfGameUpdated(game);

			return Ok(new { id = game.Id, player1Id = game.Player1Id, player2Id = game.Player2Id });
		}

		[HttpGet]
		[Route("{gameId}", Name = Game)]
		public async Task<IHttpActionResult> GetGameAsync(string gameId)
		{
			var game = await _games.GetAsync(gameId);

			if (game == null) return NotFound();

			return Ok(game);
		}

		[HttpGet]
		[Route(Name = Games)]
		public async Task<IHttpActionResult> GetGamesAsync()
		{
			return Ok(await _games.GetAllAsync());
		}

		[HttpGet]
		[Route("latest", Name = LatestGame)]
		public async Task<IHttpActionResult> GetLatestGameAsync()
		{
            // hate this implementation
			var games = await _games.GetAllAsync();
			if (games.Count() == 0) return Ok();

			var maxTicks = games.Max(g => g.CreationDate.Ticks);
			return Ok(await _games.GetAsync(g => g.CreationDate.Ticks == maxTicks));
		}

		[HttpGet]
		[Route("{gameId}/guesses", Name = Guesses)]
		public async Task<IHttpActionResult> GetGuessesAsync(string gameId)
		{
			return Ok(await _guesses.GetAllAsync(g => g.GameId == gameId));
		}

		[HttpGet]
		[Route("{gameId}/helps", Name = Helps)]
		public async Task<IHttpActionResult> GetHelpsAsync(string gameId)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

			// build response
			Dictionary<string, JottoGameHelper> helpers;
			lock (Helpers)
			{
				helpers = Helpers[gameId];
			}

			var helperModels = new List<GameHelperModel>();
			if (helpers != null)
			{
				JottoGameHelper helper;
				if (helpers.TryGetValue(game.Player1Id, out helper) && helper != null)
				{
					helperModels.Add(CreateModelFromHelper(helper));
				}
				if (helpers.TryGetValue(game.Player2Id, out helper) && helper != null)
				{
					helperModels.Add(CreateModelFromHelper(helper));
				}
			}

			return Ok(helperModels);
		}

		private static GameHelperModel CreateModelFromHelper(JottoGameHelper helper)
		{
			var knownIn = new List<char>();
			foreach (var pair in helper.KnownIn)
			{
				for (var i = 0; i < pair.Value; i++)
				{
					knownIn.Add(pair.Key);
				}
			}

			return new GameHelperModel
			{
				KnownIn = new string(knownIn.OrderBy(c => c).ToArray()),
				KnownOut = new string(helper.KnownOut.OrderBy(c => c).ToArray()),
				Unknown = new string("abcdefghijklmnopqrstuvwxyz".Except(knownIn).Except(helper.KnownOut).OrderBy(c => c).ToArray()),
				Clues = helper.Clues.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)).ToList()
			};
		}

		[HttpPost]
		[Route("{gameId}/helps")]
		public async Task<IHttpActionResult> SupplyCommandAsync(string gameId, [FromBody] HelperHintModel hint)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

			// logic
			switch (hint.Command)
			{
				case "knownin":
					if (string.IsNullOrEmpty(hint.Letter) || hint.Letter.Length != 1) return BadRequest($"bad hint letter: '{hint.Letter}'");
					return await RemoveKnownInAsync(gameId, hint.PlayerId, hint.Letter[0]);
				case "knownout":
					if (string.IsNullOrEmpty(hint.Letter) || hint.Letter.Length != 1) return BadRequest($"bad hint letter: '{hint.Letter}'");
					return await RemoveKnownOutAsync(gameId, hint.PlayerId, hint.Letter[0]);
				case "+":
				case "-":
					return await InformHelperAsync(gameId, hint);
				case "reset":
					{
						JottoGameHelper helper;
						lock (Helpers)
						{
							if (Helpers[game.Id] == null) return BadRequest("cannot add hint for completed game");
							if (!Helpers[game.Id].TryGetValue(hint.PlayerId, out helper)) return BadRequest("hint for non-existent player");
							if (helper == null) return BadRequest("cannot add hint for player who already got jotto");
						}

						// logic
						helper.Reset();

						return Ok(hint);
					}
				default:
					return Ok(hint);
			}
		}

		[HttpDelete]
		[Route("{gameId}/{playerId}/helps/knownin/{c}")]
		public async Task<IHttpActionResult> RemoveKnownInAsync(string gameId, string playerId, char c)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

			JottoGameHelper helper;
			lock (Helpers)
			{
				if (Helpers[game.Id] == null) return BadRequest("cannot add hint for completed game");
				if (!Helpers[game.Id].TryGetValue(playerId, out helper)) return BadRequest("hint for non-existent player");
				if (helper == null) return BadRequest("cannot add hint for player who already got jotto");
			}

			// logic
			helper.RemoveKnownIn(c);

			return Ok();
		}

		[HttpDelete]
		[Route("{gameId}/{playerId}/helps/knownout/c")]
		public async Task<IHttpActionResult> RemoveKnownOutAsync(string gameId, string playerId, char c)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

			JottoGameHelper helper;
			lock (Helpers)
			{
				if (Helpers[game.Id] == null) return BadRequest("cannot add hint for completed game");
				if (!Helpers[game.Id].TryGetValue(playerId, out helper)) return BadRequest("hint for non-existent player");
				if (helper == null) return BadRequest("cannot add hint for player who already got jotto");
			}

			// logic
			helper.RemoveKnownOut(c);

			return Ok();
		}

		// POSTing a letter to helps/unknown means one of three things;
		//      if hint.command == '-', remove letter from unknown and add to known out
		//      if hint.command == '+',
		//          if letter already exists in known in, add another instance of that letter to known in
		//          if letter doesn't exist in known in, remove from unknown and add to known in
		[HttpPost]
		[Route("{gameId}/helps/unknown")]
		public async Task<IHttpActionResult> InformHelperAsync(string gameId, [FromBody] HelperHintModel hint)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();

			if (string.IsNullOrEmpty(hint.Letter)) return BadRequest("hint letter must be provided");

			// logic
			JottoGameHelper helper;
			lock (Helpers)
			{
				if (Helpers[game.Id] == null) return BadRequest("cannot add hint for completed game");
				if (!Helpers[game.Id].TryGetValue(hint.PlayerId, out helper)) return BadRequest("hint for non-existent player");
				if (helper == null) return BadRequest("cannot add hint for player who already got jotto");
			}

			try
			{
				switch (hint.Command)
				{
					case "-":
						helper.AddKnownOut(hint.Letter[0]);
						break;
					case "+":
						helper.AddKnownIn(hint.Letter[0]);
						break;
				}
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}

			return Ok(hint);
		}

		[HttpPost]
		[Route("{gameId}/guesses", Name = MakeGuess)]
		public async Task<IHttpActionResult> MakeGuessAsync(string gameId, [FromBody] JottoGuessModel guess)
		{
			// validation
			JottoGame game = await _games.GetAsync(gameId);
			if (game == null) return NotFound();
			if (string.IsNullOrEmpty(game.Word2)) return BadRequest("game not ready to begin--player 2's word hasn't been provided yet");
			if (string.IsNullOrEmpty(guess.Guess) || guess.Guess.Length != 5) return BadRequest("guess must be 5 letters");
			if (!_words.IsWordInList(guess.Guess)) return BadRequest(string.Format("'{0}' not in allowed word list", guess.Guess));

			JottoPlayer player = await _players.GetAsync(guess.PlayerId);
			if (player == null) return BadRequest("player doesn't exist");

			JottoPlayer forPlayer = await _players.GetAsync(guess.ForPlayerId);
			if (forPlayer == null) return BadRequest("\"for\" player doesn't exist");

			// logic
			var jottoWord = new JottoWord(
				 guess.ForPlayerId == game.Player1Id
					  ? game.Word1
					  : game.Word2);
			int guessVal =
				 jottoWord.IsJotto(guess.Guess)
					  ? 6 // 6 means JOTTO
					  : jottoWord.GetCount(guess.Guess);
			var playerGuess = new PlayerGuess
            {
                GameId = gameId,
                PlayerId = guess.PlayerId,
                Word = guess.Guess,
                Score = guessVal
            };
			try
			{
				playerGuess = await _guesses.AddAsync(playerGuess);
                await _guesses.SaveChangesAsync();

				if (playerGuess.Score == 6)
				{
					TeardownHelpers(playerGuess);
				}
				else
				{
					UpdateHelper(playerGuess);
				}
			}
			catch (PkException)
			{
				return BadRequest("already guessed that word");
			}
			catch (FkException fe)
			{
				return BadRequest(fe.Message);
			}

			GameHub.NotifyClientsOfTurnTaken(playerGuess);

			return CreatedAtRoute(Guesses, new { gameId }, playerGuess);
		}

		private static void SetupHelpers(JottoPlayer player2, JottoGame jottoGame)
		{
			lock (Helpers)
			{
				Helpers[jottoGame.Id] = new Dictionary<string, JottoGameHelper>();
				Helpers[jottoGame.Id][jottoGame.Player1Id] = new JottoGameHelper();
				if (!player2.IsComputer) Helpers[jottoGame.Id][jottoGame.Player2Id] = new JottoGameHelper();
			}
		}

		private static void UpdateHelper(PlayerGuess playerGuess)
		{
			lock (Helpers)
			{
				Helpers[playerGuess.GameId][playerGuess.PlayerId].HandleGuess(playerGuess.Word, playerGuess.Score);
			}
		}

		private static void TeardownHelpers(PlayerGuess playerGuess)
		{
			lock (Helpers)
			{
				// tear down this player's helper
				Helpers[playerGuess.GameId][playerGuess.PlayerId] = null;

				// tear down this game's collection of helpers
				if (Helpers[playerGuess.GameId].All(pair => pair.Value == null)) Helpers[playerGuess.GameId] = null;
			}
		}
	}
}
