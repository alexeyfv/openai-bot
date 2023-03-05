using System.Text.Json;
using OpenAiBot.Models;
using RestSharp;

namespace OpenAiBot.Handlers;

public class OpenAiHandler : IHandler<OpenAiRequest, OpenAiResponse>
{
    private readonly ILogger<Worker> logger;
    private readonly OpenAiInfo openAiInfo;
    private readonly RestClient client;

    public OpenAiHandler(
        ILogger<Worker> logger,
        OpenAiInfo openAiInfo,
        RestClient restClient)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.openAiInfo = openAiInfo ?? throw new ArgumentNullException(nameof(openAiInfo));
        this.client = restClient ?? throw new ArgumentNullException(nameof(restClient));
    }

    public Task<OpenAiResponse> HandleAsync(OpenAiRequest r)
    {
        var request = new RestRequest("/chat/completions", Method.Post);
        request.AddHeader("Authorization", $"Bearer {openAiInfo.Token}");
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

        var response = client.Execute(request);
        var result = ProcessResponse(response);
        return Task.FromResult(result);
    }

    private OpenAiResponse ProcessResponse(RestResponse resp)
    {
        if (resp == null) logger.LogError($"OpenAI API request failed'");
        else if (resp.IsSuccessful && resp.Content != null)
        {
            var data = JsonSerializer.Deserialize<Response>(resp.Content);
            if (data != null)
            {
                var answer = data.choices.FirstOrDefault()?.message.content ?? "Empty";
                var tokens = data.usage.total_tokens;
                return new OpenAiResponse(answer.Trim(), tokens);
            }
            logger.LogError($"Deserializing Error: {resp.Content}");
        }
        else logger.LogError($"OpenAI API request failed {resp.Content}");
        return new OpenAiResponse("Error occurred :( We are working to fix this", 0);
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
