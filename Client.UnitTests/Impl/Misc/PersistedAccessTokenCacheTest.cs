//
//     Copyright (c) 2021 camunda services GmbH (info@camunda.com)
//
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Zeebe.Client.Impl.Misc;

[TestFixture]
public class PersistedAccessTokenCacheTest
{
    [SetUp]
    public void Init()
    {
        tempPath = Path.GetTempPath() + ".zeebe/";
        _ = Directory.CreateDirectory(tempPath);
    }

    [TearDown]
    public void CleanUp()
    {
        Directory.Delete(tempPath, true);
    }

    private string tempPath;

    [Test]
    public async Task ShouldGetToken()
    {
        // given
        var accessTokenCache = new PersistedAccessTokenCache(
            Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
            () =>
                Task.FromResult(new AccessToken("token", DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds())));

        // when
        var token = await accessTokenCache.Get("test");

        // then
        Assert.AreEqual("token", token);
    }

    [Test]
    public async Task ShouldCacheToken()
    {
        // given
        var fetchCounter = 0;
        var accessTokenCache = new PersistedAccessTokenCache(
            Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds())));

    // when
        _ = await accessTokenCache.Get("test");
        var token = await accessTokenCache.Get("test");

        // then
        Assert.AreEqual("token-0", token);
        Assert.AreEqual(1, fetchCounter);
    }

    [Test]
    public async Task ShouldCacheTokenForDifferentAudience()
    {
        // given
        var fetchCounter = 0;
        var accessTokenCache = new PersistedAccessTokenCache(
            Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds())));

        // when
        var firstToken = await accessTokenCache.Get("first");
        var secondToken = await accessTokenCache.Get("second");

        // then
        Assert.AreEqual("token-0", firstToken);
        Assert.AreEqual("token-1", secondToken);
        Assert.AreEqual(2, fetchCounter);
    }

    [Test]
    public async Task ShouldResolveNewTokenAfterExpiry()
    {
        // given
        var fetchCounter = 0;
        var accessTokenCache = new PersistedAccessTokenCache(
            Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())));

    // when
        _ = await accessTokenCache.Get("test");
        var token = await accessTokenCache.Get("test");

        // then
        Assert.AreEqual("token-1", token);
        Assert.AreEqual(2, fetchCounter);
    }

    [Test]
    public async Task ShouldReflectTokenOnDiskAfterExpiry()
    {
        // given
        var audience = "test";
        var fetchCounter = 0;
        var path = Path.Combine(tempPath, TestContext.CurrentContext.Test.Name);
        var accessTokenCache = new PersistedAccessTokenCache(path,
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())));
        var firstToken = await accessTokenCache.Get(audience);

        var credentials = await File.ReadAllTextAsync(Directory.GetFiles(path)[0]);
        Assert.That(credentials, Does.Contain(firstToken));
        Assert.That(credentials, Does.Contain(audience));

        // when
        var secondToken = await accessTokenCache.Get(audience);

        // then
        Assert.AreNotEqual(secondToken, firstToken);
        Assert.AreEqual("token-1", secondToken);
        Assert.AreEqual(2, fetchCounter);

        credentials = await File.ReadAllTextAsync(Directory.GetFiles(path)[0]);
        Assert.That(credentials, Does.Contain(secondToken));
        Assert.That(credentials, Does.Contain(audience));
    }

    [Test]
    public async Task ShouldPersistTokenToDisk()
    {
        // given
        var audience = "test";
        var fetchCounter = 0;
        var path = Path.Combine(tempPath, TestContext.CurrentContext.Test.Name);
        var accessTokenCache = new PersistedAccessTokenCache(path,
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())));

        // when
        var token = await accessTokenCache.Get(audience);

        // then
        var fileNames = Directory.GetFiles(path);
        Assert.AreEqual(1, fileNames.Length);
        var content = await File.ReadAllTextAsync(fileNames[0]);
        Assert.That(content, Does.Contain(token));
        Assert.That(content, Does.Contain(audience));
    }

    [Test]
    public async Task ShouldPersistMultipleTokenToDisk()
    {
        // given
        var fetchCounter = 0;
        var path = Path.Combine(tempPath, TestContext.CurrentContext.Test.Name);
        var accessTokenCache = new PersistedAccessTokenCache(path,
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())));

        // when
        var firstToken = await accessTokenCache.Get("first");
        var secondToken = await accessTokenCache.Get("second");

        // then
        Assert.AreEqual("token-0", firstToken);
        Assert.AreEqual("token-1", secondToken);
        Assert.AreEqual(2, fetchCounter);

        var fileNames = Directory.GetFiles(path);
        Assert.AreEqual(1, fileNames.Length);
        var content = await File.ReadAllTextAsync(fileNames[0]);
        Assert.That(content, Does.Contain(firstToken));
        Assert.That(content, Does.Contain("first"));

        Assert.That(content, Does.Contain(secondToken));
        Assert.That(content, Does.Contain("second"));
    }

    [Test]
    public async Task ShouldFetchNewTokenWhenPersistTokenGotLost()
    {
        // given
        var audience = "test";
        var fetchCounter = 0;
        var path = Path.Combine(tempPath, TestContext.CurrentContext.Test.Name);
        var accessTokenCache = new PersistedAccessTokenCache(path,
            () => Task.FromResult(new AccessToken("token-" + fetchCounter++,
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds())));
        _ = await accessTokenCache.Get(audience);
        File.Delete(Directory.GetFiles(path)[0]);

        // when
        var token = await accessTokenCache.Get(audience);

        // then
        Assert.AreEqual("token-1", token);
        Assert.AreEqual(2, fetchCounter);
        var fileNames = Directory.GetFiles(path);
        Assert.AreEqual(1, fileNames.Length);
        var content = await File.ReadAllTextAsync(fileNames[0]);
        Assert.That(content, Does.Contain(token));
        Assert.That(content, Does.Contain(audience));
    }
}