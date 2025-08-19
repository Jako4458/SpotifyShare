import * as React from 'react';
// import Cookies from 'universal-cookie';

import '../Template.css';
import './Style.css';


export default class Track extends React.Component {

    //constructor(props) {
    //    super(props);
    //    //this.state = {loading: true };
    //}

    render() {
        return (
            <React.Fragment>
                <div className="track" id={this.props.id}>
                    <img src={this.props.album.images[2].url} alt="album cover" />
                    <p id="name">{this.props.name}</p>
                    {this.RenderIfWidthIsOver(800, <p id="album">{this.props.album.name}</p>)}
                    {this.RenderIfWidthIsOver(800, <p id="artist">{this.props.album.artists.map(element => element.name)}</p>)}
                    {this.RenderIfWidthIsOver(800, <p id="duration">{Math.floor(this.props.duration_ms / 60000)}:{(Math.round(this.props.duration_ms / 1000) % 60) > 10 ? (Math.round(this.props.duration_ms / 1000) % 60) : "0" + (Math.round(this.props.duration_ms / 1000) % 60)}</p>)}
                    <button onClick={async () => this.QueueTrack(this.props.id)}>Queue</button>
                </div>
            </React.Fragment>
        );
    }

    async QueueTrack(trackId) {
        const response = await fetch(`API/SpotifyAPI/QueueTrack?trackId=${this.props.id}`,{
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'SpotifySessionId': this.props.SpotifySessionId,
            },
            body: JSON.stringify({"trackId": trackId}),
        });

        if (response.status == 200) {
            document.getElementById(`${this.props.id}`).querySelector("Button").innerText = "Queued!"
                document.getElementById(`${this.props.id}`).querySelector("Button").style.background = "green";
            setTimeout(() => {
                document.getElementById(`${this.props.id}`).querySelector("Button").innerText = "Queue";
                document.getElementById(`${this.props.id}`).querySelector("Button").style.background = "white";
            }, 1000);
        } else if (response.status == 401) {
            window.location.replace("/API/SpotifyAPI/Authorize");
        } else {
            console.error(`Error ${response.status} - Queue failed!`)
            document.getElementById(`${this.props.id}`).querySelector("Button").innerText = "Failed!"
            document.getElementById(`${this.props.id}`).querySelector("Button").style.background = "#C11B17";
            setTimeout(() => {
                document.getElementById(`${this.props.id}`).querySelector("Button").innerText = "Queue";
                document.getElementById(`${this.props.id}`).querySelector("Button").style.background = "white";
            }, 1000);
        }
    }

    RenderIfWidthIsOver(minWidth, component) {
        return (window.innerWidth > minWidth && component)
    }
}
