using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fiksu.Web;
using GetPkg.Auth.OAuth2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GetPkg.Auth.OAuth2 {
    public class OAuth2AuthenticationProvider : ISsoAuthenticationProvider {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings() { 
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        private static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(DefaultSerializerSettings);

        private readonly OAuth2Configuration _configuration;
        private readonly HttpClient _client;

        public OAuth2AuthenticationProvider(OAuth2Configuration configuration, HttpClient client) {
            _configuration = configuration;
            _client = client;
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(IHttpRequest request, SsoRequestOptions originalOptions) {
            var state = request.QueryString["state"].SingleOrDefault();
            var code = request.QueryString["code"].SingleOrDefault();

            if (code != null && state != null) {
                var token = await GetAccessTokenAsync(code, originalOptions).ConfigureAwait(false);
                return new ClaimsPrincipal(new ClaimsIdentity("oauth2.custom"));
            }
            else {
                var error = request.QueryString["error"].SingleOrDefault() ?? "unknown error";
                var description = request.QueryString["error_description"].SingleOrDefault();

                throw new Exception($"invalid response received from server: {error}" + description != null ? $" ({description})" : "");
            }
        }

        public Task<Uri> GetAuthorizationUriAsync(SsoRequestOptions options) {
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

        private async Task<OAuth2TokenResponse> GetAccessTokenAsync(string code, SsoRequestOptions options) {
            var tokenRequest = new OAuth2TokenRequest() {
                GrantType = "authorization_code",
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret,
                RedirectUri = options.RedirectUri.ToString(),
                Code = code
            };


            using (var request = new HttpRequestMessage(HttpMethod.Post, _configuration.TokenEndpoint)) {
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonConvert.SerializeObject(tokenRequest, DefaultSerializerSettings), Encoding.UTF8, "application/json");

                using (var response = await _client.SendAsync(request).ConfigureAwait(false)) {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                        return JsonConvert.DeserializeObject<OAuth2TokenResponse>(json, DefaultSerializerSettings);

                    var error = JsonConvert.DeserializeObject<OAuth2ErrorResponse>(json, DefaultSerializerSettings);
                    throw new Exception($"{(int)response.StatusCode} - invalid response returned from server: {error.Error} ({error.ErrorDescription})");
                }
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
