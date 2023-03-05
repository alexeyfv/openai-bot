using System.Collections.Concurrent;
using OpenAiBot;
using OpenAiBot.DataAccess;
using OpenAiBot.Handlers;
using OpenAiBot.Models;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Types;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>()
            .AddSingleton<IContextFactory, ContextFactory>()
            .AddSingleton<IRepository, Repository>()
            .AddSingleton<IHandler<Update>, TelegramHandler>()
            .AddSingleton<IHandler<ChatMessage, string>, MessageHandler>()
            .AddSingleton<IHandler<ChatCommand, string>, CommandHandler>()
            .AddSingleton<ConcurrentDictionary<CacheKey, ChatMessage[]>>()
            .AddSingleton<IHandler<OpenAiRequest, OpenAiResponse>, OpenAiHandler>()
            .AddSingleton<BotInfo>(s => new BotInfo(Environment.GetEnvironmentVariable("BOT_NAME") ??
                throw new InvalidOperationException("BOT_NAME doesn't exist")))
            .AddSingleton<ConnectionInfo>(s => new ConnectionInfo(Environment.GetEnvironmentVariable("DBPATH") ??
                throw new InvalidOperationException("DBPATH doesn't exist")))
            .AddSingleton<OpenAiInfo>(s => new OpenAiInfo(Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                throw new InvalidOperationException("OPENAI_API_KEY doesn't exist")))
            .AddSingleton<ITelegramBotClient>(s => new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ??
                throw new InvalidOperationException("TELEGRAM_TOKEN doesn't exist")))
            .AddSingleton<RestClient>(s => new RestClient("https://api.openai.com/v1"))
            ;
    })
    .Build();

host.Run();
