using System;
using System.Threading;
using Grpc.Core;

namespace GatewayProtocol
{
    public class ClosedGatewayClient : Gateway.GatewayClient
    {
        private const string ZeebeClientWasAlreadyDisposed = "ZeebeClient was already disposed.";

        public override AsyncServerStreamingCall<ActivateJobsResponse> ActivateJobs(ActivateJobsRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncServerStreamingCall<ActivateJobsResponse> ActivateJobs(ActivateJobsRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CancelProcessInstanceResponse CancelProcessInstance(CancelProcessInstanceRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CancelProcessInstanceResponse CancelProcessInstance(CancelProcessInstanceRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CancelProcessInstanceResponse> CancelProcessInstanceAsync(CancelProcessInstanceRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CancelProcessInstanceResponse> CancelProcessInstanceAsync(CancelProcessInstanceRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CompleteJobResponse CompleteJob(CompleteJobRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CompleteJobResponse CompleteJob(CompleteJobRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CompleteJobResponse> CompleteJobAsync(CompleteJobRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CompleteJobResponse> CompleteJobAsync(CompleteJobRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CreateProcessInstanceResponse CreateProcessInstance(CreateProcessInstanceRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override CreateProcessInstanceResponse CreateProcessInstance(CreateProcessInstanceRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CreateProcessInstanceResponse> CreateProcessInstanceAsync(CreateProcessInstanceRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<CreateProcessInstanceResponse> CreateProcessInstanceAsync(CreateProcessInstanceRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override DeployProcessResponse DeployProcess(DeployProcessRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override DeployProcessResponse DeployProcess(DeployProcessRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<DeployProcessResponse> DeployProcessAsync(DeployProcessRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<DeployProcessResponse> DeployProcessAsync(DeployProcessRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override FailJobResponse FailJob(FailJobRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override FailJobResponse FailJob(FailJobRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<FailJobResponse> FailJobAsync(FailJobRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<FailJobResponse> FailJobAsync(FailJobRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override PublishMessageResponse PublishMessage(PublishMessageRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override PublishMessageResponse PublishMessage(PublishMessageRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<PublishMessageResponse> PublishMessageAsync(PublishMessageRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<PublishMessageResponse> PublishMessageAsync(PublishMessageRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override ResolveIncidentResponse ResolveIncident(ResolveIncidentRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override ResolveIncidentResponse ResolveIncident(ResolveIncidentRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<ResolveIncidentResponse> ResolveIncidentAsync(ResolveIncidentRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<ResolveIncidentResponse> ResolveIncidentAsync(ResolveIncidentRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override SetVariablesResponse SetVariables(SetVariablesRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override SetVariablesResponse SetVariables(SetVariablesRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<SetVariablesResponse> SetVariablesAsync(SetVariablesRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<SetVariablesResponse> SetVariablesAsync(SetVariablesRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override TopologyResponse Topology(TopologyRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override TopologyResponse Topology(TopologyRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<TopologyResponse> TopologyAsync(TopologyRequest request, Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<TopologyResponse> TopologyAsync(TopologyRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override UpdateJobRetriesResponse UpdateJobRetries(UpdateJobRetriesRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override UpdateJobRetriesResponse UpdateJobRetries(UpdateJobRetriesRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<UpdateJobRetriesResponse> UpdateJobRetriesAsync(UpdateJobRetriesRequest request, Metadata headers = null,
            DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        public override AsyncUnaryCall<UpdateJobRetriesResponse> UpdateJobRetriesAsync(UpdateJobRetriesRequest request, CallOptions options)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }

        protected override Gateway.GatewayClient NewInstance(ClientBaseConfiguration configuration)
        {
            throw new ObjectDisposedException(ZeebeClientWasAlreadyDisposed);
        }
    }
}
