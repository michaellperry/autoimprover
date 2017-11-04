using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.LuisBot.Logging
{
    public class StringLogger : ILogger
    {
        private List<string> _messages = new List<string>();

        public IEnumerable<string> Message => _messages;

        public void Log(string str)
        {
            _messages.Add(str);
        }

        public void Log(object obj)
        {
            _messages.Add(JsonConvert.SerializeObject(obj));
        }
    }
}