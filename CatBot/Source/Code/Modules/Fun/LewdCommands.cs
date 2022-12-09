using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using RestSharp;

namespace CatBot.Source.Code.Modules.Fun;

public class LewdCommands : ApplicationCommandModule
{

    [SlashCommand("spank", "sends a very lewd image, hehe")]
    public async Task LewdCommand(InteractionContext ctx)
    {
        RestClient rclient = new RestClient("https://nekos.life/");
        RestRequest request = new RestRequest("api/v2/img/spank");
        RestResponse res = await rclient.GetAsync(request);


        if (res.IsSuccessful && ctx.Channel.IsNSFW)
        {
            var content = Configs.GetContent(res.Content);
            string image = content.url;

            DiscordEmbedBuilder CatgirlEmbed = new DiscordEmbedBuilder
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
}
