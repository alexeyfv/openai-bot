using System.Collections.Concurrent;
using OpenAiBot.DataAccess;
using OpenAiBot.Models;

namespace OpenAiBot.Handlers;

public class MessageHandler : IHandler<ChatMessage, string>
{
    private readonly IHandler<OpenAiRequest, OpenAiResponse> openAiHandler;
    private readonly ConcurrentDictionary<CacheKey, ChatMessage[]> cache;
    private readonly IRepository db;

    public MessageHandler(
        IHandler<OpenAiRequest, OpenAiResponse> openAiHandler,
        ConcurrentDictionary<CacheKey, ChatMessage[]> cache,
        IRepository db
        )
    {
        this.openAiHandler = openAiHandler ?? throw new ArgumentNullException(nameof(openAiHandler));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<string> HandleAsync(ChatMessage r)
    {
        // Validate message
        if (!db.HasAccess(r.UserId)) return $"You have exceeded your limit 🙅🏻‍♂️";

        // Get cached conversation
        var key = new CacheKey(r.ChatId, r.UserId);
        var cached = cache.AddOrUpdate(key, f => new[] { r }, (f, a) => a.Append(r).ToArray());

        // Send request to OpenAI API
        var response = await openAiHandler.HandleAsync(new OpenAiRequest(cached));

        // Update user data
        db.UpsertUser(r.UserId, response.Tokens);

        // Append AI-assistant response and save to cache
        cache[key] = cached.Append(r with { Text = response.Answer, Role = Role.Assistant }).ToArray();

        return response.Answer;
    }

    private string RemoveCommand(string text)
    {
        // Check if the message starts with '/<command>@<id>'
        if (text.StartsWith('/'))
        {
            // Find the first space character in the message
            int spaceIndex = text.IndexOf(' ');

            // If there is no space character, the message does not contain any text after the command
            if (spaceIndex == -1) return text;

            // Get the substring that starts after the first space character
            return text.Substring(spaceIndex + 1);
        }

        return text;
    }
}
