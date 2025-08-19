import * as React from 'react';
import Cookies from 'universal-cookie';

import '../Template.css';
import './Style.css';

import Track from './Track';


export default class PlayerControl extends React.Component {

    constructor(props) {
        super(props);
        //this.state = { playlist: {}, loading: true, loadOnScroll: true, scrollBot: false };
    }

    //componentDidMount() {
    //}

    render() {
        let content = <h1> HEY </h1>;

        return (
            <React.Fragment>
                <div className="className">
                    {content}
                </div>

            </React.Fragment>
        );
    }

    RenderIfWidthIsOver(minWidth, component) {
        return (window.innerWidth > minWidth && component)
    }


}