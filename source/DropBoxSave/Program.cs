using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Dropbox.Api;
using OAuthProtocol;

namespace ConsoleApplication
{
    class Program
    {

        private const string consumerKey = "zh5r86s4m0dx5do";
        private const string consumerSecret = "xzpn3mnutndzyqs";

        private static OAuthToken GetRequestToken()
        {
            var uri = new Uri("https://api.dropbox.com/1/oauth/request_token");

            // Generate a signature
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            string signature = oAuth.GenerateSignature(uri, consumerKey, consumerSecret,
                String.Empty, String.Empty, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1,
                out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            StringBuilder requestUri = new StringBuilder(uri.ToString());
            requestUri.AppendFormat("?oauth_consumer_key={0}&", consumerKey);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest)WebRequest.Create(new Uri(requestUri.ToString()));
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();

            var queryString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var parts = queryString.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        private static OAuthToken GetAccessToken(OAuthToken oauthToken)
        {
            var uri = "https://api.dropbox.com/1/oauth/access_token";

            OAuthBase oAuth = new OAuthBase();

            var nonce = oAuth.GenerateNonce();           
            var timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            var signature = oAuth.GenerateSignature(new Uri(uri), consumerKey, consumerSecret,
                oauthToken.Token, oauthToken.Secret, "GET", timeStamp, nonce, 
                OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            var requestUri = new StringBuilder(uri);
            requestUri.AppendFormat("?oauth_consumer_key={0}&", consumerKey);
            requestUri.AppendFormat("oauth_token={0}&", oauthToken.Token);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest) WebRequest.Create(requestUri.ToString());
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var accessToken = reader.ReadToEnd();

            var parts = accessToken.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        static void Main(string[] args)
        {
            //// Step 1/3: Get request token
            //OAuthToken oauthToken = GetRequestToken();

            //// Step 2/3: Authorize application
            //var queryString = String.Format("oauth_token={0}", oauthToken.Token);
            //var authorizeUrl = "https://www.dropbox.com/1/oauth/authorize?" + queryString;
            //Process.Start(authorizeUrl);
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();

            //// Step 3/3: Get access token
            //OAuthToken accessToken = GetAccessToken(oauthToken);



            //or use already known tokens:
            OAuthToken accessToken = new OAuthToken("q12cbpu34360x7s", "bmnb4a6ugpujs74");



            Console.WriteLine(String.Format("Your access token: {0}", accessToken.Token));
            Console.WriteLine(String.Format("Your access secret: {0}", accessToken.Secret));


            var api = new DropboxApi(consumerKey, consumerSecret, accessToken);
            
            
            var fileUp = api.UploadFile("sandbox", "a2.srt", @"D:\Download\a.srt");
            Console.WriteLine(string.Format("{0} uploaded.", fileUp.Path));


            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            var publicFolder = api.GetFiles("sandbox", "");
            foreach (var f in publicFolder.Contents)
            {
                Console.WriteLine(f.Path);
            }


            //var x = api.GetFiles("sandbox", "a.srt");
            //Console.WriteLine(x.Bytes);
            //Console.WriteLine();
            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();

            var file = api.DownloadFile("sandbox", "a.srt");
            var fn = System.IO.Path.GetTempFileName() + ".txt";
            file.Save(fn);
            Process.Start(fn);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}
