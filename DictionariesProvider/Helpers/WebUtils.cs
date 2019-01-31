using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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


        public void Dispose()
        {
            _httpClient?.Dispose();
        }

    }
}
