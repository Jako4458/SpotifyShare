import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import Cookies from 'universal-cookie';
import { v4 as uuid } from 'uuid';
import App from './App';
import registerServiceWorker from './registerServiceWorker';

const baseUrl = "/spotify/";
const rootElement = document.getElementById('root');

const cookies = new Cookies();


// let tempSessionId = cookies.get("SessionId");
//
//if (!tempSessionId) {
//    tempSessionId = uuid().slice(0, 8);
//    cookies.set("SessionId", tempSessionId);
//}
//
//const sessionId = React.createContext(tempSessionId);
//console.log(sessionId);

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement);

registerServiceWorker();

