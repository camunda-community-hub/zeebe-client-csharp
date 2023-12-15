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

using System.Threading.Tasks;

namespace Zeebe.Client.Impl.Misc;

/// <summary>
/// <see cref="AccessToken"/> cache, which allows to cache tokens per audience.
/// </summary>
public interface IAccessTokenCache
{
    /// <summary>
    /// A valid token, which is related to the given audience.
    /// </summary>
    ///
    /// The token may be cached, or new resolved if there was no token corresponding
    /// to the audience stored yet, or if the token has been expired.
    /// <param name="audience">The audience which corresponds to the token.</param>
    /// <returns>A valid token for the audience.</returns>
    Task<string> Get(string audience);

    /// <summary>
    /// An asynchronous access token resolver, which is used to fill the cache, when
    /// token can't be found.
    /// </summary>
    ///
    /// Resolver should be given to the cache, on creation time.
    /// <returns>The new access token.</returns>
    public delegate Task<AccessToken> AccessTokenResolverAsync();
}