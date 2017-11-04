using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Sample.LuisBot.SourceControl;
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

        [LuisIntent("Reminder")]
        public async Task Reminder(IDialogContext context, LuisResult result)
        {
            string eventName = result.GetSimpleEntityValue("Event");
            string time = result.GetComplexEntityValue("builtin.datetimeV2.time");
            string datetime = result.GetComplexEntityValue("builtin.datetimeV2.datetime");
            await context.PostAsync($"OK. Remember that {eventName} is at {datetime ?? time}");
            context.Wait(MessageReceived);
        }

        [LuisIntent("ListBuilds")]
        public async Task ListBuilds(IDialogContext context, LuisResult result)
        {
            try
            {
                var vstsClient = new VstsClient();
                var delay = Task.Delay(1000);
                var fetch = vstsClient.ListBuilds();
                var completed = await Task.WhenAny(fetch, delay);
                if (completed == delay)
                    await context.PostAsync("Let me get those builds for you...");

                var builds = await fetch;
                string buildsFormatted = builds
                    .Select(b => $"{b.BuildNumber,-12}{b.Result,-11}{b.For,-20}{b.FinishTime:g}")
                    .Aggregate("", (prior, current) => $"{prior}\n{current}");
                await context.PostAsync($"I found {builds.Count} builds:\n```{buildsFormatted}```");
            }
            catch (Exception ex)
            {
                await context.PostAsync($"I'm having trouble listing the builds: {ex.Message}");
            }
        }
    }
}