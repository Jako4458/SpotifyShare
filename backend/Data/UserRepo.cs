using SpotifyController.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SpotifyController.Data
{
    static public class UserRepo
    {
        static public Dictionary<string, User> Users = new Dictionary<string, User>();

        static public User TestUser { get; set; }

	//static public User TryGetUserSQL(string device_key, string hmacSecret)
	//{
		
	//}
    }
}
