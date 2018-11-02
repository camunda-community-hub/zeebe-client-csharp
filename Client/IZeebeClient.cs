/*
 * Copyright © 2017 camunda services GmbH (info@camunda.com)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Zeebe.Client.Api.Clients;
using Zeebe.Client.Api.Commands;

namespace Zeebe.Client
{

    /** The client to communicate with a Zeebe broker/cluster. */
    public interface IZeebeClient : IDisposable
    {

        /**
         * A client to
         * <li>deploy a workflow
         * <li>create a workflow instance
         * <li>cancel a workflow instance
         * <li>update the payload of a workflow instance
         * <li>request a workflow resource
         * <li>request all deployed workflows
         *
         * @return a client with access to all workflow-related operations.
         */
        IWorkflowClient WorkflowClient();

        /**
         * A client to
         * <li>create a (standalone) job
         * <li>complete a job
         * <li>mark a job as failed
         * <li>update the retries of a job
         *
         * @return a client with access to all job-related operations.
         */
        IJobClient JobClient();

        /**
         * Request the current cluster topology. Can be used to inspect which brokers are available at
         * which endpoint and which broker is the leader of which partition.
         *
         * <pre>
         * List&#60;BrokerInfo&#62; brokers = zeebeClient
         *  .newTopologyRequest()
         *  .send()
         *  .join()
         *  .getBrokers();
         *
         *  SocketAddress address = broker.getSocketAddress();
         *
         *  List&#60;PartitionInfo&#62; partitions = broker.getPartitions();
         * </pre>
         *
         * @return the request where you must call {@code send()}
         */
        ITopologyRequestStep1 TopologyRequest();
    }
}
