﻿using System;
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

namespace CatBot;

public static class Catbot
{
    public static ILoggerFactory Logger = LoggerFactory.Create((builder) => builder.AddConsole());
    public static ILogger Log = Logger.CreateLogger("CatBot Thinks:");

    public static MongoClientSettings settings = MongoClientSettings.FromConnectionString(File.ReadAllText("../../../Source/Data/mongo_config.txt"));
    public static MongoClient client = new MongoClient(settings);
    public static IMongoDatabase database = client.GetDatabase("CatBotCLUSTER");
    public static IMongoCollection<UserModel> collection = database.GetCollection<UserModel>("users");

    public static async Task Main(string[] args)
    {
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        dynamic content = Configs.LoadConfig("../../../Source/Data/config.json");
        string token = content.token;

        var Client = new DiscordClient(new DiscordConfiguration()
        {
            Token = token,
            TokenType = TokenType.Bot,
            MinimumLogLevel= LogLevel.Debug,
        });

        Client.UseSlashCommands().RegisterCommands(Assembly.GetExecutingAssembly());
        Log.Log(LogLevel.Information, "Commands loaded (surprisingly...)");

        await Client.ConnectAsync();
        Log.Log(LogLevel.Information, "{} is ready", Client);

        await Task.Delay(-1);
    }
}