namespace SpotifyController.Model.SpotifyAPI
{
    public class Search
    {
        public Tracks tracks { get; set; }
        public Artists artists { get; set; }
        public Albums albums { get; set; }
        public Playlists playlists { get; set; }
        public Shows shows { get; set; }
        public Episodes episodes { get; set; }
    }
}
