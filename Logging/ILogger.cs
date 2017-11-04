using System;

namespace LuisBot.Logging
{
    public interface ILogger
    {
        void Log(object obj);
        void Log(string str);
    }
}
