using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace CatBot.Source.Code.Modules;

public class MainCommands : ApplicationCommandModule
{
    [SlashCommand("test", "tost")]
    public async Task TestCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("tost"));
        Catbot.Log.Log(LogLevel.Information, "Best command ever executed!");
    }
}