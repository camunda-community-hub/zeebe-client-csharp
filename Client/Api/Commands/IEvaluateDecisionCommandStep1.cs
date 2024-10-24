//
//     Copyright (c) 2021 camunda services GmbH (info@camunda.com)
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

using Zeebe.Client.Api.Responses;

namespace Zeebe.Client.Api.Commands;

public interface IEvaluateDecisionCommandStep1
{
  /// <summary>
  /// Set the id of the decision to evaluate. This is the static id of the decision in the DMN XML
  /// (i.e. "&#60;decision id='my-decision'&#62;").
  /// </summary>
  ///
  /// <param name="decisionId">The DMN id of the decision.</param>
  /// <returns>the builder for this command.
  /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
  /// the command and send it to the broker.</returns>
  IEvaluateDecisionCommandStep2 DecisionId(string decisionId);

  /// <summary>
  /// Set the key of the decision to evaluate. The key is assigned by the broker while deploying the
  /// decision. It can be picked from the deployment.
  /// </summary>
  ///
  /// <param name="decisionKey">The key of the decision.</param>
  /// <returns>the builder for this command.
  /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
  /// the command and send it to the broker.</returns>
  IEvaluateDecisionCommandStep2 DecisionKey(long decisionKey);

  public interface IEvaluateDecisionCommandStep2 : IFinalCommandWithRetryStep<IEvaluateDecisionResponse>
  {
      /// <summary>
      /// Set the variables for the decision evaluation.
      /// </summary>
      ///
      /// <param name="variables">The variables JSON document as String.</param>
      /// <returns>the builder for this command.
      /// Call <see cref="IFinalCommandStep{T}.Send(System.TimeSpan?,System.Threading.CancellationToken)"/> to complete
      /// the command and send it to the broker.</returns>
      IEvaluateDecisionCommandStep2 Variables(string variables);
  }
}