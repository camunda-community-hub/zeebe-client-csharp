using System;
using System.Collections.Generic;
using GatewayProtocol;
using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Impl.Responses
{
    public class DeployResourceResponse : IDeployResourceResponse
    {
        public long Key { get; }
        public IList<IProcessMetadata> Processes { get; }
        public IList<IDecisionMetadata> Decisions { get; }
        public IList<IDecisionRequirementsMetadata> DecisionRequirements { get; }
        public IList<IFormMetadata> Forms { get; }

        public DeployResourceResponse(GatewayProtocol.DeployResourceResponse response)
        {
            Key = response.Key;
            Processes = new List<IProcessMetadata>();
            Decisions = new List<IDecisionMetadata>();
            DecisionRequirements = new List<IDecisionRequirementsMetadata>();
            Forms = new List<IFormMetadata>();

            foreach (var deployment in response.Deployments)
            {
                switch (deployment.MetadataCase)
                {
                    case Deployment.MetadataOneofCase.Process:
                        Processes.Add(new ProcessMetadata(deployment.Process));
                        break;
                    case Deployment.MetadataOneofCase.Decision:
                        Decisions.Add(new DecisionMetadata(deployment.Decision));
                        break;
                    case Deployment.MetadataOneofCase.DecisionRequirements:
                        DecisionRequirements.Add(new DecisionRequirementsMetadata(deployment.DecisionRequirements));
                        break;
                    case Deployment.MetadataOneofCase.Form:
                        Forms.Add(new FormMetadata(deployment.Form));
                        break;
                    default:
                        throw new NotImplementedException("Got deployment response for unexpected type: " +
                                                          deployment.MetadataCase);
                }
            }
        }
    }
}
