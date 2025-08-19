using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyController.Data;
using SpotifyController.Model;
using SpotifyController.Model.SpotifyAPI;
using SpotifyController.Services;
using Microsoft.AspNetCore.Mvc;
using SpotifyController.Exceptions;
using SpotifyController.Interfaces;

namespace LogenV2React.Controllers
{
    [Route("API/[controller]/[action]")]
    [ApiController]
    public class SpotifySessionController : ControllerBase
    {
        private IDataRepository _dataRepository;

        public SpotifySessionController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicSessions()
        {
            List<SharedSpotifySession> publicSpotifySessions = new List<SharedSpotifySession>();
            
            List<SpotifySession> publicSessions = await _dataRepository.GetPublicSessionList();

            foreach (SpotifySession session in publicSessions)
            {
                publicSpotifySessions.Add(
                    new SharedSpotifySession()
                    {
                        Id = session.Id,
                        Name = session.Name,
                        HasPassword = session.Password != default,
                        Url = session.Url
                    }
                );

            }

            return Ok(publicSpotifySessions);
        }

    }
}
