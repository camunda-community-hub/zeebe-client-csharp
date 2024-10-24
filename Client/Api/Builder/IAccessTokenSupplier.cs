using Google.Apis.Auth.OAuth2;

namespace Zeebe.Client.Api.Builder;

/// <summary>
/// Supplies access tokens to communicate with the gateway.
/// </summary>
public interface IAccessTokenSupplier : ITokenAccess;