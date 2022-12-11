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
    public static void RemoveOne(UserModel user, InteractionContext ctx)
    {
        user.TimeLeft -= 1;
        Console.Write($"{user.TimeLeft} ");
    }

    public async Task Timer(PeriodicTimer timer, UserModel user, InteractionContext ctx)
    {
        while (await timer.WaitForNextTickAsync())
        {
            RemoveOne(user, ctx);

            if (user.TimeLeft == 0)
            {
                await SwitchCooldown(false, ctx);
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
            TimeLeft = 30,
        };
        await Catbot.collection.InsertOneAsync(newUser);
    }

    public static async Task CheckUser(InteractionContext ctx, DiscordUser member)
    {
        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(member.Id)).FirstOrDefault();

        if (user is null)
            await AddNewUser(ctx);
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

            var newUser = new UserModel {
                DiscordID = ctx.Member.Id, PatAmount = 0, 
                MaxPats = 15, MinPats = 0,
                IsCooldown = false, TimeLeft = 30
            };

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
        await CheckUser(ctx, ctx.Member);

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

    [SlashCommand("givepats", "give pats to a user!")]
    public static async Task GivePatCommand(InteractionContext ctx, [Option("user", "User to give pats to")] DiscordUser member, [Option("patcount", "pats to give")] long patNum) 
    {
        await CheckUser(ctx, ctx.Member);

        UserModel user = Catbot.collection.Find(u => u.DiscordID.Equals(ctx.Member.Id)).FirstOrDefault();
        UserModel givenUser = Catbot.collection.Find(u => u.DiscordID.Equals(member.Id)).FirstOrDefault();

        if (user.PatAmount < patNum)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("You don't have enough pats for that!"));
        return;
        }

        if(ctx.Member.Id == member.Id)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You can't just do that!"));
        return;
        }

        var newPats = user.PatAmount - patNum;
        await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

        DiscordEmbedBuilder GiveEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Blue,
            Title = $"{DiscordEmoji.FromName(Catbot.Client, ":frog_cute:")} Aww! How Sweet!"
        };

        GiveEmbed.WithThumbnail(member.AvatarUrl);
        GiveEmbed.Timestamp = DateTime.Now;
        GiveEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        GiveEmbed.AddField($"{ctx.Member.DisplayName} has given", $"{patNum} {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")} to {member.Mention}");

        if (givenUser is null)
        {
            var newUser = new UserModel
            {
                DiscordID = member.Id, PatAmount = 0,
                MaxPats = 15, MinPats = 0,
                IsCooldown = false, TimeLeft = 30,
            };

            await Catbot.collection.InsertOneAsync(newUser);

            var givenNewPats = newUser.PatAmount + patNum;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, givenNewPats));
        }
        else
        {
            var givenNewPats = givenUser.PatAmount + patNum;
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == member.Id,
                Builders<UserModel>.Update.Set(u => u.PatAmount, givenNewPats));
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(GiveEmbed));
    }
}