namespace Dropbox.Api
{
    public class DropboxRestApi
    {
        public const string ApiVersion = "1";

        public const string BaseUri = "https://api.dropbox.com/" + ApiVersion + "/";

        public const string AuthorizeBaseUri = "https://www.dropbox.com/" + ApiVersion + "/";

        public const string ApiContentServer = "https://api-content.dropbox.com/" + ApiVersion + "/";
    }
}