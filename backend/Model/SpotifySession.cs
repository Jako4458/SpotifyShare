using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotifyController.Model
{
    public class SpotifySession
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SpotifyAPIToken SpotifyToken { get; set; }

        public DateTime EndTime { get; set; }

        public string SharingStatus { get; set; }
        public bool IsPublic => SharingStatus != "Private"; //(DateTime.Now < EndTime);

        private string _password;
        public string Password { get; set; }

	    //Host = Environment.GetEnvironmentVariable("HOST_IP");
	    //if (Host==null)
	    //    throw new Exception("Invalid Host IP - Update in Dockerfile");

        private string _baseUrl = $"http://{Environment.GetEnvironmentVariable("HOST_IP")}/spotify";
        public string Url => $"{_baseUrl}/session/{Id}/search";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        
    }
}
