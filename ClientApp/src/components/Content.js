import * as React from 'react';
import { Container } from 'reactstrap';

import './Template.css';


export default (props) => (
    <React.Fragment>
        <div className="content">
            <Container>
                {props.children}
            </Container>
        </div>
    </React.Fragment>
);

