using MongoDB.Bson.Serialization.Attributes;

namespace CatBot.Source.Code.Modules.Fun.Economy.DataBase;

[BsonIgnoreExtraElements]
public class UserModel
{
    public ulong DiscordID { get; set; }
    public long PatAmount { get; set; }
    public int MaxPats { get; set; }
    public int MinPats { get; set; }
}