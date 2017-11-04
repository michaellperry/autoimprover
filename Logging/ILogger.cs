using System;

namespace Microsoft.Bot.Sample.LuisBot.Logging
{
    public interface ILogger
    {
        void Log(object obj);
        void Log(string str);
    }
}
