import * as React from 'react';
import { Link, useHistory } from 'react-router-dom';
import Cookies from 'universal-cookie';
import './Template.css';
import uuid from "uuid";

export default class NavMenu extends React.PureComponent {
    //state = {
    //    isOpen: false,
    //};

    constructor(props) {
        super(props);
        this.state = { user: null, loading: true };
    }

    componentDidMount() {
        this.getUserDataFromSessionId()
            .then(() => {
                if (this.state.user == null) {
	   	    // If user is not logged in 
		    // But has cookie -> redirect to login  
		    let cookies = new Cookies();
		    if (cookies.get("device_key") != undefined) {
			window.location.replace("API/SpotifyAPI/authorize");
		    }
		} else { 
		    // if user is logged in -> show sharelink
                    let sharelink = document.querySelector("#shareLink");

                    sharelink.addEventListener("click", () => {
                        sharelink.style.display = "none";
                        fetch(`API/User/shareSession`)
                    })
                }
            })
    }


    render() {
        const refreshSessionsOnShare = {
            pathname: "/Sessions",
            // key: uuid(), // we could use Math.random, but that's not guaranteed unique.
            state: {
                applied: true
            }
        };

        let ContentLoggedIn = 
            <ul>
                {/*<Link to>Logout</Link>*/}
                <Link to="/Playlists"><li>Playlists</li></Link>
                <Link to="/Search"><li>Search</li></Link>
                <Link to="/Sessions"><li>Sessions</li></Link>
                <Link to={refreshSessionsOnShare} id="shareLink"><li>Share</li></Link>
            </ul>;

       let ContentLoggedOut = 
             <ul>
               <Link to="/Playlists"><li>Playlists</li></Link>
               <Link to="/Search"><li>Search</li></Link>
               <Link to="/Sessions"><li>Sessions</li></Link>
            </ul>;

        let content = this.state.user == null ? ContentLoggedOut : ContentLoggedIn;
        return (
            <header>
                <header className="edge">
                    <a href="/"><h1>{this.props.Title}</h1></a>
                    <nav>
                        { content }
                    </nav>
                </header>
            </header>
        );
    }

    async getUserDataFromSessionId() {
        const response = await fetch(`API/User/GetUser`);
        if (response.ok) {
            try {
                const data = await response.json();
                this.setState({ user: data, loading: false });
            } catch (e) {
                this.setState({ user: "Error: Something went wront (probably user parsing)!", loading: false });
            }
        } else if (response.status == 404) {
            this.setState({ playlist: { name: `Error: ${response.status}: ${response.body}` }, loading: false });
        }
    }

}
