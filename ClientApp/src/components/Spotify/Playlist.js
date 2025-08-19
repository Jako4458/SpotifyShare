import * as React from 'react';
// import Cookies from 'universal-cookie';

import '../Template.css';
import './Style.css';

import Track from './Track';


export default class Playlist extends React.Component {

    constructor(props) {
        super(props);
        this.state = { playlist: {}, loading: true, loadOnScroll: true, scrollBot: false };
        this.loadOnScroll = true;
        this.offset = 0;
        this.limit = 50;

    }

    componentDidMount() {
        this.populatePlaylistData();

        let loadAtScrollPercentage = 0.8;
        //let loadAtScrollPercentage = 1;
        let content = document.querySelector(".content")


        content.addEventListener('scroll', () => {
            this.state = {
                playlist: this.state.playlist,
                loading: this.state.loading,
                loadOnScroll: this.state.loadOnScroll,
                scrollBot: content.offsetHeight + content.scrollTop >= content.scrollHeight * loadAtScrollPercentage
            };
            if (this.state.scrollBot && this.state.loadOnScroll && !this.state.loading) {
                this.setState({loading: true})
                this.populatePlaylistTrackData();
            }
        })
    }

    render() {
        let content = this.state.playlist.tracks == null
            ? <p>Loading ...</p>
            : this.state.playlist.tracks.items.map(element => {
                let track = element.track;
                return <Track id={track.id} name={track.name} album={track.album} duration_ms={track.duration_ms} SpotifySessionId={""} />
            });

        return (
            <React.Fragment>
                <h2>{this.state.playlist.name}</h2>
                <div className="className">
                    {content}
                </div>

            </React.Fragment>
        );
    }

    RenderIfWidthIsOver(minWidth, component) {
        return (window.innerWidth > minWidth && component)
    }

    async populatePlaylistData() {
        const response = await fetch(`API/SpotifyAPI/GetPlaylist?playlistId=${this.props.match.params.id}`);
        if (response.ok) {
            try {
                const data = await response.json();
                this.setState({ playlist: data, loading: false });
                this.offset = data.tracks.limit;
            } catch (e) {
                this.setState({ playlist: { name: "Error: Response is not JSON!" }, loading: false });
            }
        } else if (response.status == 401) {
            window.location.href = `API/SpotifyAPI/authorize?redirect_uri=/playlist/${this.props.match.params.id}`
        } else {
            this.setState({ playlist: { name: `Error: ${response.status}: ${response.body}` }, loading: false });
        }
    }

    async populatePlaylistTrackData() {
        const response = await fetch(`API/SpotifyAPI/GetPlaylistTracks?playlistId=${this.props.match.params.id}&offset=${this.offset}&limit=${this.limit}`,);
        if (response.ok) {
            try {
                const data = await response.json();
                let playlist = this.state.playlist;
                playlist.tracks.items = playlist.tracks.items.concat(data.items)
                this.setState({ playlist: playlist , loading: false });
                let new_offset = data.offset + data.limit;
                this.setState({ loadOnScroll: this.offset != new_offset })
                this.offset = new_offset;
            } catch (e) {
                this.setState({ playlist: { name: "Error: Response is not JSON!" }, loading: false, loadOnScroll: false });
            }
        } else if (response.status == 401) {
            window.location.href = `API/SpotifyAPI/authorize?redirect_uri=/playlist/${this.props.match.params.id}`
        } else {
            this.setState({ playlist: { name: `Error: ${response.status}: ${response.body}` }, loading: false });
        }
        //this.setState({loading: false})

    }
}
