using System;

namespace Zeebe.Client.Impl.Commands
{
    public static class TimeSpanExtensions
    {
        public static DateTime FromUtcNow(this TimeSpan timeSpan)
        {
            return DateTime.UtcNow + timeSpan;
        }
    }
}