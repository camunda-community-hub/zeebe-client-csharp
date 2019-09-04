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
        /// Should create an client with transport encryption, the given root certificate is used
        /// to encrypt the communication.
        /// </summary>
        /// <param name="rootCertificatePath">the path to the root certificate</param>
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