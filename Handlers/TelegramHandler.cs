using OpenAiBot.DataAccess;
using OpenAiBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OpenAiBot.Handlers;

public class TelegramHandler : IHandler<Update>
{
    private readonly ILogger<Worker> logger;
    private readonly IHandler<ChatCommand, string> commandHandler;
    private readonly IHandler<ChatMessage, string> messageHandler;
    private readonly ITelegramBotClient bot;
    private readonly BotInfo botInfo;
    private readonly IRepository db;

    public TelegramHandler(
        ILogger<Worker> logger,
        IHandler<ChatCommand, string> commandHandler,
        IHandler<ChatMessage, string> messageHandler,
        ITelegramBotClient bot,
        BotInfo botInfo,
        IRepository db)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        this.bot = bot ?? throw new ArgumentNullException(nameof(bot));
        this.botInfo = botInfo ?? throw new ArgumentNullException(nameof(botInfo));
        this.db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task HandleAsync(Update r)
    {
        if (r.Message == null ||
            r.Message.From == null ||
            string.IsNullOrWhiteSpace(r.Message.Text))
        {
            logger.LogDebug($"Invalid message");
            return;
        }

        var msg = r.Message;
        var messageId = msg.MessageId;
        var chatId = msg.Chat.Id;
        var userId = msg.From.Id;
        var text = msg.Text;

        if (msg.Entities is not null &&
            msg.Entities.Length > 0 &&
            msg.Entities[0].Type == MessageEntityType.BotCommand)
        {
            // Commands
            var response = await commandHandler.HandleAsync(new ChatCommand(chatId, userId, text));
            if (string.IsNullOrWhiteSpace(response)) return;
            await bot.SendTextMessageAsync(chatId, response, replyToMessageId: messageId, parseMode: ParseMode.Html);
        }
        else if (msg.Chat.Type == ChatType.Private)
        {
            // One to one chat
            var response = await messageHandler.HandleAsync(new ChatMessage(chatId, userId, text));
            if (string.IsNullOrWhiteSpace(response)) return;
            await bot.SendTextMessageAsync(chatId, response, replyToMessageId: messageId);
        }
        else if(text.StartsWith($"@{botInfo.Name}"))
        {
            // Groups (only if bot was mentioned)
            var response = await messageHandler.HandleAsync(new ChatMessage(chatId, userId, text));
            if (string.IsNullOrWhiteSpace(response)) return;
            await bot.SendTextMessageAsync(chatId, response, replyToMessageId: messageId);
        }
    }
}
