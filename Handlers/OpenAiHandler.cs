using System.Text.Json;
using OpenAiBot.Models;
using RestSharp;

namespace OpenAiBot.Handlers;

public class OpenAiHandler : IHandler<OpenAiRequest, OpenAiResponse>
{
    private readonly ILogger<Worker> logger;
    private readonly RestClient client;

    public OpenAiHandler(
        ILogger<Worker> logger,
        RestClient restClient)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.client = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public Task<OpenAiResponse> HandleAsync(OpenAiRequest r)
    {
        var token = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
            throw new InvalidOperationException("OPENAI_API_KEY doesn't exist");

        var request = new RestRequest("/chat/completions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {token}");
        request.AddHeader("Content-Type", "application/json");

        var messages = r.Messages.Select(m => new
        {
            role = m.Role switch
            {
                Role.Assistant => "assistant",
                Role.User => "user",
                _ => throw new NotImplementedException()
            },
            content = m.Text
        });

        var body = new { model = "gpt-3.5-turbo", messages = messages };
        request.AddParameter("application/json", body, ParameterType.RequestBody);

        var resp = client.Execute(request);

        OpenAiResponse answer;

        if (resp == null)
        {
            logger.LogDebug($"OpenAI API request failed'");
            answer = new OpenAiResponse("Response is null");
        }
        else if (resp.IsSuccessful && resp.Content != null)
        {
            var response = JsonSerializer.Deserialize<Response>(resp.Content);
            answer = new OpenAiResponse(response?.choices.FirstOrDefault()?.message.content ?? "Empty");
        }
        else
        {
            logger.LogDebug($"OpenAI API request failed {resp.Content}");
            answer = new OpenAiResponse(resp.ErrorException?.Message ?? "Empty error message");
        }

        return Task.FromResult(answer);
    }

    private class Response
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
