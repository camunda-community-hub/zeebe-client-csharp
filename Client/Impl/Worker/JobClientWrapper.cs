using Zeebe.Client.Api.Commands;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Impl.Worker
{
    internal class JobClientWrapper : IJobClient
    {
        public static JobClientWrapper Wrap(IJobClient client)
        {
            return new JobClientWrapper(client);
        }

        public bool ClientWasUsed { get; private set; }

        private IJobClient Client { get; }

        private JobClientWrapper(IJobClient client)
        {
            Client = client;
            ClientWasUsed = false;
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(long jobKey)
        {
            ClientWasUsed = true;
            return Client.NewCompleteJobCommand(jobKey);
        }

        public IFailJobCommandStep1 NewFailCommand(long jobKey)
        {
            ClientWasUsed = true;
            return Client.NewFailCommand(jobKey);
        }

        public IThrowErrorCommandStep1 NewThrowErrorCommand(long jobKey)
        {
            ClientWasUsed = true;
            return Client.NewThrowErrorCommand(jobKey);
        }

        public void Reset()
        {
            ClientWasUsed = false;
        }

        public ICompleteJobCommandStep1 NewCompleteJobCommand(IJob activatedJob)
        {
            ClientWasUsed = true;
            return Client.NewCompleteJobCommand(activatedJob);
        }
    }
}