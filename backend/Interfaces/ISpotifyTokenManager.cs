using SpotifyController.Model;
using SpotifyController.Model.SpotifyAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpotifyController.Interfaces
{
    public interface ISpotifyTokenManager
    {
        //string AuthorizeUser(string session_id, string redirect_uri, bool showDialog);
        Task<(bool, SpotifyAPIToken)> GetToken(SpotifySession session, string tokenType);
    }
}
