using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Zeebe.Client.Impl.Builder
{
    [TestFixture]
    public class CamundaCloudTokenProviderTest
    {
        private HttpMessageHandlerStub MessageHandlerStub { get; set; }
        private CamundaCloudTokenProvider TokenProvider { get; set; }
        private string TokenStoragePath { get; set; }
        private static long ExpiresIn { get; set; }
        private static string Token { get; set; }

        private static string _requestUri;
        private static string _clientId;
        private static string _clientSecret;
        private static string _audience;

        [SetUp]
        public void Init()
        {
            _requestUri = "https://local.de";
            _clientId = "ID";
            _clientSecret = "SECRET";
            _audience = "AUDIENCE";
            TokenStoragePath = Path.GetTempPath() + ".zeebe/";
            TokenProvider = new CamundaCloudTokenProviderBuilder()
                .UseAuthServer(_requestUri)
                .UseClientId(_clientId)
                .UseClientSecret(_clientSecret)
                .UseAudience(_audience)
                .UsePath(TokenStoragePath)
                .Build();

            MessageHandlerStub = new HttpMessageHandlerStub();
            TokenProvider.SetHttpMessageHandler(MessageHandlerStub);
            ExpiresIn = 3600;
            Token = "REQUESTED_TOKEN";
        }

        [TearDown]
        public void CleanUp()
        {
            Directory.Delete(TokenStoragePath, true);
            TokenProvider.Dispose();
        }

        private class HttpMessageHandlerStub : HttpMessageHandler
        {
            public int RequestCount { get; set; }
            private bool _disposed = false;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                CheckDisposed();
                Assert.AreEqual(request.RequestUri, _requestUri);
                var content = await request.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                Assert.AreEqual((string)jsonObject["client_id"], _clientId);
                Assert.AreEqual((string)jsonObject["client_secret"], _clientSecret);
                Assert.AreEqual((string)jsonObject["audience"], _audience);

                RequestCount++;
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                    ""access_token"":""" + Token + @""",
                    ""token_type"":""bearer"",
                    ""expires_in"": " + ExpiresIn + @",
                    ""refresh_token"":""IwOGYzYTlmM2YxOTQ5MGE3YmNmMDFkNTVk"",
                    ""scope"":""create""}"),
                };

                return responseMessage;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _disposed = true;
            }

            private void CheckDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("HttpMessageHandlerStub");
                }
            }
        }

        [Test]
        public async Task ShouldRequestCredentials()
        {
            // given

            // when
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", token);
            Assert.AreEqual(1, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldNotThrowObjectDisposedExceptionWhenTokenExpires()
        {
            // given
            ExpiresIn = 0;
            await TokenProvider.GetAccessTokenForRequestAsync();

            // when
            Assert.DoesNotThrowAsync(async () => await TokenProvider.GetAccessTokenForRequestAsync());

            // then
            Assert.AreEqual(2, MessageHandlerStub.RequestCount);
        }
    }
}