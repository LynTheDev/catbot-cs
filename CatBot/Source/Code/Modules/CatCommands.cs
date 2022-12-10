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
        RestClient rclient = new RestClient("https://api.catboys.com");
        RestRequest request = new RestRequest("img");
        RestResponse res = await rclient.GetAsync(request);

        if (res.IsSuccessful)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            DiscordEmbedBuilder CatboyEmbed = new DiscordEmbedBuilder
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
        RestClient rclient = new RestClient("https://nekos.life");
        RestRequest request = new RestRequest("/api/v2/img/neko");
        RestResponse res = await rclient.GetAsync(request);

        if (res.IsSuccessful)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            DiscordEmbedBuilder CatgirlEmbed = new DiscordEmbedBuilder
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