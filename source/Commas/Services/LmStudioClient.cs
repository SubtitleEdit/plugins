using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic;

public class LmStudioClient : IDisposable
{
    private readonly string _prompt;
    private readonly HttpClient _httpClient;

    public LmStudioClient(string url, string prompt)
    {
        _prompt = prompt;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(url)
        };
    }

    public async Task<string> SendAsync(string text)
    {
        /*
          curl http://0.0.0.0:1234/v1/chat/completions \
          -H "Content-Type: application/json" \
          -d '{
            "model": "deepseek-r1-distill-qwen-7b",
            "messages": [
              { "role": "system", "content": "Always answer in rhymes." },
              { "role": "user", "content": "Introduce yourself." }
            ],
            "temperature": 0.7,
            "max_tokens": -1,
            "stream": true
          }'
         */

        var chatCompletionRequest = new ChatCompletionRequest(false, new[]
        {
            new Message("user", $"{_prompt}:\n\n{text}"),
            new Message("system", "You are a helpful assistant.")
        }, 0.7);

        var json = JsonConvert.SerializeObject(chatCompletionRequest);
        using var chatRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(chatRequest, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

        return chatResponse.Choices[0].Message.Content;
    }

    public void Dispose() => _httpClient.Dispose();

    public class ChatCompletionRequest(bool stream, Message[] messages, double temperature)
    {
        [JsonProperty("stream")]
        public bool Stream { get; } = stream;

        [JsonProperty("messages")]
        public Message[] Messages { get; } = messages;

        [JsonProperty("temperature")]
        public double Temperature { get; } = temperature;
    }

    public class Message(string role, string content)
    {
        [JsonProperty("role")]
        public string Role { get; } = role;

        [JsonProperty("content")]
        public string Content { get; } = content;
    }

    public class ChatResponse
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("choices")]
        public Choices[] Choices { get; set; }
    }

    public class Choices
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}