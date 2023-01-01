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
    public static void RemoveOne(UserModel user)
    {
        user.TimeLeft -= 1;
        Console.Write($"{user.TimeLeft} ");
    }

    public async Task Timer(PeriodicTimer timer, UserModel user, InteractionContext ctx)
    {
        while (await timer.WaitForNextTickAsync())
        {
            RemoveOne(user);

            if (user.TimeLeft == 0)
            {
                await DatabaseHelper.SwitchCooldown(false, ctx);
                break;
            }

        }
    }
    
    public static void CheckPats(int patToGain, DiscordEmbedBuilder PatEmbed, InteractionContext ctx)
    {
        switch (patToGain)
        {
            case 0:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has received", $"no pats... Better luck next time! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
            case 1:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has received", $"{patToGain} pat! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
            default:
                PatEmbed.AddField($"{ctx.Member.DisplayName} has received", $"{patToGain} pats! {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
                break;
        };
    }

    [SlashCommand("getpats", "gain pats!")]
    public async Task PatCommand(InteractionContext ctx)
    {
        var user = await DatabaseHelper.CheckIfUserExists(ctx, ctx.Member);

        var rand = new Random();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        var patEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Rose,
            Title = "Aww, how cute!",
        };

        patEmbed.WithThumbnail(ctx.Member.AvatarUrl);
        patEmbed.Timestamp = DateTime.Now;

        var patToGain = rand.Next(user.MinPats, user.MaxPats);
        Catbot.Log.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"User {ctx.Member.DisplayName} awarded {patToGain} pats.");

        if (user.IsCooldown)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"You have to wait {user.TimeLeft} seconds before doing that again!"));

            return;
        }

        CheckPats(patToGain, patEmbed, ctx);

        var newPats = user.PatAmount + patToGain;
        await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
            Builders<UserModel>.Update.Set(u => u.PatAmount, newPats));

        await DatabaseHelper.SwitchCooldown(true, ctx);

        await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
            Builders<UserModel>.Update.Set(u => u.TimeLeft, 30));

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .AddEmbed(patEmbed));

        await Timer(timer, user, ctx);
    }

    [SlashCommand("patinfo", "check how many pats you have!")]
    public async Task PatCountCommand(InteractionContext ctx, [Option("user", "user to check")] DiscordUser member = null)
    {
        UserModel user;
        bool isOpt;

        if (member is null)
        {
            user = await DatabaseHelper.CheckIfUserExists(ctx, ctx.Member);
            isOpt = false;
        }
        else
        {
            user = await DatabaseHelper.CheckIfUserExistsMember(member);
            isOpt = true;
        }

        var patEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Green,
            Title = $"{DiscordEmoji.FromName(Catbot.Client, ":pat:")} { (isOpt ? member.Username : ctx.Member.DisplayName) }'s pat details!"
        };

        patEmbed.WithThumbnail(isOpt ? member.AvatarUrl : ctx.Member.AvatarUrl);
        patEmbed.Timestamp = DateTime.Now;

        patEmbed.AddField($"{(isOpt ? member.Username : ctx.Member.DisplayName)} has", $"{user.PatAmount} pats!", false);
        patEmbed.AddField("Maximum Pats", $"{user.MaxPats} pats!", true);
        patEmbed.AddField("Minimum Pats", $"{user.MinPats} pats!", true);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(patEmbed));
    }

    [SlashCommand("givepats", "give pats to a user!")]
    public static async Task GivePatCommand(
        InteractionContext ctx, [Option("user", "User to give pats to")] DiscordUser member, [Option("patcount", "pats to give")] long patNum) 
    {
        var user = await DatabaseHelper.CheckIfUserExists(ctx, ctx.Member);
        var givenUser = await DatabaseHelper.CheckIfUserExistsMember(member);

        if (patNum == 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Don't be mean, {ctx.Member.DisplayName}!"));
        return;
        }
        if (patNum < 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Fuck off"));
        return;
        }

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

        var giveEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Blue,
            Title = $"{DiscordEmoji.FromName(Catbot.Client, ":frog_cute:")} Aww! How Sweet!"
        };

        giveEmbed.WithThumbnail(member.AvatarUrl);
        giveEmbed.Timestamp = DateTime.Now;
        giveEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        giveEmbed.AddField($"{ctx.Member.DisplayName} has given", $"{patNum} {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")} to {member.Mention}");

        var givenNewPats = givenUser.PatAmount + patNum;
        await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == member.Id,
            Builders<UserModel>.Update.Set(u => u.PatAmount, givenNewPats));

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(giveEmbed));
    }
}