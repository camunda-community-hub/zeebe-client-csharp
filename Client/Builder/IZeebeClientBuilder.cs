namespace Zeebe.Client.Builder
{
    public interface IZeebeClientBuilder
    {
        /// <summary>
        /// The address which the client should connect to.
        /// </summary>
        /// <param name="gatewayAddress">the address to which the client should connect to</param>
        /// <returns>
        ///   the next step builder, to configure transport security which the client should use
        /// </returns>
        IZeebeClientTransportBuilder UseGatewayAddress(string gatewayAddress);
    }

    public interface IZeebeClientTransportBuilder
    {
        /// <summary>
        /// To create a client which uses client-side SSL. The given path
        /// points to a file, which contains root certificates (PEM encoded).
        /// </summary>
        /// <param name="rootCertificatePath">the path to the root certificates</param>
        /// <returns>the builder to create a secure client</returns>
        IZeebeSecureClientBuilder UseTransportEncryption(string rootCertificatePath);

        /// <summary>
        /// To create an client without any transport encryption.
        /// </summary>
        /// <returns>the final client builder to create the client</returns>
        IZeebeClientFinalBuildStep UsePlainText();
    }

    public interface IZeebeSecureClientBuilder : IZeebeClientFinalBuildStep
    {
        /// <summary>
        /// Client should use the given access token to authenticate with.
        /// </summary>
        /// <param name="accessToken">the access token which is used for authentication</param>
        /// <returns>the final client builder step</returns>
        IZeebeClientFinalBuildStep UseAccessToken(string accessToken);
    }

    public interface IZeebeClientFinalBuildStep
    {
        /// <summary>
        /// Builds the client with the given configuration.
        /// </summary>
        /// <returns>the client which was build with the given configuration</returns>
        IZeebeClient Build();
    }
}