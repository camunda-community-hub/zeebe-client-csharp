using System;
using System.Threading;

namespace Zeebe.Client.Impl.Worker
{
    public class JobWorkerSignal
    {
        private readonly EventWaitHandle handleSignal = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle pollSignal = new EventWaitHandle(false, EventResetMode.AutoReset);

        internal JobWorkerSignal()
        {
        }

        public bool AwaitJobHandling(TimeSpan timeSpan)
        {
            return pollSignal.WaitOne(timeSpan);
        }

        public bool AwaitNewJobPolled(TimeSpan timeSpan)
        {
            return handleSignal.WaitOne(timeSpan);
        }

        public void SignalJobHandled()
        {
            pollSignal.Set();
        }

        public void SignalJobPolled()
        {
            handleSignal.Set();
        }
    }
}