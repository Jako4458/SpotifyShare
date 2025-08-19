using SpotifyController.Interfaces;
using SpotifyController.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using Dapper;

namespace SpotifyController.Data
{
    public class SqlRepo: IDataRepository
    {

        private static string connectionString = $" Server={Environment.GetEnvironmentVariable("DB_HOST")}; Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")}; User Id={Environment.GetEnvironmentVariable("DB_USER")}; Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
        
        static public async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            return connection;
        }

        ////////////////////////////////////////////////////////////////////////////////////


        public async Task<(bool, User)> TryGetUser(string userID, bool UserOnly=false)
        {

            using var connection = await GetOpenConnectionAsync();

            User user = await connection.QuerySingleOrDefaultAsync<User>("Select UserID as ID, UserID as Name from User where UserID = @userID", new {userID = userID});

            if (user == default(User))
                return (false, null);
            if (UserOnly)
                return (true, user);

            SpotifySession session = await connection.QuerySingleOrDefaultAsync<SpotifySession>("Select s.SessionID as Id, s.SessionName as Name, s.SharingStatus, s.Password, s.ExpirationTime as EndTime from Session s join User u on u.SessionID = s.SessionID where u.UserID = @UserID", new {userID = userID});

            if (session == default(SpotifySession))
            {
                Console.WriteLine("SpotifySession is default in TryGetUser");
                return (false, null);
            }

            SpotifyAPIToken spotifyToken = await connection.QuerySingleOrDefaultAsync<SpotifyAPIToken>("Select st.*, st.SpotifyTokenID as Id, st.ExpirationTime as AccessTokenExpiration from SpotifyToken st join User u on u.SpotifyTokenID = st.SpotifyTokenID where u.UserID = @UserID", new {userID = userID});

            if (spotifyToken == default(SpotifyAPIToken))
            {
                Console.WriteLine("SpotifyAPIToken is default in TryGetUser");
                return (false, null);
            }

            session.SpotifyToken = spotifyToken;
            user.SpotifySession = session;
            return (true, user);
        }

        public async Task AddUser(string userID)
        {
            using var connection = await GetOpenConnectionAsync();
            await connection.ExecuteAsync("insert into User (UserID) values (@userID)",  new {userID = userID});

        }

        public async Task AddUser(User user)
        {
            await AddUser(user.ID);
        }

        public async Task AddSpotifyConnection(string userID, SpotifySession session, SpotifyAPIToken spotifyToken)
        {
            using var connection = await GetOpenConnectionAsync();

            string Query = "insert into SpotifyToken (AccessToken, RefreshToken, Code, Status, ExpirationTime) values (@AccessToken, @RefreshToken, @Code, @Status, @AccessTokenExpiration); select last_insert_id();";
            Query = "insert into SpotifyToken (AccessToken, RefreshToken, Code, Status) values (@AccessToken, @RefreshToken, @Code, @Status); select last_insert_id();";

            // Add SpotifyToken 
            Query = "insert into SpotifyToken (AccessToken, RefreshToken, Code, Status, ExpirationTime) values (@AccessToken, @RefreshToken, @Code, @Status, @AccessTokenExpiration); select last_insert_id();";
            int spotifyTokenID = await connection.QuerySingleOrDefaultAsync<int>(Query, spotifyToken); 

            // Add session 
            int sessionID = await connection.QuerySingleOrDefaultAsync<int>("insert into Session (SessionName, SharingStatus, Password) values (@sessionName, @sharingStatus, @password); select Last_insert_id();", new {sessionName = userID, sharingStatus = "Private", password=session.Password});


            // Connect to User
            await connection.ExecuteAsync("update User set SessionID = @sessionID, SpotifyTokenID = @spotifyTokenID where UserID = @userID",
                                            new {userID = userID, SessionID = sessionID, spotifyTokenID = spotifyTokenID});
        }

        public async Task UpdateSpotifyToken(SpotifyAPIToken spotifyToken)
        {
            using var connection = await GetOpenConnectionAsync();

            await connection.ExecuteAsync("update SpotifyToken set AccessToken = @AccessToken, RefreshToken = @RefreshToken, ExpirationTime = @AccessTokenExpiration where SpotifyTokenID = @Id", spotifyToken);
        }

        public async Task ToggleShareSpotifySession(string userID)
        {
            using var connection = await GetOpenConnectionAsync();

            SpotifySession session = await connection.QuerySingleOrDefaultAsync<SpotifySession>("Select s.SessionID as Id, s.SessionName as Name, s.SharingStatus, s.Password, s.ExpirationTime as EndTime from Session s join User u on u.SessionID = s.SessionID where u.UserID = @UserID", new {userID = userID});

            string query;
            if (session.SharingStatus != "Private")
                query = "update Session set SharingStatus = 'Private' where SessionID = @sessionId;";
            else
                query = "update Session set SharingStatus = 'Public', ExpirationTime = DATE_ADD(NOW(), INTERVAL 24 HOUR) where SessionID = @sessionId;";

            await connection.ExecuteAsync(query, new {sessionId = session.Id});
        }

        public async Task<List<SpotifySession>> GetPublicSessionList()
        {
            using var connection = await GetOpenConnectionAsync();

            return connection.Query<SpotifySession>("SELECT *, SessionID as ID, SessionName as Name from Session where SharingStatus != 'Private';").ToList();
        }

        public async Task<(bool, SpotifySession)> GetPublicSessionFromID(string sessionID)
        {
            using var connection = await GetOpenConnectionAsync();

            SpotifySession publicSession =  await connection.QuerySingleOrDefaultAsync<SpotifySession>("Select SessionID as Id, SessionName as Name, SharingStatus, Password, ExpirationTime as EndTime from Session where SessionID = @sessionID", new {sessionID = sessionID});

            if (publicSession is default(SpotifySession))
                return (false, null);

            SpotifyAPIToken spotifyToken = await connection.QuerySingleOrDefaultAsync<SpotifyAPIToken>("Select st.*, st.SpotifyTokenID as Id, st.ExpirationTime as AccessTokenExpiration from SpotifyToken st join (User u join Session s on u.SessionID = s.SessionId) on u.SpotifyTokenID = st.SpotifyTokenID where s.SessionID = @sessionID", new {sessionID = sessionID});

            if (spotifyToken == default(SpotifyAPIToken))
                return (false, null);

            publicSession.SpotifyToken = spotifyToken;
            return (true, publicSession);
        }
    }
}

