using System;
using System.Collections.Generic;

namespace Zeebe.Client.Api.Responses
{
    public interface ITopology
    {
        /** @return all (known) brokers of the cluster */
        IList<IBrokerInfo> Brokers { get; }
    }
}
