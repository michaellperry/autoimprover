# AutoImprover

Invite a bot to join your dev team. He'll check on your CI builds and deploy them to QA for you.

## LUIS result extension methods

This bot uses the Language Understanding Intelligence Service (LUIS) to determine your intent. Here are some useful extension methods for parsing LUIS results.

```c#
public static class LuisResultExtensions
{
    public static string GetSimpleEntityValue(this LuisResult result, string type)
    {
        return result.Entities
            .Where(e => e.Type == type)
            .Select(e => e.Entity)
            .FirstOrDefault();
    }

    public static string GetComplexEntityValue(this LuisResult result, string type)
    {
        return result.Entities
            .Where(e => e.Type == type)
            .SelectMany(e => e.Resolution.Values)
            .OfType<IList<object>>()
            .SelectMany(l => l)
            .OfType<IDictionary<string, object>>()
            .Select(d => d["value"] as string)
            .FirstOrDefault();
    }
}
```

## Remind me about an event

The first intent that we want to parse is to remind the team about a Scrum event. Train LUIS according to the instructions in the lab, and then use this code to parse the intent.

```c#
[LuisIntent("Reminder")]
public async Task Reminder(IDialogContext context, LuisResult result)
{
    string eventName = result.GetSimpleEntityValue("Event");
    string time = result.GetComplexEntityValue("builtin.datetimeV2.time");
    string datetime = result.GetComplexEntityValue("builtin.datetimeV2.datetime");
    await context.PostAsync($"OK. Remember that {eventName} is at {datetime ?? time}");
    context.Wait(MessageReceived);
}
```

## Wait for the event to remind me

This bot is being coy. What I really want is for the bot to wait until the event and then remind me. So to do that, we'll suspend the conversation and pick it up again later.

First we'll interpret the text that LUIS gave us indicating a datetime. Unfortunately, this text was ambiguous about the time zone. That's because LUIS doesn't know the user's time zone, and it operates in UTC. If we ask for a specific time, it will give us that time with no time zone. If we ask for a relative time, such as "in five minutes", then it will give us a UTC time, again with no time zone indicated.

We'll just parse the datetime and hope for the best. Here are two methods for interpreting a datetime string, and for interpreting a time string. Add them to the `BasicLuisDialog`.

```c#
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
```

Now we can modify the `Reminder` method to interpret the strings.

```c#
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
```

We're creating a conversation reference and passing it to a new `Reminder` object. Then we start that object so that the initial request can complete. You can find the source code for `Reminder` in [Reminders](https://github.com/michaellperry/autoimprover/tree/master/Reminders).

## Speak only when spoken to

It's rude and more than a bit annoying for a bot to try to respond to every conversation happening in Slack. You could set up a channel just for conversing with the bot, but then your team would have to go to that special place to see what's going on. Instead, let your bot hang out in #General, but only respond when directly addressed.

Add these methods to the `MessageController`:

```c#
private bool SpokenTo(string text)
{
    return text.Contains($"@{_botUserName}");
}

private string RemoveAddress(string text)
{
    return text.Replace($"@{_botUserName}", "");
}
```

Fetch the bot username in the `MessageController` when it is constructed:

```c#
private readonly string _botUserName;

public MessagesController()
{
    _botUserName = ConfigurationManager.AppSettings["BotUserName"];
}
```

Now you can modify the `Post` method to respond only when spoken to, and to remove the portion of the text that addresses the bot.

```c#
/// <summary>
/// POST: api/Messages
/// receive a message from a user and send replies
/// </summary>
/// <param name="activity"></param>
[ResponseType(typeof(void))]
public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
{
    // check if activity is of type message
    if (activity.GetActivityType() == ActivityTypes.Message &&
        SpokenTo(activity.Text))
    {
        activity.Text = RemoveAddress(activity.Text);
        await Conversation.SendAsync(activity, () => new BasicLuisDialog());
    }
    else
    {
        HandleSystemMessage(activity);
    }
    return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
}
```

## List builds in VSTS

For this lab, we are using VSTS as our CI tool. Using the VSTS API, we can list the builds that have completed. Train LUIS to recognize utterances like "What builds have finished?" and then use that intent to trigger this code:

```c#
[LuisIntent("ListBuilds")]
public async Task ListBuilds(IDialogContext context, LuisResult result)
{
    try
    {
        var vstsClient = new VstsClient();
        var delay = Task.Delay(1000);
        var fetch = vstsClient.ListBuilds();
        var completed = await Task.WhenAny(fetch, delay);
        if (completed == delay)
            await context.PostAsync("Let me get those builds for you...");

        var builds = await fetch;
        string buildsFormatted = builds
            .Select(b => $"{b.BuildNumber,-12}{b.Result,-11}{b.For,-20}{b.FinishTime:g}")
            .Aggregate("", (prior, current) => $"{prior}\n{current}");
        await context.PostAsync($"I found {builds.Count} builds:\n```{buildsFormatted}```");
    }
    catch (Exception ex)
    {
        await context.PostAsync($"I'm having trouble listing the builds: {ex.Message}");
    }
}
```

You can find the `VstsClient` class in [SourceControl](https://github.com/michaellperry/autoimprover/tree/master/SourceControl).
