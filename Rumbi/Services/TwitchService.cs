using Newtonsoft.Json;
using Rumbi.Data.Config;
using System.Net.Http.Headers;

namespace Rumbi.Services
{
    public class TwitchService
    {
        private readonly RumbiConfig _config;

        public TwitchService(RumbiConfig config)
        {
            _config = config;
        }

        public async Task<string> Authenticate()
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(
                new HttpMethod("POST"),
                "https://id.twitch.tv/oauth2/token"
            );
            request.Content = new StringContent(
                $"client_id={_config.TwitchSecret}&client_secret={_config.TwitchSecret}&grant_type=client_credentials"
            );
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                "application/x-www-form-urlencoded"
            );

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException();

            string responseBody = await response.Content.ReadAsStringAsync();
            TwitchTokenResponse json = JsonConvert.DeserializeObject<TwitchTokenResponse>(
                responseBody
            );

            return json.AccessToken;
        }

        public async Task<string> GetStreamGame(string accesssToken, string channel)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(
                new HttpMethod("GET"),
                $"https://api.twitch.tv/helix/streams?user_login={channel}"
            );
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accesssToken}");
            request.Headers.TryAddWithoutValidation("Client-Id", $"{_config.TwitchClientId}");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException();

            string responseBody = await response.Content.ReadAsStringAsync();

            TwitchStreamResponse json = JsonConvert.DeserializeObject<TwitchStreamResponse>(
                responseBody
            );

            var gameName = json.Data.FirstOrDefault()?.GameName;

            if (string.IsNullOrEmpty(gameName))
                return string.Empty;
            return gameName;
        }

        public class TwitchTokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        public class TwitchStreamResponse
        {
            [JsonProperty("data")]
            public List<TwitchStreamDataResponse> Data { get; set; }
        }

        public class TwitchStreamDataResponse
        {
            [JsonProperty("game_name")]
            public string? GameName { get; set; }
        }
    }
}
