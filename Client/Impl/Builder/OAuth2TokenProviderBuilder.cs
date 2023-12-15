using System;
using Microsoft.Extensions.Logging;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Impl.Builder;

public class OAuth2TokenProviderBuilder : IOAuth2TokenProviderBuilder,
    IOAuth2TokenProviderBuilderStep2,
    IOAuth2TokenProviderBuilderStep3,
    IOAuth2TokenProviderBuilderStep4,
    IOAuth2TokenProviderBuilderFinalStep
{
    private ILoggerFactory loggerFactory;
    private string audience;
    private string authServer = "https://login.cloud.camunda.io/oauth/token";
    private string clientId;
    private string clientSecret;

    /// <inheritdoc/>
    public IOAuth2TokenProviderBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        return this;
    }

    /// <inheritdoc/>
    public IOAuth2TokenProviderBuilderStep2 UseAuthServer(string url)
    {
        authServer = url ?? throw new ArgumentNullException(nameof(url));
        return this;
    }

    /// <inheritdoc/>
    public IOAuth2TokenProviderBuilderStep3 UseClientId(string clientId)
    {
        this.clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        return this;
    }

    /// <inheritdoc/>
    public IOAuth2TokenProviderBuilderStep4 UseClientSecret(string clientSecret)
    {
        this.clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        return this;
    }

    /// <inheritdoc/>
    public IOAuth2TokenProviderBuilderFinalStep UseAudience(string audience)
    {
        this.audience = audience ?? throw new ArgumentNullException(nameof(audience));
        return this;
    }

    /// <inheritdoc/>
    public OAuth2TokenProvider Build()
    {
        return new OAuth2TokenProvider(
            authServer,
            clientId,
            clientSecret,
            audience,
            loggerFactory?.CreateLogger<OAuth2TokenProvider>());
    }
}