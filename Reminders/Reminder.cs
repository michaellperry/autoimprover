using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.LuisBot.Reminders
{
    public class Reminder
    {
        private readonly TimeSpan _delay;
        private readonly string _eventName;
        private readonly IDialogContext _context;

        public Reminder(TimeSpan delay, string eventName, IDialogContext context)
        {
            _delay = delay;
            _eventName = eventName;
            _context = context;
        }

        public void Start()
        {
            Task.Run(() => Process());
        }

        private async Task Process()
        {
            await Task.Delay(_delay);
            await _context.PostAsync($"It's time for {_eventName}.");
        }
    }
}