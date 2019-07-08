using System;
using NUnit.Framework;

namespace Zeebe.Client
{
    [TestFixture]
    public class ZeebeClientTest
    {
        [Test]
        public void ShouldThrowExceptionAfterDispose()
        {
            // given
            var zeebeClient = ZeebeClient.NewZeebeClient();

            // when
            zeebeClient.Dispose();

            // then
            var aggregateException = Assert.Throws<AggregateException>(
                () => zeebeClient.TopologyRequest().Send().Wait());

            Assert.AreEqual(1, aggregateException.InnerExceptions.Count);

            var  catchedExceptions = aggregateException.InnerExceptions[0];
            Assert.IsTrue(catchedExceptions.Message.Contains("ZeebeClient was already disposed."));
            Assert.IsInstanceOf(typeof(ObjectDisposedException), catchedExceptions);
        }

    }
}