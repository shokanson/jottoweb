using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
	using Exceptions;
	using Models;

	public class JottoRepository : IJottoRepository
	{
		private static readonly Dictionary<string, JottoGame> Games = new Dictionary<string, JottoGame>();  // gameId -> game
		private static readonly HashSet<string> Words = new HashSet<string>();
		private static readonly Dictionary<string, List<PlayerGuess>> Guesses = new Dictionary<string, List<PlayerGuess>>();    // gameId -> list of guesses
		private static readonly Dictionary<string, JottoPlayer> Players = new Dictionary<string, JottoPlayer>();    // playerId -> player

		static JottoRepository()
		{
			System.Diagnostics.Trace.TraceInformation(AppDomain.CurrentDomain.BaseDirectory + "\\fiveletterwords.lst");
			using (var reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\fiveletterwords.lst"))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (!string.IsNullOrEmpty(line)) Words.Add(line);
				}
			}

			var jottoPlayer = new JottoPlayer
			{
				IsComputer = true,
				Id = Guid.NewGuid().ToString(),
				Name = "Computer"
			};
			Players[jottoPlayer.Id] = jottoPlayer;
		}

		public Task<string> GetRandomWordAsync()
		{
			return Task.FromResult(Words.Skip(new Random().Next(0, Words.Count))
												 .Take(1)
												 .First());
		}

		public Task<bool> IsWordAsync(string word)
		{
			return Task.FromResult(Words.Contains(word));
		}

		public Task<JottoPlayer> AddPlayerAsync(string name)
		{
			// alternate key
			if (Players.Values.FirstOrDefault(p => string.Compare(p.Name, name, StringComparison.OrdinalIgnoreCase) == 0) != null)
				throw new PkException("player with that name already exists");

			var player = new JottoPlayer
			{
				Id = Guid.NewGuid().ToString(),
				Name = name,
				IsComputer = false
			};
			Players[player.Id] = player;

			return Task.FromResult(player);
		}

		public Task<IEnumerable<PlayerGuess>> GetGuessesForGameAsync(string gameId)
		{
			return Task.FromResult(
				Guesses.ContainsKey(gameId)
					? Guesses[gameId].AsEnumerable()
					: new List<PlayerGuess>()
				);
		}

		public Task<PlayerGuess> AddPlayerGuessAsync(string gameId, string playerId, string word, int score)
		{
			// referential integrity
			if (!Games.ContainsKey(gameId)) throw new FkException("cannot add guess for non-existent game");
			if (!Players.ContainsKey(playerId)) throw new FkException("cannot add guess for non-existent player");

			// primary key
			if (Guesses[gameId].FirstOrDefault(g => g.PlayerId == playerId && g.Word == word) != null)
				throw new PkException("cannot add guess for existing game, player, and word");

			var guess = new PlayerGuess
			{
				GameId = gameId,
				PlayerId = playerId,
				Word = word,
				Score = score
			};
			Guesses[guess.GameId].Add(guess);

			return Task.FromResult(guess);
		}

		public Task<JottoGame> AddGameAsync(string player1Id, string player2Id, string player1Word, string player2Word)
		{
			// referential integrity
			if (!Players.ContainsKey(player1Id)) throw new FkException("cannot add game for non-existent player");
			if (!Players.ContainsKey(player2Id)) throw new FkException("cannot add game for non-existent player");

			var game = new JottoGame
			{
				Id = Guid.NewGuid().ToString(),
				Player1Id = player1Id,
				Player2Id = player2Id,
				Word1 = player1Word,
				Word2 = player2Word,
				CreationDate = DateTime.Now
			};
			Games[game.Id] = game;
			Guesses[game.Id] = new List<PlayerGuess>();

			return Task.FromResult(game);
		}

		public Task<JottoGame> UpdateGameWord2Async(string gameId, string word)
		{
			if (!Games.ContainsKey(gameId)) throw new Exception("non-existent game");

			JottoGame game = Games[gameId];
			game.Word2 = word;

			return Task.FromResult(game);
		}

		public Task<JottoGame> GetGameAsync(string gameId)
		{
			JottoGame game;
			Games.TryGetValue(gameId, out game);

			return Task.FromResult(game);
		}

		public Task<IEnumerable<JottoGame>> GetGamesAsync()
		{
			return Task.FromResult(Games.Values.AsEnumerable());
		}

		public Task<JottoGame> FindGameAsync(Func<JottoGame, bool> predicate)
		{
			return Task.FromResult(Games.Values.FirstOrDefault(predicate));
		}

		public Task<JottoPlayer> GetPlayerByIdAsync(string playerId)
		{
			JottoPlayer player;
			Players.TryGetValue(playerId, out player);

			return Task.FromResult(player);
		}

		public Task<JottoPlayer> GetPlayerByNameAsync(string name)
		{
			return Task.FromResult(Players.Values.FirstOrDefault(player => string.Compare(player.Name, name, StringComparison.OrdinalIgnoreCase) == 0));
		}

		public Task<IEnumerable<JottoPlayer>> GetPlayersAsync()
		{
			return Task.FromResult(Players.Values.AsEnumerable());
		}
	}
}