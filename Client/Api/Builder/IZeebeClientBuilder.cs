using System;
using Microsoft.Extensions.Logging;

namespace Zeebe.Client.Api.Builder;

public interface IZeebeClientBuilder
{
    /// <summary>
    /// The logger factory which the client should use to log messages.
    /// This is optional and if not set the client will not log any messages.
    /// </summary>
    /// <param name="loggerFactory">the factory which is used to create an logger.</param>
    /// <returns>
    ///   the builder, to configure the zeebe client.
    /// </returns>
    IZeebeClientBuilder UseLoggerFactory(ILoggerFactory loggerFactory);

    /// <summary>
    /// The address which the client should connect to.
    /// </summary>
    /// <param name="gatewayAddress">the address to which the client should connect to.</param>
    /// <returns>
    ///   the next step builder, to configure transport security which the client should use.
    /// </returns>
    IZeebeClientTransportBuilder UseGatewayAddress(string gatewayAddress);
}

public interface IZeebeClientTransportBuilder
{
    /// <summary>
    /// To create a client which uses client-side SSL. The given path
    /// points to a file, which contains root certificates (PEM encoded).
    /// </summary>
    /// <param name="rootCertificatePath">the path to the root certificates.</param>
    /// <returns>the builder to create a secure client.</returns>
    IZeebeSecureClientBuilder UseTransportEncryption(string rootCertificatePath);

    /// <summary>
    /// To create a client which uses client-side SSL.
    /// </summary>
    /// <returns>the builder to create a secure client.</returns>
    IZeebeSecureClientBuilder UseTransportEncryption();

    /// <summary>
    /// To create an client without any transport encryption.
    /// </summary>
    /// <returns>the final client builder to create the client.</returns>
    IZeebeClientFinalBuildStep UsePlainText();
}

public interface IZeebeSecureClientBuilder : IZeebeClientFinalBuildStep
{
    /// <summary>
    /// DANGER: This allows untrusted certificates for the gRPC connection with Zeebe.
    ///
    /// This setting tells the client to allow to use untrusted certificates in the underlying SocketHttpHandler.
    /// For further details see https://github.com/dotnet/runtime/issues/42482. This might be useful for testing.
    /// </summary>
    /// <returns>the builder to create a secure client.</returns>
    IZeebeSecureClientBuilder AllowUntrustedCertificates();

    /// <summary>
    /// Client should use the given access token to authenticate with.
    /// </summary>
    /// <param name="accessToken">the access token which is used for authentication.</param>
    /// <returns>the final client builder step.</returns>
    IZeebeClientFinalBuildStep UseAccessToken(string accessToken);

    /// <summary>
    /// Client should use an access token to authenticate with and the given
    /// supplier should be used to receive the token.
    /// </summary>
    /// <param name="supplier">the access token supplier which is called to supplied the access token.</param>
    /// <returns>the final client builder step.</returns>
    IZeebeClientFinalBuildStep UseAccessTokenSupplier(IAccessTokenSupplier supplier);
}

public interface IZeebeClientFinalBuildStep
{
    /// <summary>
    /// Uses the given time interval to determine when to send a keepalive ping
    /// to the gateway. The default is 30 seconds.
    ///
    /// <p>This is an optional configuration.</p>
    /// </summary>
    /// <param name="keepAlive">the timespan between keep alive requests.</param>
    /// <returns>the final step builder.</returns>
    IZeebeClientFinalBuildStep UseKeepAlive(TimeSpan keepAlive);

    /// <summary>
    /// Uses the given duration provider for the send retries.
    /// On each retry, the duration to wait is calculated by calling <paramref name="sleepDurationProvider" /> with
    /// the current retry number (1 for first retry, 2 for second etc).
    ///
    /// <p>This is an optional configuration. Per default the wait time provider provides base two wait time,
    /// 2^1 seconds, 2^2 seconds, 2^3 seconds etc. until one minute is reached.</p>
    /// </summary>
    /// <param name="sleepDurationProvider">The function that provides the duration to wait for for a particular retry attempt.</param>
    /// <returns>the final step builder.</returns>
    IZeebeClientFinalBuildStep UseRetrySleepDurationProvider(Func<int, TimeSpan> sleepDurationProvider);

    /// <summary>
    /// Builds the client with the given configuration.
    /// </summary>
    /// <returns>the client which was build with the given configuration.</returns>
    IZeebeClient Build();
}