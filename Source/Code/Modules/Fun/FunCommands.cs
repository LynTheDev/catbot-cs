using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CatBot.Source.Code.Modules.Fun;

public class FunCommands : ApplicationCommandModule
{
    [SlashCommand("user", "shows details of an user")]
    public async Task DetailCommand(InteractionContext ctx, [Option("user", "User to analize")] DiscordUser member)
    {

        var avatarEmbed = new DiscordEmbedBuilder
        {
            Title = $"{member.Username}'s details!",
            Url = member.AvatarUrl
        };

        avatarEmbed.WithThumbnail(member.AvatarUrl);

        avatarEmbed.AddField("Badges", member.Flags.ToString(), false);
        avatarEmbed.AddField("Created at", member.CreationTimestamp.UtcDateTime.ToString(), false);

        avatarEmbed.AddField("Name", member.Username, true);
        avatarEmbed.AddField("Tag", member.Discriminator, true);
        avatarEmbed.AddField("ID", member.Id.ToString(), true);

        avatarEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        avatarEmbed.Timestamp = DateTime.Now;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(avatarEmbed));
    }

    [SlashCommand("catometer", "shows what percent catboy/girl you are")]
    public async Task MeterCommand(InteractionContext ctx,

        [Choice("Girl", "girl")]
        [Choice("Boy", "boy")]
        [Choice("Non Binary", "person")]
        [Option("gender", "What's your gender?")] string gender,

        [Option("user", "User to check!")] DiscordUser user = null)
    {
        var rand = new Random();
        var meter = rand.Next(1, 100);

        var meterEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.HotPink,
            Title = "Cat-O-Meter!"
        };

        if (user is null)
            user = ctx.Member;

        meterEmbed.AddField($"{user.Username} is..", $"{meter}% cat{gender}", false);

        meterEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        meterEmbed.Timestamp = DateTime.Now;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .AddEmbed(meterEmbed));
    }
}
