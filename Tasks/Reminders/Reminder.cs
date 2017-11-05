using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot.Tasks.Reminders
{
    public class Reminder : BackgroundTask
    {
        private readonly TimeSpan _delay;
        private readonly string _eventName;

        public Reminder(TimeSpan delay, string eventName, ConversationReference conversationReference) :
            base(conversationReference, "setting a reminder")
        {
            _delay = delay;
            _eventName = eventName;
        }

        protected override async Task Process()
        {
            await Task.Delay(_delay);
            await Respond($"It's time for {_eventName}.");
        }
    }
}