using System.Collections.Generic;
using System.Linq;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class DeployResponse : IDeployResponse
    {
        public long Key { get; }
        public IList<IProcessMetadata> Processes { get; }

        public DeployResponse(DeployProcessResponse response)
        {
            Key = response.Key;
            Processes = response.Processes
                .Select(metadata => new ProcessMetadata(metadata))
                .Cast<IProcessMetadata>()
                .ToList();
        }
    }
}
