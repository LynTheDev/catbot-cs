using System;
using System.IO;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace CatBot.Source.Code;

public static class Configs
{
    public static dynamic LoadConfig(string name)
    {
        string text = File.ReadAllText(name);
        dynamic content = JObject.Parse(text);

    return content;
    }

    public static dynamic GetContent(string res)
    { 
        dynamic content = JObject.Parse(res);
    return content;
    }
}
