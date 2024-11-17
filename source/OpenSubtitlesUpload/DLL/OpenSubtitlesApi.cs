using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace OpenSubtitles
{
    /// <summary>
    /// OpenSubtitles.org API
    /// Docs: http://trac.opensubtitles.org/projects/opensubtitles/wiki/XmlRpcIntro
    /// </summary>
    public class OpenSubtitlesApi
    {
        private const string XmlRpcUrl = "http://api.opensubtitles.org/xml-rpc";
        public string UserAgent { private set; get; }
        public string Token { private set; get; }
        public string LastStatus { private set; get; }

        public OpenSubtitlesApi(string userAgent)
        {
            UserAgent = userAgent;
        }

        internal string SendRequestAndGetResponse(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var webRequest = (HttpWebRequest)WebRequest.Create(XmlRpcUrl);
            webRequest.ContentType = "text/xml";
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 10000;
            webRequest.ContentLength = bytes.Length;
            using (Stream requeststream = webRequest.GetRequestStream())
            {
                requeststream.Write(bytes, 0, bytes.Length);
            }

            string response;
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            using (var sr = new StreamReader(webResponse.GetResponseStream()))
            {
                response = sr.ReadToEnd().Trim();
            }
            return response;
        }

        public bool Login(string name, string password, string language)
        {
            if (string.IsNullOrEmpty(language))
                language = "en";

            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
    <methodName>LogIn</methodName>
    <params>
    <param>
    <value><string></string></value>
    </param>
    <param>
    <value><string></string></value>
    </param>
    <param>
    <value><string></string></value>
    </param>
    <param>
    <value><string></string></value>
    </param>
    </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = name;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/string").InnerText = password;
            doc.DocumentElement.SelectSingleNode("params/param[3]/value/string").InnerText = language;
            doc.DocumentElement.SelectSingleNode("params/param[4]/value/string").InnerText = UserAgent;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);
            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            Token = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='token']/../value/string").InnerText;
            return LastStatus == "200 OK";
        }

        public bool LogOut()
        {
            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
  <methodName>LogOut</methodName>
  <params>
    <param>
      <value><string></string></value>
    </param>
  </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);
            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            return LastStatus == "200 OK";
        }

        public bool NoOperation()
        {
            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
  <methodName>NoOperation</methodName>
  <params>
    <param>
      <value><string></string></value>
    </param>
  </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);
            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            return LastStatus == "200 OK";
        }

        public bool TryUploadSubtitles(string subtitle, string subtitleFileName, string movieFileName, string movieFileNameFull, string language, string fps, Encoding encoding)
        {
            string movieByteSize = new FileInfo(movieFileNameFull).Length.ToString();
            string movieTimeMilliseconds = string.Empty;
            string movieFrames = string.Empty;
            string movieFps = string.Empty;
            string subtitleHash = CalculateMD5Hash(GetBytesWithChosenEncoding(subtitle, encoding));
            string movieHash = CalculateHash(movieFileNameFull);

            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
 <methodName>TryUploadSubtitles</methodName>
 <params>
  <param>
   <value><string>Token</string></value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>cd1</name>
      <value>
       <struct>
        <member>
         <name>subhash</name>
         <value><string>md5hash</string></value>
        </member>
        <member>
         <name>subfilename</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviehash</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviebytesize</name>
         <value><double></double></value>
        </member>
        <member>
         <name>moviefps</name>
         <value><double></double></value>
        </member>
        <member>
         <name>movietimems</name>
         <value><int></int></value>
        </member>
        <member>
         <name>movieframes</name>
         <value><int></int></value>
        </member>
        <member>
         <name>moviefilename</name>
         <value><string></string></value>
        </member>
       </struct>
      </value>
     </member>
    </struct>
   </value>
  </param>
 </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subhash']/../value/string").InnerText = subtitleHash;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subfilename']/../value/string").InnerText = subtitleFileName;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviehash']/../value/string").InnerText = movieHash;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviebytesize']/../value/double").InnerText = movieByteSize;
            if (!string.IsNullOrEmpty(fps))
                doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviefps']/../value/double").InnerText = fps;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviefilename']/../value/string").InnerText = movieFileName;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);
            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            if (LastStatus == "200 OK")
            {
                string alreadyInDb = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='alreadyindb']/../value/int").InnerText;
                return alreadyInDb == "0"; // 0 == not in db (upload can continue)
            }
            return false;
        }

        public bool UploadSubtitles(string subtitle, string subtitleFileName, string movieFileName, string movieFileNameFull, string language, string releaseName, string idMovieImdb, string comment, string hearingImpaired, string hd, string fps, Encoding encoding)
        {
            if (string.IsNullOrEmpty(hearingImpaired))
                hearingImpaired = "0";
            if (string.IsNullOrEmpty(hd))
                hd = "0";

            if (string.IsNullOrEmpty(language))
                language = "eng";
            byte[] buffer = GetBytesWithChosenEncoding(subtitle, encoding);
            string subtitleContent = System.Convert.ToBase64String(GZLib(buffer));
            string movieByteSize = new FileInfo(movieFileNameFull).Length.ToString();
            string movieTimeMilliseconds = string.Empty;
            string movieFrames = string.Empty;
            string movieFps = string.Empty;
            string subtitleHash = CalculateMD5Hash(buffer);
            string movieHash = CalculateHash(movieFileNameFull);
            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
 <methodName>UploadSubtitles</methodName>
 <params>
  <param>
   <value><string>Token</string></value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>baseinfo</name>
      <value>
       <struct>
        <member>
         <name>idmovieimdb</name>
         <value><string>0</string></value>
        </member>
        <member>
         <name>sublanguageid</name>
         <value><string></string></value>
        </member>
        <member>
         <name>hearingimpaired</name>
         <value><string></string></value>
        </member>
        <member>
         <name>highdefinition</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviereleasename</name>
         <value><string></string></value>
        </member>
        <member>
         <name>movieaka</name>
         <value><string/></value>
        </member>
        <member>
         <name>subauthorcomment</name>
         <value><string/></value>
        </member>
       </struct>
      </value>
     </member>
     <member>
      <name>cd1</name>
      <value>
       <struct>
        <member>
         <name>subhash</name>
         <value><string></string></value>
        </member>
        <member>
         <name>subfilename</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviehash</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviebytesize</name>
         <value><double></double></value>
        </member>
        <member>
         <name>moviefps</name>
         <value><double></double></value>
        </member>
        <member>
         <name>movietimems</name>
         <value><int></int></value>
        </member>
        <member>
         <name>movieframes</name>
         <value><int></int></value>
        </member>
        <member>
         <name>moviefilename</name>
         <value><string></string></value>
        </member>
        <member>
         <name>subcontent</name>
         <value><string>eNqMv ... gzipped and then base64-encoded subtitle file contents ... x7cPjA==</string></value>
        </member>
       </struct>
      </value>
     </member>
    </struct>
   </value>
  </param>
 </params>
</methodCall>
";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;

            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='idmovieimdb']/../value/string").InnerText = idMovieImdb;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='sublanguageid']/../value/string").InnerText = language;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='hearingimpaired']/../value/string").InnerText = hearingImpaired;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='highdefinition']/../value/string").InnerText = hd;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviereleasename']/../value/string").InnerText = releaseName;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subauthorcomment']/../value/string").InnerText = comment;

            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subhash']/../value/string").InnerText = subtitleHash;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subfilename']/../value/string").InnerText = subtitleFileName;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviehash']/../value/string").InnerText = movieHash;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviebytesize']/../value/double").InnerText = movieByteSize;
            if (!string.IsNullOrEmpty(fps))
                doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviefps']/../value/double").InnerText = fps;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='moviefilename']/../value/string").InnerText = movieFileName;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/struct/member/value/struct/member/name[text()='subcontent']/../value/string").InnerText = subtitleContent;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);
            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            return LastStatus == "200 OK";
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[src.Length]; // 4096
            int cnt;
            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] GZLib(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            {
                using (var mso = new MemoryStream())
                {
                    using (var gs = new ComponentAce.Compression.Libs.zlib.ZOutputStream(mso, ComponentAce.Compression.Libs.zlib.zlibConst.Z_DEFAULT_COMPRESSION))
                    {
                        CopyTo(msi, gs);
                    }
                    return mso.ToArray();
                }
            }
        }

        //public static byte[] GZip(string str)
        //{
        //    var bytes = Encoding.UTF8.GetBytes(str);

        //    using (var msi = new MemoryStream(bytes))
        //    {
        //        using (var mso = new MemoryStream())
        //        {
        //            using (var gs = new GZipStream(mso, CompressionMode.Compress))
        //            {
        //                CopyTo(msi, gs);
        //            }
        //            return mso.ToArray();
        //        }
        //    }
        //}

        //public static byte[] Remove10ByteHeaderAnd8ByteFooter(byte[] buffer)
        //{
        //    byte[] newBuffer = new byte[buffer.Length - (10 + 8)];
        //    Buffer.BlockCopy(buffer, 10, newBuffer, 0, newBuffer.Length);
        //    return newBuffer;
        //}

        public static string CalculateMD5Hash(byte[] buffer)
        {
            var md5 = MD5.Create();
            var sb = new StringBuilder();
            foreach (var b in md5.ComputeHash(buffer))
                sb.Append(b.ToString("x2").ToLower());
            return sb.ToString();
        }

        public static byte[] GetBytesWithChosenEncoding(string subtitle, Encoding encoding)
        {
            string tempFileName = Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFileName, subtitle, encoding);
            var bytes = System.IO.File.ReadAllBytes(tempFileName);
            try
            {
                System.IO.File.Delete(tempFileName);
            }
            catch { }
            return bytes;
        }

        public System.Collections.Generic.Dictionary<string, string> SearchMoviesOnIMDB(string query)
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            query = query.Trim();
            if (string.IsNullOrEmpty(query))
                return dic;

            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
 <methodName>SearchMoviesOnIMDB</methodName>
 <params>
  <param>
   <value><string>token</string></value>
  </param>
  <param>
   <value><string>search string</string></value>
  </param>
 </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/string").InnerText = query;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);

            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            if (LastStatus != "200 OK")
                return dic;

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("params/param/value/struct/member[2]/value/array/data/value"))
            {
                XmlNode id = node.SelectSingleNode("struct/member/name[text()='id']/../value/string");
                XmlNode title = node.SelectSingleNode("struct/member/name[text()='title']/../value/string");
                if (id != null && title != null)
                    dic.Add(id.InnerText, title.InnerText);
            }

            return dic;
        }

        public System.Collections.Generic.Dictionary<string, string> SearchSubtitles(string movieFileNameFull, string language)
        {
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            if (string.IsNullOrEmpty(movieFileNameFull))
                return dic;

            string movieByteSize = new FileInfo(movieFileNameFull).Length.ToString();
            string movieHash = CalculateHash(movieFileNameFull);

            string xml = "<?xml version=\"1.0\"?>" + Environment.NewLine +
@"<methodCall>
 <methodName>SearchSubtitles</methodName>
 <params>
  <param>
   <value><string>token</string></value>
  </param>
  <param>
   <value>
    <array>
     <data>
      <value>
       <struct>
        <member>
         <name>sublanguageid</name>
         <value><string>cze,eng,ger,slo</string>
         </value>
        </member>
        <member>
         <name>moviehash</name>
         <value><string></string></value>
        </member>
        <member>
         <name>moviebytesize</name>
         <value><double></double></value>
        </member>
       </struct>
      </value>
     </data>
    </array>
   </value>
  </param>
 </params>
</methodCall>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.DocumentElement.SelectSingleNode("params/param[1]/value/string").InnerText = Token;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/array/data/value/struct/member/name[text()='sublanguageid']/../value/string").InnerText = language;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/array/data/value/struct/member/name[text()='moviehash']/../value/string").InnerText = movieHash;
            doc.DocumentElement.SelectSingleNode("params/param[2]/value/array/data/value/struct/member/name[text()='moviebytesize']/../value/double").InnerText = movieByteSize;
            string response = SendRequestAndGetResponse(doc.OuterXml);
            doc.LoadXml(response);

            LastStatus = doc.DocumentElement.SelectSingleNode("params/param/value/struct/member/name[text()='status']/../value/string").InnerText;
            if (LastStatus != "200 OK")
                return dic;

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("params/param/value/struct/member[2]/value/array/data/value"))
            {
                XmlNode id = node.SelectSingleNode("struct/member/name[text()='SubDownloadLink']/../value/string");
                XmlNode title = node.SelectSingleNode("struct/member/name[text()='SubFileName']/../value/string");
                if (id != null && title != null)
                    dic.Add(id.InnerText, title.InnerText);
            }

            return dic;
        }

        #region Movie Hasher

        public static string CalculateHash(string videofileName)
        {
            return ToHexadecimal(ComputeMovieHash(videofileName));
        }

        private static byte[] ComputeMovieHash(string videofileName)
        {
            byte[] result;
            using (Stream input = File.OpenRead(videofileName))
            {
                result = ComputeMovieHash(input);
            }
            return result;
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long streamsize = input.Length;
            long lhash = streamsize;

            long i = 0;
            var buffer = new byte[sizeof(long)];
            const int c = 65536;
            while (i < c / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - c);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            var hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }

        #endregion Movie Hasher
    }
}