using SpotifyController.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyController.Data
{
    static public class SpotifySessionRepo
    {
        static private List<SpotifySession> _publicSessions = new List<SpotifySession>();
        static public List<SpotifySession> PublicSessions => _publicSessions.Where(s => s.IsPublic).ToList();

        static public (bool, SpotifySession) getPublicSpotifySession(string id)
        {
            _publicSessions = _publicSessions.Where(s => s.IsPublic).ToList();

            if (!_publicSessions.Any(s => s.Id == id))
                return (false, null);

            return (true, _publicSessions.First(s => s.Id == id));
        }

        static public void addPublicSpotifySession(SpotifySession session)
        {
            if (!_publicSessions.Any(s => s.Id == session.Id))
                _publicSessions.Add(session);
        }

    }
}
