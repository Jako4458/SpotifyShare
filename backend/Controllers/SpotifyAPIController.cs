using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyController.Data;
using SpotifyController.Model;
using SpotifyController.Model.SpotifyAPI;
using SpotifyController.Services;
using Microsoft.AspNetCore.Mvc;
using SpotifyController.Exceptions;
using SpotifyController.Utils;
using SpotifyController.Interfaces;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using FlaskAuthSDK;

namespace LogenV2React.Controllers
{
    [Route("API/[controller]/[action]")]
    [ApiController]
    public class SpotifyAPIController : ControllerBase
    {

        private SpotifyAPIService _spotifyAPIService;
        private SpotifyTokenManager _spotifyTokenManager;
        private FlaskAuthClient _flaskAuthClient;
        private IDataRepository _dataRepository;

        public SpotifyAPIController(SpotifyAPIService spotifyAPIService, SpotifyTokenManager spotifyTokenManager, FlaskAuthClient flaskAuthClient, IDataRepository dataRepository)
        {
            this._spotifyAPIService = spotifyAPIService;
            this._spotifyTokenManager = spotifyTokenManager;
            this._flaskAuthClient = flaskAuthClient;
            this._dataRepository = dataRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Authorize([FromQuery] string? redirect_uri="/spotify/")
        {
            return Redirect(_spotifyAPIService.AuthorizeUser(User.FindFirstValue(ClaimTypes.NameIdentifier), redirect_uri));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AccessToken([FromQuery] string? redirect_uri="/")
        {

            (bool userFound, User user) = await _dataRepository.TryGetUser(this.User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!userFound)
                return NotFound("User Not Found");
            
            try
            {
                (bool success, SpotifyAPIToken spotifyToken) = await _spotifyTokenManager.GetToken(user.SpotifySession);
                //(bool succes, SpotifyToken spotifyToken) = await _spotifyAPIService.GetTokenObject(user.SpotifySession);
                if (!success)
                    return NotFound($"Could not get Token for user - Token response!");
            }
            catch (SpotifyNotConnectedException)
            {
                return StatusCode(507);
                // return Redirect("/API/SpotifyAPI/Authorize");
            }

            return Redirect(redirect_uri);
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> QueueTrack([FromHeader] string? SpotifySessionId, [FromQuery] string trackId, [FromQuery] bool raw=false)
        {
            bool authSuccess = User.Identity?.IsAuthenticated == true;
            bool userFound;
            User user;

            if (authSuccess)
                (userFound, user) = await _dataRepository.TryGetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                (userFound, user) = (false, null);

            if (trackId == null)
                return NotFound("No trackId supplied!");

            SpotifySession spotifySession;
            bool sessionFound;
            (sessionFound, spotifySession) = await _dataRepository.GetPublicSessionFromID(SpotifySessionId);

            Console.WriteLine("Session id:");
            Console.WriteLine(spotifySession.Id);

            // Find a valid session
            if (!sessionFound)
            {
                Console.WriteLine("Session Found");
                // If no public session and unauhtorized
                if (!authSuccess)
                    return StatusCode(401);

                if (!userFound)
                    return NotFound("User Not Found");

                if (SpotifySessionId is null || SpotifySessionId == "" || SpotifySessionId == "undefined"  || user.SpotifySession.Id == SpotifySessionId)
                {
                    sessionFound = true;
                    spotifySession = user.SpotifySession;
                }
                else
                    return Unauthorized("User cannot access the requested session");
            }

            bool succesfullyAdded;
            string responseContent;

            // Queue to session
            try
            {
                Console.WriteLine("Session id:");
                Console.WriteLine(spotifySession.Id);
                (succesfullyAdded, responseContent) = await _spotifyAPIService.QueueTrack(spotifySession, trackId);
            }
            catch (SpotifyNotConnectedException)
            {
                return Unauthorized("Not Connected to spotify!");
            }

            if (!succesfullyAdded)
                return NotFound("Song could not be added to queue!");
            if (raw)
                return Ok(responseContent);

            return Ok("Song succesfully added to queue!");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylists([FromQuery] bool raw=false)
        {
            bool authSuccess = User.Identity?.IsAuthenticated == true;
            bool userFound;
            User user;

            if (authSuccess)
                (userFound, user) = await _dataRepository.TryGetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                (userFound, user) = (false, null);

            bool successfulGet;
            Playlists playlists;
            string responseContent;

            try
            {
                if (userFound)
                {
                    (successfulGet, playlists, responseContent) = await _spotifyAPIService.GetCurrentUsersPlaylists(user.SpotifySession);
                } else
                {
                    (successfulGet, playlists, responseContent) = await _spotifyAPIService.GetPublicPlaylistExample();
                }
                
            }
            catch (SpotifyNotConnectedException)
            {
                return Unauthorized(new {Message = "Not Connected to spotify!"});
            }

            if (!successfulGet)
                return NotFound("Playlists could not be found!");
            if (raw)
                return Ok(responseContent);

            return Ok(playlists);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylist([FromQuery] string playlistId, [FromQuery] bool raw = false)
        {
            bool authSuccess = User.Identity?.IsAuthenticated == true;
            bool userFound;
            User user;

            if (authSuccess)
                (userFound, user) = await _dataRepository.TryGetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                (userFound, user) = (false, null);

            if (playlistId == null)
                return NotFound("No playlistId supplied!");

            bool successfulGet;
            Playlist playlist;
            string responseContent;

            try
            {
                if (userFound)
                {
                    (successfulGet, playlist, responseContent) = await _spotifyAPIService.GetPlaylist(user.SpotifySession, playlistId);
                } else
                {
                    (successfulGet, playlist, responseContent) = await _spotifyAPIService.GetPlaylist(null, playlistId);
                }
            }
            catch (SpotifyNotConnectedException)
            {
                return Unauthorized(new {Message = "Not Connected to spotify!"});
            }

            if (raw) 
                return Ok(responseContent);
            if (!successfulGet)
                return NotFound("Playlist could not be found!");
            
            return Ok(playlist);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylistTracks([FromQuery] string playlistId, [FromQuery] int offset=0, [FromQuery] int limit=20, [FromQuery] bool raw = false)
        {
            bool authSuccess = User.Identity?.IsAuthenticated == true;
            bool userFound;
            User user;

            if (authSuccess)
                (userFound, user) = await _dataRepository.TryGetUser(User.FindFirstValue(ClaimTypes.NameIdentifier));
            else
                (userFound, user) = (false, null);

            if (playlistId == null)
                return NotFound("No playlistId supplied!");

            bool successfulGet;
            PlaylistTracks playlistTracks;
            string responseContent;

            try
            {
                if (userFound)
                    (successfulGet, playlistTracks, responseContent) = await _spotifyAPIService.GetPlaylistTracks(user.SpotifySession, playlistId, offset, limit);
                else 
                    (successfulGet, playlistTracks, responseContent) = await _spotifyAPIService.GetPlaylistTracks(null, playlistId, offset, limit);
            }
            catch (SpotifyNotConnectedException)
            {
                return Unauthorized(new {Message = "Not Connected to spotify!"});
            }

            if (!successfulGet)
                return NotFound("Playlist could not be found!");
            if (raw) 
                return Ok(responseContent);
            
            return Ok(playlistTracks);
        }


        public async Task<IActionResult> Search([FromHeader] string SpotifySessionId, [FromQuery] string query, [FromQuery] bool raw = false)
        {
            if (query == null)
                return NotFound("No search query found");

            bool successfulSearch;
            Search searchResult;
            string responseContent;

            try
            {
                (successfulSearch, searchResult, responseContent) = await _spotifyAPIService.Search(query);
            }
            catch (SpotifyNotConnectedException)
            {
                return Unauthorized(new { Message = "Not Connected to spotify!" });
            }

            if (!successfulSearch)
                return NotFound("Search failed!");
            if (raw)
                return Ok(responseContent);
            
            return Ok(searchResult);
        }

    }
}
