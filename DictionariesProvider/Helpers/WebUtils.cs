using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Helpers
{
    public class WebUtils : IDisposable
    {
        private readonly HttpClient _httpClient;

        public WebUtils(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // don't hang there for long
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public Task UpdateStateAsync(IEnumerable<DownloadLink> downloadLinks)
        {
            //await Task.Yield();

            /*
             Task<IEnumerable<string>> DownLoadAllUrls(string[] urls)
            {
                return Task.WhenAll(from url in urls select DownloadHtmlAsync(url));
            }
            */

            // also talks about the new c# 8.0 featuers "async in foreach"
            // https://stackoverflow.com/questions/5061761/is-it-possible-to-await-yield-return-dosomethingasync

            // pararell async run task (https://www.youtube.com/watch?v=2moh18sh5p4)

            //Parallel.ForEach(downloadLinks, link => { });

            return Task.WhenAll(downloadLinks.Select(dl =>
            {
                return Task.Run(async () =>
               {
                   try
                   {
                       HttpResponseMessage response = await _httpClient.GetAsync(dl.Url).ConfigureAwait(false);
                       dl.Status = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted;
                   }
                   catch (Exception ex)
                   {
                       Debug.Write(ex.Message);
                   }
               });
            }));

            //foreach (DownloadLink dl in downloadLinks)
            //{
            //    try
            //    {
            //        var response = await _httpClient.GetAsync(dl.Url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            //        Debug.WriteLine($"ThreadID: {Thread.CurrentThread.ManagedThreadId} - Response: {response.StatusCode} - {dl.Url.OriginalString}");
            //        dl.Status = response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted;
            //    }
            //    catch
            //    {
            //        Debug.WriteLine("Exception...");
            //        dl.Status = false;
            //    }
            //}
        }

        public async Task Download(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url/*, HttpCompletionOption.ResponseHeadersRead*/).ConfigureAwait(false);
#if DEBUG
                foreach (var item in response.Headers)
                {
                    Debug.WriteLine($"Key: {item.Key}");
                    foreach (var @value in item.Value)
                    {
                        Debug.WriteLine($"Value: {value}");
                    }
                }
#endif
                await ExtraFile(await response.Content.ReadAsStreamAsync()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // report to debug and ignore
                Debug.WriteLine($"Message: {ex.Message}");
            }
        }


        private async Task ExtraFile(Stream dicFile)
        {
            using (var zipArchive = new ZipArchive(dicFile, ZipArchiveMode.Read))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    if ((entry.FullName.EndsWith(".aff", StringComparison.OrdinalIgnoreCase) ||
                        entry.FullName.EndsWith(".dic", StringComparison.OrdinalIgnoreCase)) == false)
                    {
                        continue;
                    }

                    string output = Path.Combine(FileUtils.Dictionaries, entry.FullName);

                    try
                    {
                        if (File.Exists(output))
                        {
                            File.Delete(output);
                        }
                    }
                    catch
                    {
                    }

                    using (var entryString = entry.Open())
                    using (var outFile = new FileStream(output, FileMode.Create))
                    {
                        await entryString.CopyToAsync(outFile).ConfigureAwait(false);
                    }
                }
            }

        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

    }
}
