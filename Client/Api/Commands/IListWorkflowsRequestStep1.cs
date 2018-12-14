using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface IListWorkflowsRequestStep1 : IFinalCommandStep<IWorkflowListResponse> {
        /// <summary>
        /// Filter the workflows by the given BPMN process id.
        /// This is the static id of the process in the
        /// BPMN XML (i.e. "&#60;bpmn:process id='my-workflow'&#62;").
        /// </summary>
        /// <param name="bpmnProcessId">the BPMN process id of the workflows</param>
        /// <returns> the builder for this request</returns>
        IListWorkflowsRequestStep2 BpmnProcessId(string bpmnProcessId);
    }

    public interface IListWorkflowsRequestStep2 : IFinalCommandStep<IWorkflowListResponse>
    {
    }
}