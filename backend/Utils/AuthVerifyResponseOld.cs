using Newtonsoft.Json;
using System;

namespace SpotifyController.Utils
{
    public class AuthVerifyResponseOld
    {
        [JsonProperty("status code")]
        public int StatusCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public AuthUserDataOld AuthUserData { get; set; }

        public bool Ok => StatusCode >= 200 && StatusCode < 400;
    }
}
