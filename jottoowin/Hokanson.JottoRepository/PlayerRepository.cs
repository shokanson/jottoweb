using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hokanson.JottoRepository
{
    using Exceptions;
    using Models;

    public class PlayerRepository : RepositoryBase<JottoPlayer>
    {
        static PlayerRepository()
        {
            string id = Guid.NewGuid().ToString();
            Objects[id] = new JottoPlayer
            {
                IsComputer = true,
                Id = id,
                Name = "Computer"
            };
        }

        public override Task<JottoPlayer> AddAsync(JottoPlayer player)
        {
            // alternate key
            if (Objects.Values.FirstOrDefault(p => string.Compare(p.Name, player.Name, StringComparison.OrdinalIgnoreCase) == 0) != null)
                throw new PkException("player with that name already exists");

            player.Id = Guid.NewGuid().ToString();
            player.IsComputer = false;

            Objects[player.Id] = player;

            return Task.FromResult(player);
        }
    }
}
