import * as React from 'react';
// import Cookies from 'universal-cookie';

import '../Template.css';
import './Style.css';

import Track from './Track';

export default class Search extends React.Component {

    constructor(props) {
        super(props);
        let query = this.props.match.params.query;
        query = query ? query : "";

        let SpotifySessionId = this.props.match.params.sessionId;
        this.SpotifySessionId = SpotifySessionId ? SpotifySessionId : "undefined"; // MUST BE "" AND NOT null as backend cant handle null


        this.state = {
            value: query, search_result: null, loading: true };

        this.updateInput = this.updateInput.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);

        if (query)
            this.search(query);
    }

    render() {
        let content = this.state.value == "" ? "" :
            this.state.search_result == null
            ? <p>Loading ...</p>
            : this.state.search_result.tracks.items.map(element => {
                let track = element;
                return <Track id={track.id} name={track.name} album={track.album} duration_ms={track.duration_ms} SpotifySessionId={this.SpotifySessionId} />
            });

        return (
            <React.Fragment>
                <div className="search">
                    <form onSubmit={this.handleSubmit}>
                        <label>
                            <input type="text" value={this.state.value} onChange={this.updateInput} />
                        </label>
                        <input type="submit" value="Search"/>
                    </form>
                </div>
                <div className="className">
                    {content}
                </div>
            </React.Fragment>
        );
    }

    async updateInput(event) {
        const value = event.target.value;
        //alert(event.target.value);
        this.setState({ value: value });
        this.search(value);
    }

    async handleSubmit(event) {
        event.preventDefault();
        //this.setState({ loading: true })
        this.search(this.state.value);
        //alert(this.state.value)
    }

    async search(query) {
        query = query.trim();   
        if (!query) return;

        const response = await fetch(`API/SpotifyAPI/Search?query=${query}`,
            {
                headers: {
                'SpotifySessionId': this.SpotifySessionId,
                }
            })
        if (response.ok) {
            try {
                const data = await response.json();
                this.setState({ search_result: data, loading: false });
            } catch (e) {
                this.setState({ search_result: { name: "Error: Response is not JSON!" }, loading: false });
            }
        } else if (response.status == 401) {
            window.location.href = `API/SpotifyAPI/authorize?redirect_uri=/spotify/search/${this.state.value}`
        } else {
            this.setState({ search_result: {tracks:{ name: `Error: ${response.status}: ${response.body}` }}, loading: false });
        }
    }
}
