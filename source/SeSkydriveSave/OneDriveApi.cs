using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace OneDriveSave
{
    /// <summary>
    /// Read more here: https://dev.onedrive.com/
    /// </summary>
    public class OneDriveApi
    {
        private string _clientId;
        private string _clientWebsite;
        private const string Root = "https://graph.microsoft.com/v1.0";
        public string Scope { get; set; }
        public OAuth2Token OAuth2Token { get; set; }

        /// <summary>
        /// onedrive.appfolder or onedrive.readwrite
        /// </summary>
        /// <param name="scope">Permission(s). Seperated by space if more. Something like "files.readwrite"</param>
        public OneDriveApi(string scope, string clientId, string clientWebSite)
        {
            Scope = scope;
            _clientId = clientId;
            _clientWebsite = clientWebSite.Replace(":", "%3A").Replace("/", "%2F");
        }

        public string LoginUrl
        {
            get
            {
                return string.Format("https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={0}&scope={1}&response_type=code&grant_type=authorization_code&redirect_uri={2}", _clientId, Scope, _clientWebsite);
            }
        }

        public void InitTokens(string code)
        {
            var requestUri = new StringBuilder();
            requestUri.AppendFormat("client_id={0}&", _clientId);
            requestUri.AppendFormat("redirect_uri={0}&", _clientWebsite);
            // requestUri.AppendFormat("client_secret={0}&", ClientSecret);
            requestUri.AppendFormat("code={0}&", code);
            requestUri.Append("grant_type=authorization_code");
            var webRequest = WebRequest.Create("https://login.microsoftonline.com/common/oauth2/v2.0/token?");
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Post;
            byte[] buf = Encoding.UTF8.GetBytes(requestUri.ToString());
            webRequest.ContentLength = buf.Length;
            webRequest.GetRequestStream().Write(buf, 0, buf.Length);
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var json = reader.ReadToEnd();
            var jss = new JavaScriptSerializer();
            OAuth2Token = jss.Deserialize<OAuth2Token>(json);
        }

        private string GetResponse(Uri uri)
        {
            var webRequest = WebRequest.Create(uri);
            //            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", "Bearer " + OAuth2Token.access_token);
            var request = (HttpWebRequest)webRequest;
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

        public List<OneDriveContent> GetFiles(OneDriveContent odc, out string pathId)
        {
            var pathUri = new Uri(Root + "/drive/root");
            if (odc != null)
            {
                pathUri = new Uri(Root + "/drive/items/" + odc.Id);
            }
            var pathJson = GetResponse(pathUri);
            var oPath = (Hashtable)Nikse.Json.JSON.JsonDecode(pathJson);
            pathId = oPath["id"].ToString();

            var uri = new Uri(Root + "/drive/root/children");
            if (odc != null)
            {
                uri = new Uri(Root + "/drive/items/" + odc.Id + "/children");
            }
            var json = GetResponse(uri);

            var o = (Hashtable)Nikse.Json.JSON.JsonDecode(json);
            var list = new List<OneDriveContent>();
            foreach (Hashtable ht in (o["value"] as ArrayList))
            {
                OneDriveContent file = new OneDriveContent();
                if (ht.ContainsKey("id"))
                {
                    file.Id = ht["id"].ToString();
                    file.Name = ht["name"].ToString();
                    file.Size = Convert.ToInt64(ht["size"].ToString());
                    file.UpdatedTime = Convert.ToDateTime(ht["lastModifiedDateTime"].ToString());

                    if (ht["parentReference"] != null)
                    {
                        var parentElements = ht["parentReference"] as Hashtable;
                        if (parentElements != null && parentElements["id"] != null)
                        {
                            file.ParentId = parentElements["id"].ToString();
                        }
                        if (parentElements != null && parentElements["path"] != null)
                        {
                            file.Path = parentElements["path"].ToString();
                        }
                    }

                    if (ht["folder"] != null)
                    {
                        file.Type = "folder";
                    }
                    else
                    {
                        file.Type = "file";
                    }
                    list.Add(file);
                }
            }
            return list;
        }

        public byte[] DownloadFile(OneDriveContent oneDriveContent)
        {
            var uri = new Uri(Root + "/drive/items/" + oneDriveContent.Id + "/content");

            var webRequest = WebRequest.Create(uri);
            webRequest.Headers.Add("Authorization", "Bearer " + OAuth2Token.access_token);
            var request = (HttpWebRequest)webRequest;
            request.Method = WebRequestMethods.Http.Get;
            try
            {
                var response = request.GetResponse();
                using (var ms = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception excetion)
            {
                System.Windows.Forms.MessageBox.Show(excetion.Message);
            }
            return null;
        }

        public void UploadFile(string path, string pathId, string fileName, string content)
        {
            // HTTP request:
            // PUT /drive/items/{parent-id}:/{filename}:/content
            // PUT /drive/root:/{parent-path}/{ filename}:/content
            // PUT /drive/items/{parent-id}/children/{filename}/content
            //PATH: var webRequest = WebRequest.Create(new Uri(Root + path + "/" + HttpUtility.UrlEncode(fileName.Trim('/')) + ":/content"));
            var webRequest = WebRequest.Create(new Uri(Root + "/drive/items/" + HttpUtility.UrlEncode(pathId) + "/children/" + HttpUtility.UrlEncode(fileName.Trim('/')) + "/content")); // https://dev.onedrive.com/items/upload_put.htm
            webRequest.Headers.Add("Authorization", "Bearer " + OAuth2Token.access_token);
            webRequest.ContentType = "application/octet-stream";
            var request = (HttpWebRequest)webRequest;
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