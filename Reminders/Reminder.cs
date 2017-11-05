using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot.Reminders
{
    public class Reminder
    {
        private readonly TimeSpan _delay;
        private readonly string _eventName;
        private readonly ConversationReference _conversationReference;

        public Reminder(TimeSpan delay, string eventName, ConversationReference conversationReference)
        {
            _delay = delay;
            _eventName = eventName;
            _conversationReference = conversationReference;
        }

        public void Start()
        {
            Task.Run(() => Process());
        }

        private async Task Process()
        {
            try
            {
                await Task.Delay(_delay);
                var post = _conversationReference.GetPostToBotMessage();
                var reply = post.CreateReply($"It's time for {_eventName}.");
                var client = new ConnectorClient(new Uri(post.ServiceUrl));
                await client.Conversations.ReplyToActivityAsync(reply);
            }
            catch (Exception ex)
            {
                // Do something!
            }
        }
    }
}