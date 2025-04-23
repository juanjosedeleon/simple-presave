using Microsoft.Extensions.Options;
using SimplePresave.Libraries.Model;
using System.Text;
using System.Text.Json;

namespace SimplePresave.Libraries.Services;

public class SpotifyService
{
    public DateTimeOffset PublishDate => _publishDate;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly HttpClient _httpClient;
    private readonly DateTimeOffset _publishDate;
    private readonly string _trackId;

    public SpotifyService(HttpClient httpClient, IOptions<SpotifySettings> spotifySettings)
    {
        _httpClient = httpClient;
        var settings = spotifySettings.Value;
        _clientId = settings.ClientId;
        _clientSecret = settings.ClientSecret;
        _publishDate = settings.PublishDate;
        _trackId = settings.TrackId;
    }

    public async Task<Dictionary<string, string>> RequestAccessToken(string authorizationCode, string redirectUri)
    {
        var tokenUrl = "https://accounts.spotify.com/api/token";
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = requestBody
        };
        request.Headers.Add("Authorization", $"Basic {authHeader}");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Fallo al obtener access token. Código de estatus: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);

        if (responseData == null || string.IsNullOrEmpty(responseData.access_token) || string.IsNullOrEmpty(responseData.refresh_token))
        {
            throw new Exception("Respuesta inválida desde la Spotify API. Faltan access_token o refresh_token.");
        }

        return new Dictionary<string, string>
        {
            { "access_token", responseData.access_token },
            { "refresh_token", responseData.refresh_token },
            { "expires_in", responseData.expires_in.ToString() }
        };
    }

    public async Task<string> GetUserEmail(string accessToken)
    {
        var userUrl = "https://api.spotify.com/v1/me";
        var request = new HttpRequestMessage(HttpMethod.Get, userUrl);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Fallo al obtener access token. Código de estatus: {response.StatusCode}");
        }
        var responseContent = await response.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<SpotifyUserResponse>(responseContent);
        if (userData == null || string.IsNullOrEmpty(userData.email))
        {
            throw new Exception("Respuesta inválida desde la Spotify API. Falta email.");
        }
        return userData.email;
    }

    public async Task AddSongToLibrary(PresaveMessage message)
    {
        var trackUrl = "https://api.spotify.com/v1/me/tracks";
        var accessToken = await RefreshAccessToken(message.RefreshToken);
        var requestBody = new StringContent(JsonSerializer.Serialize(new { ids = new[] { $"{_trackId}" } }), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, trackUrl)
        {
            Content = requestBody
        };
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Fallo al agregar la canción a la biblioteca. Código de estatus: {response.StatusCode}");
        }
    }

    private async Task<string> RefreshAccessToken(string refreshToken)
    {
        var tokenUrl = "https://accounts.spotify.com/api/token";
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });
        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = requestBody
        };
        request.Headers.Add("Authorization", $"Basic {authHeader}");
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Fallo al obtener access token. Código de estatus: {response.StatusCode}");
        }
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);
        if (responseData == null || string.IsNullOrEmpty(responseData.access_token))
        {
            throw new Exception("Respuesta inválida desde la Spotify API. Falta access_token.");
        }

        return responseData.access_token;
    }
}
