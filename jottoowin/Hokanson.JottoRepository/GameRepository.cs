using System;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class GameRepository : RepositoryBase<JottoGame>
    {
        public GameRepository(IRepository<JottoPlayer> players)
        {
            _players = players;
        }

        private readonly IRepository<JottoPlayer> _players;

        public override async Task<JottoGame> AddAsync(JottoGame game)
        {
            // referential integrity
            if (await _players.GetAsync(game.Player1Id) == null) throw new FkException("cannot add game for non-existent player");
            if (await _players.GetAsync(game.Player2Id) == null) throw new FkException("cannot add game for non-existent player");

            game.Id = Guid.NewGuid().ToString();
            game.CreationDate = DateTime.Now;

            Objects[game.Id] = game;

            return game;
        }

        public override Task<JottoGame> UpdateAsync(string id, JottoGame game)
        {
            if (!Objects.ContainsKey(id)) throw new Exception("non-existent game");
            if (id != game.Id) throw new Exception($"{nameof(game.Id)} does not match provided {nameof(id)}");

            Objects[id] = game;

            return Task.FromResult(game);
        }
    }
}
