using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Helpers
{
    public class WebUtils : IDisposable
    {
        private readonly HttpClient _httpClient;

        public WebUtils(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateState(IEnumerable<DownloadLink> downloadLinks)
        {
            //var httpClientHandler = new HttpClientHandler
            //{
            //};
            foreach (DownloadLink dl in downloadLinks)
            {
                var response = await _httpClient.GetAsync(dl.Url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                dl.Status = response.StatusCode.ToString();
            }
        }

        public async Task Download(string url)
        {
            var response = await _httpClient.GetAsync(url/*, HttpCompletionOption.ResponseHeadersRead*/).ConfigureAwait(false);

#if DEBUG
            foreach (var item in response.Headers)
            {
                System.Diagnostics.Debug.WriteLine($"Key: {item.Key}");
                foreach (var @value in item.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"Value: {value}");
                }
            }
#endif
            await ExtraFile(await response.Content.ReadAsStreamAsync()).ConfigureAwait(false);
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

                    string output = Path.Combine(FileUtils.Dictionary, entry.FullName);

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
