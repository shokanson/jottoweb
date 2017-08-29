using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class GuessRepository : RepositoryBase<PlayerGuess>
    {
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

            // primary key
            if (Objects.Values.FirstOrDefault(g => g.PlayerId == guess.PlayerId && g.Word == guess.Word) != null)
                throw new PkException("cannot add guess for existing game, player, and word");

            guess.Id = Guid.NewGuid().ToString();
            Objects[guess.Id] = guess;

            return guess;
        }
    }
}
