import * as React from 'react';
import { Route } from 'react-router';

import './custom.css'

import Layout from './components/Layout';
import { Home } from './components/Home';
import Content from './components/Content';
import Playlist from './components/Spotify/Playlist';
import PlaylistLink from './components/Spotify/PlaylistLink';
import PlaylistOverview from './components/Spotify/PlaylistOverview';
import Search from './components/Spotify/Search';
import SessionOverview from './components/Spotify/SessionOverview';

export default () => (
    <Layout Title="Spotify">

        <Content>
            <Route exact path='/' component={Home} />
            <Route exact path='/playlists' component={PlaylistOverview} />
            <Route exact path='/search/:query?' component={Search} />
            <Route path='/playlist/:id' component={Playlist} />
            <Route path='/playlistlink/:id' component={PlaylistLink} />
            <Route path='/sessions' component={SessionOverview} />
            <Route exact path='/session/:sessionId/search/:query?' component={Search} />
        </Content>

    </Layout>

);

function RenderIfWidthIsOver(minWidth, component) {
    return (window.innerWidth > minWidth && component)
}
