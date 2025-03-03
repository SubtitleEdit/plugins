using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.PluginLogic.Services;

public class ReportService : IDisposable
{
    private readonly HttpClient _httpClient;

    public ReportService()
    {
        _httpClient = new HttpClient()
        {
#if DEBUG
            BaseAddress = new Uri("http://127.0.0.1:5213/api/report")
#else
            BaseAddress = new Uri("https://subtitleedit.ivandrofly.com/api/report")
#endif
        };
    }

    public async Task<bool> ReportAsync(Report report)
    {
        try
        {
            using var content = new StringContent(report.ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(string.Empty, content);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Represents a report containing a message, content, and associated metadata.
/// </summary>
public class Report
{
    public Report(string message, string content)
    {
        ThrowIfNullOrEmpty(message, nameof(message));
        ThrowIfNullOrEmpty(content, nameof(content));

        Plugin = "HI2UC";

        Message = message;
        Content = content;
        Hash = ComputeHash(content);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the provided string is null or empty.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="name">The name of the argument being validated, used in the exception message.</param>
    /// <exception cref="ArgumentException">Thrown if the provided string is null or empty.</exception>
    private void ThrowIfNullOrEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"'{name}' cannot be null or empty", name);
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash for a given string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="fileName">The input string for which the hash will be computed.</param>
    /// <returns>The hash of the given input string as a lowercase hexadecimal string.</returns>
    private string ComputeHash(string fileName)
    {
        using var sha256 = SHA256.Create();
        // Convert the fileName to a byte array
        var fileNameBytes = Encoding.UTF8.GetBytes(fileName);

        // Compute the hash
        var hashBytes = sha256.ComputeHash(fileNameBytes);

        // Convert the hash bytes to a hexadecimal string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Represents the message associated with the report.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Subtitile content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Represents the computed SHA-256 hash of the report's content.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Represents the identifier of the plugin associated with the report.
    /// </summary>
    public string Plugin { get; }

    public override string ToString()
    {
        // to json string
        return $"{{\"message\": \"{Message}\", \"content\": \"{JsonUtils.EscapeJsonString(Content)}\", \"hash\": \"{Hash}\", \"plugin\": \"{Plugin}\"}}";
    }
}