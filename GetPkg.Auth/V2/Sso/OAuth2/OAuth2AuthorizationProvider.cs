using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GetPkg.Auth.V2.Sso.OAuth2 {
    public class OAuth2AuthorizationProvider : ISsoAuthorizationProvider {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            }
        };

        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(DefaultSerializerSettings);

        private readonly HttpClient _client;

        public OAuth2AuthorizationProvider(HttpClient client) {
            _client = client;
        }

        public Task<Uri> GetAuthorizationUriAsync(ISsoAuthorizationConfiguration configuration, ISsoAuthorizationRequest request) {
            if (!(configuration is OAuth2AuthorizationConfiguration oauth2))
                throw new ArgumentOutOfRangeException(nameof(configuration), $"configuration must be an instance of {nameof(OAuth2AuthorizationConfiguration)}");

            var builder = new UriBuilder(oauth2.AuthorizationEndpoint) {
                Query = ToQueryString(new Dictionary<string, string>() {
                    { "request_type", "code" },
                    { "client_id", oauth2.ClientId },
                    { "client_secret", oauth2.ClientSecret },
                    { "scope", string.Join(" ", oauth2.Scopes) },
                    { "redirect_uri", request.RedirectUri.ToString() },
                    { "state", request.State }
                })
            };

            return Task.FromResult(builder.Uri);
        }

        public async Task<IAuthorizationProfile> CompleteAuthorizationAsync(ISsoAuthorizationConfiguration configuration, ISsoAuthorizationRequest request, ISsoAuthorizationResponse response) {
            AssertConfiguration(configuration, out var oauth2);

            response.Parameters.TryGetValue("code", out var code);
            response.Parameters.TryGetValue("state", out var state);

            if (code == null) {
                response.Parameters.TryGetValue("error", out var error);
                response.Parameters.TryGetValue("error_description", out var description);

                throw new Exception($"server returned an error; {error ?? "unknown error"} ({description ?? "no description"})");
            }

            if (state != request.State)
                throw new Exception("state parameter does not match original request");

            var token = await GetTokenAsync(oauth2, new OAuth2TokenRequest() {
                GrantType = "authorization_code",
                ClientId = oauth2.ClientId,
                ClientSecret = oauth2.ClientSecret,
                RedirectUri = request.RedirectUri.ToString(),
                Code = code
            }).ConfigureAwait(false);

            return await GetProfileAsync(token.AccessToken).ConfigureAwait(false);
        }

        private async Task<OAuth2TokenResponse> GetTokenAsync(OAuth2AuthorizationConfiguration configuration, OAuth2TokenRequest tokenRequest) {
            using (var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint)) {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonConvert.SerializeObject(tokenRequest, DefaultSerializerSettings), Encoding.UTF8, "application/json");

                using (var response = await _client.SendAsync(request).ConfigureAwait(false)) {
                    return await ParseResponseAsync<OAuth2TokenResponse>(response).ConfigureAwait(false);
                }
            }
        }

        private static void AssertConfiguration(ISsoAuthorizationConfiguration configuration, out OAuth2AuthorizationConfiguration oauth2) {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if ((oauth2 = configuration as OAuth2AuthorizationConfiguration) == null)
                throw new ArgumentOutOfRangeException(nameof(configuration), $"configuration must be an instance of {nameof(OAuth2AuthorizationConfiguration)}");
        }

        private static async Task<TResult> ParseResponseAsync<TResult>(HttpResponseMessage response) {
            try {
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var reader = new JsonTextReader(new StreamReader(stream))) {
                    if (response.IsSuccessStatusCode)
                        return DefaultSerializer.Deserialize<TResult>(reader);

                    var error = DefaultSerializer.Deserialize<OAuth2ErrorResponse>(reader);
                    throw new Exception($"invalid response received from server; {error.Error}: {error.ErrorDescription} ({(int)response.StatusCode})");
                }
            }
            catch (JsonSerializationException) {
                throw new Exception("invalid json returned from server");
            }
        }

        private static string ToQueryString(IDictionary<string, string> map) {
            var builder = new StringBuilder();

            foreach (var kvp in map) {
                if (builder.Length > 0)
                    builder.Append('&');

                builder.Append(WebUtility.UrlEncode(kvp.Key));
                builder.Append('=');
                builder.Append(WebUtility.UrlEncode(kvp.Value));
            }

            return builder.ToString();
        }
    }
}
