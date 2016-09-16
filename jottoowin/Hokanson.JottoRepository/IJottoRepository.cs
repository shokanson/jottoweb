using Hokanson.JottoRepository.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
	public interface IJottoRepository
	{
		// players
		Task<JottoPlayer> AddPlayerAsync(string name);
		Task<IEnumerable<JottoPlayer>> GetPlayersAsync();
		Task<JottoPlayer> GetPlayerByIdAsync(string id);
		Task<JottoPlayer> GetPlayerByNameAsync(string name);

		// games
		Task<JottoGame> AddGameAsync(string player1Id, string player2Id, string player1Word, string player2Word);
		Task<IEnumerable<JottoGame>> GetGamesAsync();
		Task<JottoGame> GetGameAsync(string gameId);
		Task<JottoGame> FindGameAsync(Func<JottoGame, bool> predicate);
		Task<JottoGame> UpdateGameWord2Async(string gameId, string word);

		// guesses
		Task<PlayerGuess> AddPlayerGuessAsync(string gameId, string playerId, string word, int score);
		Task<IEnumerable<PlayerGuess>> GetGuessesForGameAsync(string gameId);

		// words
		Task<string> GetRandomWordAsync();
		Task<bool> IsWordAsync(string word);
	}
}