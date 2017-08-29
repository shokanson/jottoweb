using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class GuessRepository : RepositoryBase<PlayerGuess>
    {
        // make sure to lock this for reads/writes
        private static readonly HashSet<PlayerGuess> Guesses = new HashSet<PlayerGuess>();

        public GuessRepository(IRepository<JottoPlayer> players, IRepository<JottoGame> games)
        {
            _players = players;
            _games = games;
        }

        private readonly IRepository<JottoPlayer> _players;
        private readonly IRepository<JottoGame> _games;

        public override async Task<PlayerGuess> AddAsync(PlayerGuess guess)
        {
            // referential integrity
            if (await _games.GetAsync(guess.GameId) == null) throw new FkException("cannot add guess for non-existent game");
            if (await _players.GetAsync(guess.PlayerId) == null) throw new FkException("cannot add guess for non-existent player");

            lock (Guesses)
            {
                // primary key
                if (Guesses.FirstOrDefault(g => g.PlayerId == guess.PlayerId && g.Word == guess.Word) != null)
                    throw new PkException("cannot add guess for existing game, player, and word");

                Guesses.Add(guess);
            }

            return guess;
        }

        public override Task<IEnumerable<PlayerGuess>> GetAllAsync(Func<PlayerGuess, bool> predicate)
        {
            lock (Guesses)
            {
                return Task.FromResult(Guesses.Where(predicate));
            }
        }

        public override Task<PlayerGuess> UpdateAsync(string id, PlayerGuess guess)
        {
            throw new NotImplementedException();
        }
    }
}
