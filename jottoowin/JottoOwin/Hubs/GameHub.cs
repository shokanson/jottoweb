using Hokanson.JottoRepository.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace JottoOwin.Hubs
{
    [HubName("game")]
    public class GameHub : Hub
    {
        static GameHub()
        {
            // Forces .NET types sent from server to client to appear as idiomatic JSON at client
            GlobalHost.DependencyResolver
                      .Register(typeof(JsonSerializer),
                                () => JsonSerializer.Create(new JsonSerializerSettings {
                                    ContractResolver = new SignalRContractResolver()
                                }));
        }

        public Task JoinGame(string gameId) => Groups.Add(Context.ConnectionId, gameId);

        public Task LeaveGame(string gameId) => Groups.Remove(Context.ConnectionId, gameId);

        public static void NotifyClientsOfPlayerAdded(JottoPlayer jottoPlayer)
        {
            GlobalHost.ConnectionManager
                      .GetHubContext<GameHub>()
                      .Clients
                      .All  // notify everyone
                      .playerAdded(jottoPlayer);
        }

        public static void NotifyClientsOfGameStarted(JottoGame jottoGame)
        {
            // strip out words so we don't communicate them to clients
            var filteredGame = (JottoGame)jottoGame.Clone();
            filteredGame.Word1 = filteredGame.Word2 = null;

            GlobalHost.ConnectionManager
                      .GetHubContext<GameHub>()
                      .Clients
                      .All  // notify everyone
                      .gameStarted(filteredGame);
        }

        public static void NotifyClientsOfGameUpdated(JottoGame jottoGame)
        {
            // strip out words so we don't communicate them to clients
            var filteredGame = (JottoGame)jottoGame.Clone();
            filteredGame.Word1 = filteredGame.Word2 = null;

            GlobalHost.ConnectionManager
                      .GetHubContext<GameHub>()
                      .Clients
                      .All  // notify everyone
                      .gameUpdatedWord2(filteredGame);
        }

        public static void NotifyClientsOfTurnTaken(PlayerGuess guess)
        {
            GlobalHost.ConnectionManager
                      .GetHubContext<GameHub>()
                      .Clients
                      .Group(guess.GameId)  // notify just game players
                      .turnTaken(guess);
        }
    }

    // This allows .NET types sent from server to client to appear as idiomatic JSON at client.
    // Only does this for types external to the SignalR assembly
    internal class SignalRContractResolver : IContractResolver
    {
        private readonly Assembly _assembly = typeof(Connection).Assembly;
        private readonly IContractResolver _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
        private readonly IContractResolver _defaultContractResolver = new DefaultContractResolver();

        #region IContractResolver Members

        public JsonContract ResolveContract(Type type) =>
            type.Assembly.Equals(_assembly)
                ? _defaultContractResolver.ResolveContract(type)
                : _camelCaseContractResolver.ResolveContract(type);

        #endregion
    }
}