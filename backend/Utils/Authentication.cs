using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using FlaskAuthSDK;

namespace SpotifyController.Utils
{
    public class Authentication : FlaskAuthClient
    {
        public Authentication() : base("http://authentication:5000", "spotify", Environment.GetEnvironmentVariable("SpotifyServiceLocalAuthToken")) {}
    }

}
