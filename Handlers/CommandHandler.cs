using System.Text;
using System.Collections.Concurrent;
using OpenAiBot.DataAccess;
using OpenAiBot.Models;

namespace OpenAiBot.Handlers;

public class CommandHandler : IHandler<ChatCommand, string>
{
    private readonly ILogger<Worker> logger;
    private readonly IHandler<ChatMessage, string> messageHandler;
    private readonly ConcurrentDictionary<CacheKey, ChatMessage[]> cache;
    private readonly IRepository db;

    public CommandHandler(
        ILogger<Worker> logger,
        IHandler<ChatMessage, string> messageHandler,
        ConcurrentDictionary<CacheKey, ChatMessage[]> cache,
        IRepository db
        )
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<string> HandleAsync(ChatCommand r)
    {
        var response = string.Empty;

        if (r.Text.StartsWith("/message", StringComparison.OrdinalIgnoreCase))
        {
            response = await messageHandler.HandleAsync(new ChatMessage(r.ChatId, r.UserId, r.Text));
        }
        if (r.Text.StartsWith("/jailbreak", StringComparison.OrdinalIgnoreCase))
        {
            response = $"Sorry. Not implemented yet. 🤷‍♂️";
        }
        else if (r.Text.StartsWith("/clear", StringComparison.OrdinalIgnoreCase))
        {
            var attempt = 0;
            var key = new CacheKey(r.ChatId, r.UserId);
            while (cache.ContainsKey(key) && !cache.TryRemove(key, out var _) && attempt < 3) attempt++;
            if (attempt < 3) response = $"Ok. 👌 I forgot everything we discussed.";
            else
            {
                logger.LogError($"Unable to clear message history for user id {r.UserId}");
                response = $"Unable to clear message history. 🤷‍♂️ Try again later.";
            }
        }
        else if (r.Text.StartsWith("/history", StringComparison.OrdinalIgnoreCase))
        {
            var key = new CacheKey(r.ChatId, r.UserId);
            if (cache.ContainsKey(key) && cache.TryGetValue(key, out var cached))
            {
                var sb = new StringBuilder();
                foreach (var m in cached)
                {
                    var role = m.Role switch
                    {
                        Role.Assistant => "<b>AI</b>",
                        Role.User => "<b>You</b>",
                        _ => throw new NotImplementedException()
                    };
                    sb.AppendLine($"{role}: {m.Text}");
                    sb.AppendLine();
                }
                response = sb.ToString();
            }
            else response = "Message history is empty.";
        }
        else if (r.Text.StartsWith("/id", StringComparison.OrdinalIgnoreCase))
        {
            response = $"Your id: {r.UserId}";
        }
        else if (r.Text.StartsWith("/remaining", StringComparison.OrdinalIgnoreCase))
        {
            var remaining = db.Remaining(r.UserId);
            if (remaining.Unlimited) response = $"You have unlimited access 😎";
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("<b>Remaining resources</b>");
                sb.AppendLine();
                sb.AppendLine($"Tokens: {remaining.Tokens}.");
                sb.AppendLine($"Requests: {remaining.Requests}.");
                response = sb.ToString();
            }
        }
        else if (r.Text.StartsWith("/unlimited", StringComparison.OrdinalIgnoreCase))
        {
            response = $"Ask @alexeyfv to get unlimited access";
        }

        return response;
    }
}
