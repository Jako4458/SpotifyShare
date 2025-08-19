using System;
using Newtonsoft.Json;

namespace SpotifyController.Model
{
    public class User
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public SpotifySession SpotifySession { get; set; }
        public SpotifyAPIToken SpotifyToken => SpotifySession?.SpotifyToken;
        public bool IsConnectedToSpotify => SpotifyToken != null;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
