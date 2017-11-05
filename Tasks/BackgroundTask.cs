using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot.Tasks
{
    public abstract class BackgroundTask
    {
        private readonly ConversationReference _conversationReference;
        private readonly string _description;

        public BackgroundTask(ConversationReference conversationReference, string description)
        {
            _conversationReference = conversationReference;
            _description = description;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    await Process();
                }
                catch (Exception ex)
                {
                    await Respond($"I'm having trouble {_description}: {ex.Message}");
                    // Do something!
                }
            });
        }

        protected abstract Task Process();

        protected async Task Respond(string text)
        {
            var post = _conversationReference.GetPostToBotMessage();
            var reply = post.CreateReply(text);
            var client = new ConnectorClient(new Uri(post.ServiceUrl));
            await client.Conversations.ReplyToActivityAsync(reply);
        }
    }
}