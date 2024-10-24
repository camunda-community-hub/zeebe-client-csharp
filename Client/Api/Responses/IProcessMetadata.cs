namespace Zeebe.Client.Api.Responses;

public interface IProcessMetadata
{
    /// <returns>the BPMN process id of the process.</returns>
    string BpmnProcessId { get; }

    /// <returns>the version of the deployed process.</returns>
    int Version { get; }

    /// <returns>the key of the deployed process.</returns>
    long ProcessDefinitionKey { get; }

    /// <returns>the name of the deployment resource which contains the process.</returns>
    string ResourceName { get; }
}