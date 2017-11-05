using Microsoft.Bot.Connector;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot.Tasks.SourceControl
{
    public class ListBuilds : BackgroundTask
    {
        public ListBuilds(ConversationReference conversationReference) :
            base(conversationReference, "listing the builds")
        {
        }

        protected override async Task Process()
        {
            var vstsClient = new VstsClient();
            var builds = await vstsClient.ListBuilds();
            string buildsFormatted = builds
                .Select(b => $"{b.BuildNumber,-12}{b.Result,-11}{b.For,-20}{b.FinishTime:g}")
                .Aggregate("", (prior, current) => $"{prior}\n{current}");
            await Respond($"I found {builds.Count} builds:\n```{buildsFormatted}```");
        }
    }
}