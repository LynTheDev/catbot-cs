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

        DiscordEmbedBuilder AvatarEmbed = new DiscordEmbedBuilder
        {
            Title = $"{member.Username}'s details!",
            Url = member.AvatarUrl
        };

        AvatarEmbed.WithThumbnail(member.AvatarUrl);

        AvatarEmbed.AddField("Badges", member.Flags.ToString(), false);
        AvatarEmbed.AddField("Created at", member.CreationTimestamp.UtcDateTime.ToString(), false);

        AvatarEmbed.AddField("Name", member.Username, true);
        AvatarEmbed.AddField("Tag", member.Discriminator, true);
        AvatarEmbed.AddField("ID", member.Id.ToString(), true);

        AvatarEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        AvatarEmbed.Timestamp = DateTime.Now;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(AvatarEmbed));
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

        DiscordEmbedBuilder MeterEmbed = new DiscordEmbedBuilder
        {
            Color = DiscordColor.HotPink,
            Title = "Cat-O-Meter!"
        };

        if (user is null)
            user = ctx.Member;

        MeterEmbed.AddField($"{user.Username} is..", $"{meter}% cat{gender}", false);

        MeterEmbed.WithFooter($"{ctx.Member.DisplayName}#{ctx.Member.Discriminator}", ctx.Member.AvatarUrl);
        MeterEmbed.Timestamp = DateTime.Now;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .AddEmbed(MeterEmbed));
    }
}
