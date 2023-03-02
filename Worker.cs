using ChatGptBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChatGptBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly ITelegramBotClient botClient;
    private readonly IHandler<Update> handler;

    public Worker(
        ILogger<Worker> logger,
        ITelegramBotClient botClient,
        IHandler<Update> handler)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {        
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        botClient.StartReceiving(
            async (_, u, _) => await handler.HandleAsync(u), 
            (_, e, _) => logger.LogError(e, e.Message), 
            receiverOptions, ct);

        logger.LogDebug("Telegram Bot message handler started");

        return Task.CompletedTask;
    }
}
