using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GetPkg.Auth.OAuth2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GetPkg.Auth.OAuth2 {
    public class OAuth2AuthenticationProvider : ISsoAuthenticationProvider {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            }
        };

        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(DefaultSerializerSettings);

        private readonly IOAuth2Configuration _configuration;
        private readonly HttpClient _client;

        public OAuth2AuthenticationProvider(IOAuth2Configuration configuration, HttpClient client) {
            _configuration = configuration;
            _client = client;
        }

        public Task<Uri> GetAuthorizationUriAsync(ISsoAuthorizationOptions options) {
            var builder = new UriBuilder(_configuration.AuthorizationEndpoint) {
                Query = ToQueryString(new Dictionary<string, string>() {
                    { "request_type", "code" },
                    { "client_id", _configuration.ClientId },
                    { "client_secret", _configuration.ClientSecret },
                    { "scope", string.Join(" ", _configuration.Scopes) },
                    { "redirect_uri", options.RedirectUri.ToString() },
                    { "state", options.State }
                })
            };

            return Task.FromResult(builder.Uri);
        }

        public async Task<IUserProfile> CompleteAuthorizationAsync(IDictionary<string, string> parameters, ISsoAuthorizationOptions options) {
            parameters.TryGetValue("code", out var code);
            parameters.TryGetValue("state", out var state);

            if (code == null) {
                parameters.TryGetValue("error", out var error);
                parameters.TryGetValue("error_description", out var description);

                throw new Exception($"server returned an error; {error ?? "unknown error"} ({description ?? "no description"})");
            }

            if (state != options.State)
                throw new Exception("state parameter does not match original request");

            var token = await GetTokenFromCodeAsync(code, options).ConfigureAwait(false);
            return await GetProfileAsync(token.AccessToken).ConfigureAwait(false);
        }

        public async Task<IUserProfile> RefreshAsync(string refreshToken) {
            var token = await GetTokenFromRefreshTokenAsync(refreshToken).ConfigureAwait(false);
            return await GetProfileAsync(token.AccessToken, token.RefreshToken).ConfigureAwait(false);
        }
        
        private Task<OAuth2TokenResponse> GetTokenFromCodeAsync(string code, ISsoAuthorizationOptions options) {
            return GetTokenAsync(new OAuth2TokenRequest() {
                GrantType = "authorization_code",
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret,
                RedirectUri = options.RedirectUri.ToString(),
                Code = code
            });
        }

        private Task<OAuth2TokenResponse> GetTokenFromRefreshTokenAsync(string refreshToken) {
            return GetTokenAsync(new OAuth2TokenRequest() {
                GrantType = "refresh_token",
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret,
                RefreshToken = refreshToken
            });
        }

        private async Task<OAuth2TokenResponse> GetTokenAsync(OAuth2TokenRequest tokenRequest) {
            using (var request = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint)) {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonConvert.SerializeObject(tokenRequest, DefaultSerializerSettings), Encoding.UTF8, "application/json");

                using (var response = await _client.SendAsync(request).ConfigureAwait(false)) {
                    return await ParseResponseAsync<OAuth2TokenResponse>(response).ConfigureAwait(false);
                }
            }
        }

        private async Task<IUserProfile> GetProfileAsync(string accessToken, string refreshToken) {
            using (var request = new HttpRequestMessage(HttpMethod.Get, _configuration.Profile.Endpoint)) {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using (var response = await _client.SendAsync(request).ConfigureAwait(false)) {
                    if (!response.IsSuccessStatusCode)
                        throw new Exception("failed to retrieve user profile from server");

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var obj = JObject.Parse(json);

                    var id = obj.SelectToken(_configuration.Profile.IdPath, true);
                    var email = obj.SelectToken(_configuration.Profile.EmailPath, true);

                    return new UserProfile(id.Value<string>(), email.Value<string>(), refreshToken);
                }
            }
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
            catch(JsonSerializationException) {
                throw new Exception("invalid json returned from server");
            }
        }

        private static string ToQueryString(IDictionary<string, string> map) {
            var builder = new StringBuilder();

            foreach(var kvp in map) {
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
