using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SubtitleEdit
{
    public class IspraviMeApi
    {
        private string _key;
        public IspraviMeApi(string key)
        {
            _key = key;
        }

        internal string GetName()
        {
            return "Ispravi.me";
        }

        internal string GetUrl()
        {
            throw new NotImplementedException();
        }

        internal IspraviResult CheckGrammer(string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string url = "https://ispravi.me/api/ispravi.pl";
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("textarea", text));
            nvc.Add(new KeyValuePair<string, string>("context", "on"));
            nvc.Add(new KeyValuePair<string, string>("app", _key));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
            var res = client.SendAsync(request).Result;
            if (res.IsSuccessStatusCode)
            {

                //example of ok result
                //{
                //   "request" : {
                //      "textlength" : 38,
                //      "fixpunctuation" : false,
                //      "commonerrors" : false,
                //      "newuser" : true,
                //      "contenttype" : "text/plain; charset=UTF-8",
                //      "contextual" : true,
                //      "session" : "d6ff5b6c43578be112c394dffba57c51",
                //      "key" : "EFC4E0A0-2A97-11E8-A23B-2EC0E1FE0083",
                //      "remoteip" : "62.44.134.100",
                //      "from" : "undef"
                //                    },
                //   "status" : {
                //      "text" : "OK",
                //      "time" : 2.39342,
                //      "code" : 200,
                //      "href" : null
                //   },
                //   "response" : null
                //}

                string s = res.Content.ReadAsStringAsync().Result;
                Type serializationTargetType = typeof(IspraviResult);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(serializationTargetType);
                IspraviResult jsonDeserialized = (IspraviResult)jsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(s)));
                return jsonDeserialized;
            }
            else
            {
                try
                {
                    log.AppendLine("Status code: " + res.StatusCode);
                    log.AppendLine("Error result from " + GetName() + ": " + res.Content.ReadAsStringAsync().Result);
                }
                catch (Exception ex)
                {
                    log.AppendLine(ex.Message + ": " + ex.StackTrace);
                }
            }
            log.AppendLine();
            return null;
        }
    }
}
