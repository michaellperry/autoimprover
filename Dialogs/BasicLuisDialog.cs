using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Linq;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("Reminder")]
        public async Task MyIntent(IDialogContext context, LuisResult result)
        {
            var eventName = result.Entities
                .Where(e => e.Type == "Event")
                .Select(e => e.Entity)
                .FirstOrDefault();
            var times = result.Entities
                .Where(e => e.Type == "builtin.datetimeV2.datetime")
                .SelectMany(e => e.Resolution.Values)
                .ToArray();
            var time = times
                .OfType<dynamic>()
                .Select(t => (string)t.value)
                .FirstOrDefault();
            await context.PostAsync($"OK. Remember that {eventName} is at {time}");
            context.Wait(MessageReceived);
        }
    }
}