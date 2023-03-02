using ChatGptBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatGptBot.Handlers;

public class TelegramHandler : IHandler<Update>
{
    private readonly ILogger<Worker> logger;
    private readonly IHandler<ChatGptQuestion, ChatGptAnswer> chatGptHandler;
    private readonly ITelegramBotClient telegramBotClient;

    public TelegramHandler(
        ILogger<Worker> logger,
        IHandler<ChatGptQuestion, ChatGptAnswer> chatGptHandler,
        ITelegramBotClient telegramBotClient)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.chatGptHandler = chatGptHandler ?? throw new ArgumentNullException(nameof(chatGptHandler));
        this.telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
    }

    public async Task HandleAsync(Update r)
    {
        // TODO: Add user validation

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
        if (string.IsNullOrWhiteSpace(m.Text))
        {
            logger.LogDebug($"'{nameof(m.Text)}' is null or whitespace");
            return;
        }

        var from = m.From;

        var id = from.Id.ToString();
        var username = from.Username;
        var firstName = from.FirstName;
        var lastName = from.LastName;
        var text = m.Text;

        logger.LogDebug($"Message '{text}' received from {firstName} {lastName} (id = '{id}', username = '{username}')");

        var response = await chatGptHandler.HandleAsync(new ChatGptQuestion(text));

        await telegramBotClient.SendTextMessageAsync(id, response.Answer);
    }
}
