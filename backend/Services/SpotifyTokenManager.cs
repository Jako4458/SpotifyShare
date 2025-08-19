using SpotifyController.Exceptions;
using SpotifyController.Interfaces;
using SpotifyController.Data;
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
    public class SpotifyTokenManager: ISpotifyTokenManager
    {
        private static string Host = null;
        private IDataRepository _dataRepository;
        

        public SpotifyTokenManager(HttpClient httpClient, IConfiguration config, IDataRepository dataRepository)
        {
            this._dataRepository = dataRepository;
            _clientId = Environment.GetEnvironmentVariable("SPOTIFY_API_CLIENT_ID"); 
            _clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_API_CLIENT_SECRET"); 
            
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

        public async Task<(bool, SpotifyAPIToken)> GetToken(SpotifySession session, string tokenType = "private")
        {
            if (session is null)
                throw new SpotifyNotConnectedException();


            var dict = new Dictionary<string, string>();
            dict.Add("client_id", _clientId);
            dict.Add("client_secret", _clientSecret);
            dict.Add("redirect_uri", RedirectUri);

            Console.WriteLine($"TokenType: {tokenType}");


            switch (tokenType)
            {
                case "client_credentials":
                    dict.Add("grant_type", "client_credentials");
                    break;
                case "private":
                    if (session.SpotifyToken.RefreshToken != null) 
                    { 
                        dict.Add("grant_type", "refresh_token");
                        dict.Add("refresh_token", session.SpotifyToken.RefreshToken);
                    } else
                    {
                        dict.Add("grant_type", "authorization_code");
                        dict.Add("code", session.SpotifyToken.Code);
                    }
                    break;
                default:
                    throw new Exception($"Unknown tokenType {tokenType!}");
            }

            string url = "https://accounts.spotify.com/api/token";
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(dict) };
            if (tokenType == "client_credentials")
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}");
                var auth = System.Convert.ToBase64String(plainTextBytes);
                req.Headers.Add("Authorization", $"Basic {auth}");
            }

            var res = await _httpClient.SendAsync(req);
            string resContent = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine(resContent);
                return (false, default);
            }

            string resString = await res.Content.ReadAsStringAsync();
            var TokenObject = JsonConvert.DeserializeObject<TokenResponse>(resString);

            Console.WriteLine($"TokenObject: {TokenObject}");

            session.SpotifyToken.AccessToken = TokenObject.access_token;
            session.SpotifyToken.AccessTokenExpiration = DateTime.Now.AddSeconds(TokenObject.expires_in);

            if (session.SpotifyToken.RefreshToken == null)
                session.SpotifyToken.RefreshToken = TokenObject.refresh_token;
            
            if (tokenType == "private")
                _dataRepository.UpdateSpotifyToken(session.SpotifyToken);

            return (true, session.SpotifyToken);
        }

        
    }
}
