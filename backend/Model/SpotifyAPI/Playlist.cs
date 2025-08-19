namespace SpotifyController.Model.SpotifyAPI
{
    public class Playlist
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public External_Urls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public Image[] images { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public object primary_color { get; set; }
        public bool _public { get; set; }
        public string snapshot_id { get; set; }
        public PlaylistTracks tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
}
