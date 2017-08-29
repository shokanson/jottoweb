using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class GameRepository : IRepository<JottoGame>
    {
        private static readonly ConcurrentDictionary<string, JottoGame> Games = new ConcurrentDictionary<string, JottoGame>();  // gameId -> game

        public GameRepository(IRepository<JottoPlayer> players)
        {
            _players = players;
        }

        private readonly IRepository<JottoPlayer> _players;

        public async Task<JottoGame> AddAsync(JottoGame game)
        {
            // referential integrity
            if (await _players.GetAsync(game.Player1Id) == null) throw new FkException("cannot add game for non-existent player");
            if (await _players.GetAsync(game.Player2Id) == null) throw new FkException("cannot add game for non-existent player");

            game.Id = Guid.NewGuid().ToString();
            game.CreationDate = DateTime.Now;

            Games[game.Id] = game;

            return game;
        }

        public Task<IEnumerable<JottoGame>> GetAllAsync()
        {
            return Task.FromResult(Games.Values.AsEnumerable());
        }

        public Task<JottoGame> GetAsync(string id)
        {
            Games.TryGetValue(id, out JottoGame game);

            return Task.FromResult(game);
        }

        public Task<JottoGame> GetAsync(Func<JottoGame, bool> predicate)
        {
            return Task.FromResult(Games.Values.FirstOrDefault(predicate));
        }

        public Task<IEnumerable<JottoGame>> GetAllAsync(Func<JottoGame, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<JottoGame> UpdateAsync(string id, JottoGame game)
        {
            if (!Games.ContainsKey(id)) throw new Exception("non-existent game");
            if (id != game.Id) throw new Exception($"{nameof(game.Id)} does not match provided {nameof(id)}");

            Games[id] = game;

            return Task.FromResult(game);
        }

        public Task SaveChangesAsync()
        {
            return Task.FromResult(0);
        }
    }
}
