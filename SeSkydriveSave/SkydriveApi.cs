using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SeSkydriveSave
{
    /// <summary>
    /// Read more here: http://msdn.microsoft.com/en-us/library/live/hh826543.aspx#rest
    /// </summary>
    public class SkydriveApi
    {
        private const string ClientId = "CLIENT_ID";
        private const string ClientSecret = "CLIENT_SECRET";
        private string ClientWebsite = "http://www.nikse.dk".Replace(" ", "%20").Replace(":", "%3A").Replace("/", "%2F");
        public string AccessToken { get; set; }

        public string Scope { get; set; }

        public string LoginUrl
        {
            get
            {
                return string.Format("https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=token&redirect_uri={2}", ClientId, Scope, ClientWebsite);
            }
        }

        public SkydriveApi(string scope)
        {
            Scope = scope; // "wl.skydrive" (read only) or "wl.skydrive_update" (read/write)
        }

        private string GetResponse(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            try
            {
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                return reader.ReadToEnd();
            }
            catch (Exception excetion)
            {
                System.Windows.Forms.MessageBox.Show(excetion.Message);
            }
            return null;
        }

        private byte[] GetResponseBytes(Uri uri, long size)
        {
            int blockSize = 1024;
            byte[] buffer = new byte[blockSize];
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            try
            {
                var response = request.GetResponse();
                Stream reader = response.GetResponseStream();
                MemoryStream ms = new MemoryStream();
                int totalBytesread = 0;
                int bytesRead = reader.Read(buffer, totalBytesread, blockSize);
                while (bytesRead > 0)
                {
                    totalBytesread += bytesRead;
                    bytesRead = reader.Read(buffer, 0, blockSize);
                    ms.Write(buffer, 0, bytesRead);
                }
                return ms.ToArray();
            }
            catch (Exception excetion)
            {
                System.Windows.Forms.MessageBox.Show(excetion.Message);
            }
            return null;
        }

        public List<SkydriveContent> GetFiles(string path)
        {
            var uri = new Uri(string.Format("https://apis.live.net/v5.0/{0}/files?access_token={1}", path, AccessToken));
            var json = GetResponse(uri);

            Hashtable o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            List<SkydriveContent> list = new List<SkydriveContent>();
            foreach (Hashtable ht in (o["data"] as System.Collections.ArrayList))
            {
                SkydriveContent file = new SkydriveContent();
                if (ht.ContainsKey("id"))
                {
                    file.Id = ht["id"].ToString();
                    file.Name = ht["name"].ToString();
                    file.Size = Convert.ToInt64(ht["size"].ToString());
                    file.UpdatedTime = Convert.ToDateTime(ht["updated_time"].ToString());
                    file.ParentId = ht["parent_id"].ToString();
                    file.Type = ht["type"].ToString();
                    list.Add(file);
                }
            }
            return list;
        }

        public byte[] DownloadFile(SkydriveContent skydriveContent)
        {
            var uri = new Uri(string.Format("https://apis.live.net/v5.0/{0}/content?access_token={1}", skydriveContent.Id, AccessToken));
            var json = GetResponse(uri);
            return GetResponseBytes(uri, skydriveContent.Size);
        }

        public void UploadFile(string folder, string fileName, string content)
        {
            var uri = new Uri(string.Format("https://apis.live.net/v5.0/{0}/files/{1}?access_token={2}", folder, fileName, AccessToken));
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Put;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                request.KeepAlive = true;
                request.ContentLength = buffer.Length;
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                System.Diagnostics.Debug.WriteLine(reader.ReadToEnd());
            }
            catch (Exception excetion)
            {
                System.Windows.Forms.MessageBox.Show(excetion.Message);
            }
        }
    }
}