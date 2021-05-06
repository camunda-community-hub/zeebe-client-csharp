namespace Zeebe.Client.Impl.Cloud
{
    public class Credentials
    {
        public string Accesstoken { get; set; }
        public string Tokentype { get; set; }
        public string Refreshtoken { get; set; }
        public string Expiry { get; set; }
    }
}