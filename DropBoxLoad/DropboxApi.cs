using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Dropbox.Api
{
    public class DropboxApi
    {
        public const string ApiVersion = "2";
        public const string BaseUri = "https://api.dropbox.com/" + ApiVersion + "/";
        public const string AuthorizeBaseUri = "https://www.dropbox.com/" + ApiVersion + "/";
        public const string ApiContentServer = "https://content.dropboxapi.com/" + ApiVersion + "/";
        public const string OAuthUrl = "https://www.dropbox.com/oauth2/";

        public OAuth2Token Accesstoken { get; private set; }
        private readonly string _appKey;
        private readonly string _appSecret;

        public DropboxApi(string appKey, string appSecret, OAuth2Token accessToken)
        {
            _appKey = appKey;
            _appSecret = appSecret;
            Accesstoken = accessToken;
        }

        public string GetPromptForCodeUrl()
        {
            var uri = new Uri(OAuthUrl + "authorize");
            StringBuilder requestUri = new StringBuilder(uri.ToString());
            requestUri.AppendFormat("?response_type={0}&", "code"); // code will supply an code to be copy pasted - for more info see https://www.dropbox.com/developers/documentation/http/documentation#oauth2-authorize
            requestUri.AppendFormat("client_id={0}", _appKey);
            return requestUri.ToString();
        }

        public OAuth2Token GetAccessToken(string code)
        {
            var uri = OAuthUrl + "token";
            var requestUri = new StringBuilder(uri);
            requestUri.AppendFormat("?code={0}&", code);
            requestUri.Append("grant_type=authorization_code");
            var webRequest = WebRequest.Create(requestUri.ToString());
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", "Basic " + GenerateBasicAuth());
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Post;
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            OAuth2Token oAuth2token = jss.Deserialize<OAuth2Token>(json);
            Accesstoken = oAuth2token;
            return oAuth2token;
        }

        private string GenerateBasicAuth()
        {
            return Base64Encode(_appKey + ":" + _appSecret);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private string GetResponse(Uri uri, string body)
        {
            var webRequest = WebRequest.Create(uri);
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", "Bearer " + Accesstoken.access_token);
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Post;
            byte[] buf = Encoding.UTF8.GetBytes(body);
            webRequest.ContentLength = buf.Length;
            webRequest.GetRequestStream().Write(buf, 0, buf.Length);
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        private Stream GetResponseStream(Uri uri, string arg)
        {
            var webRequest = WebRequest.Create(uri);
            webRequest.Headers.Add("Authorization", "Bearer " + Accesstoken.access_token);
            webRequest.Headers.Add("Dropbox-API-Arg", arg);
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Post;
            var response = request.GetResponse();
            return response.GetResponseStream();
        }

        public static string FormatBytesToDisplayFileSize(long fileSize)
        {
            if (fileSize <= 1024)
                return string.Format("{0} bytes", fileSize);
            if (fileSize <= 1024 * 1024)
                return string.Format("{0} kb", fileSize / 1024);
            if (fileSize <= 1024 * 1024 * 1024)
                return string.Format("{0:0.0} mb", (float)fileSize / (1024 * 1024));
            return string.Format("{0:0.0} gb", (float)fileSize / (1024 * 1024 * 1024));
        }

        public DropboxFile GetFiles(string path)
        {
            if (path.Length > 0)
            {
                path = "/" + path.TrimStart('/');
            }
            var uri = new Uri(new Uri(DropboxApi.BaseUri), "files/list_folder");
            var json = GetResponse(uri, "{\"path\":\"" + path + "\"}");
            var o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            var f = new DropboxFile();
            List<DropboxFile> list = new List<DropboxFile>();
            foreach (Hashtable ht in (o["entries"] as ArrayList))
            {
                var file = new DropboxFile();
                var tag = ht[".tag"].ToString().ToLowerInvariant();
                if (tag == "file" || tag == "folder")
                {
                    file.Path = ht["path_display"].ToString().TrimStart('/').Trim();
                    file.IsDirectory = tag == "folder";
                    if (tag == "file")
                    {
                        file.Bytes = Convert.ToInt64(ht["size"].ToString().Trim(), CultureInfo.InvariantCulture);
                        file.Description = FormatBytesToDisplayFileSize(file.Bytes);
                        file.Modified = DateTime.Parse(ht["client_modified"].ToString());
                    }
                    else
                    {
                        file.Description = "<Folder>";
                    }
                    list.Add(file);
                }
            }
            f.Contents = list;
            return f;
        }

        public DropboxFile DownloadFile(DropboxFile target)
        {
            var uri = new Uri(new Uri(ApiContentServer), "files/download");
            using (Stream responseStream = GetResponseStream(uri, "{\"path\":\"/" + target.Path.TrimStart('/') + "\"}"))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);
                } while (bytesRead > 0);

                target.Data = memoryStream.ToArray();
            }
            return target;
        }

        public DropboxFile UploadFile(string path, byte[] buffer)
        {
            path = "/" + path.TrimStart('/');
            var uri = new Uri(ApiContentServer + "files/upload");
            var webRequest = WebRequest.Create(uri);
            webRequest.ContentType = "application/octet-stream";
            webRequest.Headers.Add("Authorization", "Bearer " + Accesstoken.access_token);
            var arg = "{\"path\": \"" + path + "\",\"mode\": \"add\",\"autorename\": true,\"mute\": false}";
            webRequest.Headers.Add("Dropbox-API-Arg", arg);
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Post;
            request.KeepAlive = true;
            request.ContentLength = buffer.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();
            Hashtable o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            DropboxFile fsi = new DropboxFile
            {
                Bytes = Convert.ToInt64(o["size"]),
                Path = o["path_display"].ToString().TrimStart('/'),
                IsDirectory = false
            };
            fsi.Description = FormatBytesToDisplayFileSize(fsi.Bytes);
            return fsi;
        }

    }
}
