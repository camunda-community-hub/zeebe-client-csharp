namespace Zeebe.Client.Api.Responses;

/// <summary>
/// Response for an create process instance command.
/// </summary>
public interface IProcessInstanceResponse
{
    /// <returns>
    /// Key of the process which this instance was created for.
    /// </returns>
    long ProcessDefinitionKey { get; }

    /// <returns>
    /// BPMN process id of the process which this instance was created for.
    /// </returns>
    string BpmnProcessId { get; }

    /// <returns>
    /// Version of the process which this instance was created for.
    /// </returns>
    int Version { get; }

    /// <returns>
    /// Unique key of the created process instance on the partition.
    /// </returns>
    long ProcessInstanceKey { get; }
}