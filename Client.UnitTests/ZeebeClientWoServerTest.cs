using System;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class ZeebeClientWoServerTest
    {
        [Test]
        public void ShouldThrowExceptionAfterDispose()
        {
            // given
            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress("http://localhost")
                .UsePlainText()
                .Build();

            // when
            zeebeClient.Dispose();

            // then
            var catchedExceptions = Assert.ThrowsAsync<ObjectDisposedException>(
                () => zeebeClient.TopologyRequest().Send());

            Assert.NotNull(catchedExceptions);
            Assert.IsTrue(catchedExceptions.Message.Contains("ZeebeClient was already disposed."));
        }

        [Test]
        public void ShouldNotThrowExceptionWhenDisposingMultipleTimes()
        {
            // given
            var zeebeClient = ZeebeClient.Builder()
                .UseGatewayAddress("http://localhost")
                .UsePlainText()
                .Build();

            // when
            zeebeClient.Dispose();

            // then
            Assert.DoesNotThrow(() => zeebeClient.Dispose());
        }

        [Test]
        public void ShouldCalculateNextGreaterWaitTime()
        {
            // given
            var defaultWaitTimeProvider = ZeebeClient.DefaultWaitTimeProvider;

            // when
            var firstSpan = defaultWaitTimeProvider.Invoke(1);
            var secondSpan = defaultWaitTimeProvider.Invoke(2);

            // then
            Assert.Greater(secondSpan, firstSpan);
        }

        [Test]
        public void ShouldReturnMaxIfReachingMaxWaitTime()
        {
            // given
            var defaultWaitTimeProvider = ZeebeClient.DefaultWaitTimeProvider;

            // when
            var maxTime = defaultWaitTimeProvider.Invoke(100);

            // then
            Assert.AreEqual(TimeSpan.FromSeconds(ZeebeClient.MaxWaitTimeInSeconds), maxTime);
        }
    }
}