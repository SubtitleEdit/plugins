using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using OAuthProtocol;

namespace Dropbox.Api
{
    public class DropboxApi
    {
        private readonly OAuthToken _accessToken;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        public DropboxApi(string consumerKey, string consumerSecret, OAuthToken accessToken)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
        }

        private string GetResponse(Uri uri)
        {
            var oauth = new OAuth();
            var requestUri = oauth.SignRequest(uri, _consumerKey, _consumerSecret, _accessToken);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        //public Account GetAccountInfo()
        //{
        //    var uri = new Uri(new Uri(DropboxRestApi.BaseUri), "account/info");
        //    var json = GetResponse(uri);
        //    return ParseJson<Account>(json);
        //}

        public DropboxFile GetFiles(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxRestApi.BaseUri), String.Format("metadata/{0}/{1}", root, path));
            var json = GetResponse(uri);

            Hashtable o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            DropboxFile f = new DropboxFile();
            f.Size = o["size"].ToString();
            List<DropboxFile> list = new List<DropboxFile>();
            foreach (Hashtable ht in (o["contents"] as System.Collections.ArrayList))
            {
                DropboxFile file = new DropboxFile();
                if (Convert.ToBoolean(ht["is_dir"]) == false)
                {
                    file.Path = ht["path"].ToString().TrimStart('/').Trim();
                    file.IsDirectory = Convert.ToBoolean(ht["is_dir"]);
                    file.Bytes = Convert.ToInt64(ht["bytes"]);
                    file.Size = ht["size"].ToString().Trim();
                    file.Modified = DateTime.Parse(ht["modified"].ToString());
                    list.Add(file);
                }
            }
            f.Contents = list;

            return f;
        }

        //public FileSystemInfo CreateFolder(string root, string path)
        //{
        //    var uri = new Uri(new Uri(DropboxRestApi.BaseUri),
        //        String.Format("fileops/create_folder?root={0}&path={1}",
        //        root, UpperCaseUrlEncode(path)));
        //    var json = GetResponse(uri);
        //    return ParseJson<FileSystemInfo>(json);
        //}

        public DropboxFile DownloadFile(string root, string path)
        {
            var uri = new Uri(new Uri(DropboxRestApi.ApiContentServer), String.Format("files?root={0}&path={1}", root, UpperCaseUrlEncode(path)));
            var oauth = new OAuth();
            var requestUri = oauth.SignRequest(uri, _consumerKey, _consumerSecret, _accessToken);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Get;
            var response = request.GetResponse();

            var metadata = response.Headers["x-dropbox-metadata"];
            //var file = ParseJson<FileSystemInfo>(metadata);
            Hashtable o = (Hashtable)Nikse.Json.JSON.JsonDecode(metadata);
            DropboxFile file = new DropboxFile();
            file.Bytes = Convert.ToInt64(o["bytes"]);
            file.Path = o["path"].ToString().TrimStart('/').Trim();
            file.IsDirectory = Convert.ToBoolean(o["is_dir"]);

            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);
                } while (bytesRead > 0);

                file.Data = memoryStream.ToArray();
            }

            return file;
        }

        public DropboxFile UploadFile(string root, string path, byte[] buffer)
        {
            var uri = new Uri(new Uri(DropboxRestApi.ApiContentServer), String.Format("files_put/{0}/{1}", root, UpperCaseUrlEncode(path)));
            var oauth = new OAuth();
            var requestUri = oauth.SignRequest(uri, _consumerKey, _consumerSecret, _accessToken, "PUT");

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Http.Put;
            request.KeepAlive = true;

            request.ContentLength = buffer.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();

            //return ParseJson<FileSystemInfo>(json);

            Hashtable o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            DropboxFile fsi = new DropboxFile();
            fsi.Bytes = Convert.ToInt64(o["bytes"]);
            fsi.Path = o["path"].ToString().TrimStart('/');
            fsi.IsDirectory = Convert.ToBoolean(o["is_dir"]);
            fsi.Size = o["size"].ToString().Trim();
            return fsi;
        }

        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }

            var values = new Dictionary<string, string>()
            {
                { "+", "%20" },
                { "(", "%28" },
                { ")", "%29" }
            };

            var data = new StringBuilder(new string(temp));
            foreach (string character in values.Keys)
            {
                data.Replace(character, values[character]);
            }
            return data.ToString();
        }
    }
}
