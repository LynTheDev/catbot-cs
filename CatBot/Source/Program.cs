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

namespace CatBot;

public static class Catbot
{
    public static ILoggerFactory Logger = LoggerFactory.Create((builder) => builder.AddConsole());
    public static ILogger Log = Logger.CreateLogger("CatBot Thinks:");

    public static async Task Main(string[] args)
    {
        dynamic content = Configs.LoadConfig("../../../Source/Data/config.json");
        string token = content.token;

        var Client = new DiscordClient(new DiscordConfiguration()
        {
            Token = token,
            TokenType = TokenType.Bot,
            LoggerFactory = Logger
        });

        Client.UseSlashCommands().RegisterCommands(Assembly.GetExecutingAssembly());
        Log.Log(LogLevel.Information, "Commands loaded (surprisingly...)");

        await Client.ConnectAsync();
        Log.Log(LogLevel.Information, "{} is ready", Client);

        await Task.Delay(-1);
    }
}