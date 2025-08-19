import * as React from 'react';
// import Cookies from 'universal-cookie';

import '../Template.css';
import Session from './Session';
import './Style.css';

export default class SessionOverview extends React.Component {

    constructor(props) {
        super(props);
        this.update_interval_id = 0;
        this.state = { sessions: {}, loading: true };

    }

    componentDidMount() {
        this.populateSessionOverview();
        // delay to make newly share show up 
        setTimeout(() => {
            this.populateSessionOverview();
        }, 500);
        this.update_interval_id = setInterval(() => {
           this.populateSessionOverview();
        }, 5000);
    }

    componentWillUnmount() {
        console.log("remove update interval");
        window.clearInterval(this.update_interval_id);
    }

    render() {
        let content = this.state.sessions == null
            ? <p>Loading ...</p>
            : Array.isArray(this.state.sessions)
                ? this.state.sessions.map(session => {
                    return <Session id={session.id} name={session.name} url={session.url} />
                })
                : <p>No sessions found!</p>;

        return (
            <React.Fragment>
                <div className="playlists">
                    {content}
                </div>

            </React.Fragment>
        );
    }


    async populateSessionOverview() {
        const response = await fetch(`API/SpotifySession/GetPublicSessions`);
        if (response.ok) {
            try {
                const data = await response.json();
                this.setState({ sessions: data, loading: false });
            } catch (e) {
                this.setState({ sessions: { name: "Error: Response is not JSON!" }, loading: false });
            }
        } else {
            this.setState({ sessions: { name: `Error: ${response.status}: ${response.body}` }, loading: false });
        }
    }
}
