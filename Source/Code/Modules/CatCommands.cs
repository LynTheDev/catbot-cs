using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Net;
using System.IO;
using CatBot.Source.Code.Modules.Fun.Economy.DataBase;

namespace CatBot.Source.Code.Modules;

public class CatCommands : ApplicationCommandModule
{ 

    [SlashCommand("catboy", "sends an image of a catboy")]
    public async Task CatboyCommand(InteractionContext ctx)
    {
        var rclient = new RestClient("https://api.catboys.com");
        var request = new RestRequest("img");
        var res = await rclient.GetAsync(request);

        if (res.IsSuccessful)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            var CatboyEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,

                Title = "Here you go!",
                Url = "https://catboys.com",

                ImageUrl = image,
            };
            CatboyEmbed.WithFooter("Made with the catboys api!");
            CatboyEmbed.WithTimestamp(DateTime.Now);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(CatboyEmbed));
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(
                "Something went wrong with the request!"
            ));
        }
        
    }

    [SlashCommand("catgirl", "sends an image of a catgirl")]
    public async Task CatgirlCommand(InteractionContext ctx)
    {
        var rclient = new RestClient("https://nekos.life");
        var request = new RestRequest("/api/v2/img/neko");
        var res = await rclient.GetAsync(request);

        if (res.IsSuccessful)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            var CatgirlEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,

                Title = "Here you go!",
                Url = "https://nekos.life",

                ImageUrl = image,
            };
            CatgirlEmbed.WithFooter("Made with the nekos.life api!");
            CatgirlEmbed.WithTimestamp(DateTime.Now);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(CatgirlEmbed));
        }
        else
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(
                "Something went wrong with the request!"
            ));
        }

    }

}