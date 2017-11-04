using System;

namespace Microsoft.Bot.Sample.LuisBot.SourceControl
{
    public class Build
    {
        public string BuildNumber { get; set; }
        public DateTime FinishTime { get; set; }
        public string For { get; set; }
        public string Result { get; set; }
    }
}
