using SpotifyController.Exceptions;
using SpotifyController.Interfaces;
using SpotifyController.Model;
using SpotifyController.Model.SpotifyAPI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyController.Services
{
    public class SpotifyAPIService: ISpotifyAPIService
    {
        private SpotifySession _clientPublicSession;
        private SpotifyTokenManager _spotifyTokenManager;
        private static string Host = null;

        public SpotifyAPIService(HttpClient httpClient, IConfiguration config, SpotifyTokenManager spotifyTokenManager)
        {
            _clientId = Environment.GetEnvironmentVariable("SPOTIFY_API_CLIENT_ID"); 
            _clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_API_CLIENT_SECRET"); 
            
            _spotifyTokenManager = spotifyTokenManager;
            _clientPublicSession = new SpotifySession() {Id = _clientId, SpotifyToken = new SpotifyAPIToken() {AccessTokenExpiration = DateTime.Now}};

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Host = Environment.GetEnvironmentVariable("HOST_IP");
            if (Host==null)
                throw new Exception("Invalid Host IP - Update in Dockerfile");
        }

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.spotify.com/v1";
        public static string RedirectUri => $"https://{Host}/api/user/authorizeSpotify";


        private readonly string _clientId;
        private readonly string _clientSecret;

        // ShowDialog should be true for final/publish build
        public string AuthorizeUser(string session_id, string redirect_uri="/", bool showDialog = false)
        {
            string state = $"session_id={session_id};redirect_uri={redirect_uri}";
            List<string> authScopes = new List<string>()
            {
                "user-read-recently-played",
                "user-read-playback-state",
                "user-modify-playback-state",
                "user-read-currently-playing",
                "user-read-private",
                "user-library-read",
                "playlist-read-collaborative",
                "streaming",
            };

            string scopesAsString = authScopes.Select(scope => scope == authScopes[authScopes.Count-1] ? scope : $"{scope}%20").Aggregate((acc, scope) => $"{acc}{scope}");
            //string authPath = $"https://accounts.spotify.com/authorize?client_id={_clientId}&response_type=code&redirect_uri={RedirectUri}&scope={scopesAsString}&show_dialog={showDialog}";
            string authPath = $"https://accounts.spotify.com/authorize?client_id={_clientId}&response_type=code&redirect_uri={RedirectUri}&scope={scopesAsString}&show_dialog={showDialog}&state={state}";

            return authPath;
        }

        private async Task<(bool, T, string)> SendAPIRequest<T>(SpotifySession session, string method, string url, HttpContent content=null)
        {
            if (session is null)
                throw new SpotifyNotConnectedException();

            // if expired and refresh not succesfull return false
            if (session.SpotifyToken.AccessTokenExpiration < DateTime.Now) {
                string tokenType = "private";

                if (session.Id == _clientId)
                    tokenType = "client_credentials";

                 (bool success, SpotifyAPIToken spotifyToken) = await _spotifyTokenManager.GetToken(session, tokenType);
                 if (!success)
                    return (false, default, $"Error Couldn't Get Spotify Token!");

                session.SpotifyToken = spotifyToken;
            }

            HttpMethod requestMethod;

            switch (method.ToUpper())
            {
                case "GET":
                    requestMethod = HttpMethod.Get;
                    break;
                case "POST":
                    requestMethod = HttpMethod.Post;
                    break;
                case "PUT":
                    requestMethod = HttpMethod.Put;
                    break;
                case "DELETE":
                    requestMethod = HttpMethod.Delete;
                    break;
                default:
                    return (false, default, $"Invalid HTTP Method! - '{method.ToUpper()}'");
            }

            var request = new HttpRequestMessage(requestMethod, $"{_baseUrl}{url}");
            request.Headers.Add("Authorization", $"Bearer {session.SpotifyToken.AccessToken}");

            // if not GET add content/body
            if (requestMethod != HttpMethod.Get)
                request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var resContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return (false, default, resContent);

            try
            {
                return (true, JsonConvert.DeserializeObject<T>(resContent), resContent);
            }
            catch (JsonReaderException e)
            {
                // for debugging - raw content will still be returned when json deserialization fails
                return (true, default, resContent);
            }
            //return (true, default, resContent);
        }

        /////////////////////////////////////////////////////////////////
        
        public async Task<(bool, Playlist, string)> GetPlaylist(SpotifySession session, string playlistId)
        {
            if (session == null)
                session = _clientPublicSession;

            string url = $"/playlists/{playlistId}";

            var response = await SendAPIRequest<Playlist>(session, "GET", url);
            return response;
        }

        public async Task<(bool, PlaylistTracks, string)> GetPlaylistTracks(SpotifySession session, string playlistId, int offset=0, int limit=20)
        {
            if (session == null)
                session = _clientPublicSession;
            
            string url = $"/playlists/{playlistId}/tracks?offset={offset}&limit={limit}";

            var response = await SendAPIRequest<PlaylistTracks>(session, "GET", url);
            return response;
        }       

        public async Task<(bool, Track, string)> GetTrack(SpotifySession session, string trackId)
        {
            string url = $"/tracks/{trackId}";

            var response = await SendAPIRequest<Track>(session, "GET", url);
            return response;
        }
        public async Task<(bool, string)> QueueTrack(SpotifySession session, string trackId)
        {
            string url = $"/me/player/queue?uri=spotify%3Atrack%3A{trackId}";

            var response = await SendAPIRequest<string>(session, "POST", url);
            return (response.Item1, response.Item2);
        }

        public async Task<(bool, Search, string)> Search(string query)
        {
            string url = "/search?q=track:" + query + "&type=album,artist,playlist,track,show,episode&market=dk";

            // var response = await SendAPIRequest<Search>(session, "GET", url);
            var response = await SendAPIRequest<Search>(_clientPublicSession, "GET", url);
            return response;
        }

        public async Task<(bool, Playlists, string)> GetCurrentUsersPlaylists(SpotifySession session)
        {
            string url = "/me/playlists";

            //var response = await SendAPIRequest<List<Playlist>>(user, "GET", url);
            var response = await SendAPIRequest<Playlists>(session, "GET", url);
            return response;
        }

        public async Task<(bool, Playlists, string)> GetPublicPlaylistExample()
        {
            Playlists res = new Playlists();
            List<string> playlistIds = new List<string>() {"3bZIPyJ1rBQGgMCaulEJvd", "2oCcToQeq6nj73A2FNPMMg", "0sYmFPg4QU9dEiq2mjFp92", "13nMupvZ2oJPhI4wetleMJ"};
            
            var lookups = new List<Task<(bool, Playlist, string)>>();
           foreach (string playlistId in playlistIds)
           {
                lookups.Add(GetPlaylist(_clientPublicSession, playlistId));
           }

           (bool, Playlist, string) [] lookupResult = await Task.WhenAll(lookups);
            
           List<Playlist> playlists = new List<Playlist>();
           foreach ((bool success, Playlist playlist, string raw_res) in lookupResult)
           {
               if (success)
                   playlists.Add(playlist);
           }
           res.items = playlists.ToArray();

            return new (true, res, "success!");
        }
    }
}
