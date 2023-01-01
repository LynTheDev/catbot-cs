using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MongoDB.Driver;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace CatBot.Source.Code.Modules.Fun.Economy;

public class LeaderBoard : ApplicationCommandModule
{
    [SlashCommand("leaderboard", "Compare your pats with others!")]
    public static async Task LeaderBoardMain(InteractionContext ctx)
    {
        var userLine = string.Empty;
        var i = 0;

        var leaderboard = new DiscordEmbedBuilder()
        {
            Color = DiscordColor.Green,
            Title = "Pat Leaderboards!",
            Description = "Compare your pats!"
        };

        var users = Catbot.collection.Find(_ => true).ToList();
        var usersOrdered = users.OrderBy(d => d.PatAmount).Reverse();

        foreach (var user in usersOrdered)
        {
            i++;
            var userDetails = await Catbot.Client.GetUserAsync(user.DiscordID);
            userLine += $"**{i}**: {userDetails.Username} -> {user.PatAmount} {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}\n";
            
            if(user.DiscordID == ctx.Member.Id)
                leaderboard.AddField("Wow!", $"You are in place {i} with {user.PatAmount} {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}", false);
        }

        var inter = ctx.Client.GetInteractivity();
        var pages = inter.GeneratePagesInEmbed(userLine, SplitType.Line, leaderboard);
        await inter.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
    }
}