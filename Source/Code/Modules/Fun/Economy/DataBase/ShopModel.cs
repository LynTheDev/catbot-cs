using MongoDB.Bson.Serialization.Attributes;

namespace CatBot.Source.Code.Modules.Fun.Economy.DataBase;

[BsonIgnoreExtraElements]
public class ShopModel
{
    public ulong DiscordID { get; set; }
}