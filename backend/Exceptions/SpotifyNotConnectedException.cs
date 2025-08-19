using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyController.Exceptions
{
    public class SpotifyNotConnectedException : Exception
    {
        public SpotifyNotConnectedException() :base() {}
        public SpotifyNotConnectedException(string errorMessage = "User not connected to spotify!") : base(errorMessage) {}
    }
}
