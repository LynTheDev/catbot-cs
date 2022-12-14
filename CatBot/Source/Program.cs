using System;
using System.Threading.Tasks;
using DSharpPlus;

using CatBot.Source.Code;
using Microsoft.Extensions.Logging;
using DSharpPlus.SlashCommands;
using System.Reflection;
using Newtonsoft.Json.Linq;
using CatBot.Source.Code.Modules;
using CatBot.Source.Code.Modules.Fun;
using MongoDB.Driver;
using System.IO;

using CatBot.Source.Code.Modules.Fun.Economy.DataBase;
using CatBot.Source.Data;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using System.Threading;

namespace CatBot;

public static class Catbot
{
    public static ILoggerFactory Logger = LoggerFactory.Create((builder) => builder.AddConsole());
    public static ILogger Log = Logger.CreateLogger("CatBot Thinks:");

    public static DiscordClient Client = new DiscordClient(new DiscordConfiguration()
    {
        Token = ConfigVar.TOKEN,
        TokenType = TokenType.Bot,
    });

    public static MongoClientSettings settings = MongoClientSettings.FromConnectionString(ConfigVar.DB_URI);
    public static MongoClient client = new MongoClient(settings);
    public static IMongoDatabase database = client.GetDatabase("CatBotCLUSTER");
    public static IMongoCollection<UserModel> collection = database.GetCollection<UserModel>("users");

    public static async Task Main(string[] args)
    {
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        Client.UseSlashCommands().RegisterCommands(Assembly.GetExecutingAssembly());
        Log.Log(LogLevel.Information, "Commands loaded (surprisingly...)");

        Client.UseInteractivity(new InteractivityConfiguration()
        {
            AckPaginationButtons = true,
            Timeout = TimeSpan.FromSeconds(30)
        });

        await Client.ConnectAsync();
        Log.Log(LogLevel.Information, "{} is ready", Client.CurrentApplication.Name);

        var users = await collection.FindAsync(_ => true);
        foreach (var user in users.ToList())
            await collection.FindOneAndUpdateAsync<UserModel>(_ => true,
                    Builders<UserModel>.Update.Set(u => u.IsCooldown, false));

        using (SemaphoreSlim cancelSem = new(0, 1))
        {
            Console.CancelKeyPress += (_, e) =>
            {
                cancelSem.Release();
                e.Cancel = true;
            };
            await cancelSem.WaitAsync();
        }

        await Client.DisconnectAsync();
        Log.Log(LogLevel.Information, "Bye bye bot lol");
    }
}