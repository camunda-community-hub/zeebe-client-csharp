using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zeebe.Client.Api.Builder
{
    public interface ICredentialsProvider
    {
        // Adds credentials to the request headers.
        void ApplyCredentials(AuthInterceptorContext context, Metadata metadata);
    }
}
