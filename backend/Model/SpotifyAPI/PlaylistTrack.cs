using System;

namespace SpotifyController.Model.SpotifyAPI
{
    public class PlaylistTrack
    {
        public DateTime added_at { get; set; }
        public Added_By added_by { get; set; }
        public bool is_local { get; set; }
        public object primary_color { get; set; }
        public Track track { get; set; }
        public Video_Thumbnail video_thumbnail { get; set; }
    }
}
