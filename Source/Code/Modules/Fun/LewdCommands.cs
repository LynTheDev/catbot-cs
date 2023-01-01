using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CatBot.Source.Code.Modules.Fun;

public class LewdCommands : ApplicationCommandModule
{

    [SlashCommand("spank", "sends a very lewd image, hehe")]
    public async Task LewdCommand(InteractionContext ctx)
    {
        var rclient = new RestClient("https://nekos.life/");
        var request = new RestRequest("api/v2/img/spank");
        var res = await rclient.GetAsync(request);


        if (res.IsSuccessful && ctx.Channel.IsNSFW)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            var CatgirlEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,

                Title = "H-hehe~!",
                Url = "https://nekos.life",

                ImageUrl = image,
            };
            CatgirlEmbed.WithFooter("Made with the nekos.life api!");
            CatgirlEmbed.WithTimestamp(DateTime.Now);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AddEmbed(CatgirlEmbed));
        }
        else
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
               .WithContent("T-the channel must be n-nsfw >~<"));
    }

    [SlashCommand("rule34", "sends a rule34 one to five images of your choosing")]
    public async Task Rule34Command(InteractionContext ctx,
        [Option("limit", "limit the amount of images sent")] long limit,
        [Option("tags", "the tags for the images.")] string tags
        )
    {
        var rclient = new RestClient("https://api.rule34.xxx//");
        var request = new RestRequest($"index.php?page=dapi&s=post&q=index&json=1&limit=500&tags={tags}");
        var res = await rclient.GetAsync(request);

        if(!ctx.Channel.IsNSFW)
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("T-The channel must b-be n-nsfw!~"));
        
        if(limit > 5)
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("I-I can o-only send 5 i-images at a time!~"));

        var urls = string.Empty;
        var final = string.Empty;
        var rand = new Random();

        if (res.Content != null)
        {
            dynamic content = JArray.Parse(res.Content);
            for (var i = 1; i <= limit; i++)
            {
                urls += $"{i}. {content[rand.Next(0, content.Count - 1)]["sample_url"].ToString()}\n";
            }
            
            final = $$"""
                Search Tags: {{tags}}
                Images: 
                {{urls}}
                """;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent(final));
        }
    }
}
