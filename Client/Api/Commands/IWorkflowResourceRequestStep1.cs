using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands
{
    public interface IWorkflowResourceRequestStep1
    {

        /**
         * Set the BPMN process id of the workflow to create an instance of. This is the static id of the
         * process in the BPMN XML (i.e. "&#60;bpmn:process id='my-workflow'&#62;").
         *
         * @param bpmnProcessId the BPMN process id of the workflow
         * @return the builder for this command
         */
        IWorkflowResourceRequestStep2 BpmnProcessId(string bpmnProcessId);

        /**
         * Set the key of the workflow to create an instance of. The key is assigned by the broker while
         * deploying the workflow. It can be picked from the deployment or workflow event.
         *
         * @param workflowKey the key of the workflow
         * @return the builder for this command
         */
        IWorkflowResourceRequestStep3 WorkflowKey(long workflowKey);
    }


    public interface IWorkflowResourceRequestStep2
    {
        /**
         * Set the version of the workflow to create an instance of. The version is assigned by the
         * broker while deploying the workflow. It can be picked from the deployment or workflow event.
         *
         * @param version the version of the workflow
         * @return the builder for this command
         */
        IWorkflowResourceRequestStep3 Version(int version);

        /**
         * Use the latest version of the workflow to create an instance of.
         *
         * <p>If the latest version was deployed few moments before then it can happen that the new
         * instance is created of the previous version.
         *
         * @return the builder for this command
         */
        IWorkflowResourceRequestStep3 LatestVersion();
    }

    public interface IWorkflowResourceRequestStep3 : IFinalCommandStep<IWorkflowResourceResponse> {
    // the place for new optional parameters
    }
}