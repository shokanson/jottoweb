using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class PlayerRepository : IRepository<JottoPlayer>
    {
        private static readonly ConcurrentDictionary<string, JottoPlayer> Players = new ConcurrentDictionary<string, JottoPlayer>();    // playerId -> player

        static PlayerRepository()
        {
            var jottoPlayer = new JottoPlayer
            {
                IsComputer = true,
                Id = Guid.NewGuid().ToString(),
                Name = "Computer"
            };
            Players[jottoPlayer.Id] = jottoPlayer;
        }

        public Task<JottoPlayer> AddAsync(JottoPlayer player)
        {
            // alternate key
            if (Players.Values.FirstOrDefault(p => string.Compare(p.Name, player.Name, StringComparison.OrdinalIgnoreCase) == 0) != null)
                throw new PkException("player with that name already exists");

            player.Id = Guid.NewGuid().ToString();
            player.IsComputer = false;

            Players[player.Id] = player;

            return Task.FromResult(player);
        }

        public Task<IEnumerable<JottoPlayer>> GetAllAsync()
        {
            return Task.FromResult(Players.Values.AsEnumerable());
        }

        public Task<JottoPlayer> GetAsync(string id)
        {
            Players.TryGetValue(id, out JottoPlayer player);

            return Task.FromResult(player);
        }

        public Task<JottoPlayer> GetAsync(Func<JottoPlayer, bool> predicate)
        {
            return Task.FromResult(Players.Values.FirstOrDefault(predicate));
        }

        public Task<IEnumerable<JottoPlayer>> GetAllAsync(Func<JottoPlayer, bool> predicate)
        {
            return Task.FromResult(Players.Values.Where(predicate));
        }

        public Task<JottoPlayer> UpdateAsync(string id, JottoPlayer player)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            return Task.FromResult(0);
        }
    }
}
