using System;

namespace Zeebe.Client.Impl.Builder
{
    public class OauthCredentials
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
    }
}