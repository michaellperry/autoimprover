using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Sample.LuisBot
{
    public static class LuisResultExtensions
    {

        public static string GetSimpleEntityValue(this LuisResult result, string type)
        {
            return result.Entities
                .Where(e => e.Type == type)
                .Select(e => e.Entity)
                .FirstOrDefault();
        }

        public static string GetComplexEntityValue(this LuisResult result, string type)
        {
            return result.Entities
                .Where(e => e.Type == type)
                .SelectMany(e => e.Resolution.Values)
                .OfType<IList<object>>()
                .SelectMany(l => l)
                .OfType<IDictionary<string, object>>()
                .Select(d => d["value"] as string)
                .FirstOrDefault();
        }
    }
}