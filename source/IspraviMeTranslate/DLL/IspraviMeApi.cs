using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            return "https://ispravi.me";
        }

        internal IspraviResult CheckGrammer(string text, StringBuilder log)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string url = "https://ispravi.me/api/ispravi.pl";
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("textarea", text),
                new KeyValuePair<string, string>("context", "on"),
                new KeyValuePair<string, string>("app", _key)
            };
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };

            log.AppendLine("Calling " + GetName() + " with: " + new FormUrlEncodedContent(nvc).ReadAsStringAsync().Result);


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

                //  example of error result
//{
//    "response" : {
//      "errors" : 4,
//      "error" : [
//         {
//            "class" : "moderate",
//            "length" : 6,
//            "position" : [
//               6
//            ],
//            "suspicious" : "živoat",
//            "occurrences" : 1,
//            "suggestions" : [
//               "života",
//               "živost",
//               "život",
//               "živcat"
//            ]
//    },
//         {
//            "position" : [
//               23
//            ],
//            "length" : 6,
//            "class" : "moderate",
//            "suggestions" : [
//               "buduće"
//            ],
//            "occurrences" : 1,
//            "suspicious" : "buduce"
//         },
//         {
//            "class" : "minor",
//            "position" : [
//               0
//            ],
//            "length" : 5,
//            "suggestions" : [
//               "Cìlog",
//               "eLog",
//               "Velog",
//               "Clog",
//               "Celeg",
//               "Celom",
//               "Celon",
//               "Celox",
//               "Belog",
//               "Telog"
//            ],
//            "occurrences" : 1,
//            "suspicious" : "Celog"
//         },
//         {
//            "class" : "minor",
//            "length" : 7,
//            "position" : [
//               13
//            ],
//            "occurrences" : 1,
//            "suggestions" : [
//               "vjerujem",
//               "vezujem"
//            ],
//            "suspicious" : "verujem"
//         }
//      ]
//   },
//   "request" : {
//      "newuser" : true,
//      "commonerrors" : false,
//      "from" : "undef",
//      "session" : "ceada31a0b1f886bd9fc82fdaeb81fd1",
//      "contenttype" : "text/plain; charset=UTF-8",
//      "remoteip" : "62.44.134.100",
//      "contextual" : true,
//      "key" : "F2F09D38-2AC3-11E8-8531-4AB7E1FE0083",
//      "fixpunctuation" : false,
//      "textlength" : 36
//   },
//   "status" : {
//      "code" : 200,
//      "time" : 2.422875,
//      "href" : null,
//      "text" : "OK"
//   }
//}
                string s = res.Content.ReadAsStringAsync().Result;

                log.AppendLine("Success result from " + GetName() + ": " + s);

                Type serializationTargetType = typeof(IspraviResult);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(serializationTargetType);
                IspraviResult jsonDeserialized = (IspraviResult)jsonSerializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(s)));

                // sort by start position
                try
                {
                    if (jsonDeserialized != null && jsonDeserialized.response != null &&
                        jsonDeserialized.response.error != null)
                        jsonDeserialized.response.error = jsonDeserialized.response.error.OrderBy(p => p.position.FirstOrDefault()).ToList();
                }
                catch
                {
                    // ignore
                }

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
