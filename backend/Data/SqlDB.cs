using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SpotifyController.Model;
using SpotifyController.Utils;
using Dapper;
using MySql.Data.MySqlClient;

namespace SpotifyController.Data 
{
    static public class SqlDB
    {
	private static string connectionString = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};User Id={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
	//await using public static var connection = SqlConnection(connectionString);
	
	static public async Task<MySqlConnection> GetOpenConnectionAsync()
        {
	    var connection = new MySqlConnection(connectionString);
	    await connection.OpenAsync();
	    return connection;
	}

    static public async Task<Tuple<bool,User>> TryGetUser(HttpRequest Request)
    {

	    string device_key = default;
	    string hmac_secret = default;

	    var connection = await SqlDB.GetOpenConnectionAsync();

	    string UserIDQuery = "Select u.UserID from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    int UserID = await connection.QuerySingleOrDefaultAsync<int>(UserIDQuery, new {device_key = device_key}); 

	    if (UserID == default)
            return new Tuple<bool, User>(false, default);

	    string UsernameQuery = "Select u.Username from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    string Username = await connection.QuerySingleOrDefaultAsync<string>(UsernameQuery, new {device_key = device_key}); 

	    string SpotifySessionQuery = "Select ss.SessionUUID as Id, ss.ExpirationTime as EndTime, ss.IsPublic as IsPublic, ss.Password as _password from SpotifyServiceData sd join SpotifySession ss on sd.SpotifySessionID = ss.SpotifySessionID where sd.UserID = @UserID";
	    SpotifySession session = await connection.QuerySingleOrDefaultAsync<SpotifySession>(SpotifySessionQuery, new {UserID = UserID});

	    string SpotifyTokenQuery = "Select st.Code as Code, st.AccessToken as AccessToken, st.RefreshToken as RefreshToken, st.Status as Status, st.ExpirationTime as AccessTokenExpiration from SpotifyServiceData sd join SpotifyToken st on sd.SpotifyTokenID = st.SpotifyTokenID where sd.UserID = @UserID";
	    SpotifyAPIToken token = connection.QuerySingleOrDefault<SpotifyAPIToken>(SpotifyTokenQuery, new {UserID = UserID});

	    if (session == default)
            return new Tuple<bool, User>(true, new User() {Name = Username, SpotifySession = null});

	    session.Name = $"{Username}´s Session";
	    session.SpotifyToken = token;

	    string UpdateLastLoginQuery = "update Device set LastLogin = @loginTime where DeviceKey = @deviceKey";
	    connection.Execute(UpdateLastLoginQuery, new {deviceKey = device_key, loginTime = DateTime.Now});

	    return new Tuple<bool, User> (true, new User() {Name = Username, SpotifySession = session});
	    //return new User() {Name = Username, SpotifySession = session, SpotifyToken = token};

	}

	static public async void AddUser(HttpRequest Request, User User)
	{
	    string device_key = default;
	    string hmac_secret = default;
          //  bool succes_device_key = Request.Cookies.TryGetValue("device_key", out device_key);
         //   bool succes_hmac_secret = Request.Cookies.TryGetValue("hmac_secret", out hmac_secret);

	    //if (!(succes_device_key && succes_hmac_secret)){
		//    Console.WriteLine("device_key and/or hmac_secret NOT found!");
		 //   return;
	    //}

	    // (bool authSucces, int statusCode, string authResponse) = await Authentication.VerifyClient(Request);
        //    if (!authSucces)
	    //    return;

	    var connection = await SqlDB.GetOpenConnectionAsync();

	    string UserIDQuery = "Select u.UserID from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    int UserID = await connection.QuerySingleOrDefaultAsync<int>(UserIDQuery, new {device_key = device_key}); 
	    
	    //Console.WriteLine($"Code: {User.SpotifyToken.Code}");
	    //Console.WriteLine($"AccessToken: {User.SpotifyToken.AccessToken}");
	    //Console.WriteLine($"RefreshToken: {User.SpotifyToken.RefreshToken}");
	    
	    string AddTokenQuery = "insert into SpotifyToken (Code, Status, AccessToken, RefreshToken, ExpirationTime) values (@Code, @Status, @AccessToken, @RefreshToken, @AccessTokenExpiration);Select Last_insert_id();";
	    int TokenID = await connection.QuerySingleAsync<int>(AddTokenQuery, User.SpotifyToken);

	    string AddSessionQuery = "insert into SpotifySession (SessionUUID, Password, ExpirationTime) values (@Id, @Password, @EndTime); select last_insert_id();";
	    int SessionID = await connection.QuerySingleAsync<int>(AddSessionQuery, User.SpotifySession);

	    string AddServiceDataQuery = "insert into SpotifyServiceData (UserID, SpotifySessionID, SpotifyTokenID) values (@UserID, @SessionID, @TokenID)";
	    connection.Execute(AddServiceDataQuery, new {UserID = UserID, SessionID = SessionID, TokenID = TokenID});
	    
	}


	static public async void UpdateSpotifyToken(SpotifySession session)
	{
        string sessionID = session.Id;
        string newAccessToken = session.SpotifyToken.AccessToken;
        string newRefreshToken = session.SpotifyToken.RefreshToken;
        DateTime newExpirationTime = session.SpotifyToken.AccessTokenExpiration;

	    var connection = await SqlDB.GetOpenConnectionAsync();
        
	    Console.WriteLine($"sessionID: {session.Id}");
	    Console.WriteLine($"AccessToken: {newAccessToken}");
	    Console.WriteLine($"RefreshToken: {newRefreshToken}");

	    // string UserIDQuery = "Select u.UserID from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    // int UserID = await connection.QuerySingleOrDefaultAsync<int>(UserIDQuery, new {device_key = device_key}); 

	    string SpotifyTokenIDQuery = "select ssd.SpotifyTokenID from SpotifySession s join SpotifyServiceData ssd on s.SpotifySessionID = ssd.SpotifySessionID where s.sessionUUID = @sessionID;";
	    int tokenID = await connection.QuerySingleOrDefaultAsync<int>(SpotifyTokenIDQuery, new {sessionID = sessionID}); 

	    string UpdateSpotifyAccessToken = "update SpotifyToken set AccessToken = @newAccessToken, RefreshToken = @newRefreshToken, ExpirationTime = @newExpirationTime where SpotifyTokenID = @tokenID";
	    connection.Execute(UpdateSpotifyAccessToken, new {newAccessToken = newAccessToken, newRefreshToken = newRefreshToken, newExpirationTime = newExpirationTime, tokenID = tokenID});
	    //string UpdateSpotifyRefreshToken = "update SpotifySession set RefreshToken = @newRefreshToken where SpotifyTokenID = @tokenID";
    }

	static public async void ToggleShareSpotifySession(HttpRequest Request)
	{
        
	    string device_key = default;
	    string hmac_secret = default;
        /*
            bool succes_device_key = Request.Cookies.TryGetValue("device_key", out device_key);
            bool succes_hmac_secret = Request.Cookies.TryGetValue("hmac_secret", out hmac_secret);

	    if (!(succes_device_key && succes_hmac_secret)){
		    Console.WriteLine("device_key and/or hmac_secret NOT found!");
		    return;
	    }

	    (bool authSucces, int statusCode, string authResponse) = await Authentication.VerifyClient(Request);
            if (!authSucces)
	        return;
*/
	    var connection = await SqlDB.GetOpenConnectionAsync();

	    string UserIDQuery = "Select u.UserID from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    int UserID = await connection.QuerySingleOrDefaultAsync<int>(UserIDQuery, new {device_key = device_key}); 

	    string UsernameQuery = "Select u.Username from User u join Device d on u.UserID = d.UserID where d.DeviceKey = @device_key";
	    string Username = await connection.QuerySingleOrDefaultAsync<string>(UsernameQuery, new {device_key = device_key}); 

	    string SpotifySessionIDQuery = "Select ss.SpotifySessionID from SpotifyServiceData sd join SpotifySession ss on sd.SpotifySessionID = ss.SpotifySessionID where sd.UserID = @UserID";
	    int sessionID = await connection.QuerySingleOrDefaultAsync<int>(SpotifySessionIDQuery, new {UserID = UserID});

	    string UpdateSpotifySessionShare = "update SpotifySession set IsPublic = Not IsPublic where SpotifySessionID = @sessionID";
	    connection.Execute(UpdateSpotifySessionShare, new {sessionID = sessionID});

	    string UpdateSpotifySessionExpiration = "update SpotifySession set ExpirationTime = Date_add(NOW(), interval 1 day) where SpotifySessionID = @sessionID";
	    connection.Execute(UpdateSpotifySessionExpiration, new {sessionID = sessionID});
	    return;
	}

	static public async Task<List<SpotifySession>> GetPublicSessionList()
	{
	    var connection = await SqlDB.GetOpenConnectionAsync();

	    string PublicSessionQuery = "Select concat(u.Username, \'s session') as Name, ss.SessionUUID as Id, ss.ExpirationTime as EndTime, ss.IsPublic, ss.Password as _password from SpotifySession as ss join SpotifyServiceData ssd on ss.SpotifySessionID = ssd.SpotifySessionID join User as u on ssd.UserID = u.UserID where ss.IsPublic = 1 and now() < ss.ExpirationTime";
	    var result = await connection.QueryAsync<SpotifySession>(PublicSessionQuery);
	    return result.ToList();
	}

	static public async Task<Tuple<bool, SpotifySession>> GetPublicSessionFromID(string id)
	{
	    var connection = await SqlDB.GetOpenConnectionAsync();

	    string SessionIDQuery = "Select SpotifySessionID from SpotifySession where IsPublic = 1 and now() < ExpirationTime and SessionUUID = @id";
	    int sessionID = await connection.QuerySingleOrDefaultAsync<int>(SessionIDQuery, new {id = id});

	    string PublicSessionQuery = "Select SessionUUID as Id, ExpirationTime as EndTime, IsPublic, Password as _password from SpotifySession where IsPublic = 1 and now() < ExpirationTime and SessionUUID = @id";
	    SpotifySession session = await connection.QuerySingleOrDefaultAsync<SpotifySession>(PublicSessionQuery, new {id = id});
	    
	    if (session == null)
		    return new Tuple<bool, SpotifySession>(false, default);

	    string SpotifyTokenQuery = "Select st.Code as Code, st.AccessToken as AccessToken, st.RefreshToken as RefreshToken, st.Status as Status, st.ExpirationTime as AccessTokenExpiration from SpotifyServiceData sd join SpotifyToken st on sd.SpotifyTokenID = st.SpotifyTokenID where sd.SpotifySessionID = @sessionID";
	    SpotifyAPIToken token = await connection.QuerySingleOrDefaultAsync<SpotifyAPIToken>(SpotifyTokenQuery, new {sessionID = sessionID});
	    session.SpotifyToken = token;

	    return new Tuple<bool, SpotifySession>(true, session);
	}
    }
}
