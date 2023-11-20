using Microsoft.Extensions.Logging;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client.Api.Builder;

public interface IOAuth2TokenProviderBuilder
{
    /// <summary>
    /// Defines the logger factory which should be used by the token provider
    /// to log messages.
    /// *This is optional and no messages are logged if this method is not called.*
    /// </summary>
    /// <param name="loggerFactory">the factory to create an ILogger</param>
    /// <returns>the fluent IOAuth2TokenProviderBuilder</returns>
    IOAuth2TokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory);

    /// <summary>
    /// Defines the authorization server, from which the access token should be requested.
    /// </summary>
    /// <param name="url">an url, which points to the authorization server</param>
    /// <returns>the next step in building a OAuth2TokenProvider</returns>
    IOAuth2TokenProviderBuilderStep2 UseAuthServer(string url);
}

public interface IOAuth2TokenProviderBuilderStep2
{
    /// <summary>
    /// Defines the client id, which should be used to create the access token.
    /// </summary>
    /// <param name="clientId">the client id, which is supplied by OAuth2</param>
    /// <returns>the next step in building a OAuth2TokenProvider</returns>
    IOAuth2TokenProviderBuilderStep3 UseClientId(string clientId);
}

public interface IOAuth2TokenProviderBuilderStep3
{
    /// <summary>
    /// Defines the client secret, which should be used to create the access token.
    /// </summary>
    /// <param name="clientSecret">the client secret, which is supplied by OAuth2</param>
    /// <returns>the next step in building a OAuth2TokenProvider</returns>
    IOAuth2TokenProviderBuilderStep4 UseClientSecret(string clientSecret);
}

public interface IOAuth2TokenProviderBuilderStep4
{
    /// <summary>
    /// Defines the audience for which the token provider should create tokens.
    /// </summary>
    /// <param name="audience">the audience, which is normally a domain name</param>
    /// <returns>the next step in building a OAuth2TokenProvider</returns>
    IOAuth2TokenProviderBuilderFinalStep UseAudience(string audience);
}

public interface IOAuth2TokenProviderBuilderFinalStep
{
    /// <summary>
    /// Builds the OAuth2TokenProvider, which can be used by the ZeebeClient to
    /// communicate with a Self-Hosted Zeebe gateway, which uses identity.
    /// </summary>
    /// <returns>the OAuth2TokenProvider</returns>
    OAuth2TokenProvider Build();
}