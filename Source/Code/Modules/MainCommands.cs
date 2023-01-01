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

    [SlashCommand("help", "help command")]
    public async Task HelpCommand(InteractionContext ctx)
    {
        var EconomyString = """
                /getpats - Increase you pat count!
                /givepats (user: required) - Give pats to a user!
                /patinfo (user: optional) - Check the pat count of yourself or of a user of your choice!
                /leaderboard - Check every user's pats, globally!
            """;

        var catString = """
                /catboy - Get an image of a catboy using the catboys api!
                /catgirl - Get an image of a catgirl using the nekos.life api!
                /catometer (gender: required) (user: optional) - Check how much of a catperson you or a user are!
            """;

        var generalString = """
                /user (user: required) - Get information about a user!
                /help - This command.
            """;

        var lewdString = """
            /spank - Get a image about spanking~ 
            /rule34 - Get one to five images with tags of your choosing~   
        """;
        
        var helpEmbed = new DiscordEmbedBuilder()
        {
            Color = DiscordColor.Yellow,
            Title = "CatBot Help!",
        };

        helpEmbed.WithThumbnail(Catbot.Client.CurrentUser.AvatarUrl);

        helpEmbed.AddField("Economy Commands", EconomyString, false);
        helpEmbed.AddField("Cat Commands", catString, false);
        helpEmbed.AddField("General Commands", generalString, false);
        helpEmbed.AddField("Lewd Commands, you must be 18+ for these.", lewdString, false);

        helpEmbed.WithFooter(ctx.Member.Username, ctx.Member.AvatarUrl);
        helpEmbed.Timestamp = DateTime.UtcNow;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .AddEmbed(helpEmbed));
    }
}