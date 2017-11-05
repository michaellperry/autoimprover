using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Sample.LuisBot.Tasks.Reminders;
using Microsoft.Bot.Sample.LuisBot.Tasks.SourceControl;
using System.Linq;
using Microsoft.Bot.Builder.ConnectorEx;

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
            DateTime? moment = InterpretDateTime(datetime) ?? InterpretTime(time);
            if (eventName == null || moment == null)
            {
                await context.PostAsync($"I think you want me to remind you of something, but I can't tell what.");
            }
            else
            {
                var delay = moment.Value.Subtract(DateTime.Now);
                await context.PostAsync($"OK. I'll remind you of {eventName} in about {Math.Round(delay.TotalMinutes)} minutes.");
                new Reminder(delay, eventName, context.Activity.ToConversationReference()).Start();
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("ListBuilds")]
        public async Task ListBuilds(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("OK. Let me get those builds for you...");
            new ListBuilds(context.Activity.ToConversationReference()).Start();
            context.Wait(MessageReceived);
        }

        private DateTime? InterpretDateTime(string datetime)
        {
            DateTime moment;
            if (DateTime.TryParse(datetime, out moment))
                return moment;
            else
                return null;
        }

        private DateTime? InterpretTime(string time)
        {
            TimeSpan timeOfDay;
            if (TimeSpan.TryParse(time, out timeOfDay))
                return DateTime.Today.Add(timeOfDay);
            else
                return null;
        }
    }
}