using System;
using System.IO;
using System.Net;
using System.Web;

namespace OAuthProtocol
{
    public class OAuth
    {
        private readonly OAuthBase _oAuthBase;

        public OAuth()
        {
            _oAuthBase = new OAuthBase();
        }

        public OAuthToken GetRequestToken(Uri baseUri, string consumerKey, string consumerSecret)
        {
            var uri = new Uri(baseUri, "oauth/request_token");

            uri = SignRequest(uri, consumerKey, consumerSecret);

            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();

            var queryString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var parts = queryString.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        public Uri GetAuthorizeUri(Uri baseUri, OAuthToken requestToken)
        {
            var queryString = String.Format("oauth_token={0}", requestToken.Token);
            var authorizeUri = String.Format("{0}{1}?{2}", baseUri, "oauth/authorize", queryString);
            return new Uri(authorizeUri);
        }

        public OAuthToken GetAccessToken(Uri baseUri, string consumerKey, string consumerSecret, OAuthToken requestToken)
        {
            var uri = new Uri(baseUri, "oauth/access_token");

            uri = SignRequest(uri, consumerKey, consumerSecret, requestToken);

            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var accessToken = reader.ReadToEnd();

            var parts = accessToken.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        public Uri SignRequest(Uri uri, string consumerKey, string consumerSecret, OAuthToken token, string httpMethod)
        {
            var nonce = _oAuthBase.GenerateNonce();
            var timestamp = _oAuthBase.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;

            string requestToken = token == null ? String.Empty : token.Token;
            string tokenSecret = token == null ? String.Empty : token.Secret;

            var signature = _oAuthBase.GenerateSignature(
                uri, consumerKey, consumerSecret,
                requestToken, tokenSecret, httpMethod, timestamp,
                nonce, OAuthBase.SignatureTypes.HMACSHA1,
                out normalizedUrl, out parameters);

            var requestUri = String.Format("{0}?{1}&oauth_signature={2}", 
                normalizedUrl, parameters, HttpUtility.UrlEncode(signature));

            return new Uri(requestUri);
        }

        public Uri SignRequest(Uri uri, string consumerKey, string consumerSecret, OAuthToken token = null)
        {
            return SignRequest(uri, consumerKey, consumerSecret, token, "GET");
        }
    }
}