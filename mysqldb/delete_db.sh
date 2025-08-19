#!/bin/bash

read -p "Are you sure you want to delete database 'Spotify_service'? (y/N): " answer
case "$answer" in
    [yY][eE][sS]|[yY])
        ;;
    *)
        echo "Aborted."
        exit 1
        ;;
esac

echo "Dropping database 'Spotify_service'..."
mysql $@ -e "DROP DATABASE Spotify_service;"

echo "Deleting user 'SpotifyServiceAdmin'..."
mysql $@ -e "DROP USER IF EXISTS 'SpotifyServiceAdmin'@'172.18.0.%';"
mysql $@ -e "DROP ROLE IF EXISTS 'SpotifyAdmin'@'%';"

