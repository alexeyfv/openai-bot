using System.Text.Json;
using ChatGptBot.Models;
using RestSharp;

namespace ChatGptBot.Handlers;

public class ChatGptHandler : IHandler<ChatGptQuestion, ChatGptAnswer>
{
    private readonly ILogger<Worker> logger;
    private readonly RestClient client;

    public ChatGptHandler(
        ILogger<Worker> logger,
        RestClient restClient)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.client = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public Task<ChatGptAnswer> HandleAsync(ChatGptQuestion r)
    {
        var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
            throw new InvalidOperationException("OPENAI_API_KEY doesn't exist");

        var request = new RestRequest("/chat/completions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        var body = new { model = "gpt-3.5-turbo", messages = new[] { new { role = "user", content = r.Question } } };
        request.AddParameter("application/json", body, ParameterType.RequestBody);

        var resp = client.Execute(request);

        ChatGptAnswer answer;

        if (resp == null)
        {
            answer = new ChatGptAnswer("Response is null");
        }
        else if (resp.IsSuccessful && resp.Content != null)
        {
            logger.LogDebug($"Response is successful. Content: '{resp.ToString()}'");
            var response = JsonSerializer.Deserialize<ChatGptResponse>(resp.Content);
            answer = new ChatGptAnswer(response?.choices.FirstOrDefault()?.message.content ?? "Empty");
        }
        else
        {
            logger.LogDebug($"Error {resp.ErrorMessage}");
            answer = new ChatGptAnswer(resp.ErrorException?.Message ?? "Empty error message");
        }

        return Task.FromResult(answer);
    }

    private class ChatGptResponse
    {
        public string id { get; init; } = string.Empty;
        public string @object { get; init; } = string.Empty;
        public int created { get; init; }
        public string model { get; init; } = string.Empty;
        public Usage usage { get; init; } = new();
        public Choice[] choices { get; set; } = new Choice[] { };
    }

    private class Usage
    {
        public int prompt_tokens { get; init; }
        public int completion_tokens { get; init; }
        public int total_tokens { get; init; }
    }

    private class Choice
    {
        public Message message { get; init; } = new();
        public string finish_reason { get; init; } = string.Empty;
        public int index { get; init; }
    }

    private class Message
    {
        public string role { get; init; } = string.Empty;
        public string content { get; init; } = string.Empty;
    }
}
