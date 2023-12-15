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
    private string tempPath;

    [SetUp]
    public void Init()
    {
        tempPath = Path.GetTempPath() + ".zeebe/";
        Directory.CreateDirectory(tempPath);
    }

    [TearDown]
    public void CleanUp()
    {
        Directory.Delete(tempPath, true);
    }

    [Test]
    public async Task ShouldGetToken()
    {
        // given
        var accessTokenCache = new PersistedAccessTokenCache(Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
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
        int fetchCounter = 0;
        var accessTokenCache = new PersistedAccessTokenCache(Path.Combine(tempPath, TestContext.CurrentContext.Test.Name),
            () =>
            {
                return Task.FromResult(new AccessToken("token-" + fetchCounter++, DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeMilliseconds()));
            });

        // when
        await accessTokenCache.Get("test");
        var token = await accessTokenCache.Get("test");

        // then
        Assert.AreEqual("token-0", token);
        Assert.AreEqual(1, fetchCounter);
    }
}