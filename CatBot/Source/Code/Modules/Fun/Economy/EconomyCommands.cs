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
using System.Threading;
using System.Timers;

namespace CatBot.Source.Code.Modules.Fun.Economy;

public class EconomyCommands : ApplicationCommandModule
{
    public static async Task RemoveOne(UserModel user, InteractionContext ctx)
    {
        user.TimeLeft -= 1;
        await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.TimeLeft, user.TimeLeft));
    }

    public async Task Timer(PeriodicTimer timer, UserModel user, InteractionContext ctx)
    {
        while (await timer.WaitForNextTickAsync())
        {
            await RemoveOne(user, ctx);

            if (user.TimeLeft == 0)
            {
                await SwitchCooldown(false, ctx);

                await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                    Builders<UserModel>.Update.Set(u => u.TimeLeft, 30));

                break;
            }

        }
    }
    public static async Task AddNewUser(InteractionContext ctx)
    {
        var newUser = new UserModel { 
            DiscordID = ctx.Member.Id, 
            PatAmount = 0, MaxPats = 15, 
            MinPats = 0, 
            IsCooldown = false,
            TimeLeft = 30
        };
        await Catbot.collection.InsertOneAsync(newUser);
    }

    public static void CheckPats(int patToGain, DiscordEmbedBuilder PatEmbed, InteractionContext ctx)
    {
        switch (patToGain)
        {
            case 0:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has recieved", $"no pats... Better luck next time! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
            case 1:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has recieved", $"{patToGain} pat! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
            default:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has recieved", $"{patToGain} pats! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
        };
    }

    public static async Task SwitchCooldown(bool switchTo, InteractionContext ctx)
    {
        if (switchTo)
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                    Builders<UserModel>.Update.Set(u => u.IsCooldown, true));
        else
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                    Builders<UserModel>.Update.Set(u => u.IsCooldown, false));
    }

    [SlashCommand("getpats", "gain pats!")]
    public async Task PatCommand(InteractionContext ctx)
    {
        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        var rand = new Random();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        DiscordEmbedBuilder PatEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Rose,
            Title = "Aww, how cute!",
        };

        PatEmbed.WithThumbnail(ctx.Member.AvatarUrl);
        PatEmbed.Timestamp = DateTime.Now;

        if (user is null)
        {
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, "User is not part of the database!");

            var newUser = new UserModel { DiscordID = ctx.Member.Id, PatAmount = 0, MaxPats = 15, MinPats = 0, IsCooldown = false, TimeLeft = 30 };

            if (newUser.IsCooldown)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"You have to wait {newUser.TimeLeft} seconds before doing that again!"));

            return;
            }

            int patToGain = rand.Next(newUser.MinPats, newUser.MaxPats);

            CheckPats(patToGain, PatEmbed, ctx);

            await Catbot.collection.InsertOneAsync(newUser);
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, "User has became part of the database!");

            var newPats = newUser.PatAmount + patToGain;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

            await SwitchCooldown(true, ctx);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));

            await Timer(timer, newUser, ctx);
        }
        else
        {
            int patToGain = rand.Next(user.MinPats, user.MaxPats);
            Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"User {ctx.Member.DisplayName} awarded {patToGain} pats.");

            if (user.IsCooldown)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"You have to wait {user.TimeLeft} seconds before doing that again!"));

                return;
            }

            CheckPats(patToGain, PatEmbed, ctx);

            var newPats = user.PatAmount + patToGain;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

            await SwitchCooldown(true, ctx);

            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.TimeLeft, 30));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));

            await Timer(timer, user, ctx);
        }
    }

    [SlashCommand("patinfo", "check how many pats you have!")]
    public async Task PatCountCommand(InteractionContext ctx)
    {
        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        if (user is null)
            await AddNewUser(ctx);

        UserModel users = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();

        DiscordEmbedBuilder PatEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Green,
            Title = $"{DiscordEmoji.FromName(Catbot.Client, ":pat:")} {ctx.Member.DisplayName}'s pat details!"
        };

        PatEmbed.WithThumbnail(ctx.Member.AvatarUrl);
        PatEmbed.Timestamp = DateTime.Now;

        PatEmbed.AddField($"{ctx.Member.DisplayName} has", $"{users.PatAmount} pats!", false);
        PatEmbed.AddField("Maximum Pats", $"{users.MaxPats} pats!", true);
        PatEmbed.AddField("Minimum Pats", $"{users.MinPats} pats!", true);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(PatEmbed));

    }
}