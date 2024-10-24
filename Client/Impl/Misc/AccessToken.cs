using System;
using Newtonsoft.Json.Linq;

namespace Zeebe.Client.Impl.Misc;

/// <summary>
/// AccessToken, which consist of an token and a dueDate (expiryDate).
/// </summary>
public class AccessToken(string token, long dueDate)
{
    public string Token { get; set; } = token;
    public long DueDate { get; set; } = dueDate;

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