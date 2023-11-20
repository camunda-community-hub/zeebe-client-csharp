using System;
using Newtonsoft.Json.Linq;

namespace Zeebe.Client.Impl.Misc;

public class AccessToken
{
    public string Token { get; set; }
    public long DueDate { get; set; }

    public AccessToken(string token, long dueDate)
    {
        Token = token;
        DueDate = dueDate;
    }

    public override string ToString()
    {
        return $"{nameof(Token)}: {Token}, {nameof(DueDate)}: {DueDate}";
    }

    public static AccessToken FromJson(string result)
    {
        var jsonResult = JObject.Parse(result);
        var accessToken = (string)jsonResult["access_token"];

        var expiresInMilliSeconds = (long)jsonResult["expires_in"] * 1_000L;
        var dueDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + expiresInMilliSeconds;
        var token = new AccessToken(accessToken, dueDate);
        return token;
    }
}