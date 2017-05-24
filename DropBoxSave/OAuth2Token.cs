namespace Dropbox.Api
{
    public class OAuth2Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string uid { get; set; }
        public string account_id { get; set; }
    }
}
