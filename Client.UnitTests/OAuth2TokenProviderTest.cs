using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Zeebe.Client.Impl.Builder;
using Zeebe.Client.Impl.Misc;

namespace Zeebe.Client
{
    [TestFixture]
    public class OAuth2TokenProviderTest
    {
        private HttpMessageHandlerStub MessageHandlerStub { get; set; }
        private OAuth2TokenProvider TokenProvider { get; set; }
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
            TokenProvider = new OAuth2TokenProviderBuilder()
                .UseAuthServer(_requestUri)
                .UseClientId(_clientId)
                .UseClientSecret(_clientSecret)
                .UseAudience(_audience)
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
                Assert.AreEqual(request.RequestUri, _requestUri);
                var content = await request.Content.ReadAsStringAsync(cancellationToken);

                var formFields = GetFormFields(content);
                Assert.AreEqual(formFields["client_id"], _clientId);
                Assert.AreEqual(formFields["client_secret"], _clientSecret);
                Assert.AreEqual(formFields["audience"], _audience);

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
            var credentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);
            Assert.AreEqual(credentials["AUDIENCE"].Token, token);
        }

        [Test]
        public async Task ShouldStoreMultipleCredentials()
        {
            // given
            await TokenProvider.GetAccessTokenForRequestAsync();
            var otherProvider = new OAuth2TokenProviderBuilder()
                .UseAuthServer(_requestUri)
                .UseClientId(_clientId = "OTHERID")
                .UseClientSecret(_clientSecret = "OTHERSECRET")
                .UseAudience(_audience = "OTHER_AUDIENCE")
                .Build();
            otherProvider.SetHttpMessageHandler(MessageHandlerStub);
            otherProvider.TokenStoragePath = TokenStoragePath;
            Token = "OTHER_TOKEN";

            // when
            var token = await otherProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("OTHER_TOKEN", token);
            var files = Directory.GetFiles(TokenStoragePath);
            Assert.AreEqual(1, files.Length);
            var tokenFile = files[0];
            var content = File.ReadAllText(tokenFile);
            var credentials = JsonConvert.DeserializeObject<Dictionary<string, AccessToken>>(content);

            Assert.AreEqual(credentials.Count, 2);
            Assert.AreEqual(token, credentials["OTHER_AUDIENCE"].Token);
            Assert.AreEqual("REQUESTED_TOKEN", credentials["AUDIENCE"].Token);
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
        public async Task ShouldNotUseCachedFileForOtherAudience()
        {
            // given
            Token = "STORED_TOKEN";
            await TokenProvider.GetAccessTokenForRequestAsync();
            var otherProvider = new OAuth2TokenProviderBuilder()
                .UseAuthServer(_requestUri)
                .UseClientId(_clientId = "OTHERID")
                .UseClientSecret(_clientSecret = "OTHERSECRET")
                .UseAudience(_audience = "OTHER_AUDIENCE")
                .Build();
            otherProvider.SetHttpMessageHandler(MessageHandlerStub);
            otherProvider.TokenStoragePath = TokenStoragePath;
            Token = "OTHER_TOKEN";

            // when
            var token = await otherProvider.GetAccessTokenForRequestAsync();

            // then
            Assert.AreEqual("OTHER_TOKEN", token);
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
        
        private static Dictionary<string, string> GetFormFields(string content)
        {
            var formFields = new Dictionary<string, string>();
            var keyValuePairs = content.Split('&');
            foreach (var keyValuePair in keyValuePairs)
            {
                var pair = keyValuePair.Split('=');
                if (pair.Length == 2)
                {
                    var key = Uri.UnescapeDataString(pair[0]);
                    var value = Uri.UnescapeDataString(pair[1]);
                    formFields.Add(key, value);
                }
            }
            return formFields;
        }
    }
}