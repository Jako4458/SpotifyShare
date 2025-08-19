using System;
using Newtonsoft.Json;

namespace SpotifyController.Model
{
    public class SpotifyAPIToken
    {
        public SpotifyAPIToken() {}
        public SpotifyAPIToken(string code, string status)
        {
            Code = code;
            Status = status;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Status { get; set; }

        public DateTime AccessTokenExpiration { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
