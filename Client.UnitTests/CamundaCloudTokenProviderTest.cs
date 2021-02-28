using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NUnit.Framework;
using Zeebe.Client.Impl.Builder;

namespace Zeebe.Client
{
    [TestFixture]
    public class CamundaCloudTokenProviderTest
    {
        private HttpMessageHandlerStub MessageHandlerStub { get; set; }
        private CamundaCloudTokenProvider TokenProvider { get; set; }
        private string TokenStoragePath { get; set; }
        private static long ExpiresIn { get; set; }
        private static string Token { get; set; }

        [SetUp]
        public void Init()
        {
            TokenProvider = new CamundaCloudTokenProviderBuilder()
                .UseAuthServer("https://local.de")
                .UseClientId("ID")
                .UseClientSecret("SECRET")
                .UseAudience("AUDIENCE")
                .Build();

            MessageHandlerStub = new HttpMessageHandlerStub();
            TokenProvider.SetHttpMessageHandler(MessageHandlerStub);
            TokenStoragePath = Path.GetTempPath() + ".zeebe/";
            TokenProvider.TokenStoragePath = TokenStoragePath;
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
                Assert.AreEqual(request.RequestUri, "https://local.de");
                var content = await request.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(content);
                Assert.AreEqual((string)jsonObject["client_id"], "ID");
                Assert.AreEqual((string)jsonObject["client_secret"], "SECRET");
                Assert.AreEqual((string)jsonObject["audience"], "AUDIENCE");

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
        public async Task ShouldStoreCredentials()
        {
            // given

            // when
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", token);
            var files = Directory.GetFiles(TokenStoragePath);
            Assert.AreEqual(1, files.Length);
            var tokenFile = files[0];
            var content = File.ReadAllText(tokenFile);
            var fileToken = JsonConvert.DeserializeObject<CamundaCloudTokenProvider.AccessToken>(content);
            Assert.AreEqual(token, fileToken.Token);
        }

        [Test]
        public async Task ShouldGetTokenFromInMemory()
        {
            // given
            await TokenProvider.GetAccessTokenForRequestAsync();
            var files = Directory.GetFiles(TokenStoragePath);
            var tokenFile = files[0];
            File.WriteAllText(tokenFile, "FILE_TOKEN");

            // when
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", token);
            Assert.AreEqual(1, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldExpireInOneSecond()
        {
            // given
            ExpiresIn = 1;
            var firstToken = await TokenProvider.GetAccessTokenForRequestAsync();
            var files = Directory.GetFiles(TokenStoragePath);
            var tokenFile = files[0];
            File.WriteAllText(tokenFile, "FILE_TOKEN");

            // when
            Token = "NEW_TOKEN";
            var secondToken = await TokenProvider.GetAccessTokenForRequestAsync();
            Thread.Sleep(1_000);
            var thirdToken = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", firstToken);
            Assert.AreEqual(secondToken, firstToken);
            Assert.AreEqual("NEW_TOKEN", thirdToken);
            Assert.AreEqual(2, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldRequestNewTokenWhenExpired()
        {
            // given
            ExpiresIn = 0;
            var firstToken = await TokenProvider.GetAccessTokenForRequestAsync();
            var files = Directory.GetFiles(TokenStoragePath);
            var tokenFile = files[0];
            File.WriteAllText(tokenFile, "FILE_TOKEN");

            // when
            Token = "SECOND_TOKEN";
            var secondToken = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", firstToken);
            Assert.AreNotEqual(secondToken, firstToken);
            Assert.AreEqual("SECOND_TOKEN", secondToken);
            Assert.AreEqual(2, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldUseCachedFile()
        {
            // given
            Token = "STORED_TOKEN";
            await TokenProvider.GetAccessTokenForRequestAsync();
            // re-init the TokenProvider
            Init();

            // when
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("STORED_TOKEN", token);
            Assert.AreEqual(0, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldRequestWhenCachedFileExpired()
        {
            // given
            ExpiresIn = 0;
            Token = "STORED_TOKEN";
            await TokenProvider.GetAccessTokenForRequestAsync();
            // re-init the TokenProvider
            Init();

            // when
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("REQUESTED_TOKEN", token);
            Assert.AreEqual(1, MessageHandlerStub.RequestCount);
        }

        [Test]
        public async Task ShouldUseCachedFileAndAfterwardsInMemory()
        {
            // given
            Token = "STORED_TOKEN";
            await TokenProvider.GetAccessTokenForRequestAsync();
            // re-init the TokenProvider
            Init();

            // when
            await TokenProvider.GetAccessTokenForRequestAsync();
            var files = Directory.GetFiles(TokenStoragePath);
            var tokenFile = files[0];
            File.WriteAllText(tokenFile, "FILE_TOKEN");
            var token = await TokenProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("STORED_TOKEN", token);
            Assert.AreEqual(0, MessageHandlerStub.RequestCount);
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