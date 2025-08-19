import React from 'react';
import ReactDOM from 'react-dom';
import { MemoryRouter } from 'react-router-dom';
import Cookies from 'universal-cookie';
import { v4 as uuid } from 'uuid';

import App from './App';

it('renders without crashing', async () => {
  const div = document.createElement('div');

    let sessionId = Cookies.get("SessionId");

    if (!sessionId) {
        sessionId = uuid().slice(0, 8);
        Cookies.set("SessionId", sessionId);
    }

    console.log(sessionId);

  ReactDOM.render(
    <MemoryRouter>
      <App />
    </MemoryRouter>, div);
  await new Promise(resolve => setTimeout(resolve, 1000));
});
