using GetPkg.Auth.OAuth2;
using System;
using System.Collections.Generic;

namespace GetPkg.Auth.Oidc {
    public interface IOidcConfiguration : IOAuth2Configuration {

    }

    public class OidcConfiguration : OAuth2Configuration, IOidcConfiguration {

    }
}

/*
    how do I aim to use this?
        1) retrieve sso/direct auth provider (need to support both for different situations)
            -> if there is the option of setting up multiple, how do they expose a common interface?

            SomeServiceRepo.GetSsoProvider(string org);
            SomeServiceRepo.GetDirectProvider(string org);

            a configuration/settings object needs to be stored, with a well-defined 'type' (i.e. oidc/oauth2/etc.)
            and then set up with any arbitrary properties (i.e. endpoints, id/email paths... etc).

            if I want the authentication providers to be dependency injected, they can't take the configuration as a constructor (only other option is a param).
            which makes it hard to enforce the type of the config (can use a placeholder interface, but then can't get a compile-time guarantee).

            This approach also makes it hard to use (can't return a provider, because you then also need to have another method to fetch the config).

            Could have something like:

            var config = _service.GetSsoConfiguration(organisation);
            var provider = _factory.GetSsoProvider(config.Type);

            provider.DoTheThing(config);

            this has the downside of splitting the configuration detection because the provider needs to know its config, and the service needs to know the config also to deserialize it.

            required types:
                SsoProviderFamily <- enum (sso/direct)
                SsoProviderType <- enum (oidc/oauth2/etc.)
                ISsoProviderConfiguration <- marker interface essentially (except perhaps a fixed 'SsoProviderType Type {get;}')

                ISsoAuthorizationRequest
                    Uri RedirectUri
                    string State

                ISsoAuthorizationProvider
                    GetAuthorizationUriAsync(ISsoAuthorizationConfiguration config, ISsoAuthorizationRequest request);
                    CompleteAuthorizationAsync(ISsoAuthorizationConfiguration config, ISsoAuthorizationRequest request, IDictionary<string, string> response);

                ICredentialsAuthorizationProvider
                    AuthorizeAsync(ICredentialsAuthorizationConfiguration config, ICredentialsAuthorizationRequest request);

                IAuthorizationProviderFactory
                    GetSsoProvider(SsoProviderType type);
                    GetCredentialsProvider(SsoProviderType type);




*/
