import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
	<div>
	    <h1>Spotify Share</h1>
	    <p>This site allows sharing of listening sessions on Spotify. It utilizes the Spotify API and some features are locked behind a login requiring Spotify Premium. Songs can only be successfully queued if the user is actively listening on Spotify.</p>
	    <p>Login is possible at <a href="https://login.jbhyldgaard.dk">https://login.jbhyldgaard.dk</a>.</p>
	    <p>As this project is still marked as under development, the Spotify API requires the email connected to specific Spotify accounts to grant this site access permissions. Without these permissions certain features like queue a share session will not work. To request access, please send an email to <a href="mailto:post@jbhyldgaard.dk">post@jbhyldgaard.dk</a>.</p>
	</div>

    );
  }
}
