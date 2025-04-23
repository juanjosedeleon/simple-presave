using Microsoft.Extensions.Options;
using SimplePresave.Libraries.Model;
using System.Text;
using System.Text.Json;

namespace SimplePresave.Libraries.Services;

public class SpotifyService
{
    public class GrantType
    {
        public const string AUTHORIZATION_CODE = "authorization_code";
        public const string REFRESH_TOKEN = "refresh_token";
    }

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
        var result = await GetToken(GrantType.AUTHORIZATION_CODE, authorizationCode, redirectUri);

        return result;
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
        var accessToken = await GetNewAccessToken(message.RefreshToken);
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

    private async Task<string> GetNewAccessToken(string refreshToken)
    {
        var result = await GetToken(GrantType.REFRESH_TOKEN, refreshToken);

        return result["access_token"];
    }

    private async Task<Dictionary<string, string>> GetToken(string grantType, string tokenOrAuthCode, string redirectUri = "")
    {
        var tokenUrl = "https://accounts.spotify.com/api/token";
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

        KeyValuePair<string, string>[] requestParameters;
        switch (grantType)
        {
            case GrantType.AUTHORIZATION_CODE:
                requestParameters = new[]
                {
                    new KeyValuePair<string, string>("grant_type", GrantType.AUTHORIZATION_CODE),
                    new KeyValuePair<string, string>("code", tokenOrAuthCode),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri)
                };
                break;
            case GrantType.REFRESH_TOKEN:
                requestParameters = new[]
                {
                    new KeyValuePair<string, string>("grant_type", GrantType.REFRESH_TOKEN),
                    new KeyValuePair<string, string>("refresh_token", tokenOrAuthCode)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(grantType), grantType, null);
        }

        var requestBody = new FormUrlEncodedContent(requestParameters);
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

        return new Dictionary<string, string>
        {
            { "access_token", responseData.access_token },
            { "refresh_token", responseData.refresh_token },
            { "expires_in", responseData.expires_in.ToString() }
        };
    }
}
