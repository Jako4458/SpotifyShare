using SpotifyController.Model;
using SpotifyController.Model.SpotifyAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpotifyController.Interfaces
{
    public interface IDataRepository
    {
        Task<(bool, User)> TryGetUser(string UserID, bool UserOnly=false);
        Task AddUser(string userID);
        Task AddUser(User User);
        Task AddSpotifyConnection(string UserID, SpotifySession session, SpotifyAPIToken spotifyToken);
        Task UpdateSpotifyToken(SpotifyAPIToken spotifyToken);
        Task ToggleShareSpotifySession(string UserID);
        Task<List<SpotifySession>> GetPublicSessionList();
        Task<(bool, SpotifySession)> GetPublicSessionFromID(string sessionID);

    }
}
