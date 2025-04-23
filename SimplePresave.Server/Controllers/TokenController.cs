using Microsoft.AspNetCore.Mvc;
using SimplePresave.Libraries.Model;
using SimplePresave.Libraries.Repositories;
using SimplePresave.Libraries.Services;
using System.Text.Json;

namespace SimplePresave.Server.Controllers
{
    [ApiController]
    [Route("token")]
    public class TokenController : ControllerBase
    {
        private readonly ServiceBusRepository _serviceBusRepository;
        private readonly SpotifyService _spotifyService;
        private readonly TokenRepository _tokenRepository;

        public TokenController(
            SpotifyService spotifyService,
            TokenRepository tokenRepository,
            ServiceBusRepository serviceBusRepository)
        {
            _serviceBusRepository = serviceBusRepository;
            _spotifyService = spotifyService;
            _tokenRepository = tokenRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TokenRequestDto tokenRequest)
        {
            if (string.IsNullOrEmpty(tokenRequest.AuthorizationCode) || string.IsNullOrEmpty(tokenRequest.RedirectUri))
            {
                return BadRequest("Se requieren un authorization code y redirect URI válidos.");
            }
            try
            {
                // Obtener los tokens de Spotify y el correo del usuario, y almacenarlo en la base de datos.
                var response = await _spotifyService.RequestAccessToken(tokenRequest.AuthorizationCode, tokenRequest.RedirectUri);
                DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddSeconds(int.Parse(response["expires_in"]));

                // NOTA: Actualmente el correo electrónico no se utiliza para el proceso de presave, pero puede ser util para implementar
                // otras características, siempre recolectándolo y utilizándolo bajo autorización del usuario.
                var email = await _spotifyService.GetUserEmail(response["access_token"]);
                await _tokenRepository.SaveTokens(email, response["access_token"], response["refresh_token"], expiresAt, tokenRequest.TimeOffset);

                // Enviar los tokens al Service Bus con la fecha de publicación según la zona horaria del usuario
                var userPublishTime = _spotifyService.PublishDate.AddMinutes((double)tokenRequest.TimeOffset);
                var presaveMessage = new PresaveMessage
                {
                    UserEmail = email,
                    PublishDate = userPublishTime,
                    RefreshToken = response["refresh_token"]
                };
                await _serviceBusRepository.SendPresaveMessage(JsonSerializer.Serialize(presaveMessage), userPublishTime);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
