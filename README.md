# SpotifySession

Share your Spotify listening sessions with friends. This app provides a web interface for creating and joining Spotify sessions, powered by the Spotify API.

## Features
- Create and join Spotify listening sessions
- Spotify Premium login (required for queueing tracks)
- Backend in .NET
- Frontend in React (Create React App)
- Reverse proxy with NGINX
- Docker Compose setup


## Getting Started
### Project Structure
```text
SpotifySession/
├── backend/            # .NET backend
├── ClientApp/          # React frontend
└── reverse_proxy/      # NGINX config
```

## Setup
### 1) Clone the project

### 2) Configure environment
Copy the example file and edit values:
```bash
cp .env.example .env
```
### 3) Setup MySQL Database
```bash
cd mysqldb
./setup.sh
```

### 4) Run with Docker Compose
```bash
docker compose up --build
```

## Authentication
This project relies on a authentication service running on the same external docker network refered to as "home-server". The authentication server implementation and client integration SDK used for this project "FlaskAuthSDK" can be found on [https://github.com/Jako4458/passwordless-auth-service](https://github.com/Jako4458/passwordless-auth-service)

## License
MIT — feel free to use and adapt.