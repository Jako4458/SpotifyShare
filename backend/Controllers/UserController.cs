using System;
using System.Collections.Generic;
using SpotifyController.Data;
using SpotifyController.Model;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using SpotifyController.Utils;
using SpotifyController.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using FlaskAuthSDK;

namespace SpotifyController.Controllers
{
    [Route("API/{controller}/{action}")]
    [ApiController]
    public class UserController : ControllerBase
    {

    private FlaskAuthClient _flaskAuthClient;
    private IDataRepository _dataRepository;

	public UserController(FlaskAuthClient flaskAuthClient, IDataRepository dataRepository)
    {
        this._flaskAuthClient = flaskAuthClient;
        this._dataRepository = dataRepository;
    }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AuthorizeSpotify([FromQuery]string code, [FromQuery]string? error, [FromQuery]string? state)
        {
            string userServiceToken = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            (bool userFound, User user) = await _dataRepository.TryGetUser(userServiceToken);

            // Add a user
            if (!userFound)
                _dataRepository.AddUser(userServiceToken);

           // state is a query string
            string stateAsQueryString = state.Replace(";", "&");

            var stateQueries = HttpUtility.ParseQueryString(stateAsQueryString);
            string redirect_uri = stateQueries.Get("redirect_uri");

            if (error == null && code != null)
            {

                // Add connections for newly created user to SpotifyAPI
                // And Update Token if User already connected 
                if (!userFound || !user!.IsConnectedToSpotify)
                    _dataRepository.AddSpotifyConnection(userServiceToken, new SpotifySession() { EndTime = DateTime.Now }, new SpotifyAPIToken(code, stateAsQueryString));
                else 
                    _dataRepository.UpdateSpotifyToken(new SpotifyAPIToken(code, stateAsQueryString));
                    
            }
    
            if (stateAsQueryString != null)
                return Redirect($"/API/SpotifyAPI/AccessToken?{stateAsQueryString}");

            return Redirect($"/API/SpotifyAPI/AccessToken");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ShareSession()
        {
            _dataRepository.ToggleShareSpotifySession(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok($"Your session is public the next 24 Hours");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetUser()
        {
            var (authSuccess, authUser) = await _flaskAuthClient.VerifyClientFromHttpRequest(Request);
            if (!authSuccess)
                return StatusCode(401);

            Console.WriteLine($"AuthUser: {authUser}");

            var (userFound, user) = await _dataRepository.TryGetUser(authUser.UserServiceToken);
            Console.WriteLine($"UserFound: {userFound}");

            //(bool userFound, User user) = await _dataRepository.TryGetUser(this.User.FindFirstValue(ClaimTypes.NameIdentifier), true);

            if (!userFound)
                return NotFound("User Not Found");

            return Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            (bool authSucces, AuthUserData authUser) = await _flaskAuthClient.VerifyClientFromHttpRequest(Request);
            if (!authSucces)
                return StatusCode(401);

            (bool userFound, User user) = await _dataRepository.TryGetUser(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!userFound)
                return NotFound("User Not Found!");

            //_dataRepository.logout(this.User.FindFirstValue(ClaimTypes.NameIdentifier)); ! TODO FIX LOGOUT TO SQL 

            return Redirect("/spotify/");
        }
    }
}
