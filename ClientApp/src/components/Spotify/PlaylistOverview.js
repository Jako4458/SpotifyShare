import * as React from 'react';
// import Cookies from 'universal-cookie';

import '../Template.css';
import PlaylistLink from './PlaylistLink';
import './Style.css';

export default class PlaylistOverview extends React.Component {

    constructor(props) {
        super(props);
        this.state = { playlists: {}, loading: true };
    }

    componentDidMount() {
        this.populatePlaylistOverview();
    }

    render() {
        let content = this.state.playlists.items == null
            ? <p>Loading ...</p>
            : this.state.playlists.items.map(element => {
                let playlist = element;
                let playlistImgUrl = playlist.images[playlist.images.length - 1].url;
                let id = element.uri.substring(playlist.uri.indexOf("spotify:playlist:") + 17)
                return <PlaylistLink id={id} name={playlist.name} imageUrl={playlistImgUrl}/>
            });

        return (
            <React.Fragment>
                <div className="playlists">
                    {content}
                </div>

            </React.Fragment>
        );
    }

    RenderIfWidthIsOver(minWidth, component) {
        return (window.innerWidth > minWidth && component)
    }

    async populatePlaylistOverview() {
        const response = await fetch(`API/SpotifyAPI/GetPlaylists`,);
        if (response.ok) {
            try {
                const data = await response.json();
                this.setState({ playlists: data, loading: false });
            } catch (e) {
                this.setState({ playlists: { name: "Error: Response is not JSON!" }, loading: false });
            }
        } else if (response.status == 401) {
            window.location.href = `API/SpotifyAPI/authorize?redirect_uri=/spotify/playlists`
        } else {
            this.setState({ playlists: { name: `Error: ${response.status}: ${response.body}` }, loading: false });
        }
    }
}
