using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Services;

/// <summary>
/// A service for sending analytics data to a remote API endpoint.
/// </summary>
/// <remarks>
/// The AnalyticsService is responsible for collecting and transmitting
/// analytics information such as operating system version and last active timestamp
/// as a JSON payload.
/// </remarks>
public class AnalyticsService : IDisposable
{
    private readonly HttpClient _httpClient;

    public AnalyticsService()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://subtitleedit.ivandrofly.com/api/analytics")
        };
    }

    /// <summary>
    /// Sends the provided analytics data asynchronously to the specified API endpoint.
    /// </summary>
    /// <param name="data">The data object containing analytics information to be sent.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendAsync(Data data)
    {
        try
        {
            using (var jsonContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json"))
            {
                _ = await _httpClient.PostAsync("/api/analytics", jsonContent)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    public void Dispose() => _httpClient?.Dispose();
}

public class Data
{
    /// <summary>
    /// Gets or sets the operating system version of the device or system sending analytics data.
    /// </summary>
    public string OsVersion { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last recorded activity.
    /// </summary>
    public DateTimeOffset LastActive { get; set; }

    public override string ToString()
    {
        // convert properties to json string
        return $"{{\"osVersion\": \"{JsonUtils.EscapeJsonString(OsVersion)}\", \"lastActive\": \"{LastActive}\"}}";
    }
}