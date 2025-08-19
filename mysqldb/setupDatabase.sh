#!/bin/bash

mysql $@ < 00-setup.sql

mysql $@ -D Spotify_service < 01-createSpotifyTokenTable.sql
mysql $@ -D Spotify_service < 02-createSessionTable.sql
mysql $@ -D Spotify_service < 03-createUserTable.sql
mysql $@ -D Spotify_service < 04-addRoles.sql
mysql $@ -D Spotify_service < 05-addUsers.sql
