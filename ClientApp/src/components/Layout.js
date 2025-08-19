import * as React from 'react';

import NavMenu from './NavMenu';

export default (props) => (
    <React.Fragment>
        <NavMenu Title={props.Title}/>
        {props.children}
    </React.Fragment>
);

