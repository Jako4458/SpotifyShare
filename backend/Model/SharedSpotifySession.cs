using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyController.Model
{
    public class SharedSpotifySession
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool HasPassword;
        public string Url { get; set; }

    }
}
