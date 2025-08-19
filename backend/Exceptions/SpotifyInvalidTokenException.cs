using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyController.Exceptions
{
    public class SpotifyInvalidTokenException : Exception
    {
        public SpotifyInvalidTokenException() :base() {}
        public SpotifyInvalidTokenException(string errorMessage = "User spotify Token Not valid!") : base(errorMessage) {}
    }
}
