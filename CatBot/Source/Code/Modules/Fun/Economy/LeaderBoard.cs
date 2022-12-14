using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MongoDB.Driver;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Microsoft.VisualBasic;
using System.Threading.Channels;
using DSharpPlus.Interactivity.Extensions;

namespace CatBot.Source.Code.Modules.Fun.Economy;

public class LeaderBoard : ApplicationCommandModule
{
    [SlashCommand("leaderboard", "Compare your pats with others!")]
    public static async Task LeaderBoardMain(InteractionContext ctx)
    {
        string test = string.Empty;
        int i = 0;

        var users = Catbot.collection.Find(_ => true).ToList();
        var usersOrdered = users.OrderBy(d => d.PatAmount).Reverse();

        foreach (var user in usersOrdered)
        {
            i++;
            // var userDetails = await Catbot.Client.GetUserAsync(user.DiscordID);
            test += $"{i}: <@{user.DiscordID}> -> {user.PatAmount} {DiscordEmoji.FromName(Catbot.Client, ":pat_pat:")}\n";
        }

        var inter = ctx.Client.GetInteractivity();
        var pages = inter.GeneratePagesInEmbed(test);
        await inter.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages);
    }
}