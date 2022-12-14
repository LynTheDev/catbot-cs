using MongoDB.Driver;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;

namespace CatBot.Source.Code.Modules.Fun.Economy.DataBase;

public static class DatabaseHelper
{ 
    public static UserModel NewUser(InteractionContext ctx)
    {
        UserModel newUser = new UserModel
        {
            DiscordID = ctx.Member.Id,
            PatAmount = 0,
            MaxPats = 15,
            MinPats = 0,
            IsCooldown = false,
            TimeLeft = 30,
        };

    return newUser;
    }

    public static UserModel NewUserMember(DiscordUser member)
    {
        UserModel newUser = new UserModel
        {
            DiscordID = member.Id,
            PatAmount = 0,
            MaxPats = 15,
            MinPats = 0,
            IsCooldown = false,
            TimeLeft = 30,
        };

        return newUser;
    }

    public static async Task<UserModel> CheckIfUserExists(InteractionContext ctx, DiscordUser member)
    {
        var check = Catbot.collection.Find(u => u.DiscordID == member.Id).FirstOrDefault();

        if (check is null)
        {
            await Catbot.collection.InsertOneAsync(NewUser(ctx));
            var newCheck = Catbot.collection.Find(u => u.DiscordID == member.Id).FirstOrDefault();
        return newCheck;
        }
            

    return check;
    }

    public static async Task<UserModel> CheckIfUserExistsMember(DiscordUser member)
    {
        var check = Catbot.collection.Find(u => u.DiscordID == member.Id).FirstOrDefault();

        if (check is null)
        {
            await Catbot.collection.InsertOneAsync(NewUserMember(member));
            var newCheck = Catbot.collection.Find(u => u.DiscordID == member.Id).FirstOrDefault();
            return newCheck;
        }


        return check;
    }

    public static async Task SwitchCooldown(bool switchTo, InteractionContext ctx)
    {
        if (switchTo)
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                    Builders<UserModel>.Update.Set(u => u.IsCooldown, true));
        else
            await Catbot.collection.FindOneAndUpdateAsync<UserModel>(u => u.DiscordID == ctx.Member.Id,
                    Builders<UserModel>.Update.Set(u => u.IsCooldown, false));
    }
}
