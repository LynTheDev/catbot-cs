using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatBot.Source.Code.Modules.Fun.Economy.DataBase;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

namespace CatBot.Source.Code.Modules.Fun.Economy;

public class EconomyCommands : ApplicationCommandModule
{
    [SlashCommand("getpats", "gain pats!")]
    public async Task PatCommand(InteractionContext ctx)
    {
        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        var rand = new Random();

        DiscordEmbedBuilder PatEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Rose,
            Title = "Aww, how cute!",
        };

        if (user is null)
        {
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, "User is not part of the database!");

            var newUser = new UserModel { DiscordID = ctx.Member.Id, PatAmount = 0, MaxPats = 30, MinPats = 0 };
            int patToGain = rand.Next(newUser.MinPats, newUser.MaxPats);

            PatEmbed.AddField($"{ctx.Member.DisplayName}, you've been patted...", $"{patToGain} times!", false);

            await Catbot.collection.InsertOneAsync(newUser);
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, "User has became part of the database!");

            var newPats = newUser.PatAmount + patToGain;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));
        }
        else
        {
            int patToGain = rand.Next(user.MinPats, user.MaxPats);
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"User {ctx.Member.DisplayName} awarded {patToGain} pats.");

            PatEmbed.AddField($"{ctx.Member.DisplayName}, you've been patted...", $"{patToGain} times!", false);

            var newPats = user.PatAmount + patToGain;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));
        }
    }

    [SlashCommand("patcount", "check how many pats you have!")]
    public async Task PatCountCommand(InteractionContext ctx)
    {
        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        if (user is null)
        {
            var newUser = new UserModel { DiscordID = ctx.Member.Id, PatAmount = 0, MaxPats = 30, MinPats = 0 };
            await Catbot.collection.InsertOneAsync(newUser);
        }

        UserModel users = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        DiscordEmbedBuilder PatEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Green,
            Title = $"{ctx.Member.DisplayName}'s pat count!"
        };

        PatEmbed.AddField($"{ctx.Member.DisplayName} has", $"{users.PatAmount} pats!", false);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));

    }
}