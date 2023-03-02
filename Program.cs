using ChatGptBot;
using ChatGptBot.Handlers;
using ChatGptBot.Models;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Types;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<Worker>()
            .AddSingleton<IHandler<Update>, TelegramHandler>()
            .AddSingleton<IHandler<ChatGptQuestion, ChatGptAnswer>, ChatGptHandler>()
            .AddSingleton<ITelegramBotClient>(s => new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ??
                throw new InvalidOperationException("TELEGRAM_TOKEN doesn't exist")))
            .AddSingleton<RestClient>(s => new RestClient("https://api.openai.com/v1"))
            ;
    })
    .Build();

host.Run();