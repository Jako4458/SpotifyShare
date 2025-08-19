import * as React from 'react';
import { Link } from 'react-router-dom';

import '../Template.css';
import './Style.css';


export default class PlaylistLink extends React.Component {

    //constructor(props) {
    //    super(props);
    //    //this.state = {loading: true };
    //}

    render() {
        return (
            <React.Fragment>
                <Link to={"/playlist/" + this.props.id}>
                    <div className="playlistLink" id={this.props.id}>
                        <img src={this.props.imageUrl} alt="platlist cover" />
                        <p id="name">{this.props.name}</p>
                    </div>
                </Link>
            </React.Fragment>
        );
    }
}
