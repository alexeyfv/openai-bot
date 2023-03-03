using ChatGptBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatGptBot.Handlers;

public class TelegramHandler : IHandler<Update>
{
    private readonly ILogger<Worker> logger;
    private readonly IHandler<ChatGptRequest, ChatGptResponse> chatGptHandler;
    private readonly ITelegramBotClient bot;
    private HashSet<long> ids = new()
    {
    };

    private HashSet<string> usernames = new()
    {
    };

    public TelegramHandler(
        ILogger<Worker> logger,
        IHandler<ChatGptRequest, ChatGptResponse> chatGptHandler,
        ITelegramBotClient bot)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.chatGptHandler = chatGptHandler ?? throw new ArgumentNullException(nameof(chatGptHandler));
        this.bot = bot ?? throw new ArgumentNullException(nameof(bot));
    }

    public async Task HandleAsync(Update r)
    {
        if (r.Message == null)
        {
            logger.LogDebug($"'{nameof(r.Message)}' is null");
            return;
        }

        var m = r.Message;

        if (m.From == null)
        {
            logger.LogDebug($"'{nameof(m.From)}' is null");
            return;
        }

        var id = m.Chat.Id;

        // Validate against allowed users
        if (!string.IsNullOrWhiteSpace(m.From.Username) &&
            !usernames.Contains(m.From.Username) &&
            !ids.Contains(id))
        {
            await bot.SendTextMessageAsync(id, $"@{m.From.Username}, you don't have access 🙅🏻‍♂️");
            return;
        }

        var list = new List<ChatMessage>();
        HandleRecursively(r.Message, list);

        if (list.Count == 0) return;

        var response = await chatGptHandler.HandleAsync(new ChatGptRequest(list));

        await bot.SendTextMessageAsync(id, response.Answer, replyToMessageId: m.MessageId);
    }

    private void HandleRecursively(Message m, List<ChatMessage> messages)
    {
        if (m.ReplyToMessage is not null) HandleRecursively(m.ReplyToMessage, messages);

        if (m.From == null)
        {
            logger.LogDebug($"'{nameof(m.From)}' is null");
            return;
        }

        if (string.IsNullOrWhiteSpace(m.Text))
        {
            logger.LogDebug($"Message text cannot be empty");
            return;
        }

        var role = m.From.IsBot ? Role.Assistant : Role.User;
        messages.Add(new ChatMessage(role, RemoveCommand(m.Text)));
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
