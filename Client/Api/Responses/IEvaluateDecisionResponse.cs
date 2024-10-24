using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses;

/// <summary>
/// Response for evaluating a decision on the broker.
/// </summary>
public interface IEvaluateDecisionResponse
{
    /// <returns>
    /// the decision ID, as parsed during deployment; together with the versions forms a unique
    /// identifier for a specific decision.
    /// </returns>
    string DecisionId { get; }

    /// <returns>
    /// the assigned decision version.
    /// </returns>
    int DecisionVersion { get; }

    /// <returns>
    /// the assigned decision key, which acts as a unique identifier for this decision.
    /// </returns>
    long DecisionKey { get; }

    /// <returns>
    /// the name of the decision, as parsed during deployment.
    /// </returns>
    string DecisionName { get; }

    /// <returns>
    /// the ID of the decision requirements graph that this decision is part of, as parsed
    /// during deployment.
    /// </returns>
    string DecisionRequirementsId { get; }

    /// <returns>
    /// the assigned key of the decision requirements graph that this decision is part of.
    /// </returns>
    long DecisionRequirementsKey { get; }

    /// <returns>
    /// the output of the evaluated decision.
    /// </returns>
    string DecisionOutput { get; }

    /// <returns>
    /// a list of decisions that were evaluated within the requested decision evaluation.
    /// </returns>
    IList<IEvaluatedDecision> EvaluatedDecisions { get; }

    /// <returns>
    /// a string indicating the ID of the decision which failed during evaluation.
    /// </returns>
    string FailedDecisionId { get; }

    /// <returns>
    /// a message describing why the decision which was evaluated failed.
    /// </returns>
    string FailureMessage { get; }
}